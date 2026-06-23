using System;
using System.Collections.Generic;
using System.Drawing;

namespace loginpage1
{
    public class TitleBlockView : ICadView
    {
        public IEnumerable<ICadEntity> Render(GeometryModel geometry, EngineeringDataModel data, PointF origin)
        {
            List<ICadEntity> entities = new List<ICadEntity>();

            float width = 180f;
            float height = 75f;
            float stdTextHeight = DraftingScaleManager.GetPaperSpaceTitleBlockFieldHeight();
            float fieldLabelHeight = stdTextHeight * 0.8f;

            // 1. Title Block Outline
            entities.Add(new CadLine { Start = new PointF(0, 0), End = new PointF(width, 0), EntityColor = Color.White });
            entities.Add(new CadLine { Start = new PointF(width, 0), End = new PointF(width, height), EntityColor = Color.White });
            entities.Add(new CadLine { Start = new PointF(width, height), End = new PointF(0, height), EntityColor = Color.White });
            entities.Add(new CadLine { Start = new PointF(0, height), End = new PointF(0, 0), EntityColor = Color.White });

            // 2. Internal Divisions
            // Horizontal lines
            entities.Add(new CadLine { Start = new PointF(0, 60f), End = new PointF(width, 60f), EntityColor = Color.White });
            entities.Add(new CadLine { Start = new PointF(0, 45f), End = new PointF(width, 45f), EntityColor = Color.White });
            entities.Add(new CadLine { Start = new PointF(0, 30f), End = new PointF(width, 30f), EntityColor = Color.White });
            entities.Add(new CadLine { Start = new PointF(0, 15f), End = new PointF(width, 15f), EntityColor = Color.White });

            // Vertical lines in lower rows (15-30 and 0-15)
            entities.Add(new CadLine { Start = new PointF(45f, 0), End = new PointF(45f, 30f), EntityColor = Color.White });
            entities.Add(new CadLine { Start = new PointF(90f, 0), End = new PointF(90f, 30f), EntityColor = Color.White });
            entities.Add(new CadLine { Start = new PointF(135f, 0), End = new PointF(135f, 15f), EntityColor = Color.White });

            // 3. Static Labels
            entities.Add(new CadText { Text = "PROJECT:", Position = new PointF(2, 60f + 2f), FontSize = fieldLabelHeight, EntityColor = Color.Yellow });
            entities.Add(new CadText { Text = "CLIENT:", Position = new PointF(2, 45f + 2f), FontSize = fieldLabelHeight, EntityColor = Color.Yellow });
            entities.Add(new CadText { Text = "TITLE:", Position = new PointF(2, 30f + 2f), FontSize = fieldLabelHeight, EntityColor = Color.Yellow });
            
            entities.Add(new CadText { Text = "DRAWN BY:", Position = new PointF(2, 15f + 2f), FontSize = fieldLabelHeight, EntityColor = Color.Yellow });
            entities.Add(new CadText { Text = "CHECKED BY:", Position = new PointF(47f, 15f + 2f), FontSize = fieldLabelHeight, EntityColor = Color.Yellow });
            entities.Add(new CadText { Text = "DRG. NO:", Position = new PointF(92f, 15f + 2f), FontSize = fieldLabelHeight, EntityColor = Color.Yellow });
            
            entities.Add(new CadText { Text = "DATE:", Position = new PointF(2, 2f), FontSize = fieldLabelHeight, EntityColor = Color.Yellow });
            entities.Add(new CadText { Text = "SCALE:", Position = new PointF(92f, 2f), FontSize = fieldLabelHeight, EntityColor = Color.Yellow });
            entities.Add(new CadText { Text = "REV:", Position = new PointF(137f, 2f), FontSize = fieldLabelHeight, EntityColor = Color.Yellow });

            // 4. Dynamic Data
            entities.Add(new CadText { Text = "MEGA ENGINEERING WORKS", Position = new PointF(90f, 67.5f), FontSize = stdTextHeight * 1.5f, EntityColor = Color.Cyan, Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
            entities.Add(new CadText { Text = "STANDARD TUBESHEET", Position = new PointF(90f, 52.5f), FontSize = stdTextHeight * 1.2f, EntityColor = Color.White, Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
            entities.Add(new CadText { Text = "FABRICATION DRAWING", Position = new PointF(90f, 37.5f), FontSize = stdTextHeight * 1.2f, EntityColor = Color.White, Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
            
            entities.Add(new CadText { Text = "AUTO", Position = new PointF(22.5f, 22.5f), FontSize = stdTextHeight, EntityColor = Color.White, Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
            entities.Add(new CadText { Text = "AUTO", Position = new PointF(67.5f, 22.5f), FontSize = stdTextHeight, EntityColor = Color.White, Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
            entities.Add(new CadText { Text = "MEW-HE-001", Position = new PointF(135f, 22.5f), FontSize = stdTextHeight, EntityColor = Color.White, Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
            
            entities.Add(new CadText { Text = DateTime.Now.ToString("dd-MMM-yyyy"), Position = new PointF(45f, 7.5f), FontSize = stdTextHeight, EntityColor = Color.White, Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
            entities.Add(new CadText { Text = "N.T.S.", Position = new PointF(112.5f, 7.5f), FontSize = stdTextHeight, EntityColor = Color.White, Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
            entities.Add(new CadText { Text = "0", Position = new PointF(157.5f, 7.5f), FontSize = stdTextHeight, EntityColor = Color.White, Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });

            TranslateEntities(entities, origin.X, origin.Y);
            return entities;
        }

        private void TranslateEntities(List<ICadEntity> entities, float dx, float dy)
        {
            if (dx == 0 && dy == 0) return;

            foreach (var entity in entities)
            {
                if (entity is CadLine line)
                {
                    line.Start = new PointF(line.Start.X + dx, line.Start.Y + dy);
                    line.End = new PointF(line.End.X + dx, line.End.Y + dy);
                }
                else if (entity is CadText text)
                {
                    text.Position = new PointF(text.Position.X + dx, text.Position.Y + dy);
                }
            }
        }
    }
}
