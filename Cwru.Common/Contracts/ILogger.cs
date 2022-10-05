using System;
using System.Threading.Tasks;

namespace Cwru.Common
{
    public interface ILogger
    {
        Task ClearAsync();
        Task WriteLineAsync(Exception ex, bool printStackTrace = false);
        Task WriteAsync(string message);
        Task WriteLineAsync(string message, Exception ex, bool printStackTrace = false);
        Task WriteLineAsync(string message, bool print = true);
        Task WriteLineWithTimeAsync(string message, bool print = true);
    }
}