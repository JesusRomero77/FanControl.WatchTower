using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace WatchTower
{
    /// <summary>
    /// Pestaña de configuración del plugin CurveReader: elegir un perfil de FanControl y, para cada
    /// control y sensor, si se registra en el log, su intervalo y su umbral de cambio. Lee y escribe
    /// CurveReaderLoggerConfig.xml, el archivo que usa el plugin.
    /// </summary>
    public partial class TabCurveReader : UserControl, IConfiguratorTab
    {
        // Nombre del plugin, que es el título de la pestaña. No se traduce.
        private const string TabTitle = "CurveReader";

        // Carpeta de FanControl elegida por el usuario; null mientras no se haya configurado.
        private string _fanControlFolder;

        // Ajustes que el usuario está editando para el perfil seleccionado; null si no hay ninguno.
        private ProfileConfig _profileConfig;

        // Ajustes de cada perfil ya abierto en esta sesión, con el nombre de archivo del perfil como clave.
        // Así, al volver a un perfil, se recuperan los cambios que el usuario hizo y aún no guardó.
        private readonly Dictionary<string, ProfileConfig> _openedProfileConfigs =
            new Dictionary<string, ProfileConfig>(StringComparer.OrdinalIgnoreCase);

        // Perfiles (por nombre de archivo) cuyos ajustes ha cambiado el usuario desde el último guardado.
        // Abrir un perfil sin tocar nada no lo añade, así que no se escribe en el XML.
        private readonly HashSet<string> _changedProfileFileNames =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Idioma que tiene el XML del log (o el de la app, si el XML no existe o no lo indica). Si el
        // idioma de la app difiere de este, hay un cambio pendiente de guardar.
        private string _savedLanguageName;
        private bool _languageBaselineCaptured;

        /// <summary>
        /// Se lanza cuando la pestaña necesita que el usuario indique la ruta de FanControl y aún no la
        /// hay. MainForm abre el diálogo y después llama a SetFanControlFolder.
        /// </summary>
        public event EventHandler FanControlPathRequested;

        public TabCurveReader()
        {
            InitializeComponent();

            // El panel compartido muestra aquí la sección de curvas y avisa de los cambios del usuario.
            profileConfigPanel.ShowCurvesSection = true;
            profileConfigPanel.ItemChanged += (sender, e) => MarkProfileChanged();
        }

        public string Title => TabTitle;

        public bool HasUnsavedChanges => _changedProfileFileNames.Count > 0 || HasLanguageChanged;

        // True si el usuario ha cambiado el idioma de la app desde que se leyó el del XML.
        private bool HasLanguageChanged =>
            _languageBaselineCaptured &&
            !string.Equals(_savedLanguageName, Translator.CurrentLanguageName, StringComparison.OrdinalIgnoreCase);

        // Escribe en cada etiqueta el texto del idioma actual y reconstruye el panel de filas.
        // La primera vez que se llama (al abrir la app, antes de que el usuario toque nada) se anota
        // el idioma que ya tiene el XML, para poder detectar después si el usuario lo cambia.
        public void ApplyLanguage()
        {
            if (!_languageBaselineCaptured) CaptureLanguageBaseline();

            profileLabel.Text = Translator.Translate(TextId.Profile);
            mainToolTip.SetToolTip(refreshProfilesButton, Translator.Translate(TextId.RefreshProfiles));
            BuildProfileConfigPanel();
        }

        // Anota el idioma del XML. Si el XML no existe, está dañado o no indica idioma, se toma el
        // actual, de modo que no cuente como cambio pendiente hasta que el usuario lo cambie.
        private void CaptureLanguageBaseline()
        {
            string error;
            LoggerConfig storedConfig = LoggerConfigFile.Load(out error);
            string storedLanguage = storedConfig?.LanguageName;

            _savedLanguageName = string.IsNullOrEmpty(storedLanguage) ? Translator.CurrentLanguageName : storedLanguage;
            _languageBaselineCaptured = true;
        }

        // MainForm comunica la carpeta de FanControl: se guarda y se cargan los perfiles en el desplegable.
        public void SetFanControlFolder(string fanControlFolder)
        {
            _fanControlFolder = fanControlFolder;
            LoadProfiles();
        }

        // Guarda en CurveReaderLoggerConfig.xml solo lo que ha cambiado: parte de lo que ya hay escrito,
        // sustituye (o añade) los perfiles que el usuario ha modificado, actualiza el idioma y deja el
        // resto como estaba. Sin cambios no toca el archivo, salvo que no exista: entonces lo crea
        // (sin perfiles) para que el plugin lo encuentre con su estructura.
        public bool Save(out string error)
        {
            error = null;
            if (!HasUnsavedChanges && File.Exists(LoggerConfigFile.FilePath)) return true;

            LoggerConfig storedConfig = LoggerConfigFile.Load(out error);
            if (storedConfig == null) return false;

            storedConfig.LanguageName = Translator.CurrentLanguageName;

            // Cada perfil modificado reemplaza en su sitio al guardado con el mismo nombre de archivo,
            // o se añade al final si aún no tenía configuración.
            foreach (string fileName in _changedProfileFileNames)
            {
                ProfileConfig changedConfig = _openedProfileConfigs[fileName];
                ProfileConfig existingConfig = storedConfig.FindProfile(fileName);

                if (existingConfig == null)
                    storedConfig.Profiles.Add(changedConfig);
                else
                    storedConfig.Profiles[storedConfig.Profiles.IndexOf(existingConfig)] = changedConfig;
            }

            if (!LoggerConfigFile.Save(storedConfig, out error)) return false;

            _changedProfileFileNames.Clear();
            _savedLanguageName = Translator.CurrentLanguageName;
            return true;
        }

        // Anota que el perfil que se está editando tiene cambios sin guardar.
        private void MarkProfileChanged()
        {
            if (_profileConfig != null) _changedProfileFileNames.Add(_profileConfig.FileName);
        }

        // Rellena el desplegable con los perfiles de la carpeta que indica el CACHE y deja
        // seleccionado el perfil activo de FanControl.
        private void LoadProfiles()
        {
            profilesComboBox.Items.Clear();
            if (_fanControlFolder == null) return;

            string error;
            ProfileLocation location = ProfileLocator.Locate(_fanControlFolder, out error);
            if (location == null)
            {
                MessageBox.Show(this, string.Format(Translator.Translate(TextId.CacheReadError), error),
                    Translator.Translate(TextId.ErrorTitle), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            foreach (string path in ProfileLocator.GetProfileFiles(location.ProfilesFolder))
            {
                ProfileItem item = new ProfileItem(path);
                int index = profilesComboBox.Items.Add(item);

                if (string.Equals(item.FileName, location.ActiveProfileFileName, StringComparison.OrdinalIgnoreCase))
                    profilesComboBox.SelectedIndex = index;
            }
        }

        // Al elegir un perfil: si ya se abrió antes en esta sesión, se recuperan sus ajustes con los cambios
        // pendientes de guardar. Si no, se lee su contenido, se cruzan sus controles y sensores con los
        // ajustes guardados (o los valores por defecto) y se recuerda el resultado. Después se reconstruye el panel.
        private void ProfilesComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            ProfileItem selected = profilesComboBox.SelectedItem as ProfileItem;
            if (selected == null)
            {
                _profileConfig = null;
                BuildProfileConfigPanel();
                return;
            }

            ProfileConfig openedConfig;
            if (_openedProfileConfigs.TryGetValue(selected.FileName, out openedConfig))
            {
                _profileConfig = openedConfig;
                BuildProfileConfigPanel();
                return;
            }

            string error;
            ProfileContent content = ProfileReader.Read(selected.FilePath, out error);
            if (content == null)
            {
                MessageBox.Show(this, error, Translator.Translate(TextId.ErrorTitle), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Si el archivo de configuración existe pero no se puede leer, se avisa y se siguen
            // mostrando los valores por defecto.
            LoggerConfig storedConfig = LoggerConfigFile.Load(out error);
            if (storedConfig == null)
            {
                MessageBox.Show(this, error, Translator.Translate(TextId.ErrorTitle), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                storedConfig = new LoggerConfig();
            }

            _profileConfig = ProfileConfigBuilder.Build(
                selected.FileName, content, storedConfig.FindProfile(selected.FileName));
            _openedProfileConfigs[selected.FileName] = _profileConfig;
            BuildProfileConfigPanel();
        }

        // Botón de recargar junto al desplegable de perfiles. Si ya se eligió la ruta de FanControl,
        // vuelve a leer el CACHE y recarga la lista de perfiles (y el perfil activo). Si aún no se ha
        // elegido, pide a MainForm que la solicite al usuario.
        private void RefreshProfilesButton_Click(object sender, EventArgs e)
        {
            if (_fanControlFolder == null)
            {
                FanControlPathRequested?.Invoke(this, EventArgs.Empty);
                return;
            }

            LoadProfiles();
        }

        // Reconstruye el panel de filas a partir de _profileConfig (vacío si no hay perfil).
        private void BuildProfileConfigPanel()
        {
            profileConfigPanel.Build(_profileConfig);
        }
    }
}