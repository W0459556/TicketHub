using Azure.Storage.Queues;
using System.Text.Json;
using TicketHub.Api.Models;

namespace TicketHub.Api.Services
{
    public class QueueService
    {
        private readonly QueueClient _queueClient;

        public QueueService(IConfiguration configuration)
        {
            var connectionString = configuration["AzureStorage:ConnectionString"]
                ?? throw new ArgumentNullException("Missing connection string");
            _queueClient = new QueueClient(connectionString, "tickethub");
        }

        public async Task SendMessageAsync(PurchaseRequest purchaseRequest)
        {
            await _queueClient.CreateIfNotExistsAsync();
            var message = JsonSerializer.Serialize(purchaseRequest);
            await _queueClient.SendMessageAsync(message);
        }
    }
}