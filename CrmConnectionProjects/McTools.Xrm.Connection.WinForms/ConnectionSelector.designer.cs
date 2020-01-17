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
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.menu = new System.Windows.Forms.ToolStrip();
            this.tsbNewConnection = new System.Windows.Forms.ToolStripButton();
            this.tsbUpdateConnection = new System.Windows.Forms.ToolStripButton();
            this.tsbDeleteConnection = new System.Windows.Forms.ToolStripButton();
            this.labelSelectedConnection = new System.Windows.Forms.Label();
            this.comboBoxSelectedConnection = new System.Windows.Forms.ComboBox();
            this.cbAutoPublish = new System.Windows.Forms.CheckBox();
            this.bPublish = new System.Windows.Forms.Button();
            this.cbIgnoreExtensions = new System.Windows.Forms.CheckBox();
            this.bCreateMapping = new System.Windows.Forms.Button();
            this.cbExtendedLog = new System.Windows.Forms.CheckBox();
            this.menu.SuspendLayout();
            this.SuspendLayout();
            // 
            // bValidate
            // 
            this.bValidate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.bValidate.Location = new System.Drawing.Point(501, 335);
            this.bValidate.Name = "bValidate";
            this.bValidate.Size = new System.Drawing.Size(75, 23);
            this.bValidate.TabIndex = 6;
            this.bValidate.Text = "Save";
            this.bValidate.UseVisualStyleBackColor = true;
            this.bValidate.Click += new System.EventHandler(this.BValidateClick);
            // 
            // bCancel
            // 
            this.bCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.bCancel.Location = new System.Drawing.Point(581, 335);
            this.bCancel.Name = "bCancel";
            this.bCancel.Size = new System.Drawing.Size(75, 23);
            this.bCancel.TabIndex = 5;
            this.bCancel.Text = "Cancel";
            this.bCancel.UseVisualStyleBackColor = true;
            this.bCancel.Click += new System.EventHandler(this.BCancelClick);
            // 
            // lvConnections
            // 
            this.lvConnections.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lvConnections.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader3,
            this.columnHeader2,
            this.columnHeader4,
            this.columnHeader5});
            this.lvConnections.FullRowSelect = true;
            this.lvConnections.GridLines = true;
            this.lvConnections.Location = new System.Drawing.Point(12, 28);
            this.lvConnections.Name = "lvConnections";
            this.lvConnections.Size = new System.Drawing.Size(644, 204);
            this.lvConnections.TabIndex = 9;
            this.lvConnections.UseCompatibleStateImageBehavior = false;
            this.lvConnections.View = System.Windows.Forms.View.Details;
            this.lvConnections.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.LvConnectionsColumnClick);
            this.lvConnections.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.LvConnectionsMouseDoubleClick);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Name";
            this.columnHeader1.Width = 150;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Server";
            this.columnHeader3.Width = 150;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Organization";
            this.columnHeader2.Width = 120;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "Version";
            this.columnHeader4.Width = 100;
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "Solution";
            this.columnHeader5.Width = 120;
            // 
            // menu
            // 
            this.menu.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.menu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsbNewConnection,
            this.tsbUpdateConnection,
            this.tsbDeleteConnection});
            this.menu.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.menu.Location = new System.Drawing.Point(0, 0);
            this.menu.Name = "menu";
            this.menu.Padding = new System.Windows.Forms.Padding(10, 2, 12, 0);
            this.menu.Size = new System.Drawing.Size(668, 25);
            this.menu.TabIndex = 11;
            this.menu.Text = "toolStrip1";
            // 
            // tsbNewConnection
            // 
            this.tsbNewConnection.Image = ((System.Drawing.Image)(resources.GetObject("tsbNewConnection.Image")));
            this.tsbNewConnection.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbNewConnection.Name = "tsbNewConnection";
            this.tsbNewConnection.Size = new System.Drawing.Size(114, 20);
            this.tsbNewConnection.Text = "New connection";
            this.tsbNewConnection.Click += new System.EventHandler(this.tsbNewConnection_Click);
            // 
            // tsbUpdateConnection
            // 
            this.tsbUpdateConnection.Image = ((System.Drawing.Image)(resources.GetObject("tsbUpdateConnection.Image")));
            this.tsbUpdateConnection.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbUpdateConnection.Name = "tsbUpdateConnection";
            this.tsbUpdateConnection.Size = new System.Drawing.Size(128, 20);
            this.tsbUpdateConnection.Text = "Update connection";
            this.tsbUpdateConnection.Click += new System.EventHandler(this.tsbUpdateConnection_Click);
            // 
            // tsbDeleteConnection
            // 
            this.tsbDeleteConnection.Image = ((System.Drawing.Image)(resources.GetObject("tsbDeleteConnection.Image")));
            this.tsbDeleteConnection.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbDeleteConnection.Name = "tsbDeleteConnection";
            this.tsbDeleteConnection.Size = new System.Drawing.Size(123, 20);
            this.tsbDeleteConnection.Text = "Delete connection";
            this.tsbDeleteConnection.Click += new System.EventHandler(this.tsbDeleteConnection_Click);
            // 
            // labelSelectedConnection
            // 
            this.labelSelectedConnection.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelSelectedConnection.AutoSize = true;
            this.labelSelectedConnection.Location = new System.Drawing.Point(9, 319);
            this.labelSelectedConnection.Name = "labelSelectedConnection";
            this.labelSelectedConnection.Size = new System.Drawing.Size(105, 13);
            this.labelSelectedConnection.TabIndex = 12;
            this.labelSelectedConnection.Text = "Selected connection";
            // 
            // comboBoxSelectedConnection
            // 
            this.comboBoxSelectedConnection.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.comboBoxSelectedConnection.FormattingEnabled = true;
            this.comboBoxSelectedConnection.Location = new System.Drawing.Point(12, 335);
            this.comboBoxSelectedConnection.Name = "comboBoxSelectedConnection";
            this.comboBoxSelectedConnection.Size = new System.Drawing.Size(244, 21);
            this.comboBoxSelectedConnection.TabIndex = 13;
            this.comboBoxSelectedConnection.SelectedIndexChanged += new System.EventHandler(this.ComboBoxSelectedConnectionSelectedIndexChanged);
            // 
            // cbAutoPublish
            // 
            this.cbAutoPublish.AutoSize = true;
            this.cbAutoPublish.Checked = true;
            this.cbAutoPublish.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbAutoPublish.Location = new System.Drawing.Point(12, 246);
            this.cbAutoPublish.Name = "cbAutoPublish";
            this.cbAutoPublish.Size = new System.Drawing.Size(119, 17);
            this.cbAutoPublish.TabIndex = 14;
            this.cbAutoPublish.Text = "Publish after upload";
            this.cbAutoPublish.UseVisualStyleBackColor = true;
            // 
            // bPublish
            // 
            this.bPublish.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.bPublish.Location = new System.Drawing.Point(387, 335);
            this.bPublish.Name = "bPublish";
            this.bPublish.Size = new System.Drawing.Size(108, 23);
            this.bPublish.TabIndex = 15;
            this.bPublish.Text = "Save and Update";
            this.bPublish.UseVisualStyleBackColor = true;
            this.bPublish.Click += new System.EventHandler(this.bPublishClick);
            // 
            // cbIgnoreExtensions
            // 
            this.cbIgnoreExtensions.AutoSize = true;
            this.cbIgnoreExtensions.Location = new System.Drawing.Point(12, 269);
            this.cbIgnoreExtensions.Name = "cbIgnoreExtensions";
            this.cbIgnoreExtensions.Size = new System.Drawing.Size(193, 17);
            this.cbIgnoreExtensions.TabIndex = 16;
            this.cbIgnoreExtensions.Text = "Search with and without extensions";
            this.cbIgnoreExtensions.UseVisualStyleBackColor = true;
            // 
            // bCreateMapping
            // 
            this.bCreateMapping.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.bCreateMapping.Location = new System.Drawing.Point(500, 246);
            this.bCreateMapping.Name = "bCreateMapping";
            this.bCreateMapping.Size = new System.Drawing.Size(156, 23);
            this.bCreateMapping.TabIndex = 17;
            this.bCreateMapping.Text = "Create Mapping File";
            this.bCreateMapping.UseVisualStyleBackColor = true;
            this.bCreateMapping.Click += new System.EventHandler(this.bCreateMappingClick);
            // 
            // cbExtendedLog
            // 
            this.cbExtendedLog.AutoSize = true;
            this.cbExtendedLog.Location = new System.Drawing.Point(12, 292);
            this.cbExtendedLog.Name = "cbExtendedLog";
            this.cbExtendedLog.Size = new System.Drawing.Size(88, 17);
            this.cbExtendedLog.TabIndex = 18;
            this.cbExtendedLog.Text = "Extended log";
            this.cbExtendedLog.UseVisualStyleBackColor = true;
            // 
            // ConnectionSelector
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(668, 370);
            this.Controls.Add(this.bPublish);
            this.Controls.Add(this.cbExtendedLog);
            this.Controls.Add(this.comboBoxSelectedConnection);
            this.Controls.Add(this.cbIgnoreExtensions);
            this.Controls.Add(this.cbAutoPublish);
            this.Controls.Add(this.labelSelectedConnection);
            this.Controls.Add(this.bCreateMapping);
            this.Controls.Add(this.menu);
            this.Controls.Add(this.bValidate);
            this.Controls.Add(this.bCancel);
            this.Controls.Add(this.lvConnections);
            this.Name = "ConnectionSelector";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Microsoft Dynamics CRM Web Resources Updater Options";
            this.menu.ResumeLayout(false);
            this.menu.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button bValidate;
        private System.Windows.Forms.Button bCancel;
        private System.Windows.Forms.ListView lvConnections;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ToolStrip menu;
        private System.Windows.Forms.ToolStripButton tsbNewConnection;
        private System.Windows.Forms.ToolStripButton tsbUpdateConnection;
        private System.Windows.Forms.ToolStripButton tsbDeleteConnection;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.Label labelSelectedConnection;
        private System.Windows.Forms.ComboBox comboBoxSelectedConnection;
        private System.Windows.Forms.CheckBox cbAutoPublish;
        private System.Windows.Forms.Button bPublish;
        private System.Windows.Forms.CheckBox cbIgnoreExtensions;
        private System.Windows.Forms.Button bCreateMapping;
        private System.Windows.Forms.CheckBox cbExtendedLog;
    }
}