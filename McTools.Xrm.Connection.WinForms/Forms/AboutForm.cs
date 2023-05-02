using Cwru.Common;
using System.Windows.Forms;

namespace McTools.Xrm.Connection.WinForms
{
    public partial class AboutForm : Form
    {
        public AboutForm()
        {
            InitializeComponent();
            Version.Text = "Version: " + Info.Version;
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(Info.OpenBugUrl);
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/MscrmTools");
        }
    }
}
