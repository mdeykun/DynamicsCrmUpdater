using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Threading.Tasks;

namespace CrmWebResourcesUpdater.Common
{
    public static class Logger
    {
        private static IVsOutputWindow _outputWindow;
        private static IVsOutputWindowPane _outputWindowPane;
        private static AsyncPackage _asyncPackage;

        /// <summary>
        /// Initialize Logger output window
        /// </summary>
        public static void Initialize()
        {
            _outputWindow = AsyncPackage.GetGlobalService(typeof(SVsOutputWindow)) as IVsOutputWindow;
            var windowGuid = new Guid(ProjectGuids.OutputWindowGuidString);
            var windowTitle = "Crm Publisher";
            _outputWindow.CreatePane(ref windowGuid, windowTitle, 1, 1);
        }

        /// <summary>
        /// Initialize Logger output window async
        /// </summary>
        public static async System.Threading.Tasks.Task InitializeAsync(AsyncPackage package)
        {
            _outputWindow = await package.GetServiceAsync(typeof(SVsOutputWindow)) as IVsOutputWindow;
            var windowGuid = new Guid(ProjectGuids.OutputWindowGuidString);
            var windowTitle = "Crm Publisher";
            _asyncPackage = package;
            _outputWindow.CreatePane(ref windowGuid, windowTitle, 1, 1);
            _outputWindow.GetPane(ref windowGuid, out _outputWindowPane);
        }

        public static async System.Threading.Tasks.Task ClearAsync()
        {
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
        public static async System.Threading.Tasks.Task WriteAsync(string message)
        {
            _outputWindowPane.Activate();
            _outputWindowPane.OutputStringThreadSafe(message);

        }

        public static async System.Threading.Tasks.Task WriteLineWithTimeAsync(string message, bool print = true)
        {
            await WriteLineAsync(DateTime.Now.ToString("HH:mm") + ": " + message, print);
        }

        public static void Clear()
        {
            var windowGuid = new Guid(ProjectGuids.OutputWindowGuidString);
            IVsOutputWindowPane pane;
            _outputWindow.GetPane(ref windowGuid, out pane);
            pane.Clear();
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

        public static void WriteLineWithTime(string message, bool print = true)
        {
            WriteLine(DateTime.Now.ToString("HH:mm") + ": " + message, print);
        }
    }
}
