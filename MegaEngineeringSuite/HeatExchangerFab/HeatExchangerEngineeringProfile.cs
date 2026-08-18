using System;

namespace MegaEngineeringSuite.HeatExchangerFab
{
    /// <summary>
    /// Represents the complete, immutable baseline engineering profile loaded from Excel for a specific Shell ID.
    /// </summary>
    public class HeatExchangerEngineeringProfile
    {
        public int ShellID { get; set; }
        public double ShellBonnetTHK { get; set; }
        public double LinerAfterMachining { get; set; }
        public double DishendTHK { get; set; }
        public double TubeSheetFinishTHK { get; set; }
        public double TubeSheetRawTHK { get; set; }
        public double BodyFlangeFinishTHK { get; set; }
        public double BodyFlangeRawTHK { get; set; }
        public double PartitionPlateTHK { get; set; }
        public double BaffleTHK { get; set; }
        public string BoltSize { get; set; } = "M16";
        public double BoltLength { get; set; }
        public int NoOfBolts { get; set; }
        public double HoleDia { get; set; }
        public double FlangeID { get; set; }
        public double BoltPCD { get; set; }
        public double TubeSheetFinishOD { get; set; }
        public double TubeSheetRawOD { get; set; }
        public double LinerGasketOD { get; set; }
        public double TieRodDia { get; set; }
        public double TieRodQty { get; set; }
        public double SpacerTube { get; set; }
    }
}
