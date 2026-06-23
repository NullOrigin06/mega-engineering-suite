using System.Collections.Generic;
using System.Drawing;

namespace MegaEngineeringSuite
{
    public class SideViewRenderer : ICadView
    {
        private DimensionRenderer dimRenderer = new DimensionRenderer();
        private string titleText;

        public SideViewRenderer(string titleText)
        {
            this.titleText = titleText;
        }

        public IEnumerable<ICadEntity> Render(GeometryModel geometry, EngineeringDataModel data, PointF origin)
        {
            List<ICadEntity> entities = new List<ICadEntity>();

            float tsThk = (float)data.TubeSheetFinishTHK;
            if (tsThk <= 0) tsThk = 25f; // Default fallback
            
            float tsHeight = geometry.OuterDiameter;
            
            // Tube Sheet Body Lines (Left, Right, Top, Bottom)
            entities.Add(new CadLine { Start = new PointF(-tsThk / 2f, -tsHeight / 2f), End = new PointF(tsThk / 2f, -tsHeight / 2f), EntityColor = Color.Blue });
            entities.Add(new CadLine { Start = new PointF(-tsThk / 2f, tsHeight / 2f), End = new PointF(tsThk / 2f, tsHeight / 2f), EntityColor = Color.Blue });
            entities.Add(new CadLine { Start = new PointF(-tsThk / 2f, -tsHeight / 2f), End = new PointF(-tsThk / 2f, tsHeight / 2f), EntityColor = Color.Blue });
            entities.Add(new CadLine { Start = new PointF(tsThk / 2f, -tsHeight / 2f), End = new PointF(tsThk / 2f, tsHeight / 2f), EntityColor = Color.Blue });

            // Centerline horizontal
            entities.Add(new CadLine { Start = new PointF(-tsThk - 20, 0), End = new PointF(tsThk + 20, 0), EntityColor = Color.Red, DashStyle = System.Drawing.Drawing2D.DashStyle.DashDot });

            // Bolt holes representation in cross section (top and bottom)
            float pcdOffset = geometry.BoltPcdRadius;
            float holeRad = geometry.BoltHoleRadius;
            
            // Top hole section (Rectangle drawn as 4 lines for CAD)
            entities.Add(new CadLine { Start = new PointF(-tsThk / 2f, -pcdOffset - holeRad), End = new PointF(tsThk / 2f, -pcdOffset - holeRad), EntityColor = Color.Cyan });
            entities.Add(new CadLine { Start = new PointF(-tsThk / 2f, -pcdOffset + holeRad), End = new PointF(tsThk / 2f, -pcdOffset + holeRad), EntityColor = Color.Cyan });
            entities.Add(new CadLine { Start = new PointF(-tsThk / 2f - 10, -pcdOffset), End = new PointF(tsThk / 2f + 10, -pcdOffset), EntityColor = Color.Red, DashStyle = System.Drawing.Drawing2D.DashStyle.DashDot });

            // Bottom hole section
            entities.Add(new CadLine { Start = new PointF(-tsThk / 2f, pcdOffset - holeRad), End = new PointF(tsThk / 2f, pcdOffset - holeRad), EntityColor = Color.Cyan });
            entities.Add(new CadLine { Start = new PointF(-tsThk / 2f, pcdOffset + holeRad), End = new PointF(tsThk / 2f, pcdOffset + holeRad), EntityColor = Color.Cyan });
            entities.Add(new CadLine { Start = new PointF(-tsThk / 2f - 10, pcdOffset), End = new PointF(tsThk / 2f + 10, pcdOffset), EntityColor = Color.Red, DashStyle = System.Drawing.Drawing2D.DashStyle.DashDot });

            float outerRad = geometry.OuterDiameter / 2f;
            float stdTextHeight = DraftingScaleManager.GetPaperSpaceStandardNotesHeight();
            float titleTextHeight = DraftingScaleManager.GetPaperSpaceMainTitleHeight();
            // Labels A, B, C
            entities.Add(new CadText { Text = "'A'", Position = new PointF(tsThk / 2f + 10, -tsHeight / 2f + 10), EntityColor = Color.Cyan, TargetPaperSpaceHeight = stdTextHeight });
            entities.Add(new CadText { Text = "'B'", Position = new PointF(tsThk / 2f + 10, -tsHeight / 4f), EntityColor = Color.Cyan, TargetPaperSpaceHeight = stdTextHeight });
            entities.Add(new CadText { Text = "<-- 'C'", Position = new PointF(tsThk / 2f + 25, 0), EntityColor = Color.DodgerBlue, Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center, TargetPaperSpaceHeight = stdTextHeight });

            // Dimension Chains using DimensionRenderer
            float dimX = -tsThk / 2f - 40;
            float dimTextHeight = DraftingScaleManager.GetPaperSpaceDimensionHeight();
            
            // Outer Diameter
            entities.AddRange(dimRenderer.GenerateVerticalDimension(dimX, -tsHeight / 2f, tsHeight / 2f, $"Ø{geometry.OuterDiameter}", Color.Blue, dimTextHeight));
            
            // Shell ID (using geometry.ShellRadius * 2)
            float shellIdScale = geometry.ShellRadius * 2f;
            entities.AddRange(dimRenderer.GenerateVerticalDimension(dimX - 45, -shellIdScale / 2f, shellIdScale / 2f, $"Ø{data.ShellID}", Color.Blue, dimTextHeight));

            // Top Thickness Dimension Callout
            entities.Add(new CadDimension 
            {
                StartPoint = new PointF(-tsThk / 2f, -tsHeight / 2f),
                EndPoint = new PointF(tsThk / 2f, -tsHeight / 2f),
                DimensionLineLocation = new PointF(0, -tsHeight / 2f - 35),
                Type = DimensionType.Horizontal,
                OverrideText = $"{data.TubeSheetFinishTHK} THK",
                EntityColor = Color.Magenta,
                TargetPaperSpaceHeight = dimTextHeight
            });

            // Bottom Titles
            entities.Add(new CadText { Text = titleText, Position = new PointF(0, tsHeight / 2f + 25), EntityColor = Color.DodgerBlue, Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center, TargetPaperSpaceHeight = titleTextHeight });

            // Apply Translation
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
                else if (entity is CadLine line)
                {
                    line.Start = new PointF(line.Start.X + dx, line.Start.Y + dy);
                    line.End = new PointF(line.End.X + dx, line.End.Y + dy);
                }
                else if (entity is CadText text)
                {
                    text.Position = new PointF(text.Position.X + dx, text.Position.Y + dy);
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
