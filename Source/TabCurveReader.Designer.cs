namespace WatchTower
{
    partial class TabCurveReader
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de componentes

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.profileLabel = new System.Windows.Forms.Label();
            this.profilesComboBox = new System.Windows.Forms.ComboBox();
            this.refreshProfilesButton = new System.Windows.Forms.Button();
            this.profileConfigPanel = new WatchTower.ProfileConfigPanel();
            this.mainToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.SuspendLayout();
            // 
            // profileLabel
            // 
            this.profileLabel.AutoSize = true;
            this.profileLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.profileLabel.Location = new System.Drawing.Point(59, 30);
            this.profileLabel.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.profileLabel.Name = "profileLabel";
            this.profileLabel.Size = new System.Drawing.Size(62, 26);
            this.profileLabel.TabIndex = 0;
            this.profileLabel.Text = "Perfil";
            // 
            // profilesComboBox
            // 
            this.profilesComboBox.BackColor = System.Drawing.SystemColors.Window;
            this.profilesComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.profilesComboBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.profilesComboBox.ForeColor = System.Drawing.SystemColors.WindowText;
            this.profilesComboBox.FormattingEnabled = true;
            this.profilesComboBox.Location = new System.Drawing.Point(140, 30);
            this.profilesComboBox.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            this.profilesComboBox.Name = "profilesComboBox";
            this.profilesComboBox.Size = new System.Drawing.Size(230, 29);
            this.profilesComboBox.TabIndex = 1;
            this.profilesComboBox.SelectedIndexChanged += new System.EventHandler(this.ProfilesComboBox_SelectedIndexChanged);
            // 
            // refreshProfilesButton
            // 
            this.refreshProfilesButton.Font = new System.Drawing.Font("Segoe UI Symbol", 12F);
            this.refreshProfilesButton.Location = new System.Drawing.Point(390, 30);
            this.refreshProfilesButton.Name = "refreshProfilesButton";
            this.refreshProfilesButton.Size = new System.Drawing.Size(29, 29);
            this.refreshProfilesButton.TabIndex = 2;
            this.refreshProfilesButton.Text = "↻";
            this.refreshProfilesButton.UseVisualStyleBackColor = true;
            this.refreshProfilesButton.Click += new System.EventHandler(this.RefreshProfilesButton_Click);
            // 
            // profileConfigPanel
            // 
            this.profileConfigPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.profileConfigPanel.AutoScroll = true;
            this.profileConfigPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.profileConfigPanel.Location = new System.Drawing.Point(50, 98);
            this.profileConfigPanel.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            this.profileConfigPanel.Name = "profileConfigPanel";
            this.profileConfigPanel.Size = new System.Drawing.Size(685, 400);
            this.profileConfigPanel.TabIndex = 3;
            // 
            // TabCurveReader
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.profileConfigPanel);
            this.Controls.Add(this.refreshProfilesButton);
            this.Controls.Add(this.profilesComboBox);
            this.Controls.Add(this.profileLabel);
            this.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.Margin = new System.Windows.Forms.Padding(5);
            this.Name = "TabCurveReader";
            this.Size = new System.Drawing.Size(784, 534);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label profileLabel;
        private System.Windows.Forms.ComboBox profilesComboBox;
        private System.Windows.Forms.Button refreshProfilesButton;
        private WatchTower.ProfileConfigPanel profileConfigPanel;
        private System.Windows.Forms.ToolTip mainToolTip;
    }
}