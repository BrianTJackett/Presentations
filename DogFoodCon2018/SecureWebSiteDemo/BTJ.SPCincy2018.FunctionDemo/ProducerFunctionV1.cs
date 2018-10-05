using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;

namespace BTJ.SPCincy2018.FunctionDemo
{
    public static class ProducerFunctionV1
    {
        [FunctionName("ProducerFunctionV1")]
        //[Disable]
        public static void Run([TimerTrigger("0 */1 * * * *")]TimerInfo myTimer, [Queue("BTJDevRampQueueVSDemo", Connection = "functionQueueStorage")]ICollector<string> queueData, TraceWriter log)
        {
            log.Info($"Producer executed at: {DateTime.Now}");
            queueData.Add($"new item at {DateTime.Now}");
        }
    }
}
