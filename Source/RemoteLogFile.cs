using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace WatchTower
{
    /// <summary>
    /// Archivo de log de una grabación remota, en %LocalAppData%\FanControl y con el mismo formato que el
    /// log de CurveReader: resúmenes de perfil encerrados entre líneas "=" y un registro por línea que empieza
    /// por "[". El tamaño está limitado a 100 MB: al pasarse, se borran los registros más antiguos hasta dejar
    /// un 20% libre, y con ellos los resúmenes de perfil que se queden sin registros. Lo usa un solo hilo
    /// (el del grabador), por eso no tiene bloqueos.
    /// </summary>
    public class RemoteLogFile
    {
        /// <summary>Ancho de las líneas de "=" del resumen de perfil.</summary>
        public const int SummaryWidth = 35;
        private const long MaxSizeBytes = 100L * 1024L * 1024L;
        private const double SizeAfterPurge = 0.8;
        private const int EndOfLogMarkerWidth = 37;
        private const string FilePrefix = "RemoteSensorLogger-";
        private readonly FileInfo _file;

        public RemoteLogFile(string filePath)
        {
            _file = new FileInfo(filePath);
        }

        /// <summary>
        /// Ruta del log de un PC remoto: un archivo por PC, con su nombre o IP en el nombre del archivo.
        /// Los caracteres que Windows no admite en un nombre de archivo se sustituyen por "_".
        /// </summary>
        public static string GetFilePath(string remoteAddress)
        {
            char[] invalidChars = Path.GetInvalidFileNameChars();
            string safeAddress = new string(remoteAddress.Select(c => invalidChars.Contains(c) ? '_' : c).ToArray());

            string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FanControl");
            return Path.Combine(folder, FilePrefix + safeAddress + ".log");
        }

        /// <summary>Cabecera del log: el título centrado entre signos "=" hasta el ancho indicado.</summary>
        public static string BuildHeader(string title, int width)
        {
            int fill = Math.Max(2, width - title.Length - 2);
            int left = fill / 2;
            int right = fill - left;
            return new string('=', left) + " " + title + " " + new string('=', right);
        }

        /// <summary>
        /// Añade texto al final del log. Si el archivo se pasaría del tamaño máximo, antes se borran
        /// los registros más antiguos.
        /// </summary>
        public void Write(string text)
        {
            Directory.CreateDirectory(_file.DirectoryName);
            _file.Refresh();

            if (!_file.Exists)
            {
                File.WriteAllText(_file.FullName, text);
                return;
            }

            long newTextSize = Encoding.UTF8.GetByteCount(text);
            if (_file.Length + newTextSize <= MaxSizeBytes)
            {
                File.AppendAllText(_file.FullName, text);
                return;
            }

            WriteWithPurge(text, newTextSize);
        }

        /// <summary>Escribe la marca de fin de log en el idioma actual.</summary>
        public void WriteEndOfLogMarker()
        {
            Write(BuildHeader(Translator.Translate(TextId.EndOfLog), EndOfLogMarkerWidth) + "\r\n");
        }

        /// <summary>
        /// Quita del log la marca de fin de una grabación anterior, en cualquier idioma, para que al
        /// empezar otra grabación el archivo siga como una sola. Si no hay marca, no toca el archivo.
        /// </summary>
        public void RemoveEndOfLogMarker()
        {
            _file.Refresh();
            if (!_file.Exists) return;

            string[] lines = File.ReadAllLines(_file.FullName);
            if (!lines.Any(IsEndOfLogMarker)) return;

            File.WriteAllLines(_file.FullName, lines.Where(line => !IsEndOfLogMarker(line)));
        }

        // Reescribe el archivo sin los registros más antiguos y añade el texto nuevo al final. Todo se hace
        // en memoria (marcando líneas como null) y el archivo se escribe una sola vez.
        private void WriteWithPurge(string text, long newTextSize)
        {
            List<string> lines = File.ReadAllLines(_file.FullName).ToList();
            long currentSize = _file.Length;
            long targetSize = (long)(MaxSizeBytes * SizeAfterPurge) - newTextSize;

            // Se borran los registros desde el principio del archivo hasta liberar espacio suficiente.
            for (int i = 0; i < lines.Count && currentSize > targetSize; i++)
            {
                if (!lines[i].StartsWith("[")) continue;

                currentSize -= Encoding.UTF8.GetByteCount(lines[i] + Environment.NewLine);
                lines[i] = null;
            }

            RemoveSummariesWithoutRecords(lines);

            // Se escribe lo que queda. Se descartan las líneas en blanco del principio y las tandas de más
            // de dos seguidas, que quedan al borrar bloques del medio.
            using (StreamWriter writer = new StreamWriter(_file.FullName, false, new UTF8Encoding(false)))
            {
                bool contentStarted = false;
                int blankRun = 0;

                foreach (string line in lines)
                {
                    if (line == null) continue;

                    blankRun = string.IsNullOrWhiteSpace(line) ? blankRun + 1 : 0;
                    if (!contentStarted && blankRun > 0) continue;
                    if (blankRun > 2) continue;

                    contentStarted = true;
                    writer.WriteLine(line);
                }

                writer.Write(text);
            }
        }

        // Borra los resúmenes de perfil que se han quedado sin ningún registro detrás, antes del siguiente
        // resumen. El último resumen del archivo se conserva siempre: es el del perfil que se está grabando.
        private static void RemoveSummariesWithoutRecords(List<string> lines)
        {
            List<SummaryBlock> blocks = FindSummaryBlocks(lines);

            for (int b = 0; b < blocks.Count - 1; b++)
            {
                bool hasRecords = false;
                for (int i = blocks[b].LastLine + 1; i < blocks[b + 1].FirstLine; i++)
                {
                    if (lines[i] != null && lines[i].StartsWith("["))
                    {
                        hasRecords = true;
                        break;
                    }
                }

                if (hasRecords) continue;

                for (int i = blocks[b].FirstLine; i <= blocks[b].LastLine; i++)
                    lines[i] = null;
            }
        }

        // Localiza los resúmenes de perfil: cada uno empieza en una línea "=" y acaba en la tercera línea
        // "=" (cabecera, separador y cierre). La marca de fin de log también empieza por "=", pero no
        // es parte de ningún resumen.
        private static List<SummaryBlock> FindSummaryBlocks(List<string> lines)
        {
            List<SummaryBlock> blocks = new List<SummaryBlock>();
            int firstLine = -1;
            int equalsLines = 0;

            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i] == null || !lines[i].StartsWith("=") || IsEndOfLogMarker(lines[i])) continue;

                if (equalsLines == 0) firstLine = i;
                equalsLines++;

                if (equalsLines == 3)
                {
                    blocks.Add(new SummaryBlock { FirstLine = firstLine, LastLine = i });
                    equalsLines = 0;
                }
            }

            return blocks;
        }

        // Indica si la línea es la marca de fin de log en cualquiera de los idiomas, porque el idioma
        // pudo cambiar desde la última vez que se escribió el log.
        private static bool IsEndOfLogMarker(string line)
        {
            string trimmed = line.Trim();
            foreach (string title in Translator.GetTextInAllLanguages(TextId.EndOfLog))
                if (trimmed == BuildHeader(title, EndOfLogMarkerWidth)) return true;

            return false;
        }

        private class SummaryBlock
        {
            public int FirstLine { get; set; }
            public int LastLine { get; set; }
        }
    }
}