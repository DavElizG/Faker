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
        Task LogFailedPurchaseAsync(Purchase purchase);
    }
}
