using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace MegaEngineeringSuite
{
    public class DetailAView : ICadView
    {
        public IEnumerable<ICadEntity> Render(GeometryModel geometry, EngineeringDataModel data, PointF origin)
        {
            List<ICadEntity> entities = new List<ICadEntity>();

            // Fixed Template Geometry (Not dependent on engineering parameters)
            
            // Wavy Top Edge
            var pTopWave = new List<PointF> {
                new PointF(-35, 50), new PointF(-30, 52), new PointF(-25, 48), 
                new PointF(-20, 52), new PointF(-15, 48), new PointF(-10, 52), 
                new PointF(-5, 48), new PointF(0, 50)
            };
            
            // Right Edge & Hole Profile
            var pRightHole = new List<PointF> {
                new PointF(0, 50),
                new PointF(0, 10),
                new PointF(-4, 6),    // top chamfer
                new PointF(-15, 6),   // top flat
                new PointF(-18, 0),   // drill tip
                new PointF(-15, -6),  // bottom flat
                new PointF(-4, -6),   // bottom chamfer
                new PointF(0, -10),
                new PointF(0, -50)
            };

            // Wavy Bottom Edge
            var pBottomWave = new List<PointF> {
                new PointF(0, -50), new PointF(-5, -52), new PointF(-10, -48), 
                new PointF(-15, -52), new PointF(-20, -48), new PointF(-25, -52), 
                new PointF(-30, -48), new PointF(-35, -50)
            };

            // Left Edge
            var pLeftEdge = new List<PointF> {
                new PointF(-35, -50),
                new PointF(-35, 50)
            };

            // Combine for Hatching
            var hatchBoundary = new List<PointF>();
            hatchBoundary.AddRange(pTopWave);
            hatchBoundary.AddRange(pRightHole.GetRange(1, pRightHole.Count - 1)); // skip duplicate
            hatchBoundary.AddRange(pBottomWave.GetRange(1, pBottomWave.Count - 1));
            hatchBoundary.Add(new PointF(-35, 50)); // close

            // Hatch the body
            entities.Add(new CadHatch
            {
                BoundaryVertices = hatchBoundary,
                HatchPattern = "ANSI31",
                HatchScale = 2.0f,
                EntityColor = Color.Cyan
            });

            // Draw Outlines
            entities.Add(new CadPolyline { Vertices = pTopWave.Select(p => new CadPolylineVertex(p)).ToList(), EntityColor = Color.Gray });
            entities.Add(new CadPolyline { Vertices = pRightHole.Select(p => new CadPolylineVertex(p)).ToList(), EntityColor = Color.White });
            entities.Add(new CadPolyline { Vertices = pBottomWave.Select(p => new CadPolylineVertex(p)).ToList(), EntityColor = Color.Gray });
            entities.Add(new CadLine { Start = new PointF(-35, -50), End = new PointF(-35, 50), EntityColor = Color.White });

            // Centerline
            entities.Add(new CadLine
            {
                Start = new PointF(-45, 0),
                End = new PointF(10, 0),
                EntityColor = Color.Red,
                DashStyle = DashStyle.DashDot
            });

            // Dimensions
            float dimTextHeight = DraftingScaleManager.GetPaperSpaceDimensionHeight(); 

            // Horizontal Depth "15"
            entities.Add(new CadDimension
            {
                StartPoint = new PointF(-15, 0),
                EndPoint = new PointF(0, 0),
                DimensionLineLocation = new PointF(-7.5f, 30),
                Type = DimensionType.Horizontal,
                OverrideText = "15",
                EntityColor = Color.Magenta,
                TargetPaperSpaceHeight = dimTextHeight
            });

            // Vertical Hole Dia "M12"
            entities.Add(new CadDimension
            {
                StartPoint = new PointF(0, 6),
                EndPoint = new PointF(0, -6),
                DimensionLineLocation = new PointF(15, 0),
                Type = DimensionType.Vertical,
                OverrideText = "M12",
                EntityColor = Color.Magenta,
                TargetPaperSpaceHeight = dimTextHeight
            });

            // Note Leader
            entities.Add(new CadLeader
            {
                Vertices = new List<PointF> { new PointF(-2, 8), new PointF(-10, 40), new PointF(-35, 40) },
                HasArrowHead = true,
                EntityColor = Color.Magenta
            });

            entities.Add(new CadText
            {
                Text = "4x45° COUNTER\nSUNK HOLE",
                Position = new PointF(-35, 42),
                EntityColor = Color.DodgerBlue,
                Alignment = StringAlignment.Near,
                LineAlignment = StringAlignment.Far,
                TargetPaperSpaceHeight = DraftingScaleManager.GetPaperSpaceStandardNotesHeight()
            });

            // Title
            float titleTextHeight = DraftingScaleManager.GetPaperSpaceDetailTitleHeight();

            entities.Add(new CadText
            {
                Text = "DETAILS AT 'B'",
                Position = new PointF(-15, -65f),
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
                if (entity is CadLine line)
                {
                    line.Start = new PointF(line.Start.X + dx, line.Start.Y + dy);
                    line.End = new PointF(line.End.X + dx, line.End.Y + dy);
                }
                else if (entity is CadPolyline poly)
                {
                    for (int i = 0; i < poly.Vertices.Count; i++)
                    {
                        poly.Vertices[i].Point = new PointF(poly.Vertices[i].Point.X + dx, poly.Vertices[i].Point.Y + dy);
                    }
                }
                else if (entity is CadDimension dim)
                {
                    dim.StartPoint = new PointF(dim.StartPoint.X + dx, dim.StartPoint.Y + dy);
                    dim.EndPoint = new PointF(dim.EndPoint.X + dx, dim.EndPoint.Y + dy);
                    dim.SelectionPoint = new PointF(dim.SelectionPoint.X + dx, dim.SelectionPoint.Y + dy);
                    dim.DimensionLineLocation = new PointF(dim.DimensionLineLocation.X + dx, dim.DimensionLineLocation.Y + dy);
                }
                else if (entity is CadText text)
                {
                    text.Position = new PointF(text.Position.X + dx, text.Position.Y + dy);
                }
                else if (entity is CadLeader leader)
                {
                    for (int i = 0; i < leader.Vertices.Count; i++)
                    {
                        leader.Vertices[i] = new PointF(leader.Vertices[i].X + dx, leader.Vertices[i].Y + dy);
                    }
                }
                else if (entity is CadHatch hatch)
                {
                    for (int i = 0; i < hatch.BoundaryVertices.Count; i++)
                    {
                        hatch.BoundaryVertices[i] = new PointF(hatch.BoundaryVertices[i].X + dx, hatch.BoundaryVertices[i].Y + dy);
                    }
                }
            }
        }
    }
}
