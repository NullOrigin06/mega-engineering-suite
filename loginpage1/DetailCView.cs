using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace loginpage1
{
    public class DetailCView : ICadView
    {
        public IEnumerable<ICadEntity> Render(GeometryModel geometry, EngineeringDataModel data, PointF origin)
        {
            List<ICadEntity> entities = new List<ICadEntity>();

            float thk = (float)data.TubeSheetFinishTHK;
            float holeRad = (float)data.HoleDia / 2f;
            float gWidth = (float)data.TubeHoleGrooveWidth;
            float gDepth = (float)data.TubeHoleGrooveDepth;
            float chamfer = (float)data.TubeHoleChamfer;

            float spacing = 15f; // Standard spacing between grooves
            float centerY = -thk / 2f;
            float g1Y = centerY + spacing / 2f + gWidth / 2f; // Top of groove 1
            float g2Y = centerY - spacing / 2f + gWidth / 2f; // Top of groove 2

            float blockWidth = holeRad * 6f;

            // Right Hole Profile
            List<PointF> rightProfile = new List<PointF>
            {
                new PointF(holeRad + chamfer, 0), 
                new PointF(holeRad, -chamfer),    
                new PointF(holeRad, g1Y),         
                new PointF(holeRad + gDepth, g1Y), 
                new PointF(holeRad + gDepth, g1Y - gWidth), 
                new PointF(holeRad, g1Y - gWidth), 
                new PointF(holeRad, g2Y),         
                new PointF(holeRad + gDepth, g2Y), 
                new PointF(holeRad + gDepth, g2Y - gWidth), 
                new PointF(holeRad, g2Y - gWidth), 
                new PointF(holeRad, -thk + chamfer), 
                new PointF(holeRad + chamfer, -thk) 
            };

            // Left Hole Profile
            List<PointF> leftProfile = new List<PointF>();
            foreach (var p in rightProfile)
            {
                leftProfile.Add(new PointF(-p.X, p.Y));
            }

            // Right Body Boundary for Hatching
            List<PointF> rightBody = new List<PointF> { new PointF(blockWidth / 2, 0) };
            rightBody.AddRange(rightProfile);
            rightBody.Add(new PointF(blockWidth / 2, -thk));

            // Left Body Boundary for Hatching
            List<PointF> leftBody = new List<PointF> { new PointF(-blockWidth / 2, 0) };
            leftBody.AddRange(leftProfile);
            leftBody.Add(new PointF(-blockWidth / 2, -thk));

            // Add Hatches to Tube Sheet ONLY
            entities.Add(new CadHatch { BoundaryVertices = rightBody, HatchPattern = "ANSI31", HatchScale = 1.5f, EntityColor = Color.Cyan });
            entities.Add(new CadHatch { BoundaryVertices = leftBody, HatchPattern = "ANSI31", HatchScale = 1.5f, EntityColor = Color.Cyan });

            // Draw Hole Profiles
            for (int i = 0; i < rightProfile.Count - 1; i++) entities.Add(new CadLine { Start = rightProfile[i], End = rightProfile[i + 1], EntityColor = Color.White });
            for (int i = 0; i < leftProfile.Count - 1; i++) entities.Add(new CadLine { Start = leftProfile[i], End = leftProfile[i + 1], EntityColor = Color.White });

            // Top and Bottom Tube Sheet surfaces
            entities.Add(new CadLine { Start = new PointF(holeRad + chamfer, 0), End = new PointF(blockWidth/2, 0), EntityColor = Color.White });
            entities.Add(new CadLine { Start = new PointF(-holeRad - chamfer, 0), End = new PointF(-blockWidth/2, 0), EntityColor = Color.White });
            entities.Add(new CadLine { Start = new PointF(holeRad + chamfer, -thk), End = new PointF(blockWidth/2, -thk), EntityColor = Color.White });
            entities.Add(new CadLine { Start = new PointF(-holeRad - chamfer, -thk), End = new PointF(-blockWidth/2, -thk), EntityColor = Color.White });

            // Side breaks
            entities.Add(new CadLine { Start = new PointF(blockWidth/2, 0), End = new PointF(blockWidth/2, -thk), EntityColor = Color.Gray });
            entities.Add(new CadLine { Start = new PointF(-blockWidth/2, 0), End = new PointF(-blockWidth/2, -thk), EntityColor = Color.Gray });

            // TUBE GEOMETRY (Unhatched, inserted through hole)
            float tubeExt = 15f; // Tube sticks out 15mm on both sides
            float tubeThk = 2f; // Visual thickness of tube wall
            
            // Tube Right Wall
            entities.Add(new CadLine { Start = new PointF(holeRad, tubeExt), End = new PointF(holeRad, -thk - tubeExt), EntityColor = Color.Yellow });
            entities.Add(new CadLine { Start = new PointF(holeRad - tubeThk, tubeExt), End = new PointF(holeRad - tubeThk, -thk - tubeExt), EntityColor = Color.Yellow });
            // Tube Left Wall
            entities.Add(new CadLine { Start = new PointF(-holeRad, tubeExt), End = new PointF(-holeRad, -thk - tubeExt), EntityColor = Color.Yellow });
            entities.Add(new CadLine { Start = new PointF(-holeRad + tubeThk, tubeExt), End = new PointF(-holeRad + tubeThk, -thk - tubeExt), EntityColor = Color.Yellow });
            // Tube Ends (Break lines)
            entities.Add(new CadLine { Start = new PointF(-holeRad, tubeExt), End = new PointF(holeRad, tubeExt), EntityColor = Color.Gray });
            entities.Add(new CadLine { Start = new PointF(-holeRad, -thk - tubeExt), End = new PointF(holeRad, -thk - tubeExt), EntityColor = Color.Gray });

            // Centerline
            entities.Add(new CadLine { Start = new PointF(0, tubeExt + 10f), End = new PointF(0, -thk - tubeExt - 10f), EntityColor = Color.Red, DashStyle = DashStyle.DashDot });

            // Dimensions
            float dimTextHeight = DraftingScaleManager.GetPaperSpaceDimensionHeight();

            // Hole Dia
            entities.Add(new CadDimension
            {
                StartPoint = new PointF(-holeRad, 0),
                EndPoint = new PointF(holeRad, 0),
                DimensionLineLocation = new PointF(0, 5),
                Type = DimensionType.Horizontal,
                OverrideText = "Ø<>",
                EntityColor = Color.Magenta,
                TargetPaperSpaceHeight = dimTextHeight
            });

            // Chamfer Depth (Vertical from 0 to -chamfer)
            entities.Add(new CadDimension
            {
                StartPoint = new PointF(holeRad + chamfer, 0),
                EndPoint = new PointF(holeRad, -chamfer),
                DimensionLineLocation = new PointF(holeRad + chamfer + 2, -chamfer/2f),
                Type = DimensionType.Vertical,
                OverrideText = "<>",
                EntityColor = Color.Magenta,
                TargetPaperSpaceHeight = dimTextHeight
            });

            // Chamfer Angle (Angular Dimension)
            entities.Add(new CadDimension
            {
                AngleCenterPoint = new PointF(holeRad, -chamfer),
                StartPoint = new PointF(holeRad, 0), // Vertical reference
                EndPoint = new PointF(holeRad + chamfer, 0), // The chamfer slope point
                DimensionLineLocation = new PointF(holeRad + chamfer * 1.5f, -chamfer * 1.5f), 
                Type = DimensionType.Angular,
                OverrideText = "<>°",
                EntityColor = Color.Magenta,
                TargetPaperSpaceHeight = dimTextHeight
            });

            // Groove Depth
            entities.Add(new CadDimension
            {
                StartPoint = new PointF(holeRad, g1Y),
                EndPoint = new PointF(holeRad + gDepth, g1Y),
                DimensionLineLocation = new PointF(holeRad + gDepth / 2f, g1Y + 2),
                Type = DimensionType.Horizontal,
                OverrideText = "<> DEEP",
                EntityColor = Color.Magenta,
                TargetPaperSpaceHeight = dimTextHeight
            });

            // Groove Width
            entities.Add(new CadDimension
            {
                StartPoint = new PointF(holeRad + gDepth, g1Y),
                EndPoint = new PointF(holeRad + gDepth, g1Y - gWidth),
                DimensionLineLocation = new PointF(holeRad + gDepth + 5, g1Y - gWidth/2f),
                Type = DimensionType.Vertical,
                OverrideText = "<> WIDE GROOVE",
                EntityColor = Color.Magenta,
                TargetPaperSpaceHeight = dimTextHeight
            });

            // Title
            float titleTextHeight = DraftingScaleManager.GetPaperSpaceDetailTitleHeight();

            entities.Add(new CadText
            {
                Text = "DETAIL C\n(TUBE HOLE SECTION)",
                Position = new PointF(0, -thk - tubeExt - 10f),
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
