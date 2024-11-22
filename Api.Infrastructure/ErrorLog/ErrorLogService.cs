using Api.Domain.Entities;
using Api.Domain.Interfaces.Infraestructure;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Api.Infrastructure.ErrorLog
{
    public class ErrorLogService : IErrorLogService
    {
        private readonly HttpClient _httpClient;

        public ErrorLogService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // Método sincrónico con un mensaje de error por defecto y no reintentable
        public void LogFailedPurchase(Purchase purchase)
        {
            SendFailedPurchaseAsync(purchase, "Error desconocido", false).Wait();
        }

        // Método sincrónico para registrar una compra fallida con mensaje y retriabilidad específicos
        public void LogFailedPurchase(Purchase purchase, string errorMessage, bool isRetriable)
        {
            SendFailedPurchaseAsync(purchase, errorMessage, isRetriable).Wait();
        }

        // Método asíncrono para registrar una compra fallida con mensaje y retriabilidad específicos
        public async Task LogFailedPurchaseAsync(Purchase purchase, string errorMessage, bool isRetriable)
        {
            await SendFailedPurchaseAsync(purchase, errorMessage, isRetriable);
        }

        // Método asíncrono para enviar los datos de la compra fallida al endpoint especificado
        public async Task SendFailedPurchaseAsync(Purchase purchase, string errorMessage, bool isRetriable)
        {
            var failedPurchase = new
            {
                id = purchase.Id,
                cardNumber = purchase.Card?.CreditCardNumber,
                purchaseDate = purchase.PurchaseDate.ToString("o"),
                amount = purchase.Amount,
                status = purchase.Status.ToString(),
                errorMessage = errorMessage,
                isRetriable = isRetriable, // Enviar el valor correcto de isRetriable
                createdAt = DateTime.UtcNow.ToString("o")
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(failedPurchase), Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync("https://ptgzg54q-7209.use2.devtunnels.ms/api/ErrorLog", jsonContent);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Error al enviar la compra fallida: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Excepción al enviar la compra fallida: {ex.Message}");
            }
        }
    }
}
