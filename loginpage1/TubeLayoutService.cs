using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace loginpage1
{
    public class TubeLayoutService
    {
        public List<PointF> GenerateLayout(float shellRadius, float tubeOD, int tubeQty, float partitionPlateTHK, int noOfPass)
        {
            List<PointF> validTubes = new List<PointF>();
            float tubeRadius = tubeOD / 2f;
            float tubePitch = tubeOD * 1.25f;
            float rowSpacing = tubePitch * 0.866025f; // sin(60 deg)

            // Outer Tube Limit boundary rule
            float maxDistance = shellRadius - tubeRadius;

            // Generate a grid that's oversized to ensure we capture enough tubes.
            int maxRows = (int)Math.Ceiling(shellRadius / rowSpacing) + 5;
            int maxCols = (int)Math.Ceiling(shellRadius / tubePitch) + 5;

            // 1-Pass: Single continuous grid centered at (0, 0)
            if (noOfPass == 1)
            {
                for (int row = -maxRows; row <= maxRows; row++)
                {
                    float y = row * rowSpacing;
                    float xOffset = (Math.Abs(row) % 2 == 1) ? (tubePitch / 2f) : 0f;

                    for (int col = -maxCols; col <= maxCols; col++)
                    {
                        float x = col * tubePitch + xOffset;

                        // Validation: Inside Outer Tube Limit
                        if ((x * x + y * y) <= maxDistance * maxDistance)
                            validTubes.Add(new PointF(x, y));
                    }
                }
            }
            // 2-Pass: Split horizontally into Top and Bottom halves. Exact 25mm offset.
            else if (noOfPass == 2)
            {
                // Top half
                for (int row = 0; row <= maxRows; row++)
                {
                    float y = 25.0f + (row * rowSpacing);
                    float xOffset = (row % 2 == 1) ? (tubePitch / 2f) : 0f;
                    
                    for (int col = -maxCols; col <= maxCols; col++)
                    {
                        float x = col * tubePitch + xOffset;
                        if ((x * x + y * y) <= maxDistance * maxDistance)
                            validTubes.Add(new PointF(x, y));
                    }
                }
                
                // Bottom half
                for (int row = 0; row <= maxRows; row++)
                {
                    float y = -(25.0f + (row * rowSpacing));
                    float xOffset = (row % 2 == 1) ? (tubePitch / 2f) : 0f;
                    
                    for (int col = -maxCols; col <= maxCols; col++)
                    {
                        float x = col * tubePitch + xOffset;
                        if ((x * x + y * y) <= maxDistance * maxDistance)
                            validTubes.Add(new PointF(x, y));
                    }
                }
            }
            // 4-Pass: Split into 4 exact quadrants. Exact 25mm offset in both X and Y.
            else if (noOfPass == 4)
            {
                for (int row = 0; row <= maxRows; row++)
                {
                    float yAbs = 25.0f + (row * rowSpacing);
                    float xOffset = (row % 2 == 1) ? (tubePitch / 2f) : 0f;
                    
                    for (int col = 0; col <= maxCols; col++)
                    {
                        float xAbs = 25.0f + (col * tubePitch) + xOffset;

                        // Top-Right
                        if ((xAbs * xAbs + yAbs * yAbs) <= maxDistance * maxDistance)
                            validTubes.Add(new PointF(xAbs, yAbs));
                            
                        // Top-Left
                        if ((xAbs * xAbs + yAbs * yAbs) <= maxDistance * maxDistance)
                            validTubes.Add(new PointF(-xAbs, yAbs));
                            
                        // Bottom-Right
                        if ((xAbs * xAbs + yAbs * yAbs) <= maxDistance * maxDistance)
                            validTubes.Add(new PointF(xAbs, -yAbs));
                            
                        // Bottom-Left
                        if ((xAbs * xAbs + yAbs * yAbs) <= maxDistance * maxDistance)
                            validTubes.Add(new PointF(-xAbs, -yAbs));
                    }
                }
            }

            // Sort by distance from center to get a circular bundle
            var sortedTubes = validTubes.OrderBy(pt => Math.Sqrt(pt.X * pt.X + pt.Y * pt.Y)).ToList();

            return sortedTubes.Take(tubeQty).ToList();
        }
    }
}
