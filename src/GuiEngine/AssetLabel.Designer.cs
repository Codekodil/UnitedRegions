
namespace GuiEngine
{
    partial class AssetLabel
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
            this.previewLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // previewLabel
            // 
            this.previewLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.previewLabel.Location = new System.Drawing.Point(0, 0);
            this.previewLabel.Name = "previewLabel";
            this.previewLabel.Size = new System.Drawing.Size(150, 150);
            this.previewLabel.TabIndex = 0;
            this.previewLabel.Text = "[]";
            // 
            // AssetLabel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.previewLabel);
            this.Name = "AssetLabel";
            this.SizeChanged += new System.EventHandler(this.AssetLabel_SizeChanged);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label previewLabel;
    }
}
