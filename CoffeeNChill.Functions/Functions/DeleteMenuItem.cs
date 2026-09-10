using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Azure.Data.Tables;
using CoffeeNChill.Functions.Models;

namespace CoffeeNChill.Functions.Functions
{
    public static class DeleteMenuItem
    {
        [Function("DeleteMenuItem")]
        public static async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "menu/{partitionKey}/{id}")] HttpRequestData req,
            string partitionKey,
            string id,
            FunctionContext executionContext)
        {
            var log = executionContext.GetLogger("DeleteMenuItem");
            log.LogInformation($"Processing DeleteMenuItem request for PartitionKey: {partitionKey}, RowKey: {id}");

            var serviceClient = new TableServiceClient("UseDevelopmentStorage=true");
            var tableClient = serviceClient.GetTableClient("MenuItems");
            await tableClient.CreateIfNotExistsAsync();

            try
            {
                await tableClient.DeleteEntityAsync(partitionKey, id);

                var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
                await response.WriteStringAsync($"Menu item {id} deleted successfully.");
                return response;
            }
            catch (Azure.RequestFailedException)
            {
                var notFoundResponse = req.CreateResponse(System.Net.HttpStatusCode.NotFound);
                await notFoundResponse.WriteStringAsync($"Menu item with RowKey {id} not found.");
                return notFoundResponse;
            }
        }
    }
}
