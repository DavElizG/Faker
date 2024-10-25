using Api.Domain.Entities;
using Api.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Application.Services
{
    public class PurchaseRetryService : IPurchaseRetryService
    {
        private readonly IPurchaseSimulationService _purchaseSimulationService;
        private readonly IErrorHandlingService _errorHandlingService;
        private readonly List<Purchase> _failedPurchases;

        public PurchaseRetryService(
            IPurchaseSimulationService purchaseSimulationService,
            IErrorHandlingService errorHandlingService)
        {
            _purchaseSimulationService = purchaseSimulationService;
            _errorHandlingService = errorHandlingService;
            _failedPurchases = new List<Purchase>();
        }

        public void RetryFailedPurchases()
        {
            foreach (var purchase in _failedPurchases.ToList())
            {
                var card = purchase.Card;

                if (_purchaseSimulationService.ProcessPurchase(purchase, card))
                {
                    _failedPurchases.Remove(purchase);
                }
                else
                {
                    _errorHandlingService.LogError($"Reintento fallido para la compra con tarjeta {card.Id}");
                }
            }
        }

        public async Task RetryFailedPurchasesAsync()
        {
            foreach (var purchase in _failedPurchases.ToList())
            {
                var card = purchase.Card;

                if (await Task.Run(() => _purchaseSimulationService.ProcessPurchase(purchase, card)))
                {
                    _failedPurchases.Remove(purchase);
                }
                else
                {
                    _errorHandlingService.LogError($"Reintento fallido para la compra con tarjeta {card.Id}");
                }
            }
        }

        public void AddFailedPurchase(Purchase purchase)
        {
            _failedPurchases.Add(purchase);
        }
    }
}
