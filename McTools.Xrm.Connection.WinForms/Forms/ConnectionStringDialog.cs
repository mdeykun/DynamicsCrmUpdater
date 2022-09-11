using McTools.Xrm.Connection.WinForms.Model;
using System.Windows.Forms;

namespace McTools.Xrm.Connection.WinForms
{
    public partial class ConnectionStringDialog : Form
    {
        public ConnectionStringDialog(ConnectionDetail detail)
        {
            InitializeComponent();

            txtConnectionString.Text = detail.ConnectionString;
            lblTitle.Text = string.Format(lblTitle.Text, detail.ConnectionName);
        }
    }
}