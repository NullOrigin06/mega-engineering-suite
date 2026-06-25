using System;
using System.Collections.Generic;
using System.Drawing;

namespace MegaEngineeringSuite
{
    public class BaffleBView : ICadView
    {
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
            float leftX = -baffleRad - dimOffset;
            
            // 1. Overall Diameter Dimension
            float odDimY = actualCutY - dimOffset;
            entities.Add(new CadDimension 
            {
                StartPoint = new PointF(-baffleRad, 0),
                EndPoint = new PointF(baffleRad, 0),
                DimensionLineLocation = new PointF(0, odDimY),
                Type = DimensionType.Horizontal,
                OverrideText = "%%c<>",
                EntityColor = Color.White
            });

            // 2. Vertical Dimensions
            // Overall Height
            float bottomY = actualCutY;
            float topY = baffleRad;
            
            entities.Add(new CadDimension 
            {
                StartPoint = new PointF(0, bottomY),
                EndPoint = new PointF(0, topY),
                DimensionLineLocation = new PointF(leftX - 25f, 0),
                Type = DimensionType.Vertical,
                EntityColor = Color.White
            });
            
            // Center to Top
            entities.Add(new CadDimension 
            {
                StartPoint = new PointF(0, 0),
                EndPoint = new PointF(0, topY),
                DimensionLineLocation = new PointF(leftX, topY / 2f),
                Type = DimensionType.Vertical,
                EntityColor = Color.White
            });
            
            // Center to Bottom
            entities.Add(new CadDimension 
            {
                StartPoint = new PointF(0, bottomY),
                EndPoint = new PointF(0, 0),
                DimensionLineLocation = new PointF(leftX, bottomY / 2f),
                Type = DimensionType.Vertical,
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
                string leaderText = $"HOLES FOR TUBES %%c{data.TubeOD}\\PON {pitchStr} PITCH";
                
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
                    TargetPaperSpaceHeight = dimTextHeight,
                    EntityColor = Color.White
                });
            }

            // 4. Baffle Identification Text
            entities.Add(new CadMText
            {
                Text = "BAFFLE #2,#4",
                Position = new PointF(0, -baffleRad - 70f),
                EntityColor = Color.White,
                Alignment = StringAlignment.Center,
                TargetPaperSpaceHeight = 12f
            });

            // Shift geometry for visual balance and spacing
            float offsetY = -80f;
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
