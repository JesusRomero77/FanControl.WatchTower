using System;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Xml.Linq;

namespace WatchTower
{
    /// <summary>
    /// Pestaña de configuración del plugin RemoteSensors: nombre y/o IP del PC remoto donde corre
    /// DataServer, puerto en el que escucha y contraseña que debe coincidir con la de DataServer. Lee y
    /// escribe RemoteSensorsConfig.xml, el archivo que usa el plugin.
    /// </summary>
    public partial class TabRemoteSensors : UserControl, IConfiguratorTab
    {
        // Nombre del plugin, que es el título de la pestaña. No se traduce.
        private const string TabTitle = "RemoteSensors";

        // Nombres exactos de los elementos que lee el plugin (XmlSerializer distingue mayúsculas).
        private const string RootElementName = "ClientSettings";
        private const string PcNameElementName = "PcName";
        private const string IpElementName = "Ip";
        private const string PortElementName = "Port";
        private const string PasswordElementName = "Password";

        // Contraseña que se guarda la primera vez si el usuario no escribe otra. Es la misma que usa
        // DataServer por defecto, para que sin tocar nada las dos partes ya coincidan. El plugin debe
        // usar la misma si es él quien crea el archivo.
        private const string DefaultPassword = "Password1234";

        // Comentario que acompaña a la contraseña en el XML, igual que el de los demás campos.
        private const string PasswordComment = " Password set in DataServer on the REMOTE PC. It must be the same. ";

        // Puerto por defecto de DataServer: el que aplica el plugin cuando el campo está vacío en el
        // XML. Es el mismo que usa la pestaña de DataServer.
        private const int DefaultPort = 8765;
        private const int MinPort = 1;
        private const int MaxPort = 65535;

        // Mensaje de Windows que pone el texto gris de ejemplo en un TextBox. Con wParam = 1 el texto
        // se sigue viendo aunque el campo tenga el foco, hasta que el usuario escribe algo.
        private const int EmSetCueBanner = 0x1501;

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, string lParam);

        // Valores que muestran los campos tras leer o guardar el XML. Con ellos se detecta si el
        // usuario ha cambiado algo. Si el XML tenía una IP que no es una IPv4 válida o un puerto fuera
        // de rango, el campo lo muestra vacío y aquí figura vacío. Puerto vacío significa "usar el 8765".
        private string _savedPcName = "";
        private string _shownIp = "";
        private string _savedPort = "";

        // Contraseña que hay en el XML. Vacío significa que el XML aún no tiene ninguna configurada.
        private string _savedPassword = "";
        private bool _settingsLoaded;

        // Esta pestaña nunca necesita pedir la ruta de FanControl (el XML no depende de ella), pero
        // la interfaz obliga a tener el evento.
#pragma warning disable CS0067
        public event EventHandler FanControlPathRequested;
#pragma warning restore CS0067

        public TabRemoteSensors()
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
            "FanControl", "RemoteSensorsConfig.xml");

        // Lo que hay ahora escrito en cada campo. El nombre y el puerto se recortan por los extremos;
        // la IP es "" mientras esté vacía o a medias.
        private string CurrentPcName => pcNameTextBox.Text.Trim();
        private string CurrentIp => ipAddressBox.Value;
        private string CurrentPort => portTextBox.Text.Trim();

        // Lo que ha escrito el usuario ahora en la contraseña, recortado por los extremos.
        private string TypedPassword => passwordTextBox.Text.Trim();

        // Contraseña actual: la que ya hay en el XML o, si aún no hay ninguna, la que se guardará por
        // defecto. Es la que se ve en gris en el campo.
        private string CurrentPassword => _savedPassword.Length > 0 ? _savedPassword : DefaultPassword;

        // True si el usuario ha escrito una contraseña distinta de la actual. El campo vacío
        // significa "conservar la actual".
        private bool HasNewPassword => TypedPassword.Length > 0 && TypedPassword != CurrentPassword;

        public bool HasUnsavedChanges => _settingsLoaded &&
            (CurrentPcName != _savedPcName || CurrentIp != _shownIp || CurrentPort != _savedPort || HasNewPassword || HasPartialIp);

        // True si la IP tiene algunos números pero no los cuatro: no se puede guardar.
        private bool HasPartialIp => !ipAddressBox.IsEmpty && !ipAddressBox.IsComplete;

        // Escribe en cada etiqueta el texto del idioma actual. La primera vez que se llama (al abrir
        // la app) se leen también los valores que ya tiene el XML y se muestran en los campos.
        public void ApplyLanguage()
        {
            if (!_settingsLoaded) LoadSettings();

            warningLabel.Text = Translator.Translate(TextId.RemoteSensorsWarning);
            pcNameLabel.Text = Translator.Translate(TextId.RemotePcName);
            pcNameHelpLabel.Text = Translator.Translate(TextId.RemotePcNameHelp);
            ipLabel.Text = Translator.Translate(TextId.RemoteIp);
            ipHelpLabel.Text = Translator.Translate(TextId.RemoteIpHelp);
            portLabel.Text = Translator.Translate(TextId.RemotePort);
            passwordLabel.Text = Translator.Translate(TextId.DataServerPassword);
            portHelpLabel.Text = Translator.Translate(TextId.RemotePortHelp);
            noteLabel.Text = Translator.Translate(TextId.RemoteSensorsNote);
        }

        // Esta pestaña no usa la carpeta de FanControl.
        public void SetFanControlFolder(string fanControlFolder)
        {
        }

        // Pone el texto gris de ejemplo en un TextBox (no hace nada si aún no tiene handle).
        private static void SetCueBanner(TextBox textBox, string text)
        {
            if (textBox.IsHandleCreated)
                SendMessage(textBox.Handle, EmSetCueBanner, (IntPtr)1, text);
        }

        // Lee PcName, Ip, Port y Password del XML, si existe y tiene el formato esperado. Los tres
        // primeros se muestran en los campos; la contraseña no se escribe en el campo: queda vacío y la
        // contraseña actual se ve en gris. Si el archivo no se puede leer, los campos quedan vacíos; Save
        // avisará del problema antes de escribir nada.
        private void LoadSettings()
        {
            _settingsLoaded = true;
            string savedIp = "";
            string savedPort = "";

            try
            {
                if (File.Exists(ConfigFilePath))
                {
                    XElement root = XDocument.Load(ConfigFilePath).Root;
                    if (root != null && root.Name == RootElementName)
                    {
                        _savedPcName = ((string)root.Element(PcNameElementName) ?? "").Trim();
                        savedIp = ((string)root.Element(IpElementName) ?? "").Trim();
                        savedPort = ((string)root.Element(PortElementName) ?? "").Trim();
                        _savedPassword = ((string)root.Element(PasswordElementName) ?? "").Trim();
                    }
                }
            }
            catch (Exception)
            {
                // XML ilegible: se dejan los valores vacíos.
            }

            int port;
            _savedPort = TryParsePort(savedPort, out port) ? port.ToString(CultureInfo.InvariantCulture) : "";

            pcNameTextBox.Text = _savedPcName;
            ipAddressBox.Value = savedIp;
            _shownIp = ipAddressBox.Value;
            portTextBox.Text = _savedPort;
            passwordTextBox.Text = "";
            SetCueBanner(passwordTextBox, CurrentPassword);
        }

        // Guarda PcName, Ip, Port y Password en el XML del plugin. Una IP a medias, un puerto fuera de
        // rango o una contraseña inválida no se guardan: se devuelve un error. Sin cambios no toca el
        // archivo, salvo que no exista (lo crea) o que no tenga contraseña (escribe la de por defecto). Si
        // ya existe, se modifica solo lo que ha cambiado el usuario y se conserva el resto (comentarios y
        // valores que no ha tocado). Si el archivo existe pero no tiene el formato esperado, no se
        // sobrescribe y se devuelve un error.
        public bool Save(out string error)
        {
            error = null;

            if (HasPartialIp)
            {
                error = Translator.Translate(TextId.RemoteIpIncomplete);
                return false;
            }

            bool fileExists = File.Exists(ConfigFilePath);
            bool needsInitialPassword = _savedPassword.Length == 0;
            if (!HasUnsavedChanges && fileExists && !needsInitialPassword) return true;

            // Solo se valida el puerto si se va a escribir. Un puerto vacío es válido (= por defecto).
            bool writePort = !fileExists || CurrentPort != _savedPort;
            string normalizedPort = "";
            if (writePort && CurrentPort.Length > 0)
            {
                int port;
                if (!TryParsePort(CurrentPort, out port))
                {
                    error = string.Format(Translator.Translate(TextId.DataServerPortInvalid), MinPort, MaxPort);
                    return false;
                }
                normalizedPort = port.ToString(CultureInfo.InvariantCulture);
            }

            // Contraseña que se escribirá: la nueva que haya escrito el usuario o, si el archivo aún no
            // tiene ninguna, la de por defecto. Vacía significa que no se escribe nada.
            string passwordToWrite = HasNewPassword ? TypedPassword : (needsInitialPassword ? DefaultPassword : "");
            if (passwordToWrite.Length > 0 && !PasswordRules.IsValid(passwordToWrite))
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
                    error = string.Format(Translator.Translate(TextId.ConfigFileBadFormat),"RemoteSensorsConfig.xml", RootElementName);
                    return false;
                }

                // En un archivo nuevo se escriben los tres primeros campos; en uno existente, solo los cambiados.
                if (!fileExists || CurrentPcName != _savedPcName)
                    root.SetElementValue(PcNameElementName, CurrentPcName);
                if (!fileExists || CurrentIp != _shownIp)
                    root.SetElementValue(IpElementName, CurrentIp);
                if (writePort)
                    root.SetElementValue(PortElementName, normalizedPort);
                if (passwordToWrite.Length > 0)
                    WritePassword(root, passwordToWrite);

                Directory.CreateDirectory(Path.GetDirectoryName(ConfigFilePath));
                document.Save(ConfigFilePath);
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }

            // Lo guardado pasa a ser lo mostrado. El puerto se reescribe normalizado (08765 -> 8765) y la
            // contraseña nueva pasa a ser la actual: el campo vuelve a quedar vacío con ella en gris.
            _savedPcName = CurrentPcName;
            _shownIp = CurrentIp;
            if (writePort)
            {
                _savedPort = normalizedPort;
                portTextBox.Text = normalizedPort;
            }
            if (passwordToWrite.Length > 0)
            {
                _savedPassword = passwordToWrite;
                passwordTextBox.Text = "";
                SetCueBanner(passwordTextBox, CurrentPassword);
            }
            return true;
        }

        // Escribe la contraseña. Si el XML es de una versión anterior y aún no tiene el elemento, lo añade
        // precedido de su comentario, como queda en un archivo nuevo.
        private static void WritePassword(XElement root, string password)
        {
            if (root.Element(PasswordElementName) == null)
                root.Add(new XComment(PasswordComment));
            root.SetElementValue(PasswordElementName, password);
        }

        // Documento con la misma estructura y los mismos comentarios que crea el propio plugin la
        // primera vez que se carga, con los campos vacíos (Save escribe enseguida la contraseña).
        private static XDocument CreateDefaultDocument()
        {
            return new XDocument(
                new XDeclaration("1.0", "utf-8", null),
                new XElement(RootElementName,
                    new XComment(" Name of the REMOTE PC: type 'hostname' in a console on the remote PC. "),
                    new XElement(PcNameElementName, ""),
                    new XComment(" IP of the REMOTE PC: type 'ipconfig' in a console on the remote PC. "),
                    new XElement(IpElementName, ""),
                    new XComment(" Port DataServer listens on at the REMOTE PC. Leave empty to use 8765. "),
                    new XElement(PortElementName, ""),
                    new XComment(PasswordComment),
                    new XElement(PasswordElementName, "")));
        }

        // True si el texto es un número entero entre MinPort y MaxPort (solo dígitos).
        private static bool TryParsePort(string text, out int port)
        {
            return int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out port)
                   && port >= MinPort && port <= MaxPort;
        }

        // Solo deja escribir dígitos (y teclas de control como Retroceso) en el campo del puerto.
        private void PortTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
                e.Handled = true;
        }
    }
}