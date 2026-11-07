using System.IO;

namespace WatchTower
{
    /// <summary>
    /// Elemento del desplegable de perfiles. Muestra el nombre sin extensión (verano), pero
    /// conserva la ruta completa y el nombre con extensión (verano.json), que es el que se
    /// usará como identificador del perfil en el XML.
    /// </summary>
    public class ProfileItem
    {
        public string FilePath { get; }

        public string FileName
        {
            get { return Path.GetFileName(FilePath); }
        }

        public ProfileItem(string filePath)
        {
            FilePath = filePath;
        }

        // El desplegable muestra lo que devuelve este método.
        public override string ToString()
        {
            return Path.GetFileNameWithoutExtension(FilePath);
        }
    }
}