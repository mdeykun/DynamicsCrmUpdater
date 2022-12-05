using Cwru.Common.Extensions;
using Cwru.Common.Model;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IAsyncServiceProvider = Microsoft.VisualStudio.Shell.IAsyncServiceProvider;
using Task = System.Threading.Tasks.Task;

namespace Cwru.Common.Services
{
    public class VsDteService
    {
        private readonly IAsyncServiceProvider serviceProvider;
        private readonly Logger logger;

        public VsDteService(AsyncPackage asyncPackage, Logger logger)
        {
            this.serviceProvider = asyncPackage;
            this.logger = logger;
        }

        public async Task<ProjectInfo> GetSelectedProjectInfoAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var project = await GetSelectedProjectAsync();
            if (project == null)
            {
                return null;
            }

            var projectGuid = await GetProjectGuidAsync(project);
            if (projectGuid == Guid.Empty)
            {
                throw new Exception("Project guid can't be retrived");
            }

            var selectedItems = await GetSelectedElementsAsync();
            var elements = await GetSolutionElementsRecursiveAsync(project, selectedItems);

            return new ProjectInfo()
            {
                Root = Path.GetDirectoryName(project.FullName).ToLower(),
                Guid = projectGuid,
                Elements = elements,
                ElementsFlat = GetElementsFlatList(elements),
                SelectedElements = GetElementsFlatList(selectedItems)
            };
        }
        public async Task SetStatusBarAsync(string message, object icon = null)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var statusBar = await GetStatusBarServiceAsync();

            statusBar.IsFrozen(out var frozen);
            if (frozen == 0)
            {
                if (icon != null)
                {
                    statusBar.Animation(1, ref icon);
                }
                statusBar.SetText(message);
            }
        }
        public async Task SaveAllAsync()
        {
            var dte = await GetDteServiceAsync();
            dte.ExecuteCommand("File.SaveAll");
        }
        public async Task AddFromFileAsync(Guid projectGuid, string filePath)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var project = await GetProjectByGuidAsync(projectGuid);
            if (project == null)
            {
                throw new InvalidOperationException("Failed to load project by guid");
            }

            project.ProjectItems.AddFromFile(filePath);
        }
        public async Task OpenFileAndPlaceContentAsync(Guid projectGuid, string filePath, string content)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var project = await GetProjectByGuidAsync(projectGuid);
            if (project == null)
            {
                throw new InvalidOperationException("Failed to load project by guid");
            }

            var projectItem = await GetProjectItemByFileNameAsync(project, filePath);
            if (projectItem == null)
            {
                return;
            }

            projectItem.Open(EnvDTE.Constants.vsViewKindCode);
            projectItem.Document.ActiveWindow.Activate();

            var textDocument = (TextDocument)projectItem.Document.Object("TextDocument");

            var startPoint = textDocument.CreateEditPoint(textDocument.StartPoint);
            startPoint.Delete(textDocument.EndPoint);
            startPoint.Insert(content);
        }
        private async Task<Project> GetSelectedProjectAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var selectedItems = await GetSelectedHierarchyItemsAsync();
            var selectedItem = selectedItems.FirstOrDefault();

            var project = selectedItem?.Object as Project;
            if (project != null)
            {
                return project;
            }

            var item = selectedItem?.Object as ProjectItem;
            if (item != null)
            {
                return item.ContainingProject;
            }

            return null;
        }
        private async Task<IEnumerable<SolutionElement>> GetSolutionElementsRecursiveAsync(Project project, IEnumerable<SolutionElement> selectedItems)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            if (project == null)
            {
                return Enumerable.Empty<SolutionElement>();
            }

            return await GetSolutionElementsRecursiveAsync(ToEnumerable(project.ProjectItems), selectedItems);
        }
        private async Task<IEnumerable<SolutionElement>> GetSolutionElementsRecursiveAsync(IEnumerable<ProjectItem> itemsToCheck, IEnumerable<SolutionElement> selected)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            if (itemsToCheck == null || itemsToCheck.Count() == 0)
            {
                return Enumerable.Empty<SolutionElement>();
            }

            var result = new List<SolutionElement>();

            foreach (var item in itemsToCheck)
            {
                var solutionElement = await GetSolutionElementAsync(item);
                if (solutionElement == null)
                {
                    continue;
                }

                //if (selected != null && selected.Any(x => x.FilePath.IsEqualToLower(solutionElement.FilePath)))
                //{
                //    solutionElement.IsSelected = true;
                //}

                if (item.ProjectItems != null)
                {
                    var childItems = await GetSolutionElementsRecursiveAsync(ToEnumerable(item.ProjectItems), selected);
                    solutionElement.Childs.AddRange(childItems);
                }

                result.Add(solutionElement);
            }

            return result;
        }
        private async Task<IEnumerable<SolutionElement>> GetSelectedElementsAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var selectedItems = await GetSelectedHierarchyItemsAsync();
            var elements = new List<SolutionElement>();

            foreach (var uiItem in selectedItems)
            {
                var element = await GetSolutionElementAsync(uiItem.Object as ProjectItem);
                if (element == null)
                {
                    continue;
                }

                elements.Add(element);
            }

            return elements;
        }
        private async Task<ProjectItem> GetProjectItemByFileNameAsync(Project project, string filePath)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            if (project == null)
            {
                return null;
            }

            return await GetProjectItemByFileNameAsync(ToEnumerable(project.ProjectItems), filePath);
        }
        private async Task<ProjectItem> GetProjectItemByFileNameAsync(IEnumerable<ProjectItem> itemsToCheck, string filePath)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            if (itemsToCheck == null || itemsToCheck.Count() == 0)
            {
                return null;
            }

            foreach (var item in itemsToCheck)
            {
                var currentItemfilePath = await GetPathAsync(item);
                if (currentItemfilePath != null && currentItemfilePath.IsEqualToLower(filePath))
                {
                    return item;
                }

                if (item.ProjectItems != null)
                {
                    var foundItem = await GetProjectItemByFileNameAsync(ToEnumerable(item.ProjectItems), filePath);
                    if (foundItem != null)
                    {
                        return foundItem;
                    }
                }
            }

            return null;
        }
        private async Task<Project> GetProjectByGuidAsync(Guid projectGuid)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var solution = await GetSolutionServiceAsync();
            var result = solution.GetProjectOfGuid(projectGuid, out IVsHierarchy hierarchy);
            if (!ErrorHandler.Succeeded(result))
            {
                return null;
            }

            result = hierarchy.GetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_ExtObject, out object obj);
            if (!ErrorHandler.Succeeded(result))
            {
                return null;
            }

            var project = obj as Project;
            if (project == null)
            {
                return null;
            }

            return project;
        }
        private async Task<Guid> GetProjectGuidAsync(Project project)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var solution = await GetSolutionServiceAsync();
            var result = solution.GetProjectOfUniqueName(project.FullName, out IVsHierarchy hierarchy);
            if (!ErrorHandler.Succeeded(result) || hierarchy == null)
            {
                return Guid.Empty;
            }

            result = solution.GetGuidOfProject(hierarchy, out Guid projectGuid);
            if (!ErrorHandler.Succeeded(result))
            {
                return Guid.Empty;
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
        private async Task<IEnumerable<UIHierarchyItem>> GetSelectedHierarchyItemsAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var dte = await GetDteServiceAsync();
            var selectedItems = (object[])dte.ToolWindows.SolutionExplorer.SelectedItems;
            var result = selectedItems != null ? selectedItems.AsEnumerable().OfType<UIHierarchyItem>() : Enumerable.Empty<UIHierarchyItem>();

            return result;
        }

        private EnvDTE80.DTE2 dte = null;
        private async Task<EnvDTE80.DTE2> GetDteServiceAsync()
        {
            if (dte != null)
            {
                return dte;
            }

            dte = await serviceProvider.GetServiceAsync(typeof(DTE)) as EnvDTE80.DTE2;
            if (dte == null)
            {
                await logger.WriteLineAsync("Failed to get DTE service.");
                throw new Exception("Failed to get DTE service.");
            }

            return dte;
        }

        private IVsStatusbar statusBar = null;
        private async Task<IVsStatusbar> GetStatusBarServiceAsync()
        {
            if (statusBar != null)
            {
                return statusBar;
            }

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            statusBar = await serviceProvider.GetServiceAsync(typeof(SVsStatusbar)) as IVsStatusbar;
            if (statusBar == null)
            {
                await logger.WriteLineAsync("Failed to access status bar service");

            }

            return statusBar;
        }

        private IVsSolution solution = null;
        private async Task<IVsSolution> GetSolutionServiceAsync()
        {
            if (solution != null)
            {
                return solution;
            }

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            solution = await serviceProvider.GetServiceAsync(typeof(SVsSolution)) as IVsSolution;
            if (solution == null)
            {
                throw new InvalidOperationException("Failed to load solution service");
            }

            return solution;
        }
        private static async Task<SolutionElement> GetSolutionElementAsync(ProjectItem item)
        {
            if (item == null)
            {
                return null;
            }

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            SolutionElementType? type = null;
            if (item.Kind.IsEqualToLower(EnvDTE.Constants.vsProjectItemKindPhysicalFile))
            {
                type = SolutionElementType.File;
            }
            else if (item.Kind.IsEqualToLower(EnvDTE.Constants.vsProjectItemKindPhysicalFolder))
            {
                type = SolutionElementType.Folder;
            }
            else
            {
                return null;
            }

            var filePath = await GetPathAsync(item);
            if (filePath == null)
            {
                return null;
            }

            var solutionElement = new SolutionElement
            {
                FilePath = filePath,
                Type = type.Value
            };

            return solutionElement;
        }
        private static IEnumerable<SolutionElement> GetElementsFlatList(IEnumerable<SolutionElement> solutionElements)
        {
            var result = new List<SolutionElement>();
            if (solutionElements == null || !solutionElements.Any())
            {
                return result;
            }

            foreach (var element in solutionElements)
            {
                result.Add(element);
                result.AddRange(GetElementsFlatList(element.Childs));
            }

            return result;
        }
        private static async Task<string> GetPathAsync(ProjectItem item)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            if (item == null)
            {
                return null;
            }

            return item.FileNames[0]?.ToLower();
        }
    }
}
