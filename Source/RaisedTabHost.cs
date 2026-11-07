using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace WatchTower
{
    /// <summary>
    /// Sustituto del TabControl: una fila de pestañas y, debajo, el contenido de la pestaña activa.
    /// La pestaña activa y su contenido forman un único bloque con un marco en relieve (luz arriba y a
    /// la izquierda, sombra abajo y a la derecha), como si estuviera por encima del resto. Las pestañas
    /// inactivas son planas y de color gris. Ninguna pestaña tiene línea a su derecha.
    /// </summary>
    public class RaisedTabHost : Control
    {
        private const int HeaderHeight = 44;  // alto de la franja de pestañas, incluida la parte que sobresale la activa
        private const int Lift = 4;           // cuánto más alta es la pestaña activa que las inactivas
        private const int SideMargin = 6;     // hueco a izquierda, derecha y abajo para que se vea el marco
        private const int TabPaddingX = 24;   // relleno horizontal del texto dentro de cada pestaña
        private const int FrameThickness = 4; // grosor del marco: línea exterior más línea interior

        // Una pestaña: su título, el control que muestra y el rectángulo que ocupa en la franja.
        private sealed class PageEntry
        {
            public string Title;
            public Control Content;
            public Rectangle TabBounds;
        }

        // Un lado del marco en relieve: dónde empieza y acaba, si es de luz o de sombra, y hacia
        // dónde se desplaza su línea interior (siempre hacia dentro del bloque).
        private struct Edge
        {
            public Point From;
            public Point To;
            public bool IsLight;
            public int InnerDx;
            public int InnerDy;

            public Edge(Point from, Point to, bool isLight, int innerDx, int innerDy)
            {
                From = from;
                To = to;
                IsLight = isLight;
                InnerDx = innerDx;
                InnerDy = innerDy;
            }
        }

        private readonly List<PageEntry> _pages = new List<PageEntry>();
        private readonly Panel _contentPanel;
        private readonly Font _tabFont = new Font("Segoe UI", 13F, FontStyle.Bold);
        private int _selectedIndex = -1;

        /// <summary>Se lanza cuando el usuario elige otra pestaña.</summary>
        public event EventHandler SelectedIndexChanged;

        // Dibuja todo el control a mano y sin parpadeos. El panel de contenido, hijo del control,
        // aloja las pestañas y tiene el mismo color que el bloque.
        public RaisedTabHost()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);

            BackColor = SystemColors.Control;
            _contentPanel = new Panel { BackColor = SystemColors.Control };
            Controls.Add(_contentPanel);
        }

        /// <summary>Color de fondo de las pestañas inactivas: un gris algo más oscuro que el del bloque.</summary>
        public Color InactiveTabColor { get; set; } = Color.FromArgb(225, 225, 225);

        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set
            {
                if (value < 0 || value >= _pages.Count || value == _selectedIndex) return;

                if (_selectedIndex >= 0) _pages[_selectedIndex].Content.Visible = false;
                _selectedIndex = value;
                _pages[_selectedIndex].Content.Visible = true;

                Invalidate();
                SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        // Añade una pestaña con su título y el control que se muestra al elegirla. La primera que se
        // añade queda seleccionada.
        public void AddPage(string title, Control content)
        {
            content.Dock = DockStyle.Fill;
            content.Visible = false;
            _contentPanel.Controls.Add(content);
            _pages.Add(new PageEntry { Title = title, Content = content });

            LayoutTabs();
            if (_selectedIndex < 0) SelectedIndex = 0;
            Invalidate();
        }

        // Cambia el título de una pestaña (al cambiar de idioma) y recoloca las pestañas, porque su
        // ancho depende del texto.
        public void SetPageTitle(int index, string title)
        {
            if (index < 0 || index >= _pages.Count) return;

            _pages[index].Title = title;
            LayoutTabs();
            Invalidate();
        }

        // Rectángulo del bloque de contenido, debajo de la franja de pestañas.
        private Rectangle ContentFrame =>
            new Rectangle(SideMargin, HeaderHeight, Width - 2 * SideMargin, Height - HeaderHeight - SideMargin);

        // Reparte las pestañas de izquierda a derecha, sin huecos entre ellas. Cada una mide lo que su
        // texto más el relleno a los lados, y lo que falta hasta el borde derecho del marco se reparte a
        // partes iguales entre todas (los píxeles sobrantes, de uno en uno, a las primeras). Así la última
        // pestaña termina siempre justo donde termina el marco. Si no hay sitio (ventana muy estrecha) no
        // se encogen: se quedan en su ancho natural.
        private void LayoutTabs()
        {
            if (_pages.Count == 0) return;

            int[] naturalWidths = new int[_pages.Count];
            int totalWidth = 0;

            for (int i = 0; i < _pages.Count; i++)
            {
                naturalWidths[i] = TextRenderer.MeasureText(_pages[i].Title ?? "", _tabFont).Width + 2 * TabPaddingX;
                totalWidth += naturalWidths[i];
            }

            int extraWidth = Math.Max(0, ContentFrame.Width - totalWidth);
            int sharePerTab = extraWidth / _pages.Count;
            int remainder = extraWidth % _pages.Count;

            int x = ContentFrame.Left;

            for (int i = 0; i < _pages.Count; i++)
            {
                int width = naturalWidths[i] + sharePerTab + (i < remainder ? 1 : 0);
                _pages[i].TabBounds = new Rectangle(x, 0, width, HeaderHeight);
                x += width;
            }
        }

        // Al cambiar el tamaño se recolocan las pestañas (para que la última llegue al borde del marco) y el
        // panel de contenido dentro del marco, dejando sitio para las dos líneas del relieve.
        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);
            LayoutTabs();
            Rectangle frame = ContentFrame;
            _contentPanel.Bounds = new Rectangle(
                frame.Left + FrameThickness, frame.Top + FrameThickness,
                Math.Max(0, frame.Width - 2 * FrameThickness), Math.Max(0, frame.Height - 2 * FrameThickness));
        }

        // Clic izquierdo sobre una pestaña: la selecciona.
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button != MouseButtons.Left) return;

            for (int i = 0; i < _pages.Count; i++)
                if (_pages[i].TabBounds.Contains(e.Location))
                {
                    SelectedIndex = i;
                    break;
                }
        }

        // Pinta el control: el fondo, las pestañas inactivas (planas), y encima el bloque de la pestaña
        // activa con su contenido, rellenado y con el marco en relieve.
        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(BackColor);
            if (_pages.Count == 0 || _selectedIndex < 0) return;

            for (int i = 0; i < _pages.Count; i++)
                if (i != _selectedIndex) DrawInactiveTab(g, _pages[i]);

            DrawActiveBlock(g, _pages[_selectedIndex]);
        }

        // Pestaña inactiva: un rectángulo gris, más bajo que la activa, con el título centrado.
        private void DrawInactiveTab(Graphics g, PageEntry page)
        {
            Rectangle bounds = new Rectangle(
                page.TabBounds.Left, Lift, page.TabBounds.Width, HeaderHeight - Lift);

            using (SolidBrush brush = new SolidBrush(InactiveTabColor))
                g.FillRectangle(brush, bounds);

            TextRenderer.DrawText(g, page.Title, _tabFont, bounds, ForeColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }

        // Dibuja el bloque de la pestaña activa como una sola pieza: el contorno recorre el borde del
        // contenido, sube por el lado izquierdo de la pestaña, la rodea por arriba y baja por el lado
        // derecho. Se rellena todo con el color del bloque y se pinta el marco en relieve.
        private void DrawActiveBlock(Graphics g, PageEntry activePage)
        {
            Rectangle frame = ContentFrame;
            int contentLeft = frame.Left;
            int contentTop = frame.Top;
            int contentRight = frame.Right - 1;
            int contentBottom = frame.Bottom - 1;

            int tabLeft = activePage.TabBounds.Left;
            int tabRight = activePage.TabBounds.Right - 1;
            int tabTop = 0;

            Point p0 = new Point(contentLeft, contentBottom);
            Point p1 = new Point(contentLeft, contentTop);
            Point p2 = new Point(tabLeft, contentTop);
            Point p3 = new Point(tabLeft, tabTop);
            Point p4 = new Point(tabRight, tabTop);
            Point p5 = new Point(tabRight, contentTop);
            Point p6 = new Point(contentRight, contentTop);
            Point p7 = new Point(contentRight, contentBottom);

            using (SolidBrush brush = new SolidBrush(_contentPanel.BackColor))
                g.FillPolygon(brush, new[] { p0, p1, p2, p3, p4, p5, p6, p7 });

            DrawEdges(g, new List<Edge>
            {
                new Edge(p0, p1, true, 1, 0),    // lado izquierdo del bloque
                new Edge(p1, p2, true, 0, 1),    // borde superior del contenido, a la izquierda de la pestaña
                new Edge(p2, p3, true, 1, 0),    // lado izquierdo de la pestaña
                new Edge(p3, p4, true, 0, 1),    // parte superior de la pestaña
                new Edge(p4, p5, false, -1, 0),  // lado derecho de la pestaña
                new Edge(p5, p6, true, 0, 1),    // borde superior del contenido, a la derecha de la pestaña
                new Edge(p6, p7, false, -1, 0),  // lado derecho del bloque
                new Edge(p7, p0, false, 0, -1)   // parte inferior del bloque
            });

            Rectangle textBounds = new Rectangle(
                activePage.TabBounds.Left, tabTop, activePage.TabBounds.Width, HeaderHeight);
            TextRenderer.DrawText(g, activePage.Title, _tabFont, textBounds, ForeColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }

        // Pinta el relieve capa a capa, de la más interna a la más externa. Cada capa es una línea por cada
        // lado del contorno, desplazada hacia dentro tantos píxeles como indique el número de capa. La mitad
        // exterior del marco usa los colores fuertes (blanco y casi negro) y la mitad interior los suaves.
        // En cada capa se pinta primero la luz y después la sombra; como las capas externas se pintan al
        // final, en las esquinas gana la línea de la capa más externa, y a igualdad, la sombra. Así el
        // encuentro entre luz y sombra queda cortado en diagonal, como en el borde de un botón.
        private static void DrawEdges(Graphics g, List<Edge> edges)
        {
            for (int layer = FrameThickness - 1; layer >= 0; layer--)
            {
                bool isOuterHalf = layer < FrameThickness / 2;
                Color lightColor = isOuterHalf ? SystemColors.ControlLightLight : SystemColors.ControlLight;
                Color darkColor = isOuterHalf ? Color.DarkGray : Color.LightGray;

                foreach (Edge edge in edges)
                    if (edge.IsLight) DrawEdgeLayer(g, edge, layer, lightColor);

                foreach (Edge edge in edges)
                    if (!edge.IsLight) DrawEdgeLayer(g, edge, layer, darkColor);
            }
        }

        // Dibuja una capa de un lado del marco: la línea del lado desplazada hacia dentro del bloque.
        private static void DrawEdgeLayer(Graphics g, Edge edge, int layer, Color color)
        {
            if (edge.From == edge.To) return;

            using (Pen pen = new Pen(color))
                g.DrawLine(pen,
                    edge.From.X + edge.InnerDx * layer, edge.From.Y + edge.InnerDy * layer,
                    edge.To.X + edge.InnerDx * layer, edge.To.Y + edge.InnerDy * layer);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _tabFont.Dispose();
            base.Dispose(disposing);
        }
    }
}