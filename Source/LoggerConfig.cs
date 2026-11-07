using System;
using System.Collections.Generic;

namespace WatchTower
{
    /// <summary>
    /// Ajustes del logger para un control o un sensor. Los valores por defecto reproducen el
    /// comportamiento que tenía el plugin antes de existir la configuración.
    /// </summary>
    public class ItemConfig
    {
        public const bool DefaultIncludedInLog = true;
        public const int DefaultThreshold = 1;
        public const int MinThreshold = 0;
        public const int MaxThreshold = 10;
        public const int DefaultIntervalSeconds = 1;
        public const int MinIntervalSeconds = 1;
        public const int MaxIntervalSeconds = 3600;

        /// <summary>
        /// Segundos que el elemento descansa tras escribir una línea en el log: durante ese tiempo
        /// no se escribe nada de él, aunque su valor cambie.
        /// </summary>
        public int IntervalSeconds { get; set; } = DefaultIntervalSeconds;

        /// <summary>Identificador de FanControl: es lo que empareja estos ajustes con el elemento.</summary>
        public string Identifier { get; set; }

        /// <summary>Nombre del elemento cuando se guardó. Solo informativo, para poder leer el archivo.</summary>
        public string Name { get; set; }

        /// <summary>Si es false, el elemento no se registra en el log.</summary>
        public bool IncludedInLog { get; set; } = DefaultIncludedInLog;

        /// <summary>
        /// Cambio mínimo, en unidades enteras, respecto al último valor registrado
        /// para que se escriba una nueva línea en el log. Con 0 se escribe siempre
        /// que se cumpla el intervalo, aunque el valor no haya cambiado.
        /// </summary>
        public int Threshold { get; set; } = DefaultThreshold;
    }

    /// <summary>
    /// Ajustes de todos los controles y sensores de un perfil.
    /// </summary>
    public class ProfileConfig
    {
        /// <summary>Archivo del perfil, con extensión (por ejemplo "verano.json").</summary>
        public string FileName { get; set; }

        public List<ItemConfig> Controls { get; } = new List<ItemConfig>();
        public List<ItemConfig> Sensors { get; } = new List<ItemConfig>();
    }

    /// <summary>
    /// Configuración completa del logger: los ajustes de todos los perfiles.
    /// </summary>
    public class LoggerConfig
    {
        public const int CurrentVersion = 1;

        /// <summary>
        /// Idioma elegido en el configurador (el nombre de la clase de idioma, por ejemplo "Español").
        /// El plugin escribe el log en ese idioma. Es null si todavía no se ha elegido ninguno.
        /// </summary>
        public string LanguageName { get; set; }

        public List<ProfileConfig> Profiles { get; } = new List<ProfileConfig>();

        /// <summary>
        /// Conexión al DataServer remoto. Solo la usa el grabador remoto; es null en el XML de CurveReader.
        /// </summary>
        public RemoteConnectionConfig RemoteConnection { get; set; }

        /// <summary>
        /// Devuelve la configuración del perfil indicado, o null si no tiene ninguna guardada.
        /// </summary>
        public ProfileConfig FindProfile(string fileName)
        {
            foreach (ProfileConfig profile in Profiles)
                if (string.Equals(profile.FileName, fileName, StringComparison.OrdinalIgnoreCase)) return profile;

            return null;
        }
    }

    /// <summary>
    /// Datos de conexión al DataServer de otro PC, que usa la pestaña del grabador remoto. Todos son
    /// texto, igual que en RemoteSensorsConfig.xml: el puerto vacío significa "el de por defecto".
    /// </summary>
    public class RemoteConnectionConfig
    {
        public string PcName { get; set; } = "";
        public string Ip { get; set; } = "";
        public string Port { get; set; } = "";
        public string Password { get; set; } = "";
    }
}