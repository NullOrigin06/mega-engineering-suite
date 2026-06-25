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
#if false
            float clLen = baffleRadius + 15f;
            baffle.Entities.Add(new CadLine { Start = new PointF(-clLen, 0), End = new PointF(clLen, 0), EntityColor = Color.Red, DashStyle = System.Drawing.Drawing2D.DashStyle.DashDot });
            baffle.Entities.Add(new CadLine { Start = new PointF(0, -clLen), End = new PointF(0, clLen), EntityColor = Color.Red, DashStyle = System.Drawing.Drawing2D.DashStyle.DashDot });
#endif

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

                PointF arcStartPoint = isTopCut ? new PointF(startX, startY) : new PointF(endX, endY);
                PointF arcEndPoint = isTopCut ? new PointF(endX, endY) : new PointF(startX, startY);

                // Explicit Arc primitive
                baffle.Entities.Add(new CadArc
                {
                    Center = new PointF(0, 0),
                    Radius = baffleRadius,
                    StartAngle = startAngle,
                    EndAngle = endAngle,
                    EntityColor = Color.Blue
                });

                // Explicit Chord primitive
                // Removed the single continuous CadLine here.
                // We will stitch the cut line dynamically around the tube holes later.
                float sweep = endAngle - startAngle;
                if (sweep < 0) sweep += 360f;
                boundaryPath.AddArc(-baffleRadius, -baffleRadius, baffleRadius * 2, baffleRadius * 2, startAngle, sweep);
                boundaryPath.CloseFigure();
            }
            else
            {
                // Fallback if cut is outside radius
                baffle.Entities.Add(new CadCircle { Center = new PointF(0, 0), Radius = baffleRadius, EntityColor = Color.Blue });
                boundaryPath.AddEllipse(-baffleRadius, -baffleRadius, baffleRadius * 2, baffleRadius * 2);
            }

            // 4. Filter Tubes using exact boundary
            float tRad = geometry.TubeRadius;
            if (geometry.TubeCoordinates != null)
            {
                foreach (var pt in geometry.TubeCoordinates)
                {
                    // Exclude tubes entirely outside the baffle OD (accounting for clearance)
                    if (Math.Sqrt(pt.X * pt.X + pt.Y * pt.Y) > (baffleRadius - tRad))
                    {
                        continue;
                    }

                    if (float.IsNaN(actualCutY))
                    {
                        baffle.ActiveTubeCenters.Add(pt);
                    }
                    else
                    {
                        TubeCutClassification classification = ClassifyTubeAgainstCut(pt, actualCutY, tRad, isTopCut);

                        if (classification == TubeCutClassification.Inside)
                        {
                            baffle.ActiveTubeCenters.Add(pt);
                        }
                        else if (classification == TubeCutClassification.Intersecting)
                        {
                            baffle.SemicircleTubeCenters.Add(pt);
                        }
                        else
                        {
                            baffle.RemovedTubeCenters.Add(pt);
                        }
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
                var clippedArc = CreateCutClippedTubeArc(pt, tRad, actualCutY, isTopCut);
                clippedArc.EntityColor = Color.Blue;
                baffle.Entities.Add(clippedArc);
            }

            // Stitch the cut line boundary segments between tube cutouts
            if (Math.Abs(actualCutY) < baffleRadius)
            {
                float intersectRad = (float)Math.Asin(actualCutY / baffleRadius);
                float startAngleRad, endAngleRad;
                if (isTopCut)
                {
                    startAngleRad = intersectRad;
                    endAngleRad = (float)(Math.PI - intersectRad);
                }
                else
                {
                    startAngleRad = (float)(Math.PI - intersectRad);
                    endAngleRad = intersectRad;
                }

                float leftEdgeX = baffleRadius * (float)Math.Cos(endAngleRad);
                float rightEdgeX = baffleRadius * (float)Math.Cos(startAngleRad);
                float cutLineLeftX = Math.Min(leftEdgeX, rightEdgeX);
                float cutLineRightX = Math.Max(leftEdgeX, rightEdgeX);

                var holes = new List<Tuple<float, float>>();
                foreach (var pt in baffle.SemicircleTubeCenters)
                {
                    float yDist = actualCutY - pt.Y;
                    float xDistSq = tRad * tRad - yDist * yDist;
                    if (xDistSq >= 0)
                    {
                        float xDist = (float)Math.Sqrt(xDistSq);
                        holes.Add(Tuple.Create(pt.X - xDist, pt.X + xDist));
                    }
                }

                holes.Sort((a, b) => a.Item1.CompareTo(b.Item1));

                float currentX = cutLineLeftX;
                foreach (var hole in holes)
                {
                    if (hole.Item1 > currentX)
                    {
                        baffle.Entities.Add(new CadLine
                        {
                            Start = new PointF(currentX, actualCutY),
                            End = new PointF(hole.Item1, actualCutY),
                            EntityColor = Color.Blue
                        });
                    }
                    currentX = Math.Max(currentX, hole.Item2);
                }

                if (currentX < cutLineRightX)
                {
                    baffle.Entities.Add(new CadLine
                    {
                        Start = new PointF(currentX, actualCutY),
                        End = new PointF(cutLineRightX, actualCutY),
                        EntityColor = Color.Blue
                    });
                }
            }

            // Partitions (only within valid region)
#if false
            if (geometry.NumberOfPasses == 2 || geometry.NumberOfPasses == 4)
            {
                if ((isTopCut && actualCutY < 0) || (!isTopCut && actualCutY > 0)) 
                {
                    baffle.Entities.Add(new CadLine { Start = new PointF(-baffleRadius, 0), End = new PointF(baffleRadius, 0), EntityColor = Color.Blue });
                }
                
                if (geometry.NumberOfPasses == 4)
                {
                    float startY = isTopCut ? actualCutY : -baffleRadius;
                    float endY = isTopCut ? baffleRadius : actualCutY;
                    baffle.Entities.Add(new CadLine { Start = new PointF(0, startY), End = new PointF(0, endY), EntityColor = Color.Blue });
                }
            }
#endif

            return baffle;
        }
        public enum TubeCutClassification
        {
            Inside,
            Intersecting,
            Outside
        }

        private static TubeCutClassification ClassifyTubeAgainstCut(PointF tubeCenter, float cutY, float tubeRadius, bool isTopCut)
        {
            float signedDistance = isTopCut ? tubeCenter.Y - cutY : cutY - tubeCenter.Y;

            if (signedDistance >= tubeRadius - 0.001f)
            {
                return TubeCutClassification.Inside;
            }
            if (signedDistance <= -tubeRadius + 0.001f)
            {
                return TubeCutClassification.Outside;
            }
            return TubeCutClassification.Intersecting;
        }

        private static CadArc CreateCutClippedTubeArc(PointF tubeCenter, float tubeRadius, float cutY, bool isTopCut)
        {
            float relativeCutY = (cutY - tubeCenter.Y) / tubeRadius;
            relativeCutY = Math.Max(-1f, Math.Min(1f, relativeCutY));

            float intersectionAngle = (float)(Math.Asin(relativeCutY) * 180.0 / Math.PI);
            float startAngle;
            float endAngle;

            if (isTopCut)
            {
                startAngle = NormalizeAngle(intersectionAngle);
                endAngle = NormalizeAngle(180f - intersectionAngle);
            }
            else
            {
                startAngle = NormalizeAngle(180f - intersectionAngle);
                endAngle = NormalizeAngle(intersectionAngle);
                if (endAngle == 0f)
                {
                    endAngle = 360f;
                }
            }

            return new CadArc
            {
                Center = new PointF(tubeCenter.X, tubeCenter.Y),
                Radius = tubeRadius,
                StartAngle = startAngle,
                EndAngle = endAngle,
                EntityColor = Color.Blue
            };
        }

        private static float NormalizeAngle(float angle)
        {
            angle %= 360f;
            if (angle < 0) angle += 360f;
            return angle;
        }
    }
}
