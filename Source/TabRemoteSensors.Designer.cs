namespace WatchTower
{
    partial class TabRemoteSensors
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
            this.warningLabel = new System.Windows.Forms.Label();
            this.pcNameLabel = new System.Windows.Forms.Label();
            this.pcNameTextBox = new System.Windows.Forms.TextBox();
            this.pcNameHelpLabel = new System.Windows.Forms.Label();
            this.ipLabel = new System.Windows.Forms.Label();
            this.ipAddressBox = new WatchTower.IpAddressBox();
            this.ipHelpLabel = new System.Windows.Forms.Label();
            this.portLabel = new System.Windows.Forms.Label();
            this.portTextBox = new System.Windows.Forms.TextBox();
            this.portHelpLabel = new System.Windows.Forms.Label();
            this.passwordLabel = new System.Windows.Forms.Label();
            this.passwordTextBox = new System.Windows.Forms.TextBox();
            this.noteLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // warningLabel
            // 
            this.warningLabel.AutoSize = true;
            this.warningLabel.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.warningLabel.ForeColor = System.Drawing.Color.Firebrick;
            this.warningLabel.Location = new System.Drawing.Point(50, 25);
            this.warningLabel.MaximumSize = new System.Drawing.Size(685, 0);
            this.warningLabel.Name = "warningLabel";
            this.warningLabel.Size = new System.Drawing.Size(685, 21);
            this.warningLabel.TabIndex = 0;
            this.warningLabel.UseMnemonic = false;
            // 
            // pcNameLabel
            // 
            this.pcNameLabel.AutoSize = true;
            this.pcNameLabel.Location = new System.Drawing.Point(50, 130);
            this.pcNameLabel.Name = "pcNameLabel";
            this.pcNameLabel.Size = new System.Drawing.Size(120, 21);
            this.pcNameLabel.TabIndex = 1;
            this.pcNameLabel.UseMnemonic = false;
            // 
            // pcNameTextBox
            // 
            this.pcNameTextBox.Location = new System.Drawing.Point(290, 127);
            this.pcNameTextBox.Name = "pcNameTextBox";
            this.pcNameTextBox.Size = new System.Drawing.Size(300, 29);
            this.pcNameTextBox.TabIndex = 2;
            // 
            // pcNameHelpLabel
            // 
            this.pcNameHelpLabel.AutoSize = true;
            this.pcNameHelpLabel.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.pcNameHelpLabel.ForeColor = System.Drawing.Color.Black;
            this.pcNameHelpLabel.Location = new System.Drawing.Point(290, 160);
            this.pcNameHelpLabel.Name = "pcNameHelpLabel";
            this.pcNameHelpLabel.Size = new System.Drawing.Size(60, 19);
            this.pcNameHelpLabel.TabIndex = 3;
            this.pcNameHelpLabel.UseMnemonic = false;
            // 
            // ipLabel
            // 
            this.ipLabel.AutoSize = true;
            this.ipLabel.Location = new System.Drawing.Point(50, 190);
            this.ipLabel.Name = "ipLabel";
            this.ipLabel.Size = new System.Drawing.Size(120, 21);
            this.ipLabel.TabIndex = 4;
            this.ipLabel.UseMnemonic = false;
            // 
            // ipAddressBox
            // 
            this.ipAddressBox.BackColor = System.Drawing.SystemColors.Window;
            this.ipAddressBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ipAddressBox.Location = new System.Drawing.Point(290, 187);
            this.ipAddressBox.Name = "ipAddressBox";
            this.ipAddressBox.Size = new System.Drawing.Size(300, 31);
            this.ipAddressBox.TabIndex = 5;
            // 
            // ipHelpLabel
            // 
            this.ipHelpLabel.AutoSize = true;
            this.ipHelpLabel.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.ipHelpLabel.ForeColor = System.Drawing.Color.Black;
            this.ipHelpLabel.Location = new System.Drawing.Point(290, 222);
            this.ipHelpLabel.Name = "ipHelpLabel";
            this.ipHelpLabel.Size = new System.Drawing.Size(60, 19);
            this.ipHelpLabel.TabIndex = 6;
            this.ipHelpLabel.UseMnemonic = false;
            // 
            // portLabel
            // 
            this.portLabel.AutoSize = true;
            this.portLabel.Location = new System.Drawing.Point(50, 250);
            this.portLabel.Name = "portLabel";
            this.portLabel.Size = new System.Drawing.Size(120, 21);
            this.portLabel.TabIndex = 7;
            this.portLabel.UseMnemonic = false;
            // 
            // portTextBox
            // 
            this.portTextBox.Location = new System.Drawing.Point(290, 247);
            this.portTextBox.MaxLength = 5;
            this.portTextBox.Name = "portTextBox";
            this.portTextBox.Size = new System.Drawing.Size(300, 29);
            this.portTextBox.TabIndex = 8;
            this.portTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.PortTextBox_KeyPress);
            // 
            // portHelpLabel
            // 
            this.portHelpLabel.AutoSize = true;
            this.portHelpLabel.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.portHelpLabel.ForeColor = System.Drawing.Color.Black;
            this.portHelpLabel.Location = new System.Drawing.Point(290, 282);
            this.portHelpLabel.Name = "portHelpLabel";
            this.portHelpLabel.Size = new System.Drawing.Size(60, 19);
            this.portHelpLabel.TabIndex = 9;
            this.portHelpLabel.UseMnemonic = false;
            // 
            // passwordLabel
            // 
            this.passwordLabel.AutoSize = true;
            this.passwordLabel.Location = new System.Drawing.Point(50, 333);
            this.passwordLabel.Name = "passwordLabel";
            this.passwordLabel.Size = new System.Drawing.Size(120, 21);
            this.passwordLabel.TabIndex = 10;
            this.passwordLabel.UseMnemonic = false;
            // 
            // passwordTextBox
            // 
            this.passwordTextBox.Location = new System.Drawing.Point(290, 330);
            this.passwordTextBox.MaxLength = 64;
            this.passwordTextBox.Name = "passwordTextBox";
            this.passwordTextBox.Size = new System.Drawing.Size(300, 29);
            this.passwordTextBox.TabIndex = 11;
            // 
            // noteLabel
            // 
            this.noteLabel.AutoSize = true;
            this.noteLabel.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.noteLabel.ForeColor = System.Drawing.Color.Black;
            this.noteLabel.Location = new System.Drawing.Point(50, 380);
            this.noteLabel.MaximumSize = new System.Drawing.Size(685, 0);
            this.noteLabel.Name = "noteLabel";
            this.noteLabel.Size = new System.Drawing.Size(685, 21);
            this.noteLabel.TabIndex = 12;
            this.noteLabel.UseMnemonic = false;
            // 
            // TabRemoteSensors
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.noteLabel);
            this.Controls.Add(this.ipHelpLabel);
            this.Controls.Add(this.ipAddressBox);
            this.Controls.Add(this.ipLabel);
            this.Controls.Add(this.pcNameHelpLabel);
            this.Controls.Add(this.pcNameTextBox);
            this.Controls.Add(this.pcNameLabel);
            this.Controls.Add(this.portHelpLabel);
            this.Controls.Add(this.portTextBox);
            this.Controls.Add(this.portLabel);
            this.Controls.Add(this.warningLabel);
            this.Controls.Add(this.passwordTextBox);
            this.Controls.Add(this.passwordLabel);
            this.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.Margin = new System.Windows.Forms.Padding(5);
            this.Name = "TabRemoteSensors";
            this.Size = new System.Drawing.Size(784, 534);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label warningLabel;
        private System.Windows.Forms.Label pcNameLabel;
        private System.Windows.Forms.TextBox pcNameTextBox;
        private System.Windows.Forms.Label pcNameHelpLabel;
        private System.Windows.Forms.Label ipLabel;
        private IpAddressBox ipAddressBox;
        private System.Windows.Forms.Label ipHelpLabel;
        private System.Windows.Forms.Label portLabel;
        private System.Windows.Forms.TextBox portTextBox;
        private System.Windows.Forms.Label portHelpLabel;
        private System.Windows.Forms.Label passwordLabel;
        private System.Windows.Forms.TextBox passwordTextBox;
        private System.Windows.Forms.Label noteLabel;

    }
}