using System.Collections.Generic;

namespace WatchTower
{
    /// <summary>
    /// Base de todos los idiomas de la interfaz. Cada idioma es una clase que hereda de esta y
    /// contiene únicamente su nombre y su diccionario. La app descubre sola las clases que
    /// heredan de Language, así que para añadir un idioma basta con añadir su clase en este archivo.
    /// </summary>
    public abstract class Language
    {
        /// <summary>Nombre del idioma escrito en ese mismo idioma; es lo que se ve en el menú.</summary>
        public abstract string Name { get; }

        /// <summary>Un texto por cada TextId. Si falta alguno, se usará el del inglés.</summary>
        public abstract Dictionary<TextId, string> Texts { get; }
    }

    /// <summary>Idioma de reserva: debe tener todos los textos.</summary>
    public class English : Language
    {
        public override string Name => "English";

        public override Dictionary<TextId, string> Texts { get; } = new Dictionary<TextId, string>
        {
            { TextId.File, "File" },
            { TextId.Save, "Save" },
            { TextId.Exit, "Exit" },
            { TextId.Settings, "Settings" },
            { TextId.Language, "Language" },
            { TextId.FanControlPath, "Specify FanControl path" },
            { TextId.Help, "Help" },
            { TextId.ViewHelp, "View Help" },
            { TextId.About, "About..." },
            { TextId.Profile, "Profile:" },
            { TextId.RefreshProfiles, "Reload profiles" },
            { TextId.Controls, "Controls" },
            { TextId.Curves, "Curves" },
            { TextId.Sensors, "Sensors" },
            { TextId.Interval, "Interval (sec)" },
            { TextId.ChangeThreshold, "Minimum change" },
            { TextId.CurvesNotRecorded, "Curves are not recorded in the log yet." },
            { TextId.Log, "Log" },
            { TextId.AboutWindowTitle, "About WatchTower" },
            { TextId.AboutDescription, "WatchTower configures a family of FanControl monitoring plugins: " +
                "CurveReader ( logs sensors and controls to a file ), " +
                "DataServer ( shares them over the network ) and RemoteSensors ( shows another computer's sensors inside FanControl ). " +
                "It can also record the data of a remote FanControl into a local log.\r\n\r\n" +
                "This application is portable: it can be run from anywhere on disk without installing it." },
            { TextId.ViewOnGitHub, "View project on GitHub" },
            { TextId.AboutJsonNotice, "Includes Newtonsoft.Json (Copyright \u00A9 James Newton-King), used under the MIT License." },
            { TextId.Version, "Version" },
            { TextId.Ok, "OK" },
            { TextId.HelpWindowTitle, "WatchTower Help" },
            { TextId.HelpRequirementsTitle, "Requirements" },
            { TextId.HelpRequirementsText, "Windows with .NET Framework 4.8 installed. The CurveReader, DataServer and RemoteSensors tabs configure " +
                "plugins, so FanControl and the matching plugin must be installed on this PC. The RemoteSensorLogger tab does not need FanControl " +
                "here: it only needs network access to a PC running DataServer." },
            { TextId.HelpGettingStartedTitle, "Getting started" },
            { TextId.HelpGettingStartedText, "The first time you run the app, go to Settings > Specify FanControl " +
                "path and select FanControl.exe (or a shortcut to it). The app will locate your profiles automatically from then on." },
            { TextId.HelpTabsTitle, "The tabs" },
            { TextId.HelpTabsText, "There is one tab per plugin: CurveReader (logs this PC's sensors and controls), DataServer (shares this PC's " +
                "data over the network), RemoteSensors (shows another PC's sensors inside this FanControl) and RemoteSensorLogger (records " +
                "the data of another PC's DataServer into a log)." },
            { TextId.HelpSelectProfileTitle, "Selecting a profile" },
            { TextId.HelpSelectProfileText, "In the CurveReader tab, choose the profile you want to configure from the Profile dropdown. " +
                "In the RemoteSensorLogger tab, fill in the connection boxes and use the button to get the profile from the remote PC. " +
                "In both cases its controls and sensors will appear below, ready to edit." },
            { TextId.HelpConfigureItemsTitle, "Configuring sensors and controls" },
            { TextId.HelpConfigureItemsText, "In CurveReader and RemoteSensorLogger, for each item, the log checkbox decides whether it is logged at all. " +
                "\"Interval (sec)\" is the minimum time between two log entries for that item. " +
                "\"Minimum change\" (1 to 10) sets how much the value must change before it gets logged again." },
            { TextId.HelpNetworkTitle, "DataServer and RemoteSensors" },
            { TextId.HelpNetworkText, "Set up DataServer with WatchTower running on the PC that has FanControl and the DataServer plugin: choose a port " +
                "(or leave it empty for 8765) and a password. Set up RemoteSensors on the other PC with that PC's name or IP and the same port and " +
                "password. The default password is public, so change it. Remote sensors are for monitoring only: never use them in curves or " +
                "controls. After saving, restart the FanControl interface so the plugins read the changes." },
            { TextId.HelpRecorderTitle, "Recording a remote log" },
            { TextId.HelpRecorderText, "In the RemoteSensorLogger tab, press REC to start recording the remote PC and STOP to finish. " +
                "The status line shows the file being written and any problem, such as a wrong password or no new readings " +
                "(the FanControl interface must be open on the remote PC)." },
            { TextId.HelpSaveTitle, "Saving your changes" },
            { TextId.HelpSaveText, "Changes are kept in memory as you switch between profiles and tabs, but they are only written " +
                "to disk when you choose File > Save, which saves all the tabs. The plugins read their files the next time they start." },
            { TextId.RemoteSensorsWarning, "FOR MONITORING ONLY. NEVER use REMOTE sensors in curves or controls of this computer. " +
                "If the remote data stops, is wrong or the other computer is switched off, this computer could lose the cooling it needs " +
                "and its hardware could be permanently damaged." },
            { TextId.RemotePcName, "Remote PC name:" },
            { TextId.RemotePcNameHelp, "Type 'hostname' in a console on the remote PC." },
            { TextId.RemoteIp, "Remote PC IP:" },
            { TextId.RemoteIpHelp, "Type 'ipconfig' in a console on the remote PC." },
            { TextId.RemoteSensorsNote, "Fill in at least one of these two fields: remote PC name or remote PC IP. " +
                "If you fill in both, the IP is used. The port and the password must match the ones set in DataServer " +
                "(configured by running WatchTower on the PC where DataServer is installed). Leave the port empty to use the default (8765)." },
            { TextId.DataServerPort, "Port:" },
            { TextId.DataServerPortHelp, "Between 1 and 65535. Leave empty to use the default (8765)." },
            { TextId.DataServerPassword, "Password:" },
            { TextId.DataServerPasswordTooltip, "The current password is \"{0}\". You can change it by typing a new one in this field." },
            { TextId.DataServerNote, "This tab must be used with WatchTower running on the same PC where DataServer is installed. " +
                "Anyone who knows the password can connect and read the data. " +
                "Restart the FanControl interface after saving for the plugin to read the changes." },
            { TextId.DataServerPluginMissing, "The DataServer plugin (FanControl.DataServer.dll) was not found in FanControl's Plugins folder. " +
                "You can set the options here, but they will have no effect until the plugin is installed." },
            { TextId.ErrorTitle, "Error" },
            { TextId.SelectFanControlTitle, "Select the FanControl executable (or a shortcut to it)" },
            { TextId.FanControlFileFilter, "FanControl.exe or shortcut" },
            { TextId.InvalidPathTitle, "Invalid path" },
            { TextId.InvalidPathMessage, "The Configurations folder with the CACHE file was not found next to that executable. " +
                "Make sure you have chosen FanControl.exe." },
            { TextId.UnsavedChangesMessage, "There are unsaved changes. Do you want to save them before exiting?" },
            { TextId.CacheReadError, "Could not read FanControl's CACHE:\r\n{0}" },
            { TextId.CacheNotFound, "The CACHE file does not exist at {0}" },
            { TextId.ProfileBadFormat, "The file does not have the format of a FanControl profile." },
            { TextId.LoggerConfigBadFormat, "The configuration file does not have the expected format." },
            { TextId.ConfigFileBadFormat, "The file {0} does not have the expected format (the {1} element is missing). It has not been modified." },
            { TextId.RemoteIpIncomplete, "The remote PC's IP is incomplete: type all four numbers or leave it empty." },
            { TextId.DataServerPortInvalid, "The port must be a number between {0} and {1}, or be left empty." },
            { TextId.DataServerPasswordInvalid, "The password can only contain letters, numbers and keyboard symbols, with no spaces or accented characters." },
            { TextId.RemotePort, "Remote PC port:" },
            { TextId.RemotePortHelp, "Port DataServer listens on at the remote PC.\r\nLeave empty to use the default (8765)." },
            { TextId.EndOfLog, "END OF LOG" },
            { TextId.ActiveProfile, "ACTIVE PROFILE" },
            { TextId.ProfileNameLabel, "Name:" },
            { TextId.LogRemotePc, "Remote PC:" },
            { TextId.ControlsHeading, "CONTROLS:" },
            { TextId.SensorsHeading, "SENSORS:" },
            { TextId.ControlType, "CONTROL" },
            { TextId.SensorType, "SENSOR" },
            { TextId.RecordNameLabel, "Name" },
            { TextId.RecordValueLabel, "Value" },
            { TextId.RecStart, "REC" },
            { TextId.RecStop, "STOP" },
            { TextId.RefreshRemoteProfile, "Get the profile from the remote PC" },
            { TextId.LoggerNeedsAddress, "Fill in the name or the IP of the remote PC." },
            { TextId.LoggerNeedsPassword, "Type the password set in DataServer on the remote PC." },
            { TextId.LoggerStatusIdle, "Not recording." },
            { TextId.LoggerStatusConnecting, "Connecting to the remote PC..." },
            { TextId.LoggerStatusRecording, "Recording to {0}" },
            { TextId.LoggerStatusStale, "No new readings. Is the FanControl interface open on the remote PC?" },
            { TextId.LoggerStatusUnreachable, "Cannot reach the remote PC. Retrying..." },
            { TextId.LoggerStatusUnreachableShort, "Cannot reach the remote PC." },
            { TextId.LoggerStatusUnauthorized, "Wrong password. Recording stopped." },
            { TextId.LoggerStatusUnauthorizedShort, "Wrong password." },
            { TextId.LoggerStatusFailed, "Error: {0}" },
            { TextId.LoggerStatusProfileLoaded, "Profile loaded." },
            { TextId.LoggerStatusNoProfile, "Could not get the profile. If FanControl has just started, wait a few seconds and try again." },
            { TextId.LoggerStatusBusy, "DataServer is busy. Try again in a moment." }
        };
    }

    public class Spanish : Language
    {
        public override string Name => "Español";

        public override Dictionary<TextId, string> Texts { get; } = new Dictionary<TextId, string>
        {
            { TextId.File, "Archivo" },
            { TextId.Save, "Guardar" },
            { TextId.Exit, "Salir" },
            { TextId.Settings, "Configuración" },
            { TextId.Language, "Idioma" },
            { TextId.FanControlPath, "Especificar ruta de FanControl" },
            { TextId.Help, "Ayuda" },
            { TextId.ViewHelp, "Ver la ayuda" },
            { TextId.About, "Acerca de..." },
            { TextId.Profile, "Perfil:" },
            { TextId.RefreshProfiles, "Recargar perfiles" },
            { TextId.Controls, "Controles" },
            { TextId.Curves, "Curvas" },
            { TextId.Sensors, "Sensores" },
            { TextId.Interval, "Intervalo (seg)" },
            { TextId.ChangeThreshold, "Umbral de cambio" },
            { TextId.CurvesNotRecorded, "Las curvas aún no se registran en el log." },
            { TextId.Log, "Registro" },
            { TextId.AboutWindowTitle, "Acerca de WatchTower" },
            { TextId.AboutDescription, "WatchTower configura una familia de plugins de monitorización para FanControl: " +
                "CurveReader ( registra sensores y controles en un archivo ), " +
                "DataServer ( los comparte por la red ) y RemoteSensors ( muestra los sensores de otro ordenador dentro de FanControl ). " +
                "También puede grabar en un log local los datos de un FanControl remoto.\r\n\r\n" +
                "Este programa es portable: se puede ejecutar desde cualquier carpeta del disco sin necesidad de instalarlo." },
            { TextId.ViewOnGitHub, "Ver proyecto en GitHub" },
            { TextId.AboutJsonNotice, "Incluye Newtonsoft.Json (Copyright \u00A9 James Newton-King), bajo licencia MIT." },
            { TextId.Version, "Versión" },
            { TextId.Ok, "Aceptar" },
            { TextId.HelpWindowTitle, "Ayuda de WatchTower" },
            { TextId.HelpRequirementsTitle, "Requisitos" },
            { TextId.HelpRequirementsText, "Windows con .NET Framework 4.8 instalado. Las pestañas CurveReader, DataServer y RemoteSensors configuran " +
                "plugins, así que en este PC deben estar instalados FanControl y el plugin correspondiente. La pestaña RemoteSensorLogger no necesita " +
                "FanControl en este PC: solo acceso por red a un PC que ejecute DataServer." },
            { TextId.HelpGettingStartedTitle, "Primeros pasos" },
            { TextId.HelpGettingStartedText, "La primera vez que ejecutes la app, ve a Configuración > Especificar ruta de " +
                "FanControl y selecciona FanControl.exe (o un acceso directo). A partir de ahí la app localizará tus perfiles automáticamente." },
            { TextId.HelpTabsTitle, "Las pestañas" },
            { TextId.HelpTabsText, "Hay una pestaña por plugin: CurveReader (registra los sensores y controles de este PC), DataServer (comparte " +
                "los datos de este PC por la red), RemoteSensors (muestra los sensores de otro PC dentro de este FanControl) y RemoteSensorLogger " +
                "(graba en un log los datos del DataServer de otro PC)." },
            { TextId.HelpSelectProfileTitle, "Elegir un perfil" },
            { TextId.HelpSelectProfileText, "En la pestaña CurveReader, elige el perfil que quieras configurar en el desplegable Perfil. " +
                "En la pestaña RemoteSensorLogger, rellena los datos de conexión y usa el botón para obtener el perfil del PC remoto. " +
                "En ambos casos sus controles y sensores aparecerán debajo, listos para editar." },
            { TextId.HelpConfigureItemsTitle, "Configurar sensores y controles" },
            { TextId.HelpConfigureItemsText, "En CurveReader y RemoteSensorLogger, para cada elemento, la casilla Registro decide si se registra o no. " +
                "\"Intervalo (seg)\" es el tiempo mínimo entre dos registros de ese elemento. " +
                "\"Umbral de cambio\" (1 a 10) indica cuánto debe cambiar el valor antes de volver a registrarse." },
            { TextId.HelpNetworkTitle, "DataServer y RemoteSensors" },
            { TextId.HelpNetworkText, "Configura DataServer con WatchTower ejecutándose en el PC que tiene FanControl y el plugin DataServer: elige un " +
                "puerto (o déjalo vacío para usar el 8765) y una contraseña. Configura RemoteSensors en el otro PC con el nombre o la IP de ese PC y " +
                "el mismo puerto y contraseña. La contraseña predeterminada es pública, así que cámbiala. Los sensores remotos son solo para " +
                "monitorizar: nunca los uses en curvas ni controles. Tras guardar, reinicia la interfaz de FanControl para que los plugins lean los cambios." },
            { TextId.HelpRecorderTitle, "Grabar un log remoto" },
            { TextId.HelpRecorderText, "En la pestaña RemoteSensorLogger, pulsa GRABAR para empezar a grabar el PC remoto y PARAR para terminar. " +
                "La línea de estado muestra el archivo que se está escribiendo y cualquier problema, como una contraseña incorrecta o la falta " +
                "de lecturas nuevas (la interfaz de FanControl debe estar abierta en el PC remoto)." },
            { TextId.HelpSaveTitle, "Guardar los cambios" },
            { TextId.HelpSaveText, "Los cambios se conservan en memoria al cambiar de perfil y de pestaña, " +
                "pero solo se escriben en disco al elegir Archivo > Guardar, que guarda todas las pestañas. Los plugins leen sus archivos la próxima vez que se inicien." },
            { TextId.RemoteSensorsWarning, "SOLO PARA MONITORIZAR. NUNCA uses los sensores REMOTE en curvas o controles de este ordenador. " +
                "Si los datos remotos se detienen, son erróneos o el otro ordenador está apagado, este ordenador podría quedarse sin la " +
                "refrigeración que necesita y su hardware podría dañarse de forma permanente." },
            { TextId.RemotePcName, "Nombre del PC remoto:" },
            { TextId.RemotePcNameHelp, "Escribe 'hostname' en una consola del PC remoto." },
            { TextId.RemoteIp, "IP del PC remoto:" },
            { TextId.RemoteIpHelp, "Escribe 'ipconfig' en una consola del PC remoto." },
            { TextId.RemoteSensorsNote, "Rellena al menos uno de estos dos campos: nombre del PC remoto o IP del PC remoto. " +
                "Si rellenas los dos, se usa la IP. El puerto y la contraseña deben coincidir con los de DataServer " +
                "(se configuran ejecutando WatchTower en el PC donde esté instalado DataServer). " +
                "Deja el puerto vacío para usar el predeterminado (8765)." },
            { TextId.DataServerPort, "Puerto:" },
            { TextId.DataServerPortHelp, "Entre 1 y 65535. Déjalo vacío para usar el predeterminado (8765)." },
            { TextId.DataServerPassword, "Contraseña:" },
            { TextId.DataServerPasswordTooltip, "La contraseña actual es \"{0}\". Puedes cambiarla escribiendo otra en este campo." },
            { TextId.DataServerNote, "Esta pestaña debe usarse con WatchTower ejecutándose en el mismo PC donde esté instalado DataServer. " +
                "Quien conozca la contraseña podrá conectar y leer los datos. " +
                "Reinicia la interfaz de FanControl después de guardar para que el plugin lea los cambios." },
            { TextId.DataServerPluginMissing, "No se ha encontrado el plugin DataServer (FanControl.DataServer.dll) " +
                "en la carpeta Plugins de FanControl. " +
                "Puedes configurar las opciones aquí, pero no tendrán efecto hasta que el plugin esté instalado." },
            { TextId.ErrorTitle, "Error" },
            { TextId.SelectFanControlTitle, "Selecciona el ejecutable de FanControl (o un acceso directo)" },
            { TextId.FanControlFileFilter, "FanControl.exe o acceso directo" },
            { TextId.InvalidPathTitle, "Ruta no válida" },
            { TextId.InvalidPathMessage, "No se ha encontrado la carpeta Configurations con el archivo CACHE junto a ese ejecutable. " +
                "Comprueba que has elegido FanControl.exe." },
            { TextId.UnsavedChangesMessage, "Hay cambios sin guardar. ¿Quieres guardarlos antes de salir?" },
            { TextId.CacheReadError, "No se ha podido leer el CACHE de FanControl:\r\n{0}" },
            { TextId.CacheNotFound, "No existe el archivo CACHE en {0}" },
            { TextId.ProfileBadFormat, "El archivo no tiene el formato de un perfil de FanControl." },
            { TextId.LoggerConfigBadFormat, "El archivo de configuración no tiene el formato esperado." },
            { TextId.ConfigFileBadFormat, "El archivo {0} no tiene el formato esperado (falta el elemento {1}). No se ha modificado." },
            { TextId.RemoteIpIncomplete, "La IP del PC remoto está incompleta: escribe los cuatro números o déjala vacía." },
            { TextId.DataServerPortInvalid, "El puerto debe ser un número entre {0} y {1}, o dejarse vacío." },
            { TextId.DataServerPasswordInvalid, "La contraseña solo puede contener letras, números y símbolos del teclado, " +
                "sin espacios ni acentos." },
            { TextId.RemotePort, "Puerto del PC remoto:" },
            { TextId.RemotePortHelp, "Puerto en el que escucha DataServer en el PC remoto.\r\nDéjalo vacío para usar el predeterminado (8765)." },
            { TextId.EndOfLog, "FIN DEL LOG" },
            { TextId.ActiveProfile, "PERFIL ACTIVO" },
            { TextId.ProfileNameLabel, "Nombre:" },
            { TextId.LogRemotePc, "PC remoto:" },
            { TextId.ControlsHeading, "CONTROLES:" },
            { TextId.SensorsHeading, "SENSORES:" },
            { TextId.ControlType, "CONTROL" },
            { TextId.SensorType, "SENSOR" },
            { TextId.RecordNameLabel, "Nombre" },
            { TextId.RecordValueLabel, "Valor" },
            { TextId.RecStart, "GRABAR" },
            { TextId.RecStop, "PARAR" },
            { TextId.RefreshRemoteProfile, "Obtener el perfil del PC remoto" },
            { TextId.LoggerNeedsAddress, "Rellena el nombre o la IP del PC remoto." },
            { TextId.LoggerNeedsPassword, "Escribe la contraseña que se puso en DataServer en el PC remoto." },
            { TextId.LoggerStatusIdle, "Sin grabar." },
            { TextId.LoggerStatusConnecting, "Conectando con el PC remoto..." },
            { TextId.LoggerStatusRecording, "Grabando en {0}" },
            { TextId.LoggerStatusStale, "Sin lecturas nuevas. ¿Está abierta la interfaz de FanControl en el PC remoto?" },
            { TextId.LoggerStatusUnreachable, "No se puede conectar con el PC remoto. Reintentando..." },
            { TextId.LoggerStatusUnreachableShort, "No se puede conectar con el PC remoto." },
            { TextId.LoggerStatusUnauthorized, "Contraseña incorrecta. Grabación detenida." },
            { TextId.LoggerStatusUnauthorizedShort, "Contraseña incorrecta." },
            { TextId.LoggerStatusFailed, "Error: {0}" },
            { TextId.LoggerStatusProfileLoaded, "Perfil cargado." },
            { TextId.LoggerStatusNoProfile, "No se pudo obtener el perfil. Si FanControl acaba de arrancar, espera unos segundos y vuelve a probar." },
            { TextId.LoggerStatusBusy, "DataServer está ocupado. Vuelve a probar en un momento." }
        };
    }
}