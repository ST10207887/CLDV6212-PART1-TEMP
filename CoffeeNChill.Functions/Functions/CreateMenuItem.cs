using Azure.Data.Tables;
using CoffeeNChill.Functions.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace CoffeeNChill.Functions.Functions
{
    public class CreateMenuItem
    {
        private readonly ILogger<CreateMenuItem> _log;

        // The logger is now injected here via Dependency Injection
        public CreateMenuItem(ILogger<CreateMenuItem> log)
        {
            _log = log;
        }

        [Function("CreateMenuItem")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "menu")] HttpRequest req)
        {
            _log.LogInformation("Processing CreateMenuItem request...");

            // Read request body
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var input = JsonSerializer.Deserialize<MenuItem>(requestBody);

            if (input == null || string.IsNullOrEmpty(input.PartitionKey) || string.IsNullOrEmpty(input.RowKey))
            {
                return new BadRequestObjectResult("Invalid input. PartitionKey and RowKey are required.");
            }

            // Connect to Table Storage
            var serviceClient = new TableServiceClient("UseDevelopmentStorage=true");
            var tableClient = serviceClient.GetTableClient("MenuItems");
            await tableClient.CreateIfNotExistsAsync();

            // Insert entity
            await tableClient.AddEntityAsync(input);

            return new OkObjectResult($"Menu item {input.Name} added successfully.");
        }
    }
}
