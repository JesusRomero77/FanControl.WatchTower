namespace WatchTower
{
    partial class TabRemoteSensorLogger
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
            this.connectionTable = new System.Windows.Forms.TableLayoutPanel();
            this.pcNameLabel = new System.Windows.Forms.Label();
            this.pcNameTextBox = new System.Windows.Forms.TextBox();
            this.ipLabel = new System.Windows.Forms.Label();
            this.ipAddressBox = new WatchTower.IpAddressBox();
            this.portLabel = new System.Windows.Forms.Label();
            this.portTextBox = new System.Windows.Forms.TextBox();
            this.passwordLabel = new System.Windows.Forms.Label();
            this.passwordTextBox = new System.Windows.Forms.TextBox();
            this.recButton = new System.Windows.Forms.Button();
            this.statusLabel = new System.Windows.Forms.Label();
            this.profileFlowPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.refreshProfileButton = new System.Windows.Forms.Button();
            this.profileLabel = new System.Windows.Forms.Label();
            this.profileNameLabel = new System.Windows.Forms.Label();
            this.profileConfigPanel = new WatchTower.ProfileConfigPanel();
            this.mainToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.connectionTable.SuspendLayout();
            this.profileFlowPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // connectionTable
            // 
            this.connectionTable.AutoSize = true;
            this.connectionTable.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.connectionTable.ColumnCount = 4;
            this.connectionTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.connectionTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.connectionTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.connectionTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.connectionTable.Controls.Add(this.pcNameLabel, 0, 0);
            this.connectionTable.Controls.Add(this.pcNameTextBox, 1, 0);
            this.connectionTable.Controls.Add(this.ipLabel, 2, 0);
            this.connectionTable.Controls.Add(this.ipAddressBox, 3, 0);
            this.connectionTable.Controls.Add(this.portLabel, 0, 1);
            this.connectionTable.Controls.Add(this.portTextBox, 1, 1);
            this.connectionTable.Controls.Add(this.passwordLabel, 2, 1);
            this.connectionTable.Controls.Add(this.passwordTextBox, 3, 1);
            this.connectionTable.Location = new System.Drawing.Point(30, 12);
            this.connectionTable.Name = "connectionTable";
            this.connectionTable.RowCount = 2;
            this.connectionTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.connectionTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.connectionTable.Size = new System.Drawing.Size(690, 76);
            this.connectionTable.TabIndex = 0;
            // 
            // pcNameLabel
            // 
            this.pcNameLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.pcNameLabel.AutoSize = true;
            this.pcNameLabel.Name = "pcNameLabel";
            this.pcNameLabel.UseMnemonic = false;
            // 
            // pcNameTextBox
            // 
            this.pcNameTextBox.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.pcNameTextBox.Margin = new System.Windows.Forms.Padding(3, 4, 24, 4);
            this.pcNameTextBox.Name = "pcNameTextBox";
            this.pcNameTextBox.Size = new System.Drawing.Size(180, 29);
            this.pcNameTextBox.TabIndex = 1;
            // 
            // ipLabel
            // 
            this.ipLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.ipLabel.AutoSize = true;
            this.ipLabel.Name = "ipLabel";
            this.ipLabel.UseMnemonic = false;
            // 
            // ipAddressBox
            // 
            this.ipAddressBox.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.ipAddressBox.BackColor = System.Drawing.SystemColors.Window;
            this.ipAddressBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ipAddressBox.Name = "ipAddressBox";
            this.ipAddressBox.Size = new System.Drawing.Size(180, 31);
            this.ipAddressBox.TabIndex = 2;
            // 
            // portLabel
            // 
            this.portLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.portLabel.AutoSize = true;
            this.portLabel.Name = "portLabel";
            this.portLabel.UseMnemonic = false;
            // 
            // portTextBox
            // 
            this.portTextBox.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.portTextBox.Margin = new System.Windows.Forms.Padding(3, 4, 24, 4);
            this.portTextBox.MaxLength = 5;
            this.portTextBox.Name = "portTextBox";
            this.portTextBox.Size = new System.Drawing.Size(180, 29);
            this.portTextBox.TabIndex = 3;
            this.portTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.PortTextBox_KeyPress);
            // 
            // passwordLabel
            // 
            this.passwordLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.passwordLabel.AutoSize = true;
            this.passwordLabel.Name = "passwordLabel";
            this.passwordLabel.UseMnemonic = false;
            // 
            // passwordTextBox
            // 
            this.passwordTextBox.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.passwordTextBox.MaxLength = 64;
            this.passwordTextBox.Name = "passwordTextBox";
            this.passwordTextBox.Size = new System.Drawing.Size(180, 29);
            this.passwordTextBox.TabIndex = 4;
            // 
            // recButton
            // 
            this.recButton.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.recButton.Location = new System.Drawing.Point(33, 98);
            this.recButton.Name = "recButton";
            this.recButton.Size = new System.Drawing.Size(140, 36);
            this.recButton.TabIndex = 5;
            this.recButton.UseVisualStyleBackColor = true;
            this.recButton.Click += new System.EventHandler(this.RecButton_Click);
            // 
            // statusLabel
            // 
            this.statusLabel.Location = new System.Drawing.Point(190, 92);
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(564, 48);
            this.statusLabel.TabIndex = 6;
            this.statusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.statusLabel.UseMnemonic = false;
            // 
            // profileFlowPanel
            // 
            this.profileFlowPanel.AutoSize = true;
            this.profileFlowPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.profileFlowPanel.Controls.Add(this.refreshProfileButton);
            this.profileFlowPanel.Controls.Add(this.profileLabel);
            this.profileFlowPanel.Controls.Add(this.profileNameLabel);
            this.profileFlowPanel.Location = new System.Drawing.Point(30, 144);
            this.profileFlowPanel.Name = "profileFlowPanel";
            this.profileFlowPanel.Size = new System.Drawing.Size(400, 35);
            this.profileFlowPanel.TabIndex = 7;
            this.profileFlowPanel.WrapContents = false;
            // 
            // refreshProfileButton
            // 
            this.refreshProfileButton.Font = new System.Drawing.Font("Segoe UI Symbol", 12F);
            this.refreshProfileButton.Name = "refreshProfileButton";
            this.refreshProfileButton.Size = new System.Drawing.Size(29, 29);
            this.refreshProfileButton.TabIndex = 0;
            this.refreshProfileButton.Text = "↻";
            this.refreshProfileButton.UseVisualStyleBackColor = true;
            this.refreshProfileButton.Click += new System.EventHandler(this.RefreshProfileButton_Click);
            // 
            // profileLabel
            // 
            this.profileLabel.AutoSize = true;
            this.profileLabel.Margin = new System.Windows.Forms.Padding(8, 5, 3, 0);
            this.profileLabel.Name = "profileLabel";
            this.profileLabel.UseMnemonic = false;
            // 
            // profileNameLabel
            // 
            this.profileNameLabel.AutoSize = true;
            this.profileNameLabel.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.profileNameLabel.Margin = new System.Windows.Forms.Padding(3, 5, 3, 0);
            this.profileNameLabel.Name = "profileNameLabel";
            this.profileNameLabel.Text = "—";
            this.profileNameLabel.UseMnemonic = false;
            // 
            // profileConfigPanel
            // 
            this.profileConfigPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top 
            | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.profileConfigPanel.AutoScroll = true;
            this.profileConfigPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.profileConfigPanel.Location = new System.Drawing.Point(30, 185);
            this.profileConfigPanel.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            this.profileConfigPanel.Name = "profileConfigPanel";
            this.profileConfigPanel.Size = new System.Drawing.Size(724, 330);
            this.profileConfigPanel.TabIndex = 8;
            // 
            // TabRemoteSensorLogger
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.profileConfigPanel);
            this.Controls.Add(this.profileFlowPanel);
            this.Controls.Add(this.statusLabel);
            this.Controls.Add(this.recButton);
            this.Controls.Add(this.connectionTable);
            this.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.Margin = new System.Windows.Forms.Padding(5);
            this.Name = "TabRemoteSensorLogger";
            this.Size = new System.Drawing.Size(784, 534);
            this.connectionTable.ResumeLayout(false);
            this.connectionTable.PerformLayout();
            this.profileFlowPanel.ResumeLayout(false);
            this.profileFlowPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel connectionTable;
        private System.Windows.Forms.Label pcNameLabel;
        private System.Windows.Forms.TextBox pcNameTextBox;
        private System.Windows.Forms.Label ipLabel;
        private IpAddressBox ipAddressBox;
        private System.Windows.Forms.Label portLabel;
        private System.Windows.Forms.TextBox portTextBox;
        private System.Windows.Forms.Label passwordLabel;
        private System.Windows.Forms.TextBox passwordTextBox;
        private System.Windows.Forms.Button recButton;
        private System.Windows.Forms.Label statusLabel;
        private System.Windows.Forms.FlowLayoutPanel profileFlowPanel;
        private System.Windows.Forms.Button refreshProfileButton;
        private System.Windows.Forms.Label profileLabel;
        private System.Windows.Forms.Label profileNameLabel;
        private ProfileConfigPanel profileConfigPanel;
        private System.Windows.Forms.ToolTip mainToolTip;
    }
}