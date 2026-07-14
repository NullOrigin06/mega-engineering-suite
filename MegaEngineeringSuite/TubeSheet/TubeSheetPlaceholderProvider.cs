using System;
using System.Collections.Generic;

namespace MegaEngineeringSuite.TubeSheet
{
    public class TubeSheetPlaceholderProvider : IPlaceholderProvider
    {
        private readonly TubeSheetData _data;
        private readonly DrawingInformation _info;
        
        public TubeSheetPlaceholderProvider(TubeSheetData data, DrawingInformation info)
        {
            _data = data;
            _info = info;
        }

        public IReadOnlyDictionary<string, string> GetValues(MigrationProfile profile)
        {
            var values = new Dictionary<string, string>();

            if (profile == MigrationProfile.Stage8_DetailADimensions)
            {
                values["TS_OD"] = _data.OutsideDiameter.ToString();
                values["TS_ID"] = _data.InsideDiameter.ToString();
                values["TS_STEP_OD"] = _data.StepOutsideDiameter.ToString();
                values["TS_THK"] = _data.Thickness.ToString(); // Or StepInsideDiameter if that's intended, but prompt says TS_THK. I will map it to Thickness. Wait, user said TS_THK is the new authoritative name.
            }
            else
            {
                // Legacy/Production mapping
                values["<TS_OD>"] = _data.OutsideDiameter.ToString();
                values["<TS_ID>"] = _data.InsideDiameter.ToString();
                values["<TS_STEP_OD>"] = _data.StepOutsideDiameter.ToString();
                values["<TS_STEP_ID>"] = _data.StepInsideDiameter.ToString();
                values["<THK>"] = _data.Thickness.ToString();
                
                values["<CUSTOMER_NAME>"] = _info.CustomerName ?? "";
                values["<PROJECT_NUMBER>"] = _info.ProjectNo ?? "";
                values["<DRAWING_NUMBER>"] = _info.DrawingNo ?? "";
                values["<DRAWING_TITLE>"] = _info.Title ?? "";
                values["<REVISION>"] = _info.Revision ?? "";
                values["<DATE>"] = _info.Date.ToString("dd-MM-yyyy");
                values["<DRAWN_BY>"] = _info.PreparedBy ?? "";
                values["<CHECKED_BY>"] = _info.CheckedBy ?? "";
                values["<APPROVED_BY>"] = _info.ApprovedBy ?? "";
            }

            return values;
        }

    }
}
