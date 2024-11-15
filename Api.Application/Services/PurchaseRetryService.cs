using Api.Domain.Entities;
using Api.Domain.Interfaces;
using Api.Domain.Interfaces.Infraestructure;
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
    private readonly IEventSource _eventSource;
    private readonly ILogger<PurchaseRetryService> _logger;

    public PurchaseRetryService(
        IPurchaseSimulationService purchaseSimulationService,
        IErrorHandlingService errorHandlingService,
        IFailedPurchaseStore failedPurchaseStore,
        IEventSource eventSource,
        ILogger<PurchaseRetryService> logger)
    {
        _purchaseSimulationService = purchaseSimulationService;
        _errorHandlingService = errorHandlingService;
        _failedPurchaseStore = failedPurchaseStore;
        _eventSource = eventSource;
        _logger = logger;
        Console.WriteLine($"PurchaseRetryService instance created. FailedPurchaseStore hash code: {_failedPurchaseStore.GetHashCode()}");
    }

    public void RetryFailedPurchases()
    {
        foreach (var purchase in _failedPurchaseStore.GetAllFailedPurchases().ToList())
        {
            RetryPurchase(purchase).GetAwaiter().GetResult();
        }
    }

    public async Task RetryFailedPurchasesAsync()
    {
        foreach (var purchase in _failedPurchaseStore.GetAllFailedPurchases().ToList())
        {
            await RetryPurchase(purchase);
        }
    }

    private async Task RetryPurchase(Purchase purchase)
    {
        if (purchase.Card == null)
        {
            _logger.LogError($"Error: La tarjeta para la compra {purchase.Id} es null. No se puede procesar.");
            return;
        }

        var isSuccess = await _purchaseSimulationService.ProcessPurchaseAsync(purchase, purchase.Card);

        if (isSuccess)
        {
            // Envía el evento de éxito al Service Bus
            await _eventSource.SendPurchaseEventAsync(purchase, true);
            _logger.LogInformation($"Evento de compra {purchase.Id} enviado al Service Bus como procesada exitosamente.");

            // Elimina la compra de la lista de fallidas
            _failedPurchaseStore.RemoveFailedPurchase(purchase);
            _logger.LogInformation($"Compra {purchase.Id} procesada exitosamente y eliminada de la lista de fallidas.");
        }
        else
        {
            _logger.LogWarning($"Reintento fallido para la compra {purchase.Id}.");
        }
    }

    public async Task<bool> RetryFailedPurchaseByIdAsync(Guid purchaseId)
    {
        _logger.LogInformation($"Intentando reintentar la compra con ID: {purchaseId}");
        var purchase = _failedPurchaseStore.GetFailedPurchaseById(purchaseId);

        if (purchase == null)
        {
            _logger.LogError($"No se encontró la compra con ID: {purchaseId}");
            throw new Exception($"No se encontró la compra con ID: {purchaseId}");
        }

        if (purchase.Card == null)
        {
            _logger.LogError($"Error: La tarjeta para la compra {purchase.Id} es null. No se puede procesar.");
            return false;
        }

        var isSuccess = await _purchaseSimulationService.ProcessPurchaseAsync(purchase, purchase.Card);
        if (isSuccess)
        {
            await _eventSource.SendPurchaseEventAsync(purchase, true);
            _failedPurchaseStore.RemoveFailedPurchase(purchase);
            _logger.LogInformation($"Compra {purchase.Id} procesada exitosamente, enviada al Service Bus, y eliminada de la lista de fallidas.");
        }
        else
        {
            _logger.LogWarning($"Reintento fallido para la compra {purchase.Id}.");
        }

        return isSuccess;
    }

    public void AddFailedPurchase(Purchase purchase)
    {
        _failedPurchaseStore.AddFailedPurchase(purchase);
        _logger.LogInformation($"Compra fallida añadida: {purchase.Id}");
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
