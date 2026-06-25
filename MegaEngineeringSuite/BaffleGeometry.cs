using System.Collections.Generic;
using System.Drawing;

namespace MegaEngineeringSuite
{
    public class BaffleGeometry
    {
        public bool IsTopCut { get; set; }
        public float BaffleOD { get; set; }
        public float BaffleRadius { get; set; }
        public float BaffleThickness { get; set; }
        public float TubeRadius { get; set; }
        public float TheoreticalCutDepth { get; set; }
        public float TheoreticalCutLineY { get; set; }
        public float ActualCutDepth { get; set; }
        public float SnappedCutLineY { get; set; }
        public float CutHalfWidth { get; set; }
        public float TheoreticalRemovedAreaRatio { get; set; }
        public float ActualRemovedAreaRatio { get; set; }
        public int Quantity { get; set; }
        public PointF CutLeftPoint { get; set; }
        public PointF CutRightPoint { get; set; }
        public PointF BaffleOdDimensionStartPoint { get; set; }
        public PointF BaffleOdDimensionEndPoint { get; set; }
        public PointF BaffleOdDimensionLineLocation { get; set; }
        public PointF CutDepthDimensionStartPoint { get; set; }
        public PointF CutDepthDimensionEndPoint { get; set; }
        public PointF CutDepthDimensionLineLocation { get; set; }
        public RectangleF Bounds { get; set; }
        public List<PointF> ActiveTubeCenters { get; set; } = new List<PointF>();
        public List<PointF> RemovedTubeCenters { get; set; } = new List<PointF>();
        public List<PointF> SemicircleTubeCenters { get; set; } = new List<PointF>();
        public List<ICadEntity> Entities { get; set; } = new List<ICadEntity>();
    }
}
