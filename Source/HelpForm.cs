using System;
using System.Drawing;
using System.Windows.Forms;

namespace WatchTower
{
    /// <summary>
    /// Ventana de ayuda: una serie de secciones (título en negrita + texto explicativo) con el
    /// funcionamiento básico del programa, dentro de un recuadro con fondo algo más oscuro que
    /// el resto de la ventana. Si las secciones no caben, el recuadro tiene barra de desplazamiento.
    /// Se muestra siempre en el idioma que tenga activo el formulario principal en ese momento.
    /// </summary>
    public class HelpForm : Form
    {
        // Cuánto se oscurece el fondo de la ventana para el recuadro del texto de ayuda.
        // Mismo valor que usa el recuadro de la descripción en AboutForm.
        private const int ContentPanelDarkening = 15;

        // Cuántos puntos se suma al tamaño de letra del formulario para el texto de ayuda.
        private const float HelpFontIncrease = 2F;

        // Altura máxima de la zona visible del recuadro; si el texto es más alto, se desplaza.
        private const int MaxViewportHeight = 460;

        // Separación vertical entre una sección y la siguiente.
        private const int SectionSpacing = 15;

        // Secciones de la ayuda en el orden en que se muestran: título y texto de cada una.
        private static readonly TextId[][] _sections =
        {
            new[] { TextId.HelpRequirementsTitle, TextId.HelpRequirementsText },
            new[] { TextId.HelpGettingStartedTitle, TextId.HelpGettingStartedText },
            new[] { TextId.HelpTabsTitle, TextId.HelpTabsText },
            new[] { TextId.HelpSelectProfileTitle, TextId.HelpSelectProfileText },
            new[] { TextId.HelpConfigureItemsTitle, TextId.HelpConfigureItemsText },
            new[] { TextId.HelpNetworkTitle, TextId.HelpNetworkText },
            new[] { TextId.HelpRecorderTitle, TextId.HelpRecorderText },
            new[] { TextId.HelpSaveTitle, TextId.HelpSaveText }
        };

        public HelpForm()
        {
            BuildLayout();
        }

        // Construye el recuadro con las secciones, dentro de una zona con desplazamiento, y debajo
        // el botón OK. Cada control se añade al formulario (o al panel) justo después de crearlo y
        // antes de usar su tamaño para colocar el siguiente: un control con AutoSize no tiene su
        // tamaño real hasta que está en un contenedor.
        private void BuildLayout()
        {
            Text = Translator.Translate(TextId.HelpWindowTitle);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Icon = Properties.Resources.WatchTower;

            const int margin = 20;
            const int contentWidth = 420;

            Panel contentPanel = BuildContentPanel(contentWidth);

            // La zona visible reserva el ancho de la barra de desplazamiento solo si hace falta.
            bool needsScroll = contentPanel.Height > MaxViewportHeight;
            ScrollPanel viewport = new ScrollPanel
            {
                AutoScroll = true,
                BackColor = contentPanel.BackColor,
                Location = new Point(margin, margin),
                Size = new Size(
                    contentPanel.Width + (needsScroll ? SystemInformation.VerticalScrollBarWidth : 0),
                    Math.Min(contentPanel.Height, MaxViewportHeight))
            };
            Controls.Add(viewport);
            viewport.Controls.Add(contentPanel);

            // Con el foco en la zona visible, la rueda del ratón desplaza el texto.
            viewport.MouseEnter += (sender, e) => viewport.Focus();
            Shown += (sender, e) => viewport.Focus();

            Button okButton = new Button
            {
                Text = Translator.Translate(TextId.Ok),
                AutoSize = true,
                DialogResult = DialogResult.OK
            };
            Controls.Add(okButton);
            okButton.Location = new Point(viewport.Right - okButton.Width, viewport.Bottom + 20);

            AcceptButton = okButton;
            ClientSize = new Size(viewport.Right + margin, okButton.Bottom + margin);
        }

        // Crea el recuadro con todas las secciones dentro, con fondo algo más oscuro que el de
        // la ventana. El ancho es fijo (igual que el recuadro de AboutForm), y el alto se calcula
        // a partir de dónde termina la última sección.
        private Panel BuildContentPanel(int textWidth)
        {
            const int padding = 10;

            Panel panel = new Panel
            {
                BackColor = Darken(BackColor, ContentPanelDarkening),
                Location = new Point(0, 0)
            };

            Font titleFont = new Font(Font.FontFamily, Font.Size + HelpFontIncrease, FontStyle.Bold);
            Font bodyFont = new Font(Font.FontFamily, Font.Size + HelpFontIncrease);
            panel.Disposed += (sender, e) => { titleFont.Dispose(); bodyFont.Dispose(); };

            // Cada sección se coloca debajo de la anterior.
            int sectionTop = padding;
            int lastBottom = padding;
            foreach (TextId[] section in _sections)
            {
                lastBottom = AddSection(panel, section[0], section[1], titleFont, bodyFont, padding, sectionTop, textWidth);
                sectionTop = lastBottom + SectionSpacing;
            }

            panel.Size = new Size(textWidth + padding * 2, lastBottom + padding);
            return panel;
        }

        // Añade una sección (título en negrita + texto normal debajo) dentro del panel indicado
        // y devuelve la posición Y de su borde inferior, para poder colocar la siguiente sección.
        private int AddSection(Panel panel, TextId titleId, TextId textId, Font titleFont, Font bodyFont,
            int left, int top, int width)
        {
            Label titleLabel = new Label
            {
                Text = Translator.Translate(titleId),
                Font = titleFont,
                AutoSize = true,
                UseMnemonic = false,
                Location = new Point(left, top)
            };
            panel.Controls.Add(titleLabel);

            Label bodyLabel = new Label
            {
                Text = Translator.Translate(textId),
                Font = bodyFont,
                AutoSize = true,
                UseMnemonic = false,
                MaximumSize = new Size(width, 0),
                Location = new Point(left, titleLabel.Bottom + 5)
            };
            panel.Controls.Add(bodyLabel);

            return bodyLabel.Bottom;
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

        // Panel con barra de desplazamiento que puede recibir el foco; un Panel normal no puede,
        // y sin foco la rueda del ratón no lo desplaza.
        private sealed class ScrollPanel : Panel
        {
            public ScrollPanel()
            {
                SetStyle(ControlStyles.Selectable, true);
                TabStop = true;
            }
        }
    }
}