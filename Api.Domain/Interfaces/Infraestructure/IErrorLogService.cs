using Api.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Domain.Interfaces.Infraestructure
{
    public interface IErrorLogService
    {
        void LogFailedPurchase(Purchase purchase);
        void LogFailedPurchase(Purchase purchase, string errorMessage, bool isRetriable); // Nueva sobrecarga
        Task LogFailedPurchaseAsync(Purchase purchase, string errorMessage, bool isRetriable);
        Task SendFailedPurchaseAsync(Purchase purchase, string errorMessage, bool isRetriable);
    }
}
