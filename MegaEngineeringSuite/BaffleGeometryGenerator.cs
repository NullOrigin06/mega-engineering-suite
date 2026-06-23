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

            // 3. Filter Tubes
            float tRad = geometry.TubeRadius;
            if (geometry.TubeCoordinates != null)
            {
                foreach (var pt in geometry.TubeCoordinates)
                {
                    if (Math.Abs(pt.Y - actualCutY) < 1.0f) // Tolerance for being on the cut line
                    {
                        baffle.SemicircleTubeCenters.Add(pt);
                    }
                    else if (isTopCut)
                    {
                        if (pt.Y < actualCutY) baffle.RemovedTubeCenters.Add(pt);
                        else baffle.ActiveTubeCenters.Add(pt);
                    }
                    else
                    {
                        if (pt.Y > actualCutY) baffle.RemovedTubeCenters.Add(pt);
                        else baffle.ActiveTubeCenters.Add(pt);
                    }
                }
            }

            // 4. Generate Entities
            // Center lines
            float clLen = baffleRadius + 30;
            baffle.Entities.Add(new CadLine { Start = new PointF(-clLen, 0), End = new PointF(clLen, 0), EntityColor = Color.Red, DashStyle = System.Drawing.Drawing2D.DashStyle.DashDot });
            baffle.Entities.Add(new CadLine { Start = new PointF(0, -clLen), End = new PointF(0, clLen), EntityColor = Color.Red, DashStyle = System.Drawing.Drawing2D.DashStyle.DashDot });

            // Baffle Outline (Arc instead of Circle)
            if (Math.Abs(actualCutY) < baffleRadius)
            {
                float intersectRad = (float)Math.Asin(actualCutY / baffleRadius);
                float startAngle, endAngle;

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

                baffle.Entities.Add(new CadArc 
                { 
                    Center = new PointF(0, 0), 
                    Radius = baffleRadius, 
                    StartAngle = startAngle, 
                    EndAngle = endAngle, 
                    EntityColor = Color.Magenta 
                });

                // Cut Line
                float cutX = (float)Math.Sqrt(baffleRadius * baffleRadius - actualCutY * actualCutY);
                baffle.Entities.Add(new CadLine { Start = new PointF(-cutX, actualCutY), End = new PointF(cutX, actualCutY), EntityColor = Color.Magenta });
            }
            else
            {
                // Fallback if cut is outside radius
                baffle.Entities.Add(new CadCircle { Center = new PointF(0, 0), Radius = baffleRadius, EntityColor = Color.Magenta });
            }

            // Active Tube Holes
            foreach (var pt in baffle.ActiveTubeCenters)
            {
                baffle.Entities.Add(new CadCircle { Center = new PointF(pt.X, pt.Y), Radius = tRad, EntityColor = Color.Blue });
            }

            // Semicircular Tube Holes (Cut Row)
            foreach (var pt in baffle.SemicircleTubeCenters)
            {
                // Baffle A (isTopCut = true): Keep the +Y half of the hole. In Cartesian, this is the upper half.
                // Baffle B (isTopCut = false): Keep the -Y half of the hole. In Cartesian, this is the lower half.
                float startAngle = isTopCut ? 0f : 180f;
                float endAngle = isTopCut ? 180f : 360f; 
                baffle.Entities.Add(new CadArc 
                { 
                    Center = new PointF(pt.X, pt.Y), 
                    Radius = tRad, 
                    StartAngle = startAngle, 
                    EndAngle = endAngle, 
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
