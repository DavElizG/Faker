using Api.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Domain.Interfaces
{
    public interface IPurchaseProcessorService
    {
        Task<bool> ProcessPurchaseAsync(Purchase purchase, Card card);
    }

}
