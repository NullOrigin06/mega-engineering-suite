using System;
using System.Collections.Generic;
using System.Drawing;
namespace MegaEngineeringSuite
{
    public class RowCountRenderer
    {
        private const string RowCountLayerName = "ROW_COUNT";
        private readonly RowCountLayoutService rowCountLayoutService = new RowCountLayoutService();

        public IEnumerable<ICadEntity> GenerateRowCounts(GeometryModel geometry, bool alignLeft = false)
        {
            List<ICadEntity> entities = new List<ICadEntity>();

            var rowReferences = rowCountLayoutService.GenerateLayout(geometry, alignLeft);
            if (rowReferences.Count == 0)
            {
                return entities;
            }

            foreach (var rowReference in rowReferences)
            {
                entities.Add(new CadLine
                {
                    Start = new PointF(rowReference.LineStartX, rowReference.RowY),
                    End = new PointF(rowReference.TextAnchorX, rowReference.RowY),
                    EntityColor = Color.Red,
                    DashStyle = System.Drawing.Drawing2D.DashStyle.DashDotDot,
                    LayerName = RowCountLayerName,
                    LinetypeName = "PHANTOM"
                });

                entities.Add(new CadText
                {
                    Text = rowReference.Count.ToString(),
                    Position = new PointF(rowReference.TextPositionX, rowReference.RowY),
                    EntityColor = Color.Blue,
                    Alignment = alignLeft ? StringAlignment.Far : StringAlignment.Near,
                    LineAlignment = StringAlignment.Center,
                    TargetPaperSpaceHeight = 31f,
                    LayerName = RowCountLayerName
                });
            }

            return entities;
        }
    }
}
