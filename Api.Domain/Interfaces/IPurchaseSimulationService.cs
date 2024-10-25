using Api.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Domain.Interfaces
{
    public interface IPurchaseSimulationService
    {
        void GeneratePurchases(List<Product> products, List<Affiliate> affiliates, List<Card> cards, int count);
        List<Purchase> GetPurchases();
        Purchase GeneratePurchase();
        Task SimulatePurchaseAsync();
        bool ProcessPurchase(Purchase purchase, Card card);
        Task<bool> ProcessPurchaseAsync(Purchase purchase, Card card); 
    }
}
