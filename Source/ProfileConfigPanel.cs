using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace WatchTower
{
    /// <summary>
    /// Panel con las filas de configuración de un perfil: una sección de controles, otra opcional de
    /// curvas y otra de sensores. Cada fila tiene la casilla de registro, el nombre, el intervalo y el
    /// umbral de cambio. Lo comparten las pestañas de CurveReader y de RemoteSensorLogger. Los cambios
    /// del usuario se escriben directamente en el ProfileConfig que se pasa a Build, y el panel avisa
    /// con ItemChanged para que la pestaña marque el perfil como modificado.
    /// </summary>
    public class ProfileConfigPanel : Panel
    {
        // Puntos que se resta al tamaño de la fuente de letra del desplegable del umbral respecto a la del formulario.
        private const float RowFontReduction = 1F;

        /// <summary>Si es true, se muestra la sección de curvas con la nota de que aún no se registran.</summary>
        public bool ShowCurvesSection { get; set; }

        /// <summary>Se lanza cuando el usuario cambia la casilla, el intervalo o el umbral de cualquier fila.</summary>
        public event EventHandler ItemChanged;

        // Reconstruye el panel desde cero a partir de 'config' (vacío si es null): una tabla con la
        // sección de controles, la de curvas si está activada y la de sensores, en el orden de FanControl.
        public void Build(ProfileConfig config)
        {
            SuspendLayout();

            // Se eliminan los controles anteriores (al eliminar uno sale solo de la lista).
            while (Controls.Count > 0)
                Controls[0].Dispose();

            if (config != null)
            {
                TableLayoutPanel table = new TableLayoutPanel
                {
                    Dock = DockStyle.Top,
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink,
                    ColumnCount = 4
                };
                table.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));       // casilla de registro
                table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));  // nombre
                table.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));       // intervalo
                table.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));       // umbral

                AddSection(table, Translator.Translate(TextId.Controls), config.Controls);
                if (ShowCurvesSection) AddCurvesSection(table);
                AddSection(table, Translator.Translate(TextId.Sensors), config.Sensors);

                Controls.Add(table);
            }

            ResumeLayout();
        }

        private void OnItemChanged()
        {
            ItemChanged?.Invoke(this, EventArgs.Empty);
        }

        // Añade una sección con filas: la cabecera y una fila por cada control o sensor.
        private void AddSection(TableLayoutPanel table, string title, List<ItemConfig> items)
        {
            AddSectionHeader(table, title);
            foreach (ItemConfig item in items)
                AddItemRow(table, item);
        }

        // Añade la sección de curvas: la misma cabecera que las demás, para dejarla preparada para
        // cuando las curvas se registren, y una nota indicando que de momento no se registran.
        private void AddCurvesSection(TableLayoutPanel table)
        {
            AddSectionHeader(table, Translator.Translate(TextId.Curves));

            Label note = new Label
            {
                Text = Translator.Translate(TextId.CurvesNotRecorded),
                AutoSize = true,
                UseMnemonic = false,
                ForeColor = SystemColors.GrayText
            };
            AddFullWidthRow(table, note);
        }

        // Añade la cabecera de una sección: encabezado de la casilla de registro, título de la sección
        // en negrita, encabezados del intervalo y del umbral, y una línea fina debajo.
        private void AddSectionHeader(TableLayoutPanel table, string title)
        {
            int headerRow = table.RowCount;
            table.RowCount = headerRow + 1;

            Label logHeaderLabel = new Label
            {
                Text = Translator.Translate(TextId.Log),
                AutoSize = true,
                UseMnemonic = false,
                Margin = new Padding(3, 12, 3, 3),
                Anchor = AnchorStyles.Top
            };
            Label titleLabel = new Label
            {
                Text = title,
                Font = new Font(Font, FontStyle.Bold),
                AutoSize = true,
                UseMnemonic = false,
                Margin = new Padding(3, 12, 3, 3)
            };
            Label intervalHeaderLabel = new Label
            {
                Text = Translator.Translate(TextId.Interval),
                AutoSize = true,
                UseMnemonic = false,
                Margin = new Padding(3, 12, 3, 3),
                Anchor = AnchorStyles.Top
            };
            Label thresholdHeaderLabel = new Label
            {
                Text = Translator.Translate(TextId.ChangeThreshold),
                AutoSize = true,
                UseMnemonic = false,
                Margin = new Padding(3, 12, 3, 3),
                Anchor = AnchorStyles.Top
            };
            table.Controls.Add(logHeaderLabel, 0, headerRow);
            table.Controls.Add(titleLabel, 1, headerRow);
            table.Controls.Add(intervalHeaderLabel, 2, headerRow);
            table.Controls.Add(thresholdHeaderLabel, 3, headerRow);

            Label line = new Label { BorderStyle = BorderStyle.Fixed3D, AutoSize = false, Height = 2, Dock = DockStyle.Fill };
            AddFullWidthRow(table, line);
        }

        // Añade la fila de un control o sensor: casilla de registro, nombre, intervalo y umbral.
        // Cada cambio del usuario se escribe directamente en 'item' y avisa con ItemChanged.
        private void AddItemRow(TableLayoutPanel table, ItemConfig item)
        {
            int row = table.RowCount;
            table.RowCount = row + 1;

            // La casilla va sola en su columna; Anchor = None la centra bajo el encabezado.
            CheckBox includeCheckBox = new CheckBox
            {
                Checked = item.IncludedInLog,
                AutoSize = true,
                Anchor = AnchorStyles.None
            };

            // UseMnemonic = false para que un "&" en el nombre se muestre tal cual.
            Label nameLabel = new Label
            {
                Text = item.Name,
                AutoSize = true,
                UseMnemonic = false,
                Anchor = AnchorStyles.Left
            };

            NumericUpDown intervalNumeric = CreateIntervalNumericUpDown(item.IntervalSeconds);
            ComboBox thresholdComboBox = CreateValueComboBox(
                Enumerable.Range(ItemConfig.MinThreshold, ItemConfig.MaxThreshold - ItemConfig.MinThreshold + 1), item.Threshold);

            // Refleja el estado de la casilla en el resto de la fila: si el elemento no se registra,
            // su nombre y el control del intervalo se sombrean y los dos controles se deshabilitan.
            Action updateRowAppearance = () =>
            {
                bool included = includeCheckBox.Checked;
                nameLabel.BackColor = included ? Color.Transparent : SystemColors.ControlDark;
                intervalNumeric.BackColor = included ? SystemColors.Window : SystemColors.ControlDark;
                intervalNumeric.Enabled = included;
                thresholdComboBox.Enabled = included;
            };
            updateRowAppearance();

            // Los manejadores se asignan después de dar el valor inicial a los controles, para que
            // cargar la fila no cuente como un cambio del usuario.
            includeCheckBox.CheckedChanged += (sender, e) =>
            {
                item.IncludedInLog = includeCheckBox.Checked;
                updateRowAppearance();
                OnItemChanged();
            };
            intervalNumeric.ValueChanged += (sender, e) =>
            {
                item.IntervalSeconds = (int)intervalNumeric.Value;
                OnItemChanged();
            };
            thresholdComboBox.SelectedIndexChanged += (sender, e) =>
            {
                item.Threshold = (int)thresholdComboBox.SelectedItem;
                OnItemChanged();
            };

            // Pulsar sobre el nombre también marca o desmarca la casilla, como si fuera su texto.
            nameLabel.Click += (sender, e) => includeCheckBox.Checked = !includeCheckBox.Checked;

            table.Controls.Add(includeCheckBox, 0, row);
            table.Controls.Add(nameLabel, 1, row);
            table.Controls.Add(intervalNumeric, 2, row);
            table.Controls.Add(thresholdComboBox, 3, row);
        }

        // Añade una fila con un único control que ocupa todo el ancho de la tabla.
        private static void AddFullWidthRow(TableLayoutPanel table, Control control)
        {
            int row = table.RowCount;
            table.RowCount = row + 1;
            table.Controls.Add(control, 0, row);
            table.SetColumnSpan(control, table.ColumnCount);
        }

        // Dibuja cada elemento del desplegable del umbral. Si el desplegable está deshabilitado, el
        // fondo es ControlDark, igual que el del nombre del elemento; si no, se usan los colores
        // normales de Windows, incluido el resaltado al pasar el ratón por la lista.
        private void RowComboBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            if (e.Index < 0) return;

            bool enabled = comboBox.Enabled;
            bool highlighted = enabled && (e.State & DrawItemState.Selected) != 0;

            Color backColor = !enabled ? SystemColors.ControlDark : highlighted ? SystemColors.Highlight : SystemColors.Window;
            Color textColor = !enabled ? SystemColors.ControlText : highlighted ? SystemColors.HighlightText : SystemColors.WindowText;

            using (SolidBrush brush = new SolidBrush(backColor))
                e.Graphics.FillRectangle(brush, e.Bounds);

            TextRenderer.DrawText(e.Graphics, comboBox.Items[e.Index].ToString(), e.Font, e.Bounds,
                textColor, TextFormatFlags.Left | TextFormatFlags.VerticalCenter);

            if (enabled) e.DrawFocusRectangle();
        }

        // Crea el control numérico del intervalo, con el mismo estilo de letra que el desplegable:
        // negrita y algo más pequeña que la del formulario.
        private NumericUpDown CreateIntervalNumericUpDown(int value)
        {
            Font numericFont = new Font(Font.FontFamily, Math.Max(Font.Size - RowFontReduction, 6F), FontStyle.Bold);

            // Minimum y Maximum se asignan antes que Value, porque Value debe estar dentro del rango.
            NumericUpDown numeric = new NumericUpDown
            {
                Font = numericFont,
                Minimum = ItemConfig.MinIntervalSeconds,
                Maximum = ItemConfig.MaxIntervalSeconds,
                Value = value,
                Width = 60,
                Anchor = AnchorStyles.None
            };
            numeric.Disposed += (sender, e) => numericFont.Dispose();

            return numeric;
        }

        // Crea un desplegable de fila con los valores indicados y uno seleccionado, con el estilo común:
        // letra en negrita algo más pequeña que la del formulario y fondo ControlDark al deshabilitarse.
        private ComboBox CreateValueComboBox(IEnumerable<int> values, int selectedValue)
        {
            Font comboFont = new Font(Font.FontFamily, Math.Max(Font.Size - RowFontReduction, 6F), FontStyle.Bold);

            ComboBox comboBox = new ComboBox
            {
                Font = comboFont,
                DropDownStyle = ComboBoxStyle.DropDownList,
                DrawMode = DrawMode.OwnerDrawFixed,
                ItemHeight = comboFont.Height + 2,
                Width = 60,
                Anchor = AnchorStyles.None
            };
            comboBox.DrawItem += RowComboBox_DrawItem;
            comboBox.Disposed += (sender, e) => comboFont.Dispose();

            foreach (int value in values)
                comboBox.Items.Add(value);
            comboBox.SelectedItem = selectedValue;

            return comboBox;
        }
    }
}