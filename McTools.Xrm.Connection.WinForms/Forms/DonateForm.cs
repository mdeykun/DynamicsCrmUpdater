using Cwru.Common;
using System.Windows.Forms;

namespace McTools.Xrm.Connection.WinForms
{
    public partial class DonateForm : Form
    {
        public DonateForm()
        {
            InitializeComponent();

            usdterc20.Text = Info.UsdtErc20;
            usdttrc20.Text = Info.UsdtTrc20;
            ethereum.Text = Info.EthErc20;
            bitcoin.Text = Info.Btc;
        }

        private void copyUsdtErc20_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Clipboard.SetText(Info.UsdtErc20);
        }

        private void copyUsdtTrc20_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Clipboard.SetText(Info.UsdtTrc20);
        }

        private void copyEthereum_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Clipboard.SetText(Info.EthErc20);
        }

        private void copyBitcoin_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Clipboard.SetText(Info.Btc);
        }
    }
}
