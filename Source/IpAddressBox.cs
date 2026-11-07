using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace WatchTower
{
    /// <summary>
    /// Casilla para una dirección IPv4: cuatro campos numéricos de 0 a 255 separados por puntos.
    /// Puede quedar vacía (la IP no siempre es obligatoria) y indica si está vacía, completa o a medias.
    /// </summary>
    public class IpAddressBox : UserControl
    {
        private const int SegmentCount = 4;
        private const int MaxSegmentValue = 255;
        private const int MaxSegmentDigits = 3;

        // Longitud máxima del texto de un campo. Es mayor que tres solo para poder pegar una IP
        // completa ("255.255.255.255"); al escribir a mano ya se limita a tres cifras.
        private const int MaxPastedLength = 15;

        private readonly TextBox[] _segments = new TextBox[SegmentCount];

        // True mientras el código (no el usuario) escribe en los campos, para que TextChanged no lo
        // tome por una escritura del usuario ni salte de campo.
        private bool _updating;

        // Construye la casilla: un borde con fondo de cuadro de texto y, dentro, cuatro campos sin
        // borde separados por puntos. Los campos se reparten el ancho a partes iguales.
        public IpAddressBox()
        {
            BorderStyle = BorderStyle.FixedSingle;
            BackColor = SystemColors.Window;

            TableLayoutPanel layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = SegmentCount * 2 - 1,
                RowCount = 1
            };
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            for (int i = 0; i < SegmentCount; i++)
            {
                layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F / SegmentCount));
                _segments[i] = CreateSegment(i);
                layout.Controls.Add(_segments[i], i * 2, 0);

                if (i < SegmentCount - 1)
                {
                    layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
                    Label dot = new Label
                    {
                        Text = ".",
                        AutoSize = true,
                        UseMnemonic = false,
                        Anchor = AnchorStyles.None,
                        Margin = Padding.Empty
                    };
                    layout.Controls.Add(dot, i * 2 + 1, 0);
                }
            }

            Controls.Add(layout);
            Size = new Size(190, _segments[0].PreferredHeight + 6);
        }

        /// <summary>Ningún campo tiene nada escrito.</summary>
        public bool IsEmpty => _segments.All(segment => segment.TextLength == 0);

        /// <summary>Los cuatro campos tienen un número.</summary>
        public bool IsComplete => _segments.All(segment => segment.TextLength > 0);

        /// <summary>
        /// La IP como texto ("192.168.1.5", sin ceros a la izquierda) si está completa, o "" si está
        /// vacía o a medias. Al asignarla, una IP IPv4 válida se muestra en los cuatro campos y
        /// cualquier otro valor (vacío, texto que no es una IP...) deja la casilla vacía.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Value
        {
            get
            {
                if (!IsComplete) return "";

                return string.Join(".", _segments.Select(segment =>
                    int.Parse(segment.Text, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture)));
            }
            set
            {
                int[] parts;
                if (!string.IsNullOrWhiteSpace(value) && TryParseAddress(value, out parts))
                    SetSegments(parts);
                else
                    SetSegments(null);
            }
        }

        // Crea uno de los cuatro campos: sin borde, con el texto centrado y con los manejadores que
        // limitan lo que se puede escribir y mueven el cursor entre campos.
        private TextBox CreateSegment(int index)
        {
            TextBox segment = new TextBox
            {
                BorderStyle = BorderStyle.None,
                BackColor = SystemColors.Window,
                TextAlign = HorizontalAlignment.Center,
                MaxLength = MaxPastedLength,
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                Margin = new Padding(2, 0, 2, 0),
                Tag = index
            };
            segment.KeyPress += Segment_KeyPress;
            segment.KeyDown += Segment_KeyDown;
            segment.TextChanged += Segment_TextChanged;

            // Al entrar en un campo se selecciona su contenido, para poder sustituirlo escribiendo.
            segment.Enter += (sender, e) => ((TextBox)sender).SelectAll();

            return segment;
        }

        // Teclas escritas en un campo: solo se admiten dígitos (hasta tres por campo, contando con que
        // el texto seleccionado se sustituye) y el punto, que no se escribe sino que salta al
        // campo siguiente. Las teclas de control (retroceso, Ctrl+V...) se dejan pasar.
        private void Segment_KeyPress(object sender, KeyPressEventArgs e)
        {
            TextBox segment = (TextBox)sender;
            int index = (int)segment.Tag;

            if (char.IsControl(e.KeyChar)) return;

            if (e.KeyChar == '.')
            {
                e.Handled = true;
                if (segment.TextLength > 0) FocusSegment(index + 1);
                return;
            }

            bool isDigit = e.KeyChar >= '0' && e.KeyChar <= '9';
            bool fieldIsFull = segment.TextLength - segment.SelectionLength >= MaxSegmentDigits;
            if (!isDigit || fieldIsFull) e.Handled = true;
        }

        // Teclas de movimiento: retroceso o flecha izquierda al principio del campo llevan al campo
        // anterior, y la flecha derecha al final del campo lleva al siguiente.
        private void Segment_KeyDown(object sender, KeyEventArgs e)
        {
            TextBox segment = (TextBox)sender;
            int index = (int)segment.Tag;
            bool noSelection = segment.SelectionLength == 0;

            bool goBack = (e.KeyCode == Keys.Back || e.KeyCode == Keys.Left)
                          && noSelection && segment.SelectionStart == 0 && index > 0;
            bool goForward = e.KeyCode == Keys.Right
                             && noSelection && segment.SelectionStart == segment.TextLength && index < SegmentCount - 1;

            if (goBack) FocusSegment(index - 1);
            else if (goForward) FocusSegment(index + 1);
            else return;

            e.Handled = true;
            e.SuppressKeyPress = true;
        }

        // Cambio de texto en un campo. Si se ha pegado una IP completa, se reparte entre los cuatro
        // campos. Si no, se descarta lo que no sea un dígito, se limita a tres cifras y a 255, y al
        // completar el campo el cursor salta solo al siguiente.
        private void Segment_TextChanged(object sender, EventArgs e)
        {
            if (_updating) return;

            TextBox segment = (TextBox)sender;
            int index = (int)segment.Tag;
            string text = segment.Text;

            int[] pastedParts;
            if (text.Contains(".") && TryParseAddress(text, out pastedParts))
            {
                SetSegments(pastedParts);
                FocusSegment(SegmentCount - 1);
                return;
            }

            string cleaned = new string(text.Where(character => character >= '0' && character <= '9').ToArray());
            if (cleaned.Length > MaxSegmentDigits) cleaned = cleaned.Substring(0, MaxSegmentDigits);
            if (cleaned.Length > 0 && int.Parse(cleaned, CultureInfo.InvariantCulture) > MaxSegmentValue)
                cleaned = MaxSegmentValue.ToString(CultureInfo.InvariantCulture);

            if (cleaned != text) SetSegmentText(segment, cleaned);

            // Con tres cifras, o con dos que ya pasan de 25 (una tercera superaría 255), el campo está lleno.
            bool isFull = cleaned.Length == MaxSegmentDigits
                          || (cleaned.Length == 2 && int.Parse(cleaned, CultureInfo.InvariantCulture) > 25);
            if (isFull && segment.Focused && segment.SelectionStart == segment.TextLength && index < SegmentCount - 1)
                FocusSegment(index + 1);
        }

        // Da el foco al campo indicado, si existe.
        private void FocusSegment(int index)
        {
            if (index >= 0 && index < SegmentCount) _segments[index].Focus();
        }

        // Cambia el texto de un campo desde el código, dejando el cursor al final.
        private void SetSegmentText(TextBox segment, string text)
        {
            _updating = true;
            try
            {
                segment.Text = text;
                segment.SelectionStart = segment.TextLength;
            }
            finally
            {
                _updating = false;
            }
        }

        // Rellena los cuatro campos con los números indicados, o los vacía si se pasa null.
        private void SetSegments(int[] parts)
        {
            _updating = true;
            try
            {
                for (int i = 0; i < SegmentCount; i++)
                    _segments[i].Text = parts == null ? "" : parts[i].ToString(CultureInfo.InvariantCulture);
            }
            finally
            {
                _updating = false;
            }
        }

        // Convierte un texto "a.b.c.d" en sus cuatro números. Cada parte debe tener de una a tres cifras
        // y valer de 0 a 255; si algo no cumple, devuelve false.
        private static bool TryParseAddress(string text, out int[] parts)
        {
            parts = new int[SegmentCount];
            string[] pieces = text.Trim().Split('.');
            if (pieces.Length != SegmentCount) return false;

            for (int i = 0; i < SegmentCount; i++)
            {
                if (pieces[i].Length == 0 || pieces[i].Length > MaxSegmentDigits) return false;
                if (!int.TryParse(pieces[i], NumberStyles.None, CultureInfo.InvariantCulture, out parts[i])) return false;
                if (parts[i] > MaxSegmentValue) return false;
            }

            return true;
        }
    }
}