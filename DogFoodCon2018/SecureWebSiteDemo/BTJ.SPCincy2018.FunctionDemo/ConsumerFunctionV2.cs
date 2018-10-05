using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;

namespace BTJ.SPCincy2018.FunctionDemo
{
    public static class ConsumerFunctionV2
    {
        [FunctionName("ConsumerFunctionV2")]
        [Disable("ConsumerFunctionV2-Disable")]
        public static void Run([QueueTrigger("%demo-queue-name%", Connection = "functionQueueStorage")]string myQueueItem, TraceWriter log)
        {
            log.Info($"Consumer firing from queue processed: {myQueueItem}");

        }
    }
}
