using System;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;

namespace BTJ.SPO.AutoApplyRetentionLabel
{
    // ref https://blog.kloud.com.au/2017/09/07/monitoring-azure-storage-queues-with-application-insights-and-azure-monitor/
    public static class RetentionQueueMonitor
    {
        //private static TelemetryClient _telemetry = new TelemetryClient() { InstrumentationKey = Constants.AppInsightsInstrumentationKey };

        [Disable]
        [FunctionName("RetentionQueueMonitor")]
        public static void Run([TimerTrigger("%QueueMonitorTimerSchedule%")]TimerInfo timerInfo, ILogger logger)
        {
            logger.LogInformation($"QueueMonitor timer trigger function executed at: {DateTime.UtcNow}.");

            var connectionString = Environment.GetEnvironmentVariable("ExternalStorageAccount");
            var account = CloudStorageAccount.Parse(connectionString);
            var queueClient = account.CreateCloudQueueClient();

            foreach (var queue in queueClient.ListQueues())
            {
                // get a reference to the queue and retrieve the queue length
                queue.FetchAttributes();
                var length = queue.ApproximateMessageCount;

                // log the length
                //_telemetry.TrackMetric($"Queue length - {queue.Name}", (double)length);
                logger.LogInformation($"Queue length - {queue.Name} - {(double)length}");
            }
        }
    }

}
