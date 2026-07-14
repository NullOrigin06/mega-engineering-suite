using System;
using System.Threading;
using System.Threading.Tasks;

namespace MegaEngineeringSuite.Infrastructure.Cad
{
    public interface ISynchronizationProvider
    {
        void InitializeSynchronization();
        Task<bool> WaitForCompletionAsync(TimeSpan timeout, CancellationToken cancellationToken);
    }
}
