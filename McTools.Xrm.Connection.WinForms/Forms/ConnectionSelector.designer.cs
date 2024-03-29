﻿namespace McTools.Xrm.Connection.WinForms
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
            this.bSave = new System.Windows.Forms.Button();
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
            this.pnlMain = new System.Windows.Forms.Panel();
            this.showAboutButton = new System.Windows.Forms.Button();
            this.showDonationButton = new System.Windows.Forms.Button();
            this.menu.SuspendLayout();
            this.pnlFooter.SuspendLayout();
            this.pnlMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // bSave
            // 
            this.bSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.bSave.Location = new System.Drawing.Point(580, 105);
            this.bSave.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.bSave.Name = "bSave";
            this.bSave.Size = new System.Drawing.Size(74, 23);
            this.bSave.TabIndex = 3;
            this.bSave.Text = "Save";
            this.bSave.UseVisualStyleBackColor = true;
            this.bSave.Click += new System.EventHandler(this.bSave_Click);
            // 
            // bCancel
            // 
            this.bCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.bCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.bCancel.Location = new System.Drawing.Point(662, 105);
            this.bCancel.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.bCancel.Name = "bCancel";
            this.bCancel.Size = new System.Drawing.Size(74, 23);
            this.bCancel.TabIndex = 4;
            this.bCancel.Text = "Cancel";
            this.bCancel.UseVisualStyleBackColor = true;
            this.bCancel.Click += new System.EventHandler(this.bCancel_Click);
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
            this.lvConnections.Location = new System.Drawing.Point(4, 4);
            this.lvConnections.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.lvConnections.MultiSelect = false;
            this.lvConnections.Name = "lvConnections";
            this.lvConnections.Size = new System.Drawing.Size(736, 243);
            this.lvConnections.TabIndex = 2;
            this.lvConnections.UseCompatibleStateImageBehavior = false;
            this.lvConnections.View = System.Windows.Forms.View.Details;
            this.lvConnections.AfterLabelEdit += new System.Windows.Forms.LabelEditEventHandler(this.lvConnections_AfterLabelEdit);
            this.lvConnections.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.lvConnectionsColumn_Click);
            this.lvConnections.SelectedIndexChanged += new System.EventHandler(this.lvConnections_SelectedIndexChanged);
            this.lvConnections.KeyDown += new System.Windows.Forms.KeyEventHandler(this.lvConnections_KeyDown);
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
            this.menu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsbNewConnection,
            this.tsbUpdateConnection,
            this.tsbDeleteConnection,
            this.tsbUpdateSolution});
            this.menu.Location = new System.Drawing.Point(0, 0);
            this.menu.Name = "menu";
            this.menu.Padding = new System.Windows.Forms.Padding(5, 0, 0, 0);
            this.menu.Size = new System.Drawing.Size(744, 25);
            this.menu.TabIndex = 1;
            this.menu.Text = "tsMain";
            // 
            // tsbNewConnection
            // 
            this.tsbNewConnection.Image = ((System.Drawing.Image)(resources.GetObject("tsbNewConnection.Image")));
            this.tsbNewConnection.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbNewConnection.Name = "tsbNewConnection";
            this.tsbNewConnection.Size = new System.Drawing.Size(114, 22);
            this.tsbNewConnection.Text = "New connection";
            this.tsbNewConnection.Click += new System.EventHandler(this.tsbNewConnection_Click);
            // 
            // tsbUpdateConnection
            // 
            this.tsbUpdateConnection.Image = ((System.Drawing.Image)(resources.GetObject("tsbUpdateConnection.Image")));
            this.tsbUpdateConnection.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbUpdateConnection.Name = "tsbUpdateConnection";
            this.tsbUpdateConnection.Size = new System.Drawing.Size(128, 22);
            this.tsbUpdateConnection.Text = "Update connection";
            this.tsbUpdateConnection.Click += new System.EventHandler(this.tsbUpdateConnection_Click);
            // 
            // tsbDeleteConnection
            // 
            this.tsbDeleteConnection.Image = ((System.Drawing.Image)(resources.GetObject("tsbDeleteConnection.Image")));
            this.tsbDeleteConnection.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbDeleteConnection.Name = "tsbDeleteConnection";
            this.tsbDeleteConnection.Size = new System.Drawing.Size(123, 22);
            this.tsbDeleteConnection.Text = "Delete connection";
            this.tsbDeleteConnection.Click += new System.EventHandler(this.tsbDeleteConnection_Click);
            // 
            // tsbUpdateSolution
            // 
            this.tsbUpdateSolution.Image = global::McTools.Xrm.Connection.WinForms.Properties.Resources.ico_16_7100;
            this.tsbUpdateSolution.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbUpdateSolution.Name = "tsbUpdateSolution";
            this.tsbUpdateSolution.Size = new System.Drawing.Size(112, 22);
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
            this.pnlFooter.Controls.Add(this.bSave);
            this.pnlFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlFooter.Location = new System.Drawing.Point(0, 276);
            this.pnlFooter.Margin = new System.Windows.Forms.Padding(2);
            this.pnlFooter.Name = "pnlFooter";
            this.pnlFooter.Size = new System.Drawing.Size(744, 134);
            this.pnlFooter.TabIndex = 5;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 83);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(105, 13);
            this.label1.TabIndex = 10;
            this.label1.Text = "Selected connection";
            // 
            // cbExtendedLog
            // 
            this.cbExtendedLog.AutoSize = true;
            this.cbExtendedLog.Location = new System.Drawing.Point(12, 53);
            this.cbExtendedLog.Name = "cbExtendedLog";
            this.cbExtendedLog.Size = new System.Drawing.Size(88, 17);
            this.cbExtendedLog.TabIndex = 9;
            this.cbExtendedLog.Text = "Extended log";
            this.cbExtendedLog.UseVisualStyleBackColor = true;
            // 
            // cbIgnoreExtensions
            // 
            this.cbIgnoreExtensions.AutoSize = true;
            this.cbIgnoreExtensions.Location = new System.Drawing.Point(12, 29);
            this.cbIgnoreExtensions.Name = "cbIgnoreExtensions";
            this.cbIgnoreExtensions.Size = new System.Drawing.Size(193, 17);
            this.cbIgnoreExtensions.TabIndex = 8;
            this.cbIgnoreExtensions.Text = "Search with and without extensions";
            this.cbIgnoreExtensions.UseVisualStyleBackColor = true;
            // 
            // cbAutoPublish
            // 
            this.cbAutoPublish.AutoSize = true;
            this.cbAutoPublish.Location = new System.Drawing.Point(12, 5);
            this.cbAutoPublish.Name = "cbAutoPublish";
            this.cbAutoPublish.Size = new System.Drawing.Size(119, 17);
            this.cbAutoPublish.TabIndex = 7;
            this.cbAutoPublish.Text = "Publish after upload";
            this.cbAutoPublish.UseVisualStyleBackColor = true;
            // 
            // bCreateMapping
            // 
            this.bCreateMapping.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.bCreateMapping.Location = new System.Drawing.Point(580, 5);
            this.bCreateMapping.Name = "bCreateMapping";
            this.bCreateMapping.Size = new System.Drawing.Size(156, 23);
            this.bCreateMapping.TabIndex = 6;
            this.bCreateMapping.Text = "Create Mapping File";
            this.bCreateMapping.UseVisualStyleBackColor = true;
            this.bCreateMapping.Click += new System.EventHandler(this.bCreateMapping_Click);
            // 
            // comboBoxSelectedConnection
            // 
            this.comboBoxSelectedConnection.FormattingEnabled = true;
            this.comboBoxSelectedConnection.Location = new System.Drawing.Point(12, 102);
            this.comboBoxSelectedConnection.Name = "comboBoxSelectedConnection";
            this.comboBoxSelectedConnection.Size = new System.Drawing.Size(297, 21);
            this.comboBoxSelectedConnection.TabIndex = 5;
            // 
            // pnlMain
            // 
            this.pnlMain.Controls.Add(this.lvConnections);
            this.pnlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMain.Location = new System.Drawing.Point(0, 25);
            this.pnlMain.Margin = new System.Windows.Forms.Padding(2);
            this.pnlMain.Name = "pnlMain";
            this.pnlMain.Padding = new System.Windows.Forms.Padding(4);
            this.pnlMain.Size = new System.Drawing.Size(744, 251);
            this.pnlMain.TabIndex = 6;
            // 
            // showAboutButton
            // 
            this.showAboutButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.showAboutButton.Location = new System.Drawing.Point(717, 1);
            this.showAboutButton.Name = "showAboutButton";
            this.showAboutButton.Size = new System.Drawing.Size(23, 23);
            this.showAboutButton.TabIndex = 7;
            this.showAboutButton.Text = "?";
            this.showAboutButton.UseVisualStyleBackColor = true;
            this.showAboutButton.Click += new System.EventHandler(this.bAboutClick);
            // 
            // showDonationButton
            // 
            this.showDonationButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.showDonationButton.Location = new System.Drawing.Point(651, 1);
            this.showDonationButton.Name = "showDonationButton";
            this.showDonationButton.Size = new System.Drawing.Size(60, 23);
            this.showDonationButton.TabIndex = 8;
            this.showDonationButton.Text = "Donate";
            this.showDonationButton.UseVisualStyleBackColor = true;
            this.showDonationButton.Click += new System.EventHandler(this.showDonationButton_Click);
            // 
            // ConnectionSelector
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.bCancel;
            this.ClientSize = new System.Drawing.Size(744, 410);
            this.Controls.Add(this.showDonationButton);
            this.Controls.Add(this.showAboutButton);
            this.Controls.Add(this.pnlMain);
            this.Controls.Add(this.pnlFooter);
            this.Controls.Add(this.menu);
            this.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.Name = "ConnectionSelector";
            this.ShowIcon = false;
            this.Text = "Connection Manager";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.connectionSelector_KeyDown);
            this.menu.ResumeLayout(false);
            this.menu.PerformLayout();
            this.pnlFooter.ResumeLayout(false);
            this.pnlFooter.PerformLayout();
            this.pnlMain.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button bSave;
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
        private System.Windows.Forms.Button showAboutButton;
        private System.Windows.Forms.Button showDonationButton;
    }
}