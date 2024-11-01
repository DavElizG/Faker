using Api.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Api.Domain.Interfaces
{
    public interface IPurchaseSimulationService
    {
        void GeneratePurchases(List<Product> products, List<Affiliate> affiliates, List<Card> cards, int count);
        List<Purchase> GetPurchases();
        Purchase GeneratePurchase();
        Task SimulatePurchases(); // Método para generar y simular compras automáticamente
        Task SimulatePurchaseAsync(); // Método para simular una compra individual
        bool ProcessPurchase(Purchase purchase, Card card);
        Task<bool> ProcessPurchaseAsync(Purchase purchase, Card card);
    }
}
