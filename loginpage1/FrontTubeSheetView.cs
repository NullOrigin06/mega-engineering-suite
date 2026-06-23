using System;
using System.Collections.Generic;
using System.Drawing;

namespace loginpage1
{
    public class FrontTubeSheetView : TubeSheetViewBase
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
                Text = "VIEW FROM D\nFRONT TUBE SHEET",
                Position = new PointF(0, (geometry.OuterDiameter / 2f) + 25),
                EntityColor = Color.DodgerBlue,
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center,
                TargetPaperSpaceHeight = titleTextHeight 
            });

            // 3. Row Counts
            entities.AddRange(rowCountRenderer.GenerateRowCounts(geometry, alignLeft: true));

            // 4. Annotations & Callouts
            var callouts = GenerateFrontCalloutModels(geometry, data);
            float exclusionRadius = geometry.OuterDiameter / 2f;
            entities.AddRange(annotationEngine.GenerateAnnotations(callouts, exclusionRadius));

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

            float otlRadius = (geometry.ShellRadius * 2 - (float)data.TubeOD * 2) / 2f;
            entities.Add(new CadDimension
            {
                StartPoint = new PointF(-otlRadius + origin.X, origin.Y),
                EndPoint = new PointF(otlRadius + origin.X, origin.Y),
                DimensionLineLocation = new PointF(origin.X, otlRadius + 40 + origin.Y),
                Type = DimensionType.Horizontal,
                OverrideText = "OTL Ø<>",
                EntityColor = Color.Magenta,
                TargetPaperSpaceHeight = dimTextHeight
            });

            return entities;
        }

        private List<CalloutLeader> GenerateFrontCalloutModels(GeometryModel geometry, EngineeringDataModel data)
        {
            List<CalloutLeader> leaders = new List<CalloutLeader>();
            float outerRad = geometry.OuterDiameter / 2f;
            float pcdRad = geometry.BoltPcdRadius;

            // 1. Bolt Holes (Top Right, mirrored from Rear View)
            leaders.Add(new CalloutLeader
            {
                Text = $"Ø{data.HoleDia}, {data.NoOfBolts} HOLES EQUI.",
                TargetPoint = new PointF((float)(pcdRad * Math.Cos(Math.PI/4)), (float)(-pcdRad * Math.Sin(Math.PI/4))),
                AlignRight = true
            });

            // 2. Gasket Seating (Middle Right)
            leaders.Add(new CalloutLeader
            {
                Text = "GASKET SEATING SURFACE\nFOR PASS PARTITION PLATE",
                TargetPoint = new PointF(outerRad - 20, 0),
                AlignRight = true
            });

            // 3. Tubes (Top Left, mirrored)
            leaders.Add(new CalloutLeader
            {
                Text = $"{data.TubeQty} NOS TUBES\nHOLES FOR Ø{data.TubeOD:F2}\nON TRIANGULAR PITCH",
                TargetPoint = new PointF((float)(-outerRad * 0.8 * Math.Cos(Math.PI/4)), (float)(-outerRad * 0.8 * Math.Sin(Math.PI/4))),
                AlignRight = false
            });

            // 4. Partition Thickness (Bottom Center)
            leaders.Add(new CalloutLeader
            {
                Text = $"{data.TubeSheetFinishTHK} THK",
                TargetPoint = new PointF(0, outerRad - 20),
                AlignRight = false
            });

            return leaders;
        }
    }
}
