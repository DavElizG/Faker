using System;

namespace Api.Domain.Interfaces
{
    public interface IErrorHandlingService
    {
        Task LogErrorAsync(object error);
        void HandleError(Exception ex);
        void HandleCardError(Exception ex);
        void HandlePurchaseStatusError(Exception ex);
        void LogError(string message);
        void HandleNoFundsError(Guid cardId);
        void HandleInactiveCardError(Guid cardId);
        void HandleTransactionLimitExceededError(Guid cardId, decimal amount);
        void HandleCardExpiredError(Guid cardId);
        void HandleFraudDetectedError(Guid cardId);
    }
}
