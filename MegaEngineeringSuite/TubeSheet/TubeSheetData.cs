namespace MegaEngineeringSuite.TubeSheet
{
    public class TubeSheetData
    {
        // Legacy compatibility properties
        public double OD { get; set; } = 1000;
        public double ID { get; set; } = 800;
        public double Thickness { get; set; } = 50;

        // Production properties (Pipeline V2)
        public double OutsideDiameter { get; set; } = 1000;
        public double InsideDiameter { get; set; } = 800;
        public double StepOutsideDiameter { get; set; } = 1050;
        public double StepInsideDiameter { get; set; } = 750;
    }
}
