using System;
using System.Drawing;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;


namespace WatchTower
{
    /// <summary>
    /// Ventana principal: solo contiene el menú y el TabControl. Todo lo demás vive dentro de las
    /// pestañas, una por plugin. La ventana habla con ellas solo a través de IConfiguratorTab.
    /// </summary>
    public partial class MainForm : Form
    {
        // Pestañas del configurador, en el mismo orden que en el TabControl.
        private readonly List<IConfiguratorTab> _tabs = new List<IConfiguratorTab>();

        public MainForm()
        {
            InitializeComponent();
            Icon = Properties.Resources.WatchTower;
        }

        // Al abrir: se crea el menú de idiomas, se aplica el idioma guardado (o el de por defecto),
        // se crean las pestañas y, si el usuario ya eligió antes la ruta de FanControl y sigue siendo
        // válida, se les comunica.
        private void MainForm_Load(object sender, EventArgs e)
        {
            BuildLanguageMenu();
            Translator.SetLanguage(AppSettings.LoadLanguage(Translator.CurrentLanguageName));
            BuildTabs();
            ApplyLanguage();

            string savedFolder = AppSettings.LoadFanControlFolder();
            if (savedFolder != null && File.Exists(FanControlLocator.GetCachePath(savedFolder)))
                SetFanControlFolder(savedFolder);
        }

        // Crea las pestañas. Para añadir un plugin nuevo basta con añadir aquí su pestaña.
        private void BuildTabs()
        {
            AddTab(new TabCurveReader());
            AddTab(new TabRemoteSensors());
            AddTab(new TabDataServer());
            AddTab(new TabRemoteSensorLogger());
        }

        // Añade una pestaña al control de pestañas y la conecta con la ventana. Se pide un UserControl que
        // además cumpla IConfiguratorTab: lo primero para poder colocarlo en la ventana, lo segundo
        // para que MainForm sepa hablar con él.
        private void AddTab<T>(T tab) where T : UserControl, IConfiguratorTab
        {
            tab.FanControlPathRequested += (sender, e) => SelectFanControlPath();

            mainTabHost.AddPage(tab.Title, tab);
            _tabs.Add(tab);
        }

        // Comunica a todas las pestañas la carpeta de FanControl.
        private void SetFanControlFolder(string fanControlFolder)
        {
            foreach (IConfiguratorTab tab in _tabs)
                tab.SetFanControlFolder(fanControlFolder);
        }

        // Pregunta al usuario por FanControl.exe (o un acceso directo), comprueba que junto a él exista
        // Configurations\CACHE, recuerda la ruta y se la comunica a las pestañas. Lo usan el menú
        // Configuración > Especificar Ruta Fan Control y las pestañas que piden la ruta.
        private void SelectFanControlPath()
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Title = Translator.Translate(TextId.SelectFanControlTitle);
                dialog.Filter = Translator.Translate(TextId.FanControlFileFilter) + "|*.exe;*.lnk";

                if (dialog.ShowDialog(this) != DialogResult.OK) return;

                string folder;
                if (!FanControlLocator.TryGetFanControlFolder(dialog.FileName, out folder))
                {
                    MessageBox.Show(this,Translator.Translate(TextId.InvalidPathMessage),
                        Translator.Translate(TextId.InvalidPathTitle),MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                AppSettings.SaveFanControlFolder(folder);
                SetFanControlFolder(folder);
            }
        }

        private void FanControlPathToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SelectFanControlPath();
        }

        // Archivo > Guardar.
        private void SaveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveAllTabs();
        }

        // Llama a Save en todas las pestañas (cada una decide si tiene algo que escribir) y, si alguna
        // falla, muestra todos los errores juntos. Devuelve true si todas se guardaron bien.
        private bool SaveAllTabs()
        {
            List<string> errors = new List<string>();

            foreach (IConfiguratorTab tab in _tabs)
            {
                string error;
                if (!tab.Save(out error))
                    errors.Add(tab.Title + ": " + error);
            }

            if (errors.Count == 0) return true;

            MessageBox.Show(this, string.Join("\r\n", errors), Translator.Translate(TextId.ErrorTitle),
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        // Archivo > Salir: se cierra la ventana (no Application.Exit) para que pase por
        // MainForm_FormClosing y avise de los cambios sin guardar.
        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        // Al cerrar (menú, X de la ventana o Alt+F4): si alguna pestaña tiene cambios pendientes se
        // pregunta si guardarlos. Cancelar, o un fallo al guardar, mantiene la ventana abierta.
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!_tabs.Any(tab => tab.HasUnsavedChanges)) return;

            DialogResult answer = MessageBox.Show(this,
                Translator.Translate(TextId.UnsavedChangesMessage),
                "WatchTower", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

            if (answer == DialogResult.Cancel)
                e.Cancel = true;
            else if (answer == DialogResult.Yes && !SaveAllTabs())
                e.Cancel = true;
        }

        // Escribe en cada menú el texto del idioma actual, marca cuál está activo y pide a cada
        // pestaña que haga lo mismo con las suyas; también actualiza el título de cada pestaña.
        private void ApplyLanguage()
        {
            fileToolStripMenuItem.Text = Translator.Translate(TextId.File);
            saveToolStripMenuItem.Text = Translator.Translate(TextId.Save);
            exitToolStripMenuItem.Text = Translator.Translate(TextId.Exit);

            configurationToolStripMenuItem.Text = Translator.Translate(TextId.Settings);
            languageToolStripMenuItem.Text = Translator.Translate(TextId.Language);
            fanControlPathToolStripMenuItem.Text = Translator.Translate(TextId.FanControlPath);

            helpToolStripMenuItem.Text = Translator.Translate(TextId.Help);
            viewHelpToolStripMenuItem.Text = Translator.Translate(TextId.ViewHelp);
            aboutToolStripMenuItem.Text = Translator.Translate(TextId.About);

            // Los nombres de los idiomas no se traducen: cada uno aparece en su propio idioma.
            foreach (ToolStripMenuItem item in languageToolStripMenuItem.DropDownItems)
                item.Checked = (string)item.Tag == Translator.CurrentLanguageName;

            for (int i = 0; i < _tabs.Count; i++)
            {
                _tabs[i].ApplyLanguage();
                mainTabHost.SetPageTitle(i, _tabs[i].Title);
            }
        }

        // Crea una entrada en el menú Configuración > Idioma por cada idioma disponible. El nombre
        // del idioma se guarda en Tag para saber cuál se ha elegido cuando el usuario hace clic.
        private void BuildLanguageMenu()
        {
            languageToolStripMenuItem.DropDownItems.Clear();

            foreach (string languageName in Translator.AvailableLanguageNames)
            {
                ToolStripMenuItem item = new ToolStripMenuItem(languageName) { Tag = languageName };
                item.Click += LanguageToolStripMenuItem_Click;
                languageToolStripMenuItem.DropDownItems.Add(item);
            }
        }

        // Clic en un idioma del menú: cambia la interfaz a ese idioma y lo recuerda en los ajustes de la
        // app. El idioma también se escribe en el XML de CurveReader la próxima vez que se guarde.
        private void LanguageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string languageName = (string)((ToolStripMenuItem)sender).Tag;

            Translator.SetLanguage(languageName);
            AppSettings.SaveLanguage(languageName);
            ApplyLanguage();
        }

        // Menú Ayuda > Acerca de.
        private void AboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (AboutForm aboutForm = new AboutForm())
                aboutForm.ShowDialog(this);
        }

        // Menú Ayuda > Ver la ayuda.
        private void ViewHelpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (HelpForm helpForm = new HelpForm())
                helpForm.ShowDialog(this);
        }
    }
}