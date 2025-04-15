using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Azure.Storage.Queues;
using System.Runtime.InteropServices.Marshalling;
using System.Text.Json;
using System.Text;

namespace TicketHub.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PurchaseController : Controller
    {
        private readonly ILogger<PurchaseController> _logger;
        private readonly IConfiguration _configuration;

        public PurchaseController(ILogger<PurchaseController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok("Get success");
        }

        [HttpPost]
        public async Task<IActionResult> Post(Purchase tickethub)
        {
            if (ModelState.IsValid == false)
            {
                return BadRequest(ModelState);
            }
            string queueName = "tickethub";
            string? connectionString = _configuration["AzureStorageConnectionString"];
            QueueClient queueClient = new QueueClient(connectionString, queueName);

            string message = JsonSerializer.Serialize(tickethub);

            var plainTextBytes = Encoding.UTF8.GetBytes(message);
            await queueClient.SendMessageAsync(Convert.ToBase64String(plainTextBytes));

            return Ok("Success- message posted to Storge Queue");
        }
    }
}
