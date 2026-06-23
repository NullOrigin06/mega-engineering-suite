using System;
using System.Collections.Generic;
using System.Drawing;

namespace MegaEngineeringSuite
{
    public class GeometryCalculationService
    {
        public GeometryModel CalculateGeometry(EngineeringDataModel data)
        {
            ValidateGeometry(data);

            GeometryModel geometry = new GeometryModel();
            geometry.CenterPoint = CalculateCenterPoint();
            geometry.OuterDiameter = CalculateOuterDiameter(data.TubeSheetFinishOD);
            geometry.TubeSheetRadius = CalculateTubeSheetCircle(data.TubeSheetFinishOD);
            geometry.BoltPcdRadius = CalculateBoltCircle(data.BoltPCD);
            geometry.BoltHoleRadius = (float)(data.HoleDia / 2.0);
            geometry.BoltHoleCoordinates = CalculateBoltHoleCoordinates(data.BoltPCD, data.NoOfBolts);
            geometry.NumberOfBolts = data.NoOfBolts;

            // Tube Layout Generation
            TubeLayoutService tubeLayoutService = new TubeLayoutService();
            geometry.ShellRadius = (float)(data.ShellID / 2.0);
            geometry.ShellOuterRadius = (float)(data.FlangeID / 2.0);
            geometry.TubeRadius = (float)(data.TubeOD / 2.0);
            geometry.TubePitch = (float)(data.TubeOD * 1.25);
            geometry.NumberOfPasses = data.NoOfPass;
            geometry.PartitionPlateThickness = (float)data.PartitionPlateTHK;
            
            geometry.TubeCoordinates = tubeLayoutService.GenerateLayout(
                geometry.ShellRadius, 
                (float)data.TubeOD, 
                data.TubeQty, 
                (float)data.PartitionPlateTHK, 
                data.NoOfPass
            );

            // Row Analysis and Validation
            TubeRowAnalysisService rowAnalyzer = new TubeRowAnalysisService();
            geometry.RowTubeCounts = rowAnalyzer.AnalyzeRows(geometry.TubeCoordinates);
            rowAnalyzer.ValidateFabrication(geometry.TubeCoordinates, data.TubeQty);

            return geometry;
        }

        private void ValidateGeometry(EngineeringDataModel data)
        {
            if (data.NoOfBolts <= 0)
            {
                throw new ArgumentException("Number of bolts must be greater than zero.");
            }
            if (data.HoleDia <= 0)
            {
                throw new ArgumentException("Hole diameter must be a valid positive number.");
            }
            if (data.BoltPCD >= data.TubeSheetFinishOD)
            {
                throw new ArgumentException("Bolt PCD must be strictly less than the Tube Sheet O.D.");
            }
        }

        private PointF CalculateCenterPoint()
        {
            return new PointF(0, 0);
        }

        private float CalculateOuterDiameter(double tubeSheetOD)
        {
            return (float)tubeSheetOD;
        }

        private float CalculateTubeSheetCircle(double tubeSheetOD)
        {
            return (float)(tubeSheetOD / 2.0);
        }

        private float CalculateBoltCircle(double boltPCD)
        {
            return (float)(boltPCD / 2.0);
        }

        private List<PointF> CalculateBoltHoleCoordinates(double boltPcd, int noOfBolts)
        {
            List<PointF> coordinates = new List<PointF>();
            double radius = boltPcd / 2.0;
            
            // Assume the first bolt is at the top (90 degrees / PI/2 radians)
            // or we can just start at 0 radians (right side). Starting at right side is standard for CAD.
            double angleIncrement = (2 * Math.PI) / noOfBolts;
            
            for (int i = 0; i < noOfBolts; i++)
            {
                double angle = i * angleIncrement;
                float x = (float)(radius * Math.Cos(angle));
                float y = (float)(radius * Math.Sin(angle));
                coordinates.Add(new PointF(x, y));
            }
            
            return coordinates;
        }
    }
}
