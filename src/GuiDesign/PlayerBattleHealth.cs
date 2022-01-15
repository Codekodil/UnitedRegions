using GuiEngine;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace GuiDesign
{
    public partial class PlayerBattleHealth : UserControl, IHealthDisplay
    {
        public string MonsterName
        {
            get => NameLabel.TextContent;
            set => NameLabel.TextContent = value;
        }
        private int _maxHp = 1;
        public int MaxHp
        {
            get => _maxHp;
            set
            {
                _maxHp = Math.Max(1, value);
                hpMaxLabel.TextContent = _maxHp.ToString();
                MoveHealthbar();
            }
        }
        private int _currentHp = 0;
        public int CurrentHp
        {
            get => _currentHp;
            set
            {
                _currentHp = Math.Max(0, value);
                hpLabel.TextContent = _currentHp.ToString();
                MoveHealthbar();
            }
        }
        private int _level = 0;
        public int Level
        {
            get => _level;
            set
            {
                _level = Math.Max(1, value);
                levelLabel.TextContent = $"Lv{_level}";
            }
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
                mainPanel.Location = new Point((int)Math.Round(Width * _offScreen), 0);
            }
        }
        private void MoveHealthbar()
        {
            this.TryInvoke(Do);
            void Do()
            {
                var ratio = (float)CurrentHp / MaxHp;
                var size = new Size(Math.Min((hpPanel.Size.Width * CurrentHp) / MaxHp, hpPanel.Size.Width), hpPanel.Size.Height);
                hpGreen.Size = size;
                hpOrange.Size = size;
                hpRed.Size = size;
                hpGreen.Visible = ratio >= .5f;
                hpOrange.Visible = !hpGreen.Visible && ratio >= .2f;
                hpRed.Visible = !hpGreen.Visible && !hpOrange.Visible;
            }
        }
        public PlayerBattleHealth()
        {
            InitializeComponent();
            CurrentHp = CurrentHp;
            MaxHp = MaxHp;
        }
    }
}
