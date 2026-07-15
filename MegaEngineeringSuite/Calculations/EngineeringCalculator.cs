using System;

namespace MegaEngineeringSuite.Calculations
{
    public static class EngineeringCalculator
    {
        public static double CalculateTubeSheetWeight(EngineeringDataModel data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            // Formula:
            // ( (0.7854 * TubeSheetFinishOD^2 * TubeSheetFinishTHK * 8 * 10^-6) 
            // - (0.7854 * TubeOD^2 * TubeSheetFinishTHK * 8 * 10^-6 * TubeQty) ) * 2
            
            double term1 = 0.7854 * Math.Pow(data.TubeSheetFinishOD, 2) * data.TubeSheetFinishTHK * 8 * Math.Pow(10, -6);
            double term2 = 0.7854 * Math.Pow(data.TubeOD, 2) * data.TubeSheetFinishTHK * 8 * Math.Pow(10, -6) * data.TubeQty;
            
            return (term1 - term2) * 2.0;
        }


    }
}
