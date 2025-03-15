using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Hotel_Management_System.Services
{
    public class PayMongoService
    {
        private readonly HttpClient _httpClient;
        private readonly string _secretKey;

        public PayMongoService(IConfiguration configuration)
        {
            _httpClient = new HttpClient();
            _secretKey = configuration["PayMongo:SecretKey"] ?? throw new Exception("PayMongo API Key is missing!");

            string encodedKey = Convert.ToBase64String(Encoding.UTF8.GetBytes(_secretKey + ":"));
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Basic {encodedKey}");
        }

        public async Task<string?> CreatePayment(decimal amount, string currency, string paymentMethod)
        {
            currency = currency.ToUpper();

            var requestBody = new
            {
                data = new
                {
                    attributes = new
                    {
                        amount = (int)(amount * 100),
                        currency = currency,
                        payment_method_allowed = new[] { "gcash", "card", "paymaya" },
                        description = "Hotel Booking Payment",
                        statement_descriptor = "Nuxus Hotel",
                        capture_type = "automatic",
                        return_url = "https://localhost:5076/api/payment/payment-confirmation",
                        payment_method_options = new
                        {
                            card = new { request_three_d_secure = "any" }
                        }
                    }
                }
            };

            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("https://api.paymongo.com/v1/payment_intents", content);
            var responseBody = await response.Content.ReadAsStringAsync();

            Console.WriteLine("PayMongo Response: " + responseBody);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("Error: Failed to create payment intent.");
                return null;
            }

            return responseBody;
        }

        public async Task<string?> AttachPaymentMethod(string paymentIntentId, string paymentMethodId)
        {
            var requestBody = new
            {
                data = new
                {
                    attributes = new
                    {
                        payment_method = paymentMethodId,
                        client_key = paymentIntentId
                    }
                }
            };

            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"https://api.paymongo.com/v1/payment_intents/{paymentIntentId}/attach", content);
            var responseBody = await response.Content.ReadAsStringAsync();

            Console.WriteLine("Attach Payment Method Response: " + responseBody);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("Error: Failed to attach payment method.");
                return null;
            }

            return responseBody;
        }
    }
}

