using System;
using System.Threading.Tasks;

namespace Cwru.Common
{
    public interface ILogger
    {
        Task ClearAsync();

        Task WriteLineAsync();
        Task WriteLineAsync(string message);
        Task WriteLineAsync(Exception ex);
        Task WriteLineAsync(string message, Exception ex);

        Task WriteLineWithTimeAsync(string message);

        Task WriteDebugAsync(string message);
        Task WriteDebugAsync(Exception ex);
        Task WriteDebugAsync(string message, Exception ex);
    }
}