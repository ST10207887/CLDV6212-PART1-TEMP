using Azure;
using Azure.Storage.Files.Shares;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading.Tasks;

namespace CoffeeNChill.Functions.Functions
{
    public static class UploadStaffDocument
    {
        [Function("UploadStaffDocument")]
        public static async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "staff/upload/{fileName}")] HttpRequestData req,
            string fileName,
            FunctionContext executionContext)
        {
            var log = executionContext.GetLogger("UploadStaffDocument");
            log.LogInformation($"Uploading staff document: {fileName}");

            // Connect to Azurite File Share
            var connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
            var shareClient = new ShareClient(connectionString, "staff-docs");

            await shareClient.CreateIfNotExistsAsync();

            var directoryClient = shareClient.GetRootDirectoryClient();
            var fileClient = directoryClient.GetFileClient(fileName);

            // Read request body (file content)
            using var stream = new MemoryStream();
            await req.Body.CopyToAsync(stream);
            stream.Position = 0;

            await fileClient.CreateAsync(stream.Length);
            await fileClient.UploadRangeAsync(
                new HttpRange(0, stream.Length),
                stream);

            var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
            await response.WriteStringAsync($"File {fileName} uploaded successfully.");
            return response;
        }
    }
}
