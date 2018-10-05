using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;

namespace BTJ.SPCincy2018.FunctionDemo
{
    public static class ConsumerFunctionV1
    {
        [FunctionName("ConsumerFunctionV1")]
        [Disable]
        public static void Run([QueueTrigger("BTJDevRampQueueVSDemo", Connection = "functionQueueStorage")]string myQueueItem, TraceWriter log)
        {
            log.Info($"Consumer firing from queue processed: {myQueueItem}");
        }
    }
}
