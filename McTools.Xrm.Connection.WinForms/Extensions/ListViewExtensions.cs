using McTools.Xrm.Connection.WinForms.Model;
using System.Collections.Generic;
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

        public static void RemoveSelected(this ListView listView)
        {
            var toDelete = listView.SelectedItems.ToList();

            foreach (var item in toDelete)
            {
                listView.Items.Remove(item);
            }
        }

        public static List<T> GetTagValues<T>(this ListViewItemCollection collection)
        {
            var list = new List<T>();
            foreach (ListViewItem item in collection)
            {
                list.Add((T)item.Tag);
            }

            return list;
        }

        public static List<T> GetTagValues<T>(this SelectedListViewItemCollection collection)
        {
            var list = new List<T>();
            foreach (ListViewItem item in collection)
            {
                list.Add((T)item.Tag);
            }

            return list;
        }

        public static List<ListViewItem> ToList(this SelectedListViewItemCollection collection)
        {
            var list = new List<ListViewItem>();
            foreach (ListViewItem item in collection)
            {
                list.Add(item);
            }

            return list;
        }
    }
}
