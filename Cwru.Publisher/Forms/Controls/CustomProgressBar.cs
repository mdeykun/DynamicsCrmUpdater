using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Cwru.Publisher.Forms.Controls
{
    public class CustomProgressBar
    {
        private readonly ProgressBar progressBar;
        private CancellationTokenSource cancellationTokenSource;

        public CustomProgressBar(ProgressBar progressBar)
        {
            this.progressBar = progressBar;
            this.progressBar.Maximum = 1000;
            this.progressBar.Step = 1;
        }

        public void Show()
        {
            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();
                cancellationTokenSource.Dispose();
                cancellationTokenSource = null;
                progressBar.Value = 0;
            }

            var progress = new Progress<int>(v =>
            {
                // This lambda is executed in context of UI thread,
                // so it can safely update form controls
                progressBar.Value = v;
            });

            cancellationTokenSource = new CancellationTokenSource();
            var ct = cancellationTokenSource.Token;

            var task = Task.Run(() =>
            {
                var step = 1;
                while (!ct.IsCancellationRequested)
                {
                    ((IProgress<int>)progress).Report(step);
                    step = step < 1000 ? step + 1 : 1;
                    Thread.Sleep(10);
                }

                ((IProgress<int>)progress).Report(0);

            }, ct);
        }

        public void Hide()
        {
            if (cancellationTokenSource == null)
            {
                return;
            }

            progressBar.Value = 0;
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
            cancellationTokenSource = null;
        }
    }
}
