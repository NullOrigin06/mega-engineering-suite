using System;
using System.Collections.Generic;
using System.Drawing;

namespace MegaEngineeringSuite
{
    public class RearTubeSheetView : TubeSheetViewBase
    {
        private RowCountRenderer rowCountRenderer = new RowCountRenderer();
        private AnnotationPlacementEngine annotationEngine = new AnnotationPlacementEngine();

        public override IEnumerable<ICadEntity> Render(GeometryModel geometry, EngineeringDataModel data, PointF origin)
        {
            List<ICadEntity> entities = new List<ICadEntity>();

            // 1. Base Geometry
            entities.AddRange(GenerateTubeSheetGeometry(geometry, data));

            // 2. Title
            float titleTextHeight = DraftingScaleManager.GetPaperSpaceMainTitleHeight();

            entities.Add(new CadText
            {
                Text = "VIEW FROM C\nREAR SIDE TUBE SHEET",
                Position = new PointF(0, (geometry.OuterDiameter / 2f) + 25),
                EntityColor = Color.DodgerBlue,
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center,
                TargetPaperSpaceHeight = titleTextHeight 
            });

            // 3. Row Counts
            entities.AddRange(rowCountRenderer.GenerateRowCounts(geometry));

            // 4. Annotations & Callouts
            var callouts = GenerateCalloutModels(geometry, data);
            float exclusionRadius = geometry.OuterDiameter / 2f;
            entities.AddRange(annotationEngine.GenerateAnnotations(callouts, exclusionRadius));

            // 4b. Offset Dimensions
            entities.AddRange(OffsetDimensionHelper.GenerateOffsetDimensions(geometry.TubeCoordinates));

            // Apply translation
            TranslateEntities(entities, origin.X, origin.Y);

            // 5. Add Dimensions
            float dimTextHeight = DraftingScaleManager.GetPaperSpaceDimensionHeight();
            entities.Add(new CadDimension
            {
                SelectionPoint = new PointF((float)(geometry.ShellRadius * Math.Cos(Math.PI/3)) + origin.X, (float)(-geometry.ShellRadius * Math.Sin(Math.PI/3)) + origin.Y),
                DimensionLineLocation = new PointF(geometry.ShellRadius + 40 + origin.X, -geometry.ShellRadius - 40 + origin.Y),
                Type = DimensionType.Diameter,
                OverrideText = "SHELL ID Ø<>",
                EntityColor = Color.Blue,
                TargetPaperSpaceHeight = dimTextHeight
            });

            entities.Add(new CadDimension
            {
                SelectionPoint = new PointF((float)(geometry.BoltPcdRadius * Math.Cos(Math.PI*2/3)) + origin.X, (float)(-geometry.BoltPcdRadius * Math.Sin(Math.PI*2/3)) + origin.Y),
                DimensionLineLocation = new PointF(-geometry.BoltPcdRadius - 40 + origin.X, -geometry.BoltPcdRadius - 40 + origin.Y),
                Type = DimensionType.Diameter,
                OverrideText = "PCD Ø<>",
                EntityColor = Color.Red,
                TargetPaperSpaceHeight = dimTextHeight
            });

            float flangeIdRadius = (float)(data.FlangeID / 2.0);
            entities.Add(new CadDimension
            {
                SelectionPoint = new PointF((float)(flangeIdRadius * Math.Cos(Math.PI*5/6)) + origin.X, (float)(-flangeIdRadius * Math.Sin(Math.PI*5/6)) + origin.Y),
                DimensionLineLocation = new PointF(-flangeIdRadius - 60 + origin.X, -flangeIdRadius - 60 + origin.Y),
                Type = DimensionType.Diameter,
                OverrideText = "FLANGE ID Ø<>",
                EntityColor = Color.Blue,
                TargetPaperSpaceHeight = dimTextHeight
            });

            entities.Add(new CadDimension
            {
                SelectionPoint = new PointF(0, geometry.OuterDiameter / 2f + origin.Y),
                DimensionLineLocation = new PointF(geometry.OuterDiameter / 2f + 40 + origin.X, geometry.OuterDiameter / 2f + 40 + origin.Y),
                Type = DimensionType.Diameter,
                OverrideText = "FINISH OD Ø<>",
                EntityColor = Color.Blue,
                TargetPaperSpaceHeight = dimTextHeight
            });



            return entities;
        }

        private List<CalloutLeader> GenerateCalloutModels(GeometryModel geometry, EngineeringDataModel data)
        {
            List<CalloutLeader> leaders = new List<CalloutLeader>();
            float outerRad = geometry.OuterDiameter / 2f;
            float pcdRad = geometry.BoltPcdRadius;
            
            // Dynamically set clearances and shared baseline to prevent overlapping boundaries and each other
            float annotationSideClearance = Math.Max(120f, outerRad * 0.25f);
            float sharedTopY = outerRad + 240f;
            float gasketNoteCenterY = -outerRad - 125f;

            // 1. Bolt Holes (Top Left)
            leaders.Add(new CalloutLeader
            {
                Text = $"Ø{data.HoleDia}, {data.NoOfBolts} HOLES EQUI.\nON {data.BoltPCD} P.C.D.",
                TargetPoint = FindBoltHoleNear(geometry, new PointF((float)(-pcdRad * Math.Cos(Math.PI / 4)), (float)(pcdRad * Math.Sin(Math.PI / 4)))),
                AlignRight = false,
                LeaderVerticalDirection = 1f,
                TextCenterY = sharedTopY,
                SideClearance = annotationSideClearance
            });

            // 2. Gasket Seating (Bottom Left)
            leaders.Add(new CalloutLeader
            {
                Text = "GASKET SEATING SURFACE\nFOR PASS PARTITION PLATE",
                TargetPoint = new PointF(-outerRad + 20, 0),
                AlignRight = false,
                LeaderVerticalDirection = -1f,
                TextCenterY = gasketNoteCenterY,
                SideClearance = annotationSideClearance
            });

            // 3. Tubes (Top Right)
            leaders.Add(new CalloutLeader
            {
                Text = $"{data.TubeQty} NOS. TUBE HOLES\nFOR Ø{data.TubeOD:F1}",
                TargetPoint = FindOuterTubeHole(geometry, rightSide: true),
                AlignRight = true,
                LeaderVerticalDirection = 1f,
                TextCenterY = sharedTopY,
                SideClearance = annotationSideClearance
            });

            return leaders;
        }

        private PointF FindBoltHoleNear(GeometryModel geometry, PointF fallback)
        {
            PointF best = fallback;
            float bestDistance = float.MaxValue;

            foreach (var hole in geometry.BoltHoleCoordinates)
            {
                float dx = hole.X - fallback.X;
                float dy = hole.Y - fallback.Y;
                float distance = dx * dx + dy * dy;
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = hole;
                }
            }

            return best;
        }

        private PointF FindOuterTubeHole(GeometryModel geometry, bool rightSide)
        {
            PointF fallback = new PointF(
                (rightSide ? 1f : -1f) * (float)(geometry.OuterDiameter * 0.4f * Math.Cos(Math.PI / 4)),
                (float)(-geometry.OuterDiameter * 0.4f * Math.Sin(Math.PI / 4)));
            PointF best = fallback;
            float bestDistance = float.MinValue;

            foreach (var tube in geometry.TubeCoordinates)
            {
                if ((rightSide && tube.X <= 0f) || (!rightSide && tube.X >= 0f) || tube.Y <= 0f)
                {
                    continue;
                }

                float distance = tube.X * tube.X + tube.Y * tube.Y;
                if (distance > bestDistance)
                {
                    bestDistance = distance;
                    best = tube;
                }
            }

            return best;
        }
    }
}
