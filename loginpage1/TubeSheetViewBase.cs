using System;
using System.Collections.Generic;
using System.Drawing;

namespace loginpage1
{
    public abstract class TubeSheetViewBase : ICadView
    {
        public abstract IEnumerable<ICadEntity> Render(GeometryModel geometry, EngineeringDataModel data, PointF origin);

        protected IEnumerable<ICadEntity> GenerateTubeSheetGeometry(GeometryModel geometry, EngineeringDataModel data)
        {
            List<ICadEntity> entities = new List<ICadEntity>();

            // Center lines
            float clLen = geometry.OuterDiameter / 2f + 30;
            entities.Add(new CadLine { Start = new PointF(-clLen, 0), End = new PointF(clLen, 0), EntityColor = Color.Red, DashStyle = System.Drawing.Drawing2D.DashStyle.DashDot });
            entities.Add(new CadLine { Start = new PointF(0, -clLen), End = new PointF(0, clLen), EntityColor = Color.Red, DashStyle = System.Drawing.Drawing2D.DashStyle.DashDot });

            // Main Circles
            float outerRad = geometry.OuterDiameter / 2f;
            entities.Add(new CadCircle { Center = new PointF(0, 0), Radius = outerRad, EntityColor = Color.Blue });

            float shellOuterRad = geometry.ShellOuterRadius;
            entities.Add(new CadCircle { Center = new PointF(0, 0), Radius = shellOuterRad, EntityColor = Color.Blue });

            float shellInnerRad = geometry.ShellRadius;
            entities.Add(new CadCircle { Center = new PointF(0, 0), Radius = shellInnerRad, EntityColor = Color.Blue });

            float pcdRad = geometry.BoltPcdRadius;
            entities.Add(new CadCircle { Center = new PointF(0, 0), Radius = pcdRad, EntityColor = Color.Red, DashStyle = System.Drawing.Drawing2D.DashStyle.DashDotDot });

            // Bolt Holes
            float holeRad = geometry.BoltHoleRadius;
            foreach (var point in geometry.BoltHoleCoordinates)
            {
                entities.Add(new CadCircle { Center = new PointF(point.X, point.Y), Radius = holeRad, EntityColor = Color.DarkGray, IsFilled = true });
                entities.Add(new CadCircle { Center = new PointF(point.X, point.Y), Radius = holeRad, EntityColor = Color.Red });
            }

            // Tubes
            float tRad = geometry.TubeRadius;
            if (geometry.TubeCoordinates != null)
            {
                foreach (var pt in geometry.TubeCoordinates)
                {
                    entities.Add(new CadCircle { Center = new PointF(pt.X, pt.Y), Radius = tRad, EntityColor = Color.Blue });
                }
            }

            // Partitions (Yellow)
            float sRad = geometry.ShellRadius;
            if (geometry.NumberOfPasses == 2 || geometry.NumberOfPasses == 4)
            {
                entities.Add(new CadLine { Start = new PointF(-sRad, 0), End = new PointF(sRad, 0), EntityColor = Color.Yellow }); // Horizontal
                if (geometry.NumberOfPasses == 4)
                {
                    entities.Add(new CadLine { Start = new PointF(0, -sRad), End = new PointF(0, sRad), EntityColor = Color.Yellow }); // Vertical
                }
            }

            return entities;
        }

        protected void TranslateEntities(List<ICadEntity> entities, float dx, float dy)
        {
            if (dx == 0 && dy == 0) return;

            foreach (var entity in entities)
            {
                if (entity is CadCircle circle)
                {
                    circle.Center = new PointF(circle.Center.X + dx, circle.Center.Y + dy);
                }
                else if (entity is CadLine line)
                {
                    line.Start = new PointF(line.Start.X + dx, line.Start.Y + dy);
                    line.End = new PointF(line.End.X + dx, line.End.Y + dy);
                }
                else if (entity is CadText text)
                {
                    text.Position = new PointF(text.Position.X + dx, text.Position.Y + dy);
                }
                else if (entity is CadDimension dim)
                {
                    dim.StartPoint = new PointF(dim.StartPoint.X + dx, dim.StartPoint.Y + dy);
                    dim.EndPoint = new PointF(dim.EndPoint.X + dx, dim.EndPoint.Y + dy);
                    dim.SelectionPoint = new PointF(dim.SelectionPoint.X + dx, dim.SelectionPoint.Y + dy);
                    dim.DimensionLineLocation = new PointF(dim.DimensionLineLocation.X + dx, dim.DimensionLineLocation.Y + dy);
                }
                else if (entity is CadLeader leader)
                {
                    for (int i = 0; i < leader.Vertices.Count; i++)
                    {
                        leader.Vertices[i] = new PointF(leader.Vertices[i].X + dx, leader.Vertices[i].Y + dy);
                    }
                }
            }
        }
    }
}
