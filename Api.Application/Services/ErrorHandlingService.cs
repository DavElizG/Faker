using Api.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Api.Application.Services
{
    public class ErrorHandlingService : IErrorHandlingService
    {
        private readonly HttpClient _httpClient;
        private readonly string _errorEndpoint;

        public ErrorHandlingService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _errorEndpoint = $"{configuration["ErrorService:Url"]}/api/purchases/log-error"; // Cambia el endpoint si es diferente
        }

        // Método auxiliar para enviar el error al endpoint
        private async Task SendErrorToEndpointAsync(object error)
        {
            try
            {
                var errorContent = JsonSerializer.Serialize(error);
                var content = new StringContent(errorContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(_errorEndpoint, content);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al enviar el error al endpoint: {ex.Message}");
            }
        }

        public void HandleError(Exception ex)
        {
            var error = new { ErrorType = "GeneralError", Message = ex.Message };
            SendErrorToEndpointAsync(error).Wait();
        }

        public void HandleCardError(Exception ex)
        {
            var error = new { ErrorType = "CardError", Message = ex.Message };
            SendErrorToEndpointAsync(error).Wait();
        }

        public void HandlePurchaseStatusError(Exception ex)
        {
            var error = new { ErrorType = "PurchaseStatusError", Message = ex.Message };
            SendErrorToEndpointAsync(error).Wait();
        }

        public void LogError(string message)
        {
            var error = new { ErrorType = "LogError", Message = message };
            SendErrorToEndpointAsync(error).Wait();
        }

        public void HandleNoFundsError(Guid cardId)
        {
            var error = new { ErrorType = "NoFunds", CardId = cardId, Message = "Fondos insuficientes." };
            SendErrorToEndpointAsync(error).Wait();
        }

        public void HandleInactiveCardError(Guid cardId)
        {
            var error = new { ErrorType = "InactiveCard", CardId = cardId, Message = "La tarjeta está inactiva." };
            SendErrorToEndpointAsync(error).Wait();
        }

        public void HandleTransactionLimitExceededError(Guid cardId, decimal amount)
        {
            var error = new { ErrorType = "TransactionLimitExceeded", CardId = cardId, Amount = amount, Message = "Límite de transacción excedido." };
            SendErrorToEndpointAsync(error).Wait();
        }

        public void HandleCardExpiredError(Guid cardId)
        {
            var error = new { ErrorType = "CardExpired", CardId = cardId, Message = "La tarjeta ha expirado." };
            SendErrorToEndpointAsync(error).Wait();
        }

        public void HandleFraudDetectedError(Guid cardId)
        {
            var error = new { ErrorType = "FraudDetected", CardId = cardId, Message = "Actividad fraudulenta detectada." };
            SendErrorToEndpointAsync(error).Wait();
        }
        public async Task LogErrorAsync(object error)
        {
            try
            {
                var errorContent = JsonSerializer.Serialize(error);
                var content = new StringContent(errorContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(_errorEndpoint, content);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al enviar el error al endpoint: {ex.Message}");
            }
        }
    }
}
