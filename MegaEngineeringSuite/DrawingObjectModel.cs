#pragma warning disable CS8618
using System.Collections.Generic;
using System.Drawing;

namespace MegaEngineeringSuite
{
    public interface ICadEntity
    {
        Color EntityColor { get; set; }
    }

    public class CadCircle : ICadEntity
    {
        public PointF Center { get; set; }
        public float Radius { get; set; }
        public Color EntityColor { get; set; }
        public bool IsFilled { get; set; }
        public System.Drawing.Drawing2D.DashStyle DashStyle { get; set; } = System.Drawing.Drawing2D.DashStyle.Solid;
    }

    public class CadArc : ICadEntity
    {
        public PointF Center { get; set; }
        public float Radius { get; set; }
        public float StartAngle { get; set; }
        public float EndAngle { get; set; }
        public Color EntityColor { get; set; }
    }

    public class CadLine : ICadEntity
    {
        public PointF Start { get; set; }
        public PointF End { get; set; }
        public Color EntityColor { get; set; }
        public System.Drawing.Drawing2D.DashStyle DashStyle { get; set; } = System.Drawing.Drawing2D.DashStyle.Solid;
    }

    public class CadText : ICadEntity
    {
        public string Text { get; set; }
        public PointF Position { get; set; }
        public Color EntityColor { get; set; }
        public StringAlignment Alignment { get; set; } = StringAlignment.Near;
        public StringAlignment LineAlignment { get; set; } = StringAlignment.Near;
        public float FontSize { get; set; } = 10f;
        public float TargetPaperSpaceHeight { get; set; } = 3.5f;
    }

    public class CadLeader : ICadEntity
    {
        public List<PointF> Vertices { get; set; } = new List<PointF>();
        public Color EntityColor { get; set; }
        public bool HasArrowHead { get; set; } = false;
    }

    public class CadPolyline : ICadEntity
    {
        public List<PointF> Vertices { get; set; } = new List<PointF>();
        public Color EntityColor { get; set; }
        public bool IsClosed { get; set; } = false;
    }


    public class CadDimension : ICadEntity
    {
        public PointF StartPoint { get; set; }
        public PointF EndPoint { get; set; }
        public PointF SelectionPoint { get; set; }
        public PointF DimensionLineLocation { get; set; }
        public DimensionType Type { get; set; }
        public string LayerName { get; set; } = "DIMENSIONS";
        public string OverrideText { get; set; }
        public float TextHeight { get; set; } = 10f;
        public float TargetPaperSpaceHeight { get; set; } = 3.5f;
        public Color EntityColor { get; set; }
        public PointF AngleCenterPoint { get; set; } // Used for Angular dimensions
    }

    public class CadHatch : ICadEntity
    {
        public string LayerName { get; set; } = "Hatch";
        public Color EntityColor { get; set; } = Color.Cyan;
        public string HatchPattern { get; set; } = "ANSI31";
        public float HatchScale { get; set; } = 1.0f;
        public List<PointF> BoundaryVertices { get; set; } = new List<PointF>();
    }

    public enum DimensionType
    {
        Horizontal,
        Vertical,
        Aligned,
        Diameter,
        Radius,
        Angular
    }

    public class DrawingModel
    {
        public List<ICadEntity> Entities { get; set; } = new List<ICadEntity>();
        
        public void Add(ICadEntity entity)
        {
            Entities.Add(entity);
        }

        public void AddRange(IEnumerable<ICadEntity> entities)
        {
            Entities.AddRange(entities);
        }
    }
}
