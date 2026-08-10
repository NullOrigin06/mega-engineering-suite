using System;
using System.Collections.Generic;
using MegaEngineeringSuite.Infrastructure.Logging;

namespace MegaEngineeringSuite.HeatExchangerFab
{
    public static class HeatExchangerFabFormatter
    {
        public static Dictionary<string, string> Format(HeatExchangerFabData data)
        {
            if (data == null) data = new HeatExchangerFabData();

            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            // 1. Mandatory Short Placeholder Standard (Full Engineering Callout Strings)
            string linerThkStr = FormatValue(data.LinerTHK);
            string shellIdStr = FormatValue(data.ShellID);
            string linerIdStr = FormatValue(data.LinerID);

            map["{{BFT}}"] = FormatValue(data.FlangeTHK);
            map["{{BFI}}"] = $"FLANGE ID {FormatValue(data.FlangeID)}";
            map["{{BFO}}"] = $"FLANGE OD {FormatValue(data.FlangeOD)}";
            map["{{BFOD}}"] = $"FLANGE OD {FormatValue(data.FlangeOD)}";
            map["{{LTH}}"] = $"ID {linerIdStr} x {linerThkStr} THK";
            map["{{LT}}"] = FormatValue(data.LinerTHK);
            map["{{LID}}"] = $"ID {shellIdStr} x {linerThkStr} THK";
            map["{{LOD}}"] = $"LINER OD {FormatValue(data.LinerOD)}";
            map["{{TSO}}"] = $"TUBESHEET OD {FormatValue(data.TubeSheetOD)}";
            map["{{TST}}"] = FormatValue(data.TubeSheetTHK);
            map["{{SID}}"] = $"SERRATION ID {FormatValue(data.SerrationID)}";
            map["{{SOD}}"] = $"SERRATION OD {FormatValue(data.SerrationOD)}";

            // 2. Single Formatted Bolt Hole Callout String {{BHC}}
            string holeDiaStr = data.HoleDia % 1 == 0 ? data.HoleDia.ToString("F0") : data.HoleDia.ToString("G");
            string pcdStr = data.BoltPCD % 1 == 0 ? data.BoltPCD.ToString("F0") : data.BoltPCD.ToString("G");
            map["{{BHC}}"] = $"Ø{holeDiaStr} {data.NoOfBolts} HOLES ON\nP.C.D. {pcdStr}";

            // 3. Extended / Backward Compatible Parameter Tokens
            map["{{SHELL_ID}}"] = FormatValue(data.ShellID);
            map["{{SHELL_THK}}"] = $"{FormatValue(data.ShellTHK)} THK.";
            map["{{SHELL_LENGTH}}"] = FormatValue(data.ShellLength);
            map["{{TUBE_OD}}"] = FormatValue(data.TubeOD);
            map["{{TUBE_THK}}"] = $"{FormatValue(data.TubeTHK)} THK.";
            map["{{TUBE_LENGTH}}"] = $"{FormatValue(data.TubeLength)} Lg.";
            map["{{TUBE_QTY}}"] = data.TotalTubes.ToString();
            map["{{TUBE_PITCH}}"] = FormatValue(data.TubePitch);
            map["{{PITCH_TYPE}}"] = data.PitchType ?? "30° Triangular";
            map["{{NO_OF_PASSES}}"] = data.NoOfPasses.ToString();
            map["{{BAFFLE_QTY}}"] = data.BaffleQty.ToString("D2");
            map["{{BAFFLE_THK}}"] = $"{FormatValue(data.BaffleTHK)} THK.";

            map["{{TUBESHEET_OD}}"] = FormatValue(data.TubeSheetOD);
            map["{{TUBESHEET_THK}}"] = $"{FormatValue(data.TubeSheetTHK)} THK.";
            map["{{BODY_FLANGE_OD}}"] = FormatValue(data.FlangeOD);
            map["{{BODY_FLANGE_ID}}"] = FormatValue(data.FlangeID);
            map["{{BODY_FLANGE_THK}}"] = $"{FormatValue(data.FlangeTHK)} THK.";

            map["{{TIEROD_QTY}}"] = data.TieRodQty.ToString("D2");
            map["{{TIEROD_DIA}}"] = $"Ø{FormatValue(data.TieRodDia)}";

            map["{{MOC_SHELL}}"] = data.ShellMaterial ?? "SA 240 Gr 304";
            map["{{MOC_TUBE}}"] = data.TubeMaterial ?? "SA 249 TP304";
            map["{{MOC_TUBESHEET}}"] = data.TubeSheetMaterial ?? "SA 240 Gr 304";
            map["{{MOC_BAFFLE}}"] = data.BaffleMaterial ?? "SA 240 Gr 304";
            map["{{MOC_TIEROD}}"] = data.TieRodMaterial ?? "AISI 304";
            map["{{MOC_GASKET}}"] = data.GasketMaterial ?? "EPDM (FOOD G)";
            map["{{MOC_FLANGE}}"] = data.FlangeMaterial ?? "IS 2062 Gr. B";

            // 4. Mandatory Pre-Generation Validation & Logging
            ValidatePlaceholders(map);

            return map;
        }

        private static string FormatValue(double val)
        {
            if (double.IsNaN(val) || double.IsInfinity(val)) return "0";
            return val % 1 == 0 ? val.ToString("F0") : val.ToString("G");
        }

        private static void ValidatePlaceholders(Dictionary<string, string> map)
        {
            string[] requiredKeys = { "{{BFT}}", "{{BFI}}", "{{BFO}}", "{{LTH}}", "{{LID}}", "{{LOD}}", "{{TSO}}", "{{SID}}", "{{SOD}}", "{{BHC}}" };

            foreach (string key in requiredKeys)
            {
                if (!map.ContainsKey(key) || string.IsNullOrWhiteSpace(map[key]))
                {
                    SimpleLogger.Log("HeatExchangerFab", $"Warning: Missing placeholder value: {key}");
                }
            }
        }
    }
}
