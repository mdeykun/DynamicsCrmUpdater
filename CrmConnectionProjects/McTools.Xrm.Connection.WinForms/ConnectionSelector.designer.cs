namespace McTools.Xrm.Connection.WinForms
{
    partial class ConnectionSelector
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConnectionSelector));
            this.bValidate = new System.Windows.Forms.Button();
            this.bCancel = new System.Windows.Forms.Button();
            this.lvConnections = new System.Windows.Forms.ListView();
            this.chName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chServer = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chOrganization = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chUser = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chVersion = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chSolution = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.menu = new System.Windows.Forms.ToolStrip();
            this.tsbNewConnection = new System.Windows.Forms.ToolStripButton();
            this.tsbUpdateConnection = new System.Windows.Forms.ToolStripButton();
            this.tsbDeleteConnection = new System.Windows.Forms.ToolStripButton();
            this.tsbUpdateSolution = new System.Windows.Forms.ToolStripButton();
            this.pnlFooter = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.cbExtendedLog = new System.Windows.Forms.CheckBox();
            this.cbIgnoreExtensions = new System.Windows.Forms.CheckBox();
            this.cbAutoPublish = new System.Windows.Forms.CheckBox();
            this.bCreateMapping = new System.Windows.Forms.Button();
            this.comboBoxSelectedConnection = new System.Windows.Forms.ComboBox();
            this.bPublish = new System.Windows.Forms.Button();
            this.pnlMain = new System.Windows.Forms.Panel();
            this.menu.SuspendLayout();
            this.pnlFooter.SuspendLayout();
            this.pnlMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // bValidate
            // 
            this.bValidate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.bValidate.Location = new System.Drawing.Point(773, 129);
            this.bValidate.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.bValidate.Name = "bValidate";
            this.bValidate.Size = new System.Drawing.Size(99, 28);
            this.bValidate.TabIndex = 3;
            this.bValidate.Text = "Save";
            this.bValidate.UseVisualStyleBackColor = true;
            this.bValidate.Click += new System.EventHandler(this.BValidateClick);
            // 
            // bCancel
            // 
            this.bCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.bCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.bCancel.Location = new System.Drawing.Point(883, 129);
            this.bCancel.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.bCancel.Name = "bCancel";
            this.bCancel.Size = new System.Drawing.Size(99, 28);
            this.bCancel.TabIndex = 4;
            this.bCancel.Text = "Cancel";
            this.bCancel.UseVisualStyleBackColor = true;
            this.bCancel.Click += new System.EventHandler(this.BCancelClick);
            // 
            // lvConnections
            // 
            this.lvConnections.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.chName,
            this.chServer,
            this.chOrganization,
            this.chUser,
            this.chVersion,
            this.chSolution});
            this.lvConnections.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvConnections.FullRowSelect = true;
            this.lvConnections.GridLines = true;
            this.lvConnections.HideSelection = false;
            this.lvConnections.LabelEdit = true;
            this.lvConnections.Location = new System.Drawing.Point(5, 5);
            this.lvConnections.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.lvConnections.Name = "lvConnections";
            this.lvConnections.Size = new System.Drawing.Size(982, 299);
            this.lvConnections.TabIndex = 2;
            this.lvConnections.UseCompatibleStateImageBehavior = false;
            this.lvConnections.View = System.Windows.Forms.View.Details;
            this.lvConnections.AfterLabelEdit += new System.Windows.Forms.LabelEditEventHandler(this.LvConnections_AfterLabelEdit);
            this.lvConnections.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.LvConnectionsColumnClick);
            this.lvConnections.SelectedIndexChanged += new System.EventHandler(this.lvConnections_SelectedIndexChanged);
            this.lvConnections.KeyDown += new System.Windows.Forms.KeyEventHandler(this.lvConnections_KeyDown);
            this.lvConnections.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.LvConnectionsMouseDoubleClick);
            // 
            // chName
            // 
            this.chName.Text = "Name";
            this.chName.Width = 180;
            // 
            // chServer
            // 
            this.chServer.Text = "Server";
            this.chServer.Width = 120;
            // 
            // chOrganization
            // 
            this.chOrganization.Text = "Organization";
            this.chOrganization.Width = 120;
            // 
            // chUser
            // 
            this.chUser.Text = "User";
            this.chUser.Width = 86;
            // 
            // chVersion
            // 
            this.chVersion.Text = "Version";
            this.chVersion.Width = 90;
            // 
            // chSolution
            // 
            this.chSolution.Text = "Solution";
            this.chSolution.Width = 132;
            // 
            // menu
            // 
            this.menu.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.menu.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsbNewConnection,
            this.tsbUpdateConnection,
            this.tsbDeleteConnection,
            this.tsbUpdateSolution});
            this.menu.Location = new System.Drawing.Point(0, 0);
            this.menu.Name = "menu";
            this.menu.Padding = new System.Windows.Forms.Padding(0);
            this.menu.Size = new System.Drawing.Size(992, 31);
            this.menu.TabIndex = 1;
            this.menu.Text = "tsMain";
            // 
            // tsbNewConnection
            // 
            this.tsbNewConnection.Image = ((System.Drawing.Image)(resources.GetObject("tsbNewConnection.Image")));
            this.tsbNewConnection.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbNewConnection.Name = "tsbNewConnection";
            this.tsbNewConnection.Size = new System.Drawing.Size(140, 28);
            this.tsbNewConnection.Text = "New connection";
            this.tsbNewConnection.Click += new System.EventHandler(this.tsbNewConnection_Click);
            // 
            // tsbUpdateConnection
            // 
            this.tsbUpdateConnection.Image = ((System.Drawing.Image)(resources.GetObject("tsbUpdateConnection.Image")));
            this.tsbUpdateConnection.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbUpdateConnection.Name = "tsbUpdateConnection";
            this.tsbUpdateConnection.Size = new System.Drawing.Size(159, 28);
            this.tsbUpdateConnection.Text = "Update connection";
            this.tsbUpdateConnection.Click += new System.EventHandler(this.tsbUpdateConnection_Click);
            // 
            // tsbDeleteConnection
            // 
            this.tsbDeleteConnection.Image = ((System.Drawing.Image)(resources.GetObject("tsbDeleteConnection.Image")));
            this.tsbDeleteConnection.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbDeleteConnection.Name = "tsbDeleteConnection";
            this.tsbDeleteConnection.Size = new System.Drawing.Size(154, 28);
            this.tsbDeleteConnection.Text = "Delete connection";
            this.tsbDeleteConnection.Click += new System.EventHandler(this.tsbDeleteConnection_Click);
            // 
            // tsbUpdateSolution
            // 
            this.tsbUpdateSolution.Image = global::McTools.Xrm.Connection.WinForms.Properties.Resources.ico_16_7100;
            this.tsbUpdateSolution.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbUpdateSolution.Name = "tsbUpdateSolution";
            this.tsbUpdateSolution.Size = new System.Drawing.Size(141, 28);
            this.tsbUpdateSolution.Text = "Update Solution";
            this.tsbUpdateSolution.ToolTipText = "Update solution for selected connection";
            this.tsbUpdateSolution.Click += new System.EventHandler(this.tsbUpdateSolution_Click);
            // 
            // pnlFooter
            // 
            this.pnlFooter.Controls.Add(this.label1);
            this.pnlFooter.Controls.Add(this.cbExtendedLog);
            this.pnlFooter.Controls.Add(this.cbIgnoreExtensions);
            this.pnlFooter.Controls.Add(this.cbAutoPublish);
            this.pnlFooter.Controls.Add(this.bCreateMapping);
            this.pnlFooter.Controls.Add(this.comboBoxSelectedConnection);
            this.pnlFooter.Controls.Add(this.bCancel);
            this.pnlFooter.Controls.Add(this.bPublish);
            this.pnlFooter.Controls.Add(this.bValidate);
            this.pnlFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlFooter.Location = new System.Drawing.Point(0, 340);
            this.pnlFooter.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.pnlFooter.Name = "pnlFooter";
            this.pnlFooter.Size = new System.Drawing.Size(992, 165);
            this.pnlFooter.TabIndex = 5;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 102);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(136, 17);
            this.label1.TabIndex = 10;
            this.label1.Text = "Selected connection";
            // 
            // cbExtendedLog
            // 
            this.cbExtendedLog.AutoSize = true;
            this.cbExtendedLog.Location = new System.Drawing.Point(16, 65);
            this.cbExtendedLog.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.cbExtendedLog.Name = "cbExtendedLog";
            this.cbExtendedLog.Size = new System.Drawing.Size(112, 21);
            this.cbExtendedLog.TabIndex = 9;
            this.cbExtendedLog.Text = "Extended log";
            this.cbExtendedLog.UseVisualStyleBackColor = true;
            // 
            // cbIgnoreExtensions
            // 
            this.cbIgnoreExtensions.AutoSize = true;
            this.cbIgnoreExtensions.Location = new System.Drawing.Point(16, 36);
            this.cbIgnoreExtensions.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.cbIgnoreExtensions.Name = "cbIgnoreExtensions";
            this.cbIgnoreExtensions.Size = new System.Drawing.Size(250, 21);
            this.cbIgnoreExtensions.TabIndex = 8;
            this.cbIgnoreExtensions.Text = "Search with and without extensions";
            this.cbIgnoreExtensions.UseVisualStyleBackColor = true;
            // 
            // cbAutoPublish
            // 
            this.cbAutoPublish.AutoSize = true;
            this.cbAutoPublish.Location = new System.Drawing.Point(16, 6);
            this.cbAutoPublish.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.cbAutoPublish.Name = "cbAutoPublish";
            this.cbAutoPublish.Size = new System.Drawing.Size(156, 21);
            this.cbAutoPublish.TabIndex = 7;
            this.cbAutoPublish.Text = "Publish after upload";
            this.cbAutoPublish.UseVisualStyleBackColor = true;
            // 
            // bCreateMapping
            // 
            this.bCreateMapping.Location = new System.Drawing.Point(773, 6);
            this.bCreateMapping.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.bCreateMapping.Name = "bCreateMapping";
            this.bCreateMapping.Size = new System.Drawing.Size(208, 28);
            this.bCreateMapping.TabIndex = 6;
            this.bCreateMapping.Text = "Create Mapping File";
            this.bCreateMapping.UseVisualStyleBackColor = true;
            this.bCreateMapping.Click += new System.EventHandler(this.bCreateMappingClick);
            // 
            // comboBoxSelectedConnection
            // 
            this.comboBoxSelectedConnection.FormattingEnabled = true;
            this.comboBoxSelectedConnection.Location = new System.Drawing.Point(16, 126);
            this.comboBoxSelectedConnection.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.comboBoxSelectedConnection.Name = "comboBoxSelectedConnection";
            this.comboBoxSelectedConnection.Size = new System.Drawing.Size(395, 24);
            this.comboBoxSelectedConnection.TabIndex = 5;
            this.comboBoxSelectedConnection.SelectedIndexChanged += new System.EventHandler(this.ComboBoxSelectedConnectionSelectedIndexChanged);
            // 
            // bPublish
            // 
            this.bPublish.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.bPublish.Location = new System.Drawing.Point(664, 129);
            this.bPublish.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.bPublish.Name = "bPublish";
            this.bPublish.Size = new System.Drawing.Size(99, 28);
            this.bPublish.TabIndex = 3;
            this.bPublish.Text = "Publish";
            this.bPublish.UseVisualStyleBackColor = true;
            this.bPublish.Click += new System.EventHandler(this.bPublishClick);
            // 
            // pnlMain
            // 
            this.pnlMain.Controls.Add(this.lvConnections);
            this.pnlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMain.Location = new System.Drawing.Point(0, 31);
            this.pnlMain.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.pnlMain.Name = "pnlMain";
            this.pnlMain.Padding = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.pnlMain.Size = new System.Drawing.Size(992, 309);
            this.pnlMain.TabIndex = 6;
            // 
            // ConnectionSelector
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.bCancel;
            this.ClientSize = new System.Drawing.Size(992, 505);
            this.Controls.Add(this.pnlMain);
            this.Controls.Add(this.pnlFooter);
            this.Controls.Add(this.menu);
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "ConnectionSelector";
            this.ShowIcon = false;
            this.Text = "Connection Manager";
            this.Load += new System.EventHandler(this.ConnectionSelector_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ConnectionSelector_KeyDown);
            this.menu.ResumeLayout(false);
            this.menu.PerformLayout();
            this.pnlFooter.ResumeLayout(false);
            this.pnlFooter.PerformLayout();
            this.pnlMain.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

     

        #endregion

        private System.Windows.Forms.Button bValidate;
        private System.Windows.Forms.Button bCancel;
        private System.Windows.Forms.ListView lvConnections;
        private System.Windows.Forms.ColumnHeader chName;
        private System.Windows.Forms.ColumnHeader chOrganization;
        private System.Windows.Forms.ColumnHeader chServer;
        private System.Windows.Forms.ColumnHeader chVersion;
        private System.Windows.Forms.ToolStrip menu;
        private System.Windows.Forms.ToolStripButton tsbNewConnection;
        private System.Windows.Forms.ToolStripButton tsbUpdateConnection;
        private System.Windows.Forms.ToolStripButton tsbDeleteConnection;
        private System.Windows.Forms.Panel pnlFooter;
        private System.Windows.Forms.Panel pnlMain;
        private System.Windows.Forms.ColumnHeader chUser;
        private System.Windows.Forms.ColumnHeader chSolution;
        private System.Windows.Forms.ToolStripButton tsbUpdateSolution;
        private System.Windows.Forms.ComboBox comboBoxSelectedConnection;
        private System.Windows.Forms.CheckBox cbExtendedLog;
        private System.Windows.Forms.CheckBox cbIgnoreExtensions;
        private System.Windows.Forms.CheckBox cbAutoPublish;
        private System.Windows.Forms.Button bCreateMapping;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button bPublish;
    }
}