using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace MegaEngineeringSuite
{
    public class RowCountReference
    {
        public float RowY { get; set; }
        public float LineStartX { get; set; }
        public float TextAnchorX { get; set; }
        public float TextPositionX { get; set; }
        public int Count { get; set; }
    }

    public class RowCountLayoutService
    {
        private const float TubeEdgeClearance = 2f;
        private const float InnerTextOffset = 96f;
        private const float StaggerOffset = 48f;
        private const float LeaderTextGap = 10f;

        public List<RowCountReference> GenerateLayout(GeometryModel geometry, bool alignLeft = false)
        {
            if (geometry == null)
            {
                return new List<RowCountReference>();
            }

            return GenerateLayout(
                geometry.TubeCoordinates,
                geometry.TubeRadius,
                geometry.OuterDiameter,
                geometry.RowTubeCounts,
                alignLeft);
        }

        public List<RowCountReference> GenerateLayout(
            IEnumerable<PointF> tubeCoordinates,
            float tubeRadius,
            float outerDiameter,
            bool alignLeft = false)
        {
            return GenerateLayout(
                tubeCoordinates,
                tubeRadius,
                outerDiameter,
                Array.Empty<int>(),
                alignLeft);
        }

        public List<RowCountReference> GenerateLayout(
            IEnumerable<PointF> tubeCoordinates,
            float tubeRadius,
            float outerDiameter,
            IReadOnlyList<int> rowTubeCounts,
            bool alignLeft = false)
        {
            if (tubeCoordinates == null)
            {
                return new List<RowCountReference>();
            }

            var groupedRows = tubeCoordinates
                .GroupBy(pt => Math.Round(pt.Y, 2))
                .OrderByDescending(group => group.Key)
                .ToList();

            if (!groupedRows.Any())
            {
                return new List<RowCountReference>();
            }

            float outerRadius = outerDiameter / 2f;
            float innerTextColumnX = outerRadius + InnerTextOffset;
            float outerTextColumnX = innerTextColumnX + StaggerOffset;

            List<RowCountReference> layout = new List<RowCountReference>(groupedRows.Count);
            for (int i = 0; i < groupedRows.Count; i++)
            {
                var row = groupedRows[i];
                int count = i < rowTubeCounts.Count ? rowTubeCounts[i] : row.Count();
                float y = (float)row.Key;
                float val = (outerRadius * outerRadius) - (y * y);
                float boundaryX = val > 0 ? (float)Math.Sqrt(val) : 0f;
                
                float lineStartX = alignLeft
                    ? -boundaryX - TubeEdgeClearance
                    : boundaryX + TubeEdgeClearance;

                bool useOuterColumn = i % 2 != 0;
                float textColumnX = useOuterColumn ? outerTextColumnX : innerTextColumnX;
                float currentTextPosX = alignLeft ? -textColumnX : textColumnX;
                float currentLeaderEndX = alignLeft
                    ? currentTextPosX + LeaderTextGap
                    : currentTextPosX - LeaderTextGap;

                layout.Add(new RowCountReference
                {
                    RowY = (float)row.Key,
                    LineStartX = lineStartX,
                    TextAnchorX = currentLeaderEndX,
                    TextPositionX = currentTextPosX,
                    Count = count
                });
            }

            return layout;
        }
    }
}
