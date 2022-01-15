using System;
using System.Windows.Forms;

namespace GuiDesign
{
    public partial class MainBattleGui : UserControl
    {
        public IHealthDisplay PlayerHealthDisplay1 => playerBattleHealth1;
        public IHealthDisplay PlayerHealthDisplay2 => playerBattleHealth2;
        public IHealthDisplay OpponentHealthDisplay1 => opponentBattleHealth1;
        public IHealthDisplay OpponentHealthDisplay2 => opponentBattleHealth2;

        public void SetInfo(string info)
        {
            Invoke(new Action(() => winFrame.TextContent = info));
        }

        public void SetOffScreen(float offScreen)
        {
            Invoke(new Action(() => winFrame.OffScreen = offScreen));
        }

        public MainBattleGui()
        {
            InitializeComponent();
        }
    }
}
