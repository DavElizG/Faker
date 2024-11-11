using Api.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Api.Domain.Interfaces
{
    public interface IPurchaseSimulationService
    {
        void GeneratePurchases(List<Product> products, List<Affiliate> affiliates, List<Card> cards, int count);
        List<Purchase> GetPurchases(); // Método para obtener la lista de compras generadas
        Purchase GeneratePurchase(); // Método para generar una compra individual
        Task SimulatePurchases(); // Método para generar y simular compras automáticamente
        Task SimulatePurchaseAsync(); // Método para simular una compra individual de manera asincrónica
        bool ProcessPurchase(Purchase purchase, Card card); // Método para procesar la compra de manera sincrónica
        Task<bool> ProcessPurchaseAsync(Purchase purchase, Card card); // Método para procesar la compra de manera asincrónica
        List<object> GetErrorLogs(); // Método para obtener la lista de errores registrados
    }
}
