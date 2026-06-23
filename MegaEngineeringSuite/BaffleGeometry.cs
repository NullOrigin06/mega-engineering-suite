using System.Collections.Generic;
using System.Drawing;

namespace MegaEngineeringSuite
{
    public class BaffleGeometry
    {
        public float BaffleOD { get; set; }
        public float BaffleThickness { get; set; }
        public float TheoreticalCutDepth { get; set; }
        public float ActualCutDepth { get; set; }
        public RectangleF Bounds { get; set; }
        public List<PointF> ActiveTubeCenters { get; set; } = new List<PointF>();
        public List<PointF> RemovedTubeCenters { get; set; } = new List<PointF>();
        public List<PointF> SemicircleTubeCenters { get; set; } = new List<PointF>();
        public List<ICadEntity> Entities { get; set; } = new List<ICadEntity>();
    }
}
