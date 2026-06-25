using System;
using System.Collections.Generic;
using System.Drawing;

namespace MegaEngineeringSuite
{
    public class SheetLayoutEngine
    {
        private DrawingLayoutEngine engineeringEngine = new DrawingLayoutEngine();
        
        private float sheetWidth = 841f;
        private float sheetHeight = 594f;
        private float marginL = 20f;
        private float marginR = 10f;
        private float marginT = 10f;
        private float marginB = 10f;

        // Configurable Mega-Zone Percentages
        private float engWidthRatio = 0.75f;
        private float docWidthRatio = 0.25f;

        private float topEngHeightRatio = 0.40f;
        private float midEngHeightRatio = 0.35f;
        private float botEngHeightRatio = 0.25f;

        private float docTopHeightRatio = 0.40f;
        private float docMidHeightRatio = 0.40f;
        private float docBotHeightRatio = 0.20f;

        private float zonePadding = 15f;

        public bool DebugMode { get; set; } = false;

        public DrawingModel GenerateModel(GeometryModel geometry, EngineeringDataModel data)
        {
            DrawingModel finalSheet = new DrawingModel();

            // 1. Generate blocks
            List<DrawingBlock> blocks = engineeringEngine.GenerateBlocks(geometry, data);

            // 2. Define View Area
            float viewAreaMinX = marginL;
            float viewAreaMaxX = sheetWidth - marginR; 
            float viewAreaMinY = marginB;
            float viewAreaMaxY = sheetHeight - marginT; 

            float viewAreaW = viewAreaMaxX - viewAreaMinX;
            float viewAreaH = viewAreaMaxY - viewAreaMinY;

            // 3. Setup Zones
            Dictionary<SheetZone, RectangleF> zoneRects = new Dictionary<SheetZone, RectangleF>();
            
            float engWidth = viewAreaW * engWidthRatio;
            float docWidth = viewAreaW * docWidthRatio;

            // Engineering Heights
            float topEngH = viewAreaH * topEngHeightRatio;
            float midEngH = viewAreaH * midEngHeightRatio;
            float botEngH = viewAreaH * botEngHeightRatio;

            // Engineering Ys (Origin is bottom-left, so Bottom zone is lowest Y)
            float botEngY = viewAreaMinY;
            float midEngY = botEngY + botEngH;
            float topEngY = midEngY + midEngH;

            // Documentation Heights
            float docTopH = viewAreaH * docTopHeightRatio;
            float docMidH = viewAreaH * docMidHeightRatio;
            float docBotH = viewAreaH * docBotHeightRatio;

            // Documentation Ys
            float docBotY = viewAreaMinY;

            // Top Engineering
            zoneRects[SheetZone.TopEngineeringLeft] = new RectangleF(viewAreaMinX, topEngY, engWidth / 2f, topEngH);
            zoneRects[SheetZone.TopEngineeringRight] = new RectangleF(viewAreaMinX + engWidth / 2f, topEngY, engWidth / 2f, topEngH);

            // Middle Engineering
            zoneRects[SheetZone.MiddleEngineeringLeft] = new RectangleF(viewAreaMinX, midEngY, engWidth / 2f, midEngH);
            zoneRects[SheetZone.MiddleEngineeringRight] = new RectangleF(viewAreaMinX + engWidth / 2f, midEngY, engWidth / 2f, midEngH);

            // Bottom Engineering
            zoneRects[SheetZone.BottomEngineeringLeft] = new RectangleF(viewAreaMinX, botEngY, engWidth / 3f, botEngH);
            zoneRects[SheetZone.BottomEngineeringMid] = new RectangleF(viewAreaMinX + engWidth / 3f, botEngY, engWidth / 3f, botEngH);
            zoneRects[SheetZone.BottomEngineeringRight] = new RectangleF(viewAreaMinX + 2f * engWidth / 3f, botEngY, engWidth / 3f, botEngH);

            // Doc Column (Unified stack placed in DocColumnBottom zone)
            float docX = viewAreaMinX + engWidth;
            zoneRects[SheetZone.DocColumnBottom] = new RectangleF(docX, docBotY, docWidth, viewAreaH);

            // 4. Draw construction rectangles and zone names
            if (DebugMode)
            {
                foreach(var kvp in zoneRects)
                {
                    RectangleF z = kvp.Value;
                    
                    finalSheet.Add(new CadLine { Start = new PointF(z.Left, z.Top), End = new PointF(z.Right, z.Top), EntityColor = Color.DarkGray, DashStyle = System.Drawing.Drawing2D.DashStyle.Dash });
                    finalSheet.Add(new CadLine { Start = new PointF(z.Right, z.Top), End = new PointF(z.Right, z.Bottom), EntityColor = Color.DarkGray, DashStyle = System.Drawing.Drawing2D.DashStyle.Dash });
                    finalSheet.Add(new CadLine { Start = new PointF(z.Right, z.Bottom), End = new PointF(z.Left, z.Bottom), EntityColor = Color.DarkGray, DashStyle = System.Drawing.Drawing2D.DashStyle.Dash });
                    finalSheet.Add(new CadLine { Start = new PointF(z.Left, z.Bottom), End = new PointF(z.Left, z.Top), EntityColor = Color.DarkGray, DashStyle = System.Drawing.Drawing2D.DashStyle.Dash });

                    finalSheet.Add(new CadText
                    {
                        Text = kvp.Key.ToString(),
                        Position = new PointF(z.X + z.Width / 2f, z.Y + z.Height / 2f),
                        EntityColor = Color.Red,
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center,
                        TargetPaperSpaceHeight = 5f
                    });
                }
            }

            // 5. Compute uniform scales for aligned zones
            float uniformDetailScale = float.MaxValue;
            var detailZones = new List<SheetZone> { SheetZone.BottomEngineeringLeft, SheetZone.BottomEngineeringMid, SheetZone.BottomEngineeringRight };
            foreach (var zone in detailZones)
            {
                if (zoneRects.TryGetValue(zone, out RectangleF zoneRect))
                {
                    float targetW = zoneRect.Width - (zonePadding * 2f);
                    float targetH = zoneRect.Height - (zonePadding * 2f);
                    if (targetW <= 0 || targetH <= 0) continue;

                    var zoneBlocks = System.Linq.Enumerable.ToList(System.Linq.Enumerable.Where(blocks, b => b.Zone == zone));
                    if (zoneBlocks.Count == 0) continue;

                    float totalW = 0;
                    float maxH = 0;
                    float gap = 250f;

                    foreach (var b in zoneBlocks)
                    {
                        totalW += b.Bounds.Width;
                        if (b.Bounds.Height > maxH) maxH = b.Bounds.Height;
                    }
                    totalW += gap * (zoneBlocks.Count - 1);
                    if (totalW == 0) continue;

                    float scaleX = targetW / totalW;
                    float scaleY = targetH / maxH;
                    float scaleFactor = Math.Min(scaleX, scaleY);
                    if (scaleFactor < uniformDetailScale)
                    {
                        uniformDetailScale = scaleFactor;
                    }
                }
            }

            float uniformAssemblyScale = float.MaxValue;
            var assemblyZones = new List<SheetZone> { SheetZone.TopEngineeringLeft, SheetZone.TopEngineeringRight };
            foreach (var zone in assemblyZones)
            {
                if (zoneRects.TryGetValue(zone, out RectangleF zoneRect))
                {
                    float targetW = zoneRect.Width - (zonePadding * 2f);
                    float targetH = zoneRect.Height - (zonePadding * 2f);
                    if (targetW <= 0 || targetH <= 0) continue;

                    var zoneBlocks = System.Linq.Enumerable.ToList(System.Linq.Enumerable.Where(blocks, b => b.Zone == zone));
                    if (zoneBlocks.Count == 0) continue;

                    float totalW = 0;
                    float maxH = 0;
                    float gap = 250f;

                    foreach (var b in zoneBlocks)
                    {
                        totalW += b.Bounds.Width;
                        if (b.Bounds.Height > maxH) maxH = b.Bounds.Height;
                    }
                    totalW += gap * (zoneBlocks.Count - 1);
                    if (totalW == 0) continue;

                    float scaleX = targetW / totalW;
                    float scaleY = targetH / maxH;
                    float scaleFactor = Math.Min(scaleX, scaleY);
                    if (scaleFactor < uniformAssemblyScale)
                    {
                        uniformAssemblyScale = scaleFactor;
                    }
                }
            }

            float uniformBaffleScale = float.MaxValue;
            var baffleZones = new List<SheetZone> { SheetZone.MiddleEngineeringLeft, SheetZone.MiddleEngineeringRight };
            foreach (var zone in baffleZones)
            {
                if (zoneRects.TryGetValue(zone, out RectangleF zoneRect))
                {
                    float targetW = zoneRect.Width - (zonePadding * 2f);
                    float targetH = zoneRect.Height - (zonePadding * 2f);
                    if (targetW <= 0 || targetH <= 0) continue;

                    var zoneBlocks = System.Linq.Enumerable.ToList(System.Linq.Enumerable.Where(blocks, b => b.Zone == zone));
                    if (zoneBlocks.Count == 0) continue;

                    float totalW = 0;
                    float maxH = 0;
                    float gap = 250f;

                    foreach (var b in zoneBlocks)
                    {
                        totalW += b.Bounds.Width;
                        if (b.Bounds.Height > maxH) maxH = b.Bounds.Height;
                    }
                    totalW += gap * (zoneBlocks.Count - 1);
                    if (totalW == 0) continue;

                    float scaleX = targetW / totalW;
                    float scaleY = targetH / maxH;
                    float scaleFactor = Math.Min(scaleX, scaleY);
                    if (scaleFactor < uniformBaffleScale)
                    {
                        uniformBaffleScale = scaleFactor;
                    }
                }
            }

            // 6. Initial scale and translation
            var blocksByZone = System.Linq.Enumerable.ToList(System.Linq.Enumerable.GroupBy(blocks, b => b.Zone));
            foreach (var zoneGroup in blocksByZone)
            {
                if (zoneRects.TryGetValue(zoneGroup.Key, out RectangleF zoneRect))
                {
                    float targetW = zoneRect.Width - (zonePadding * 2f);
                    float targetH = zoneRect.Height - (zonePadding * 2f);
                    if (targetW <= 0 || targetH <= 0) continue; // Safety

                    var groupBlocks = System.Linq.Enumerable.ToList(zoneGroup);

                    // Combine all blocks in the zone for unified scaling and placement
                    var sortedBlocks = System.Linq.Enumerable.ToList(System.Linq.Enumerable.OrderBy(groupBlocks, b => b.Priority));
                    float totalW = 0;
                    float maxH = 0;
                    float gap = 250f; // Model space gap

                    foreach(var b in sortedBlocks)
                    {
                        totalW += b.Bounds.Width;
                        if (b.Bounds.Height > maxH) maxH = b.Bounds.Height;
                    }
                    totalW += gap * (sortedBlocks.Count - 1);
                    if (totalW == 0) continue;

                    float scaleX = targetW / totalW;
                    float scaleY = targetH / maxH;
                    float scale = Math.Min(scaleX, scaleY);
                    
                    if (sortedBlocks.Count > 0 && !sortedBlocks[0].AllowScaling)
                    {
                        scale = 1.0f;
                    }

                    // Apply uniform scale overrides for professional balance
                    if (detailZones.Contains(zoneGroup.Key) && uniformDetailScale != float.MaxValue)
                    {
                        scale = uniformDetailScale;
                    }
                    else if (assemblyZones.Contains(zoneGroup.Key) && uniformAssemblyScale != float.MaxValue)
                    {
                        scale = uniformAssemblyScale;
                    }
                    else if (baffleZones.Contains(zoneGroup.Key) && uniformBaffleScale != float.MaxValue)
                    {
                        scale = uniformBaffleScale * 1.75f;
                    }

                    float currentX = zoneRect.X + (zoneRect.Width - (totalW * scale)) / 2f;
                    float centerY = zoneRect.Y + zoneRect.Height / 2f;

                    // Override for Doc Column Right Alignment
                    if (zoneGroup.Key == SheetZone.DocColumnBottom)
                    {
                        currentX = zoneRect.Right - (totalW * scale) - zonePadding;
                    }

                    foreach(var b in sortedBlocks)
                    {
                        b.PreferredScale = scale;
                        if (b.AllowScaling)
                        {
                            ScaleEntities(b.Entities, scale);
                            b.Bounds = DrawingBoundsCalculator.CalculateBounds(b.Entities);
                        }

                        float dx = currentX - b.Bounds.MinX;
                        float dy = centerY - (b.Bounds.MinY + b.Bounds.Height / 2f);
                        if (baffleZones.Contains(zoneGroup.Key))
                        {
                            dy -= (b.Bounds.Height * 0.1f); // Shift down visually to leave space for title above
                        }
                        if (zoneGroup.Key == SheetZone.DocColumnBottom)
                        {
                            dy = (zoneRect.Y + zonePadding) - b.Bounds.MinY;
                        }

                        TranslateEntities(b.Entities, dx, dy);
                        b.Bounds = DrawingBoundsCalculator.CalculateBounds(b.Entities);

                        currentX += b.Bounds.Width + (gap * scale);
                    }
                }
            }

            // 6. Merge into final sheet
            foreach (var block in blocks)
            {
                finalSheet.AddRange(block.Entities);
            }

            // 7. Paper Space Elements
            DrawBorder(finalSheet);

            return finalSheet;
        }

        private void DrawBorder(DrawingModel model)
        {
            model.Add(new CadLine { Start = new PointF(marginL, marginB), End = new PointF(sheetWidth - marginR, marginB), EntityColor = Color.White });
            model.Add(new CadLine { Start = new PointF(sheetWidth - marginR, marginB), End = new PointF(sheetWidth - marginR, sheetHeight - marginT), EntityColor = Color.White });
            model.Add(new CadLine { Start = new PointF(sheetWidth - marginR, sheetHeight - marginT), End = new PointF(marginL, sheetHeight - marginT), EntityColor = Color.White });
            model.Add(new CadLine { Start = new PointF(marginL, sheetHeight - marginT), End = new PointF(marginL, marginB), EntityColor = Color.White });
        }

        private void ScaleEntities(List<ICadEntity> entities, float scale)
        {
            foreach (var entity in entities)
            {
                if (entity is CadCircle circle)
                {
                    circle.Center = new PointF(circle.Center.X * scale, circle.Center.Y * scale);
                    circle.Radius *= scale;
                }
                else if (entity is CadArc arc)
                {
                    arc.Center = new PointF(arc.Center.X * scale, arc.Center.Y * scale);
                    arc.Radius *= scale;
                }
                else if (entity is CadLine line)
                {
                    line.Start = new PointF(line.Start.X * scale, line.Start.Y * scale);
                    line.End = new PointF(line.End.X * scale, line.End.Y * scale);
                }
                else if (entity is CadText text)
                {
                    text.Position = new PointF(text.Position.X * scale, text.Position.Y * scale);
                    text.FontSize = text.TargetPaperSpaceHeight;
                }
                else if (entity is CadMText mtext)
                {
                    mtext.Position = new PointF(mtext.Position.X * scale, mtext.Position.Y * scale);
                    mtext.FontSize = mtext.TargetPaperSpaceHeight;
                }
                else if (entity is CadDimension dim)
                {
                    dim.StartPoint = new PointF(dim.StartPoint.X * scale, dim.StartPoint.Y * scale);
                    dim.EndPoint = new PointF(dim.EndPoint.X * scale, dim.EndPoint.Y * scale);
                    dim.SelectionPoint = new PointF(dim.SelectionPoint.X * scale, dim.SelectionPoint.Y * scale);
                    dim.DimensionLineLocation = new PointF(dim.DimensionLineLocation.X * scale, dim.DimensionLineLocation.Y * scale);
                    dim.TextHeight = dim.TargetPaperSpaceHeight;
                    if (dim.Type == DimensionType.Angular)
                    {
                        dim.AngleCenterPoint = new PointF(dim.AngleCenterPoint.X * scale, dim.AngleCenterPoint.Y * scale);
                    }
                }
                else if (entity is CadLeader leader)
                {
                    for (int i = 0; i < leader.Vertices.Count; i++)
                    {
                        leader.Vertices[i] = new PointF(leader.Vertices[i].X * scale, leader.Vertices[i].Y * scale);
                    }
                }
                else if (entity is CadPolyline polyline)
                {
                    for (int i = 0; i < polyline.Vertices.Count; i++)
                    {
                        polyline.Vertices[i].Point = new PointF(polyline.Vertices[i].Point.X * scale, polyline.Vertices[i].Point.Y * scale);
                    }
                }
                else if (entity is CadHatch hatch)
                {
                    for (int i = 0; i < hatch.BoundaryVertices.Count; i++)
                    {
                        hatch.BoundaryVertices[i] = new PointF(hatch.BoundaryVertices[i].X * scale, hatch.BoundaryVertices[i].Y * scale);
                    }
                    hatch.HatchScale *= scale;
                }
            }
        }

        private void TranslateEntities(List<ICadEntity> entities, float dx, float dy)
        {
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
                else if (entity is CadMText mtext)
                {
                    mtext.Position = new PointF(mtext.Position.X + dx, mtext.Position.Y + dy);
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
                        polyline.Vertices[i].Point = new PointF(polyline.Vertices[i].Point.X + dx, polyline.Vertices[i].Point.Y + dy);
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
