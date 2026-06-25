using System;
using System.Collections.Generic;
using System.Drawing;

namespace MegaEngineeringSuite
{
    public class DrawingBounds
    {
        public float MinX { get; set; } = float.MaxValue;
        public float MaxX { get; set; } = float.MinValue;
        public float MinY { get; set; } = float.MaxValue;
        public float MaxY { get; set; } = float.MinValue;

        public float Width => MaxX > MinX ? MaxX - MinX : 0;
        public float Height => MaxY > MinY ? MaxY - MinY : 0;

        public void AddPoint(PointF point)
        {
            if (point.X < MinX) MinX = point.X;
            if (point.X > MaxX) MaxX = point.X;
            if (point.Y < MinY) MinY = point.Y;
            if (point.Y > MaxY) MaxY = point.Y;
        }

        public bool Intersects(DrawingBounds other)
        {
            if (MaxX < other.MinX || MinX > other.MaxX) return false;
            if (MaxY < other.MinY || MinY > other.MaxY) return false;
            return true;
        }
    }

    public static class DrawingBoundsCalculator
    {
        public static DrawingBounds CalculateBounds(IEnumerable<ICadEntity> entities)
        {
            DrawingBounds box = new DrawingBounds();

            foreach (var entity in entities)
            {
                if (entity is CadCircle circle)
                {
                    box.AddPoint(new PointF(circle.Center.X - circle.Radius, circle.Center.Y - circle.Radius));
                    box.AddPoint(new PointF(circle.Center.X + circle.Radius, circle.Center.Y + circle.Radius));
                }
                else if (entity is CadArc arc)
                {
                    box.AddPoint(new PointF(arc.Center.X - arc.Radius, arc.Center.Y - arc.Radius));
                    box.AddPoint(new PointF(arc.Center.X + arc.Radius, arc.Center.Y + arc.Radius));
                }
                else if (entity is CadLine line)
                {
                    box.AddPoint(line.Start);
                    box.AddPoint(line.End);
                }
                else if (entity is CadText text)
                {
                    box.AddPoint(text.Position);
                    // Do not add the full text extent so text length doesn't dictate block scale!
                }
                else if (entity is CadDimension dim)
                {
                    box.AddPoint(dim.StartPoint);
                    box.AddPoint(dim.EndPoint);
                    box.AddPoint(dim.SelectionPoint);
                    box.AddPoint(dim.DimensionLineLocation);
                }
                else if (entity is CadLeader leader)
                {
                    foreach (var pt in leader.Vertices)
                    {
                        box.AddPoint(pt);
                    }
                }
                else if (entity is CadPolyline polyline)
                {
                    foreach (var pt in polyline.Vertices.Select(v => v.Point))
                    {
                        box.AddPoint(pt);
                    }
                }
                else if (entity is CadHatch hatch)
                {
                    foreach (var pt in hatch.BoundaryVertices)
                    {
                        box.AddPoint(pt);
                    }
                }
            }

            return box;
        }
    }
}
