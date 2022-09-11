using Cwru.Common.Extensions;
using System;
using System.Security;
using System.Windows.Forms;

namespace McTools.Xrm.Connection.WinForms
{
    /// <summary>
    /// Formulaire Windows permettant de demander le mot de
    /// passe d'un utilisateur
    /// </summary>
    public partial class PasswordForm : Form
    {
        #region Variables

        /// <summary>
        /// Nom de domaine pour l'utilisateur
        /// </summary>
        private string userDomain;

        /// <summary>
        /// Login de l'utilisateur
        /// </summary>
        private string userLogin;

        /// <summary>
        /// Mot de passe de l'utilisateur
        /// </summary>
        private SecureString userPassword;

        #endregion Variables

        #region Constructeur

        /// <summary>
        /// Créé une nouvelle instance de la classe PasswordForm
        /// </summary>
        public PasswordForm(string connectionName)
        {
            InitializeComponent();

            lblConnectionName.Text = connectionName;
        }

        #endregion Constructeur

        #region Propriétés

        public bool SavePassword { get; set; }

        /// <summary>
        /// Obtient ou définit le nom de domaine pour l'utilisateur
        /// </summary>
        public string UserDomain
        {
            get { return userDomain; }
            set { userDomain = value; }
        }

        /// <summary>
        /// Obtient ou définit le login de l'utilisateur
        /// </summary>
        public string UserLogin
        {
            get { return userLogin; }
            set { userLogin = value; }
        }

        /// <summary>
        /// Obtient le mot de passe de l'utilisateur
        /// </summary>
        public SecureString UserPassword
        {
            get { return userPassword; }
        }

        #endregion Propriétés

        #region Méthodes

        protected override void OnLoad(EventArgs e)
        {
            this.tbUserLogin.Text = string.Format("{0}{1}{2}",
                userDomain,
                !string.IsNullOrEmpty(userDomain) ? "\\" : "",
                userLogin);

            tbPassword.Focus();

            base.OnLoad(e);
        }

        private void bCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void bValidate_Click(object sender, EventArgs e)
        {
            bool go = true;

            if (this.tbPassword.Text.Length == 0)
            {
                if (MessageBox.Show(this, "Are you sure you want to leave the password empty?", "Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    go = false;
                }
            }

            if (go)
            {
                this.userPassword = this.tbPassword.Text.ToSecureString();
                this.SavePassword = this.chkSavePassword.Checked;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void chkShowCharacters_CheckedChanged(object sender, EventArgs e)
        {
            this.tbPassword.PasswordChar = this.chkShowCharacters.Checked ? (char)0 : '•';
        }

        private void tbPassword_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
            {
                bValidate_Click(null, null);
            }
        }

        #endregion Méthodes
    }
}