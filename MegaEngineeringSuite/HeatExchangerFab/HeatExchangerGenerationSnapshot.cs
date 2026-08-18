using System;

namespace MegaEngineeringSuite.HeatExchangerFab
{
    /// <summary>
    /// Represents the final, immutable resolved engineering snapshot passed to CAD generators.
    /// Combines baseline profile, active UI overrides, user inputs, and non-Excel extras.
    /// </summary>
    public class HeatExchangerGenerationSnapshot
    {
        public string RunId { get; set; } = Guid.NewGuid().ToString("N");
        public DateTime Timestamp { get; set; } = DateTime.Now;

        // Core Shell & Profile Dimensions
        public double ShellID { get; set; }
        public double ShellTHK { get; set; }
        public double ShellLength { get; set; }

        // Tube Parameters
        public double TubeOD { get; set; }
        public double TubeTHK { get; set; }
        public double TubeLength { get; set; }
        public int TotalTubes { get; set; }
        public double TubePitch { get; set; }
        public string PitchType { get; set; } = "30° Triangular";
        public int NoOfPasses { get; set; } = 1;

        // Tube Sheet & Flange Dimensions
        public double TubeSheetOD { get; set; }
        public double TubeSheetTHK { get; set; }
        public double TubeSheetRawOD { get; set; }
        public double TubeSheetRawTHK { get; set; }
        public double FlangeOD { get; set; }
        public double FlangeID { get; set; }
        public double FlangeTHK { get; set; }
        public double BodyFlangeRawTHK { get; set; }

        // Internal & Sealing Dimensions
        public double LinerOD { get; set; }
        public double LinerID { get; set; }
        public double LinerTHK { get; set; }
        public double SerrationOD { get; set; }
        public double SerrationID { get; set; }

        // Baffle, Partition & Fasteners
        public int BaffleQty { get; set; } = 5;
        public double BaffleTHK { get; set; } = 4.0;
        public double PartitionPlateTHK { get; set; } = 8.0;
        public string BoltSize { get; set; } = "M16";
        public double BoltLength { get; set; } = 100.0;
        public int NoOfBolts { get; set; } = 24;
        public double HoleDia { get; set; } = 18.0;
        public double BoltPCD { get; set; } = 895.0;

        // Tie Rods & Spacers
        public int TieRodQty { get; set; } = 6;
        public double TieRodDia { get; set; } = 12.0;
        public double SpacerTube { get; set; } = 10.0;

        // Extras
        public double BonnetShellFSLength { get; set; } = 500.0;
        public double BonnetShellRSLength { get; set; } = 500.0;
        public double DishendTHK { get; set; } = 5.0;

        // Materials of Construction (MOC)
        public string ShellMaterial { get; set; } = "SA 240 Gr 304";
        public string TubeMaterial { get; set; } = "SA 249 TP304";
        public string TubeSheetMaterial { get; set; } = "SA 240 Gr 304";
        public string BaffleMaterial { get; set; } = "SA 240 Gr 304";
        public string TieRodMaterial { get; set; } = "AISI 304";
        public string GasketMaterial { get; set; } = "EPDM (FOOD G)";
        public string FlangeMaterial { get; set; } = "IS 2062 Gr. B";

        /// <summary>
        /// Converts snapshot into HeatExchangerFabData for backwards compatibility with generator formatting.
        /// </summary>
        public HeatExchangerFabData ToFabData()
        {
            return new HeatExchangerFabData
            {
                ShellID = this.ShellID,
                ShellTHK = this.ShellTHK,
                ShellLength = this.ShellLength,
                TubeOD = this.TubeOD,
                TubeTHK = this.TubeTHK,
                TubeLength = this.TubeLength,
                TotalTubes = this.TotalTubes,
                TubePitch = this.TubePitch,
                PitchType = this.PitchType,
                NoOfPasses = this.NoOfPasses,
                TubeSheetOD = this.TubeSheetOD,
                TubeSheetTHK = this.TubeSheetTHK,
                FlangeOD = this.FlangeOD,
                FlangeID = this.FlangeID,
                FlangeTHK = this.FlangeTHK,
                LinerOD = this.LinerOD,
                LinerID = this.LinerID,
                LinerTHK = this.LinerTHK,
                SerrationOD = this.SerrationOD,
                SerrationID = this.SerrationID,
                BaffleQty = this.BaffleQty,
                BaffleTHK = this.BaffleTHK,
                TieRodQty = this.TieRodQty,
                TieRodDia = this.TieRodDia,
                NoOfBolts = this.NoOfBolts,
                BoltPCD = this.BoltPCD,
                HoleDia = this.HoleDia,
                BonnetShellFSLength = this.BonnetShellFSLength,
                BonnetShellRSLength = this.BonnetShellRSLength,
                DishendTHK = this.DishendTHK,
                ShellMaterial = this.ShellMaterial,
                TubeMaterial = this.TubeMaterial,
                TubeSheetMaterial = this.TubeSheetMaterial,
                BaffleMaterial = this.BaffleMaterial,
                TieRodMaterial = this.TieRodMaterial,
                GasketMaterial = this.GasketMaterial,
                FlangeMaterial = this.FlangeMaterial
            };
        }
    }
}
