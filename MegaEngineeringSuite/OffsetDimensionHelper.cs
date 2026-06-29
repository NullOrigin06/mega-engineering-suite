using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace MegaEngineeringSuite
{
    public static class OffsetDimensionHelper
    {
        public static PointF? GetReferenceHole(IEnumerable<PointF> tubeCoordinates)
        {
            if (tubeCoordinates == null || !tubeCoordinates.Any()) return null;
            var candidates = tubeCoordinates.Where(p => p.X > 0 && p.Y > 0).ToList();
            if (!candidates.Any()) return null;
            
            // Order by closest to origin
            return candidates.OrderBy(p => p.X * p.X + p.Y * p.Y).First();
        }

        public static IEnumerable<ICadEntity> GenerateOffsetDimensions(IEnumerable<PointF> tubeCoordinates, float outerDiameter = 1000f)
        {
            var entities = new List<ICadEntity>();
            var refHoleOpt = GetReferenceHole(tubeCoordinates);
            if (refHoleOpt == null) return entities;
            var refHole = refHoleOpt.Value;

            // 1. Projection Lines
            entities.Add(new CadLine
            {
                Start = refHole,
                End = new PointF(refHole.X, 0),
                LayerName = "CENTER",
                LinetypeName = "CENTER",
                DashStyle = System.Drawing.Drawing2D.DashStyle.DashDot,
                EntityColor = Color.Yellow
            });
            entities.Add(new CadLine
            {
                Start = refHole,
                End = new PointF(0, refHole.Y),
                LayerName = "CENTER",
                LinetypeName = "CENTER",
                DashStyle = System.Drawing.Drawing2D.DashStyle.DashDot,
                EntityColor = Color.Yellow
            });

            float outerRadius = outerDiameter / 2f;
            float dimOffset = outerRadius + 80f;

            // 2. Horizontal Offset (Measure X)
            entities.Add(new CadDimension
            {
                StartPoint = new PointF(0, 0),
                EndPoint = new PointF(refHole.X, 0),
                DimensionLineLocation = new PointF(refHole.X / 2f, -dimOffset), // below the hole
                Type = DimensionType.Horizontal,
                OverrideText = Math.Round(refHole.X, 1).ToString(),
                TargetPaperSpaceHeight = 31f,
                TextHeight = 31f,
                EntityColor = Color.Blue
            });

            // 3. Vertical Offset (Measure Y)
            entities.Add(new CadDimension
            {
                StartPoint = new PointF(0, 0),
                EndPoint = new PointF(0, refHole.Y),
                DimensionLineLocation = new PointF(-dimOffset, refHole.Y / 2f), // left of the hole
                Type = DimensionType.Vertical,
                OverrideText = Math.Round(refHole.Y, 1).ToString(),
                TargetPaperSpaceHeight = 31f,
                TextHeight = 31f,
                EntityColor = Color.Blue
            });

            return entities;
        }
    }
}
