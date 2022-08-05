using CrmWebResourcesUpdater.Service.Client;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace McTools.Xrm.Connection.WinForms
{
    /// <summary>
    /// Formulaire de sélection d'une connexion à un serveur
    /// Crm dans une liste de connexions existantes.
    /// </summary>
    public partial class ConnectionSelector : Form
    {
        #region Variables

        private readonly bool isConnectionSelection;
        //private int currentIndex;
        private bool hadCreatedNewConnection;

        /// <summary>
        /// Obtient la connexion sélectionnée
        /// </summary>
        public List<ConnectionDetail> SelectedConnections { get; private set; }

        private readonly CrmWebResourcesUpdaterClient crmWebResourceUpdaterClient;

        #endregion Variables

        #region Constructeur

        /// <summary>
        /// Créé une nouvelle instance de la classe ConnectionSelector
        /// </summary>
        public ConnectionSelector(bool isConnectionSelection = true)
        {
            InitializeComponent();

            this.isConnectionSelection = isConnectionSelection;

            if (isConnectionSelection)
            {
                bValidate.Text = @"Connect";
            }

            crmWebResourceUpdaterClient = CrmWebResourcesUpdaterClient.Instance;
        }

        public ConnectionSelector(CrmConnections connections, ConnectionDetail selectedConnection = null, bool allowMultipleSelection = false, bool showPublisButton = false)
        {
            InitializeComponent();

            tsbDeleteConnection.Visible = true;
            tsbUpdateConnection.Visible = true;
            bCancel.Text = "Cancel";
            bValidate.Visible = true;

            bPublish.Visible = showPublisButton;


            connections.Connections.Sort();
            ConnectionManager.Instance.ConnectionsList = connections;
            ConnectionList = connections;
            cbAutoPublish.Checked = connections.PublishAfterUpload;
            cbIgnoreExtensions.Checked = connections.IgnoreExtensions;
            cbExtendedLog.Checked = connections.ExtendedLog;

            SelectedConnection = selectedConnection;

            lvConnections.MultiSelect = allowMultipleSelection;

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

                var details = ConnectionManager.Instance.ConnectionsList.Connections;
                if (ConnectionManager.Instance.ConnectionsList.UseMruDisplay)
                {
                    details = ConnectionManager.Instance.ConnectionsList.Connections.OrderByDescending(c => c.LastUsedOn).ThenBy(c => c.ConnectionName).ToList();
                }

                foreach (ConnectionDetail detail in details)
                {
                    var item = new ListViewItem(detail.ConnectionName);
                    item.SubItems.Add(detail.ServerName == null ? detail.OriginalUrl : detail.ServerName);
                    item.SubItems.Add(detail.Organization);
                    item.SubItems.Add(string.IsNullOrEmpty(detail.UserDomain) ? detail.UserName : $"{detail.UserDomain}\\{detail.UserName}");
                    item.SubItems.Add(detail.OrganizationVersion);
                    item.SubItems.Add(detail.SolutionName);
                    item.Tag = detail;
                    item.ImageIndex = GetImageIndex(detail);

                    if (!ConnectionManager.Instance.ConnectionsList.UseMruDisplay)
                    {
                        item.Group = GetGroup(detail);
                    }

                    lvConnections.Items.Add(item);
                }

                if (!ConnectionManager.Instance.ConnectionsList.UseMruDisplay)
                {
                    var groups = new ListViewGroup[lvConnections.Groups.Count];

                    lvConnections.Groups.CopyTo(groups, 0);

                    Array.Sort(groups, new GroupComparer());

                    lvConnections.BeginUpdate();
                    lvConnections.Groups.Clear();
                    lvConnections.Groups.AddRange(groups);
                    lvConnections.EndUpdate();
                }

                tsbNewConnection.Enabled = !ConnectionManager.Instance.ConnectionsList.IsReadOnly;
                tsbUpdateConnection.Enabled = !ConnectionManager.Instance.ConnectionsList.IsReadOnly;
                //tsbCloneConnection.Enabled = !ConnectionManager.Instance.ConnectionsList.IsReadOnly;
                tsbDeleteConnection.Enabled = !ConnectionManager.Instance.ConnectionsList.IsReadOnly;
                //tsbUpdatePassword.Enabled = !ConnectionManager.Instance.ConnectionsList.IsReadOnly;
                //tsb_UseMru.Enabled = !ConnectionManager.Instance.ConnectionsList.IsReadOnly;
            }
            catch(Exception ex)
            {
                MessageBox.Show($"Failed to display connections: {ex.Message}\r\n{ex.StackTrace}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadConnectionFile()
        {
            //tsb_UseMru.Checked = ConnectionManager.Instance.ConnectionsList.UseMruDisplay;
            //tsb_UseMru.CheckedChanged += tsb_UseMru_CheckedChanged;

            if (isConnectionSelection)
            {
                Text = @"Select a connection";
                tsbDeleteConnection.Visible = false;
                tsbUpdateConnection.Visible = false;
                //tsbCloneConnection.Visible = false;
                //tsbRemoveConnectionList.Visible = false;
                tsbUpdateSolution.Visible = false;
                bCancel.Text = @"Cancel";
                bValidate.Visible = true;
            }
            else
            {
                Text = @"Connections Manager";
                tsbDeleteConnection.Visible = true;
                tsbUpdateConnection.Visible = true;
                //tsbCloneConnection.Visible = true;
                tsbUpdateSolution.Visible = true;
                //tsbRemoveConnectionList.Visible = true;
                bCancel.Text = @"Close";
                bValidate.Visible = false;
            }

            DisplayConnections();
        }

        private void LoadImages()
        {
            lvConnections.SmallImageList = new ImageList();
            lvConnections.SmallImageList.Images.Add(RessourceManager.GetImage("McTools.Xrm.Connection.WinForms.Resources.CRMOnlineLive_16.png"));
            lvConnections.SmallImageList.Images.Add(RessourceManager.GetImage("McTools.Xrm.Connection.WinForms.Resources.server_key.png"));
            lvConnections.SmallImageList.Images.Add(RessourceManager.GetImage("McTools.Xrm.Connection.WinForms.Resources.powerapps16.png"));
        }

        #endregion Constructeur

        #region Properties

        public bool HadCreatedNewConnection => hadCreatedNewConnection;

        public ConnectionDetail SelectedConnection { get; set; }

        #endregion Properties

        #region Méthodes

        private void BCancelClick(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void BValidateClick(object sender, EventArgs e)
        {
            ConnectionList.Connections = lvConnections.Items.Cast<ListViewItem>().Select(x => x.Tag as ConnectionDetail).ToList();
            ConnectionList.PublishAfterUpload = cbAutoPublish.Checked;
            ConnectionList.IgnoreExtensions = cbIgnoreExtensions.Checked;
            ConnectionList.ExtendedLog = cbExtendedLog.Checked;
            DialogResult = DialogResult.OK;
        }

        private void bPublishClick(object sender, EventArgs e)
        {
            ConnectionList.Connections = lvConnections.Items.Cast<ListViewItem>().Select(x => x.Tag as ConnectionDetail).ToList();
            ConnectionList.PublishAfterUpload = cbAutoPublish.Checked;
            ConnectionList.IgnoreExtensions = cbIgnoreExtensions.Checked;
            ConnectionList.ExtendedLog = cbExtendedLog.Checked;
            DialogResult = DialogResult.Yes;
            Close();
        }

        private void LvConnectionsColumnClick(object sender, ColumnClickEventArgs e)
        {
            lvConnections.Sorting = lvConnections.Sorting == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;
            lvConnections.ListViewItemSorter = new ListViewItemComparer(e.Column, lvConnections.Sorting);
        }

        private void LvConnectionsMouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (isConnectionSelection)
            {
                BValidateClick(sender, e);
            }
            else if (!ConnectionManager.Instance.ConnectionsList.IsReadOnly)
            {
                tsbUpdateConnection_Click(sender, null);
            }
        }

        #endregion Méthodes

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
            if (ConnectionList == null || ConnectionList.Connections == null)
            {
                return;
            }

            foreach (ConnectionDetail detail in ConnectionList.Connections)
            {
                comboBoxSelectedConnection.Items.Add(detail);
            }
            if (SelectedConnection != null)
            {
                comboBoxSelectedConnection.SelectedIndex = ConnectionList.Connections.FindIndex(x => x.ConnectionId == SelectedConnection.ConnectionId);
            };
        }

        private void ConnectionSelector_Load(object sender, EventArgs e)
        {
            //var mostRecentFile = ConnectionsList.Instance.Files.OrderByDescending(f => f.LastUsed).FirstOrDefault();
            //var index = 0;
            //var indexToSelect = 0;

            //tsbRemoveConnectionList.Enabled = false;
            //currentIndex = 0;

            //if (mostRecentFile != null)
            //{
            //    foreach (var file in ConnectionsList.Instance.Files.OrderBy(k => k.Name))
            //    {
            //        if (file.Name == mostRecentFile.Name)
            //        {
            //            indexToSelect = index;
            //        }
            //        else if (!Uri.IsWellFormedUriString(file.Path, UriKind.Absolute))
            //        {
            //            tsbMoveToExistingFile.DropDownItems.Add(new ToolStripButton
            //            {
            //                Text = file.Name,
            //                Tag = file,
            //                Size = new Size(155, 22),
            //                AutoSize = true
            //            });
            //        }

            //        tscbbConnectionsFile.Items.Add(file);
            //        index++;
            //    }
            //}

            //tscbbConnectionsFile.Items.Add("<Create new connection file>");
            //tscbbConnectionsFile.Items.Add("<Add an existing connection file>");
            //tscbbConnectionsFile.SelectedIndexChanged -= tscbbConnectionsFile_SelectedIndexChanged;
            //tscbbConnectionsFile.SelectedIndex = indexToSelect;
            //tscbbConnectionsFile.SelectedIndexChanged += tscbbConnectionsFile_SelectedIndexChanged;

            //if (tscbbConnectionsFile.SelectedItem != null && !ConnectionManager.FileExists(((ConnectionFile)tscbbConnectionsFile.SelectedItem).Path))
            //{
            //    CleanFileList((ConnectionFile)tscbbConnectionsFile.SelectedItem);
            //    return;
            //}

            //tsbMoveToExistingFile.Enabled = tscbbConnectionsFile.Items.Count > 3;
            //tsbRemoveConnectionList.Enabled = tscbbConnectionsFile.Items.Count > 3;

            // Display connections
            //LoadConnectionFile();

            //lvConnections_SelectedIndexChanged(this, new EventArgs());
        }

        private ListViewGroup GetGroup(ConnectionDetail detail)
        {
            string groupName;

            if (detail.UseOnline)
            {
                groupName = "Online";
            }
            else if (detail.UseIfd)
            {
                groupName = "Claims authentication - Internet Facing Deployment";
            }
            else
            {
                groupName = "OnPremise";
            }

            var group = lvConnections.Groups.Cast<ListViewGroup>().FirstOrDefault(g => g.Name == groupName);
            if (group == null)
            {
                group = new ListViewGroup(groupName, groupName);
                lvConnections.Groups.Add(group);
            }

            return group;
        }

        private int GetImageIndex(ConnectionDetail detail)
        {
            if (detail.UseOnline)
            {
                return 2;
            }

            if (detail.UseIfd)
            {
                return 1;
            }

            return 0;
        }

        private void lvConnections_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter || lvConnections.SelectedItems.Count == 0)
                return;

            BValidateClick(null, null);
        }

        private void lvConnections_SelectedIndexChanged(object sender, EventArgs e)
        {
            bValidate.Enabled = lvConnections.SelectedItems.Count > 0;
            //tsbShowConnectionString.Visible = lvConnections.SelectedItems.Count == 1;
            //toolStripSeparator4.Visible = lvConnections.SelectedItems.Count == 1;

            tsbUpdateConnection.Visible = lvConnections.SelectedItems.Count == 1;

            //tsbCloneConnection.Visible = lvConnections.SelectedItems.Count > 0;
            //tsbDeleteConnection.Visible = lvConnections.SelectedItems.Count > 0;
            //tsbUpdatePassword.Visible = lvConnections.SelectedItems.Count > 0;
            //toolStripSeparator3.Visible = lvConnections.SelectedItems.Count > 0;

            //tsbMoveToExistingFile.Visible = lvConnections.SelectedItems.Count > 0;
            //tsbMoveToNewFile.Visible = lvConnections.SelectedItems.Count > 0;

            tsbUpdateSolution.Visible = lvConnections.SelectedItems.Count == 1;
        }

        private void tsb_UseMru_CheckedChanged(object sender, EventArgs e)
        {
            var tsb = (ToolStripButton)sender;
            ConnectionManager.Instance.ConnectionsList.UseMruDisplay = tsb.Checked;

            DisplayConnections();
        }

        private void tscbbConnectionsFile_SelectedIndexChanged(object sender, EventArgs e)
        {
            //var cbbValue = tscbbConnectionsFile.SelectedItem;
            //var connectionFile = cbbValue as ConnectionFile;
            //bool loadConnections = true;

            //// If null, then we selected an action rather than a connection file
            //if (connectionFile == null)
            //{
            //    tscbbConnectionsFile.SelectedIndexChanged -= tscbbConnectionsFile_SelectedIndexChanged;
            //    tscbbConnectionsFile.SelectedIndex = currentIndex;
            //    tscbbConnectionsFile.SelectedIndexChanged += tscbbConnectionsFile_SelectedIndexChanged;

            //    // It can be a new file
            //    if (cbbValue.ToString() == "<Create new connection file>")
            //    {
            //        var nfd = new NewConnectionFileDialog();
            //        if (nfd.ShowDialog(this) == DialogResult.OK)
            //        {
            //            ConnectionManager.ConfigurationFile = nfd.CreatedFilePath;

            //            var newIndex = tscbbConnectionsFile.Items.Count - 2;

            //            tscbbConnectionsFile.Items.Insert(newIndex,
            //                ConnectionsList.Instance.Files.First(f => f.Path == nfd.CreatedFilePath));
            //            tscbbConnectionsFile.SelectedIndex = newIndex;
            //            tsbRemoveConnectionList.Enabled = true;

            //            tsbMoveToExistingFile.Enabled = tscbbConnectionsFile.Items.Count > 3;
            //        }
            //        else
            //        {
            //            loadConnections = false;
            //        }
            //    }
            //    // Or an existing file
            //    else
            //    {
            //        var afd = new AddConnectionFileDialog();
            //        if (afd.ShowDialog(this) == DialogResult.OK)
            //        {
            //            var newIndex = tscbbConnectionsFile.Items.Count - 2;

            //            tscbbConnectionsFile.Items.Insert(newIndex, afd.OpenedFile);
            //            tscbbConnectionsFile.SelectedIndex = newIndex;
            //            tsbRemoveConnectionList.Enabled = true;
            //        }
            //        else
            //        {
            //            loadConnections = false;
            //        }
            //    }
            //}
            //else
            //{
            //    if (!ConnectionManager.FileExists(connectionFile.Path))
            //    {
            //        CleanFileList(connectionFile);
            //        return;
            //    }

            //    currentIndex = tscbbConnectionsFile.SelectedIndex;

            //    // Or it is a connection file so we load it for the connection manager
            //    ConnectionManager.ConfigurationFile = connectionFile.Path;

            //    tsbRemoveConnectionList.Enabled = tscbbConnectionsFile.Items.Count > 3;

            //    connectionFile.LastUsed = DateTime.Now;

            //    tsbMoveToExistingFile.DropDownItems.Clear();
            //    foreach (var file in ConnectionsList.Instance.Files.OrderBy(k => k.Name))
            //    {
            //        if (connectionFile.Path == file.Path || Uri.IsWellFormedUriString(file.Path, UriKind.Absolute))
            //        {
            //            continue;
            //        }

            //        tsbMoveToExistingFile.DropDownItems.Add(new ToolStripButton
            //        {
            //            Text = file.Name,
            //            Tag = file,
            //            Size = new Size(155, 22),
            //            AutoSize = true
            //        });
            //    }
            //}

            //if (loadConnections)
            //{
            //    LoadConnectionFile();
            //}

            //ConnectionsList.Instance.Save();
        }

        #region Connection actions

        private void LvConnections_AfterLabelEdit(object sender, LabelEditEventArgs e)
        {
            if (!e.CancelEdit)
            {
                var detail = (ConnectionDetail)lvConnections.Items[e.Item].Tag;
                detail.ConnectionName = e.Label;

            }
        }

        private void tsbCloneConnection_Click(object sender, EventArgs e)
        {
            var newItems = new List<ListViewItem>();

            foreach (ListViewItem item in lvConnections.SelectedItems)
            {
                var detail = (ConnectionDetail)item.Tag;

                var newDetail = ConnectionManager.Instance.ConnectionsList.CloneConnection(detail);

                var newItem = new ListViewItem(newDetail.ConnectionName);
                newItem.SubItems.Add(newDetail.ServerName);
                newItem.SubItems.Add(newDetail.Organization);
                newItem.SubItems.Add(newDetail.OrganizationVersion);
                newItem.Tag = newDetail;
                newItem.Group = GetGroup(newDetail);
                newItem.ImageIndex = GetImageIndex(newDetail);

                newItems.Add(newItem);
            }

            lvConnections.Items.AddRange(newItems.ToArray());
        }

        private void tsbDeleteConnection_Click(object sender, EventArgs e)
        {
            if (lvConnections.SelectedItems.Count > 0 &&
                MessageBox.Show(this, @"Are you sure you want to delete selected connection(s)?", @"Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                return;

            foreach (ListViewItem connectionItem in lvConnections.SelectedItems)
            {
                var detailToRemove = (ConnectionDetail)connectionItem.Tag;

                lvConnections.Items.Remove(lvConnections.SelectedItems[0]);

                ConnectionManager.Instance.ConnectionsList.Connections.RemoveAll(d => d.ConnectionId == detailToRemove.ConnectionId);
            }

        }

        private void tsbNewConnection_Click(object sender, EventArgs e)
        {
            var cForm = new ConnectionWizard2
            {
                StartPosition = FormStartPosition.CenterParent
            };

            if (cForm.ShowDialog(this) == DialogResult.OK)
            {
                var newConnection = cForm.CrmConnectionDetail;
                hadCreatedNewConnection = true;

                var item = new ListViewItem(newConnection.ConnectionName);
                item.SubItems.Add(newConnection.ServerName);
                item.SubItems.Add(newConnection.Organization);
                item.SubItems.Add(newConnection.UserName);
                item.SubItems.Add(newConnection.OrganizationVersion);
                item.SubItems.Add(newConnection.SolutionName);
                item.Tag = newConnection;
                item.Group = GetGroup(newConnection);
                item.ImageIndex = GetImageIndex(newConnection);

                lvConnections.Items.Add(item);
                lvConnections.SelectedItems.Clear();
                item.Selected = true;

                lvConnections.Sort();

                if (isConnectionSelection)
                {
                    BValidateClick(sender, e);
                }

                // If the connection id is not found and the user want to save
                // the connection (ie. he provided a name for the connection)
                if (ConnectionManager.Instance.ConnectionsList.Connections.FirstOrDefault(d => d.ConnectionId == newConnection.ConnectionId) == null
                             && !string.IsNullOrEmpty(newConnection.ConnectionName))
                {
                    ConnectionManager.Instance.ConnectionsList.Connections.Add(newConnection);
                    RefreshComboBoxSelectedConnection();
                }
            }
        }

        private void tsbShowConnectionString_Click(object sender, EventArgs e)
        {
            var connections = lvConnections.SelectedItems
                .Cast<ListViewItem>().Select(lvi => (ConnectionDetail)lvi.Tag)
                .ToList();

            if (connections.Count == 1)
            {
                var csd = new ConnectionStringDialog(connections.First());
                csd.ShowDialog(this);
            }
        }

        private async void tsbUpdateConnection_Click(object sender, EventArgs e)
        {
            if (lvConnections.SelectedItems.Count == 1)
            {
                ListViewItem item = lvConnections.SelectedItems[0];

                var cd = (ConnectionDetail)item.Tag;

                if (cd.IsFromSdkLoginCtrl)
                {
                    var connectionDetail = await crmWebResourceUpdaterClient.UseSdkLoginControlAsync(cd.ConnectionId.Value, true);
                    if(connectionDetail != null)
                    {
                        cd = connectionDetail;

                        item.Tag = cd;
                        lvConnections.Items.Remove(item);
                        lvConnections.Items.Add(item);
                        lvConnections.Refresh();//RedrawItems(0, lvConnections.Items.Count - 1, false);

                        var updatedConnectionDetail = ConnectionManager.Instance.ConnectionsList.Connections.FirstOrDefault(
                            c => c.ConnectionId == cd.ConnectionId);

                        ConnectionManager.Instance.ConnectionsList.Connections.Remove(updatedConnectionDetail);
                        ConnectionManager.Instance.ConnectionsList.Connections.Add(cd);

                        RefreshComboBoxSelectedConnection();
                    }
                    //var ctrl = new CRMLoginForm1(cd.ConnectionId.Value, true);
                    //ctrl.ShowDialog();

                    //if (ctrl.CrmConnectionMgr.CrmSvc?.IsReady ?? false)
                    //{
                    //    cd.Organization = ctrl.CrmConnectionMgr.ConnectedOrgUniqueName;
                    //    cd.OrganizationFriendlyName = ctrl.CrmConnectionMgr.ConnectedOrgFriendlyName;
                    //    cd.OrganizationDataServiceUrl =
                    //        ctrl.CrmConnectionMgr.ConnectedOrgPublishedEndpoints[EndpointType.OrganizationDataService];
                    //    cd.OrganizationServiceUrl =
                    //        ctrl.CrmConnectionMgr.ConnectedOrgPublishedEndpoints[EndpointType.OrganizationService];
                    //    cd.WebApplicationUrl =
                    //        ctrl.CrmConnectionMgr.ConnectedOrgPublishedEndpoints[EndpointType.WebApplication];
                    //    cd.ServerName = new Uri(cd.WebApplicationUrl).Host;
                    //    cd.OrganizationVersion = ctrl.CrmConnectionMgr.CrmSvc.ConnectedOrgVersion.ToString();


                    //    item.Tag = cd;
                    //    lvConnections.Items.Remove(item);
                    //    lvConnections.Items.Add(item);
                    //    lvConnections.Refresh();//RedrawItems(0, lvConnections.Items.Count - 1, false);

                    //    var updatedConnectionDetail = ConnectionManager.Instance.ConnectionsList.Connections.FirstOrDefault(
                    //        c => c.ConnectionId == cd.ConnectionId);

                    //    ConnectionManager.Instance.ConnectionsList.Connections.Remove(updatedConnectionDetail);
                    //    ConnectionManager.Instance.ConnectionsList.Connections.Add(cd);

                    //    RefreshComboBoxSelectedConnection();
                    //}

                    return;
                }

                var cForm = new ConnectionWizard2(cd)
                {
                    StartPosition = FormStartPosition.CenterParent
                };

                if (cForm.ShowDialog(this) == DialogResult.OK)
                {
                    item.Tag = cForm.CrmConnectionDetail;
                    item.SubItems[0].Text = cForm.CrmConnectionDetail.ConnectionName;
                    item.SubItems[1].Text = cForm.CrmConnectionDetail.ServerName;
                    item.SubItems[2].Text = cForm.CrmConnectionDetail.Organization;
                    if (item.SubItems.Count == 4)
                    {
                        item.SubItems[3].Text = cForm.CrmConnectionDetail.OrganizationVersion;
                    }
                    else
                    {
                        item.SubItems.Add(cForm.CrmConnectionDetail.OrganizationVersion);
                    }
                    item.Group = GetGroup(cForm.CrmConnectionDetail);
                    item.SubItems[5].Text = cForm.CrmConnectionDetail.SolutionName;

                    lvConnections.Refresh();

                    var updatedConnectionDetail = ConnectionManager.Instance.ConnectionsList.Connections.FirstOrDefault(
                            c => c.ConnectionId == cForm.CrmConnectionDetail.ConnectionId);

                    ConnectionManager.Instance.ConnectionsList.Connections.Remove(updatedConnectionDetail);
                    ConnectionManager.Instance.ConnectionsList.Connections.Add(cForm.CrmConnectionDetail);

                    RefreshComboBoxSelectedConnection();
                }
            }
        }

        private void tsbUpdatePassword_Click(object sender, EventArgs e)
        {
            var connections = lvConnections.SelectedItems
                .Cast<ListViewItem>().Select(lvi => (ConnectionDetail)lvi.Tag)
                .ToList();

            if (!connections.Any())
            {
                return;
            }

            var upDialog = new UpdatePasswordForm(connections);
            var result = upDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                MessageBox.Show(this, @"Connections have been updated!", @"Information", MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            else if (result == DialogResult.Ignore)
            {
                MessageBox.Show(this, @"No connection were updated!", @"Warning", MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        #endregion Connection actions

        #region Connection file actions

        private void CleanFileList(ConnectionFile connectionFile)
        {
            //var message = $"The file '{connectionFile.Path}' does not exist and will be removed from the list!";
            //MessageBox.Show(this, message, @"Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);

            //var index = tscbbConnectionsFile.SelectedIndex != 0 ? tscbbConnectionsFile.SelectedIndex - 1 : 0;
            //ConnectionsList.Instance.Files.Remove(connectionFile);
            //tscbbConnectionsFile.Items.Remove(tscbbConnectionsFile.SelectedItem);
            //tscbbConnectionsFile.SelectedIndex = index;
        }

        private void tsbMoveToExistingFile_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            var connectionsIds =
                lvConnections.SelectedItems.Cast<ListViewItem>().Select(i => ((ConnectionDetail)i.Tag).ConnectionId);

            var currentFilePath = ConnectionManager.ConfigurationFile;
            var newFilePath = ((ConnectionFile)e.ClickedItem.Tag).Path;

            var currentDoc = new XmlDocument();
            currentDoc.Load(currentFilePath);
            var newDoc = new XmlDocument();
            newDoc.Load(newFilePath);

            foreach (var connectionId in connectionsIds)
            {
                var nodeToMove = currentDoc.SelectSingleNode("//ConnectionDetail[ConnectionId/text()='" + connectionId + "']");
                if (nodeToMove == null) continue;

                newDoc.SelectSingleNode("//Connections")?.AppendChild(newDoc.ImportNode(nodeToMove, true));
                currentDoc.SelectSingleNode("//Connections")?.RemoveChild(nodeToMove);
            }

            currentDoc.Save(currentFilePath);
            newDoc.Save(newFilePath);
            LoadConnectionFile();
        }

        private void tsbMoveToNewFile_Click(object sender, EventArgs e)
        {
            //bool loadConnections = true;

            //var nfd = new NewConnectionFileDialog();
            //if (nfd.ShowDialog(this) == DialogResult.OK)
            //{
            //    var scs = lvConnections.SelectedItems.Cast<ListViewItem>().Select(i => (ConnectionDetail)i.Tag).ToList();
            //    foreach (var sc in scs)
            //    {
            //        ConnectionManager.Instance.ConnectionsList.Connections.Remove(sc);
            //    }

            //    ConnectionManager.Instance.SaveConnectionsFile();

            //    ConnectionManager.ConfigurationFile = nfd.CreatedFilePath;

            //    var newIndex = tscbbConnectionsFile.Items.Count - 2;

            //    tscbbConnectionsFile.Items.Insert(newIndex, ConnectionsList.Instance.Files.First(f => f.Path == nfd.CreatedFilePath));
            //    tscbbConnectionsFile.SelectedIndex = newIndex;
            //    tsbRemoveConnectionList.Enabled = true;

            //    ConnectionManager.Instance.ConnectionsList.Connections = scs;
            //    ConnectionManager.Instance.SaveConnectionsFile();
            //}
            //else
            //{
            //    loadConnections = false;
            //}

            //if (loadConnections)
            //{
            //    LoadConnectionFile();
            //}

            //ConnectionsList.Instance.Save();

            //tsbMoveToExistingFile.Enabled = tscbbConnectionsFile.Items.Count > 3;
        }

        private void tsbRemoveConnectionList_Click(object sender, EventArgs e)
        {
            //var message = "Are you sure you want to remove this connections list file?\n\nThe file is not deleted physically but just removed from connections files list";
            //if (MessageBox.Show(this, message, @"Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question) ==
            //    DialogResult.No)
            //    return;

            //var item = (ConnectionFile)tscbbConnectionsFile.SelectedItem;
            //tscbbConnectionsFile.Items.RemoveAt(tscbbConnectionsFile.SelectedIndex);
            //ConnectionsList.Instance.Files.Remove(item);
            //ConnectionsList.Instance.Save();
            //tscbbConnectionsFile.SelectedIndex = tscbbConnectionsFile.Items.Count - 3;

            //tsbMoveToExistingFile.Enabled = tscbbConnectionsFile.Items.Count > 3;
        }

        private void tsbRenameFile_Click(object sender, EventArgs e)
        {
            //var index = tscbbConnectionsFile.SelectedIndex;
            //var item = (ConnectionFile)tscbbConnectionsFile.SelectedItem;
            //var dialog = new RenameConnectionFileDialog(item.Name ?? "N/A");
            //if (dialog.ShowDialog() == DialogResult.OK)
            //{
            //    item.Name = dialog.NewName;
            //    ConnectionsList.Instance.Save();

            //    tscbbConnectionsFile.Items.Remove(item);
            //    tscbbConnectionsFile.Items.Insert(index, item);
            //    tscbbConnectionsFile.SelectedIndex = index;
            //}
        }

        #endregion Connection file actions

        private void tsbUpdateSolution_Click(object sender, EventArgs e)
        {
            if (lvConnections.SelectedItems.Count == 1)
            {
                ListViewItem item = lvConnections.SelectedItems[0];
                var cd = (ConnectionDetail)item.Tag;

                var upDialog = new UpdateSolutionForm(cd);
                var result = upDialog.ShowDialog();
                if (result == DialogResult.OK)
                {

                    item.Tag = cd;
                    item.SubItems[0].Text = cd.ConnectionName;
                    item.SubItems[1].Text = cd.ServerName;
                    item.SubItems[2].Text = cd.Organization;
                    if (item.SubItems.Count == 4)
                    {
                        item.SubItems[3].Text = cd.OrganizationVersion;
                    }
                    else
                    {
                        item.SubItems.Add(cd.OrganizationVersion);
                    }
                    item.SubItems[5].Text = cd.SolutionName;

                    item.Group = GetGroup(cd);
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

        private void ComboBoxSelectedConnectionSelectedIndexChanged(object sender, EventArgs e)
        {
            SelectedConnection = comboBoxSelectedConnection.SelectedItem as ConnectionDetail;
        }
        public Func<Task> OnCreateMappingFile { get; set; }
        private void bCreateMappingClick(object sender, EventArgs e)
        {
            OnCreateMappingFile();
        }

        public CrmConnections ConnectionList { get; set; }

        private void button1_Click(object sender, EventArgs e)
        {
            var aboutForm = new AboutForm();
            aboutForm.ShowDialog();
        }
    }
}