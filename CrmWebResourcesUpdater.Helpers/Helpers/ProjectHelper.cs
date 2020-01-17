using CrmWebResourcesUpdater.Common;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Threading;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using IAsyncServiceProvider = Microsoft.VisualStudio.Shell.IAsyncServiceProvider;

namespace CrmWebResourcesUpdater.Helpers
{
    public class ProjectHelper
    {

        private IAsyncServiceProvider _serviceProvider;
        private JoinableTaskFactory _joinableTaskFactory;
        private Dictionary<Guid, Settings> _settingsCache = new Dictionary<Guid, Settings>();

        public ProjectHelper(AsyncPackage asyncPackage): this(asyncPackage, asyncPackage.JoinableTaskFactory) {}
        public ProjectHelper(IAsyncServiceProvider serviceProvider, JoinableTaskFactory joinableTaskFactory)
        {
            _serviceProvider = serviceProvider;
            _joinableTaskFactory =  joinableTaskFactory;
        }

        public string GetProjectRoot(Project project)
        {
            return Path.GetDirectoryName(project.FullName).ToLower();
        }


        /// <summary>
        /// Creates context menu command for extension
        /// </summary>
        /// <param name="comandSet">Guid for command set in context menu</param>
        /// <param name="commandID">Guid for command</param>
        /// <param name="invokeHandler">Handler for menu command</param>
        /// <returns>Returns context menu command</returns>
        public OleMenuCommand GetMenuCommand(Guid comandSet, int commandID, EventHandler invokeHandler)
        {
            CommandID menuCommandID = new CommandID(comandSet, commandID);
            return new OleMenuCommand(invokeHandler, menuCommandID);
        }

        /// <summary>
        /// Shows Configuration Error Dialog
        /// </summary>
        /// <returns>Returns result of an error dialog</returns>
        public DialogResult ShowErrorDialog()
        {
            var title = "Configuration error";
            var text = "It seems that Publisher has not been configured yet or connection is not selected.\r\n\r\n" +
            "We can open configuration window for you now or you can do it later by clicking \"Publish options\" in the context menu of the project.\r\n\r\n" +
            "Do you want to open configuration window now?";
            return MessageBox.Show(text, title, MessageBoxButtons.YesNo, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
        }

        /// <summary>
        /// Gets Guid of project
        /// </summary>
        /// <param name="project">Project to get guid of</param>
        /// <returns>Returns project guid</returns>
        public async Task<Guid> GetProjectGuidAsync(Project project)
        {
            Guid projectGuid = Guid.Empty;
            IVsHierarchy hierarchy;
            IVsSolution solution = await _serviceProvider.GetServiceAsync(typeof(SVsSolution)) as IVsSolution;
            solution.GetProjectOfUniqueName(project.FullName, out hierarchy);
            if (hierarchy != null)
            {
                solution.GetGuidOfProject(hierarchy, out projectGuid);
            }
            return projectGuid;
        }



        /// <summary>
        /// Gets selected project
        /// </summary>
        /// <returns>Returns selected project</returns>
        public async Task<Project> GetSelectedProjectAsync()
        {
            var dte = await _serviceProvider.GetServiceAsync(typeof(DTE)) as EnvDTE80.DTE2;
            if (dte == null)
            {
                Logger.WriteLine("Failed to get DTE service.");
                throw new Exception("Failed to get DTE service.");
            }
            UIHierarchyItem uiHierarchyItem = Enumerable.FirstOrDefault<UIHierarchyItem>(Enumerable.OfType<UIHierarchyItem>((IEnumerable)(object[])dte.ToolWindows.SolutionExplorer.SelectedItems));
            var project = uiHierarchyItem.Object as Project;
            if (project != null)
            {
                return project;
            }
            var item = uiHierarchyItem.Object as ProjectItem;
            if(item != null)
            {
                return item.ContainingProject;
            }

            Document doc = dte.ActiveDocument;
            if(doc != null && doc.ProjectItem != null)
            {
                return doc.ProjectItem.ContainingProject;
            }
            return null;
        }



        public async System.Threading.Tasks.Task SetStatusBarAsync(string message, object icon = null)
        {
            var statusBar = await _serviceProvider.GetServiceAsync(typeof(SVsStatusbar)) as IVsStatusbar;
            int frozen;
            statusBar.IsFrozen(out frozen);
            if (frozen == 0)
            {


                //object icon = (short)Microsoft.VisualStudio.Shell.Interop.Constants.SBAI_Deploy;
                if (icon != null)
                {
                    statusBar.Animation(1, ref icon);
                }
                //
                statusBar.SetText(message);
            }
        }

        public async System.Threading.Tasks.Task SaveAllAsync()
        {
            var dte = await _serviceProvider.GetServiceAsync(typeof(DTE)) as EnvDTE80.DTE2;
            if (dte == null)
            {
                Logger.WriteLine("Failed to get DTE service.");
                throw new Exception("Failed to get DTE service.");
            }
            dte.ExecuteCommand("File.SaveAll");
        }

        /// <summary>
        /// Gets items selected in solution explorer
        /// </summary>
        /// <returns>Returns list of items which was selected in solution explorer</returns>
        public async Task<List<ProjectItem>> GetSelectedItemsAsync()
        {
            var dte = await _serviceProvider.GetServiceAsync(typeof(DTE)) as EnvDTE80.DTE2;
            if (dte == null)
            {
                Logger.WriteLine("Failed to get DTE service.");
                throw new Exception("Failed to get DTE service.");
            }

            var uiHierarchyItems = Enumerable.OfType<UIHierarchyItem>((IEnumerable)(object[])dte.ToolWindows.SolutionExplorer.SelectedItems);
            var items = new List<ProjectItem>();
            foreach (var uiItem in uiHierarchyItems)
            {
                var item = uiItem.Object as ProjectItem;
                if (item != null)
                {
                    items.Add(item);
                }
            }
            if(items.Count == 0)
            {
                Document doc = dte.ActiveDocument;
                if (doc != null && doc.ProjectItem != null)
                {
                    items.Add(doc.ProjectItem);
                }
            }
            return items;
        }

        /// <summary>
        /// Iterates through ProjectItems list and adds files paths to the output list
        /// </summary>
        /// <param name="list">List of project items</param>
        public List<string> GetProjectFiles(List<ProjectItem> list)
        {
            if(list == null)
            {
                return null;
            }

            var files = new List<string>();
            foreach (ProjectItem item in list)
            {
                if (item.Kind.ToLower() == Settings.FileKindGuid.ToLower())
                {
                    var path = Path.GetDirectoryName(item.FileNames[0]).ToLower();
                    var fileName = Path.GetFileName(item.FileNames[0]);
                    files.Add(path + "\\" + fileName);
                }

                if (item.ProjectItems != null)
                {
                    var childItems = GetProjectFiles(item.ProjectItems);
                    files.AddRange(childItems);
                }
            }

            return files;
        }

        public List<string> GetProjectFiles(Project project)
        {
            return GetProjectFiles(project.ProjectItems);
        }

        /// <summary>
        /// Iterates through ProjectItems tree and adds files paths to the list
        /// </summary>
        /// <param name="projectItems">List of project items</param>
        public List<string> GetProjectFiles(ProjectItems projectItems)
        {
            var list = new List<ProjectItem>();
            foreach (ProjectItem item in projectItems)
            {
                list.Add(item);
            }
            var projectFiles = GetProjectFiles(list);
            return projectFiles;
        }

        public async Task<List<string>> GetSelectedFilesAsync()
        {
            var selectedItems = await GetSelectedItemsAsync();
            return GetProjectFiles(selectedItems);
        }

        public async Task<string> GetSelectedFilePathAsync()
        {
            var files = await GetSelectedFilesAsync();
            return files.FirstOrDefault();
        }

        

        public async Task<List<string>> GetProjectFilesAsync()
        {
            var selectedProject = await GetSelectedProjectAsync();
            if(selectedProject == null)
            {
                return null;
            }
            var projectFiles = GetProjectFiles(selectedProject.ProjectItems);
            return projectFiles;
        }
    }
}
