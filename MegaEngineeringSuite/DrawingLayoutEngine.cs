using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace MegaEngineeringSuite
{
    public class DrawingLayoutEngine
    {
        private ICadView rearSideView = new SideViewRenderer("(VIEW FROM E)\nREAR SIDE VIEW");
        private ICadView rearView = new RearTubeSheetView();
        private ICadView frontView = new FrontTubeSheetView();
        private ICadView frontSideView = new SideViewRenderer("(VIEW FROM F)\nFRONT SIDE VIEW");
        
        private ICadView baffleAView = new BaffleAView();
        private ICadView baffleBView = new BaffleBView();

        // Detail Views
        private ICadView detailAView = new DetailAView();
        private ICadView detailBView = new DetailBView();
        private ICadView detailCView = new DetailCView();

        // Documentation Views
        private ICadView specTableView = new SpecificationTableView();
        private ICadView notesBlockView = new NotesBlockView();
        private ICadView titleBlockView = new TitleBlockView();

        public List<DrawingBlock> GenerateBlocks(GeometryModel geometry, EngineeringDataModel data)
        {
            List<DrawingBlock> blocks = new List<DrawingBlock>();
            if (geometry == null || data == null) return blocks;

            // 1. Rear Assembly (Tube Sheet + Side View)
            var rearSheetEntities = rearView.Render(geometry, data, PointF.Empty).ToList();
            var rearSideEntities = rearSideView.Render(geometry, data, new PointF(geometry.OuterDiameter / 2f + 150f, 0)).ToList();
            var rearAssemblyEntities = rearSheetEntities.Concat(rearSideEntities).ToList();
            blocks.Add(new DrawingBlock
            {
                Name = "Rear Assembly",
                Entities = rearAssemblyEntities,
                Zone = SheetZone.TopEngineeringLeft,
                Priority = 1
            });

            // 2. Front Assembly (Tube Sheet + Side View)
            var frontSheetEntities = frontView.Render(geometry, data, PointF.Empty).ToList();
            var frontSideEntities = frontSideView.Render(geometry, data, new PointF(geometry.OuterDiameter / 2f + 150f, 0)).ToList();
            var frontAssemblyEntities = frontSheetEntities.Concat(frontSideEntities).ToList();
            blocks.Add(new DrawingBlock
            {
                Name = "Front Assembly",
                Entities = frontAssemblyEntities,
                Zone = SheetZone.TopEngineeringRight,
                Priority = 1
            });

            // 5. Baffle A
            blocks.Add(new DrawingBlock
            {
                Name = "Baffle Plate A",
                Entities = baffleAView.Render(geometry, data, PointF.Empty).ToList(),
                Zone = SheetZone.MiddleEngineeringLeft,
                Priority = 3
            });

            // 6. Baffle B
            blocks.Add(new DrawingBlock
            {
                Name = "Baffle Plate B",
                Entities = baffleBView.Render(geometry, data, PointF.Empty).ToList(),
                Zone = SheetZone.MiddleEngineeringRight,
                Priority = 3
            });

            // 7. Detail A
            blocks.Add(new DrawingBlock
            {
                Name = "Detail A",
                Entities = detailAView.Render(geometry, data, PointF.Empty).ToList(),
                Zone = SheetZone.BottomEngineeringLeft,
                Priority = 4
            });

            // 8. Detail B
            blocks.Add(new DrawingBlock
            {
                Name = "Detail B",
                Entities = detailBView.Render(geometry, data, PointF.Empty).ToList(),
                Zone = SheetZone.BottomEngineeringMid,
                Priority = 4
            });

            // 9. Detail C
            blocks.Add(new DrawingBlock
            {
                Name = "Detail C",
                Entities = detailCView.Render(geometry, data, PointF.Empty).ToList(),
                Zone = SheetZone.BottomEngineeringRight,
                Priority = 4
            });

            // 10. Documentation Stack (Spec Table, Notes, Title Block)
            var specEntities = specTableView.Render(geometry, data, PointF.Empty).ToList();
            var specBounds = DrawingBoundsCalculator.CalculateBounds(specEntities);
            
            var notesEntities = notesBlockView.Render(geometry, data, PointF.Empty).ToList();
            var notesBounds = DrawingBoundsCalculator.CalculateBounds(notesEntities);
            
            var titleEntities = titleBlockView.Render(geometry, data, PointF.Empty).ToList();
            var titleBounds = DrawingBoundsCalculator.CalculateBounds(titleEntities);

            // Stack them vertically with a fixed gap (e.g., 50mm). We translate Notes and Title down.
            // Spec is at top. 
            // Notes goes below Spec
            float gap = 50f;
            float notesDy = specBounds.MinY - notesBounds.MaxY - gap;
            TranslateEntities(notesEntities, 0, notesDy);
            notesBounds = DrawingBoundsCalculator.CalculateBounds(notesEntities); // Update bounds after move

            // Title goes below Notes
            float titleDy = notesBounds.MinY - titleBounds.MaxY - gap;
            TranslateEntities(titleEntities, 0, titleDy);
            
            // Align them all to the right. Spec is base (0).
            // Shift Notes to right align with Spec
            float notesDx = specBounds.MaxX - notesBounds.MaxX;
            TranslateEntities(notesEntities, notesDx, 0);

            // Shift Title to right align with Spec
            float titleDx = specBounds.MaxX - titleBounds.MaxX;
            TranslateEntities(titleEntities, titleDx, 0);

            var docStackEntities = specEntities.Concat(notesEntities).Concat(titleEntities).ToList();
            blocks.Add(new DrawingBlock
            {
                Name = "Documentation Column",
                Entities = docStackEntities,
                Zone = SheetZone.DocColumnBottom,
                Priority = 5
            });

            // Compute bounds for all
            foreach(var block in blocks)
            {
                block.Bounds = DrawingBoundsCalculator.CalculateBounds(block.Entities);
            }

            return blocks;
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
                else if (entity is CadPolyline polyline)
                {
                    for (int i = 0; i < polyline.Vertices.Count; i++)
                    {
                        polyline.Vertices[i] = new PointF(polyline.Vertices[i].X + dx, polyline.Vertices[i].Y + dy);
                    }
                }
                else if (entity is CadHatch hatch)
                {
                    for (int i = 0; i < hatch.BoundaryVertices.Count; i++)
                    {
                        hatch.BoundaryVertices[i] = new PointF(hatch.BoundaryVertices[i].X + dx, hatch.BoundaryVertices[i].Y + dy);
                    }
                }
            }
        }
    }
}
