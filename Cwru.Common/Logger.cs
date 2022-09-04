using Cwru.Common.Config;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace Cwru.Common
{
    public class Logger : ILogger
    {
        private IVsOutputWindow outputWindow;
        private IVsOutputWindowPane outputWindowPane;

        private readonly Microsoft.VisualStudio.Shell.IAsyncServiceProvider serviceProvider;

        public Logger(AsyncPackage asyncPackage)
        {
            this.serviceProvider = asyncPackage;
        }

        public async Task ClearAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var pane = await GetOutputPaneAsync();
            pane.Clear();
        }

        /// <summary>
        /// Writes message to output window
        /// </summary>
        /// <param name="message">Text message to write</param>
        public async Task WriteAsync(string message)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var pane = await GetOutputPaneAsync();
            pane.Activate();
            pane.OutputStringThreadSafe(message);
        }

        /// <summary>
        /// Writes message to output window
        /// </summary>
        /// <param name="message">Text message to write</param>
        public async Task WriteAsync(Exception ex)
        {
            await WriteLineAsync("An error occured: " + ex.Message + "\r\n" + ex.StackTrace);
        }

        /// <summary>
        /// Writes message to output window
        /// </summary>
        /// <param name="message">Text message to write</param>
        public async Task WriteAsync(string message, Exception ex)
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                await WriteLineAsync(message);
            }

            await WriteLineAsync("An error occured: " + ex.Message + "\r\n" + ex.StackTrace);
        }

        /// <summary>
        /// Adds line feed to message and writes it to output window
        /// </summary>
        /// <param name="message">text message to write</param>
        /// <param name="print">print or ignore call using for extended logging</param>
        public async Task WriteLineAsync(string message, bool print = true)
        {
            if (print)
            {
                await WriteAsync(message + "\r\n");
            }
        }

        public async Task WriteLineWithTimeAsync(string message, bool print = true)
        {
            await WriteLineAsync(DateTime.Now.ToString("HH:mm") + ": " + message, print);
        }

        ///// <summary>
        ///// Writes message to output window
        ///// </summary>
        ///// <param name="message">Text message to write</param>
        //public async Task Write(string message)
        //{
        //    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

        //    var windowGuid = new Guid(ProjectGuids.OutputWindowGuidString);
        //    IVsOutputWindowPane pane;
        //    (await GetOutputAsync()).GetPane(ref windowGuid, out pane);
        //    pane.Activate();
        //    pane.OutputString(message);
        //}

        //private async Task<IVsOutputWindow> GetOutputAsync()
        //{
        //    if (outputWindow != null)
        //    {
        //        return outputWindow;
        //    }

        //    await SetOutputsAsync();

        //    return outputWindow;
        //}

        private async Task<IVsOutputWindowPane> GetOutputPaneAsync()
        {
            if (outputWindowPane != null)
            {
                return outputWindowPane;
            }

            await SetOutputsAsync();

            return outputWindowPane;
        }

        private async Task SetOutputsAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var svsOutputWindow = await serviceProvider.GetServiceAsync(typeof(SVsOutputWindow));
            if (svsOutputWindow == null)
            {
                throw new InvalidOperationException("Failed to initialize output window");
            }
            outputWindow = svsOutputWindow as IVsOutputWindow;
            var windowGuid = new Guid(Consts.OutputWindowGuidString);
            var windowTitle = "Crm Publisher";

            outputWindow.CreatePane(ref windowGuid, windowTitle, 1, 1);
            outputWindow.GetPane(ref windowGuid, out outputWindowPane);
        }
    }
}
