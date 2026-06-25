using System;
using System.Collections.Generic;
using System.Drawing;

namespace MegaEngineeringSuite
{
    public class BaffleAView : ICadView
    {
        private BaffleGeometryGenerator generator = new BaffleGeometryGenerator();

        public IEnumerable<ICadEntity> Render(GeometryModel geometry, EngineeringDataModel data, PointF origin)
        {
            var baffleGeometry = generator.GenerateBaffleGeometry(geometry, data, isTopCut: true);
            var entities = baffleGeometry.Entities;

            // Phase B1: Shell ID Reference
            float baffleRad = baffleGeometry.BaffleOD / 2f;

            // Phase B4 & B5 Annotations
            float dimTextHeight = 10f; // Standard dim text height
            float actualCutY = -(-geometry.ShellRadius - baffleGeometry.ActualCutDepth);

            // 1. Baffle OD Dimension
            entities.Add(new CadDimension 
            { 
                StartPoint = new PointF(0, baffleRad),
                EndPoint = new PointF(0, -baffleRad),
                DimensionLineLocation = new PointF(baffleRad + 15, 0),
                Type = DimensionType.Vertical,
                OverrideText = "BAFFLE O.D. <>",
                EntityColor = Color.Yellow,
                TargetPaperSpaceHeight = dimTextHeight
            });

            // 2. Cut Depth Dimension
            entities.Add(new CadDimension 
            {
                StartPoint = new PointF(-100, -geometry.ShellRadius),
                EndPoint = new PointF(-100, actualCutY),
                DimensionLineLocation = new PointF(-baffleRad - 15, (-geometry.ShellRadius + actualCutY) / 2f),
                Type = DimensionType.Vertical,
                EntityColor = Color.Yellow,
                TargetPaperSpaceHeight = dimTextHeight
            });

            // 3. Information Callout (MText)
            int qty = true ? (int)Math.Ceiling(data.BaffleQty / 2.0) : (int)Math.Floor(data.BaffleQty / 2.0);
            string calloutText = $"BAFFLE A (TOP)\\P\\P" +
                                 $"Baffle O.D. : {data.BaffleOD:F2} mm\\P" +
                                 $"THK         : {data.BaffleTHK:F2} mm\\P" +
                                 $"CUT         : 25 %\\P" +
                                 $"QTY         : {qty} Nos.\\P" +
                                 $"M.O.C.      : {data.Material}";

            entities.Add(new CadMText
            {
                Text = calloutText,
                Position = new PointF(0, -baffleRad - 45),
                EntityColor = Color.White,
                Alignment = StringAlignment.Center,
                TargetPaperSpaceHeight = DraftingScaleManager.GetPaperSpaceStandardNotesHeight()
            });

            // Shift geometry for visual balance and spacing
            float offsetY = -30f;
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
