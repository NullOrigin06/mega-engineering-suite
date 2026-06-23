using System.Collections.Generic;
using System.Drawing;

namespace MegaEngineeringSuite
{
    public class DimensionRenderer
    {
        public IEnumerable<ICadEntity> GenerateVerticalDimension(float xPos, float yStart, float yEnd, string text, Color color, float textHeight = 10f)
        {
            List<ICadEntity> entities = new List<ICadEntity>();

            entities.Add(new CadDimension 
            { 
                StartPoint = new PointF(xPos + 10, yStart), // Assuming the geometry is offset from dim line
                EndPoint = new PointF(xPos + 10, yEnd),
                DimensionLineLocation = new PointF(xPos, (yStart + yEnd) / 2f),
                Type = DimensionType.Vertical,
                OverrideText = text,
                EntityColor = color,
                TargetPaperSpaceHeight = textHeight
            });

            return entities;
        }
    }
}
