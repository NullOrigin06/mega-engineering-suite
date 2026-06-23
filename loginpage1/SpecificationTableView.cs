using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace loginpage1
{
    public class SpecificationTableView : ICadView
    {
        public IEnumerable<ICadEntity> Render(GeometryModel geometry, EngineeringDataModel data, PointF origin)
        {
            List<ICadEntity> entities = new List<ICadEntity>();

            float stdTextHeight = DraftingScaleManager.GetPaperSpaceTableCellHeight();
            float headerTextHeight = DraftingScaleManager.GetPaperSpaceSpecHeaderHeight();
            
            // Increase row height by 25% (was 2.5f, now 3.1f)
            float rowHeight = stdTextHeight * 3.1f;
            
            var rows = new List<Tuple<string, string>>
            {
                new Tuple<string, string>("H.E.A (m²)", data.HTA.ToString("F2")),
                new Tuple<string, string>("Shell Diameter", (geometry.ShellRadius * 2).ToString("F1")),
                new Tuple<string, string>("Tube O.D.", data.TubeOD.ToString("F2")),
                new Tuple<string, string>("Tube Pitch", geometry.TubePitch.ToString("F1")),
                new Tuple<string, string>("Tube Count", data.TubeQty.ToString()),
                new Tuple<string, string>("No Of Passes", data.NoOfPass.ToString()),
                new Tuple<string, string>("Tube Hole Qty", geometry.TubeCoordinates != null ? geometry.TubeCoordinates.Count.ToString() : data.TubeQty.ToString()),
                new Tuple<string, string>("Baffle Qty", data.BaffleQty.ToString()),
                new Tuple<string, string>("Baffle Thickness", data.BaffleTHK.ToString()),
                new Tuple<string, string>("Material", data.Material)
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
            float totalHeight = (rows.Count + 1) * rowHeight; // +1 for header
            
            // Start drawing from top (totalHeight) downwards
            currentY = totalHeight;

            // Title row
            entities.Add(new CadLine { Start = new PointF(0, currentY), End = new PointF(tableWidth, currentY), EntityColor = Color.White });
            entities.Add(new CadText 
            { 
                Text = "SPECIFICATION", 
                Position = new PointF(tableWidth / 2f, currentY - rowHeight / 2f), 
                EntityColor = Color.Cyan, 
                Alignment = StringAlignment.Center, 
                LineAlignment = StringAlignment.Center, 
                FontSize = headerTextHeight 
            });
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
                    EntityColor = Color.Yellow, 
                    Alignment = StringAlignment.Near, 
                    LineAlignment = StringAlignment.Center, 
                    FontSize = stdTextHeight 
                });

                // Col 2 text
                entities.Add(new CadText 
                { 
                    Text = row.Item2, 
                    Position = new PointF(col1Width + 10, currentY - rowHeight / 2f), 
                    EntityColor = Color.White, 
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
            entities.Add(new CadLine { Start = new PointF(col1Width, 0), End = new PointF(col1Width, totalHeight - rowHeight), EntityColor = Color.White });
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
