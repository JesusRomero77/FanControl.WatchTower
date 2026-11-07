using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;

namespace WatchTower
{
    /// <summary>
    /// Un control o un sensor de un perfil de FanControl que el logger puede registrar.
    /// </summary>
    public class LoggableItem
    {
        /// <summary>Nombre que ve el usuario (el NickName del perfil); si no tiene, el identificador.</summary>
        public string Name { get; set; }

        /// <summary>Identificador estable con el que el plugin empareja el elemento.</summary>
        public string Identifier { get; set; }
    }

    /// <summary>
    /// Controles y sensores que contiene un perfil.
    /// </summary>
    public class ProfileContent
    {
        public List<LoggableItem> Controls { get; } = new List<LoggableItem>();
        public List<LoggableItem> Sensors { get; } = new List<LoggableItem>();
    }

    /// <summary>
    /// Lee un archivo de perfil de FanControl y extrae sus controles y sensores, aplicando los
    /// mismos filtros que el plugin para que la lista coincida con lo que el logger puede registrar.
    /// </summary>
    public static class ProfileReader
    {
        private const string RootKey = "FanControl";
        private const string ControlsKey = "Controls";
        private const string SensorsKey = "CustomSensors";
        private const string NameKey = "NickName";
        private const string IdentifierKey = "Identifier";
        private const string HiddenKey = "IsHidden";
        private const string EnabledKey = "Enable";

        /// <summary>
        /// Devuelve el contenido del perfil, o null (con el motivo en 'error') si el archivo
        /// no se puede leer o no tiene el formato de un perfil de FanControl.
        /// </summary>
        public static ProfileContent Read(string profileFilePath, out string error)
        {
            error = null;
            try
            {
                JObject profile = JObject.Parse(File.ReadAllText(profileFilePath));

                JObject fanControl = profile[RootKey] as JObject;
                if (fanControl == null)
                {
                    error = Translator.Translate(TextId.ProfileBadFormat);
                    return null;
                }

                // Cada sección se lee por separado: si falta una, no impide leer las demás.
                ProfileContent content = new ProfileContent();
                ReadItems(fanControl[ControlsKey] as JArray, content.Controls, true);
                ReadItems(fanControl[SensorsKey] as JArray, content.Sensors, false);
                return content;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return null;
            }
        }

        // Recorre una lista del perfil (controles o sensores) y añade a 'items' los elementos que
        // el plugin también tendría en cuenta: sin ocultos, sin deshabilitados (solo en los
        // controles) y con identificador.
        private static void ReadItems(JArray tokens, List<LoggableItem> items, bool checkEnabled)
        {
            if (tokens == null) return;

            foreach (JToken token in tokens)
            {
                JObject element = token as JObject;
                if (element == null) continue;

                if (element[HiddenKey]?.Value<bool?>() == true) continue;
                if (checkEnabled && element[EnabledKey]?.Value<bool?>() == false) continue;

                string identifier = element[IdentifierKey]?.ToString();
                if (string.IsNullOrWhiteSpace(identifier)) continue;

                string name = element[NameKey]?.ToString();
                items.Add(new LoggableItem
                {
                    Name = string.IsNullOrWhiteSpace(name) ? identifier : name,
                    Identifier = identifier
                });
            }
        }
    }
}