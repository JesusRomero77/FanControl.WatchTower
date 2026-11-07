using System;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace WatchTower
{
    /// <summary>
    /// Ventana "Acerca de": icono, nombre y versión del programa, una breve descripción de qué es,
    /// el enlace al repositorio de GitHub y el aviso de licencia de Newtonsoft.Json. Se muestra
    /// siempre en el idioma que tenga activo el formulario principal en ese momento.
    /// </summary>
    public class AboutForm : Form
    {
        // URL del repositorio de GitHub donde se aloja el proyecto (la app configuradora).
        private const string GitHubRepositoryUrl = "https://github.com/JesusRomero77/FanControl.WatchTower";

        // Cuánto se oscurece el fondo de la ventana para el recuadro de la descripción.
        private const int DescriptionPanelDarkening = 15;

        public AboutForm()
        {
            BuildLayout();
        }

        // Construye todos los controles de la ventana en código. Cada control se añade al
        // formulario justo después de crearlo y ANTES de usar su tamaño para colocar el
        // siguiente: un control con AutoSize no tiene su tamaño real hasta que está en un
        // contenedor, así que leerlo antes daría medidas provisionales (como le pasaba al
        // botón, que por eso quedaba recortado por el borde de la ventana).
        private void BuildLayout()
        {
            Text = Translator.Translate(TextId.AboutWindowTitle);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Icon = Properties.Resources.WatchTower;

            const int margin = 20;
            const int iconSize = 64;
            const int textLeft = margin + iconSize + 15;
            const int textWidth = 300;

            PictureBox iconBox = new PictureBox
            {
                Image = Properties.Resources.WatchTower.ToBitmap(),
                SizeMode = PictureBoxSizeMode.Zoom,
                Size = new Size(iconSize, iconSize),
                Location = new Point(margin, margin)
            };
            Controls.Add(iconBox);

            Label nameVersionLabel = new Label
            {
                Text = "WatchTower " + Translator.Translate(TextId.Version) + " " + GetDisplayVersion(),
                Font = new Font(Font, FontStyle.Bold),
                AutoSize = true,
                UseMnemonic = false,
                Location = new Point(textLeft, margin)
            };
            Controls.Add(nameVersionLabel);

            Panel descriptionPanel = BuildDescriptionPanel(textLeft, nameVersionLabel.Bottom + 10, textWidth);
            Controls.Add(descriptionPanel);

            LinkLabel gitHubLink = new LinkLabel
            {
                Text = Translator.Translate(TextId.ViewOnGitHub),
                AutoSize = true,
                UseMnemonic = false,
                Location = new Point(textLeft, descriptionPanel.Bottom + 15)
            };
            gitHubLink.LinkClicked += (sender, e) => Process.Start(GitHubRepositoryUrl);
            Controls.Add(gitHubLink);

            Label jsonNoticeLabel = new Label
            {
                Text = Translator.Translate(TextId.AboutJsonNotice),
                AutoSize = true,
                UseMnemonic = false,
                ForeColor = SystemColors.GrayText,
                MaximumSize = new Size(textWidth, 0),
                Location = new Point(textLeft, gitHubLink.Bottom + 15)
            };
            Controls.Add(jsonNoticeLabel);

            Button okButton = new Button
            {
                Text = Translator.Translate(TextId.Ok),
                AutoSize = true,
                DialogResult = DialogResult.OK
            };
            Controls.Add(okButton);
            okButton.Location = new Point(jsonNoticeLabel.Right - okButton.Width, jsonNoticeLabel.Bottom + 20);

            AcceptButton = okButton;
            ClientSize = new Size(descriptionPanel.Right + margin, okButton.Bottom + margin);
        }

        // Crea el recuadro con el texto explicativo del programa: un panel con fondo algo más
        // oscuro que el de la ventana, con la etiqueta de la descripción dentro con un pequeño
        // margen. La etiqueta se añade al panel (y este a la ventana) antes de medir su tamaño,
        // por el mismo motivo explicado en BuildLayout.
        private Panel BuildDescriptionPanel(int left, int top, int textWidth)
        {
            const int padding = 8;

            Panel panel = new Panel
            {
                BackColor = Darken(BackColor, DescriptionPanelDarkening),
                Location = new Point(left, top)
            };

            Label descriptionLabel = new Label
            {
                Text = Translator.Translate(TextId.AboutDescription),
                AutoSize = true,
                UseMnemonic = false,
                MaximumSize = new Size(textWidth, 0),
                Location = new Point(padding, padding)
            };
            panel.Controls.Add(descriptionLabel);

            panel.Size = new Size(textWidth + padding * 2, descriptionLabel.Bottom + padding);
            return panel;
        }

        // Resta 'amount' a cada componente de color, sin bajar de 0, para obtener un tono
        // ligeramente más oscuro que el original.
        private static Color Darken(Color baseColor, int amount)
        {
            return Color.FromArgb(
                Math.Max(baseColor.R - amount, 0),
                Math.Max(baseColor.G - amount, 0),
                Math.Max(baseColor.B - amount, 0));
        }

        // Versión del ensamblado (AssemblyVersion), recortando los dos últimos números cuando son
        // cero, para no mostrar algo como "2.0.0.0" cuando basta con "2.0".
        private static string GetDisplayVersion()
        {
            Version version = Assembly.GetExecutingAssembly().GetName().Version;
            return version.Build == 0 && version.Revision == 0 ? string.Format("{0}.{1}", version.Major, version.Minor) : version.ToString();
        }
    }
}