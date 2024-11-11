using Api.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Domain.Interfaces
{
    public interface IErrorHandlingService
    {
        void HandleError(Exception ex, Purchase purchase); // Agregar parámetro Purchase
        void HandleCardError(Exception ex, Purchase purchase); // Agregar parámetro Purchase
        void HandlePurchaseStatusError(Exception ex, Purchase purchase); // Agregar parámetro Purchase
        void LogError(Purchase purchase, string message, bool isRetriable);
        void HandleNoFundsError(Guid cardId, Purchase purchase); // Agregar parámetro Purchase
        void HandleInactiveCardError(Guid cardId, Purchase purchase); // Agregar parámetro Purchase
        void HandleTransactionLimitExceededError(Guid cardId, decimal amount, Purchase purchase); // Agregar parámetro Purchase
        void HandleCardExpiredError(Guid cardId, Purchase purchase); // Agregar parámetro Purchase
        void HandleFraudDetectedError(Guid cardId, Purchase purchase); // Agregar parámetro Purchase
        bool IsRetriableError(string errorType);
    }

}
