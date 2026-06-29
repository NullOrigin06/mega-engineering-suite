#pragma warning disable CS8618, CS8622
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;
using System.Windows.Forms;

namespace MegaEngineeringSuite
{
    public class EngineeringDrawingCanvas : UserControl
    {
        private const float MinZoom = 0.05f;
        private const float MaxZoom = 24f;
        private const float FitPadding = 40f;
        private const float MinimumReadableTextPixels = 5f;

        private static readonly Color PreviewBackColor = Color.FromArgb(14, 16, 24);
        private static readonly Color SheetBackColor = Color.FromArgb(18, 21, 31);
        private static readonly Color SheetBorderColor = Color.FromArgb(90, 101, 121);
        private static readonly Color ToolbarBackColor = Color.FromArgb(31, 35, 45);
        private static readonly Color ToolbarButtonColor = Color.FromArgb(45, 51, 64);
        private static readonly Color ToolbarButtonHoverColor = Color.FromArgb(58, 66, 82);

        private readonly SheetLayoutEngine drawingEngine = new SheetLayoutEngine();

        public GeometryModel CurrentGeometry { get; private set; }
        public EngineeringDataModel CurrentData { get; private set; }
        public DrawingModel CurrentModel { get; private set; }

        private float zoomScale = 1.0f;
        private PointF panOffset = PointF.Empty;
        private float contentWidth = 1000f;
        private float contentHeight = 700f;
        private float minX = 0f;
        private float minY = 0f;
        private bool isFitView = true;
        private bool isPanning;
        private Point lastPanPoint;

        private ToolStrip toolStrip;
        private ToolStripButton btnZoomIn;
        private ToolStripButton btnZoomOut;
        private ToolStripButton btnZoomFit;
        private ToolStripButton btnZoom100;
        private ToolStripLabel lblZoom;

        public EngineeringDrawingCanvas()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint,
                true);

            DoubleBuffered = true;
            Dock = DockStyle.Fill;
            BackColor = PreviewBackColor;
            TabStop = true;

            InitializeToolbar();

            MouseWheel += OnCanvasMouseWheel;
            MouseDown += OnCanvasMouseDown;
            MouseMove += OnCanvasMouseMove;
            MouseUp += OnCanvasMouseUp;
            MouseLeave += OnCanvasMouseLeave;
            Resize += OnCanvasResize;
            Paint += OnCanvasPaint;
        }

        private void InitializeToolbar()
        {
            toolStrip = new ToolStrip
            {
                AutoSize = false,
                BackColor = ToolbarBackColor,
                Dock = DockStyle.Top,
                ForeColor = Color.White,
                GripStyle = ToolStripGripStyle.Hidden,
                Height = 42,
                Padding = new Padding(8, 6, 8, 6),
                RenderMode = ToolStripRenderMode.Professional,
                Renderer = new ToolStripProfessionalRenderer(new PreviewToolStripColorTable())
            };

            btnZoomOut = CreateToolButton("-", "Zoom out", (s, e) => ZoomAtCenter(1f / 1.2f));
            btnZoomIn = CreateToolButton("+", "Zoom in", (s, e) => ZoomAtCenter(1.2f));
            btnZoomFit = CreateToolButton("Fit", "Fit drawing to preview", (s, e) => ZoomFit());
            btnZoom100 = CreateToolButton("100%", "Reset to actual scale", (s, e) => Zoom100());

            lblZoom = new ToolStripLabel("100%")
            {
                AutoSize = false,
                ForeColor = Color.FromArgb(226, 232, 240),
                TextAlign = ContentAlignment.MiddleCenter,
                Width = 72
            };

            ToolStripLabel hintLabel = new ToolStripLabel("Mouse wheel zoom | Drag to pan")
            {
                ForeColor = Color.FromArgb(148, 163, 184),
                Margin = new Padding(10, 0, 0, 0)
            };

            toolStrip.Items.Add(btnZoomOut);
            toolStrip.Items.Add(btnZoomIn);
            toolStrip.Items.Add(new ToolStripSeparator());
            toolStrip.Items.Add(btnZoomFit);
            toolStrip.Items.Add(btnZoom100);
            toolStrip.Items.Add(new ToolStripSeparator());
            toolStrip.Items.Add(lblZoom);
            toolStrip.Items.Add(hintLabel);

            Controls.Add(toolStrip);
        }

        private ToolStripButton CreateToolButton(string text, string tooltip, EventHandler clickHandler)
        {
            ToolStripButton button = new ToolStripButton(text)
            {
                AutoSize = false,
                BackColor = ToolbarButtonColor,
                DisplayStyle = ToolStripItemDisplayStyle.Text,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = Color.White,
                Height = 28,
                Margin = new Padding(3, 0, 3, 0),
                ToolTipText = tooltip,
                Width = text.Length <= 1 ? 34 : 58
            };

            button.Click += clickHandler;
            return button;
        }

        public void LoadDrawing(GeometryModel geometry, EngineeringDataModel data)
        {
            CurrentGeometry = geometry;
            CurrentData = data;

            CurrentModel = drawingEngine.GenerateModel(geometry, data);

            UpdateDrawingBounds();
            ZoomFit();
            Refresh();
        }

        private void UpdateDrawingBounds()
        {
            if (CurrentModel == null || CurrentModel.Entities.Count == 0)
            {
                minX = 0f;
                minY = 0f;
                contentWidth = 1000f;
                contentHeight = 700f;
                return;
            }

            DrawingBounds bounds = CalculatePreviewBounds();

            if (bounds.MinX == float.MaxValue)
            {
                minX = 0f;
                minY = 0f;
                contentWidth = 1000f;
                contentHeight = 700f;
                return;
            }

            minX = bounds.MinX;
            minY = bounds.MinY;
            contentWidth = Math.Max(1f, bounds.Width);
            contentHeight = Math.Max(1f, bounds.Height);
        }

        private DrawingBounds CalculatePreviewBounds()
        {
            DrawingBounds bounds = new DrawingBounds();

            foreach (ICadEntity entity in CurrentModel.Entities)
            {
                if (entity is CadCircle circle)
                {
                    bounds.AddPoint(new PointF(circle.Center.X - circle.Radius, circle.Center.Y - circle.Radius));
                    bounds.AddPoint(new PointF(circle.Center.X + circle.Radius, circle.Center.Y + circle.Radius));
                }
                else if (entity is CadArc arc)
                {
                    bounds.AddPoint(new PointF(arc.Center.X - arc.Radius, arc.Center.Y - arc.Radius));
                    bounds.AddPoint(new PointF(arc.Center.X + arc.Radius, arc.Center.Y + arc.Radius));
                }
                else if (entity is CadLine line)
                {
                    bounds.AddPoint(line.Start);
                    bounds.AddPoint(line.End);
                }
                else if (entity is CadText text)
                {
                    AddTextBounds(bounds, text.Text, text.Position, text.FontSize, text.Alignment, text.LineAlignment);
                }
                else if (entity is CadMText mText)
                {
                    AddTextBounds(bounds, mText.Text, mText.Position, mText.FontSize, mText.Alignment, mText.LineAlignment);
                }
                else if (entity is CadLeader leader)
                {
                    foreach (PointF vertex in leader.Vertices)
                    {
                        bounds.AddPoint(vertex);
                    }
                }
                else if (entity is CadPolyline polyline)
                {
                    foreach (CadPolylineVertex vertex in polyline.Vertices)
                    {
                        bounds.AddPoint(vertex.Point);
                    }
                }
                else if (entity is CadHatch hatch)
                {
                    foreach (PointF vertex in hatch.BoundaryVertices)
                    {
                        bounds.AddPoint(vertex);
                    }
                }
                else if (entity is CadDimension dimension)
                {
                    bounds.AddPoint(dimension.StartPoint);
                    bounds.AddPoint(dimension.EndPoint);
                    bounds.AddPoint(dimension.SelectionPoint);
                    bounds.AddPoint(dimension.DimensionLineLocation);
                    bounds.AddPoint(dimension.AngleCenterPoint);

                    string dimensionText = string.IsNullOrWhiteSpace(dimension.OverrideText) ? "<DIM>" : dimension.OverrideText;
                    AddTextBounds(bounds, dimensionText, dimension.DimensionLineLocation, dimension.TextHeight, StringAlignment.Center, StringAlignment.Center);
                }
            }

            return bounds;
        }

        private static void AddTextBounds(
            DrawingBounds bounds,
            string text,
            PointF position,
            float fontSize,
            StringAlignment alignment,
            StringAlignment lineAlignment)
        {
            string[] lines = (text ?? string.Empty).Split('\n');
            int maxLineLength = lines.Length == 0 ? 1 : lines.Max(line => line.Length);
            float width = Math.Max(fontSize, maxLineLength * fontSize * 0.64f);
            float height = Math.Max(fontSize, lines.Length * fontSize * 1.35f);

            float left = position.X;
            if (alignment == StringAlignment.Center)
            {
                left -= width / 2f;
            }
            else if (alignment == StringAlignment.Far)
            {
                left -= width;
            }

            float top = position.Y;
            if (lineAlignment == StringAlignment.Center)
            {
                top -= height / 2f;
            }
            else if (lineAlignment == StringAlignment.Far)
            {
                top -= height;
            }

            bounds.AddPoint(new PointF(left, top));
            bounds.AddPoint(new PointF(left + width, top + height));
        }

        private void ZoomAtCenter(float factor)
        {
            Rectangle viewport = GetViewportBounds();
            Point center = new Point(viewport.Left + viewport.Width / 2, viewport.Top + viewport.Height / 2);
            ZoomAt(factor, center);
        }

        private void ZoomAt(float factor, Point screenPoint)
        {
            if (CurrentModel == null)
            {
                return;
            }

            Rectangle viewport = GetViewportBounds();
            if (viewport.Width <= 0 || viewport.Height <= 0)
            {
                return;
            }

            PointF modelPoint = ScreenToModel(screenPoint, viewport);
            float newZoom = Math.Clamp(zoomScale * factor, MinZoom, MaxZoom);
            if (Math.Abs(newZoom - zoomScale) < 0.0001f)
            {
                return;
            }

            PointF modelCenter = GetModelCenter();
            zoomScale = newZoom;
            panOffset = new PointF(
                screenPoint.X - (viewport.Left + viewport.Width / 2f) - ((modelPoint.X - modelCenter.X) * zoomScale),
                screenPoint.Y - (viewport.Top + viewport.Height / 2f) - ((modelPoint.Y - modelCenter.Y) * zoomScale));

            isFitView = false;
            UpdateZoomLabel();
            Invalidate();
        }

        private void ZoomFit()
        {
            UpdateDrawingBounds();

            Rectangle viewport = GetViewportBounds();
            if (viewport.Width <= 0 || viewport.Height <= 0 || contentWidth <= 0 || contentHeight <= 0)
            {
                return;
            }

            float availableWidth = Math.Max(1f, viewport.Width - (FitPadding * 2f));
            float availableHeight = Math.Max(1f, viewport.Height - (FitPadding * 2f));
            float scaleX = availableWidth / contentWidth;
            float scaleY = availableHeight / contentHeight;

            zoomScale = Math.Clamp(Math.Min(scaleX, scaleY), MinZoom, MaxZoom);
            panOffset = PointF.Empty;
            isFitView = true;

            UpdateZoomLabel();
            Invalidate();
        }

        private void Zoom100()
        {
            if (CurrentModel == null)
            {
                return;
            }

            zoomScale = 1.0f;
            panOffset = PointF.Empty;
            isFitView = false;

            UpdateZoomLabel();
            Invalidate();
        }

        private void UpdateZoomLabel()
        {
            if (lblZoom == null)
            {
                return;
            }

            lblZoom.Text = $"{zoomScale * 100f:0}%";
        }

        private Rectangle GetViewportBounds()
        {
            int top = toolStrip?.Bottom ?? 0;
            return new Rectangle(0, top, ClientSize.Width, Math.Max(0, ClientSize.Height - top));
        }

        private PointF GetModelCenter()
        {
            return new PointF(minX + (contentWidth / 2f), minY + (contentHeight / 2f));
        }

        private PointF ScreenToModel(Point screenPoint, Rectangle viewport)
        {
            PointF modelCenter = GetModelCenter();

            return new PointF(
                ((screenPoint.X - (viewport.Left + viewport.Width / 2f) - panOffset.X) / zoomScale) + modelCenter.X,
                ((screenPoint.Y - (viewport.Top + viewport.Height / 2f) - panOffset.Y) / zoomScale) + modelCenter.Y);
        }

        private void OnCanvasMouseWheel(object sender, MouseEventArgs e)
        {
            if (CurrentModel == null)
            {
                return;
            }

            Focus();
            ZoomAt(e.Delta > 0 ? 1.12f : 1f / 1.12f, e.Location);
        }

        private void OnCanvasMouseDown(object sender, MouseEventArgs e)
        {
            if (CurrentModel == null)
            {
                return;
            }

            Focus();

            if (e.Button == MouseButtons.Left || e.Button == MouseButtons.Middle)
            {
                isPanning = true;
                lastPanPoint = e.Location;
                Capture = true;
                Cursor = Cursors.SizeAll;
            }
        }

        private void OnCanvasMouseMove(object sender, MouseEventArgs e)
        {
            if (!isPanning)
            {
                return;
            }

            panOffset = new PointF(
                panOffset.X + e.X - lastPanPoint.X,
                panOffset.Y + e.Y - lastPanPoint.Y);

            lastPanPoint = e.Location;
            isFitView = false;
            Invalidate();
        }

        private void OnCanvasMouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left && e.Button != MouseButtons.Middle)
            {
                return;
            }

            isPanning = false;
            Capture = false;
            Cursor = Cursors.Default;
        }

        private void OnCanvasMouseLeave(object sender, EventArgs e)
        {
            if (isPanning)
            {
                return;
            }

            Cursor = Cursors.Default;
        }

        private void OnCanvasResize(object sender, EventArgs e)
        {
            if (isFitView && CurrentModel != null)
            {
                ZoomFit();
                return;
            }

            Invalidate();
        }

        private void OnCanvasPaint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(PreviewBackColor);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            Rectangle viewport = GetViewportBounds();
            if (viewport.Width <= 0 || viewport.Height <= 0)
            {
                return;
            }

            DrawGrid(g, viewport);

            if (CurrentModel == null || CurrentModel.Entities.Count == 0)
            {
                DrawEmptyState(g, viewport);
                return;
            }

            using Region clip = new Region(viewport);
            g.SetClip(clip, CombineMode.Replace);

            GraphicsState state = g.Save();
            PointF modelCenter = GetModelCenter();

            g.TranslateTransform(
                viewport.Left + (viewport.Width / 2f) + panOffset.X,
                viewport.Top + (viewport.Height / 2f) + panOffset.Y);
            g.ScaleTransform(zoomScale, zoomScale);
            g.TranslateTransform(-modelCenter.X, -modelCenter.Y);

            DrawSheetSurface(g);

            foreach (ICadEntity entity in CurrentModel.Entities)
            {
                DrawEntity(g, entity);
            }

            g.Restore(state);
            g.ResetClip();
            DrawViewportOverlay(g, viewport);
        }

        private void DrawGrid(Graphics g, Rectangle viewport)
        {
            using Pen minorPen = new Pen(Color.FromArgb(20, 148, 163, 184));
            using Pen majorPen = new Pen(Color.FromArgb(34, 148, 163, 184));

            int minorSpacing = 24;
            int majorSpacing = minorSpacing * 4;

            for (int x = viewport.Left; x <= viewport.Right; x += minorSpacing)
            {
                g.DrawLine(x % majorSpacing == 0 ? majorPen : minorPen, x, viewport.Top, x, viewport.Bottom);
            }

            for (int y = viewport.Top; y <= viewport.Bottom; y += minorSpacing)
            {
                g.DrawLine(y % majorSpacing == 0 ? majorPen : minorPen, viewport.Left, y, viewport.Right, y);
            }
        }

        private void DrawSheetSurface(Graphics g)
        {
            float margin = 8f / Math.Max(zoomScale, MinZoom);
            RectangleF sheetBounds = new RectangleF(
                minX - margin,
                minY - margin,
                contentWidth + (margin * 2f),
                contentHeight + (margin * 2f));

            using Brush sheetBrush = new SolidBrush(SheetBackColor);
            using Pen sheetPen = new Pen(SheetBorderColor, 1.2f / zoomScale);

            g.FillRectangle(sheetBrush, sheetBounds);
            g.DrawRectangle(sheetPen, sheetBounds.X, sheetBounds.Y, sheetBounds.Width, sheetBounds.Height);
        }

        private void DrawEmptyState(Graphics g, Rectangle viewport)
        {
            using Font titleFont = new Font("Segoe UI", 12f, FontStyle.Bold);
            using Font bodyFont = new Font("Segoe UI", 9.5f, FontStyle.Regular);
            using Brush titleBrush = new SolidBrush(Color.FromArgb(226, 232, 240));
            using Brush bodyBrush = new SolidBrush(Color.FromArgb(148, 163, 184));

            string title = "Drawing preview";
            string body = "Enter inputs and click Calculate to generate a structured preview.";

            SizeF titleSize = g.MeasureString(title, titleFont);
            SizeF bodySize = g.MeasureString(body, bodyFont);
            float centerX = viewport.Left + viewport.Width / 2f;
            float centerY = viewport.Top + viewport.Height / 2f;

            g.DrawString(title, titleFont, titleBrush, centerX - titleSize.Width / 2f, centerY - titleSize.Height);
            g.DrawString(body, bodyFont, bodyBrush, centerX - bodySize.Width / 2f, centerY + 8f);
        }

        private void DrawViewportOverlay(Graphics g, Rectangle viewport)
        {
            string info = $"Zoom {zoomScale * 100f:0}%";
            using Font font = new Font("Segoe UI", 8.5f, FontStyle.Regular);
            using Brush brush = new SolidBrush(Color.FromArgb(170, 203, 213, 225));

            SizeF size = g.MeasureString(info, font);
            g.DrawString(info, font, brush, viewport.Right - size.Width - 12f, viewport.Bottom - size.Height - 10f);
        }

        private void DrawEntity(Graphics g, ICadEntity entity)
        {
            if (entity is CadCircle circle)
            {
                RectangleF bounds = new RectangleF(
                    circle.Center.X - circle.Radius,
                    circle.Center.Y - circle.Radius,
                    circle.Radius * 2f,
                    circle.Radius * 2f);

                if (circle.IsFilled)
                {
                    using Brush brush = new SolidBrush(GetPreviewColor(circle.EntityColor));
                    g.FillEllipse(brush, bounds);
                }
                else
                {
                    using Pen pen = CreatePreviewPen(circle.EntityColor, circle.DashStyle);
                    g.DrawEllipse(pen, bounds);
                }
            }
            else if (entity is CadArc arc)
            {
                float sweepAngle = arc.EndAngle - arc.StartAngle;
                if (sweepAngle < 0)
                {
                    sweepAngle += 360f;
                }

                using Pen pen = CreatePreviewPen(arc.EntityColor, DashStyle.Solid);
                g.DrawArc(
                    pen,
                    arc.Center.X - arc.Radius,
                    arc.Center.Y - arc.Radius,
                    arc.Radius * 2f,
                    arc.Radius * 2f,
                    arc.StartAngle,
                    sweepAngle);
            }
            else if (entity is CadLine line)
            {
                using Pen pen = CreatePreviewPen(line.EntityColor, line.DashStyle);
                g.DrawLine(pen, line.Start, line.End);
            }
            else if (entity is CadText text)
            {
                DrawText(g, text.Text, text.Position, text.FontSize, text.EntityColor, text.Alignment, text.LineAlignment);
            }
            else if (entity is CadMText mText)
            {
                DrawText(g, mText.Text, mText.Position, mText.FontSize, mText.EntityColor, mText.Alignment, mText.LineAlignment);
            }
            else if (entity is CadLeader leader && leader.Vertices.Count > 1)
            {
                using Pen pen = CreatePreviewPen(leader.EntityColor, DashStyle.Solid);

                for (int i = 0; i < leader.Vertices.Count - 1; i++)
                {
                    g.DrawLine(pen, leader.Vertices[i], leader.Vertices[i + 1]);
                }

                if (leader.HasArrowHead)
                {
                    DrawArrowHead(g, leader);
                }
            }
            else if (entity is CadPolyline polyline && polyline.Vertices.Count > 1)
            {
                using Pen pen = CreatePreviewPen(polyline.EntityColor, DashStyle.Solid);
                PointF[] points = polyline.Vertices.Select(v => v.Point).ToArray();

                if (polyline.IsClosed)
                {
                    g.DrawPolygon(pen, points);
                }
                else
                {
                    g.DrawLines(pen, points);
                }
            }
            else if (entity is CadHatch hatch && hatch.BoundaryVertices.Count > 2)
            {
                using Brush brush = new SolidBrush(Color.FromArgb(70, GetPreviewColor(hatch.EntityColor)));
                g.FillPolygon(brush, hatch.BoundaryVertices.ToArray());
            }
            else if (entity is CadDimension dimension)
            {
                DrawDimension(g, dimension);
            }
        }

        private Pen CreatePreviewPen(Color color, DashStyle dashStyle)
        {
            Pen pen = new Pen(GetPreviewColor(color), 1.15f / Math.Max(zoomScale, MinZoom))
            {
                DashStyle = dashStyle
            };

            return pen;
        }

        private static Color GetPreviewColor(Color color)
        {
            if (color == Color.Blue)
            {
                return Color.FromArgb(42, 111, 255);
            }

            if (color == Color.Red)
            {
                return Color.FromArgb(255, 86, 86);
            }

            if (color == Color.DarkGray)
            {
                return Color.FromArgb(92, 105, 124);
            }

            if (color == Color.White)
            {
                return Color.FromArgb(235, 241, 248);
            }

            if (color == Color.Yellow)
            {
                return Color.FromArgb(250, 204, 21);
            }

            if (color == Color.Cyan)
            {
                return Color.FromArgb(34, 211, 238);
            }

            if (color == Color.Magenta)
            {
                return Color.FromArgb(217, 70, 239);
            }

            return color;
        }

        private void DrawText(
            Graphics g,
            string text,
            PointF position,
            float fontSize,
            Color color,
            StringAlignment alignment,
            StringAlignment lineAlignment)
        {
            if (string.IsNullOrWhiteSpace(text) || fontSize * zoomScale < MinimumReadableTextPixels)
            {
                return;
            }

            using Font font = new Font("Consolas", Math.Max(1f, fontSize), FontStyle.Regular, GraphicsUnit.World);
            using Brush brush = new SolidBrush(GetPreviewColor(color));
            using StringFormat format = new StringFormat
            {
                Alignment = alignment,
                LineAlignment = lineAlignment
            };

            g.DrawString(text, font, brush, position, format);
        }

        private void DrawArrowHead(Graphics g, CadLeader leader)
        {
            PointF p0 = leader.Vertices[0];
            PointF p1 = leader.Vertices[1];
            double angle = Math.Atan2(p0.Y - p1.Y, p0.X - p1.X);

            // Arrowhead dimensions scaled to drawing units, not screen pixels.
            // Length = 2.5× the standard note text height (31 drawing units).
            // Width  = 1/3 of length — giving a sharp closed filled triangle per
            // standard mechanical drafting practice.
            const float noteTextHeight = 31f;
            float arrowLength = noteTextHeight * 2.5f;
            float arrowWidth  = arrowLength / 3f;

            PointF pt1 = new PointF(
                p0.X - arrowLength * (float)Math.Cos(angle) - arrowWidth * (float)Math.Sin(angle),
                p0.Y - arrowLength * (float)Math.Sin(angle) + arrowWidth * (float)Math.Cos(angle));

            PointF pt2 = new PointF(
                p0.X - arrowLength * (float)Math.Cos(angle) + arrowWidth * (float)Math.Sin(angle),
                p0.Y - arrowLength * (float)Math.Sin(angle) - arrowWidth * (float)Math.Cos(angle));

            using Brush brush = new SolidBrush(GetPreviewColor(leader.EntityColor));
            g.FillPolygon(brush, new[] { p0, pt1, pt2 });
        }

        private void DrawDimension(Graphics g, CadDimension dimension)
        {
            using Pen pen = CreatePreviewPen(dimension.EntityColor, DashStyle.Solid);

            if (dimension.Type == DimensionType.Horizontal ||
                dimension.Type == DimensionType.Vertical ||
                dimension.Type == DimensionType.Aligned)
            {
                g.DrawLine(pen, dimension.StartPoint, dimension.EndPoint);
                g.DrawLine(pen, dimension.StartPoint, dimension.DimensionLineLocation);
                g.DrawLine(pen, dimension.EndPoint, dimension.DimensionLineLocation);
            }
            else if (dimension.Type == DimensionType.Diameter || dimension.Type == DimensionType.Radius)
            {
                g.DrawLine(pen, dimension.SelectionPoint, dimension.DimensionLineLocation);
            }
            else if (dimension.Type == DimensionType.Angular)
            {
                g.DrawLine(pen, dimension.AngleCenterPoint, dimension.StartPoint);
                g.DrawLine(pen, dimension.AngleCenterPoint, dimension.EndPoint);
            }

            string text = string.IsNullOrWhiteSpace(dimension.OverrideText) ? "<DIM>" : dimension.OverrideText;
            DrawText(
                g,
                text,
                dimension.DimensionLineLocation,
                dimension.TextHeight,
                dimension.EntityColor,
                StringAlignment.Center,
                StringAlignment.Center);
        }

        private sealed class PreviewToolStripColorTable : ProfessionalColorTable
        {
            public override Color ToolStripGradientBegin => ToolbarBackColor;
            public override Color ToolStripGradientMiddle => ToolbarBackColor;
            public override Color ToolStripGradientEnd => ToolbarBackColor;
            public override Color ToolStripBorder => ToolbarBackColor;
            public override Color ButtonSelectedGradientBegin => ToolbarButtonHoverColor;
            public override Color ButtonSelectedGradientMiddle => ToolbarButtonHoverColor;
            public override Color ButtonSelectedGradientEnd => ToolbarButtonHoverColor;
            public override Color ButtonPressedGradientBegin => Color.FromArgb(71, 85, 105);
            public override Color ButtonPressedGradientMiddle => Color.FromArgb(71, 85, 105);
            public override Color ButtonPressedGradientEnd => Color.FromArgb(71, 85, 105);
            public override Color SeparatorDark => Color.FromArgb(71, 85, 105);
            public override Color SeparatorLight => Color.FromArgb(71, 85, 105);
        }
    }
}
