
namespace GuiDesign
{
    partial class OpponentBattleHealth
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
            this.bgImage = new GuiEngine.AssetImage();
            this.NameLabel = new GuiEngine.AssetLabel();
            this.hpPanel = new System.Windows.Forms.Panel();
            this.hpRed = new GuiEngine.AssetImage();
            this.hpOrange = new GuiEngine.AssetImage();
            this.hpGreen = new GuiEngine.AssetImage();
            this.hpFrame = new GuiEngine.AssetImage();
            this.mainPanel = new System.Windows.Forms.Panel();
            this.levelLabel = new GuiEngine.AssetLabel();
            this.hpPanel.SuspendLayout();
            this.mainPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // bgImage
            // 
            this.bgImage.AssetPath = "CommonBattleAssets.OpponentHealthDisplay";
            this.bgImage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bgImage.Location = new System.Drawing.Point(0, 0);
            this.bgImage.Name = "bgImage";
            this.bgImage.Size = new System.Drawing.Size(512, 128);
            this.bgImage.TabIndex = 0;
            // 
            // NameLabel
            // 
            this.NameLabel.AssetPath = "CommonGuiAssets.BaseFont";
            this.NameLabel.BackColor = System.Drawing.Color.DimGray;
            this.NameLabel.ForeColor = System.Drawing.Color.White;
            this.NameLabel.Lines = 1;
            this.NameLabel.Location = new System.Drawing.Point(35, 18);
            this.NameLabel.Name = "NameLabel";
            this.NameLabel.Size = new System.Drawing.Size(264, 53);
            this.NameLabel.TabIndex = 1;
            this.NameLabel.TetxAlignment = System.Drawing.ContentAlignment.TopLeft;
            this.NameLabel.TextContent = "NAME";
            // 
            // hpPanel
            // 
            this.hpPanel.Controls.Add(this.hpRed);
            this.hpPanel.Controls.Add(this.hpOrange);
            this.hpPanel.Controls.Add(this.hpGreen);
            this.hpPanel.Location = new System.Drawing.Point(224, 91);
            this.hpPanel.Name = "hpPanel";
            this.hpPanel.Size = new System.Drawing.Size(192, 10);
            this.hpPanel.TabIndex = 2;
            // 
            // hpRed
            // 
            this.hpRed.AssetPath = "CommonGuiAssets.HealthRed";
            this.hpRed.Location = new System.Drawing.Point(0, 0);
            this.hpRed.Name = "hpRed";
            this.hpRed.Size = new System.Drawing.Size(192, 10);
            this.hpRed.TabIndex = 2;
            // 
            // hpOrange
            // 
            this.hpOrange.AssetPath = "CommonGuiAssets.HealthOrange";
            this.hpOrange.Location = new System.Drawing.Point(0, 0);
            this.hpOrange.Name = "hpOrange";
            this.hpOrange.Size = new System.Drawing.Size(192, 10);
            this.hpOrange.TabIndex = 1;
            // 
            // hpGreen
            // 
            this.hpGreen.AssetPath = "CommonGuiAssets.HealthGreen";
            this.hpGreen.Location = new System.Drawing.Point(0, 0);
            this.hpGreen.Name = "hpGreen";
            this.hpGreen.Size = new System.Drawing.Size(192, 10);
            this.hpGreen.TabIndex = 0;
            // 
            // hpFrame
            // 
            this.hpFrame.AssetPath = "CommonGuiAssets.HealthEmpty";
            this.hpFrame.Location = new System.Drawing.Point(224, 88);
            this.hpFrame.Name = "hpFrame";
            this.hpFrame.Size = new System.Drawing.Size(192, 16);
            this.hpFrame.TabIndex = 0;
            // 
            // mainPanel
            // 
            this.mainPanel.Controls.Add(this.levelLabel);
            this.mainPanel.Controls.Add(this.hpPanel);
            this.mainPanel.Controls.Add(this.hpFrame);
            this.mainPanel.Controls.Add(this.NameLabel);
            this.mainPanel.Controls.Add(this.bgImage);
            this.mainPanel.Location = new System.Drawing.Point(0, 0);
            this.mainPanel.Name = "mainPanel";
            this.mainPanel.Size = new System.Drawing.Size(512, 128);
            this.mainPanel.TabIndex = 0;
            // 
            // levelLabel
            // 
            this.levelLabel.AssetPath = "CommonGuiAssets.SmallFont";
            this.levelLabel.BackColor = System.Drawing.Color.DimGray;
            this.levelLabel.ForeColor = System.Drawing.Color.White;
            this.levelLabel.Lines = 1;
            this.levelLabel.Location = new System.Drawing.Point(327, 31);
            this.levelLabel.Name = "levelLabel";
            this.levelLabel.Size = new System.Drawing.Size(100, 40);
            this.levelLabel.TabIndex = 5;
            this.levelLabel.TetxAlignment = System.Drawing.ContentAlignment.TopLeft;
            this.levelLabel.TextContent = "Lv1";
            // 
            // OpponentBattleHealth
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.mainPanel);
            this.Name = "OpponentBattleHealth";
            this.Size = new System.Drawing.Size(512, 128);
            this.hpPanel.ResumeLayout(false);
            this.mainPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private GuiEngine.AssetImage bgImage;
        private GuiEngine.AssetLabel NameLabel;
        private System.Windows.Forms.Panel hpPanel;
        private GuiEngine.AssetImage hpFrame;
        private System.Windows.Forms.Panel mainPanel;
        private GuiEngine.AssetImage hpGreen;
        private GuiEngine.AssetImage hpRed;
        private GuiEngine.AssetImage hpOrange;
        private GuiEngine.AssetLabel levelLabel;
    }
}
