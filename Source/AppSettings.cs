using System;
using System.IO;
using System.Xml.Linq;

namespace WatchTower
{
    /// <summary>
    /// Ajustes propios de la app que se recuerdan entre ejecuciones (la carpeta de FanControl y
    /// el idioma). Se guardan en un pequeño XML dentro de %LocalAppData%\FanControl.
    /// No los usa el plugin.
    /// </summary>
    public static class AppSettings
    {
        private const string SettingsFolderName = "FanControl";
        private const string SettingsFileName = "WatchTowerSettings.xml";
        private const string RootElementName = "Settings";
        private const string FanControlFolderElementName = "FanControlFolder";
        private const string LanguageElementName = "Language";

        // Ruta completa del archivo de ajustes. Se calcula con la carpeta AppData del usuario
        // actual, así que funciona con cualquier nombre de usuario.
        private static string SettingsFilePath
        {
            get
            {
                string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                return Path.Combine(appDataFolder, SettingsFolderName, SettingsFileName);
            }
        }

        /// <summary>Carpeta de FanControl guardada, o null si no hay ninguna.</summary>
        public static string LoadFanControlFolder()
        {
            return ReadValue(FanControlFolderElementName);
        }

        /// <summary>
        /// Guarda la carpeta de FanControl. Si no se puede escribir, la app sigue funcionando
        /// igual; solo tendrá que pedir la ruta de nuevo la próxima vez.
        /// </summary>
        public static void SaveFanControlFolder(string fanControlFolder)
        {
            WriteValue(FanControlFolderElementName, fanControlFolder);
        }

        /// <summary>
        /// Nombre del idioma guardado. Si no hay ninguno, devuelve el que se pasa como
        /// 'defaultLanguageName'. Si el idioma guardado ya no existe, Translator.SetLanguage lo ignora.
        /// </summary>
        public static string LoadLanguage(string defaultLanguageName)
        {
            string languageName = ReadValue(LanguageElementName);
            return string.IsNullOrEmpty(languageName) ? defaultLanguageName : languageName;
        }
        public static void SaveLanguage(string languageName)
        {
            WriteValue(LanguageElementName, languageName);
        }

        /// <summary>
        /// Lee el valor de un elemento del archivo. Devuelve null si el archivo o el elemento no
        /// existen o no se pueden leer: un archivo dañado no debe impedir que la app arranque.
        /// </summary>
        private static string ReadValue(string elementName)
        {
            try
            {
                if (!File.Exists(SettingsFilePath)) return null;

                XDocument document = XDocument.Load(SettingsFilePath);
                return (string)document.Root?.Element(elementName);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Escribe el valor de un elemento conservando el resto del archivo. Si no se puede
        /// escribir, la app sigue funcionando igual; solo perderá ese ajuste.
        /// </summary>
        private static void WriteValue(string elementName, string value)
        {
            try
            {
                XDocument document = LoadOrCreateDocument();

                XElement element = document.Root.Element(elementName);
                if (element == null)
                    document.Root.Add(new XElement(elementName, value));
                else
                    element.Value = value;

                Directory.CreateDirectory(Path.GetDirectoryName(SettingsFilePath));
                document.Save(SettingsFilePath);
            }
            catch (Exception)
            {
                // Sin permisos o disco no disponible: no es un error que deba molestar al usuario.
            }
        }

        // Carga el archivo existente, o crea un documento vacío si no existe o está dañado.
        private static XDocument LoadOrCreateDocument()
        {
            try
            {
                if (File.Exists(SettingsFilePath))
                {
                    XDocument document = XDocument.Load(SettingsFilePath);
                    if (document.Root != null) return document;
                }
            }
            catch (Exception)
            {
                // Archivo dañado: se empieza con uno nuevo.
            }

            return new XDocument(new XElement(RootElementName));
        }
    }
}