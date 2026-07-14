using System;
using System.IO;
using System.Threading.Tasks;
using MegaEngineeringSuite.Infrastructure.Cad;
using MegaEngineeringSuite.TubeSheet;

namespace COMTestApp
{
    public class TemplateAuditor
    {
        public static void RunAudit()
        {
            Console.WriteLine("Starting Template Audit...");
            MegaEngineeringSuite.AppConfigManager.Load();
            string templatePath = MegaEngineeringSuite.AppConfigManager.Current.DwgTemplatePath;
            if (!File.Exists(templatePath))
            {
                Console.WriteLine("Template not found: " + templatePath);
                return;
            }

            var sessionManager = CadSessionManager.Instance;
            dynamic cadApp = sessionManager.GetCadApplication();
            if (cadApp == null)
            {
                Console.WriteLine("Could not start CAD application.");
                return;
            }

            // Open document
            string tempPath = Path.Combine(Path.GetTempPath(), "AUDIT_TUBE_SHEET.dwg");
            File.Copy(templatePath, tempPath, true);
            dynamic doc = cadApp.Documents.Open(tempPath);
            var adapter = new GstarCadAdapter();
            adapter.OpenDrawing(tempPath);
            
            var syncProvider = new MegaEngineeringSuite.Infrastructure.Cad.CadUserVariableSynchronizationProvider(adapter);
            var pipelineContext = new PipelineContext(adapter, syncProvider, default);
            
            var discoveryEngine = new AnnotationDiscoveryEngine();
            discoveryEngine.DiscoverAnnotations(pipelineContext);
            
            var schema = new TubeSheetPlaceholderSchema();
            var validator = new PlaceholderSchemaValidator();
            var report = validator.ValidateSchema(pipelineContext.PlaceholderIndex, schema);

            string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs", "TemplateAuditReport.md");
            using (var writer = new StreamWriter(logPath, false))
            {
                writer.WriteLine("# Template Audit Report\n");
                writer.WriteLine("## Existing Placeholders");
                foreach (var p in pipelineContext.PlaceholderIndex.Enumerate())
                {
                    writer.WriteLine($"- `{p.PlaceholderName}` on `{p.Layer}`");
                }
                
                writer.WriteLine("\n## Missing Required Placeholders");
                foreach (var m in report.MissingRequired)
                {
                    writer.WriteLine($"- `{m}`");
                }
                
                writer.WriteLine("\n## Unexpected Placeholders");
                foreach (var u in report.Unexpected)
                {
                    writer.WriteLine($"- `{u}`");
                }
            }
            
            doc.Close(false);
            sessionManager.ReleaseCadApplication();
            if (File.Exists(tempPath)) File.Delete(tempPath);
            
            Console.WriteLine("Audit complete. Report generated at: " + logPath);
        }
    }
}
