using System;
using System.Collections.Generic;
using System.Drawing;

namespace loginpage1
{
    public class GeometryModel
    {
        public PointF CenterPoint { get; set; }
        public float TubeSheetRadius { get; set; }
        public float BoltPcdRadius { get; set; }
        public float BoltHoleRadius { get; set; }
        public List<PointF> BoltHoleCoordinates { get; set; } = new List<PointF>();
        public float OuterDiameter { get; set; }
        public int NumberOfBolts { get; set; }

        // Tube Layout Properties
        public float TubeRadius { get; set; }
        public float TubePitch { get; set; }
        public float ShellRadius { get; set; }
        public float ShellOuterRadius { get; set; }
        public int NumberOfPasses { get; set; }
        public float PartitionPlateThickness { get; set; }
        public List<PointF> TubeCoordinates { get; set; } = new List<PointF>();
        public List<int> RowTubeCounts { get; set; } = new List<int>();
        
        public GeometryModel()
        {
            CenterPoint = new PointF(0, 0);
        }
    }
}
