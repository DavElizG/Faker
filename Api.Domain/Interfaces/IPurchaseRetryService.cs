using Api.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Domain.Interfaces
{
    public interface IPurchaseRetryService
    {
        void RetryFailedPurchases(); // Reintento sincrónico de todas las fallidas
        Task RetryFailedPurchasesAsync(); // Reintento asincrónico de todas las fallidas
        void AddFailedPurchase(Purchase purchase); // Agregar una compra fallida
        Task<bool> RetryFailedPurchaseByIdAsync(Guid purchaseId); // Nuevo: reintento de una compra específica
        List<Purchase> GetAllFailedPurchases(); // Nuevo: obtener todas las compras fallidas


        
    }
}
