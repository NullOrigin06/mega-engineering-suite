using System;

namespace MegaEngineeringSuite.HeatExchangerFab
{
    public static class HeatExchangerFabDataMapper
    {
        public static HeatExchangerFabData Map(EngineeringDataModel data)
        {
            if (data == null) return new HeatExchangerFabData();

            double flangeID = data.FlangeID > 0 ? data.FlangeID : 932.0;
            double flangeOD = data.TubeSheetFinishOD > 0 ? data.TubeSheetFinishOD : 1070.0;
            double linerOD = data.LinerGasketOD > 0 ? data.LinerGasketOD : 984.0;
            double linerID = flangeID > 12.0 ? flangeID - 12.0 : 920.0;

            return new HeatExchangerFabData
            {
                ShellID = data.ShellID > 0 ? data.ShellID : 914.0,
                ShellTHK = 5.0,
                ShellLength = 2906.0,
                TubeOD = data.TubeOD > 0 ? data.TubeOD : 25.4,
                TubeTHK = 1.2,
                TubeLength = data.TubeLength > 0 ? data.TubeLength : 3000.0,
                TotalTubes = data.TubeQty > 0 ? data.TubeQty : 488,
                TubePitch = 31.75,
                PitchType = "30° Triangular",
                TubeSheetOD = flangeOD,
                TubeSheetTHK = data.TubeSheetFinishTHK > 0 ? data.TubeSheetFinishTHK : 25.0,
                FlangeOD = flangeOD,
                FlangeID = flangeID,
                FlangeTHK = data.BodyFlangeFinishTHK > 0 ? data.BodyFlangeFinishTHK : 36.0,
                
                LinerTHK = 5.0,
                LinerID = linerID,
                LinerOD = linerOD,
                SerrationID = linerID,
                SerrationOD = linerOD,

                HoleDia = data.HoleDia > 0 ? data.HoleDia : 22.5,
                NoOfBolts = data.NoOfBolts > 0 ? data.NoOfBolts : 28,
                BoltPCD = data.BoltPCD > 0 ? data.BoltPCD : 1020.0,

                BaffleQty = data.BaffleQty > 0 ? data.BaffleQty : 5,
                BaffleTHK = data.BaffleTHK > 0 ? data.BaffleTHK : 4.0,
                NoOfPasses = data.NoOfPass > 0 ? data.NoOfPass : 1,
                TieRodQty = data.TieRodQty > 0 ? (int)data.TieRodQty : 6,
                TieRodDia = data.TieRodDia > 0 ? data.TieRodDia : 12.0,

                BonnetShellFSLength = data.BonnetShellFSLength > 0 ? data.BonnetShellFSLength : 500.0,
                BonnetShellRSLength = data.BonnetShellRSLength > 0 ? data.BonnetShellRSLength : 500.0,
                DishendTHK = data.DishendTHK > 0 ? data.DishendTHK : 5.0
            };
        }
    }
}
