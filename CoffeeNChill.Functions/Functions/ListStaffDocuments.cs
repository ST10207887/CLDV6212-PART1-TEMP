using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;

namespace CoffeeNChill.Functions.Functions
{
    public static class ListStaffDocuments
    {
        [Function("ListStaffDocuments")]
        public static async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "staff/list")] HttpRequestData req,
            FunctionContext executionContext)
        {
            var log = executionContext.GetLogger("ListStaffDocuments");
            log.LogInformation("Processing ListStaffDocuments request...");

            var connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
            var shareClient = new ShareClient(connectionString, "staff-docs");
            await shareClient.CreateIfNotExistsAsync();

            var directoryClient = shareClient.GetRootDirectoryClient();
            var files = directoryClient.GetFilesAndDirectories().ToList();

            var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
            await response.WriteAsJsonAsync(files.Select(f => new { f.Name, f.IsDirectory }));
            return response;
        }
    }
}
