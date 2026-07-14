using System;
using System.IO;
using System.Threading.Tasks;
using MegaEngineeringSuite.BonnetFlange;
using MegaEngineeringSuite;
using MegaEngineeringSuite.Infrastructure.Logging;

namespace COMTestApp
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            AppConfigManager.Load();
            Console.WriteLine("Templates Path: " + AppConfigManager.Current.BonnetTemplatePath);
            Console.WriteLine("Output Path: " + AppConfigManager.Current.BonnetOutputFolder);

            try
            {
                var data = new BonnetFlangeData
                {
                    OD = 1070,
                    ID = 932,
                    Thickness = 36,
                    LinerOD = 984,
                    LinerID = 920
                };

                // Mimic the exact Task.Run call from BonnetFlangeForm
                string outputPath = Task.Run(() =>
                {
                    var generator = new BonnetFlangeGenerator();
                    return generator.Generate(data, new DrawingInformation());
                }).GetAwaiter().GetResult();
                TemplateAuditor.RunAudit();
                Console.WriteLine("Success: " + outputPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine("FAILURE:");
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
