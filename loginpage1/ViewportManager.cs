using System;
using System.Drawing;

namespace loginpage1
{
    public class ViewportLayout
    {
        public float LeftZoneWidth { get; set; }
        public float RightZoneWidth { get; set; }
        public float Scale { get; set; }
        public PointF LeftCenter { get; set; }
        public PointF RightCenter { get; set; }
        public RectangleF LeftBounds { get; set; }
        public RectangleF RightBounds { get; set; }
    }

    public class ViewportManager
    {
        public ViewportLayout CalculateViewport(float canvasWidth, float canvasHeight, float maxGeometryDim)
        {
            if (maxGeometryDim <= 0) maxGeometryDim = 1000f; // Safe fallback

            float padding = 150f; // Reserved for dimensions and leaders
            float availableHeight = canvasHeight - padding * 2;

            // 35% for side view, 65% for front view
            float leftZoneWidth = canvasWidth * 0.35f;
            float rightZoneWidth = canvasWidth * 0.65f;
            float availableRightWidth = rightZoneWidth - padding * 2;

            float scale = Math.Min(availableRightWidth / maxGeometryDim, availableHeight / maxGeometryDim);
            if (scale <= 0) scale = 0.1f; // Prevent scale of zero

            float leftCenterX = leftZoneWidth / 2f;
            float leftCenterY = canvasHeight / 2f;

            float rightCenterX = leftZoneWidth + rightZoneWidth / 2f;
            float rightCenterY = canvasHeight / 2f;

            return new ViewportLayout
            {
                LeftZoneWidth = leftZoneWidth,
                RightZoneWidth = rightZoneWidth,
                Scale = scale,
                LeftCenter = new PointF(leftCenterX, leftCenterY),
                RightCenter = new PointF(rightCenterX, rightCenterY),
                LeftBounds = new RectangleF(0, 0, leftZoneWidth, canvasHeight),
                RightBounds = new RectangleF(leftZoneWidth, 0, rightZoneWidth, canvasHeight)
            };
        }
    }
}
