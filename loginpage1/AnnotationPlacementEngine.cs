#pragma warning disable CS8618
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace loginpage1
{
    public class CalloutLeader
    {
        public string Text { get; set; }
        public PointF TargetPoint { get; set; }
        public bool AlignRight { get; set; }
    }

    public class AnnotationPlacementEngine
    {
        public IEnumerable<ICadEntity> GenerateAnnotations(List<CalloutLeader> callouts, float exclusionRadius)
        {
            List<ICadEntity> entities = new List<ICadEntity>();

            float safeMarginOffset = Math.Max(40f, (exclusionRadius * 2f) * 0.08f);
            float safeMargin = exclusionRadius + safeMarginOffset; 

            // Distribute callouts around the circle based on AlignRight (true = right side, false = left side)
            // The user requested specific positions: Top-Right, Top-Left, Right, Left.
            // We can determine placement logic by their position in the list or TargetPoint.
            // Here we map dynamically based on AlignRight and TargetPoint Y.
            
            float leftStackY = -150f;
            float rightStackY = -150f;
            float verticalSpacing = 40f;

            foreach (var callout in callouts.OrderBy(c => c.TargetPoint.Y))
            {
                var lines = callout.Text.Split('\n');
                float maxLen = lines.Max(l => l.Length);
                float noteHeight = DraftingScaleManager.GetPaperSpaceStandardNotesHeight();
                SizeF textSize = new SizeF(maxLen * (noteHeight * 0.8f), lines.Length * (noteHeight * 1.5f));

                float textX;
                float textY;
                float pointerStartX = callout.TargetPoint.X;
                float pointerEndX;
                
                if (callout.AlignRight)
                {
                    // Placed securely on the right margin
                    textX = safeMargin;
                    textY = Math.Max(rightStackY, callout.TargetPoint.Y - textSize.Height / 2f);
                    pointerEndX = textX - 5f;
                    
                    rightStackY = textY + textSize.Height + verticalSpacing;
                }
                else
                {
                    // Placed securely on the left margin
                    textX = -safeMargin - textSize.Width;
                    textY = Math.Max(leftStackY, callout.TargetPoint.Y - textSize.Height / 2f);
                    pointerEndX = textX + textSize.Width + 5f;
                    
                    leftStackY = textY + textSize.Height + verticalSpacing;
                }

                // Draw Text
                entities.Add(new CadText 
                { 
                    Text = callout.Text, 
                    Position = new PointF(textX, textY), 
                    EntityColor = Color.DodgerBlue,
                    TargetPaperSpaceHeight = noteHeight
                });

                // Leader Line: Target -> Diagonal Elbow -> Horizontal -> Text
                PointF pTarget = new PointF(callout.TargetPoint.X, callout.TargetPoint.Y);
                // Calculate an elbow point to make it look like a CAD leader
                float landingLength = 15f;
                float elbowX = callout.AlignRight ? pointerEndX - landingLength : pointerEndX + landingLength;
                float textCenterY = textY + textSize.Height / 2f;
                PointF pElbow = new PointF(elbowX, textCenterY);
                PointF pTextEdge = new PointF(pointerEndX, textCenterY);

                entities.Add(new CadLeader 
                { 
                    Vertices = new List<PointF> { pTarget, pElbow, pTextEdge }, 
                    EntityColor = Color.DodgerBlue,
                    HasArrowHead = true
                });
            }

            return entities;
        }
    }
}
