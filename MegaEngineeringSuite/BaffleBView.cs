using System.Collections.Generic;
using System.Drawing;

namespace MegaEngineeringSuite
{
    public class BaffleBView : ICadView
    {
        private BaffleGeometryGenerator generator = new BaffleGeometryGenerator();

        public IEnumerable<ICadEntity> Render(GeometryModel geometry, EngineeringDataModel data, PointF origin)
        {
            var baffleGeometry = generator.GenerateBaffleGeometry(geometry, data, isTopCut: false);
            var entities = baffleGeometry.Entities;

            // Phase 2 Annotations
            float baffleRad = baffleGeometry.BaffleOD / 2f;
            AnnotationPlacementEngine annotationEngine = new AnnotationPlacementEngine();
            List<CalloutLeader> callouts = new List<CalloutLeader>();

            float stdTextHeight = DraftingScaleManager.GetPaperSpaceStandardNotesHeight();
            float titleTextHeight = DraftingScaleManager.GetPaperSpaceMainTitleHeight();

            // 1. Title
            entities.Add(new CadText 
            { 
                Text = "BAFFLE PLATE B\n(BOTTOM OPENING)", 
                Position = new PointF(0, (geometry.OuterDiameter / 2f) + 40), 
                EntityColor = Color.DodgerBlue, 
                Alignment = StringAlignment.Center, 
                LineAlignment = StringAlignment.Center, 
                TargetPaperSpaceHeight = titleTextHeight 
            });

            // 2. Thickness Note
            entities.Add(new CadText
            {
                Text = $"{baffleGeometry.BaffleThickness} THK",
                Position = new PointF(0, baffleRad + 100 - stdTextHeight * 2.5f),
                EntityColor = Color.White,
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center,
                TargetPaperSpaceHeight = stdTextHeight
            });

            // 3. OD Dimension
            float dimTextHeight = DraftingScaleManager.GetPaperSpaceDimensionHeight();
            entities.Add(new CadDimension 
            { 
                SelectionPoint = new PointF((float)(baffleRad * Math.Cos(Math.PI/6)), (float)(baffleRad * Math.Sin(Math.PI/6))),
                DimensionLineLocation = new PointF(baffleRad + 80, baffleRad + 80),
                Type = DimensionType.Diameter,
                OverrideText = "BAFFLE Ø<>",
                EntityColor = Color.Magenta,
                TargetPaperSpaceHeight = dimTextHeight
            });

            // 4. Cut Depth Dimension and Note
            float actualCutY = (geometry.ShellRadius - baffleGeometry.ActualCutDepth);
            entities.Add(new CadDimension 
            {
                StartPoint = new PointF(-100, geometry.ShellRadius),
                EndPoint = new PointF(-100, actualCutY),
                DimensionLineLocation = new PointF(-baffleRad - 60, (geometry.ShellRadius + actualCutY) / 2f),
                Type = DimensionType.Vertical,
                EntityColor = Color.Magenta,
                TargetPaperSpaceHeight = dimTextHeight
            });
            
            callouts.Add(new CalloutLeader
            {
                Text = "CUT TO ROW CENTERLINE",
                TargetPoint = new PointF(0, actualCutY),
                AlignRight = false
            });

            // 5. Tube Hole Count (Lower Left)
            // Wait, Baffle B is Bottom Cut. Lower part is removed. Top part is kept.
            // Target point should be Top Left.
            callouts.Add(new CalloutLeader
            {
                Text = $"{baffleGeometry.ActiveTubeCenters.Count} HOLES FOR TUBES\nON TRIANGULAR PITCH",
                TargetPoint = new PointF((float)(-baffleRad * 0.5 * Math.Cos(Math.PI/4)), (float)(-baffleRad * 0.5 * Math.Sin(Math.PI/4))),
                AlignRight = false
            });

            entities.AddRange(annotationEngine.GenerateAnnotations(callouts, baffleRad));

            // Translate
            TranslateEntities(entities, origin.X, origin.Y);

            return entities;
        }

        private void TranslateEntities(List<ICadEntity> entities, float dx, float dy)
        {
            if (dx == 0 && dy == 0) return;

            foreach (var entity in entities)
            {
                if (entity is CadCircle circle)
                {
                    circle.Center = new PointF(circle.Center.X + dx, circle.Center.Y + dy);
                }
                else if (entity is CadArc arc)
                {
                    arc.Center = new PointF(arc.Center.X + dx, arc.Center.Y + dy);
                }
                else if (entity is CadLine line)
                {
                    line.Start = new PointF(line.Start.X + dx, line.Start.Y + dy);
                    line.End = new PointF(line.End.X + dx, line.End.Y + dy);
                }
                else if (entity is CadText text)
                {
                    text.Position = new PointF(text.Position.X + dx, text.Position.Y + dy);
                }
                else if (entity is CadDimension dim)
                {
                    dim.StartPoint = new PointF(dim.StartPoint.X + dx, dim.StartPoint.Y + dy);
                    dim.EndPoint = new PointF(dim.EndPoint.X + dx, dim.EndPoint.Y + dy);
                    dim.SelectionPoint = new PointF(dim.SelectionPoint.X + dx, dim.SelectionPoint.Y + dy);
                    dim.DimensionLineLocation = new PointF(dim.DimensionLineLocation.X + dx, dim.DimensionLineLocation.Y + dy);
                }
                else if (entity is CadLeader leader)
                {
                    for (int i = 0; i < leader.Vertices.Count; i++)
                    {
                        leader.Vertices[i] = new PointF(leader.Vertices[i].X + dx, leader.Vertices[i].Y + dy);
                    }
                }
            }
        }
    }
}
