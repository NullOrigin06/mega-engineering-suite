using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace MegaEngineeringSuite
{
    public class BaffleGeometryGenerator
    {
                public BaffleGeometry GenerateBaffleGeometry(GeometryModel geometry, EngineeringDataModel data, bool isTopCut)
        {
            var baffle = new BaffleGeometry();
            
            baffle.BaffleOD = (float)data.ShellID - 5.0f;
            System.Diagnostics.Debug.WriteLine("BaffleOD auto-calculated as ShellID - 5 mm");

            baffle.BaffleThickness = (float)data.BaffleTHK;
            float baffleRadius = baffle.BaffleOD / 2f;

            // 1. Calculate Theoretical Cut Depth (from center)
            float cutSign = isTopCut ? -1f : 1f;
            float theoreticalCutY = cutSign * (geometry.ShellRadius - (geometry.ShellRadius * 2f * 0.25f)); 

            baffle.TheoreticalCutDepth = (float)(geometry.ShellRadius * 2f * 0.25f);

            // 2. Adjust cut to the nearest valid tube row
            float actualCutY = theoreticalCutY;
            if (geometry.TubeCoordinates != null && geometry.TubeCoordinates.Count > 0)
            {
                var rowYs = geometry.TubeCoordinates.Select(p => p.Y).Distinct().OrderBy(y => y).ToList();
                float closestRowY = rowYs.OrderBy(y => Math.Abs(y - theoreticalCutY)).First();
                actualCutY = closestRowY;
            }

            baffle.ActualCutDepth = geometry.ShellRadius - Math.Abs(actualCutY);

            // 3. Generate Boundary Entities
            // Center lines
            float clLen = baffleRadius + 15f;
            baffle.Entities.Add(new CadLine { Start = new PointF(-clLen, 0), End = new PointF(clLen, 0), EntityColor = Color.Red, DashStyle = System.Drawing.Drawing2D.DashStyle.DashDot });
            baffle.Entities.Add(new CadLine { Start = new PointF(0, -clLen), End = new PointF(0, clLen), EntityColor = Color.Red, DashStyle = System.Drawing.Drawing2D.DashStyle.DashDot });

            float startAngle = 0f;
            float endAngle = 360f;
            System.Drawing.Drawing2D.GraphicsPath boundaryPath = new System.Drawing.Drawing2D.GraphicsPath();

            // Baffle Outline
            if (Math.Abs(actualCutY) < baffleRadius)
            {
                float intersectRad = (float)Math.Asin(actualCutY / baffleRadius);

                if (isTopCut)
                {
                    // Cut is at negative Y. Keep positive Y (bottom).
                    float startDeg = (float)(intersectRad * 180.0 / Math.PI);
                    if (startDeg < 0) startDeg += 360f;
                    startAngle = startDeg; 
                    endAngle = 180f - (float)(intersectRad * 180.0 / Math.PI); 
                }
                else
                {
                    // Cut is at positive Y. Keep negative Y (top).
                    float startDeg = 180f - (float)(intersectRad * 180.0 / Math.PI); 
                    startAngle = startDeg;
                    endAngle = (float)(intersectRad * 180.0 / Math.PI); 
                    if (endAngle < 0) endAngle += 360f;
                }

                float sweepDeg = endAngle - startAngle;
                if (sweepDeg < 0) sweepDeg += 360f;
                float sweepRad = (float)(sweepDeg * Math.PI / 180.0);
                float bulge = (float)Math.Tan(sweepRad / 4.0);

                float startX = baffleRadius * (float)Math.Cos(startAngle * Math.PI / 180.0);
                float startY = baffleRadius * (float)Math.Sin(startAngle * Math.PI / 180.0);

                float endX = baffleRadius * (float)Math.Cos(endAngle * Math.PI / 180.0);
                float endY = baffleRadius * (float)Math.Sin(endAngle * Math.PI / 180.0);

                var poly = new CadPolyline
                {
                    EntityColor = Color.Magenta,
                    IsClosed = true,
                    Vertices = new System.Collections.Generic.List<CadPolylineVertex>
                    {
                        new CadPolylineVertex(new PointF(startX, startY), bulge),
                        new CadPolylineVertex(new PointF(endX, endY), 0f)
                    }
                };
                baffle.Entities.Add(poly);

                float sweep = endAngle - startAngle;
                if (sweep < 0) sweep += 360f;
                boundaryPath.AddArc(-baffleRadius, -baffleRadius, baffleRadius * 2, baffleRadius * 2, startAngle, sweep);
                boundaryPath.CloseFigure();
            }
            else
            {
                // Fallback if cut is outside radius
                baffle.Entities.Add(new CadCircle { Center = new PointF(0, 0), Radius = baffleRadius, EntityColor = Color.Magenta });
                boundaryPath.AddEllipse(-baffleRadius, -baffleRadius, baffleRadius * 2, baffleRadius * 2);
            }

            // 4. Filter Tubes using exact boundary
            float tRad = geometry.TubeRadius;
            if (geometry.TubeCoordinates != null)
            {
                foreach (var pt in geometry.TubeCoordinates)
                {
                    if (Math.Abs(pt.Y - actualCutY) < 1.0f) // Semicircles on cut line
                    {
                        // Even if it's on the cut line, verify it's inside the horizontal span of the cut
                        float maxCutX = (float)Math.Sqrt(baffleRadius * baffleRadius - actualCutY * actualCutY);
                        if (Math.Abs(pt.X) <= maxCutX)
                        {
                            baffle.SemicircleTubeCenters.Add(pt);
                        }
                        else
                        {
                            baffle.RemovedTubeCenters.Add(pt);
                        }
                    }
                    else if (boundaryPath.IsVisible(pt))
                    {
                        baffle.ActiveTubeCenters.Add(pt);
                    }
                    else
                    {
                        baffle.RemovedTubeCenters.Add(pt);
                    }
                }
            }

            // Active Tube Holes
            foreach (var pt in baffle.ActiveTubeCenters)
            {
                baffle.Entities.Add(new CadCircle { Center = new PointF(pt.X, pt.Y), Radius = tRad, EntityColor = Color.Blue });
            }

            // Semicircular Tube Holes (Cut Row)
            foreach (var pt in baffle.SemicircleTubeCenters)
            {
                float sAngle = isTopCut ? 0f : 180f;
                float eAngle = isTopCut ? 180f : 360f; 
                baffle.Entities.Add(new CadArc 
                { 
                    Center = new PointF(pt.X, pt.Y), 
                    Radius = tRad, 
                    StartAngle = sAngle, 
                    EndAngle = eAngle, 
                    EntityColor = Color.Blue 
                });
            }

            // Partitions (only within valid region)
            if (geometry.NumberOfPasses == 2 || geometry.NumberOfPasses == 4)
            {
                if ((isTopCut && actualCutY < 0) || (!isTopCut && actualCutY > 0)) 
                {
                    baffle.Entities.Add(new CadLine { Start = new PointF(-baffleRadius, 0), End = new PointF(baffleRadius, 0), EntityColor = Color.Yellow });
                }
                
                if (geometry.NumberOfPasses == 4)
                {
                    float startY = isTopCut ? actualCutY : -baffleRadius;
                    float endY = isTopCut ? baffleRadius : actualCutY;
                    baffle.Entities.Add(new CadLine { Start = new PointF(0, startY), End = new PointF(0, endY), EntityColor = Color.Yellow });
                }
            }

            return baffle;
        }
    }
}
