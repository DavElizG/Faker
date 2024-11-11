using Api.Domain.Entities;
using Api.Domain.Enums;
using Api.Domain.Interfaces.Infraestructure;
using Api.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Application.Services
{
    public class PurchaseProcessorService : IPurchaseProcessorService
    {
        private readonly IErrorHandlingService _errorHandlingService;
        private readonly IEventSource _eventSource;
        private readonly ILogger<PurchaseProcessorService> _logger;
        private readonly List<Purchase> _failedPurchases;  // Lista de compras fallidas en memoria

        public PurchaseProcessorService(
            IErrorHandlingService errorHandlingService,
            IEventSource eventSource,
            ILogger<PurchaseProcessorService> logger)
        {
            _errorHandlingService = errorHandlingService;
            _eventSource = eventSource;
            _logger = logger;
            _failedPurchases = new List<Purchase>();
        }

        public async Task<bool> ProcessPurchaseAsync(Purchase purchase, Card card)
        {
            bool isSuccess = true;

            try
            {
                _logger.LogInformation("Procesando compra {PurchaseId}.", purchase.Id);
                decimal totalPurchaseAmount = purchase.Product.Price;

                if (card.Funds < totalPurchaseAmount)
                {
                    _errorHandlingService.HandleNoFundsError(card.Id, purchase);
                    purchase.Status = PurchaseStatus.Failed;
                    isSuccess = false;

                    // Agregar compra fallida a la lista en memoria
                    AddFailedPurchase(purchase);
                }
                else
                {
                    purchase.Status = PurchaseStatus.Completed;
                    card.Funds -= totalPurchaseAmount;
                    await _eventSource.SendPurchaseEventAsync(purchase, isSuccess);
                    _logger.LogInformation("Compra {PurchaseId} procesada exitosamente.", purchase.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar la compra {PurchaseId}.", purchase.Id);
                _errorHandlingService.HandleError(ex, purchase);
                isSuccess = false;

                // Agregar compra fallida a la lista en memoria en caso de excepción
                AddFailedPurchase(purchase);
            }

            return isSuccess;
        }

        // Método para agregar compras fallidas a la lista y evitar duplicados
        private void AddFailedPurchase(Purchase purchase)
        {
            if (!_failedPurchases.Any(p => p.Id == purchase.Id))
            {
                _failedPurchases.Add(purchase);
                _logger.LogInformation("Compra fallida agregada a la lista: {PurchaseId}", purchase.Id);
            }
            else
            {
                _logger.LogInformation("Compra fallida ya existe en la lista: {PurchaseId}", purchase.Id);
            }
        }

        // Método de diagnóstico para imprimir todas las compras fallidas almacenadas
        public void PrintFailedPurchases()
        {
            _logger.LogInformation("Listando todas las compras fallidas almacenadas:");
            foreach (var failedPurchase in _failedPurchases)
            {
                _logger.LogInformation("Compra fallida: {PurchaseId}", failedPurchase.Id);
            }
        }

        // Método para reintentar todas las compras fallidas
        public async Task RetryAllFailedPurchasesAsync()
        {
            foreach (var purchase in _failedPurchases.ToList())  // ToList para evitar modificaciones durante la iteración
            {
                var card = purchase.Card;  // Supongamos que la tarjeta está referenciada en el objeto Purchase

                bool isSuccess = await ProcessPurchaseAsync(purchase, card);

                // Si se procesa exitosamente en el reintento, eliminar de la lista
                if (isSuccess)
                {
                    _failedPurchases.Remove(purchase);
                    _logger.LogInformation("Compra fallida procesada exitosamente y removida de la lista: {PurchaseId}", purchase.Id);
                }
                else
                {
                    _logger.LogWarning("Reintento fallido para la compra: {PurchaseId}", purchase.Id);
                }
            }
        }
    }
}
