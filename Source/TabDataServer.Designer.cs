using System.Windows.Forms;

namespace WatchTower
{
    partial class TabDataServer
    {
        private System.ComponentModel.IContainer components = null;
        private Label portLabel;
        private TextBox portTextBox;
        private Label portHelpLabel;
        private Label passwordLabel;
        private TextBox passwordTextBox;
        private Label passwordHelpIcon;
        private Label noteLabel;
        private ToolTip toolTip;
        private System.Windows.Forms.Label pluginMissingLabel;
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
            this.portLabel = new System.Windows.Forms.Label();
            this.portTextBox = new System.Windows.Forms.TextBox();
            this.portHelpLabel = new System.Windows.Forms.Label();
            this.passwordLabel = new System.Windows.Forms.Label();
            this.passwordTextBox = new System.Windows.Forms.TextBox();
            this.passwordHelpIcon = new System.Windows.Forms.Label();
            this.noteLabel = new System.Windows.Forms.Label();
            this.pluginMissingLabel = new System.Windows.Forms.Label();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.SuspendLayout();
            // 
            // portLabel
            // 
            this.portLabel.AutoSize = true;
            this.portLabel.Location = new System.Drawing.Point(50, 60);
            this.portLabel.Name = "portLabel";
            this.portLabel.Size = new System.Drawing.Size(120, 21);
            this.portLabel.TabIndex = 0;
            this.portLabel.UseMnemonic = false;
            // 
            // portTextBox
            // 
            this.portTextBox.Location = new System.Drawing.Point(290, 57);
            this.portTextBox.MaxLength = 5;
            this.portTextBox.Name = "portTextBox";
            this.portTextBox.Size = new System.Drawing.Size(300, 29);
            this.portTextBox.TabIndex = 1;
            this.portTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.PortTextBox_KeyPress);
            // 
            // portHelpLabel
            // 
            this.portHelpLabel.AutoSize = true;
            this.portHelpLabel.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.portHelpLabel.ForeColor = System.Drawing.Color.Black;
            this.portHelpLabel.Location = new System.Drawing.Point(290, 90);
            this.portHelpLabel.Name = "portHelpLabel";
            this.portHelpLabel.Size = new System.Drawing.Size(60, 19);
            this.portHelpLabel.TabIndex = 2;
            this.portHelpLabel.UseMnemonic = false;
            // 
            // passwordLabel
            // 
            this.passwordLabel.AutoSize = true;
            this.passwordLabel.Location = new System.Drawing.Point(50, 140);
            this.passwordLabel.Name = "passwordLabel";
            this.passwordLabel.Size = new System.Drawing.Size(120, 21);
            this.passwordLabel.TabIndex = 3;
            this.passwordLabel.UseMnemonic = false;
            // 
            // passwordTextBox
            // 
            this.passwordTextBox.Location = new System.Drawing.Point(290, 137);
            this.passwordTextBox.MaxLength = 64;
            this.passwordTextBox.Name = "passwordTextBox";
            this.passwordTextBox.Size = new System.Drawing.Size(300, 29);
            this.passwordTextBox.TabIndex = 4;
            // 
            // passwordHelpIcon
            // 
            this.passwordHelpIcon.Cursor = System.Windows.Forms.Cursors.Help;
            this.passwordHelpIcon.Location = new System.Drawing.Point(600, 139);
            this.passwordHelpIcon.Name = "passwordHelpIcon";
            this.passwordHelpIcon.Size = new System.Drawing.Size(26, 26);
            this.passwordHelpIcon.TabIndex = 5;
            this.passwordHelpIcon.Paint += new System.Windows.Forms.PaintEventHandler(this.PasswordHelpIcon_Paint);
            // 
            // noteLabel
            // 
            this.noteLabel.AutoSize = true;
            this.noteLabel.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.noteLabel.ForeColor = System.Drawing.Color.Black;
            this.noteLabel.Location = new System.Drawing.Point(50, 230);
            this.noteLabel.MaximumSize = new System.Drawing.Size(685, 0);
            this.noteLabel.Name = "noteLabel";
            this.noteLabel.Size = new System.Drawing.Size(685, 21);
            this.noteLabel.TabIndex = 6;
            this.noteLabel.UseMnemonic = false;
            // 
            // pluginMissingLabel
            // 
            this.pluginMissingLabel.AutoSize = true;
            this.pluginMissingLabel.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.pluginMissingLabel.ForeColor = System.Drawing.Color.Firebrick;
            this.pluginMissingLabel.Location = new System.Drawing.Point(50, 370);
            this.pluginMissingLabel.MaximumSize = new System.Drawing.Size(685, 0);
            this.pluginMissingLabel.Name = "pluginMissingLabel";
            this.pluginMissingLabel.Size = new System.Drawing.Size(685, 21);
            this.pluginMissingLabel.TabIndex = 7;
            this.pluginMissingLabel.UseMnemonic = false;
            this.pluginMissingLabel.Visible = false;
            // 
            // toolTip
            // 
            this.toolTip.AutoPopDelay = 30000;
            this.toolTip.InitialDelay = 200;
            this.toolTip.ReshowDelay = 100;
            // 
            // TabDataServer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.noteLabel);
            this.Controls.Add(this.passwordHelpIcon);
            this.Controls.Add(this.passwordTextBox);
            this.Controls.Add(this.passwordLabel);
            this.Controls.Add(this.portHelpLabel);
            this.Controls.Add(this.portTextBox);
            this.Controls.Add(this.portLabel);
            this.Controls.Add(this.pluginMissingLabel);
            this.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.Margin = new System.Windows.Forms.Padding(5);
            this.Name = "TabDataServer";
            this.Size = new System.Drawing.Size(784, 534);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion


    }
}