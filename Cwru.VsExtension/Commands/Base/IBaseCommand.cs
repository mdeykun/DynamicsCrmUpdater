using System.Threading.Tasks;

namespace Cwru.VsExtension.Commands.Base
{
    internal interface IBaseCommand
    {
        Task ExecuteAsync();
    }
}
