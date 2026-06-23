using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace loginpage1
{
    public class TubeRowAnalysisService
    {
        public List<int> AnalyzeRows(List<PointF> tubeCoordinates)
        {
            if (tubeCoordinates == null || !tubeCoordinates.Any())
            {
                return new List<int>();
            }

            // Group tubes by their Y coordinate (rounded to 2 decimal places to handle float variations)
            // Order by Y descending (top to bottom)
            var groupedRows = tubeCoordinates
                .GroupBy(pt => Math.Round(pt.Y, 2))
                .OrderByDescending(g => g.Key)
                .Select(g => g.Count())
                .ToList();

            return groupedRows;
        }

        public void ValidateFabrication(List<PointF> tubeCoordinates, int expectedTubeCount)
        {
            if (tubeCoordinates == null) return;
            
            var rowCounts = AnalyzeRows(tubeCoordinates);
            int totalTubeCount = rowCounts.Sum();

            if (totalTubeCount != expectedTubeCount)
            {
                throw new InvalidOperationException($"Fabrication Validation Failed: Row count total ({totalTubeCount}) does not match expected generated tube quantity ({expectedTubeCount}).");
            }
        }
    }
}
