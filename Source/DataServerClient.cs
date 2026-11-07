using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace WatchTower
{
    /// <summary>Resultado de una consulta a DataServer.</summary>
    public enum DataServerStatus
    {
        /// <summary>Respuesta correcta; los datos están en Data.</summary>
        Ok,

        /// <summary>El servidor aún no tiene datos (arrancando o cargando un perfil). Conviene reintentar.</summary>
        NotReady,

        /// <summary>La contraseña no es la correcta.</summary>
        Unauthorized,

        /// <summary>El servidor tiene demasiados números pendientes. Conviene reintentar en unos segundos.</summary>
        Busy,

        /// <summary>No se pudo conectar: PC apagado, IP o puerto erróneos, FanControl cerrado, tiempo agotado.</summary>
        Unreachable,

        /// <summary>El servidor respondió algo que no se entiende.</summary>
        InvalidResponse
    }

    /// <summary>Respuesta de DataServer: un estado y, si fue Ok, los datos.</summary>
    public class DataServerResponse<T> where T : class
    {
        public DataServerStatus Status { get; private set; }
        public T Data { get; private set; }

        /// <summary>Detalle técnico del fallo, para mostrarlo o registrarlo. Es null si todo fue bien.</summary>
        public string Message { get; private set; }

        public bool IsOk => Status == DataServerStatus.Ok;

        public static DataServerResponse<T> Success(T data)
        {
            return new DataServerResponse<T> { Status = DataServerStatus.Ok, Data = data };
        }

        public static DataServerResponse<T> Failure(DataServerStatus status, string message)
        {
            return new DataServerResponse<T> { Status = status, Message = message };
        }
    }

    /// <summary>Un sensor o control tal como lo describe /profile.</summary>
    public class RemoteElementInfo
    {
        public string Identifier { get; set; }
        public string Name { get; set; }

        /// <summary>Sensores: identificador del sensor físico. Controles: nombre de la curva asignada.</summary>
        public string DataSource { get; set; }
    }

    /// <summary>Respuesta de /profile: el perfil activo del PC remoto y sus elementos.</summary>
    public class RemoteProfileInfo
    {
        /// <summary>Sube cada vez que el servidor prepara un perfil, aunque sea el mismo.</summary>
        public int ProfileId { get; set; }

        public string ProfileName { get; set; }
        public List<RemoteElementInfo> Sensors { get; set; }
        public List<RemoteElementInfo> Controls { get; set; }
    }

    /// <summary>Una lectura de un sensor o control. Value es null si FanControl no la reportó.</summary>
    public class RemoteReading
    {
        public string Identifier { get; set; }
        public double? Value { get; set; }
    }

    /// <summary>Respuesta de /readings: todas las lecturas de un instante.</summary>
    public class RemoteReadingsSnapshot
    {
        /// <summary>Sube en cada foto nueva. Si deja de subir, los datos están parados.</summary>
        public long Sequence { get; set; }

        /// <summary>Hora UTC de captura, con formato dd-MM-yy HH:mm:ss.</summary>
        public string TimestampUtc { get; set; }

        /// <summary>Perfil al que pertenecen las lecturas. Si no coincide con el de /profile, hay que volver a pedirlo.</summary>
        public int ProfileId { get; set; }

        public List<RemoteReading> Sensors { get; set; }
        public List<RemoteReading> Controls { get; set; }
    }

    /// <summary>
    /// Cliente de DataServer. Cada consulta de datos son dos peticiones: primero se pide a /challenge un
    /// número de un solo uso y después se pide /profile o /readings con la cabecera
    ///     Authorization: HMAC &lt;número&gt;:&lt;huella&gt;
    /// donde la huella es HMAC-SHA256 con la contraseña como clave y el número como mensaje. La
    /// contraseña no viaja nunca. No guarda estado entre consultas, así que se puede llamar desde
    /// cualquier hilo; las consultas son síncronas, por lo que deben hacerse fuera del hilo de la interfaz.
    /// </summary>
    public class DataServerClient
    {
        private const string AuthorizationScheme = "HMAC ";
        private const int RequestTimeoutMilliseconds = 3000;

        private readonly string _host;
        private readonly int _port;
        private readonly string _password;

        /// <param name="host">Dirección IP o nombre del PC remoto.</param>
        /// <param name="port">Puerto en el que escucha DataServer.</param>
        /// <param name="password">Contraseña de DataServer.</param>
        public DataServerClient(string host, int port, string password)
        {
            _host = host;
            _port = port;
            _password = password;
        }

        /// <summary>Pide el perfil activo del PC remoto con la lista de sus sensores y controles.</summary>
        public DataServerResponse<RemoteProfileInfo> GetProfile()
        {
            return GetAuthenticated<RemoteProfileInfo>("/profile");
        }

        /// <summary>Pide las últimas lecturas de todos los sensores y controles del perfil activo.</summary>
        public DataServerResponse<RemoteReadingsSnapshot> GetReadings()
        {
            return GetAuthenticated<RemoteReadingsSnapshot>("/readings");
        }

        // Pide un número de un solo uso, calcula la huella con la contraseña y hace la petición de
        // datos con la cabecera de autorización.
        private DataServerResponse<T> GetAuthenticated<T>(string path) where T : class
        {
            DataServerResponse<ChallengeResponse> challenge = Get<ChallengeResponse>("/challenge", null);
            if (!challenge.IsOk)
                return DataServerResponse<T>.Failure(challenge.Status, challenge.Message);

            string nonce = challenge.Data.Nonce;
            if (string.IsNullOrEmpty(nonce))
                return DataServerResponse<T>.Failure(DataServerStatus.InvalidResponse, "Empty challenge");

            string header = AuthorizationScheme + nonce + ":" + ComputeHmac(_password, nonce);
            return Get<T>(path, header);
        }

        // Hace una petición GET y convierte la respuesta JSON en el tipo pedido. Los errores de red y
        // los códigos de error de HTTP no lanzan excepciones: se devuelven como estado de la respuesta.
        // KeepAlive desactivado y Proxy nulo: el servidor cierra la conexión tras cada respuesta y en una
        // red local no tiene sentido pasar por el proxy del sistema.
        private DataServerResponse<T> Get<T>(string path, string authorizationHeader) where T : class
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://" + _host + ":" + _port + path);
                request.Method = "GET";
                request.Timeout = RequestTimeoutMilliseconds;
                request.ReadWriteTimeout = RequestTimeoutMilliseconds;
                request.KeepAlive = false;
                request.Proxy = null;

                if (authorizationHeader != null)
                    request.Headers[HttpRequestHeader.Authorization] = authorizationHeader;

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    return ReadBody<T>(response);
            }
            catch (WebException ex)
            {
                // Sin respuesta es un fallo de conexión; con respuesta, el código dice qué pasó.
                HttpWebResponse errorResponse = ex.Response as HttpWebResponse;
                if (errorResponse == null)
                    return DataServerResponse<T>.Failure(DataServerStatus.Unreachable, ex.Message);

                using (errorResponse)
                    return DataServerResponse<T>.Failure(StatusFromHttpCode(errorResponse.StatusCode), ex.Message);
            }
            catch (Exception ex)
            {
                // Dirección con formato inválido u otro fallo al preparar la petición.
                return DataServerResponse<T>.Failure(DataServerStatus.Unreachable, ex.Message);
            }
        }

        // Lee el cuerpo JSON de una respuesta correcta y lo convierte al tipo pedido.
        private static DataServerResponse<T> ReadBody<T>(HttpWebResponse response) where T : class
        {
            try
            {
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                {
                    T data = JsonConvert.DeserializeObject<T>(reader.ReadToEnd());
                    if (data == null)
                        return DataServerResponse<T>.Failure(DataServerStatus.InvalidResponse, "Empty response");

                    return DataServerResponse<T>.Success(data);
                }
            }
            catch (JsonException ex)
            {
                return DataServerResponse<T>.Failure(DataServerStatus.InvalidResponse, ex.Message);
            }
        }

        // Traduce el código HTTP de un error al estado que entiende el resto del programa.
        private static DataServerStatus StatusFromHttpCode(HttpStatusCode code)
        {
            switch ((int)code)
            {
                case 401: return DataServerStatus.Unauthorized;
                case 429: return DataServerStatus.Busy;
                case 503: return DataServerStatus.NotReady;
                default: return DataServerStatus.InvalidResponse;
            }
        }

        // Huella HMAC-SHA256 del número con la contraseña como clave, en hexadecimal y minúsculas.
        // Debe dar exactamente lo mismo que HmacAuth.Compute del servidor.
        private static string ComputeHmac(string password, string nonce)
        {
            using (HMACSHA256 hmac = new HMACSHA256(Encoding.UTF8.GetBytes(password)))
            {
                byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(nonce));
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }

        // Respuesta de /challenge. Newtonsoft ignora mayúsculas al leer, así que "nonce" encaja con Nonce.
        private class ChallengeResponse
        {
            public string Nonce { get; set; }
        }
    }
}