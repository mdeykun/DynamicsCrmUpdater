namespace McTools.Xrm.Connection.WinForms.CustomControls
{
    partial class ConnectionSucceededControl
    {
        /// <summary> 
        /// Variable nécessaire au concepteur.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Nettoyage des ressources utilisées.
        /// </summary>
        /// <param name="disposing">true si les ressources managées doivent être supprimées ; sinon, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Code généré par le Concepteur de composants

        /// <summary> 
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas 
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
            this.label2 = new System.Windows.Forms.Label();
            this.txtConnectionName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnClearEnvHighlight = new System.Windows.Forms.Button();
            this.btnSetEnvHighlight = new System.Windows.Forms.Button();
            this.lblHighlight = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.comboBoxSolutions = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 47);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(35, 13);
            this.label2.TabIndex = 18;
            this.label2.Text = "Name";
            // 
            // txtConnectionName
            // 
            this.txtConnectionName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtConnectionName.Location = new System.Drawing.Point(84, 45);
            this.txtConnectionName.Name = "txtConnectionName";
            this.txtConnectionName.Size = new System.Drawing.Size(406, 20);
            this.txtConnectionName.TabIndex = 19;
            // 
            // label1
            // 
            this.label1.Dock = System.Windows.Forms.DockStyle.Top;
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(491, 40);
            this.label1.TabIndex = 17;
            this.label1.Text = "The connection was created successfully. If you want to save this connection, ple" +
    "ase provide a name for this connection.";
            // 
            // btnClearEnvHighlight
            // 
            this.btnClearEnvHighlight.Location = new System.Drawing.Point(163, 178);
            this.btnClearEnvHighlight.Margin = new System.Windows.Forms.Padding(2);
            this.btnClearEnvHighlight.Name = "btnClearEnvHighlight";
            this.btnClearEnvHighlight.Size = new System.Drawing.Size(183, 22);
            this.btnClearEnvHighlight.TabIndex = 23;
            this.btnClearEnvHighlight.Text = "Clear Environment Highlight";
            this.btnClearEnvHighlight.UseVisualStyleBackColor = true;
            this.btnClearEnvHighlight.Visible = false;
            this.btnClearEnvHighlight.Click += new System.EventHandler(this.btnClearEnvHighlight_Click);
            // 
            // btnSetEnvHighlight
            // 
            this.btnSetEnvHighlight.Location = new System.Drawing.Point(6, 178);
            this.btnSetEnvHighlight.Margin = new System.Windows.Forms.Padding(2);
            this.btnSetEnvHighlight.Name = "btnSetEnvHighlight";
            this.btnSetEnvHighlight.Size = new System.Drawing.Size(154, 22);
            this.btnSetEnvHighlight.TabIndex = 22;
            this.btnSetEnvHighlight.Text = "Set Environment Highlight";
            this.btnSetEnvHighlight.UseVisualStyleBackColor = true;
            this.btnSetEnvHighlight.Click += new System.EventHandler(this.btnSetEnvHighlight_Click);
            // 
            // lblHighlight
            // 
            this.lblHighlight.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblHighlight.Location = new System.Drawing.Point(3, 153);
            this.lblHighlight.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblHighlight.Name = "lblHighlight";
            this.lblHighlight.Size = new System.Drawing.Size(488, 23);
            this.lblHighlight.TabIndex = 21;
            this.lblHighlight.Text = "You can also define an environment highlight for this connection";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 81);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(45, 13);
            this.label3.TabIndex = 18;
            this.label3.Text = "Solution";
            // 
            // comboBoxSolutions
            // 
            this.comboBoxSolutions.FormattingEnabled = true;
            this.comboBoxSolutions.Location = new System.Drawing.Point(84, 74);
            this.comboBoxSolutions.Name = "comboBoxSolutions";
            this.comboBoxSolutions.Size = new System.Drawing.Size(406, 21);
            this.comboBoxSolutions.TabIndex = 24;
            // 
            // ConnectionSucceededControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.comboBoxSolutions);
            this.Controls.Add(this.btnClearEnvHighlight);
            this.Controls.Add(this.btnSetEnvHighlight);
            this.Controls.Add(this.lblHighlight);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtConnectionName);
            this.Controls.Add(this.label1);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "ConnectionSucceededControl";
            this.Size = new System.Drawing.Size(491, 219);
            this.Load += new System.EventHandler(this.ConnectionSucceededControl_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtConnectionName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnClearEnvHighlight;
        private System.Windows.Forms.Button btnSetEnvHighlight;
        private System.Windows.Forms.Label lblHighlight;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox comboBoxSolutions;
    }
}
