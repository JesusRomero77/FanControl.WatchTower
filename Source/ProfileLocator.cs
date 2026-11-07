using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WatchTower
{
    /// <summary>
    /// Cuál es el perfil activo de FanControl y en qué carpeta están los perfiles,
    /// según el archivo CACHE.
    /// </summary>
    public class ProfileLocation
    {
        /// <summary>Archivo del perfil activo (por ejemplo "verano.json"); vacío si el CACHE no lo indica.</summary>
        public string ActiveProfileFileName { get; set; }

        /// <summary>Carpeta donde están los perfiles, según el CACHE.</summary>
        public string ProfilesFolder { get; set; }
    }

    /// <summary>
    /// Localiza los perfiles de FanControl: lee el CACHE para saber cuál es el perfil activo y en
    /// qué carpeta están los perfiles, y lista los archivos de perfil de esa carpeta.
    /// </summary>
    public static class ProfileLocator
    {
        // Nombres de los campos del CACHE que necesitamos y patrón de los archivos de perfil.
        private const string ActiveProfileKey = "CurrentConfigFileName";
        private const string ProfilesFolderKey = "CustomConfigFolder";
        private const string ProfileFilePattern = "*.json";

        /// <summary>
        /// Lee el CACHE de la instalación de FanControl indicada. La carpeta de perfiles sale del
        /// propio CACHE; solo si no la indica se usa Configurations, igual que hace el plugin.
        /// Devuelve null, con el motivo en 'error', si no se puede leer.
        /// </summary>
        public static ProfileLocation Locate(string fanControlFolder, out string error)
        {
            error = null;
            try
            {
                string cachePath = FanControlLocator.GetCachePath(fanControlFolder);
                if (!File.Exists(cachePath))
                {
                    error = string.Format(Translator.Translate(TextId.CacheNotFound), cachePath);
                    return null;
                }

                JObject cache = JObject.Parse(File.ReadAllText(cachePath));
                string customFolder = cache[ProfilesFolderKey]?.ToString();

                return new ProfileLocation
                {
                    ActiveProfileFileName = cache[ActiveProfileKey]?.ToString(),
                    ProfilesFolder = string.IsNullOrWhiteSpace(customFolder) ? Path.GetDirectoryName(cachePath): customFolder
                };
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return null;
            }
        }

        /// <summary>
        /// Devuelve las rutas de los perfiles (.json) de la carpeta, ordenadas por nombre.
        /// Si la carpeta no existe devuelve una lista vacía.
        /// </summary>
        public static List<string> GetProfileFiles(string profilesFolder)
        {
            if (!Directory.Exists(profilesFolder)) return new List<string>();

            return Directory.GetFiles(profilesFolder, ProfileFilePattern).OrderBy(path => Path.GetFileName(path),
                StringComparer.OrdinalIgnoreCase).ToList();
        }
    }
}