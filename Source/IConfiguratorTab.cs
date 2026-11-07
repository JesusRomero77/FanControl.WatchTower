using System;

namespace WatchTower
{
    /// <summary>
    /// Contrato que cumple cada pestaña del configurador (CurveReader, DataServer, RemoteSensors...).
    /// MainForm solo conoce esta interfaz: no sabe qué plugin hay detrás de cada pestaña, así que
    /// añadir una pestaña nueva no exige tocar la lógica de la ventana principal.
    /// </summary>
    public interface IConfiguratorTab
    {
        /// <summary>Título de la pestaña (el nombre del plugin).</summary>
        string Title { get; }

        /// <summary>True si el usuario ha cambiado algo en la interfaz que aún no se ha guardado.</summary>
        bool HasUnsavedChanges { get; }

        /// <summary>Vuelve a escribir los textos de la pestaña en el idioma actual de Translator.</summary>
        void ApplyLanguage();

        /// <summary>
        /// MainForm avisa de la carpeta de FanControl elegida (o recuperada de los ajustes), por si la
        /// pestaña la necesita, como la de CurveReader para leer los perfiles.
        /// </summary>
        void SetFanControlFolder(string fanControlFolder);

        /// <summary>
        /// La pestaña lo lanza cuando necesita que el usuario indique la ruta de FanControl (por ejemplo,
        /// al pulsar recargar sin haberla elegido). MainForm abre el diálogo; la pestaña no conoce la ventana.
        /// </summary>
        event EventHandler FanControlPathRequested;

        /// <summary>
        /// MainForm la llama en todas las pestañas al elegir Archivo > Guardar, tengan o no cambios.
        /// Cada pestaña decide qué escribe: si hay cambios, modifica solo lo cambiado en el XML de su
        /// plugin conservando el resto; si no los hay y el XML no existe, lo crea con los campos vacíos;
        /// si no los hay y ya existe, no lo toca. Devuelve false y un mensaje en 'error' si falla.
        /// </summary>
        bool Save(out string error);
    }
}