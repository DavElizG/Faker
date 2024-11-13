using Api.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Domain.Interfaces
{
    public interface IFailedPurchaseStore
    {
        void AddFailedPurchase(Purchase purchase);
        Purchase GetFailedPurchaseById(Guid purchaseId);
        List<Purchase> GetAllFailedPurchases();
        void RemoveFailedPurchase(Purchase purchase);
    }
}
