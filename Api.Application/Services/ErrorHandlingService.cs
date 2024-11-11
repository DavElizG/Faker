using Api.Domain.Entities;
using Api.Domain.Interfaces;
using Api.Domain.Interfaces.Infraestructure;
using System;

namespace Api.Application.Services
{
    public class ErrorHandlingService : IErrorHandlingService
    {
        private readonly IErrorLogService _errorLogService;

        public ErrorHandlingService(IErrorLogService errorLogService)
        {
            _errorLogService = errorLogService;
        }

        public void HandleError(Exception ex, Purchase purchase)
        {
            LogError(purchase, $"Error General: {ex.Message}", isRetriable: false);
        }

        public void HandleCardError(Exception ex, Purchase purchase)
        {
            LogError(purchase, $"Error de Tarjeta: {ex.Message}", isRetriable: false);
        }

        public void HandlePurchaseStatusError(Exception ex, Purchase purchase)
        {
            LogError(purchase, $"Error de Estado de Compra: {ex.Message}", isRetriable: false);
        }

        public void HandleNoFundsError(Guid cardId, Purchase purchase)
        {
            var message = $"Error: La tarjeta con ID {cardId} no tiene fondos suficientes.";
            LogError(purchase, message, isRetriable: true);
        }

        public void HandleInactiveCardError(Guid cardId, Purchase purchase)
        {
            var message = $"Error: La tarjeta con ID {cardId} está inactiva.";
            LogError(purchase, message, isRetriable: false);
        }

        public void HandleTransactionLimitExceededError(Guid cardId, decimal amount, Purchase purchase)
        {
            var message = $"Error: La tarjeta con ID {cardId} ha excedido el límite de transacción con un monto de {amount}.";
            LogError(purchase, message, isRetriable: true);
        }

        public void HandleCardExpiredError(Guid cardId, Purchase purchase)
        {
            var message = $"Error: La tarjeta con ID {cardId} ha expirado.";
            LogError(purchase, message, isRetriable: false);
        }

        public void HandleFraudDetectedError(Guid cardId, Purchase purchase)
        {
            var message = $"Error: Se ha detectado fraude en la tarjeta con ID {cardId}.";
            LogError(purchase, message, isRetriable: false);
        }

        public void LogError(Purchase purchase, string message, bool isRetriable)
        {
            _errorLogService.LogFailedPurchase(purchase, message, isRetriable);
        }

        public bool IsRetriableError(string errorType)
        {
            return errorType switch
            {
                "NoFunds" => true,
                "TransactionLimitExceeded" => true,
                _ => false
            };
        }
    }
}
