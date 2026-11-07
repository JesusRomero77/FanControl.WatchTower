using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WatchTower
{
    /// <summary>
    /// Pestaña del grabador remoto: se conecta al DataServer de otro PC (nombre o IP, puerto y contraseña),
    /// muestra el perfil activo de ese PC con los mismos ajustes de registro que CurveReader (sin curvas) y,
    /// con el botón REC, graba en un log las lecturas que cumplan esos ajustes. Lee y escribe
    /// RemoteSensorLoggerConfig.xml en %LocalAppData%\FanControl.
    /// </summary>
    public partial class TabRemoteSensorLogger : UserControl, IConfiguratorTab
    {
        // Nombre de la pestaña. No se traduce.
        private const string TabTitle = "RemoteSensorLogger";

        // Puerto por defecto de DataServer: el que se usa si el campo está vacío.
        private const int DefaultPort = 8765;
        private const int MinPort = 1;
        private const int MaxPort = 65535;

        // Mensaje de Windows que pone el texto gris de ejemplo en un TextBox.
        private const int EmSetCueBanner = 0x1501;

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, string lParam);

        // Valores de conexión que muestran los campos tras leer o guardar el XML. Con ellos se detecta si
        // el usuario ha cambiado algo. Puerto vacío significa "usar el 8765".
        private string _savedPcName = "";
        private string _shownIp = "";
        private string _savedPort = "";
        private string _savedPassword = "";
        private bool _settingsLoaded;

        // Ajustes de cada perfil remoto ya visto en esta sesión, con el nombre del perfil como clave, y
        // lo que había guardado en el XML. Los usa también el hilo del grabador, así que todo acceso va
        // dentro de este candado.
        private readonly object _configLock = new object();
        private readonly Dictionary<string, ProfileConfig> _openedProfileConfigs =
            new Dictionary<string, ProfileConfig>(StringComparer.OrdinalIgnoreCase);
        private LoggerConfig _storedConfig = new LoggerConfig();

        // Perfiles (por nombre) cuyos ajustes ha cambiado el usuario desde el último guardado. Solo se
        // toca desde el hilo de la interfaz.
        private readonly HashSet<string> _changedProfileFileNames =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Ajustes del perfil que muestra el panel; null si aún no hay ninguno.
        private ProfileConfig _profileConfig;

        // Grabación en curso (null si no se está grabando) y último estado conocido de la pestaña.
        // Si la grabación se detuvo por un fallo (contraseña incorrecta, error de escritura), ese
        // estado se mantiene en pantalla después de detenerse.
        private RemoteLogRecorder _recorder;
        private RecorderStatus _status = RecorderStatus.Stopped;
        private string _statusDetail;
        private RecorderStatus? _failure;
        private string _failureDetail;
        private string _logFileName = "";

        // Esta pestaña no necesita la carpeta de FanControl, pero la interfaz obliga a tener el evento.
#pragma warning disable CS0067
        public event EventHandler FanControlPathRequested;
#pragma warning restore CS0067

        public TabRemoteSensorLogger()
        {
            InitializeComponent();

            profileConfigPanel.ShowCurvesSection = false;
            profileConfigPanel.ItemChanged += (sender, e) => MarkProfileChanged();

            // El texto gris solo se puede poner cuando el TextBox ya tiene handle de Windows, y el
            // handle puede recrearse, así que se pone cada vez que se crea.
            portTextBox.HandleCreated += (sender, e) =>
                SetCueBanner(portTextBox, DefaultPort.ToString(CultureInfo.InvariantCulture));

            // Al cerrar la app se detiene la grabación y se espera un momento a que escriba la marca de
            // fin de log.
            Disposed += (sender, e) =>
            {
                RemoteLogRecorder recorder = _recorder;
                if (recorder == null) return;

                recorder.Stop();
                recorder.WaitUntilStopped(3000);
            };
        }

        public string Title => TabTitle;

        // Lo que hay ahora escrito en cada campo, recortado por los extremos. La IP es "" mientras esté
        // vacía o a medias.
        private string CurrentPcName => pcNameTextBox.Text.Trim();
        private string CurrentIp => ipAddressBox.Value;
        private string CurrentPort => portTextBox.Text.Trim();
        private string CurrentPassword => passwordTextBox.Text.Trim();

        // True si la IP tiene algunos números pero no los cuatro: no se puede usar ni guardar.
        private bool HasPartialIp => !ipAddressBox.IsEmpty && !ipAddressBox.IsComplete;

        private bool HasConnectionChanged =>
            CurrentPcName != _savedPcName || CurrentIp != _shownIp ||
            CurrentPort != _savedPort || CurrentPassword != _savedPassword;

        public bool HasUnsavedChanges =>
            _settingsLoaded && (HasConnectionChanged || HasPartialIp || _changedProfileFileNames.Count > 0);

        // Escribe en cada etiqueta el texto del idioma actual y reconstruye el panel de ajustes. La
        // primera vez que se llama (al abrir la app) se leen también los valores del XML.
        public void ApplyLanguage()
        {
            if (!_settingsLoaded) LoadSettings();

            pcNameLabel.Text = Translator.Translate(TextId.RemotePcName);
            ipLabel.Text = Translator.Translate(TextId.RemoteIp);
            portLabel.Text = Translator.Translate(TextId.RemotePort);
            passwordLabel.Text = Translator.Translate(TextId.DataServerPassword);
            profileLabel.Text = Translator.Translate(TextId.Profile);
            mainToolTip.SetToolTip(refreshProfileButton, Translator.Translate(TextId.RefreshRemoteProfile));

            UpdateRecButton();
            UpdateStatus();
            profileConfigPanel.Build(_profileConfig);
        }

        // Esta pestaña no usa la carpeta de FanControl.
        public void SetFanControlFolder(string fanControlFolder)
        {
        }

        // Pone el texto gris de ejemplo en un TextBox (no hace nada si aún no tiene handle).
        private static void SetCueBanner(TextBox textBox, string text)
        {
            if (textBox.IsHandleCreated)
                SendMessage(textBox.Handle, EmSetCueBanner, (IntPtr)1, text);
        }

        // Lee del XML la conexión y los ajustes de los perfiles. Si el archivo no existe, los campos
        // quedan vacíos. Si existe pero no se puede leer, también; Save avisará del problema antes de
        // escribir nada.
        private void LoadSettings()
        {
            _settingsLoaded = true;

            string error;
            LoggerConfig stored = LoggerConfigFile.Load(LoggerConfigFile.RemoteFilePath, out error);
            lock (_configLock)
                _storedConfig = stored ?? new LoggerConfig();

            RemoteConnectionConfig connection = stored?.RemoteConnection ?? new RemoteConnectionConfig();

            int port;
            _savedPcName = connection.PcName;
            _savedPort = TryParsePort(connection.Port, out port) ? port.ToString(CultureInfo.InvariantCulture) : "";
            _savedPassword = connection.Password;

            pcNameTextBox.Text = _savedPcName;
            ipAddressBox.Value = connection.Ip;
            _shownIp = ipAddressBox.Value;
            portTextBox.Text = _savedPort;
            passwordTextBox.Text = _savedPassword;
        }

        // Guarda en RemoteSensorLoggerConfig.xml la conexión y los perfiles modificados, sin tocar el resto
        // de lo que ya hubiera. Sin cambios no toca el archivo, salvo que no exista: entonces lo crea.
        // Una IP a medias, un puerto fuera de rango o una contraseña con caracteres no válidos no se
        // guardan: se devuelve un error.
        public bool Save(out string error)
        {
            error = null;

            if (HasPartialIp)
            {
                error = Translator.Translate(TextId.RemoteIpIncomplete);
                return false;
            }

            bool fileExists = File.Exists(LoggerConfigFile.RemoteFilePath);
            if (!HasUnsavedChanges && fileExists) return true;

            string normalizedPort = "";
            if (CurrentPort.Length > 0)
            {
                int port;
                if (!TryParsePort(CurrentPort, out port))
                {
                    error = string.Format(Translator.Translate(TextId.DataServerPortInvalid), MinPort, MaxPort);
                    return false;
                }
                normalizedPort = port.ToString(CultureInfo.InvariantCulture);
            }

            if (CurrentPassword.Length > 0 && !PasswordRules.IsValid(CurrentPassword))
            {
                error = Translator.Translate(TextId.DataServerPasswordInvalid);
                return false;
            }

            LoggerConfig storedConfig = LoggerConfigFile.Load(LoggerConfigFile.RemoteFilePath, out error);
            if (storedConfig == null) return false;

            storedConfig.RemoteConnection = new RemoteConnectionConfig
            {
                PcName = CurrentPcName,
                Ip = CurrentIp,
                Port = normalizedPort,
                Password = CurrentPassword
            };

            // Cada perfil modificado reemplaza en su sitio al guardado con el mismo nombre, o se añade
            // al final si aún no tenía configuración.
            lock (_configLock)
            {
                foreach (string fileName in _changedProfileFileNames)
                {
                    ProfileConfig changedConfig = _openedProfileConfigs[fileName];
                    ProfileConfig existingConfig = storedConfig.FindProfile(fileName);

                    if (existingConfig == null)
                        storedConfig.Profiles.Add(changedConfig);
                    else
                        storedConfig.Profiles[storedConfig.Profiles.IndexOf(existingConfig)] = changedConfig;
                }
            }

            if (!LoggerConfigFile.Save(LoggerConfigFile.RemoteFilePath, storedConfig, out error)) return false;

            // Lo guardado pasa a ser lo mostrado. El puerto y la contraseña se reescriben normalizados.
            _savedPcName = CurrentPcName;
            _shownIp = CurrentIp;
            _savedPort = normalizedPort;
            _savedPassword = CurrentPassword;
            portTextBox.Text = normalizedPort;
            passwordTextBox.Text = CurrentPassword;
            _changedProfileFileNames.Clear();

            // Se vuelve a leer lo guardado para que el grabador no comparta objetos con lo que edita el usuario.
            string reloadError;
            LoggerConfig reloaded = LoggerConfigFile.Load(LoggerConfigFile.RemoteFilePath, out reloadError);
            if (reloaded != null)
            {
                lock (_configLock)
                    _storedConfig = reloaded;
            }

            return true;
        }

        // Anota que el perfil que se está mostrando tiene cambios sin guardar.
        private void MarkProfileChanged()
        {
            if (_profileConfig != null) _changedProfileFileNames.Add(_profileConfig.FileName);
        }

        // Botón REC / STOP. Si se está grabando, pide parar y bloquea el botón hasta que el grabador
        // confirme que ha terminado (así no se puede empezar otra grabación sobre la anterior).
        private void RecButton_Click(object sender, EventArgs e)
        {
            if (_recorder != null)
            {
                recButton.Enabled = false;
                _recorder.Stop();
                return;
            }

            StartRecording();
        }

        // Valida la conexión y arranca una grabación nueva. Los campos de conexión se bloquean mientras
        // dure; el panel de ajustes sigue editable.
        private void StartRecording()
        {
            string host;
            int port;
            string password;
            if (!TryGetConnection(out host, out port, out password)) return;

            RemoteLogFile logFile = new RemoteLogFile(RemoteLogFile.GetFilePath(host));
            DataServerClient client = new DataServerClient(host, port, password);
            RemoteLogRecorder recorder = new RemoteLogRecorder(
                client, logFile, host + ":" + port.ToString(CultureInfo.InvariantCulture), ProvideProfileConfig);
            recorder.StatusChanged += (sender, e) => PostToUi(() => OnRecorderStatusChanged(recorder, e));

            _logFileName = Path.GetFileName(RemoteLogFile.GetFilePath(host));
            _failure = null;
            _failureDetail = null;
            _recorder = recorder;
            SetRecordingUi(true);

            recorder.Start();
        }

        // Llegan los cambios de estado del grabador (ya en el hilo de la interfaz). Se ignoran los de un
        // grabador que ya no es el actual. Cuando la grabación termina por un fallo, el estado del fallo
        // se queda en pantalla.
        private void OnRecorderStatusChanged(RemoteLogRecorder recorder, RecorderStatusEventArgs e)
        {
            if (recorder != _recorder) return;

            if (e.Status == RecorderStatus.Unauthorized || e.Status == RecorderStatus.Failed)
            {
                _failure = e.Status;
                _failureDetail = e.Message;
            }

            if (e.Status == RecorderStatus.Stopped)
            {
                _recorder = null;
                _status = _failure ?? RecorderStatus.Stopped;
                _statusDetail = _failureDetail;
                SetRecordingUi(false);
            }
            else
            {
                _status = e.Status;
                _statusDetail = e.Message;
            }

            UpdateStatus();
        }

        // Botón ↻: pide el perfil activo del PC remoto y lo muestra en el panel, para poder configurarlo
        // sin grabar. La petición va en otro hilo para no congelar la ventana.
        private async void RefreshProfileButton_Click(object sender, EventArgs e)
        {
            if (_recorder != null) return;

            string host;
            int port;
            string password;
            if (!TryGetConnection(out host, out port, out password)) return;

            DataServerClient client = new DataServerClient(host, port, password);
            refreshProfileButton.Enabled = false;
            SetTransientStatus(Translator.Translate(TextId.LoggerStatusConnecting), SystemColors.ControlText);

            DataServerResponse<RemoteProfileInfo> response;
            try
            {
                response = await Task.Run(() => client.GetProfile());
            }
            catch (Exception ex)
            {
                if (IsDisposed) return;
                refreshProfileButton.Enabled = _recorder == null;
                SetTransientStatus(string.Format(Translator.Translate(TextId.LoggerStatusFailed), ex.Message), Color.Firebrick);
                return;
            }

            if (IsDisposed) return;
            refreshProfileButton.Enabled = _recorder == null;

            switch (response.Status)
            {
                case DataServerStatus.Ok:
                    ShowProfile(GetOrCreateProfileConfig(response.Data));
                    SetTransientStatus(Translator.Translate(TextId.LoggerStatusProfileLoaded), Color.DarkGreen);
                    break;
                case DataServerStatus.Unauthorized:
                    SetTransientStatus(Translator.Translate(TextId.LoggerStatusUnauthorizedShort), Color.Firebrick);
                    break;
                case DataServerStatus.Unreachable:
                    SetTransientStatus(Translator.Translate(TextId.LoggerStatusUnreachableShort), Color.Firebrick);
                    break;
                case DataServerStatus.Busy:
                    SetTransientStatus(Translator.Translate(TextId.LoggerStatusBusy), Color.Firebrick);
                    break;
                default:
                    SetTransientStatus(Translator.Translate(TextId.LoggerStatusNoProfile), Color.Firebrick);
                    break;
            }
        }

        // Función que usa el grabador cada vez que recibe un perfil nuevo. Corre en el hilo del grabador:
        // calcula los ajustes con el candado y avisa a la interfaz (sin esperarla) para que los muestre.
        private ProfileConfig ProvideProfileConfig(RemoteProfileInfo profile)
        {
            ProfileConfig config = GetOrCreateProfileConfig(profile);
            PostToUi(() => ShowProfile(config));
            return config;
        }

        // Devuelve los ajustes del perfil remoto. Si ya había unos con los mismos controles y sensores,
        // se devuelven tal cual (con los cambios que el usuario haya hecho). Si el perfil cambió, se crean
        // unos nuevos partiendo de lo ya editado o, si no, de lo guardado en el XML; lo que sea nuevo
        // lleva los valores por defecto.
        private ProfileConfig GetOrCreateProfileConfig(RemoteProfileInfo profile)
        {
            string key = GetProfileKey(profile);
            ProfileContent content = ToProfileContent(profile);

            lock (_configLock)
            {
                ProfileConfig current;
                _openedProfileConfigs.TryGetValue(key, out current);
                if (current != null && HasSameItems(current, content)) return current;

                ProfileConfig basis = current ?? _storedConfig.FindProfile(key);
                ProfileConfig config = ProfileConfigBuilder.Build(key, content, basis);
                _openedProfileConfigs[key] = config;
                return config;
            }
        }

        // Muestra en el panel los ajustes de un perfil (en el hilo de la interfaz).
        private void ShowProfile(ProfileConfig config)
        {
            if (_profileConfig == config) return;

            _profileConfig = config;
            profileNameLabel.Text = GetDisplayProfileName(config.FileName);
            profileConfigPanel.Build(config);
        }

        // El perfil del servidor se identifica por su nombre de archivo; si faltara, se usa uno genérico.
        private static string GetProfileKey(RemoteProfileInfo profile)
        {
            return string.IsNullOrWhiteSpace(profile.ProfileName) ? "RemoteProfile" : profile.ProfileName;
        }

        // El servidor envía el nombre del archivo del perfil; se muestra sin la extensión ".json".
        private static string GetDisplayProfileName(string profileName)
        {
            if (profileName != null && profileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                return profileName.Substring(0, profileName.Length - ".json".Length);

            return profileName;
        }

        // Convierte el perfil recibido del servidor en el formato que usa ProfileConfigBuilder.
        private static ProfileContent ToProfileContent(RemoteProfileInfo profile)
        {
            ProfileContent content = new ProfileContent();
            AddLoggableItems(content.Controls, profile.Controls);
            AddLoggableItems(content.Sensors, profile.Sensors);
            return content;
        }

        // Pasa los elementos del servidor a elementos registrables, con las mismas reglas que el grabador:
        // sin identificador no se tienen en cuenta y, sin nombre, se usa el identificador.
        private static void AddLoggableItems(List<LoggableItem> target, List<RemoteElementInfo> elements)
        {
            if (elements == null) return;

            foreach (RemoteElementInfo element in elements)
            {
                if (string.IsNullOrEmpty(element.Identifier)) continue;

                target.Add(new LoggableItem
                {
                    Identifier = element.Identifier,
                    Name = string.IsNullOrWhiteSpace(element.Name) ? element.Identifier : element.Name
                });
            }
        }

        // True si los ajustes ya tienen exactamente los mismos controles y sensores (identificador y
        // nombre, en el mismo orden) que el contenido del perfil.
        private static bool HasSameItems(ProfileConfig config, ProfileContent content)
        {
            return SameItems(config.Controls, content.Controls) && SameItems(config.Sensors, content.Sensors);
        }

        private static bool SameItems(List<ItemConfig> configs, List<LoggableItem> items)
        {
            return configs.Select(config => config.Identifier + "\0" + config.Name)
                .SequenceEqual(items.Select(item => item.Identifier + "\0" + item.Name), StringComparer.Ordinal);
        }

        // Comprueba los campos de conexión. Si todo está bien devuelve true con el destino (la IP si está
        // rellena; si no, el nombre), el puerto (el 8765 si está vacío) y la contraseña. Si no, muestra
        // el motivo y devuelve false.
        private bool TryGetConnection(out string host, out int port, out string password)
        {
            host = null;
            port = DefaultPort;
            password = null;
            string message = null;

            if (HasPartialIp)
            {
                message = Translator.Translate(TextId.RemoteIpIncomplete);
            }
            else
            {
                host = CurrentIp.Length > 0 ? CurrentIp : CurrentPcName;

                if (host.Length == 0)
                    message = Translator.Translate(TextId.LoggerNeedsAddress);
                else if (CurrentPort.Length > 0 && !TryParsePort(CurrentPort, out port))
                    message = string.Format(Translator.Translate(TextId.DataServerPortInvalid), MinPort, MaxPort);
                else if (CurrentPassword.Length == 0)
                    message = Translator.Translate(TextId.LoggerNeedsPassword);
                else if (!PasswordRules.IsValid(CurrentPassword))
                    message = Translator.Translate(TextId.DataServerPasswordInvalid);
            }

            if (message != null)
            {
                MessageBox.Show(this, message, Translator.Translate(TextId.ErrorTitle),
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            password = CurrentPassword;
            return true;
        }

        // Bloquea (o desbloquea) los campos de conexión y el botón ↻ según haya o no una grabación en curso.
        private void SetRecordingUi(bool recording)
        {
            pcNameTextBox.Enabled = !recording;
            ipAddressBox.Enabled = !recording;
            portTextBox.Enabled = !recording;
            passwordTextBox.Enabled = !recording;
            refreshProfileButton.Enabled = !recording;
            recButton.Enabled = true;
            UpdateRecButton();
        }

        private void UpdateRecButton()
        {
            bool recording = _recorder != null;
            recButton.Text = recording
                ? "■ " + Translator.Translate(TextId.RecStop)
                : "● " + Translator.Translate(TextId.RecStart);
            recButton.ForeColor = recording ? SystemColors.ControlText : Color.Firebrick;
        }

        // Escribe en la etiqueta de estado el texto y el color que corresponden al estado de la grabación.
        private void UpdateStatus()
        {
            switch (_status)
            {
                case RecorderStatus.Connecting:
                    SetTransientStatus(Translator.Translate(TextId.LoggerStatusConnecting), SystemColors.ControlText);
                    break;
                case RecorderStatus.Recording:
                    SetTransientStatus(string.Format(Translator.Translate(TextId.LoggerStatusRecording), _logFileName),
                        Color.DarkGreen);
                    break;
                case RecorderStatus.DataStale:
                    SetTransientStatus(Translator.Translate(TextId.LoggerStatusStale), Color.Firebrick);
                    break;
                case RecorderStatus.Unreachable:
                    SetTransientStatus(Translator.Translate(TextId.LoggerStatusUnreachable), Color.Firebrick);
                    break;
                case RecorderStatus.Unauthorized:
                    SetTransientStatus(Translator.Translate(TextId.LoggerStatusUnauthorized), Color.Firebrick);
                    break;
                case RecorderStatus.Failed:
                    SetTransientStatus(string.Format(Translator.Translate(TextId.LoggerStatusFailed), _statusDetail),
                        Color.Firebrick);
                    break;
                default:
                    SetTransientStatus(Translator.Translate(TextId.LoggerStatusIdle), SystemColors.ControlText);
                    break;
            }
        }

        private void SetTransientStatus(string text, Color color)
        {
            statusLabel.Text = text;
            statusLabel.ForeColor = color;
        }

        // Ejecuta una acción en el hilo de la interfaz sin esperar a que termine. Si la pestaña ya se está
        // cerrando, no hace nada.
        private void PostToUi(Action action)
        {
            try
            {
                if (!IsDisposed && IsHandleCreated) BeginInvoke(action);
            }
            catch (InvalidOperationException)
            {
            }
        }

        // True si el texto es un número entero entre MinPort y MaxPort (solo dígitos).
        private static bool TryParsePort(string text, out int port)
        {
            return int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out port)
                   && port >= MinPort && port <= MaxPort;
        }

        // Solo deja escribir dígitos (y teclas de control como Retroceso) en el campo del puerto.
        private void PortTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
                e.Handled = true;
        }
    }
}