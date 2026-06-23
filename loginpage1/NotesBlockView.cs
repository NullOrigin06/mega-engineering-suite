using System;
using System.Collections.Generic;
using System.Drawing;

namespace loginpage1
{
    public class NotesBlockView : ICadView
    {
        private List<string> StandardNotes = new List<string>
        {
            "ALL DIMENSIONS ARE IN MM.",
            "REMOVE ALL BURRS AND SHARP EDGES.",
            "TUBE HOLES TO BE DRILLED ON TRIANGULAR PITCH.",
            "VERIFY ALL DIMENSIONS BEFORE FABRICATION.",
            "PARTITION PLATE TO BE FITTED AS SHOWN.",
            "STANDARD SHOP TOLERANCES APPLY."
        };

        public IEnumerable<ICadEntity> Render(GeometryModel geometry, EngineeringDataModel data, PointF origin)
        {
            List<ICadEntity> entities = new List<ICadEntity>();

            float width = 180f;
            float stdTextHeight = DraftingScaleManager.GetPaperSpaceStandardNotesHeight();
            float headerHeight = DraftingScaleManager.GetPaperSpaceSpecHeaderHeight();
            
            float lineSpacing = stdTextHeight * 2.5f; 
            float padding = 10f;
            float totalHeight = padding * 2 + headerHeight + (StandardNotes.Count + 1) * lineSpacing; // Top down structure

            // Border
            entities.Add(new CadLine { Start = new PointF(0, 0), End = new PointF(width, 0), EntityColor = Color.White });
            entities.Add(new CadLine { Start = new PointF(width, 0), End = new PointF(width, totalHeight), EntityColor = Color.White });
            entities.Add(new CadLine { Start = new PointF(width, totalHeight), End = new PointF(0, totalHeight), EntityColor = Color.White });
            entities.Add(new CadLine { Start = new PointF(0, totalHeight), End = new PointF(0, 0), EntityColor = Color.White });

            // Note: In typical CAD (and this app), origin is bottom-left and Y goes up.
            // We'll draw from top (totalHeight) downwards.
            float currentY = totalHeight - padding - headerHeight / 2f;

            entities.Add(new CadText 
            { 
                Text = "GENERAL NOTES", 
                Position = new PointF(width / 2f, currentY), 
                EntityColor = Color.Cyan, 
                Alignment = StringAlignment.Center, 
                LineAlignment = StringAlignment.Center, 
                FontSize = headerHeight 
            });

            // Separator under header
            currentY -= headerHeight / 2f + padding / 2f;
            entities.Add(new CadLine { Start = new PointF(0, currentY), End = new PointF(width, currentY), EntityColor = Color.White });

            currentY -= lineSpacing;

            for (int i = 0; i < StandardNotes.Count; i++)
            {
                // Align numbers to X=10, notes to X=20
                entities.Add(new CadText 
                { 
                    Text = $"{i + 1}.", 
                    Position = new PointF(10, currentY), 
                    EntityColor = Color.White, 
                    Alignment = StringAlignment.Near, 
                    LineAlignment = StringAlignment.Center, 
                    FontSize = stdTextHeight 
                });

                entities.Add(new CadText 
                { 
                    Text = StandardNotes[i], 
                    Position = new PointF(20, currentY), 
                    EntityColor = Color.White, 
                    Alignment = StringAlignment.Near, 
                    LineAlignment = StringAlignment.Center, 
                    FontSize = stdTextHeight 
                });

                currentY -= lineSpacing;
            }

            // Apply translation
            TranslateEntities(entities, origin.X, origin.Y);

            return entities;
        }

        private void TranslateEntities(List<ICadEntity> entities, float dx, float dy)
        {
            if (dx == 0 && dy == 0) return;

            foreach (var entity in entities)
            {
                if (entity is CadText text)
                {
                    text.Position = new PointF(text.Position.X + dx, text.Position.Y + dy);
                }
                else if (entity is CadLine line)
                {
                    line.Start = new PointF(line.Start.X + dx, line.Start.Y + dy);
                    line.End = new PointF(line.End.X + dx, line.End.Y + dy);
                }
            }
        }
    }
}
