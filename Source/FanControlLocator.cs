using System.IO;

namespace WatchTower
{
    /// <summary>
    /// Sabe dónde está el archivo CACHE dentro de una instalación de FanControl
    /// y comprueba que la carpeta que elige el usuario es realmente la de FanControl.
    /// </summary>
    public static class FanControlLocator
    {
        /// <summary>
        /// Ruta donde debe estar el CACHE: carpeta de FanControl \ Configurations \ CACHE.
        /// No comprueba que el archivo exista.
        /// </summary>
        public static string GetCachePath(string fanControlFolder)
        {
            return Path.Combine(fanControlFolder, "Configurations", "CACHE");
        }

        /// <summary>
        /// Recibe el archivo que elige el usuario (FanControl.exe, o un acceso directo que
        /// Windows ya habrá convertido en la ruta del .exe) y devuelve su carpeta, siempre
        /// que junto a él exista Configurations\CACHE. Devuelve false si no es FanControl.
        /// </summary>
        public static bool TryGetFanControlFolder(string selectedFile, out string fanControlFolder)
        {
            fanControlFolder = null;
            if (string.IsNullOrWhiteSpace(selectedFile) || !File.Exists(selectedFile)) return false;

            string folder = Path.GetDirectoryName(selectedFile);
            if (!File.Exists(GetCachePath(folder))) return false;

            fanControlFolder = folder;
            return true;
        }
    }
}