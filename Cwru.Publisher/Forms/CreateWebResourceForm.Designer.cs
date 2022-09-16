namespace Cwru.Publisher.Forms
{
    partial class CreateWebResourceForm
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
            this.tbName = new System.Windows.Forms.TextBox();
            this.lbName = new System.Windows.Forms.Label();
            this.tbDisplayName = new System.Windows.Forms.TextBox();
            this.lbDisplayName = new System.Windows.Forms.Label();
            this.tbDescription = new System.Windows.Forms.TextBox();
            this.lbDescription = new System.Windows.Forms.Label();
            this.lbType = new System.Windows.Forms.Label();
            this.cbType = new System.Windows.Forms.ComboBox();
            this.bCreate = new System.Windows.Forms.Button();
            this.bCancel = new System.Windows.Forms.Button();
            this.tbPrefix = new System.Windows.Forms.TextBox();
            this.lbPrefixDelimiter = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // tbName
            // 
            this.tbName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbName.Location = new System.Drawing.Point(167, 10);
            this.tbName.Name = "tbName";
            this.tbName.Size = new System.Drawing.Size(227, 20);
            this.tbName.TabIndex = 0;
            // 
            // lbName
            // 
            this.lbName.AutoSize = true;
            this.lbName.Location = new System.Drawing.Point(13, 13);
            this.lbName.Name = "lbName";
            this.lbName.Size = new System.Drawing.Size(35, 13);
            this.lbName.TabIndex = 1;
            this.lbName.Text = "Name";
            // 
            // tbDisplayName
            // 
            this.tbDisplayName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbDisplayName.Location = new System.Drawing.Point(118, 36);
            this.tbDisplayName.Name = "tbDisplayName";
            this.tbDisplayName.Size = new System.Drawing.Size(276, 20);
            this.tbDisplayName.TabIndex = 0;
            // 
            // lbDisplayName
            // 
            this.lbDisplayName.AutoSize = true;
            this.lbDisplayName.Location = new System.Drawing.Point(13, 39);
            this.lbDisplayName.Name = "lbDisplayName";
            this.lbDisplayName.Size = new System.Drawing.Size(72, 13);
            this.lbDisplayName.TabIndex = 1;
            this.lbDisplayName.Text = "Display Name";
            // 
            // tbDescription
            // 
            this.tbDescription.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbDescription.Location = new System.Drawing.Point(118, 62);
            this.tbDescription.Multiline = true;
            this.tbDescription.Name = "tbDescription";
            this.tbDescription.Size = new System.Drawing.Size(276, 67);
            this.tbDescription.TabIndex = 0;
            // 
            // lbDescription
            // 
            this.lbDescription.AutoSize = true;
            this.lbDescription.Location = new System.Drawing.Point(13, 65);
            this.lbDescription.Name = "lbDescription";
            this.lbDescription.Size = new System.Drawing.Size(60, 13);
            this.lbDescription.TabIndex = 1;
            this.lbDescription.Text = "Description";
            // 
            // lbType
            // 
            this.lbType.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lbType.AutoSize = true;
            this.lbType.Location = new System.Drawing.Point(13, 138);
            this.lbType.Name = "lbType";
            this.lbType.Size = new System.Drawing.Size(31, 13);
            this.lbType.TabIndex = 1;
            this.lbType.Text = "Type";
            // 
            // cbType
            // 
            this.cbType.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbType.FormattingEnabled = true;
            this.cbType.Location = new System.Drawing.Point(118, 135);
            this.cbType.Name = "cbType";
            this.cbType.Size = new System.Drawing.Size(276, 21);
            this.cbType.TabIndex = 2;
            // 
            // bCreate
            // 
            this.bCreate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.bCreate.Location = new System.Drawing.Point(320, 170);
            this.bCreate.Name = "bCreate";
            this.bCreate.Size = new System.Drawing.Size(75, 23);
            this.bCreate.TabIndex = 3;
            this.bCreate.Text = "Create";
            this.bCreate.UseVisualStyleBackColor = true;
            this.bCreate.Click += new System.EventHandler(this.bCreateClick);
            // 
            // bCancel
            // 
            this.bCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.bCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.bCancel.Location = new System.Drawing.Point(239, 170);
            this.bCancel.Name = "bCancel";
            this.bCancel.Size = new System.Drawing.Size(75, 23);
            this.bCancel.TabIndex = 3;
            this.bCancel.Text = "Cancel";
            this.bCancel.UseVisualStyleBackColor = true;
            this.bCancel.Click += new System.EventHandler(this.bCancelClick);
            // 
            // tbPrefix
            // 
            this.tbPrefix.Location = new System.Drawing.Point(118, 10);
            this.tbPrefix.Name = "tbPrefix";
            this.tbPrefix.Size = new System.Drawing.Size(30, 20);
            this.tbPrefix.TabIndex = 4;
            // 
            // lbPrefixDelimiter
            // 
            this.lbPrefixDelimiter.AutoSize = true;
            this.lbPrefixDelimiter.Location = new System.Drawing.Point(151, 15);
            this.lbPrefixDelimiter.Name = "lbPrefixDelimiter";
            this.lbPrefixDelimiter.Size = new System.Drawing.Size(13, 13);
            this.lbPrefixDelimiter.TabIndex = 5;
            this.lbPrefixDelimiter.Text = "_";
            // 
            // CreateWebResourceForm
            // 
            this.AcceptButton = this.bCreate;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.bCancel;
            this.ClientSize = new System.Drawing.Size(410, 206);
            this.Controls.Add(this.lbPrefixDelimiter);
            this.Controls.Add(this.tbPrefix);
            this.Controls.Add(this.bCancel);
            this.Controls.Add(this.bCreate);
            this.Controls.Add(this.cbType);
            this.Controls.Add(this.lbType);
            this.Controls.Add(this.lbDescription);
            this.Controls.Add(this.lbDisplayName);
            this.Controls.Add(this.lbName);
            this.Controls.Add(this.tbDescription);
            this.Controls.Add(this.tbDisplayName);
            this.Controls.Add(this.tbName);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "CreateWebResourceForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Create Web Resource";
            this.Load += new System.EventHandler(this.CreateWebResourceFormLoad);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox tbName;
        private System.Windows.Forms.Label lbName;
        private System.Windows.Forms.TextBox tbDisplayName;
        private System.Windows.Forms.Label lbDisplayName;
        private System.Windows.Forms.TextBox tbDescription;
        private System.Windows.Forms.Label lbDescription;
        private System.Windows.Forms.Label lbType;
        private System.Windows.Forms.ComboBox cbType;
        private System.Windows.Forms.Button bCreate;
        private System.Windows.Forms.Button bCancel;
        private System.Windows.Forms.TextBox tbPrefix;
        private System.Windows.Forms.Label lbPrefixDelimiter;
    }
}