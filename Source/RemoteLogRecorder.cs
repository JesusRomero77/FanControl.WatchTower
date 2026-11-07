using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;

namespace WatchTower
{
    /// <summary>Estado de una grabación remota.</summary>
    public enum RecorderStatus
    {
        /// <summary>Esperando a obtener el perfil del PC remoto (arrancando, sin datos aún o reintentando).</summary>
        Connecting,

        /// <summary>Grabando: llegan lecturas nuevas.</summary>
        Recording,

        /// <summary>Se conecta, pero las lecturas no se actualizan (FanControl del PC remoto con la interfaz cerrada).</summary>
        DataStale,

        /// <summary>No se puede conectar con el PC remoto. Se sigue reintentando.</summary>
        Unreachable,

        /// <summary>La contraseña es incorrecta. La grabación se detiene.</summary>
        Unauthorized,

        /// <summary>Fallo inesperado, normalmente al escribir el log. La grabación se detiene.</summary>
        Failed,

        /// <summary>La grabación ha terminado. Siempre es el último estado.</summary>
        Stopped
    }

    public class RecorderStatusEventArgs : EventArgs
    {
        public RecorderStatusEventArgs(RecorderStatus status, string message)
        {
            Status = status;
            Message = message;
        }

        public RecorderStatus Status { get; }

        /// <summary>Detalle técnico del fallo, si lo hay.</summary>
        public string Message { get; }
    }

    /// <summary>
    /// Graba en un log las lecturas de un DataServer remoto. En un hilo propio, una vez por segundo: pide el
    /// perfil, escribe su resumen en el log, y después pide las lecturas y escribe las que cumplan el filtro de
    /// cada elemento (casilla, intervalo y umbral), con la misma lógica que el plugin CurveReader. Los ajustes
    /// se leen de los ItemConfig en cada vuelta, así que si el usuario los cambia durante la grabación, el
    /// cambio se aplica enseguida. Se crea un grabador nuevo por cada grabación.
    /// </summary>
    public class RemoteLogRecorder
    {
        private const int PollIntervalMilliseconds = 1000;

        // Consultas seguidas con el mismo número de lectura tras las cuales los datos se dan por parados.
        private const int StalePollsThreshold = 5;

        private readonly DataServerClient _client;
        private readonly RemoteLogFile _logFile;
        private readonly string _remoteAddress;
        private readonly Func<RemoteProfileInfo, ProfileConfig> _getProfileConfig;
        private readonly ManualResetEvent _stopSignal = new ManualResetEvent(false);
        private readonly ManualResetEvent _stoppedSignal = new ManualResetEvent(false);

        private Thread _thread;
        private RecorderStatus? _currentStatus;
        private bool _hasWritten;

        // Perfil que se está grabando. Es null mientras haya que pedirlo (o volver a pedirlo).
        private RemoteProfileInfo _profile;
        private string _lastProfileKey;
        private List<RecordedItem> _controls = new List<RecordedItem>();
        private List<RecordedItem> _sensors = new List<RecordedItem>();
        private long _lastSequence = -1;
        private int _samePollCount;

        /// <param name="client">Cliente ya configurado con la IP o nombre, el puerto y la contraseña.</param>
        /// <param name="logFile">Archivo donde se graba.</param>
        /// <param name="remoteAddress">Texto que identifica al PC remoto en el resumen del log, por ejemplo "192.168.1.50:8765".</param>
        /// <param name="getProfileConfig">
        /// Devuelve los ajustes de registro del perfil remoto indicado, con un ItemConfig por cada control y
        /// sensor. Se llama desde el hilo del grabador, cada vez que hay un perfil nuevo. Si la implementación
        /// toca la interfaz, debe pasar al hilo de la interfaz con Invoke.
        /// </param>
        public RemoteLogRecorder(DataServerClient client, RemoteLogFile logFile, string remoteAddress,
            Func<RemoteProfileInfo, ProfileConfig> getProfileConfig)
        {
            _client = client;
            _logFile = logFile;
            _remoteAddress = remoteAddress;
            _getProfileConfig = getProfileConfig;
        }

        /// <summary>
        /// Se lanza desde el hilo del grabador cuando cambia el estado. El manejador no debe tocar la
        /// interfaz directamente: debe pasar al hilo de la interfaz con BeginInvoke.
        /// </summary>
        public event EventHandler<RecorderStatusEventArgs> StatusChanged;

        /// <summary>Empieza a grabar en un hilo propio. No hace nada si ya está grabando.</summary>
        public void Start()
        {
            if (_thread != null) return;

            _thread = new Thread(Run) { IsBackground = true, Name = "RemoteLogRecorder" };
            _thread.Start();
        }

        /// <summary>Pide que se detenga la grabación. No espera: el estado Stopped avisa de cuándo termina.</summary>
        public void Stop()
        {
            _stopSignal.Set();
        }

        /// <summary>Espera a que la grabación termine, como máximo el tiempo indicado. Devuelve true si terminó.</summary>
        public bool WaitUntilStopped(int milliseconds)
        {
            return _stoppedSignal.WaitOne(milliseconds);
        }

        // Cuerpo del hilo: una vuelta por segundo hasta que se pida parar o aparezca un error que impida seguir.
        // Al terminar, si se escribió algo en esta grabación, se pone la marca de fin de log.
        private void Run()
        {
            try
            {
                _logFile.RemoveEndOfLogMarker();
                SetStatus(RecorderStatus.Connecting, null);

                Stopwatch stopwatch = new Stopwatch();
                while (!_stopSignal.WaitOne(0))
                {
                    stopwatch.Restart();
                    if (!Poll()) break;

                    int remaining = PollIntervalMilliseconds - (int)stopwatch.ElapsedMilliseconds;
                    if (remaining > 0 && _stopSignal.WaitOne(remaining)) break;
                }
            }
            catch (Exception ex)
            {
                SetStatus(RecorderStatus.Failed, ex.Message);
            }
            finally
            {
                CloseLog();
                SetStatus(RecorderStatus.Stopped, null);
                _stoppedSignal.Set();
            }
        }

        // Una vuelta: si no hay perfil, lo pide; si lo hay, pide las lecturas. Devuelve false si hay que parar.
        private bool Poll()
        {
            return _profile == null ? PollProfile() : PollReadings();
        }

        // Pide el perfil al PC remoto. Con contraseña incorrecta se para; con cualquier otro fallo se
        // reintenta en la vuelta siguiente.
        private bool PollProfile()
        {
            DataServerResponse<RemoteProfileInfo> response = _client.GetProfile();

            switch (response.Status)
            {
                case DataServerStatus.Ok:
                    LoadProfile(response.Data);
                    return true;
                case DataServerStatus.Unauthorized:
                    SetStatus(RecorderStatus.Unauthorized, response.Message);
                    return false;
                case DataServerStatus.Unreachable:
                    SetStatus(RecorderStatus.Unreachable, response.Message);
                    return true;
                default:
                    SetStatus(RecorderStatus.Connecting, response.Message);
                    return true;
            }
        }

        // Pide las lecturas al PC remoto. Si el servidor dice que aún no tiene datos, se vuelve a pedir el
        // perfil, porque probablemente esté cargando uno nuevo.
        private bool PollReadings()
        {
            DataServerResponse<RemoteReadingsSnapshot> response = _client.GetReadings();

            switch (response.Status)
            {
                case DataServerStatus.Ok:
                    return ProcessReadings(response.Data);
                case DataServerStatus.Unauthorized:
                    SetStatus(RecorderStatus.Unauthorized, response.Message);
                    return false;
                case DataServerStatus.Unreachable:
                    SetStatus(RecorderStatus.Unreachable, response.Message);
                    return true;
                case DataServerStatus.Busy:
                    SetStatus(RecorderStatus.Connecting, response.Message);
                    return true;
                default:
                    _profile = null;
                    SetStatus(RecorderStatus.Connecting, response.Message);
                    return true;
            }
        }

        // Guarda el perfil recibido. Si es el mismo que ya se estaba grabando (solo se volvió a pedir por un
        // fallo pasajero), se conservan los elementos y su estado. Si es nuevo, se piden sus ajustes, se
        // crean los elementos vigilados desde cero y se escribe su resumen en el log.
        private void LoadProfile(RemoteProfileInfo profile)
        {
            string key = profile.ProfileId + "|" + profile.ProfileName;
            _profile = profile;
            if (key == _lastProfileKey) return;

            ProfileConfig config = _getProfileConfig(profile);
            _controls = BuildItems(profile.Controls, config.Controls);
            _sensors = BuildItems(profile.Sensors, config.Sensors);
            _lastSequence = -1;
            _samePollCount = 0;
            _lastProfileKey = key;

            WriteToLog(BuildProfileSummary(profile));
        }

        // Crea un elemento vigilado por cada control o sensor del perfil, emparejado por Identifier con sus
        // ajustes. Si no hubiera ajustes para alguno, se usan los de por defecto.
        private static List<RecordedItem> BuildItems(List<RemoteElementInfo> elements, List<ItemConfig> configs)
        {
            List<RecordedItem> items = new List<RecordedItem>();
            if (elements == null) return items;

            foreach (RemoteElementInfo element in elements)
            {
                if (string.IsNullOrEmpty(element.Identifier)) continue;

                ItemConfig config = configs.FirstOrDefault(candidate =>
                    string.Equals(candidate.Identifier, element.Identifier, StringComparison.Ordinal))
                    ?? new ItemConfig { Identifier = element.Identifier, Name = element.Name };

                items.Add(new RecordedItem(element, config));
            }

            return items;
        }

        // Procesa una foto de lecturas. Antes de escribir nada comprueba que el perfil no ha cambiado, que
        // el servidor no se ha reiniciado (el número de lectura baja) y que los datos no están parados (el
        // número de lectura no sube). Los segundos transcurridos son la diferencia entre números de lectura,
        // así que una consulta perdida no falsea los intervalos.
        private bool ProcessReadings(RemoteReadingsSnapshot snapshot)
        {
            if (snapshot.ProfileId != _profile.ProfileId)
            {
                _profile = null;
                SetStatus(RecorderStatus.Connecting, null);
                return true;
            }

            if (snapshot.Sequence < _lastSequence)
            {
                _profile = null;
                _lastProfileKey = null;
                SetStatus(RecorderStatus.Connecting, null);
                return true;
            }

            if (snapshot.Sequence == _lastSequence)
            {
                _samePollCount++;
                if (_samePollCount >= StalePollsThreshold) SetStatus(RecorderStatus.DataStale, null);
                return true;
            }

            long elapsedSeconds = _lastSequence < 0 ? 1 : snapshot.Sequence - _lastSequence;
            _lastSequence = snapshot.Sequence;
            _samePollCount = 0;
            SetStatus(RecorderStatus.Recording, null);

            WriteRecords(snapshot, elapsedSeconds);
            return true;
        }

        // Recorre los controles y después los sensores, y escribe de una vez en el log todas las líneas
        // que toquen en esta vuelta.
        private void WriteRecords(RemoteReadingsSnapshot snapshot, long elapsedSeconds)
        {
            string timestamp = DateTime.Now.ToString("dd/MM/yy HH:mm:ss", CultureInfo.InvariantCulture);
            StringBuilder text = new StringBuilder();

            AppendDueRecords(text, _controls, ToValues(snapshot.Controls), TextId.ControlType, timestamp, elapsedSeconds);
            AppendDueRecords(text, _sensors, ToValues(snapshot.Sensors), TextId.SensorType, timestamp, elapsedSeconds);

            if (text.Length > 0) WriteToLog(text.ToString());
        }

        // Decide, para cada elemento, si debe escribirse, con la misma lógica que CurveReader: se descartan
        // en orden los no incluidos y los que aún están dentro de su intervalo de descanso; solo a los que
        // llegan hasta el final se les compara el valor con el último escrito (diferencia >= umbral; con
        // umbral 0 se escribe siempre). La primera lectura de un elemento se escribe siempre.
        private static void AppendDueRecords(StringBuilder text, List<RecordedItem> items,
            Dictionary<string, double?> values, TextId typeId, string timestamp, long elapsedSeconds)
        {
            string typeName = Translator.Translate(typeId);
            string nameLabel = Translator.Translate(TextId.RecordNameLabel);
            string valueLabel = Translator.Translate(TextId.RecordValueLabel);

            foreach (RecordedItem item in items)
            {
                ItemConfig config = item.Config;
                if (!config.IncludedInLog) continue;

                item.SecondsSinceLastLog += elapsedSeconds;
                if (item.HasBeenLogged && item.SecondsSinceLastLog < config.IntervalSeconds) continue;

                double? value;
                if (!values.TryGetValue(item.Identifier, out value) || value == null) continue;

                if (item.HasBeenLogged && Math.Abs(value.Value - item.LastLoggedValue) < config.Threshold) continue;

                item.LastLoggedValue = value.Value;
                item.HasBeenLogged = true;
                item.SecondsSinceLastLog = 0;

                text.Append("[" + timestamp + "] - " + typeName + ": " + nameLabel + " = " + item.Name
                    + " | " + valueLabel + " = " + value.Value.ToString("0.#", CultureInfo.InvariantCulture) + "\r\n");
            }
        }

        // Convierte la lista de lecturas en un diccionario por Identifier, para buscar cada una al instante.
        private static Dictionary<string, double?> ToValues(List<RemoteReading> readings)
        {
            Dictionary<string, double?> values = new Dictionary<string, double?>();
            if (readings == null) return values;

            foreach (RemoteReading reading in readings)
                if (reading.Identifier != null) values[reading.Identifier] = reading.Value;

            return values;
        }

        // Resumen del perfil con el mismo formato que el de CurveReader (tres líneas "="), pero sin la
        // sección de curvas y con el PC remoto en lugar de la ruta del archivo del perfil.
        private string BuildProfileSummary(RemoteProfileInfo profile)
        {
            string separator = new string('=', RemoteLogFile.SummaryWidth);
            StringBuilder text = new StringBuilder("\r\n\r\n");

            text.Append(RemoteLogFile.BuildHeader(Translator.Translate(TextId.ActiveProfile), RemoteLogFile.SummaryWidth)).Append("\r\n");
            text.Append(Translator.Translate(TextId.ProfileNameLabel)).Append(' ').Append(GetDisplayProfileName(profile.ProfileName)).Append("\r\n");
            text.Append(Translator.Translate(TextId.LogRemotePc)).Append(' ').Append(_remoteAddress).Append("\r\n");
            text.Append(separator).Append("\r\n\r\n");

            text.Append(Translator.Translate(TextId.ControlsHeading)).Append("\r\n");
            foreach (RecordedItem item in _controls) text.Append("- ").Append(item.Name).Append("\r\n");

            text.Append("\r\n").Append(Translator.Translate(TextId.SensorsHeading)).Append("\r\n");
            foreach (RecordedItem item in _sensors) text.Append("- ").Append(item.Name).Append("\r\n");

            text.Append(separator).Append("\r\n\r\n");
            return text.ToString();
        }

        // El servidor envía el nombre del archivo del perfil; se muestra sin la extensión ".json".
        private static string GetDisplayProfileName(string profileName)
        {
            if (profileName != null && profileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                return profileName.Substring(0, profileName.Length - ".json".Length);

            return profileName;
        }

        private void WriteToLog(string text)
        {
            _logFile.Write(text);
            _hasWritten = true;
        }

        // Pone la marca de fin de log, pero solo si esta grabación llegó a escribir algo, para no dejar
        // archivos con solo la marca cuando falla la conexión desde el principio.
        private void CloseLog()
        {
            if (!_hasWritten) return;

            try
            {
                _logFile.WriteEndOfLogMarker();
            }
            catch (Exception)
            {
                // Si ni la marca se puede escribir, no hay nada más que hacer.
            }
        }

        private void SetStatus(RecorderStatus status, string message)
        {
            if (_currentStatus == status) return;

            _currentStatus = status;
            StatusChanged?.Invoke(this, new RecorderStatusEventArgs(status, message));
        }

        // Un control o sensor vigilado: su ajuste (compartido con la interfaz, por eso los cambios del
        // usuario se aplican al instante) y el estado necesario para decidir cuándo escribirlo.
        private class RecordedItem
        {
            public RecordedItem(RemoteElementInfo element, ItemConfig config)
            {
                Identifier = element.Identifier;
                Name = string.IsNullOrWhiteSpace(element.Name) ? element.Identifier : element.Name;
                Config = config;
            }

            public string Identifier { get; }
            public string Name { get; }
            public ItemConfig Config { get; }
            public double LastLoggedValue { get; set; }
            public bool HasBeenLogged { get; set; }
            public long SecondsSinceLastLog { get; set; }
        }
    }
}