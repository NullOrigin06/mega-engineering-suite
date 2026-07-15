using System;
using MegaEngineeringSuite;
using MegaEngineeringSuite.Calculations;

namespace COMTestApp
{
    public static class TracingRunner
    {
        public static void Run()
        {
            Console.WriteLine("--- STAGE 11.1 RUNTIME TRACE ---");
            
            var data1 = new EngineeringDataModel 
            { 
                TubeSheetFinishOD = 1070, 
                TubeSheetFinishTHK = 25, 
                TubeOD = 19.05, 
                TubeQty = 600, 
                BaffleOD = 914, 
                BaffleTHK = 4 
            };

            Console.WriteLine($"TS_OD: {data1.TubeSheetFinishOD}");
            Console.WriteLine($"TS_THK: {data1.TubeSheetFinishTHK}");
            Console.WriteLine($"TS_WEIGHT: {EngineeringCalculator.CalculateTubeSheetWeight(data1)}");
        }
    }
}
