using System;

namespace MegaEngineeringSuite.HeatExchangerFab
{
    public class HeatExchangerFabData
    {
        // Shell Parameters
        public double ShellID { get; set; } = 914.0;
        public double ShellTHK { get; set; } = 5.0;
        public double ShellLength { get; set; } = 2906.0;

        // Tube Parameters
        public double TubeOD { get; set; } = 25.4;
        public double TubeTHK { get; set; } = 1.2;
        public double TubeLength { get; set; } = 3000.0;
        public int TotalTubes { get; set; } = 488;
        public double TubePitch { get; set; } = 31.75;
        public string PitchType { get; set; } = "30° Triangular";

        // Tube Sheet & Flange Parameters
        public double TubeSheetOD { get; set; } = 1070.0;
        public double TubeSheetTHK { get; set; } = 25.0;
        public double FlangeOD { get; set; } = 1070.0;
        public double FlangeID { get; set; } = 932.0;
        public double FlangeTHK { get; set; } = 36.0;

        // Liner & Serration Parameters
        public double LinerTHK { get; set; } = 5.0;
        public double LinerID { get; set; } = 920.0;
        public double LinerOD { get; set; } = 984.0;
        public double SerrationID { get; set; } = 920.0;
        public double SerrationOD { get; set; } = 984.0;

        // Bolt & Hole Parameters
        public double HoleDia { get; set; } = 22.5;
        public int NoOfBolts { get; set; } = 28;
        public double BoltPCD { get; set; } = 1020.0;

        // Baffle & Tie Rod Parameters
        public int BaffleQty { get; set; } = 5;
        public double BaffleTHK { get; set; } = 4.0;
        public int NoOfPasses { get; set; } = 1;
        public int TieRodQty { get; set; } = 6;
        public double TieRodDia { get; set; } = 12.0;

        // Materials of Construction (MOC)
        public string ShellMaterial { get; set; } = "SA 240 Gr 304";
        public string TubeMaterial { get; set; } = "SA 249 TP304";
        public string TubeSheetMaterial { get; set; } = "SA 240 Gr 304";
        public string BaffleMaterial { get; set; } = "SA 240 Gr 304";
        public string TieRodMaterial { get; set; } = "AISI 304";
        public string GasketMaterial { get; set; } = "EPDM (FOOD G)";
        public string FlangeMaterial { get; set; } = "IS 2062 Gr. B";
    }
}
