using Api.Domain.Entities;
using Api.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class PurchaseRetryService : IPurchaseRetryService
{
    private readonly IPurchaseProcessorService _purchaseProcessorService;
    private readonly IErrorHandlingService _errorHandlingService;
    private readonly List<Purchase> _failedPurchases;

    public PurchaseRetryService(IPurchaseProcessorService purchaseProcessorService, IErrorHandlingService errorHandlingService)
    {
        _purchaseProcessorService = purchaseProcessorService;
        _errorHandlingService = errorHandlingService;
        _failedPurchases = new List<Purchase>();
    }

    public void RetryFailedPurchases()
    {
        foreach (var purchase in _failedPurchases.ToList())  // Crear una copia para evitar problemas al modificar la lista
        {
            if (_errorHandlingService.IsRetriableError("GeneralError"))
            {
                if (purchase.Card == null)
                {
                    Console.WriteLine($"Error: La tarjeta para la compra {purchase.Id} es null. No se puede procesar.");
                    continue;
                }

                var result = _purchaseProcessorService.ProcessPurchaseAsync(purchase, purchase.Card).GetAwaiter().GetResult();

                if (result)
                {
                    _failedPurchases.Remove(purchase);  // Si se procesa con éxito, quitar de la lista
                    Console.WriteLine($"Compra {purchase.Id} procesada exitosamente y eliminada de la lista de fallidas.");
                }
                else
                {
                    Console.WriteLine($"Reintento fallido para la compra {purchase.Id}.");
                }
            }
        }
    }

    public async Task RetryFailedPurchasesAsync()
    {
        foreach (var purchase in _failedPurchases.ToList())
        {
            if (_errorHandlingService.IsRetriableError("GeneralError"))
            {
                if (purchase.Card == null)
                {
                    Console.WriteLine($"Error: La tarjeta para la compra {purchase.Id} es null. No se puede procesar.");
                    continue;
                }

                var result = await _purchaseProcessorService.ProcessPurchaseAsync(purchase, purchase.Card);

                if (result)
                {
                    _failedPurchases.Remove(purchase);  // Si se procesa con éxito, quitar de la lista
                    Console.WriteLine($"Compra {purchase.Id} procesada exitosamente y eliminada de la lista de fallidas.");
                }
                else
                {
                    Console.WriteLine($"Reintento fallido para la compra {purchase.Id}.");
                }
            }
        }
    }

    public void AddFailedPurchase(Purchase purchase)
    {
        if (!_failedPurchases.Any(p => p.Id == purchase.Id))
        {
            _failedPurchases.Add(purchase);
            Console.WriteLine($"Compra fallida añadida: {purchase.Id}");
        }
        else
        {
            Console.WriteLine($"La compra {purchase.Id} ya está en la lista de fallidas.");
        }
    }

    public async Task<bool> RetryFailedPurchaseByIdAsync(Guid purchaseId)
    {
        var purchase = _failedPurchases.FirstOrDefault(p => p.Id == purchaseId);
        if (purchase != null && _errorHandlingService.IsRetriableError("GeneralError"))
        {
            if (purchase.Card == null)
            {
                Console.WriteLine($"Error: La tarjeta para la compra {purchase.Id} es null. No se puede procesar.");
                return false;
            }

            var isSuccess = await _purchaseProcessorService.ProcessPurchaseAsync(purchase, purchase.Card);
            if (isSuccess)
            {
                _failedPurchases.Remove(purchase);  // Remover si el reintento fue exitoso
                Console.WriteLine($"Compra {purchase.Id} procesada exitosamente y eliminada de la lista de fallidas.");
            }
            return isSuccess;
        }
        throw new Exception($"No se encontró la compra con ID: {purchaseId}");
    }

    public List<Purchase> GetAllFailedPurchases()
    {
        return _failedPurchases;
    }

    public void PrintFailedPurchases()
    {
        foreach (var purchase in _failedPurchases)
        {
            Console.WriteLine($"Compra fallida almacenada: {purchase.Id}");
        }
    }
}
