using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Xml.Linq;

namespace WatchTower
{
    /// <summary>
    /// Pestaña de configuración del plugin DataServer: puerto en el que escucha el servidor HTTP y
    /// contraseña que deben enviar los clientes en la cabecera Authorization. Lee y escribe
    /// DataServerConfig.xml, el archivo que usa el plugin.
    /// </summary>
    public partial class TabDataServer : UserControl, IConfiguratorTab
    {
        // Nombre del plugin, que es el título de la pestaña. No se traduce.
        private const string TabTitle = "DataServer";

        // Carpeta de FanControl indicada por el usuario; null mientras no la haya indicado.
        private string _fanControlFolder;

        // Nombres exactos de los elementos del XML (XmlSerializer distingue mayúsculas).
        private const string RootElementName = "ServerSettings";
        private const string PortElementName = "Port";
        private const string PasswordElementName = "Password";

        // Puerto por defecto: el que aplica el plugin cuando el campo está vacío en el XML.
        private const int DefaultPort = 8765;

        // Contraseña que se guarda la primera vez si el usuario no escribe otra. El plugin debe usar
        // la misma si es él quien crea el archivo.
        private const string DefaultPassword = "Password1234";

        private const int MinPort = 1;
        private const int MaxPort = 65535;

        // Mensaje de Windows que pone el texto gris de ejemplo en un TextBox. Con wParam = 1 el texto
        // se sigue viendo aunque el campo tenga el foco, hasta que el usuario escribe algo.
        private const int EmSetCueBanner = 0x1501;

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, string lParam);

        // Valores que hay en el XML (o que muestran los campos tras leerlo o guardarlo). Con ellos se
        // detecta si el usuario ha cambiado algo. En el puerto, vacío significa "usar el de por defecto".
        // En la contraseña, vacío significa que el XML aún no tiene ninguna configurada.
        private string _savedPort = "";
        private string _savedPassword = "";
        private bool _settingsLoaded;

        // Esta pestaña nunca necesita pedir la ruta de FanControl (el XML no depende de ella), pero
        // la interfaz obliga a tener el evento.
#pragma warning disable CS0067
        public event EventHandler FanControlPathRequested;
#pragma warning restore CS0067

        public TabDataServer()
        {
            InitializeComponent();

            // El texto gris solo se puede poner cuando el TextBox ya tiene handle de Windows, y el
            // handle puede recrearse, así que se pone cada vez que se crea.
            portTextBox.HandleCreated += (sender, e) =>
                SetCueBanner(portTextBox, DefaultPort.ToString(CultureInfo.InvariantCulture));
            passwordTextBox.HandleCreated += (sender, e) =>
                SetCueBanner(passwordTextBox, CurrentPassword);
        }

        public string Title => TabTitle;

        // Ruta del XML del plugin, en %LocalAppData%\FanControl.
        private static string ConfigFilePath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "FanControl", "DataServerConfig.xml");

        // Lo que ha escrito el usuario ahora en cada campo, recortado por los extremos.
        private string TypedPort => portTextBox.Text.Trim();
        private string TypedPassword => passwordTextBox.Text.Trim();

        // Contraseña actual: la que ya hay en el XML o, si aún no hay ninguna, la que se guardará por
        // defecto. Es la que se ve en gris en el campo y en la ventana emergente del icono "?".
        private string CurrentPassword => _savedPassword.Length > 0 ? _savedPassword : DefaultPassword;

        // True si el usuario ha escrito una contraseña distinta de la actual. El campo vacío
        // significa "conservar la actual".
        private bool HasNewPassword => TypedPassword.Length > 0 && TypedPassword != CurrentPassword;

        public bool HasUnsavedChanges =>
            _settingsLoaded && (TypedPort != _savedPort || HasNewPassword);

        // Escribe en cada etiqueta el texto del idioma actual. La primera vez que se llama (al abrir
        // la app) se leen también los valores que ya tiene el XML y se muestran en los campos.
        public void ApplyLanguage()
        {
            if (!_settingsLoaded) LoadSettings();

            portLabel.Text = Translator.Translate(TextId.DataServerPort);
            portHelpLabel.Text = Translator.Translate(TextId.DataServerPortHelp);
            passwordLabel.Text = Translator.Translate(TextId.DataServerPassword);
            noteLabel.Text = Translator.Translate(TextId.DataServerNote);
            pluginMissingLabel.Text = Translator.Translate(TextId.DataServerPluginMissing);

            UpdatePasswordCue();
        }

        // Recuerda la carpeta de FanControl y comprueba si el plugin está instalado en ella.
        public void SetFanControlFolder(string fanControlFolder)
        {
            _fanControlFolder = fanControlFolder;
            UpdatePluginMissingWarning();
        }

        // Muestra el aviso si se conoce la carpeta de FanControl y el plugin no está instalado.
        // Sin carpeta conocida no se puede saber, así que no se muestra nada.
        private void UpdatePluginMissingWarning()
        {
            pluginMissingLabel.Visible = _fanControlFolder != null && !IsPluginInstalled(_fanControlFolder);
        }

        // True si la dll del plugin está en la carpeta Plugins, ya sea suelta (copiada a mano) o dentro de
        // su subcarpeta (como la deja el instalador de plugins de FanControl).
        private static bool IsPluginInstalled(string fanControlFolder)
        {
            return File.Exists(Path.Combine(fanControlFolder, "Plugins", "FanControl.DataServer.dll")) ||
                   File.Exists(Path.Combine(fanControlFolder, "Plugins", "DataServer", "FanControl.DataServer.dll"));
        }

        // Al volver a esta pestaña se repite la comprobación, por si el usuario ha instalado el plugin
        // con WatchTower abierto.
        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (Visible) UpdatePluginMissingWarning();
        }

        // Pone el texto gris de ejemplo en un TextBox (no hace nada si aún no tiene handle).
        private static void SetCueBanner(TextBox textBox, string text)
        {
            if (textBox.IsHandleCreated)
                SendMessage(textBox.Handle, EmSetCueBanner, (IntPtr)1, text);
        }

        // Muestra la contraseña actual en gris en el campo y en el texto de la ventana emergente del
        // icono "?". Se llama al cambiar de idioma y cada vez que cambia la contraseña guardada.
        private void UpdatePasswordCue()
        {
            SetCueBanner(passwordTextBox, CurrentPassword);
            toolTip.SetToolTip(passwordHelpIcon,
                string.Format(Translator.Translate(TextId.DataServerPasswordTooltip), CurrentPassword));
        }

        // Lee Port y Password del XML, si existe y tiene el formato esperado. El puerto se muestra en
        // el campo (si no es válido, vacío); la contraseña no se escribe en el campo: queda vacío y la
        // contraseña actual se ve en gris. Si el archivo no se puede leer, todo queda vacío; Save
        // avisará del problema antes de escribir nada.
        private void LoadSettings()
        {
            _settingsLoaded = true;
            string savedPort = "";
            string savedPassword = "";

            try
            {
                if (File.Exists(ConfigFilePath))
                {
                    XElement root = XDocument.Load(ConfigFilePath).Root;
                    if (root != null && root.Name == RootElementName)
                    {
                        savedPort = ((string)root.Element(PortElementName) ?? "").Trim();
                        savedPassword = ((string)root.Element(PasswordElementName) ?? "").Trim();
                    }
                }
            }
            catch (Exception)
            {
                // XML ilegible: se dejan los valores vacíos.
            }

            int port;
            _savedPort = TryParsePort(savedPort, out port) ? port.ToString(CultureInfo.InvariantCulture) : "";
            _savedPassword = savedPassword;

            portTextBox.Text = _savedPort;
            passwordTextBox.Text = "";
        }

        // Guarda Port y Password en el XML del plugin. Si no hay cambios y el archivo ya existe con
        // contraseña, no lo toca. Si no existe, lo crea. Si existe pero no tiene contraseña, escribe
        // la de por defecto. Si ya existe, se modifica solo lo que ha cambiado el usuario y se conserva
        // el resto (comentarios y valores que no ha tocado). Un valor inválido o un archivo con formato
        // inesperado no se sobrescribe: se devuelve un error.
        public bool Save(out string error)
        {
            error = null;

            bool fileExists = File.Exists(ConfigFilePath);
            bool needsInitialPassword = _savedPassword.Length == 0;
            if (!HasUnsavedChanges && fileExists && !needsInitialPassword) return true;

            // En un archivo nuevo se escribe el puerto; en uno existente, solo si ha cambiado.
            bool writePort = !fileExists || TypedPort != _savedPort;

            // Contraseña que se escribirá: la nueva que haya escrito el usuario o, si el archivo aún no
            // tiene ninguna, la de por defecto. Vacía significa que no se escribe nada.
            string passwordToWrite = HasNewPassword ? TypedPassword : (needsInitialPassword ? DefaultPassword : "");

            // Solo se validan los campos que se van a escribir. Un puerto vacío es válido (= por defecto).
            string normalizedPort = "";
            if (writePort && TypedPort.Length > 0)
            {
                int port;
                if (!TryParsePort(TypedPort, out port))
                {
                    error = string.Format(Translator.Translate(TextId.DataServerPortInvalid), MinPort, MaxPort);
                    return false;
                }
                normalizedPort = port.ToString(CultureInfo.InvariantCulture);
            }

            if (passwordToWrite.Length > 0 && !IsValidPassword(passwordToWrite))
            {
                error = Translator.Translate(TextId.DataServerPasswordInvalid);
                return false;
            }

            try
            {
                XDocument document = fileExists ? XDocument.Load(ConfigFilePath) : CreateDefaultDocument();
                XElement root = document.Root;

                if (root == null || root.Name != RootElementName)
                {
                    error = string.Format(Translator.Translate(TextId.ConfigFileBadFormat),"DataServerConfig.xml", RootElementName);
                    return false;
                }

                if (writePort) root.SetElementValue(PortElementName, normalizedPort);
                if (passwordToWrite.Length > 0) root.SetElementValue(PasswordElementName, passwordToWrite);

                Directory.CreateDirectory(Path.GetDirectoryName(ConfigFilePath));
                document.Save(ConfigFilePath);
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }

            // Lo guardado pasa a ser lo mostrado. El puerto se reescribe normalizado (08765 -> 8765) y
            // la contraseña nueva pasa a ser la actual: el campo vuelve a quedar vacío con ella en gris.
            if (writePort)
            {
                _savedPort = normalizedPort;
                portTextBox.Text = normalizedPort;
            }
            if (passwordToWrite.Length > 0)
            {
                _savedPassword = passwordToWrite;
                passwordTextBox.Text = "";
                UpdatePasswordCue();
            }
            return true;
        }

        // Documento con la estructura y los comentarios que crearía el propio plugin la primera vez,
        // con los dos campos vacíos (Save escribe enseguida la contraseña).
        private static XDocument CreateDefaultDocument()
        {
            return new XDocument(
                new XDeclaration("1.0", "utf-8", null),
                new XElement(RootElementName,
                    new XComment(" Port the server listens on (1-65535). Leave empty to use 8765. "),
                    new XElement(PortElementName, ""),
                    new XComment(" Password clients must send in the Authorization header. "),
                    new XElement(PasswordElementName, "")));
        }

        // True si el texto es un número entero entre MinPort y MaxPort (solo dígitos).
        private static bool TryParsePort(string text, out int port)
        {
            return int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out port)
                   && port >= MinPort && port <= MaxPort;
        }

        // Todos los caracteres deben ser ASCII visible, porque la contraseña viaja en una cabecera HTTP.
        private static bool IsValidPassword(string password)
        {
            return password.All(character => character > ' ' && character <= '~');
        }

        // Solo deja escribir dígitos (y teclas de control como Retroceso) en el campo del puerto.
        private void PortTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
                e.Handled = true;
        }

        // Dibuja el icono "?": un círculo azul con la interrogación en blanco.
        private void PasswordHelpIcon_Paint(object sender, PaintEventArgs e)
        {
            Control icon = (Control)sender;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle circle = new Rectangle(1, 1, icon.Width - 3, icon.Height - 3);
            using (Brush fill = new SolidBrush(Color.SteelBlue))
                e.Graphics.FillEllipse(fill, circle);

            using (Font font = new Font("Segoe UI", 11F, FontStyle.Bold))
                TextRenderer.DrawText(e.Graphics, "?", font, circle, Color.White,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
        }
    }
}