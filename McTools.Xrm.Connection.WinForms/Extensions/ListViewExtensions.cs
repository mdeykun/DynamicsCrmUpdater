using McTools.Xrm.Connection.WinForms.Model;
using System.Windows.Forms;
using static System.Windows.Forms.ListView;

namespace McTools.Xrm.Connection.WinForms.Extensions
{
    public static class ListViewExtensions
    {
        public static void UpdateWith(this ListViewItem item, ConnectionDetail connectionDetail)
        {
            item.Tag = connectionDetail;
            item.ImageIndex = connectionDetail.GetImageIndex();
            item.SubItems[0].Text = connectionDetail.ConnectionName;
            item.SubItems[1].Text = connectionDetail.ServerName;
            item.SubItems[2].Text = connectionDetail.Organization;
            item.SubItems[3].Text = connectionDetail.GetUserName();
            item.SubItems[4].Text = connectionDetail.OrganizationVersion;
            item.SubItems[5].Text = connectionDetail.SolutionName;
        }

        public static void Add(this ListViewItemCollection collection, ConnectionDetail connectionDetail, bool selected = false)
        {
            collection.Add(new ListViewItem(connectionDetail.ConnectionName)
            {
                Tag = connectionDetail,
                ImageIndex = connectionDetail.GetImageIndex(),
                Selected = selected,
                SubItems =
                    {
                        connectionDetail.OriginalUrl,
                        connectionDetail.Organization,
                        connectionDetail.GetUserName(),
                        connectionDetail.OrganizationVersion,
                        connectionDetail.SolutionName
                    }
            });
        }
    }
}
