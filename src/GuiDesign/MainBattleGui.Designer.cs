
namespace GuiDesign
{
    partial class MainBattleGui
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.opponentBattleHealth2 = new GuiDesign.OpponentBattleHealth();
            this.opponentBattleHealth1 = new GuiDesign.OpponentBattleHealth();
            this.playerBattleHealth2 = new GuiDesign.PlayerBattleHealth();
            this.playerBattleHealth1 = new GuiDesign.PlayerBattleHealth();
            this.winFrame = new GuiDesign.WinFrame();
            this.SuspendLayout();
            // 
            // opponentBattleHealth2
            // 
            this.opponentBattleHealth2.CurrentHp = 0;
            this.opponentBattleHealth2.Level = 1;
            this.opponentBattleHealth2.Location = new System.Drawing.Point(0, 3);
            this.opponentBattleHealth2.MaxHp = 1;
            this.opponentBattleHealth2.MonsterName = "Opponent2";
            this.opponentBattleHealth2.Name = "opponentBattleHealth2";
            this.opponentBattleHealth2.OffScreen = 1F;
            this.opponentBattleHealth2.Size = new System.Drawing.Size(512, 128);
            this.opponentBattleHealth2.TabIndex = 4;
            // 
            // opponentBattleHealth1
            // 
            this.opponentBattleHealth1.CurrentHp = 0;
            this.opponentBattleHealth1.Level = 1;
            this.opponentBattleHealth1.Location = new System.Drawing.Point(-24, 115);
            this.opponentBattleHealth1.MaxHp = 1;
            this.opponentBattleHealth1.MonsterName = "Opponent1";
            this.opponentBattleHealth1.Name = "opponentBattleHealth1";
            this.opponentBattleHealth1.OffScreen = 1F;
            this.opponentBattleHealth1.Size = new System.Drawing.Size(512, 128);
            this.opponentBattleHealth1.TabIndex = 5;
            // 
            // playerBattleHealth2
            // 
            this.playerBattleHealth2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.playerBattleHealth2.CurrentHp = 25;
            this.playerBattleHealth2.Level = 1;
            this.playerBattleHealth2.Location = new System.Drawing.Point(985, 677);
            this.playerBattleHealth2.MaxHp = 100;
            this.playerBattleHealth2.MonsterName = "Player1";
            this.playerBattleHealth2.Name = "playerBattleHealth2";
            this.playerBattleHealth2.OffScreen = 1F;
            this.playerBattleHealth2.Size = new System.Drawing.Size(512, 160);
            this.playerBattleHealth2.TabIndex = 1;
            // 
            // playerBattleHealth1
            // 
            this.playerBattleHealth1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.playerBattleHealth1.CurrentHp = 40;
            this.playerBattleHealth1.Level = 1;
            this.playerBattleHealth1.Location = new System.Drawing.Point(985, 511);
            this.playerBattleHealth1.MaxHp = 50;
            this.playerBattleHealth1.MonsterName = "Player2";
            this.playerBattleHealth1.Name = "playerBattleHealth1";
            this.playerBattleHealth1.OffScreen = 1F;
            this.playerBattleHealth1.Size = new System.Drawing.Size(512, 160);
            this.playerBattleHealth1.TabIndex = 0;
            // 
            // winFrame
            // 
            this.winFrame.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.winFrame.Location = new System.Drawing.Point(0, 843);
            this.winFrame.Name = "winFrame";
            this.winFrame.OffScreen = 1F;
            this.winFrame.Size = new System.Drawing.Size(1497, 208);
            this.winFrame.TabIndex = 6;
            this.winFrame.TetxAlignment = System.Drawing.ContentAlignment.TopLeft;
            this.winFrame.TetxLines = 2;
            this.winFrame.TextContent = "";
            // 
            // MainBattleGui
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.winFrame);
            this.Controls.Add(this.opponentBattleHealth2);
            this.Controls.Add(this.opponentBattleHealth1);
            this.Controls.Add(this.playerBattleHealth2);
            this.Controls.Add(this.playerBattleHealth1);
            this.Name = "MainBattleGui";
            this.Size = new System.Drawing.Size(1497, 1051);
            this.ResumeLayout(false);

        }

        #endregion

        private PlayerBattleHealth playerBattleHealth1;
        private PlayerBattleHealth playerBattleHealth2;
        private OpponentBattleHealth opponentBattleHealth2;
        private OpponentBattleHealth opponentBattleHealth1;
        private WinFrame winFrame;
    }
}
