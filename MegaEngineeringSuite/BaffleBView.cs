using System;
using System.Collections.Generic;
using System.Drawing;

namespace MegaEngineeringSuite
{
    public class BaffleBView : ICadView
    {
        private void ExtractGeometricReferences(BaffleGeometry geometry, out PointF topBoundary, out PointF bottomBoundary, out PointF leftBoundary, out PointF rightBoundary, out float cutBoundaryY, out float partitionY, out float centerY)
        {
            float minX = float.MaxValue, maxX = float.MinValue;
            float minY = float.MaxValue, maxY = float.MinValue;
            
            CadArc mainArc = null;
            CadLine mainCutLine = null;

            foreach (var entity in geometry.Entities)
            {
                if (entity is CadArc arc)
                {
                    if (mainArc == null || arc.Radius > mainArc.Radius)
                        mainArc = arc;
                }
                else if (entity is CadLine line)
                {
                    if (line.EntityColor == Color.Red) continue;
                    
                    if (Math.Abs(line.Start.Y - line.End.Y) < 0.01f) // Horizontal line
                    {
                        if (mainCutLine == null || Math.Abs(line.Start.X - line.End.X) > Math.Abs(mainCutLine.Start.X - mainCutLine.End.X))
                        {
                            mainCutLine = line;
                        }
                    }
                }
            }

            centerY = mainArc != null ? mainArc.Center.Y : 0f;
            partitionY = centerY; // Fallback to center for now, could be updated if actual partition entities are parsed

            foreach (var entity in geometry.Entities)
            {
                if (entity is CadLine line)
                {
                    if (line.EntityColor == Color.Red) continue; // Ignore centerlines
                    
                    minX = Math.Min(minX, Math.Min(line.Start.X, line.End.X));
                    maxX = Math.Max(maxX, Math.Max(line.Start.X, line.End.X));
                    minY = Math.Min(minY, Math.Min(line.Start.Y, line.End.Y));
                    maxY = Math.Max(maxY, Math.Max(line.Start.Y, line.End.Y));
                }
                else if (entity is CadCircle circle)
                {
                    minX = Math.Min(minX, circle.Center.X - circle.Radius);
                    maxX = Math.Max(maxX, circle.Center.X + circle.Radius);
                    minY = Math.Min(minY, circle.Center.Y - circle.Radius);
                    maxY = Math.Max(maxY, circle.Center.Y + circle.Radius);
                }
                else if (entity is CadArc arc)
                {
                    int steps = 36;
                    float angleStep = (arc.EndAngle - arc.StartAngle) / steps;
                    if (arc.EndAngle < arc.StartAngle) angleStep = (arc.EndAngle + 360f - arc.StartAngle) / steps;
                    for (int i = 0; i <= steps; i++)
                    {
                        float angle = arc.StartAngle + i * angleStep;
                        float rad = angle * (float)Math.PI / 180f;
                        float px = arc.Center.X + arc.Radius * (float)Math.Cos(rad);
                        float py = arc.Center.Y + arc.Radius * (float)Math.Sin(rad);
                        minX = Math.Min(minX, px);
                        maxX = Math.Max(maxX, px);
                        minY = Math.Min(minY, py);
                        maxY = Math.Max(maxY, py);
                    }
                }
            }

            if (minX == float.MaxValue) { minX = maxX = minY = maxY = 0f; }

            leftBoundary = new PointF(minX, centerY);
            rightBoundary = new PointF(maxX, centerY);
            topBoundary = new PointF(0, maxY);
            bottomBoundary = new PointF(0, minY);
            cutBoundaryY = mainCutLine != null ? mainCutLine.Start.Y : centerY;
        }

        

        

        private BaffleGeometryGenerator generator = new BaffleGeometryGenerator();

        public IEnumerable<ICadEntity> Render(GeometryModel geometry, EngineeringDataModel data, PointF origin)
        {
            var baffleGeometry = generator.GenerateBaffleGeometry(geometry, data, isTopCut: false);
            var entities = baffleGeometry.Entities;

            // Phase B1: Shell ID Reference
            float baffleRad = baffleGeometry.BaffleOD / 2f;

            // Centerlines
            float clLen = baffleRad + 25f;
            entities.Add(new CadLine { Start = new PointF(-clLen, 0), End = new PointF(clLen, 0), EntityColor = Color.Red, DashStyle = System.Drawing.Drawing2D.DashStyle.DashDot });
            entities.Add(new CadLine { Start = new PointF(0, -clLen), End = new PointF(0, clLen), EntityColor = Color.Red, DashStyle = System.Drawing.Drawing2D.DashStyle.DashDot });

            // Phase B4 & B5 Annotations
            float dimTextHeight = 10f; // Standard dim text height
            float actualCutY = (-geometry.ShellRadius - baffleGeometry.ActualCutDepth);

            // --- DRAFTING & ANNOTATIONS ---
            float dimOffset = 60f;
            // --- EXPLICIT GEOMETRIC REFERENCE DIMENSIONS ---
            ExtractGeometricReferences(baffleGeometry, out PointF topBoundary, out PointF bottomBoundary, out PointF leftBoundary, out PointF rightBoundary, out float cutBoundaryY, out float partitionY, out float centerY);
            
            float leftDimX = leftBoundary.X - dimOffset;
            
            // Determine which boundary is the arc and which is the cut
            float arcBoundaryY = Math.Abs(topBoundary.Y) > Math.Abs(bottomBoundary.Y) ? topBoundary.Y : bottomBoundary.Y;
            
            // 1. Overall Diameter Dimension
            float odDimY = topBoundary.Y < bottomBoundary.Y ? topBoundary.Y - dimOffset : bottomBoundary.Y - dimOffset;

            entities.Add(new CadDimension 
            {
                StartPoint = leftBoundary,
                EndPoint = rightBoundary,
                DimensionLineLocation = new PointF(0, odDimY),
                Type = DimensionType.Horizontal,
                OverrideText = $"%%c{Math.Round(baffleGeometry.BaffleOD)}",
                EntityColor = Color.White
            });

            // 2. Vertical Dimensions
            // Overall Height = Arc Edge -> Cut Edge
            entities.Add(new CadDimension 
            {
                StartPoint = new PointF(0, cutBoundaryY),
                EndPoint = new PointF(0, arcBoundaryY),
                DimensionLineLocation = new PointF(leftDimX - 115f, 0),
                Type = DimensionType.Vertical,
                OverrideText = $"{Math.Round(baffleGeometry.BaffleRadius + Math.Abs(baffleGeometry.SnappedCutLineY))}",
                EntityColor = Color.White
            });
            
            // Radius Dimension = Centerline -> Arc Edge
            entities.Add(new CadDimension 
            {
                StartPoint = new PointF(0, 0),
                EndPoint = new PointF(0, arcBoundaryY),
                DimensionLineLocation = new PointF(leftDimX - 75f, arcBoundaryY / 2f),
                Type = DimensionType.Vertical,
                OverrideText = $"{Math.Round(baffleGeometry.BaffleRadius)}",
                EntityColor = Color.White
            });
            
            // Cut Dimension = Centerline -> Cut Edge
            entities.Add(new CadDimension 
            {
                StartPoint = new PointF(0, 0),
                EndPoint = new PointF(0, cutBoundaryY),
                DimensionLineLocation = new PointF(leftDimX - 35f, cutBoundaryY / 2f),
                Type = DimensionType.Vertical,
                OverrideText = $"{Math.Round(Math.Abs(baffleGeometry.SnappedCutLineY))}",
                EntityColor = Color.White
            });

            // 3. Tube Hole Leader Note
            PointF? targetHole = null;
            if (baffleGeometry.ActiveTubeCenters.Count > 0)
            {
                float maxDist = 0;
                foreach (var pt in baffleGeometry.ActiveTubeCenters)
                {
                    if (pt.X > 0)
                    {
                        float dist = (float)Math.Sqrt(pt.X * pt.X + pt.Y * pt.Y);
                        if (dist > maxDist)
                        {
                            maxDist = dist;
                            targetHole = pt;
                        }
                    }
                }
            }
            
            if (targetHole.HasValue)
            {
                string pitchStr = "TRIANGULAR";
                string leaderText = $"HOLES FOR TUBES Ø{data.TubeOD:F1}\nON {pitchStr} PITCH";
                
                float lStartX = targetHole.Value.X;
                float lStartY = targetHole.Value.Y;
                float lMidX = lStartX + 40f;
                float lMidY = lStartY - 40f;
                float lEndX = lMidX + 20f;
                
                entities.Add(new CadLeader
                {
                    Vertices = new List<PointF> { new PointF(lStartX, lStartY), new PointF(lMidX, lMidY), new PointF(lEndX, lMidY) },
                    HasArrowHead = true,
                    EntityColor = Color.White
                });
                
                entities.Add(new CadMText
                {
                    Position = new PointF(lEndX + 5f, lMidY),
                    Text = leaderText,
                    Alignment = StringAlignment.Near,
                    LineAlignment = StringAlignment.Center,
                    TargetPaperSpaceHeight = 31f,
                    EntityColor = Color.Blue
                });
            }



            // 4. Baffle Identification Text
            entities.Add(new CadMText
            {
                Text = "BAFFLE #2,#4",
                Position = new PointF(0, -baffleRad - 70f),
                EntityColor = Color.Blue,
                Alignment = StringAlignment.Center,
                TargetPaperSpaceHeight = 31f
            });

            // Shift geometry for visual balance and spacing
            float offsetY = 0f;
            foreach (var entity in entities)
            {
                if (entity is CadCircle circle) circle.Center = new PointF(circle.Center.X, circle.Center.Y + offsetY);
                else if (entity is CadLine line) { line.Start = new PointF(line.Start.X, line.Start.Y + offsetY); line.End = new PointF(line.End.X, line.End.Y + offsetY); }
                else if (entity is CadArc arc) arc.Center = new PointF(arc.Center.X, arc.Center.Y + offsetY);
                else if (entity is CadPolyline poly) { foreach (var v in poly.Vertices) v.Point = new PointF(v.Point.X, v.Point.Y + offsetY); }
                else if (entity is CadDimension dim) { dim.StartPoint = new PointF(dim.StartPoint.X, dim.StartPoint.Y + offsetY); dim.EndPoint = new PointF(dim.EndPoint.X, dim.EndPoint.Y + offsetY); dim.DimensionLineLocation = new PointF(dim.DimensionLineLocation.X, dim.DimensionLineLocation.Y + offsetY); }
                else if (entity is CadMText mtext) mtext.Position = new PointF(mtext.Position.X, mtext.Position.Y + offsetY);
                else if (entity is CadText text) text.Position = new PointF(text.Position.X, text.Position.Y + offsetY);
            }

            return entities;
        }
    }
}
