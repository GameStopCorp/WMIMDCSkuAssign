using System;
using Microsoft.Extensions.Logging;

namespace SupplyChain.Svc.MSDCSKUAssign.Services.Extensions
{
    public static class LoggerExtension
    {
        public static void LogTimeDiff(DateTime t1, DateTime t2, ILogger log, string message)
        {
            TimeSpan diff = t2.Subtract(t1);
            log.LogInformation(message + ". Hours- {0}, Minutes- {1}, Seconds- {2}", diff.Hours, diff.Minutes, diff.Seconds);
        }
    }
}
