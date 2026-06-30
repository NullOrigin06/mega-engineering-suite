#pragma warning disable CS8618
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace MegaEngineeringSuite
{
    public class CalloutLeader
    {
        public string Text { get; set; }
        public PointF TargetPoint { get; set; }
        public bool AlignRight { get; set; }
        public float LeaderVerticalDirection { get; set; } = 0f;
        public float? TextCenterY { get; set; }
        public float? SideClearance { get; set; }
    }

    public class AnnotationPlacementEngine
    {
        private const float StandardLandingLength = 70f;
        private const float StandardTextGap = 15f;
        private const float MinimumSideClearance = 90f;

        public IEnumerable<ICadEntity> GenerateAnnotations(List<CalloutLeader> callouts, float exclusionRadius)
        {
            List<ICadEntity> entities = new List<ICadEntity>();

            float safeMarginOffset = Math.Max(MinimumSideClearance, (exclusionRadius * 2f) * 0.1f);

            // Distribute callouts around the circle based on AlignRight (true = right side, false = left side)
            // The user requested specific positions: Top-Right, Top-Left, Right, Left.
            // We can determine placement logic by their position in the list or TargetPoint.
            // Here we map dynamically based on AlignRight and TargetPoint Y.
            
            float topBandY = -exclusionRadius + 55f;
            float leftStackY = topBandY;
            float rightStackY = topBandY;
            float verticalSpacing = 60f;

            foreach (var callout in callouts.OrderBy(c => c.TargetPoint.Y))
            {
                var lines = callout.Text.Split('\n');
                float maxLen = lines.Max(l => l.Length);
                float noteHeight = DraftingScaleManager.GetPaperSpaceStandardNotesHeight();
                SizeF textSize = new SizeF(maxLen * (noteHeight * 0.8f), lines.Length * (noteHeight * 1.5f));

                float textX;
                float textY;
                float pointerEndX;
                float elbowX;
                float textCenterY;
                float sideClearance = callout.SideClearance ?? safeMarginOffset;
                float safeMargin = exclusionRadius + sideClearance;
                
                if (callout.AlignRight)
                {
                    // Placed securely on the right margin
                    textX = safeMargin;
                    textY = Math.Max(rightStackY, callout.TargetPoint.Y - textSize.Height / 2f);
                    pointerEndX = textX - StandardTextGap;
                    elbowX = pointerEndX - StandardLandingLength;
                    
                    rightStackY = textY + textSize.Height + verticalSpacing;
                }
                else
                {
                    // Placed securely on the left margin
                    textX = -safeMargin - textSize.Width;
                    textY = Math.Max(leftStackY, callout.TargetPoint.Y - textSize.Height / 2f);
                    pointerEndX = textX + textSize.Width + StandardTextGap;
                    elbowX = pointerEndX + StandardLandingLength;
                    
                    leftStackY = textY + textSize.Height + verticalSpacing;
                }

                float verticalDirection = callout.LeaderVerticalDirection;
                if (verticalDirection == 0f)
                {
                    verticalDirection = callout.TargetPoint.Y <= 0f ? -1f : 1f;
                }

                textCenterY = callout.TextCenterY ??
                    callout.TargetPoint.Y + Math.Sign(verticalDirection) * Math.Abs(elbowX - callout.TargetPoint.X);
                textY = textCenterY - textSize.Height / 2f;

                // Draw Text
                entities.Add(new CadMText 
                { 
                    Text = callout.Text, 
                    Position = new PointF(textX, textY), 
                    EntityColor = Color.Blue,
                    Alignment = StringAlignment.Near,
                    LineAlignment = StringAlignment.Near,
                    TargetPaperSpaceHeight = 31f
                });

                // Leader Line: Target -> Diagonal Elbow -> Horizontal -> Text
                PointF pTarget = new PointF(callout.TargetPoint.X, callout.TargetPoint.Y);
                PointF pElbow = new PointF(elbowX, textCenterY);
                PointF pTextEdge = new PointF(pointerEndX, textCenterY);

                entities.Add(new CadLeader 
                { 
                    Vertices = new List<PointF> { pTarget, pElbow, pTextEdge }, 
                    EntityColor = Color.Magenta,
                    HasArrowHead = true
                });
            }

            return entities;
        }
    }
}
