using System;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using TicketHub.Api.Models; 

namespace TicketHub.Functions
{
    public class ProcessTicketPurchase
    {
        private readonly ILogger _logger;

        public ProcessTicketPurchase(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<ProcessTicketPurchase>();
        }

        [Function("ProcessTicketPurchase")]
        public async Task Run(
            [QueueTrigger("tickethub", Connection = "AzureWebJobsStorage")] string queueItem)
        {
            try
            {
                var purchase = JsonSerializer.Deserialize<PurchaseRequest>(queueItem);
                if (purchase == null) throw new ArgumentException("Invalid queue message");

                string? sqlConnString = Environment.GetEnvironmentVariable("SQL_CONNECTION_STRING");
                if (string.IsNullOrEmpty(sqlConnString))
                    throw new InvalidOperationException("SQL_CONNECTION_STRING is not set");

                using (var conn = new SqlConnection(sqlConnString))
                {
                    await conn.OpenAsync();

                    var cmd = new SqlCommand(@"
                        INSERT INTO TicketPurchases (
                            ConcertId, Email, Name, Phone, Quantity,
                            CreditCard, Expiration, SecurityCode,
                            Address, City, Province, PostalCode, Country,
                            RawData
                        ) VALUES (
                            @ConcertId, @Email, @Name, @Phone, @Quantity,
                            @CreditCard, @Expiration, @SecurityCode,
                            @Address, @City, @Province, @PostalCode, @Country,
                            @RawData
                        )", conn);

                    cmd.Parameters.AddWithValue("@ConcertId", purchase.ConcertId);
                    cmd.Parameters.AddWithValue("@Email", purchase.Email);
                    cmd.Parameters.AddWithValue("@Name", purchase.Name);
                    cmd.Parameters.AddWithValue("@Phone", purchase.Phone);
                    cmd.Parameters.AddWithValue("@Quantity", purchase.Quantity);
                    cmd.Parameters.AddWithValue("@CreditCard", purchase.CreditCard);
                    cmd.Parameters.AddWithValue("@Expiration", purchase.Expiration);
                    cmd.Parameters.AddWithValue("@SecurityCode", purchase.SecurityCode);
                    cmd.Parameters.AddWithValue("@Address", purchase.Address);
                    cmd.Parameters.AddWithValue("@City", purchase.City);
                    cmd.Parameters.AddWithValue("@Province", purchase.Province);
                    cmd.Parameters.AddWithValue("@PostalCode", purchase.PostalCode);
                    cmd.Parameters.AddWithValue("@Country", purchase.Country);
                    cmd.Parameters.AddWithValue("@RawData", queueItem); 

                    await cmd.ExecuteNonQueryAsync();
                }

                _logger.LogInformation($"Processed purchase for {purchase.Email}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing queue message");
                throw; 
            }
        }
    }
}