using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.Storage.Queue;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using UploadPortal.Models;
using System.Text.Json;
using System.Text.Json.Serialization;



namespace UploadPortal.Controllers
{
    public class FileBlobLookup
    {
        public string Container { get; set; }

        public string Blob { get; set; }
    }
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _configuration;

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            // Retrieve storage account from connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(_configuration["AzureWebJobsStorage"]);

            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            var containers = blobClient.ListContainers();
            
            return View(containers);
        }

        public IActionResult Privacy()
        {
            return View();
        }


        [HttpPost]
        public IActionResult UploadTestFile(IFormFile file, string containerName)
        {           
            // Retrieve storage account from connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(_configuration["AzureWebJobsStorage"]);

            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve reference to a previously created container.
            CloudBlobContainer container = blobClient.GetContainerReference(containerName);

            // Create the container if it doesn't already exist.
            container.CreateIfNotExists();

            container.SetPermissions(
                new BlobContainerPermissions
                {
                    PublicAccess =
                        BlobContainerPublicAccessType.Blob
                });           

            // Retrieve reference to a blob named "myblob".
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(file.FileName);

            blockBlob.UploadFromStream(file.OpenReadStream());

            // put a message to queue
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            CloudQueue q = queueClient.GetQueueReference("queue");
            
            var l = new FileBlobLookup { Container = containerName, Blob = file.FileName };

            CloudQueueMessage message = new CloudQueueMessage(JsonSerializer.Serialize(l));            
            q.AddMessage(message);

            ViewBag.Message = "Successful!" + 
                "<br/>Container: " + containerName +
                "<br/>Blob name: " + file.FileName +
                "<br/>Size: " + file.Length;

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
