using System;
using System.Collections.Generic;
using System.Linq;

namespace WatchTower
{
    /// <summary>
    /// Un identificador por cada texto de la interfaz. Solo contiene el nombre del texto;
    /// el texto en sí está en los diccionarios de cada idioma.
    /// </summary>
    public enum TextId
    {
        File, Save, Exit,
        Settings, Language, FanControlPath,
        Help, ViewHelp, About,
        Profile, RefreshProfiles,
        Controls, Curves, Sensors,
        Log, Interval, ChangeThreshold,
        CurvesNotRecorded,
        AboutWindowTitle, AboutDescription, ViewOnGitHub, AboutJsonNotice, Version, Ok,
        HelpWindowTitle, HelpRequirementsTitle, HelpRequirementsText, HelpGettingStartedTitle, HelpGettingStartedText,
        HelpSelectProfileTitle, HelpSelectProfileText, HelpConfigureItemsTitle, HelpConfigureItemsText, HelpSaveTitle, HelpSaveText,
        HelpTabsTitle, HelpTabsText, HelpNetworkTitle, HelpNetworkText, HelpRecorderTitle, HelpRecorderText,
        RemoteSensorsWarning, RemotePcName, RemotePcNameHelp, RemoteIp, RemoteIpHelp, RemoteSensorsNote,
        DataServerPort, DataServerPortHelp, DataServerPassword, DataServerPasswordTooltip, DataServerNote, DataServerPluginMissing,
        ErrorTitle, SelectFanControlTitle, FanControlFileFilter, InvalidPathTitle, InvalidPathMessage, UnsavedChangesMessage,
        CacheReadError, CacheNotFound, ProfileBadFormat, LoggerConfigBadFormat, ConfigFileBadFormat,
        RemoteIpIncomplete, DataServerPortInvalid, DataServerPasswordInvalid, RemotePort, RemotePortHelp,
        EndOfLog, ActiveProfile, ProfileNameLabel, LogRemotePc, ControlsHeading, SensorsHeading,
        ControlType, SensorType, RecordNameLabel, RecordValueLabel,
        RecStart, RecStop, RefreshRemoteProfile, LoggerNeedsAddress, LoggerNeedsPassword,
        LoggerStatusIdle, LoggerStatusConnecting, LoggerStatusRecording, LoggerStatusStale,
        LoggerStatusUnreachable, LoggerStatusUnreachableShort, LoggerStatusUnauthorized,
        LoggerStatusUnauthorizedShort, LoggerStatusFailed, LoggerStatusProfileLoaded,
        LoggerStatusNoProfile, LoggerStatusBusy
    }

    /// <summary>
    /// Devuelve los textos de la interfaz en el idioma actual. Los idiomas disponibles son las
    /// clases que heredan de Language (ver Language.cs); aquí no hay que tocar nada para añadir uno.
    /// </summary>
    public static class Translator
    {
        // Idioma de arranque y de reserva: si a otro idioma le falta un texto, se usa el de este.
        // Debe existir una clase de idioma cuyo Name sea este.
        private const string DefaultLanguageName = "English";

        // Todos los idiomas disponibles, descubiertos al arrancar. El orden de estas tres líneas
        // importa: cada una usa la anterior.
        private static readonly List<Language> _languages = DiscoverLanguages();
        private static readonly Language _defaultLanguage = FindLanguage(DefaultLanguageName);
        private static Language _currentLanguage = _defaultLanguage;

        /// <summary>Nombre del idioma actual (el mismo con el que aparece en el menú).</summary>
        public static string CurrentLanguageName => _currentLanguage?.Name ?? DefaultLanguageName;

        /// <summary>Nombres de los idiomas disponibles, ordenados alfabéticamente.</summary>
        public static IEnumerable<string> AvailableLanguageNames => _languages.Select(language => language.Name);

        /// <summary>
        /// Cambia el idioma actual. Si no existe ningún idioma con ese nombre (por ejemplo, un ajuste
        /// guardado de un idioma que ya no está), no hace nada y se mantiene el actual.
        /// </summary>
        public static void SetLanguage(string languageName)
        {
            Language language = FindLanguage(languageName);
            if (language != null) _currentLanguage = language;
        }

        /// <summary>
        /// Devuelve el texto en el idioma actual. Si a ese idioma le falta el texto, devuelve el
        /// inglés, y si tampoco está, el nombre del identificador, para que el fallo se vea.
        /// </summary>
        public static string Translate(TextId id)
        {
            string text;
            if (_currentLanguage != null && _currentLanguage.Texts.TryGetValue(id, out text)) return text;
            if (_defaultLanguage != null && _defaultLanguage.Texts.TryGetValue(id, out text)) return text;
            return id.ToString();
        }

        // Busca por reflexión todas las clases que heredan de Language, crea una instancia de cada
        // una y las ordena por nombre. Así añadir un idioma es solo añadir su clase.
        private static List<Language> DiscoverLanguages()
        {
            return typeof(Language).Assembly.GetTypes()
                .Where(type => type.IsSubclassOf(typeof(Language)) && !type.IsAbstract)
                .Select(type => (Language)Activator.CreateInstance(type))
                .OrderBy(language => language.Name, StringComparer.CurrentCultureIgnoreCase)
                .ToList();
        }

        // Devuelve el idioma con ese nombre, o null si no existe.
        private static Language FindLanguage(string languageName)
        {
            return _languages.FirstOrDefault(language =>
                string.Equals(language.Name, languageName, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Devuelve el texto en todos los idiomas disponibles (sin repetidos). Sirve para reconocer
        /// en un archivo un texto que pudo escribirse en un idioma distinto del actual.
        /// </summary>
        public static IEnumerable<string> GetTextInAllLanguages(TextId id)
        {
            List<string> texts = new List<string>();

            foreach (Language language in _languages)
            {
                string text;
                if (language.Texts.TryGetValue(id, out text) && !texts.Contains(text))
                    texts.Add(text);
            }

            return texts;
        }
    }
}