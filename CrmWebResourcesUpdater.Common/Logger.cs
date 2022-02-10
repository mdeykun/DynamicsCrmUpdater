using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace CrmWebResourcesUpdater.Common
{
    public static class Logger
    {
        private static IVsOutputWindow _outputWindow;
        private static IVsOutputWindowPane _outputWindowPane;
        private static AsyncPackage _asyncPackage;

        /// <summary>
        /// Initialize Logger output window async
        /// </summary>
        public static async System.Threading.Tasks.Task InitializeAsync(AsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
             var svsOutputWindow = await package.GetServiceAsync(typeof(SVsOutputWindow));
            if (svsOutputWindow == null)
            {
                throw new InvalidOperationException("Failed to initialize output window");
            }
            _outputWindow = svsOutputWindow as IVsOutputWindow;
            var windowGuid = new Guid(ProjectGuids.OutputWindowGuidString);
            var windowTitle = "Crm Publisher";
            _asyncPackage = package;
            _outputWindow.CreatePane(ref windowGuid, windowTitle, 1, 1);
            _outputWindow.GetPane(ref windowGuid, out _outputWindowPane);
        }

        public static async System.Threading.Tasks.Task ClearAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            _outputWindowPane.Clear();
        }

        /// <summary>
        /// Adds line feed to message and writes it to output window
        /// </summary>
        /// <param name="message">text message to write</param>
        /// <param name="print">print or ignore call using for extended logging</param>
        public static async System.Threading.Tasks.Task WriteLineAsync(string message, bool print = true)
        {
            if(print)
            {
                await WriteAsync(message + "\r\n");
            }
        }

        /// <summary>
        /// Writes message to output window
        /// </summary>
        /// <param name="message">Text message to write</param>
        public static async Task WriteAsync(string message)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            _outputWindowPane.Activate();
            _outputWindowPane.OutputStringThreadSafe(message);

        }

        public static async System.Threading.Tasks.Task WriteLineWithTimeAsync(string message, bool print = true)
        {
            await WriteLineAsync(DateTime.Now.ToString("HH:mm") + ": " + message, print);
        }

        /// <summary>
        /// Adds line feed to message and writes it to output window
        /// </summary>
        /// <param name="message">text message to write</param>
        /// <param name="print">print or ignore call using for extended logging</param>
        public static void WriteLine(string message, bool print = true)
        {
            if (print)
            {
                Write(message + "\r\n");
            }
        }

        /// <summary>
        /// Writes message to output window
        /// </summary>
        /// <param name="message">Text message to write</param>
        public static void Write(string message)
        {
            var windowGuid = new Guid(ProjectGuids.OutputWindowGuidString);
            IVsOutputWindowPane pane;
            _outputWindow.GetPane(ref windowGuid, out pane);
            pane.Activate();
            pane.OutputString(message);
        }
    }
}
