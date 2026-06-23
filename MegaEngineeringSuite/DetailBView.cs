using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace MegaEngineeringSuite
{
    public class DetailBView : ICadView
    {
        public IEnumerable<ICadEntity> Render(GeometryModel geometry, EngineeringDataModel data, PointF origin)
        {
            List<ICadEntity> entities = new List<ICadEntity>();

            // Fixed Template Geometry (Not dependent on engineering parameters)
            float pitch = 31.75f;
            float tRad = 9.525f; // 19.05 diameter

            PointF p1 = new PointF(0, 0);
            PointF p2 = new PointF(pitch, 0);
            PointF p3 = new PointF(pitch / 2f, (float)(pitch * Math.Sin(Math.PI / 3)));

            // Draw holes
            entities.Add(new CadCircle { Center = p1, Radius = tRad, EntityColor = Color.Blue });
            entities.Add(new CadCircle { Center = p2, Radius = tRad, EntityColor = Color.Blue });
            entities.Add(new CadCircle { Center = p3, Radius = tRad, EntityColor = Color.Blue });

            // Draw pitch triangle (Solid red lines per reference)
            entities.Add(new CadLine { Start = p1, End = p2, EntityColor = Color.Red, DashStyle = DashStyle.Solid });
            entities.Add(new CadLine { Start = p2, End = p3, EntityColor = Color.Red, DashStyle = DashStyle.Solid });
            entities.Add(new CadLine { Start = p3, End = p1, EntityColor = Color.Red, DashStyle = DashStyle.Solid });

            float dimTextHeight = DraftingScaleManager.GetPaperSpaceDimensionHeight();

            // Pitch Dimension
            entities.Add(new CadDimension
            {
                StartPoint = p1,
                EndPoint = p2,
                DimensionLineLocation = new PointF(pitch / 2f, -tRad - 5),
                Type = DimensionType.Horizontal,
                OverrideText = "31.75",
                EntityColor = Color.Magenta,
                TargetPaperSpaceHeight = dimTextHeight
            });

            // Angular Dimension (60 degrees)
            entities.Add(new CadDimension
            {
                AngleCenterPoint = p2,
                StartPoint = p1,
                EndPoint = p3,
                DimensionLineLocation = new PointF(p2.X + 15, p2.Y + 15),
                Type = DimensionType.Angular,
                OverrideText = "60°",
                EntityColor = Color.Magenta,
                TargetPaperSpaceHeight = dimTextHeight
            });

            // Title
            float titleTextHeight = DraftingScaleManager.GetPaperSpaceDetailTitleHeight();

            entities.Add(new CadText
            {
                Text = "TYP TUBE PITCH",
                Position = new PointF(pitch / 2f, -tRad - 25f),
                EntityColor = Color.DodgerBlue,
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Near,
                TargetPaperSpaceHeight = titleTextHeight 
            });

            // Apply translation
            TranslateEntities(entities, origin.X, origin.Y);

            return entities;
        }

        private void TranslateEntities(List<ICadEntity> entities, float dx, float dy)
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
                else if (entity is CadDimension dim)
                {
                    dim.StartPoint = new PointF(dim.StartPoint.X + dx, dim.StartPoint.Y + dy);
                    dim.EndPoint = new PointF(dim.EndPoint.X + dx, dim.EndPoint.Y + dy);
                    dim.SelectionPoint = new PointF(dim.SelectionPoint.X + dx, dim.SelectionPoint.Y + dy);
                    dim.DimensionLineLocation = new PointF(dim.DimensionLineLocation.X + dx, dim.DimensionLineLocation.Y + dy);
                    if (dim.Type == DimensionType.Angular)
                    {
                        dim.AngleCenterPoint = new PointF(dim.AngleCenterPoint.X + dx, dim.AngleCenterPoint.Y + dy);
                    }
                }
                else if (entity is CadText text)
                {
                    text.Position = new PointF(text.Position.X + dx, text.Position.Y + dy);
                }
            }
        }
    }
}
