using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace Cwru.Common
{
    public class Logger : ILogger
    {
        public static readonly Guid OutputWindowGuid = new Guid("10B2DB3C-1CB4-43B4-80D4-A03204A616D4");

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

        public async Task WriteAsync(string message)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var pane = await GetOutputPaneAsync();
            pane.Activate();
            pane.OutputStringThreadSafe(message);
        }

        public async Task WriteLineAsync()
        {
            await WriteAsync("\r\n");
        }

        public async Task WriteLineAsync(Exception ex, bool printStackTrace = false)
        {
            await WriteLineAsync("An error occured: " + ex.Message);
            await WriteLineAsync(ex.StackTrace, printStackTrace);
        }

        public async Task WriteLineAsync(string message, Exception ex, bool printStackTrace = false)
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                await WriteLineAsync(message);
            }

            await WriteLineAsync(ex, printStackTrace);
        }

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
            var windowGuid = OutputWindowGuid;
            var windowTitle = "Crm Publisher";

            outputWindow.CreatePane(ref windowGuid, windowTitle, 1, 1);
            outputWindow.GetPane(ref windowGuid, out outputWindowPane);
        }
    }
}
