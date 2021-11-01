using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.IO;

namespace FleksbitWebJob
{
    public class Function
    {
        public class FileBlobLookup
        {
            public string Container { get; set; }

            public string Blob { get; set; }
        }
        public static void ProcessQueueMessage(
            [QueueTrigger("queue")] FileBlobLookup blobLookup, 
            [Blob("{Container}/{Blob}", FileAccess.Read)] Stream myBlob,
            [Blob("{Container}/kopija-{Blob}", FileAccess.Write)] Stream outputBlob,
            ILogger logger)
        {
            logger.LogInformation($"Blob name:{blobLookup.Blob} \n Size: {myBlob.Length} bytes");
            myBlob.CopyTo(outputBlob);
        }
    }
}
