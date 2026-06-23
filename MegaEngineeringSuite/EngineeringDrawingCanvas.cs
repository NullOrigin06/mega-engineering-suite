#pragma warning disable CS8618, CS8622
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace MegaEngineeringSuite
{
    public class EngineeringDrawingCanvas : UserControl
    {
        private SheetLayoutEngine drawingEngine = new SheetLayoutEngine();
        public GeometryModel CurrentGeometry { get; private set; }
        public EngineeringDataModel CurrentData { get; private set; }
        public DrawingModel CurrentModel { get; private set; }

        private float zoomScale = 1.0f;
        private PointF panOffset = new PointF(0, 0);

        private float contentWidth = 1000f;
        private float contentHeight = 1000f;
        private float minX = 0f;
        private float minY = 0f;

        private ToolStrip toolStrip;
        private ToolStripButton btnZoomIn;
        private ToolStripButton btnZoomOut;
        private ToolStripButton btnZoomFit;
        private ToolStripButton btnZoom100;

        public EngineeringDrawingCanvas()
        {
            this.DoubleBuffered = true;
            this.BackColor = Color.FromArgb(20, 20, 30);
            InitializeToolbar();

            this.MouseWheel += OnCanvasMouseWheel;
            this.Paint += OnCanvasPaint;
        }

        private void InitializeToolbar()
        {
            toolStrip = new ToolStrip();
            toolStrip.GripStyle = ToolStripGripStyle.Hidden;
            toolStrip.BackColor = Color.WhiteSmoke;

            btnZoomIn = new ToolStripButton("Zoom In (+)");
            btnZoomIn.Click += (s, e) => { Zoom(1.2f); };

            btnZoomOut = new ToolStripButton("Zoom Out (-)");
            btnZoomOut.Click += (s, e) => { Zoom(1 / 1.2f); };

            btnZoomFit = new ToolStripButton("Zoom Fit");
            btnZoomFit.Click += (s, e) => { ZoomFit(); };

            btnZoom100 = new ToolStripButton("100%");
            btnZoom100.Click += (s, e) => { Zoom100(); };

            toolStrip.Items.Add(btnZoomIn);
            toolStrip.Items.Add(btnZoomOut);
            toolStrip.Items.Add(new ToolStripSeparator());
            toolStrip.Items.Add(btnZoomFit);
            toolStrip.Items.Add(btnZoom100);

            this.Controls.Add(toolStrip);
        }

        public void LoadDrawing(GeometryModel geometry, EngineeringDataModel data)
        {
            CurrentGeometry = geometry;
            CurrentData = data;

            var tempService = new TemplateDrawingService();
            var groupedViews = tempService.GenerateTemplateViews(geometry, data);
            
            CurrentModel = new DrawingModel();
            
            // For preview purposes, we just dump them into a flat model.
            // Since they are generated at (0,0), they will overlap in the preview, 
            // but this is acceptable for Phase T1 as we are validating CAD placement.
            foreach (var kvp in groupedViews)
            {
                foreach (var entity in kvp.Value)
                {
                    CurrentModel.Entities.Add(entity);
                }
            }

            ZoomFit();
        }

        private new void UpdateBounds()
        {
            if (CurrentModel == null || CurrentModel.Entities.Count == 0) return;

            minX = float.MaxValue; minY = float.MaxValue;
            float maxX = float.MinValue, maxY = float.MinValue;

            foreach (var entity in CurrentModel.Entities)
            {
                if (entity is CadCircle c)
                {
                    minX = Math.Min(minX, c.Center.X - c.Radius);
                    minY = Math.Min(minY, c.Center.Y - c.Radius);
                    maxX = Math.Max(maxX, c.Center.X + c.Radius);
                    maxY = Math.Max(maxY, c.Center.Y + c.Radius);
                }
                else if (entity is CadLine l)
                {
                    minX = Math.Min(minX, Math.Min(l.Start.X, l.End.X));
                    minY = Math.Min(minY, Math.Min(l.Start.Y, l.End.Y));
                    maxX = Math.Max(maxX, Math.Max(l.Start.X, l.End.X));
                    maxY = Math.Max(maxY, Math.Max(l.Start.Y, l.End.Y));
                }
                else if (entity is CadText t)
                {
                    minX = Math.Min(minX, t.Position.X);
                    minY = Math.Min(minY, t.Position.Y);
                    maxX = Math.Max(maxX, t.Position.X);
                    maxY = Math.Max(maxY, t.Position.Y);
                }
                else if (entity is CadLeader leader)
                {
                    foreach (var v in leader.Vertices)
                    {
                        minX = Math.Min(minX, v.X);
                        minY = Math.Min(minY, v.Y);
                        maxX = Math.Max(maxX, v.X);
                        maxY = Math.Max(maxY, v.Y);
                    }
                }
            }

            if (minX == float.MaxValue) { minX = -1000; minY = -1000; maxX = 1000; maxY = 1000; }

            contentWidth = maxX - minX;
            contentHeight = maxY - minY;
        }

        private void UpdateCanvasSize()
        {
            if (CurrentModel == null) return;
            
            int newWidth = (int)(contentWidth * zoomScale) + 300;
            int newHeight = (int)(contentHeight * zoomScale) + 300;

            Size containerSize = this.Parent != null ? this.Parent.ClientSize : new Size(800, 600);
            
            // Allow the canvas to grow so parent panel scrollbars appear
            this.Size = new Size(Math.Max(newWidth, containerSize.Width), Math.Max(newHeight, containerSize.Height));
            
            // Adjust panOffset so top-left of drawing is correctly positioned
            panOffset = new PointF(150f - (minX * zoomScale), 150f - (minY * zoomScale));
        }

        private void Zoom(float factor)
        {
            float newZoom = zoomScale * factor;
            if (newZoom < 0.01f || newZoom > 100f) return;

            zoomScale = newZoom;
            UpdateCanvasSize();
            this.Invalidate();
        }

        private void ZoomFit()
        {
            UpdateBounds();
            if (contentWidth <= 0 || contentHeight <= 0) return;

            Size containerSize = this.Parent != null ? this.Parent.ClientSize : new Size(800, 600);

            float padding = 150f;
            float scaleX = (containerSize.Width - padding * 2) / contentWidth;
            float scaleY = (containerSize.Height - padding * 2) / contentHeight;
            zoomScale = Math.Min(scaleX, scaleY);
            if (zoomScale <= 0) zoomScale = 0.1f;

            UpdateCanvasSize();
            this.Invalidate();
        }

        private void Zoom100()
        {
            zoomScale = 1.0f;
            UpdateCanvasSize();
            this.Invalidate();
        }

        private void OnCanvasMouseWheel(object sender, MouseEventArgs e)
        {
            if (CurrentGeometry == null) return;
            float factor = e.Delta > 0 ? 1.1f : (1f / 1.1f);
            Zoom(factor);
        }

        private void OnCanvasPaint(object sender, PaintEventArgs e)
        {
            if (CurrentModel == null) return;

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Offset to match the drawing coordinates
            g.TranslateTransform(panOffset.X, panOffset.Y);
            g.ScaleTransform(zoomScale, zoomScale);

            // Render DrawingModel Entities
            foreach (var entity in CurrentModel.Entities)
            {
                if (entity is CadCircle circle)
                {
                    if (circle.IsFilled)
                        g.FillEllipse(new SolidBrush(circle.EntityColor), circle.Center.X - circle.Radius, circle.Center.Y - circle.Radius, circle.Radius * 2, circle.Radius * 2);
                    else
                    {
                        using (Pen p = new Pen(circle.EntityColor, 1f / zoomScale) { DashStyle = circle.DashStyle })
                        {
                            g.DrawEllipse(p, circle.Center.X - circle.Radius, circle.Center.Y - circle.Radius, circle.Radius * 2, circle.Radius * 2);
                        }
                    }
                }
                else if (entity is CadArc arc)
                {
                    float sweepAngle = arc.EndAngle - arc.StartAngle;
                    if (sweepAngle < 0) sweepAngle += 360f;
                    g.DrawArc(new Pen(arc.EntityColor, 1f / zoomScale), 
                        arc.Center.X - arc.Radius, arc.Center.Y - arc.Radius, 
                        arc.Radius * 2, arc.Radius * 2, 
                        arc.StartAngle, sweepAngle);
                }
                else if (entity is CadLine line)
                {
                    using (Pen p = new Pen(line.EntityColor, 1f / zoomScale) { DashStyle = line.DashStyle })
                    {
                        g.DrawLine(p, line.Start, line.End);
                    }
                }
                else if (entity is CadText text)
                {
                    using (Font f = new Font("Consolas", text.FontSize))
                    using (Brush b = new SolidBrush(text.EntityColor))
                    {
                        StringFormat sf = new StringFormat { Alignment = text.Alignment, LineAlignment = text.LineAlignment };
                        
                        float scaledFontSize = text.FontSize * zoomScale;
                        if (scaledFontSize < 1f) continue;
                        
                        g.DrawString(text.Text, f, b, text.Position, sf);
                    }
                }
                else if (entity is CadLeader leader && leader.Vertices.Count > 1)
                {
                    using (Pen p = new Pen(leader.EntityColor, 1f / zoomScale))
                    {
                        for (int i = 0; i < leader.Vertices.Count - 1; i++)
                        {
                            g.DrawLine(p, leader.Vertices[i], leader.Vertices[i + 1]);
                        }
                        
                        if (leader.HasArrowHead)
                        {
                            PointF p0 = leader.Vertices[0];
                            PointF p1 = leader.Vertices[1];
                            double angle = Math.Atan2(p0.Y - p1.Y, p0.X - p1.X);
                            
                            float arrowLength = 10f / zoomScale;
                            float arrowWidth = 4f / zoomScale;
                            
                            PointF pt1 = new PointF(
                                p0.X - arrowLength * (float)Math.Cos(angle) - arrowWidth * (float)Math.Sin(angle),
                                p0.Y - arrowLength * (float)Math.Sin(angle) + arrowWidth * (float)Math.Cos(angle)
                            );
                            
                            PointF pt2 = new PointF(
                                p0.X - arrowLength * (float)Math.Cos(angle) + arrowWidth * (float)Math.Sin(angle),
                                p0.Y - arrowLength * (float)Math.Sin(angle) - arrowWidth * (float)Math.Cos(angle)
                            );
                            
                            using (SolidBrush b = new SolidBrush(leader.EntityColor))
                            {
                                g.FillPolygon(b, new PointF[] { p0, pt1, pt2 });
                            }
                        }
                    }
                }
                else if (entity is CadPolyline polyline && polyline.Vertices.Count > 1)
                {
                    using (Pen p = new Pen(polyline.EntityColor, 1f / zoomScale))
                    {
                        if (polyline.IsClosed)
                            g.DrawPolygon(p, polyline.Vertices.ToArray());
                        else
                            g.DrawLines(p, polyline.Vertices.ToArray());
                    }
                }
                else if (entity is CadHatch hatch && hatch.BoundaryVertices.Count > 2)
                {
                    // Render Hatch as semi-transparent fill for preview purposes
                    using (Brush b = new SolidBrush(Color.FromArgb(80, hatch.EntityColor)))
                    {
                        g.FillPolygon(b, hatch.BoundaryVertices.ToArray());
                    }
                }
                else if (entity is CadDimension dim)
                {
                    using (Pen p = new Pen(dim.EntityColor, 1f / zoomScale))
                    {
                        if (dim.Type == DimensionType.Horizontal || dim.Type == DimensionType.Vertical || dim.Type == DimensionType.Aligned)
                        {
                            g.DrawLine(p, dim.StartPoint, dim.EndPoint);
                            g.DrawLine(p, dim.StartPoint, dim.DimensionLineLocation);
                            g.DrawLine(p, dim.EndPoint, dim.DimensionLineLocation);
                        }
                        else if (dim.Type == DimensionType.Diameter || dim.Type == DimensionType.Radius)
                        {
                            g.DrawLine(p, dim.SelectionPoint, dim.DimensionLineLocation);
                        }
                        else if (dim.Type == DimensionType.Angular)
                        {
                            g.DrawLine(p, dim.AngleCenterPoint, dim.StartPoint);
                            g.DrawLine(p, dim.AngleCenterPoint, dim.EndPoint);
                        }
                    }

                    using (Font f = new Font("Consolas", dim.TextHeight))
                    using (Brush b = new SolidBrush(dim.EntityColor))
                    {
                        StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                        float scaledFontSize = dim.TextHeight * zoomScale;
                        if (scaledFontSize >= 1f)
                        {
                            string t = string.IsNullOrEmpty(dim.OverrideText) ? "<DIM>" : dim.OverrideText;
                            g.DrawString(t, f, b, dim.DimensionLineLocation, sf);
                        }
                    }
                }
            }
        }
    }
}
