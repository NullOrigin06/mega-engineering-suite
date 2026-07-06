namespace MegaEngineeringSuite
{
    internal static class Program
    {
        public static ValidationResult? StartupValidation { get; private set; }

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            StartupValidation = StartupValidator.Validate();
            Application.Run(new Form1());
        }
    }
}