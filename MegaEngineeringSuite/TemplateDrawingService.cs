using System.Collections.Generic;
using System.Drawing;

namespace MegaEngineeringSuite
{
    public class TemplateDrawingService
    {
        public Dictionary<string, List<ICadEntity>> GenerateTemplateViews(GeometryModel geometry, EngineeringDataModel data)
        {
            var views = new Dictionary<string, List<ICadEntity>>();

            var rearTubeSheet = new RearTubeSheetView();
            views["REAR_TS_ANCHOR"] = new List<ICadEntity>(rearTubeSheet.Render(geometry, data, PointF.Empty));

            var frontTubeSheet = new FrontTubeSheetView();
            views["FRONT_TS_ANCHOR"] = new List<ICadEntity>(frontTubeSheet.Render(geometry, data, PointF.Empty));

            var rearSideView = new SideViewRenderer("VIEW FROM C\nREAR SIDE TUBE SHEET");
            views["REAR_SIDEVIEW_ANCHOR"] = new List<ICadEntity>(rearSideView.Render(geometry, data, PointF.Empty));

            var frontSideView = new SideViewRenderer("VIEW FROM D\nFRONT TUBE SHEET");
            views["FRONT_SIDEVIEW_ANCHOR"] = new List<ICadEntity>(frontSideView.Render(geometry, data, PointF.Empty));

            var baffleA = new BaffleAView();
            views["BAFFLE_A_ANCHOR"] = new List<ICadEntity>(baffleA.Render(geometry, data, PointF.Empty));

            var baffleB = new BaffleBView();
            views["BAFFLE_B_ANCHOR"] = new List<ICadEntity>(baffleB.Render(geometry, data, PointF.Empty));

            return views;
        }
    }
}
