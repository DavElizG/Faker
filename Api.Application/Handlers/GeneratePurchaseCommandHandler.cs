using Api.Application.Commands;
using Api.Application.Exceptions;
using Api.Domain.Entities;
using Api.Domain.Interfaces;
using Api.Domain.Interfaces.Infraestructure;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Application.Handlers
{
    // Handler para el comando GeneratePurchaseCommand
    public class GeneratePurchaseCommandHandler : IRequestHandler<GeneratePurchaseCommand, Unit>
    {
        private readonly IPurchaseSimulationService _purchaseSimulationService;
        private readonly IErrorHandlingService _errorHandlingService;
        private readonly IErrorLogService _errorLogService;

        // Constructor que inyecta los servicios necesarios
        public GeneratePurchaseCommandHandler(
            IPurchaseSimulationService purchaseSimulationService,
            IErrorHandlingService errorHandlingService,
            IErrorLogService errorLogService)
        {
            _purchaseSimulationService = purchaseSimulationService;
            _errorHandlingService = errorHandlingService;
            _errorLogService = errorLogService;
        }

        // Método que maneja el comando GeneratePurchaseCommand
        public async Task<Unit> Handle(GeneratePurchaseCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Genera las compras utilizando los datos proporcionados en el comando
                _purchaseSimulationService.GeneratePurchases(request.Products, request.Affiliates, request.Cards, request.Count);

                // Simula una compra asincrónicamente
                await _purchaseSimulationService.SimulatePurchaseAsync();
            }
            catch (CardException ex)
            {
                // Maneja las excepciones relacionadas con la tarjeta
                var purchase = new Purchase(); // Crear o obtener la compra relevante
                _errorHandlingService.HandleCardError(ex, purchase);

                // Determina si es reintentable y registra el error
                bool isRetriable = _errorHandlingService.IsRetriableError("CardError");
                await _errorLogService.LogFailedPurchaseAsync(purchase, ex.Message, isRetriable);
            }
            catch (PurchaseStatusException ex)
            {
                // Maneja las excepciones relacionadas con el estado de la compra
                var purchase = new Purchase(); // Crear o obtener la compra relevante
                _errorHandlingService.HandlePurchaseStatusError(ex, purchase);

                // Determina si es reintentable y registra el error
                bool isRetriable = _errorHandlingService.IsRetriableError("PurchaseStatusError");
                await _errorLogService.LogFailedPurchaseAsync(purchase, ex.Message, isRetriable);
            }
            catch (Exception ex)
            {
                // Maneja cualquier otra excepción
                var purchase = new Purchase(); // Crear o obtener la compra relevante
                _errorHandlingService.HandleError(ex, purchase);

                // Determina si es reintentable y registra el error como no reintentable
                bool isRetriable = false;
                await _errorLogService.LogFailedPurchaseAsync(purchase, $"Error general: {ex.Message}", isRetriable);
            }

            // Retorna un valor Unit para indicar que el comando se ha manejado correctamente
            return Unit.Value;
        }
    }
}
