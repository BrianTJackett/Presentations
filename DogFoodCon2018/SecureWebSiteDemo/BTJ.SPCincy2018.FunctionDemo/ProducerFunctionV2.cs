using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;

namespace BTJ.SPCincy2018.FunctionDemo
{
    public static class ProducerFunctionV2
    {
        [FunctionName("ProducerFunctionV2")]
        [Disable("ProducerFunctionV2-Disable")]
        public static void Run([TimerTrigger("%producerTimerSchedule%")]TimerInfo myTimer, [Queue("%demo-queue-name%", Connection = "functionQueueStorage")]ICollector<string> queueData, TraceWriter log)
        {
            log.Info($"C# Timer trigger function executed at: {DateTime.Now}");
            queueData.Add($"new item at {DateTime.Now}");
        }
    }
}
