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
                if (purchase == null)
                {
                    _logger.LogError("Failed to deserialize purchase request");
                    return;
                }

                using var conn = new SqlConnection(Environment.GetEnvironmentVariable("SQL_CONNECTION_STRING"));
                await conn.OpenAsync();

                // Extract last 4 digits of credit card
                var creditCardLast4 = purchase.CreditCard.Length >= 4
                    ? purchase.CreditCard.Substring(purchase.CreditCard.Length - 4)
                    : purchase.CreditCard;

                using var cmd = new SqlCommand(@"
                    INSERT INTO TicketPurchases (
                        ConcertId, Email, Name, Phone, Quantity,
                        CreditCard, CreditCardLast4, Expiration, SecurityCode,
                        Address, City, Province, PostalCode, Country,
                        RawData
                    ) VALUES (
                        @ConcertId, @Email, @Name, @Phone, @Quantity,
                        @CreditCard, @CreditCardLast4, @Expiration, @SecurityCode,
                        @Address, @City, @Province, @PostalCode, @Country,
                        @RawData
                    )", conn);

                // Add parameters
                cmd.Parameters.AddWithValue("@ConcertId", purchase.ConcertId);
                cmd.Parameters.AddWithValue("@Email", purchase.Email.Trim());
                cmd.Parameters.AddWithValue("@Name", purchase.Name.Trim());
                cmd.Parameters.AddWithValue("@Phone", purchase.Phone.Trim());
                cmd.Parameters.AddWithValue("@Quantity", purchase.Quantity);
                cmd.Parameters.AddWithValue("@CreditCard", purchase.CreditCard.Trim());
                cmd.Parameters.AddWithValue("@CreditCardLast4", creditCardLast4);
                cmd.Parameters.AddWithValue("@Expiration", purchase.Expiration.Trim());
                cmd.Parameters.AddWithValue("@SecurityCode", purchase.SecurityCode.Trim());
                cmd.Parameters.AddWithValue("@Address", purchase.Address.Trim());
                cmd.Parameters.AddWithValue("@City", purchase.City.Trim());
                cmd.Parameters.AddWithValue("@Province", purchase.Province.Trim());
                cmd.Parameters.AddWithValue("@PostalCode", purchase.PostalCode.Trim());
                cmd.Parameters.AddWithValue("@Country", purchase.Country.Trim());
                cmd.Parameters.AddWithValue("@RawData", queueItem);

                await cmd.ExecuteNonQueryAsync();

                _logger.LogInformation($"Successfully processed purchase for {purchase.Email}");
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "JSON deserialization error");
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Database error occurred");
                throw; // Will retry for transient errors
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error processing purchase");
                throw;
            }
        }
    }
}