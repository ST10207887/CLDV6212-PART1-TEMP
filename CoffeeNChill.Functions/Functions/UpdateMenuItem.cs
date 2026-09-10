using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Azure.Data.Tables;
using CoffeeNChill.Functions.Models;
using System.Text.Json;

namespace CoffeeNChill.Functions.Functions
{
    public static class UpdateMenuItem
    {
        [Function("UpdateMenuItem")]
        public static async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = "menu/{id}")] HttpRequestData req,
            string id,
            FunctionContext executionContext)
        {
            var log = executionContext.GetLogger("UpdateMenuItem");
            log.LogInformation($"Processing UpdateMenuItem request for RowKey: {id}");

            var serviceClient = new TableServiceClient("UseDevelopmentStorage=true");
            var tableClient = serviceClient.GetTableClient("MenuItems");
            await tableClient.CreateIfNotExistsAsync();

            // Read request body
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var updateData = JsonSerializer.Deserialize<MenuItem>(requestBody);

            if (updateData == null || string.IsNullOrEmpty(updateData.PartitionKey))
            {
                var badResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                await badResponse.WriteStringAsync("Invalid input. PartitionKey is required.");
                return badResponse;
            }

            // Retrieve existing entity
            try
            {
                var existing = await tableClient.GetEntityAsync<MenuItem>(updateData.PartitionKey, id);

                // Update fields
                existing.Value.Price = updateData.Price;
                existing.Value.IsAvailable = updateData.IsAvailable;

                await tableClient.UpdateEntityAsync(existing.Value, existing.Value.ETag, TableUpdateMode.Replace);

                var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
                await response.WriteStringAsync($"Menu item {id} updated successfully.");
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
