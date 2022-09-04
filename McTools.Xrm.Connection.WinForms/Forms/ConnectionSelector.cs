using Cwru.CrmRequests.Common.Contracts;
using McTools.Xrm.Connection.WinForms.Extensions;
using McTools.Xrm.Connection.WinForms.Misc;
using McTools.Xrm.Connection.WinForms.Model;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace McTools.Xrm.Connection.WinForms
{
    public partial class ConnectionSelector : Form
    {
        public ConnectionDetailsList connectionsList;
        private ICrmRequests crmRequests;

        public ConnectionSelector(
            ICrmRequests crmRequests,
            ConnectionDetailsList connectionsList)
        {
            InitializeComponent();

            this.crmRequests = crmRequests;
            this.connectionsList = connectionsList;

            tsbDeleteConnection.Visible = true;
            tsbUpdateConnection.Visible = true;
            bValidate.Visible = true;
            lvConnections.MultiSelect = false;
            cbAutoPublish.Checked = connectionsList.PublishAfterUpload;
            cbIgnoreExtensions.Checked = connectionsList.IgnoreExtensions;
            cbExtendedLog.Checked = connectionsList.ExtendedLog;

            DisplayConnections();
            RefreshComboBoxSelectedConnection();
        }

        private void DisplayConnections()
        {
            lvConnections.Items.Clear();
            lvConnections.Groups.Clear();
            try
            {
                LoadImages();
                foreach (ConnectionDetail detail in connectionsList.Connections)
                {
                    lvConnections.Items.Add(detail);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to display connections: {ex.Message}\r\n{ex.StackTrace}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadImages()
        {
            lvConnections.SmallImageList = new ImageList();
            lvConnections.SmallImageList.Images.Add(RessourceManager.GetImage("McTools.Xrm.Connection.WinForms.Resources.CRMOnlineLive_16.png"));
            lvConnections.SmallImageList.Images.Add(RessourceManager.GetImage("McTools.Xrm.Connection.WinForms.Resources.server_key.png"));
            lvConnections.SmallImageList.Images.Add(RessourceManager.GetImage("McTools.Xrm.Connection.WinForms.Resources.powerapps16.png"));
        }

        private void BCancelClick(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void BSaveClick(object sender, EventArgs e)
        {
            var selectedConnection = comboBoxSelectedConnection.SelectedItem as ConnectionDetail;

            connectionsList = new ConnectionDetailsList(lvConnections.Items.Cast<ListViewItem>().Select(x => x.Tag as ConnectionDetail))
            {
                PublishAfterUpload = cbAutoPublish.Checked,
                IgnoreExtensions = cbIgnoreExtensions.Checked,
                ExtendedLog = cbExtendedLog.Checked,
                SelectedConnectionId = selectedConnection?.ConnectionId
            };

            DialogResult = DialogResult.OK;
        }

        private void LvConnectionsColumnClick(object sender, ColumnClickEventArgs e)
        {
            lvConnections.Sorting = lvConnections.Sorting == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;
            lvConnections.ListViewItemSorter = new ListViewItemComparer(e.Column, lvConnections.Sorting);
        }

        private void ConnectionSelector_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.N)
            {
                tsbNewConnection_Click(null, null);
            }

            if (e.Control && e.KeyCode == Keys.U)
            {
                tsbUpdateConnection_Click(null, null);
            }

            if (e.Control && e.KeyCode == Keys.D)
            {
                tsbDeleteConnection_Click(null, null);
            }
        }

        private void RefreshComboBoxSelectedConnection()
        {
            comboBoxSelectedConnection.Items.Clear();
            comboBoxSelectedConnection.SelectedText = "";
            comboBoxSelectedConnection.Text = "";
            if (connectionsList == null || connectionsList.Connections == null)
            {
                return;
            }

            foreach (ConnectionDetail detail in connectionsList.Connections)
            {
                comboBoxSelectedConnection.Items.Add(detail);
            }
            if (connectionsList.SelectedConnectionId != null)
            {
                comboBoxSelectedConnection.SelectedIndex = connectionsList.Connections.FindIndex(x => x.ConnectionId == connectionsList.SelectedConnectionId);
            };
        }

        private void lvConnections_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter || lvConnections.SelectedItems.Count == 0)
                return;

            BSaveClick(null, null);
        }

        private void lvConnections_SelectedIndexChanged(object sender, EventArgs e)
        {
            bValidate.Enabled = lvConnections.SelectedItems.Count > 0;
            tsbUpdateConnection.Visible = lvConnections.SelectedItems.Count == 1;
            tsbUpdateSolution.Visible = lvConnections.SelectedItems.Count == 1;
        }

        private void LvConnections_AfterLabelEdit(object sender, LabelEditEventArgs e)
        {
            if (!e.CancelEdit)
            {
                var detail = (ConnectionDetail)lvConnections.Items[e.Item].Tag;
                detail.ConnectionName = e.Label;
            }
        }

        private void tsbDeleteConnection_Click(object sender, EventArgs e)
        {
            if (lvConnections.SelectedItems.Count > 0)
            {
                var result = MessageBox.Show(this, @"Are you sure you want to delete selected connection(s)?", @"Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.No)
                {
                    return;
                }
            }

            foreach (ListViewItem connectionItem in lvConnections.SelectedItems)
            {
                var detailToRemove = (ConnectionDetail)connectionItem.Tag;
                lvConnections.Items.Remove(lvConnections.SelectedItems[0]);
                connectionsList.Connections.RemoveAll(d => d.ConnectionId == detailToRemove.ConnectionId);
            }
        }

        private void tsbNewConnection_Click(object sender, EventArgs e)
        {
            var cForm = new ConnectionWizard(crmRequests)
            {
                StartPosition = FormStartPosition.CenterParent
            };

            if (cForm.ShowDialog(this) == DialogResult.OK)
            {
                var detail = cForm.CrmConnectionDetail;

                lvConnections.SelectedItems.Clear();
                lvConnections.Items.Add(detail, true);
                lvConnections.Sort();

                if (connectionsList.Connections.All(d => d.ConnectionId != detail.ConnectionId) && !string.IsNullOrEmpty(detail.ConnectionName))
                {
                    connectionsList.Connections.Add(detail);
                    RefreshComboBoxSelectedConnection();
                }
            }
        }

        private async void tsbUpdateConnection_Click(object sender, EventArgs e)
        {
            if (lvConnections.SelectedItems.Count == 1)
            {
                ListViewItem item = lvConnections.SelectedItems[0];

                var cd = (ConnectionDetail)item.Tag;

                var cForm = new ConnectionWizard(crmRequests, cd)
                {
                    StartPosition = FormStartPosition.CenterParent
                };

                if (cForm.ShowDialog(this) == DialogResult.OK)
                {
                    item.UpdateWith(cForm.CrmConnectionDetail);
                    lvConnections.Refresh();

                    var updatedConnectionDetail = connectionsList.Connections.FirstOrDefault(c => c.ConnectionId == cForm.CrmConnectionDetail.ConnectionId);
                    connectionsList.Connections.Remove(updatedConnectionDetail);
                    connectionsList.Connections.Add(cForm.CrmConnectionDetail);

                    RefreshComboBoxSelectedConnection();
                }
            }
        }

        private void tsbUpdateSolution_Click(object sender, EventArgs e)
        {
            if (lvConnections.SelectedItems.Count == 1)
            {
                var item = lvConnections.SelectedItems[0];
                var cd = (ConnectionDetail)item.Tag;

                var upDialog = new UpdateSolutionForm(crmRequests, cd);
                var result = upDialog.ShowDialog();
                if (result == DialogResult.OK)
                {
                    item.UpdateWith(cd);
                    lvConnections.Refresh();

                    MessageBox.Show(this, @"Connection have been updated!", @"Information", MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                else if (result == DialogResult.Ignore)
                {
                    MessageBox.Show(this, @"No connection were updated!", @"Warning", MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
            }
        }
        public Func<Task> OnCreateMappingFile { get; set; }

        private void bCreateMappingClick(object sender, EventArgs e)
        {
            OnCreateMappingFile();
        }

        private void bAboutClick(object sender, EventArgs e)
        {
            var aboutForm = new AboutForm();
            aboutForm.ShowDialog();
        }
    }
}