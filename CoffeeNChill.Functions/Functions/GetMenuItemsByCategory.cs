using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Azure.Data.Tables;
using CoffeeNChill.Functions.Models;

namespace CoffeeNChill.Functions.Functions
{
    public static class GetMenuItemsByCategory
    {
        [Function("GetMenuItemsByCategory")]
        public static async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "menu/category/{category}")] HttpRequestData req,
            string category,
            FunctionContext executionContext)
        {
            var log = executionContext.GetLogger("GetMenuItemsByCategory");
            log.LogInformation($"Processing GetMenuItemsByCategory request for category: {category}");

            var serviceClient = new TableServiceClient("UseDevelopmentStorage=true");
            var tableClient = serviceClient.GetTableClient("MenuItems");
            await tableClient.CreateIfNotExistsAsync();

            // Query by PartitionKey (category)
            var items = tableClient.Query<MenuItem>(x => x.PartitionKey == category).ToList();

            var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
            await response.WriteAsJsonAsync(items);
            return response;
        }
    }
}
