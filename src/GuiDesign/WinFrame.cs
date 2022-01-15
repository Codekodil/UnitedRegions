using System;
using System.Drawing;
using System.Windows.Forms;

namespace GuiDesign
{
    public partial class WinFrame : UserControl
    {
        public string TextContent
        {
            get => label.TextContent ?? "";
            set => label.TextContent = value;
        }
        public ContentAlignment TetxAlignment
        {
            get => label.TetxAlignment;
            set => label.TetxAlignment = value;
        }
        public int TetxLines
        {
            get => label.Lines;
            set => label.Lines = value;
        }


        private float _offScreen;
        public float OffScreen
        {
            get => _offScreen;
            set
            {
                _offScreen = Math.Min(1f, Math.Max(0f, value));
#if DEBUG
                if (DesignMode) return;
#endif
                offsetPanel.Location = new Point(0, (int)Math.Round(Height * _offScreen));
            }
        }
        public WinFrame()
        {
            InitializeComponent();
        }
    }
}
