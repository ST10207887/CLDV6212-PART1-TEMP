using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Azure.Storage.Files.Shares;

namespace CoffeeNChill.Functions.Functions
{
    public static class DownloadStaffDocument
    {
        [Function("DownloadStaffDocument")]
        public static async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "staff/download/{fileName}")] HttpRequestData req,
            string fileName,
            FunctionContext executionContext)
        {
            var log = executionContext.GetLogger("DownloadStaffDocument");
            log.LogInformation($"Downloading staff document: {fileName}");

            var connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
            var shareClient = new ShareClient(connectionString, "staff-docs");
            await shareClient.CreateIfNotExistsAsync();

            var directoryClient = shareClient.GetRootDirectoryClient();
            var fileClient = directoryClient.GetFileClient(fileName);

            if (await fileClient.ExistsAsync())
            {
                var download = await fileClient.DownloadAsync();
                var stream = new MemoryStream();
                await download.Value.Content.CopyToAsync(stream);
                stream.Position = 0;

                var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
                response.Headers.Add("Content-Disposition", $"attachment; filename=\"{fileName}\"");

                await response.WriteBytesAsync(stream.ToArray());
                return response;
            }
            else
            {
                var notFoundResponse = req.CreateResponse(System.Net.HttpStatusCode.NotFound);
                await notFoundResponse.WriteStringAsync($"File {fileName} not found.");
                return notFoundResponse;
            }
        }
    }
}
