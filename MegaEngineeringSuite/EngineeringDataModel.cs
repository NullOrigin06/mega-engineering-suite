#pragma warning disable CS8618
using System;
using System.Collections.Generic;

namespace MegaEngineeringSuite
{
    public class EngineeringDataModel
    {
        // User Inputs
        public double TubeOD { get; set; }
        public int TubeQty { get; set; }
        public int NoOfPass { get; set; }
        public double HTA { get; set; }
        public double TubeLength { get; set; }
        public int BaffleQty { get; set; } = 5;
        public string Material { get; set; } = "SS304 / Carbon Steel";
        public double TubeTHK { get; set; } = 1.6;
        public double ShellBonnetTHK { get; set; } = 5.0;
        public double LinerAfterMachining { get; set; } = 3.0;

        // Lookup Properties
        public int ShellID { get; set; }
        public double TubeSheetFinishTHK { get; set; }
        public double TubeSheetRawTHK { get; set; }
        public double BodyFlangeFinishTHK { get; set; }
        public double BodyFlangeRawTHK { get; set; }
        public double PartitionPlateTHK { get; set; }
        public double BaffleTHK { get; set; }
        public double BaffleOD { get; set; }
        public string BoltSize { get; set; }
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

        // Detail View Properties (Phase 4A)
        public double PartitionGrooveDepth { get; set; } = 5.0;
        public double PartitionGrooveWidth { get; set; } = 10.0;
        public double TubeHoleGrooveWidth { get; set; } = 3.0;
        public double TubeHoleGrooveDepth { get; set; } = 0.4;
        public double TubeHoleChamfer { get; set; } = 1.0;

        // Extras Properties
        public double BonnetShellFSLength { get; set; }
        public double BonnetShellRSLength { get; set; }
        public double DishendTHK { get; set; }

        /// <summary>
        /// Converts the strongly typed properties into a dictionary for easy display in a DataGridView.
        /// </summary>
        public Dictionary<string, string> ToDisplayDictionary()
        {
            return new Dictionary<string, string>
            {
                { "Shell I.D.", ShellID.ToString() },
                { "Tube Sheet Finish THK", TubeSheetFinishTHK.ToString() },
                { "Tube Sheet Raw THK", TubeSheetRawTHK.ToString() },
                { "Body Flange Finish THK", BodyFlangeFinishTHK.ToString() },
                { "Body Flange Raw THK", BodyFlangeRawTHK.ToString() },
                { "Partition Plate THK", PartitionPlateTHK.ToString() },
                { "Baffle THK", BaffleTHK.ToString() },
                { "Bolt Size", BoltSize },
                { "Bolt Length", BoltLength.ToString() },
                { "No Of Bolts", NoOfBolts.ToString() },
                { "Hole Dia.", HoleDia.ToString() },
                { "Flange I.D.", FlangeID.ToString() },
                { "Bolt P.C.D.", BoltPCD.ToString() },
                { "Tube Sheet Finish O.D.", TubeSheetFinishOD.ToString() },
                { "Tube Sheet Raw O.D.", TubeSheetRawOD.ToString() },
                { "Liner / Gasket O.D.", LinerGasketOD.ToString() },
                { "Tie Rod Dia.", TieRodDia.ToString() },
                { "Tie Rod Qty.", TieRodQty.ToString() },
                { "Spacer Tube", SpacerTube.ToString() },
                { "Bonnet Shell FS Length", BonnetShellFSLength.ToString() },
                { "Bonnet Shell RS Length", BonnetShellRSLength.ToString() },
                { "Dishend THK", DishendTHK.ToString() }
            };
        }
    }
}
