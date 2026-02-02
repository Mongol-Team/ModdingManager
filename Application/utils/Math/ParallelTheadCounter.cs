using Application.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Matematics = System.Math;
namespace Application.utils.Math
{
    public static class ParallelTheadCounter
    {
        public static int CalculateMaxDegreeOfParallelism()
        {
            if (ModManagerSettings.MaxPercentForParallelUsage <= 0)
                return 1;

            if (ModManagerSettings.MaxPercentForParallelUsage >= 100)
                return Environment.ProcessorCount;

            double percent = ModManagerSettings.MaxPercentForParallelUsage / 100.0;
            int threads = (int)Matematics.Max(1, Matematics.Round(Environment.ProcessorCount * percent));

            return Matematics.Min(threads, Environment.ProcessorCount);
        }
    }
}
