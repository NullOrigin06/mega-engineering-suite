using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace MegaEngineeringSuite
{
    public class SpecificationTableView : ICadView
    {
        public IEnumerable<ICadEntity> Render(GeometryModel geometry, EngineeringDataModel data, PointF origin)
        {
            List<ICadEntity> entities = new List<ICadEntity>();

            float stdTextHeight = 31f;
            float headerTextHeight = 31f;
            
            // Increase row height by 25% (was 2.5f, now 3.1f)
            float rowHeight = stdTextHeight * 3.1f;
            
            var rows = new List<Tuple<string, string>>
            {
                new Tuple<string, string>("1) H.T.A.", data.HTA.ToString("F2")),
                new Tuple<string, string>("2) SHELL DIAMETER", (geometry.ShellRadius * 2).ToString("F1")),
                new Tuple<string, string>("3) NO. OF TUBES", data.TubeQty.ToString()),
                new Tuple<string, string>("4) TUBE (ERW)", data.TubeOD.ToString("F2")),
                new Tuple<string, string>("5) NO. OF PASSES", data.NoOfPass.ToString()),
                new Tuple<string, string>("6) TUBE HOLE", geometry.TubeCoordinates != null ? geometry.TubeCoordinates.Count.ToString() : data.TubeQty.ToString()),
                new Tuple<string, string>("7) TRIANGULAR PITCH", geometry.TubePitch.ToString("F1")),
                new Tuple<string, string>("8) M.O.C.", data.Material),
                new Tuple<string, string>("9) TUBESHEET QTY.", "2")
            };

            // Calculate required width
            // Approx width per character = fontSize * 0.8
            float maxCol1Chars = rows.Max(r => r.Item1.Length);
            float maxCol2Chars = rows.Max(r => r.Item2.Length);
            
            float reqCol1Width = Math.Max(90f, maxCol1Chars * stdTextHeight * 0.8f + 20f);
            float reqCol2Width = Math.Max(90f, maxCol2Chars * stdTextHeight * 0.8f + 20f);
            float reqTotalWidth = reqCol1Width + reqCol2Width;
            
            float tableWidth = Math.Max(180f, reqTotalWidth);
            float col1Width = tableWidth / 2f; 
            float col2Width = tableWidth / 2f;

            float currentY = 0;
            float totalHeight = (rows.Count + 2) * rowHeight; // +1 for header, +1 for blank row
            
            // Start drawing from top (totalHeight) downwards
            currentY = totalHeight;

            // Title row
            entities.Add(new CadLine { Start = new PointF(0, currentY), End = new PointF(tableWidth, currentY), EntityColor = Color.White });
            entities.Add(new CadText 
            { 
                Text = "SPECIFICATION :-", 
                Position = new PointF(tableWidth / 2f, currentY - rowHeight / 2f), 
                EntityColor = Color.Blue, 
                Alignment = StringAlignment.Center, 
                LineAlignment = StringAlignment.Center, 
                FontSize = headerTextHeight 
            });
            currentY -= rowHeight;

            // Blank row
            entities.Add(new CadLine { Start = new PointF(0, currentY), End = new PointF(tableWidth, currentY), EntityColor = Color.White });
            currentY -= rowHeight;

            // Data rows
            foreach (var row in rows)
            {
                entities.Add(new CadLine { Start = new PointF(0, currentY), End = new PointF(tableWidth, currentY), EntityColor = Color.White });
                
                // Col 1 text
                entities.Add(new CadText 
                { 
                    Text = row.Item1, 
                    Position = new PointF(10, currentY - rowHeight / 2f), 
                    EntityColor = Color.Blue, 
                    Alignment = StringAlignment.Near, 
                    LineAlignment = StringAlignment.Center, 
                    FontSize = stdTextHeight 
                });

                // Col 2 text
                entities.Add(new CadText 
                { 
                    Text = row.Item2, 
                    Position = new PointF(col1Width + 10, currentY - rowHeight / 2f), 
                    EntityColor = Color.Blue, 
                    Alignment = StringAlignment.Near, 
                    LineAlignment = StringAlignment.Center, 
                    FontSize = stdTextHeight 
                });

                currentY -= rowHeight;
            }

            // Bottom border
            entities.Add(new CadLine { Start = new PointF(0, currentY), End = new PointF(tableWidth, currentY), EntityColor = Color.White });

            // Vertical borders
            entities.Add(new CadLine { Start = new PointF(0, 0), End = new PointF(0, totalHeight), EntityColor = Color.White });
            entities.Add(new CadLine { Start = new PointF(col1Width, 0), End = new PointF(col1Width, totalHeight - rowHeight * 2), EntityColor = Color.White }); // Adjust vertical divider to not go through header and blank row
            entities.Add(new CadLine { Start = new PointF(tableWidth, 0), End = new PointF(tableWidth, totalHeight), EntityColor = Color.White });

            // Apply translation
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
