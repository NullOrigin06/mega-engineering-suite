using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace loginpage1
{
    public class RowCountRenderer
    {
        public IEnumerable<ICadEntity> GenerateRowCounts(GeometryModel geometry, bool alignLeft = false)
        {
            List<ICadEntity> entities = new List<ICadEntity>();

            if (geometry.RowTubeCounts == null || geometry.TubeCoordinates == null || !geometry.TubeCoordinates.Any())
                return entities;

            var groupedY = geometry.TubeCoordinates
                .GroupBy(pt => Math.Round(pt.Y, 2))
                .OrderByDescending(gY => gY.Key)
                .ToList();

            bool denseMode = groupedY.Count > 10;

            for (int i = 0; i < groupedY.Count && i < geometry.RowTubeCounts.Count; i++)
            {
                if (denseMode && i % 2 != 0) continue;

                float yPos = (float)groupedY[i].Key;
                float safeMargin = geometry.OuterDiameter / 2f + 50f;

                if (alignLeft)
                {
                    float minX = groupedY[i].Min(pt => pt.X);
                    float textX = -safeMargin;
                    
                    entities.Add(new CadText 
                    { 
                        Text = geometry.RowTubeCounts[i].ToString(), 
                        Position = new PointF(textX - 10, yPos - (geometry.TubeRadius)), 
                        EntityColor = Color.Magenta,
                        Alignment = StringAlignment.Far,
                        TargetPaperSpaceHeight = DraftingScaleManager.GetPaperSpaceRowCountHeight()
                    });

                    entities.Add(new CadLine 
                    { 
                        Start = new PointF(minX - geometry.TubeRadius, yPos), 
                        End = new PointF(textX - 5, yPos), 
                        EntityColor = Color.Magenta 
                    });
                }
                else
                {
                    float maxX = groupedY[i].Max(pt => pt.X);
                    float textX = safeMargin;
                    
                    entities.Add(new CadText 
                    { 
                        Text = geometry.RowTubeCounts[i].ToString(), 
                        Position = new PointF(textX + 10, yPos - (geometry.TubeRadius)), 
                        EntityColor = Color.Magenta,
                        Alignment = StringAlignment.Near,
                        TargetPaperSpaceHeight = DraftingScaleManager.GetPaperSpaceRowCountHeight()
                    });

                    entities.Add(new CadLine 
                    { 
                        Start = new PointF(maxX + geometry.TubeRadius, yPos), 
                        End = new PointF(textX + 5, yPos), 
                        EntityColor = Color.Magenta 
                    });
                }
            }

            return entities;
        }
    }
}
