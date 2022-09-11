using CrmWebResourcesUpdater.Common.Config;
using CrmWebResourcesUpdater.Common.Model;
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
using Task = System.Threading.Tasks.Task;

namespace CrmWebResourcesUpdater.Common.Services
{
    public class VsDteService
    {
        private IAsyncServiceProvider serviceProvider;

        public VsDteService(AsyncPackage asyncPackage)
        {
            serviceProvider = asyncPackage;
        }

        public async Task<ProjectInfo> GetSelectedProjectInfoAsync()
        {
            var project = await GetSelectedProjectAsync();

            var projectInfo = new ProjectInfo()
            {
                Root = Path.GetDirectoryName(project.FullName).ToLower(),
                Guid = await GetProjectGuidAsync(project),
                Files = await ExtractProjectFilesAsync(ToEnumerable(project.ProjectItems)),
                SelectedFiles = await GetSelectedFilesAsync()
            };

            return projectInfo;
        }

        public OleMenuCommand GetMenuCommand(Guid comandSet, int commandID, EventHandler invokeHandler)
        {
            CommandID menuCommandID = new CommandID(comandSet, commandID);
            return new OleMenuCommand(invokeHandler, menuCommandID);
        }

        public DialogResult ShowErrorDialog()
        {
            var title = "Configuration error";
            var text = "It seems that Publisher has not been configured yet or connection is not selected.\r\n\r\n" +
            "We can open configuration window for you now or you can do it later by clicking \"Publish options\" in the context menu of the project.\r\n\r\n" +
            "Do you want to open configuration window now?";
            return MessageBox.Show(text, title, MessageBoxButtons.YesNo, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
        }

        public async Task SetStatusBarAsync(string message, object icon = null)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var svsStatusbar = await serviceProvider.GetServiceAsync(typeof(SVsStatusbar));
            if (svsStatusbar == null)
            {
                await Logger.WriteLineAsync("Failed to access status bar");
                return;
            }
            var statusBar = svsStatusbar as IVsStatusbar;
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

        public async Task SaveAllAsync()
        {
            var dte = await GetDte();
            dte.ExecuteCommand("File.SaveAll");
        }

        private async Task<Project> GetSelectedProjectAsync()
        {
            var dte = await GetDte();

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            //UIHierarchyItem uiHierarchyItem = Enumerable.FirstOrDefault<UIHierarchyItem>(Enumerable.OfType<UIHierarchyItem>((IEnumerable)(object[])dte.ToolWindows.SolutionExplorer.SelectedItems));

            var selectedItems = (object[])dte.ToolWindows.SolutionExplorer.SelectedItems;
            var uiHierarchyItem = Enumerable.FirstOrDefault<UIHierarchyItem>(Enumerable.OfType<UIHierarchyItem>((IEnumerable)selectedItems));

            //Microsoft.VisualStudio.PlatformUI.UIHierarchyMarshaler


            var project = uiHierarchyItem.Object as Project;
            if (project != null)
            {
                return project;
            }
            var item = uiHierarchyItem.Object as ProjectItem;
            if (item != null)
            {
                return item.ContainingProject;
            }

            Document doc = dte.ActiveDocument;
            if (doc != null && doc.ProjectItem != null)
            {
                return doc.ProjectItem.ContainingProject;
            }
            return null;
        }

        private async Task<IEnumerable<ProjectItem>> GetSelectedItemsAsync()
        {
            var dte = await GetDte();

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
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
            if (items.Count == 0)
            {
                Document doc = dte.ActiveDocument;
                if (doc != null && doc.ProjectItem != null)
                {
                    items.Add(doc.ProjectItem);
                }
            }
            return items;
        }

        private async Task<IEnumerable<string>> ExtractProjectFilesAsync(IEnumerable<ProjectItem> list)
        {
            if (list == null)
            {
                return null;
            }

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var files = new List<string>();
            foreach (ProjectItem item in list)
            {
                if (item.Kind.ToLower() == Consts.FileKindGuid.ToLower())
                {
                    var path = Path.GetDirectoryName(item.FileNames[0]).ToLower();
                    var fileName = Path.GetFileName(item.FileNames[0]);
                    files.Add(path + "\\" + fileName);
                }

                if (item.ProjectItems != null)
                {
                    var childItems = await ExtractProjectFilesAsync(ToEnumerable(item.ProjectItems));
                    files.AddRange(childItems);
                }
            }

            return files;
        }

        public async Task AddFromFileAsync(Guid projectGuid, string filePath)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var project = await GetProjectByGuidAsync(projectGuid);
            project.ProjectItems.AddFromFile(filePath);
        }

        private async Task<IEnumerable<string>> GetSelectedFilesAsync()
        {
            var selectedItems = await GetSelectedItemsAsync();
            return await ExtractProjectFilesAsync(selectedItems);
        }

        private async Task<Project> GetProjectByGuidAsync(Guid projectGuid)
        {
            var svsSolution = await serviceProvider.GetServiceAsync(typeof(SVsSolution));
            if (svsSolution == null)
            {
                throw new InvalidOperationException("Failed to load project guid");
            }
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var solution = svsSolution as IVsSolution;

            IVsHierarchy hierarchy;
            solution.GetProjectOfGuid(ref projectGuid, out hierarchy);

            var project = hierarchy as Project;
            if (project != null)
            {
                return project;
            }
            return null;
        }

        private async Task<Guid> GetProjectGuidAsync(Project project)
        {
            Guid projectGuid = Guid.Empty;
            IVsHierarchy hierarchy;
            var svsSolution = await serviceProvider.GetServiceAsync(typeof(SVsSolution));
            if (svsSolution == null)
            {
                throw new InvalidOperationException("Failed to load project guid");
            }
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var solution = svsSolution as IVsSolution;
            solution.GetProjectOfUniqueName(project.FullName, out hierarchy);
            if (hierarchy != null)
            {
                solution.GetGuidOfProject(hierarchy, out projectGuid);
            }
            return projectGuid;
        }

        private static IEnumerable<ProjectItem> ToEnumerable(ProjectItems projectItems)
        {
            var list = new List<ProjectItem>();
            foreach (ProjectItem item in projectItems)
            {
                list.Add(item);
            }

            return list;
        }

        private async Task<EnvDTE80.DTE2> GetDte()
        {
            var dte = await serviceProvider.GetServiceAsync(typeof(DTE)) as EnvDTE80.DTE2;
            if (dte == null)
            {
                await Logger.WriteLineAsync("Failed to get DTE service.");
                throw new Exception("Failed to get DTE service.");
            }

            return dte;
        }

        //public async Task<string> GetProjectRootAsync(Project project)
        //{
        //    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        //    return Path.GetDirectoryName(project.FullName).ToLower();
        //}

        //private async Task<string> GetSelectedFilePathAsync()
        //{
        //    var files = await GetSelectedFilesAsync();
        //    return files.FirstOrDefault();
        //}

        //public async Task<IEnumerable<string>> GetProjectFilesAsync()
        //{
        //    var selectedProject = await GetSelectedProjectAsync();
        //    if (selectedProject == null)
        //    {
        //        return null;
        //    }
        //    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        //    var projectFiles = await GetProjectFilesAsync(selectedProject.ProjectItems);
        //    return projectFiles;
        //}
    }
}
