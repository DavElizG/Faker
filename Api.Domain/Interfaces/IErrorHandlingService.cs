using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Domain.Interfaces
{
    public interface IErrorHandlingService
    {
        void HandleError(Exception ex);
        void HandleCardError(Exception ex);
        void HandlePurchaseStatusError(Exception ex);
        void LogError(string message);
        void HandleNoFundsError(Guid cardId);
        void HandleInactiveCardError(Guid cardId);
    }
}
