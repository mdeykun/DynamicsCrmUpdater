using Cwru.Common;
using Cwru.Common.Model;
using Cwru.Common.Services;
using Cwru.CrmRequests.Common.Contracts;
using McTools.Xrm.Connection.WinForms.Extensions;
using McTools.Xrm.Connection.WinForms.Misc;
using McTools.Xrm.Connection.WinForms.Model;
using System;
using System.Linq;
using System.Windows.Forms;

namespace McTools.Xrm.Connection.WinForms
{
    public partial class ConnectionSelector : Form
    {
        private readonly Logger logger;
        private readonly ICrmRequests crmRequests;
        private readonly SolutionsService solutionsService;
        private readonly ProjectInfo projectInfo;
        private readonly MappingService mappingHelper;

        private ConnectionDetailsList connectionsList;

        public ConnectionSelector(
            Logger logger,
            ProjectInfo projectInfo,
            MappingService mappingHelper,
            ICrmRequests crmRequests,
            SolutionsService solutionsService,
            ConnectionDetailsList connectionsList)
        {
            InitializeComponent();

            this.crmRequests = crmRequests;
            this.solutionsService = solutionsService;
            this.connectionsList = connectionsList;
            this.mappingHelper = mappingHelper;
            this.logger = logger;

            this.projectInfo = projectInfo;

            cbAutoPublish.Checked = connectionsList.PublishAfterUpload;
            cbIgnoreExtensions.Checked = connectionsList.IgnoreExtensions;
            cbExtendedLog.Checked = connectionsList.ExtendedLog;

            DisplayConnections();
            RefreshComboBoxSelectedConnection();
            HideShowMenuItems();
        }

        public ConnectionDetailsList ConnectionsList => connectionsList;

        private void bCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private async void bSave_Click(object sender, EventArgs e)
        {
            var selectedConnection = comboBoxSelectedConnection.SelectedItem as ConnectionDetail;
            if (selectedConnection == null)
            {
                await logger.WriteLineAsync("Connection is not selected");
                MessageBox.Show("Connection is not selected", "Microsoft Dynamics CRM Web Resources Updater", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            connectionsList = new ConnectionDetailsList(lvConnections.Items.Cast<ListViewItem>().Select(x => x.Tag as ConnectionDetail))
            {
                PublishAfterUpload = cbAutoPublish.Checked,
                IgnoreExtensions = cbIgnoreExtensions.Checked,
                ExtendedLog = cbExtendedLog.Checked,
                SelectedConnectionId = selectedConnection?.ConnectionId
            };

            DialogResult = DialogResult.OK;
        }

        private void lvConnectionsColumn_Click(object sender, ColumnClickEventArgs e)
        {
            lvConnections.Sorting = lvConnections.Sorting == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;
            lvConnections.ListViewItemSorter = new ListViewItemComparer(e.Column, lvConnections.Sorting);
        }

        private void connectionSelector_KeyDown(object sender, KeyEventArgs e)
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

        private void lvConnections_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter || lvConnections.SelectedItems.Count == 0)
            {
                return;
            }

            bSave_Click(null, null);
        }

        private void lvConnections_SelectedIndexChanged(object sender, EventArgs e)
        {
            HideShowMenuItems();
        }

        private void lvConnections_AfterLabelEdit(object sender, LabelEditEventArgs e)
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

            var selectedConnections = lvConnections.SelectedItems.GetTagValues<ConnectionDetail>();
            selectedConnections.ForEach(connectionToDelete =>
            {
                connectionsList.Connections.RemoveAll(c => c.ConnectionId == connectionToDelete.ConnectionId);
            });

            DisplayConnections();
            RefreshComboBoxSelectedConnection();
        }

        private void tsbNewConnection_Click(object sender, EventArgs e)
        {
            var cForm = new ConnectionWizard(crmRequests, solutionsService)
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

        private void tsbUpdateConnection_Click(object sender, EventArgs e)
        {
            if (lvConnections.SelectedItems.Count == 1)
            {
                var item = lvConnections.SelectedItems[0];

                var cd = (ConnectionDetail)item.Tag;

                var cForm = new ConnectionWizard(crmRequests, solutionsService, cd)
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

                var dialog = new UpdateSolutionForm(solutionsService, cd);
                var result = dialog.ShowDialog();
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

        private async void bCreateMapping_Click(object sender, EventArgs e)
        {
            try
            {
                await mappingHelper.CreateMappingFileAsync(projectInfo);
                MessageBox.Show("UploaderMapping.config successfully created", "Microsoft Dynamics CRM Web Resources Updater", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                await logger.WriteLineAsync("Error occured during mapping file creation", ex);
                throw;
            }
        }

        private void bAboutClick(object sender, EventArgs e)
        {
            new AboutForm().ShowDialog();
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

        private void HideShowMenuItems()
        {
            tsbUpdateConnection.Visible = lvConnections.SelectedItems.Count == 1;
            tsbUpdateSolution.Visible = lvConnections.SelectedItems.Count == 1;
            tsbDeleteConnection.Visible = lvConnections.SelectedItems.Count == 1;
        }

        private void DisplayConnections()
        {
            lvConnections.Items.Clear();
            lvConnections.Groups.Clear();
            try
            {
                LoadImages();
                foreach (var detail in connectionsList.Connections)
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
            //lvConnections.SmallImageList.Images.Add(RessourceManager.GetImage("McTools.Xrm.Connection.WinForms.Resources.CRMOnlineLive_16.png"));
            //lvConnections.SmallImageList.Images.Add(RessourceManager.GetImage("McTools.Xrm.Connection.WinForms.Resources.server_key.png"));
            lvConnections.SmallImageList.Images.Add(RessourceManager.GetImage("McTools.Xrm.Connection.WinForms.Resources.powerapps16.png"));
        }
    }
}