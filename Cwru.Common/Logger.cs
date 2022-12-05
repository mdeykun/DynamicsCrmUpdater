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
        public static readonly Guid OutputWindowGuid = new Guid("10B2DB3C-1CB4-43B4-80D4-A03204A616D4");

        private readonly ToolConfig toolConfig = null;
        private IVsOutputWindow outputWindow;
        private IVsOutputWindowPane outputWindowPane;


        private readonly Microsoft.VisualStudio.Shell.IAsyncServiceProvider serviceProvider;

        public Logger(AsyncPackage asyncPackage, ToolConfig toolConfig = null)
        {
            this.serviceProvider = asyncPackage;
            this.toolConfig = toolConfig;
        }

        public async Task ClearAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var pane = await GetOutputPaneAsync();
            pane.Clear();
        }

        public async Task WriteDebugAsync(string message)
        {
            if (toolConfig?.ExtendedLog == true)
            {
                await WriteLineAsync(message);
            }
        }

        public async Task WriteDebugAsync(Exception ex)
        {
            if (toolConfig?.ExtendedLog == true)
            {
                await WriteLineAsync(ex);
            }
        }

        public async Task WriteDebugAsync(string message, Exception ex)
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                await WriteDebugAsync(message);
            }

            await WriteDebugAsync(ex);
        }

        public async Task WriteLineAsync()
        {
            await WriteAsync("\r\n");
        }

        public async Task WriteLineAsync(Exception ex)
        {
            await WriteLineAsync("An error occured: " + ex.Message);
            await WriteLineAsync(ex.StackTrace);
        }

        public async Task WriteLineAsync(string message, Exception ex)
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                await WriteLineAsync(message);
            }

            await WriteLineAsync(ex);
        }

        public async Task WriteLineAsync(string message)
        {
            await WriteAsync(message + "\r\n");
        }

        public async Task WriteLineWithTimeAsync(string message)
        {
            await WriteLineAsync(DateTime.Now.ToString("HH:mm") + ": " + message);
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

        private async Task WriteAsync(string message)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var pane = await GetOutputPaneAsync();
            pane.Activate();
            pane.OutputStringThreadSafe(message);
        }
    }
}
