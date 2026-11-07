using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace WatchTower
{
    /// <summary>
    /// Lee y escribe la configuración del logger en el archivo XML que usará el plugin,
    /// dentro de %LocalAppData%\FanControl.
    /// </summary>
    public static class LoggerConfigFile
    {
        private const string LanguageAttributeName = "Language";
        private const string FolderName = "FanControl";
        private const string FileName = "CurveReaderLoggerConfig.xml";
        private const string IntervalSecondsAttributeName = "IntervalSeconds";

        // Nombres de los elementos y atributos del XML, iguales a los que usa el plugin.
        private const string RootElementName = "LoggerConfig";
        private const string ProfileElementName = "Profile";
        private const string ControlElementName = "Control";
        private const string SensorElementName = "Sensor";
        private const string VersionAttributeName = "Version";
        private const string FileNameAttributeName = "FileName";
        private const string IdentifierAttributeName = "Identifier";
        private const string NameAttributeName = "Name";
        private const string IncludedInLogAttributeName = "IncludedInLog";
        private const string ThresholdAttributeName = "Threshold";
        private const string RemoteFileName = "RemoteSensorLoggerConfig.xml";
        private const string ConnectionElementName = "Connection";
        private const string PcNameAttributeName = "PcName";
        private const string IpAttributeName = "Ip";
        private const string PortAttributeName = "Port";
        private const string PasswordAttributeName = "Password";

        // Ruta del archivo de CurveReader y del archivo del grabador remoto, en la carpeta AppData del usuario actual.
        public static string FilePath => GetPath(FileName);
        public static string RemoteFilePath => GetPath(RemoteFileName);

        private static string GetPath(string fileName)
        {
            string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return Path.Combine(appDataFolder, FolderName, fileName);
        }

        /// <summary>Lee la configuración de CurveReader (ver la versión con ruta).</summary>
        public static LoggerConfig Load(out string error)
        {
            return Load(FilePath, out error);
        }

        /// <summary>
        /// Lee la configuración del archivo indicado. Si no existe devuelve una configuración vacía (sin
        /// perfiles). Devuelve null, con el motivo en 'error', si existe pero no se puede leer.
        /// </summary>
        public static LoggerConfig Load(string filePath, out string error)
        {
            error = null;
            LoggerConfig config = new LoggerConfig();
            if (!File.Exists(filePath)) return config;

            try
            {
                XDocument document = XDocument.Load(filePath);
                if (document.Root == null || document.Root.Name != RootElementName)
                {
                    error = Translator.Translate(TextId.LoggerConfigBadFormat);
                    return null;
                }
                config.LanguageName = (string)document.Root.Attribute(LanguageAttributeName);

                XElement connectionElement = document.Root.Element(ConnectionElementName);
                if (connectionElement != null)
                {
                    config.RemoteConnection = new RemoteConnectionConfig
                    {
                        PcName = ((string)connectionElement.Attribute(PcNameAttributeName) ?? "").Trim(),
                        Ip = ((string)connectionElement.Attribute(IpAttributeName) ?? "").Trim(),
                        Port = ((string)connectionElement.Attribute(PortAttributeName) ?? "").Trim(),
                        Password = ((string)connectionElement.Attribute(PasswordAttributeName) ?? "").Trim()
                    };
                }

                foreach (XElement profileElement in document.Root.Elements(ProfileElementName))
                {
                    string fileName = (string)profileElement.Attribute(FileNameAttributeName);
                    if (string.IsNullOrWhiteSpace(fileName)) continue;

                    ProfileConfig profile = new ProfileConfig { FileName = fileName };
                    ReadItems(profileElement, ControlElementName, profile.Controls);
                    ReadItems(profileElement, SensorElementName, profile.Sensors);
                    config.Profiles.Add(profile);
                }

                return config;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return null;
            }
        }

        /// <summary>Guarda la configuración de CurveReader (ver la versión con ruta).</summary>
        public static bool Save(LoggerConfig config, out string error)
        {
            return Save(FilePath, config, out error);
        }

        /// <summary>
        /// Guarda la configuración en el archivo indicado. Escribe primero en un archivo temporal y después
        /// sustituye al definitivo, para que el archivo nunca quede a medias. Devuelve false, con el
        /// motivo en 'error', si no se ha podido guardar.
        /// </summary>
        public static bool Save(string filePath, LoggerConfig config, out string error)
        {
            error = null;
            try
            {
                XElement root = new XElement(RootElementName,
                    new XAttribute(VersionAttributeName, LoggerConfig.CurrentVersion));

                if (!string.IsNullOrWhiteSpace(config.LanguageName))
                    root.Add(new XAttribute(LanguageAttributeName, config.LanguageName));

                if (config.RemoteConnection != null)
                {
                    root.Add(new XElement(ConnectionElementName,
                        new XAttribute(PcNameAttributeName, config.RemoteConnection.PcName ?? string.Empty),
                        new XAttribute(IpAttributeName, config.RemoteConnection.Ip ?? string.Empty),
                        new XAttribute(PortAttributeName, config.RemoteConnection.Port ?? string.Empty),
                        new XAttribute(PasswordAttributeName, config.RemoteConnection.Password ?? string.Empty)));
                }

                foreach (ProfileConfig profile in config.Profiles)
                {
                    XElement profileElement = new XElement(ProfileElementName,
                        new XAttribute(FileNameAttributeName, profile.FileName));
                    AddItems(profileElement, ControlElementName, profile.Controls);
                    AddItems(profileElement, SensorElementName, profile.Sensors);
                    root.Add(profileElement);
                }

                Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                string tempPath = filePath + ".tmp";
                new XDocument(root).Save(tempPath);

                if (File.Exists(filePath)) File.Replace(tempPath, filePath, null);
                else File.Move(tempPath, filePath);

                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        // Lee los controles o los sensores de un perfil. Los valores que falten o no sean
        // válidos se sustituyen por los valores por defecto.
        private static void ReadItems(XElement profileElement, string elementName, List<ItemConfig> items)
        {
            foreach (XElement element in profileElement.Elements(elementName))
            {
                string identifier = (string)element.Attribute(IdentifierAttributeName);
                if (string.IsNullOrWhiteSpace(identifier)) continue;

                items.Add(new ItemConfig
                {
                    Identifier = identifier,
                    Name = (string)element.Attribute(NameAttributeName),
                    IncludedInLog = ParseIncludedInLog((string)element.Attribute(IncludedInLogAttributeName)),
                    IntervalSeconds = ParseIntervalSeconds((string)element.Attribute(IntervalSecondsAttributeName)),
                    Threshold = ParseThreshold((string)element.Attribute(ThresholdAttributeName))
                });
            }
        }

        // Escribe los controles o los sensores de un perfil como elementos del XML.
        private static void AddItems(XElement profileElement, string elementName, List<ItemConfig> items)
        {
            foreach (ItemConfig item in items)
            {
                profileElement.Add(new XElement(elementName,
                    new XAttribute(IdentifierAttributeName, item.Identifier),
                    new XAttribute(NameAttributeName, item.Name ?? string.Empty),
                    new XAttribute(IncludedInLogAttributeName, item.IncludedInLog),
                    new XAttribute(IntervalSecondsAttributeName, item.IntervalSeconds),
                    new XAttribute(ThresholdAttributeName, item.Threshold)));
            }
        }

        private static bool ParseIncludedInLog(string text)
        {
            bool value;
            return bool.TryParse(text, out value) ? value : ItemConfig.DefaultIncludedInLog;
        }

        // El umbral debe ser un entero entre el mínimo y el máximo; si no, se usa el valor por defecto.
        private static int ParseThreshold(string text)
        {
            int value;
            if (!int.TryParse(text, out value)) return ItemConfig.DefaultThreshold;
            if (value < ItemConfig.MinThreshold || value > ItemConfig.MaxThreshold) return ItemConfig.DefaultThreshold;
            return value;
        }

        // El intervalo debe ser un entero entre el mínimo y el máximo; si no, se usa el valor por defecto.
        private static int ParseIntervalSeconds(string text)
        {
            int value;
            if (!int.TryParse(text, out value)) return ItemConfig.DefaultIntervalSeconds;
            if (value < ItemConfig.MinIntervalSeconds || value > ItemConfig.MaxIntervalSeconds) return ItemConfig.DefaultIntervalSeconds;
            return value;
        }
    }
}