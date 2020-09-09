namespace McTools.Xrm.Connection.WinForms
{
    partial class UpdateSolutionForm
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.lblHeaderDesc = new System.Windows.Forms.Label();
            this.lblHeaderTitle = new System.Windows.Forms.Label();
            this.lbSolution = new System.Windows.Forms.Label();
            this.comboBoxSolutions = new System.Windows.Forms.ComboBox();
            this.btnOk = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.lbProgressBar = new System.Windows.Forms.Label();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.BackColor = System.Drawing.Color.White;
            this.panel1.Controls.Add(this.lblHeaderDesc);
            this.panel1.Controls.Add(this.lblHeaderTitle);
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(572, 60);
            this.panel1.TabIndex = 11;
            // 
            // lblHeaderDesc
            // 
            this.lblHeaderDesc.AutoSize = true;
            this.lblHeaderDesc.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.lblHeaderDesc.Location = new System.Drawing.Point(13, 38);
            this.lblHeaderDesc.Name = "lblHeaderDesc";
            this.lblHeaderDesc.Size = new System.Drawing.Size(272, 13);
            this.lblHeaderDesc.TabIndex = 13;
            this.lblHeaderDesc.Text = "You are about to update solution for connection {0}";
            // 
            // lblHeaderTitle
            // 
            this.lblHeaderTitle.AutoSize = true;
            this.lblHeaderTitle.Font = new System.Drawing.Font("Segoe UI", 14F);
            this.lblHeaderTitle.Location = new System.Drawing.Point(3, 9);
            this.lblHeaderTitle.Name = "lblHeaderTitle";
            this.lblHeaderTitle.Size = new System.Drawing.Size(146, 25);
            this.lblHeaderTitle.TabIndex = 12;
            this.lblHeaderTitle.Text = "Update solution";
            // 
            // lbSolution
            // 
            this.lbSolution.AutoSize = true;
            this.lbSolution.Location = new System.Drawing.Point(12, 71);
            this.lbSolution.Name = "lbSolution";
            this.lbSolution.Size = new System.Drawing.Size(68, 13);
            this.lbSolution.TabIndex = 17;
            this.lbSolution.Text = "New solution";
            // 
            // comboBoxSolutions
            // 
            this.comboBoxSolutions.FormattingEnabled = true;
            this.comboBoxSolutions.Location = new System.Drawing.Point(127, 68);
            this.comboBoxSolutions.Name = "comboBoxSolutions";
            this.comboBoxSolutions.Size = new System.Drawing.Size(430, 21);
            this.comboBoxSolutions.TabIndex = 13;
            // 
            // btnOk
            // 
            this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOk.Location = new System.Drawing.Point(396, 179);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 23);
            this.btnOk.TabIndex = 2;
            this.btnOk.Text = "OK";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.Location = new System.Drawing.Point(477, 179);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // lbProgressBar
            // 
            this.lbProgressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbProgressBar.Location = new System.Drawing.Point(41, 128);
            this.lbProgressBar.Name = "lbProgressBar";
            this.lbProgressBar.Size = new System.Drawing.Size(491, 22);
            this.lbProgressBar.TabIndex = 19;
            this.lbProgressBar.Text = "Loading solutions from your Microsoft Dynamics CRM organization";
            this.lbProgressBar.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // progressBar1
            // 
            this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar1.Location = new System.Drawing.Point(41, 94);
            this.progressBar1.Margin = new System.Windows.Forms.Padding(2);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(491, 20);
            this.progressBar1.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.progressBar1.TabIndex = 18;
            // 
            // UpdateSolutionForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(564, 214);
            this.Controls.Add(this.lbProgressBar);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.comboBoxSolutions);
            this.Controls.Add(this.lbSolution);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "UpdateSolutionForm";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Update Solution";
            this.Load += new System.EventHandler(this.UpdateSolutionForm_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label lblHeaderDesc;
        private System.Windows.Forms.Label lblHeaderTitle;
        private System.Windows.Forms.Label lbSolution;
        private System.Windows.Forms.ComboBox comboBoxSolutions;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label lbProgressBar;
        private System.Windows.Forms.ProgressBar progressBar1;
    }
}