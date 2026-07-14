using System.Text;

namespace MegaEngineeringSuite.TubeSheet
{
    public static class PipelineSynchronizationEmitter
    {
        public static void AppendSynchronizationSignal(StringBuilder lspContent)
        {
            if (AppConfigManager.Current.UsePipelineV2)
            {
                lspContent.AppendLine("; --- PIPELINE V2 SYNCHRONIZATION ---");
                lspContent.AppendLine("(setvar \"USERI1\" 1)");
            }
            lspContent.AppendLine("(princ)");
        }
        
        public static void PrependSynchronizationSignal(StringBuilder lspContent)
        {
            if (AppConfigManager.Current.UsePipelineV2)
            {
                lspContent.AppendLine("; --- PIPELINE V2 INITIALIZATION ---");
                lspContent.AppendLine("(setvar \"USERI1\" 0)");
            }
        }
    }
}
