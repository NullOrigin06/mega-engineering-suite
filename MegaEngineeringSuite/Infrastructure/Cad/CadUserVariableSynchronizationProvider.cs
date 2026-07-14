using System;
using System.Threading;
using System.Threading.Tasks;
using MegaEngineeringSuite.Infrastructure.Logging;

namespace MegaEngineeringSuite.Infrastructure.Cad
{
    public class CadUserVariableSynchronizationProvider : ISynchronizationProvider
    {
        private readonly ICadAdapter _cadAdapter;
        private readonly string _variableName;

        public CadUserVariableSynchronizationProvider(ICadAdapter cadAdapter, string variableName = "USERI1")
        {
            _cadAdapter = cadAdapter;
            _variableName = variableName;
        }

        public void InitializeSynchronization()
        {
            _cadAdapter.SetSystemVariable(_variableName, 0);
            SimpleLogger.Log("SyncProvider", $"Initialized '{_variableName}' to 0.");
        }

        public async Task<bool> WaitForCompletionAsync(TimeSpan timeout, CancellationToken cancellationToken)
        {
            using var timeoutCts = new CancellationTokenSource(timeout);
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);
            
            try
            {
                while (true)
                {
                    linkedCts.Token.ThrowIfCancellationRequested();

                    object varObj = _cadAdapter.GetSystemVariable(_variableName);
                    object cmdActiveObj = _cadAdapter.GetSystemVariable("CMDACTIVE");
                    int status = Convert.ToInt32(varObj);
                    int cmdActive = Convert.ToInt32(cmdActiveObj);

                    if (status == 1 && cmdActive == 0)
                    {
                        SimpleLogger.Log("SyncProvider", "Success: LISP reported completion (1) and CAD is idle (CMDACTIVE=0).");
                        return true;
                    }
                    if (status == -1)
                    {
                        SimpleLogger.Log("SyncProvider", "Failure: LISP reported an error (-1).");
                        return false;
                    }

                    await Task.Delay(500, linkedCts.Token);
                }
            }
            catch (OperationCanceledException)
            {
                if (timeoutCts.IsCancellationRequested)
                {
                    SimpleLogger.Log("SyncProvider", $"Timeout: Waited {timeout.TotalSeconds} seconds but LISP did not complete.");
                    return false; // Timeout is treated as a failure for the pipeline.
                }
                throw; // Actual cancellation requested by the caller.
            }
        }
    }
}
