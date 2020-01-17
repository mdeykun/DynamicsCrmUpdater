using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Microsoft.Xrm.Sdk.Client;
using EnvDTE;
using System.IO;

namespace McTools.Xrm.Connection.WinForms
{
    /// <summary>
    /// Formulaire de sélection d'une connexion à un serveur
    /// Crm dans une liste de connexions existantes.
    /// </summary>
    public partial class ConnectionSelector : Form
    {
        #region Variables

        /// <summary>
        /// Connexion sélectionnée
        /// </summary>
        List<ConnectionDetail> selectedConnections;

        /// <summary>
        /// Obtient la connexion sélectionnée
        /// </summary>
        public List<ConnectionDetail> SelectedConnections
        {
            get { return selectedConnections; }
        }

        private bool hadCreatedNewConnection;

        public CrmConnections ConnectionList { get; set; }

        private readonly ConnectionManager connectionManager;

        #endregion

        #region Constructeur

        /// <summary>
        /// Créé une nouvelle instance de la classe ConnectionSelector
        /// </summary>
        /// <param name="connections">Liste des connexions disponibles</param>
        /// <param name="cManager">Gestionnaire des connexions</param>

        public ConnectionSelector(CrmConnections connections, ConnectionManager cManager, ConnectionDetail selectedConnection = null, bool allowMultipleSelection = false, bool showPublisButton = false)
        {
            InitializeComponent();

            tsbDeleteConnection.Visible = true;
            tsbUpdateConnection.Visible = true;
            bCancel.Text = "Cancel";
            bValidate.Visible = true;

            bPublish.Visible = showPublisButton;
            

            connectionManager = cManager;
            connections.Connections.Sort();

            ConnectionList = connections;
            cbAutoPublish.Checked = connections.PublishAfterUpload;
            cbIgnoreExtensions.Checked = connections.IgnoreExtensions;
            cbExtendedLog.Checked = connections.ExtendedLog;

            SelectedConnection = selectedConnection;

            lvConnections.MultiSelect = allowMultipleSelection;

            LoadImages();

            foreach (ConnectionDetail detail in connections.Connections)
            {
                var item = new ListViewItem(detail.ConnectionName);
                item.SubItems.Add(detail.ServerName);
                item.SubItems.Add(detail.OrganizationFriendlyName);
                item.SubItems.Add(detail.OrganizationVersion);
                item.SubItems.Add(detail.SolutionFriendlyName);
                item.Tag = detail;
                item.Group = GetGroup(detail.AuthType);
                item.ImageIndex = GetImageIndex(detail);

                lvConnections.Items.Add(item);

            }

            RefreshComboBoxSelectedConnection();

            var groups = new ListViewGroup[lvConnections.Groups.Count];

            lvConnections.Groups.CopyTo(groups, 0);

            Array.Sort(groups, new GroupComparer());

            lvConnections.BeginUpdate();
            lvConnections.Groups.Clear();
            lvConnections.Groups.AddRange(groups);
            lvConnections.EndUpdate();

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
            if (SelectedConnection != null) {
                comboBoxSelectedConnection.SelectedIndex = ConnectionList.Connections.FindIndex(x => x.ConnectionId == SelectedConnection.ConnectionId);
            };
        }

        private void LoadImages()
        {
            lvConnections.SmallImageList = new ImageList();
            lvConnections.SmallImageList.Images.Add(RessourceManager.GetImage("McTools.Xrm.Connection.WinForms.Resources.server.png"));
            lvConnections.SmallImageList.Images.Add(RessourceManager.GetImage("McTools.Xrm.Connection.WinForms.Resources.server_key.png"));
            lvConnections.SmallImageList.Images.Add(RessourceManager.GetImage("McTools.Xrm.Connection.WinForms.Resources.CRMOnlineLive_16.png"));
        }

        #endregion

        #region Properties

        public bool HadCreatedNewConnection
        {
            get { return hadCreatedNewConnection; }
        }

        public ConnectionDetail SelectedConnection { get; set; }

        #endregion

        #region Méthodes

        private void LvConnectionsMouseDoubleClick(object sender, MouseEventArgs e)
        {
        }

        private void BValidateClick(object sender, EventArgs e)
        {
            ConnectionList.Connections = lvConnections.Items.Cast<ListViewItem>().Select(x => x.Tag as ConnectionDetail).ToList();
            ConnectionList.PublishAfterUpload = cbAutoPublish.Checked;
            ConnectionList.IgnoreExtensions = cbIgnoreExtensions.Checked;
            ConnectionList.ExtendedLog = cbExtendedLog.Checked;
            DialogResult = DialogResult.OK;
            Close();
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

        private void BCancelClick(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void LvConnectionsColumnClick(object sender, ColumnClickEventArgs e)
        {
            lvConnections.Sorting = lvConnections.Sorting == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;
            lvConnections.ListViewItemSorter = new ListViewItemComparer(e.Column, lvConnections.Sorting);
        }

        #endregion

        private void tsbNewConnection_Click(object sender, EventArgs e)
        {
            var cForm = new ConnectionForm(true, false)
            {
                ConnectionList = ConnectionList,
                StartPosition = FormStartPosition.CenterParent
            };

            if (cForm.ShowDialog(this) == DialogResult.OK)
            {
                var newConnection = cForm.CrmConnectionDetail;
                hadCreatedNewConnection = true;

                var item = new ListViewItem(newConnection.ConnectionName);
                item.SubItems.Add(newConnection.ServerName);
                item.SubItems.Add(newConnection.Organization);
                item.SubItems.Add(newConnection.OrganizationVersion);
                item.SubItems.Add(newConnection.SolutionFriendlyName);
                item.Tag = newConnection;
                item.Group = GetGroup(newConnection.AuthType);
                item.ImageIndex = GetImageIndex(newConnection);

                lvConnections.Items.Add(item);
                lvConnections.SelectedItems.Clear();
                item.Selected = true;

                if (!connectionManager.ConnectionsList.Connections.Contains(newConnection))
                    connectionManager.ConnectionsList.Connections.Add(newConnection);

                RefreshComboBoxSelectedConnection();

            }
        }

        private void tsbUpdateConnection_Click(object sender, EventArgs e)
        {
            if (lvConnections.SelectedItems.Count == 1)
            {
                ListViewItem item = lvConnections.SelectedItems[0];

                var cForm = new ConnectionForm(false, false)
                {
                    CrmConnectionDetail = (ConnectionDetail)item.Tag,
                    ConnectionList = ConnectionList,
                    StartPosition = FormStartPosition.CenterParent
                };

                if (cForm.ShowDialog(this) == DialogResult.OK)
                {
                    item.SubItems[0].Text = cForm.CrmConnectionDetail.ConnectionName;
                    item.SubItems[1].Text = cForm.CrmConnectionDetail.ServerName;
                    item.SubItems[2].Text = cForm.CrmConnectionDetail.OrganizationFriendlyName;
                    item.SubItems[3].Text = cForm.CrmConnectionDetail.OrganizationVersion;
                    item.SubItems[4].Text = cForm.CrmConnectionDetail.SolutionFriendlyName;

                    item.Group = GetGroup(cForm.CrmConnectionDetail.AuthType);

                    lvConnections.Refresh();

                    RefreshComboBoxSelectedConnection();
                }
                
            }
        }

        private void tsbDeleteConnection_Click(object sender, EventArgs e)
        {
            if (lvConnections.SelectedItems.Count > 0)
            {
                var connectionToDelete = lvConnections.SelectedItems[0].Tag as ConnectionDetail;
                if (SelectedConnection != null && SelectedConnection.ConnectionId == connectionToDelete.ConnectionId)
                {
                    SelectedConnection = null;
                }

                lvConnections.Items.Remove(lvConnections.SelectedItems[0]);


                ConnectionList.Connections = lvConnections.Items.Cast<ListViewItem>().Select(i => (ConnectionDetail)i.Tag).ToList();

                RefreshComboBoxSelectedConnection();
            }

        }

        private ListViewGroup GetGroup(AuthenticationProviderType type)
        {
            string groupName = string.Empty;

            switch (type)
            {
                case AuthenticationProviderType.ActiveDirectory:
                    groupName = "OnPremise";
                    break;
                case AuthenticationProviderType.OnlineFederation:
                    groupName = "CRM Online - Office 365";
                    break;
                case AuthenticationProviderType.LiveId:
                    groupName = "CRM Online - CTP";
                    break;
                case AuthenticationProviderType.Federation:
                    groupName = "Claims authentication - Internet Facing Deployment";
                    break;
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

            if (detail.UseOsdp)
            {
                return 2;
            }

            if (detail.UseIfd)
            {
                return 1;
            }

            return 0;
        }

        private void ComboBoxSelectedConnectionSelectedIndexChanged(object sender, EventArgs e)
        {
            SelectedConnection = comboBoxSelectedConnection.SelectedItem as ConnectionDetail;
        }

        public Action OnCreateMappingFile { get; set; }
        private void bCreateMappingClick(object sender, EventArgs e)
        {
            OnCreateMappingFile();
        }
    }
}