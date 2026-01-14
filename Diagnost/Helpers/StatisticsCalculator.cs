using Diagnost.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Diagnost.Helpers
{
    public static class StatisticsCalculator
    {
        // Розрахунок для Тесту 1
        public static DiagnosticResult CalculatePzmr(List<long> reactionTimes, int errorsTotal)
        {
            var result = new DiagnosticResult();
            
            if (reactionTimes != null && reactionTimes.Any())
            {
                double avg = reactionTimes.Average();
                
                // Стандартне відхилення (Sample Standard Deviation)
                double sumSquares = reactionTimes.Sum(t => Math.Pow(t - avg, 2));
                double stdDev = reactionTimes.Count > 1 ? Math.Sqrt(sumSquares / (reactionTimes.Count - 1)) : 0;

                
                result.PZMRLatet = Math.Round(avg, 2);  
                result.PZMRvidhil = Math.Round(stdDev, 4);    
            }
            
            result.PZMR_ErrorsTotal = errorsTotal;
            return result;
        }

        // Розрахунок для Тесту 2
        public static DiagnosticResult CalculatePV2(List<long> reactionTimes,
            int currentAttempt,
            int errorsMissed,
            int errorsWrongBtn,
            int errorsFalseAlarm)
        {
            var result = new DiagnosticResult();
            
            if (reactionTimes != null && reactionTimes.Any())
            {
                double avg = reactionTimes.Average();
                
                double sumSquares = reactionTimes.Sum(t => Math.Pow(t - avg, 2));
                double stdDev = reactionTimes.Count > 1 ? Math.Sqrt(sumSquares / (reactionTimes.Count - 1)) : 0;
                
                result.PV2_3Latet = Math.Round(avg, 2); 
                result.PV2_StdDev_ms = Math.Round(stdDev, 4);
            }
            
            result.PV2_ErrorsMissed = errorsMissed;
            result.PV2_ErrorsWrongButton = errorsWrongBtn;
            result.PV2_ErrorsFalseAlarm = errorsFalseAlarm;
            
            return result;
        }

        // Розрахунок для Тесту 3
        public static DiagnosticResult CalculateUFP(List<long> reactionTimes, int currentAttempt, int errorsMissed,
            int errorsWrongBtn, int errorsTotal)
        {
           var result = new DiagnosticResult();
           if (reactionTimes != null && reactionTimes.Any())
           {
               double avg = reactionTimes.Average();
               
               double sumSquares = reactionTimes.Sum(t => Math.Pow(t - avg, 2));
               double stdDev = reactionTimes.Count > 1 ? Math.Sqrt(sumSquares / (reactionTimes.Count - 1)) : 0;
               
               result.UFPLatet = Math.Round(avg, 2);
               result.UFP_StdDev_ms = Math.Round(stdDev, 4);
           }

           result.UFP_ErrorsTotal = errorsTotal;

           return result;
        }
    }
}