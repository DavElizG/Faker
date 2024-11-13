using Api.Domain.Entities;
using Api.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class PurchaseRetryService : IPurchaseRetryService
{
    private readonly IFailedPurchaseStore _failedPurchaseStore;
    private readonly IPurchaseSimulationService _purchaseSimulationService;
    private readonly IErrorHandlingService _errorHandlingService;
    private readonly ILogger<PurchaseRetryService> _logger;

    public PurchaseRetryService(
        IPurchaseSimulationService purchaseSimulationService,
        IErrorHandlingService errorHandlingService,
        IFailedPurchaseStore failedPurchaseStore,
        ILogger<PurchaseRetryService> logger)
    {
        _purchaseSimulationService = purchaseSimulationService;
        _errorHandlingService = errorHandlingService;
        _failedPurchaseStore = failedPurchaseStore;
        _logger = logger;
        Console.WriteLine($"PurchaseRetryService instance created. FailedPurchaseStore hash code: {_failedPurchaseStore.GetHashCode()}");

    }

    public void RetryFailedPurchases()
    {
        foreach (var purchase in _failedPurchaseStore.GetAllFailedPurchases().ToList())
        {
            if (_errorHandlingService.IsRetriableError("GeneralError"))
            {
                if (purchase.Card == null)
                {
                    _logger.LogError($"Error: La tarjeta para la compra {purchase.Id} es null. No se puede procesar.");
                    continue;
                }

                var result = _purchaseSimulationService.ProcessPurchaseAsync(purchase, purchase.Card).GetAwaiter().GetResult();

                if (result)
                {
                    _failedPurchaseStore.RemoveFailedPurchase(purchase);
                    _logger.LogInformation($"Compra {purchase.Id} procesada exitosamente y eliminada de la lista de fallidas.");
                }
                else
                {
                    _logger.LogWarning($"Reintento fallido para la compra {purchase.Id}.");
                }
            }
        }
    }

    public async Task RetryFailedPurchasesAsync()
    {
        foreach (var purchase in _failedPurchaseStore.GetAllFailedPurchases().ToList())
        {
            if (_errorHandlingService.IsRetriableError("GeneralError"))
            {
                if (purchase.Card == null)
                {
                    _logger.LogError($"Error: La tarjeta para la compra {purchase.Id} es null. No se puede procesar.");
                    continue;
                }

                var result = await _purchaseSimulationService.ProcessPurchaseAsync(purchase, purchase.Card);

                if (result)
                {
                    _failedPurchaseStore.RemoveFailedPurchase(purchase);
                    _logger.LogInformation($"Compra {purchase.Id} procesada exitosamente y eliminada de la lista de fallidas.");
                }
                else
                {
                    _logger.LogWarning($"Reintento fallido para la compra {purchase.Id}.");
                }
            }
        }
    }

    public void AddFailedPurchase(Purchase purchase)
    {
        _failedPurchaseStore.AddFailedPurchase(purchase);
        _logger.LogInformation($"Compra fallida añadida: {purchase.Id}");
    }

    public async Task<bool> RetryFailedPurchaseByIdAsync(Guid purchaseId)
    {
        _logger.LogInformation($"Intentando reintentar la compra con ID: {purchaseId}");
        var purchase = _failedPurchaseStore.GetFailedPurchaseById(purchaseId);
        if (purchase != null && _errorHandlingService.IsRetriableError("GeneralError"))
        {
            if (purchase.Card == null)
            {
                _logger.LogError($"Error: La tarjeta para la compra {purchase.Id} es null. No se puede procesar.");
                return false;
            }

            var isSuccess = await _purchaseSimulationService.ProcessPurchaseAsync(purchase, purchase.Card);
            if (isSuccess)
            {
                _failedPurchaseStore.RemoveFailedPurchase(purchase);
                _logger.LogInformation($"Compra {purchase.Id} procesada exitosamente y eliminada de la lista de fallidas.");
            }
            return isSuccess;
        }
        _logger.LogError($"No se encontró la compra con ID: {purchaseId}");
        throw new Exception($"No se encontró la compra con ID: {purchaseId}");
    }

    public List<Purchase> GetAllFailedPurchases()
    {
        return _failedPurchaseStore.GetAllFailedPurchases();
    }

    public void PrintFailedPurchases()
    {
        foreach (var purchase in _failedPurchaseStore.GetAllFailedPurchases())
        {
            _logger.LogInformation($"Compra fallida almacenada: {purchase.Id}");
        }
    }
}