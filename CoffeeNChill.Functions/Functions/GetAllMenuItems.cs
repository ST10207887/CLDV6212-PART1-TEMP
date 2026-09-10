using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Azure.Data.Tables;
using CoffeeNChill.Functions.Models;

namespace CoffeeNChill.Functions.Functions
{
    public static class GetAllMenuItems
    {
        [Function("GetAllMenuItems")]
        public static async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "menu")] HttpRequestData req,
            FunctionContext executionContext)
        {
            var log = executionContext.GetLogger("GetAllMenuItems");
            log.LogInformation("Processing GetAllMenuItems request...");

            var serviceClient = new TableServiceClient("UseDevelopmentStorage=true");
            var tableClient = serviceClient.GetTableClient("MenuItems");
            await tableClient.CreateIfNotExistsAsync();

            // Query all entities
            var items = tableClient.Query<MenuItem>().ToList();

            var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
            await response.WriteAsJsonAsync(items);
            return response;
        }
    }
}
