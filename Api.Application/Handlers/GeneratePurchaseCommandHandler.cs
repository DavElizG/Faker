using Api.Application.Commands;
using Api.Application.Exceptions;
using Api.Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Application.Handlers
{
    // Handler para el comando GeneratePurchaseCommand
    public class GeneratePurchaseCommandHandler : IRequestHandler<GeneratePurchaseCommand, Unit>
    {
        private readonly IPurchaseSimulationService _purchaseSimulationService;
        private readonly IErrorHandlingService _errorHandlingService;

        // Constructor que inyecta los servicios necesarios
        public GeneratePurchaseCommandHandler(IPurchaseSimulationService purchaseSimulationService, IErrorHandlingService errorHandlingService)
        {
            _purchaseSimulationService = purchaseSimulationService;
            _errorHandlingService = errorHandlingService;
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
                _errorHandlingService.HandleCardError(ex);
            }
            catch (PurchaseStatusException ex)
            {
                // Maneja las excepciones relacionadas con el estado de la compra
                _errorHandlingService.HandlePurchaseStatusError(ex);
            }
            catch (Exception ex)
            {
                // Maneja cualquier otra excepción
                _errorHandlingService.HandleError(ex);
            }

            // Retorna un valor Unit para indicar que el comando se ha manejado correctamente
            return Unit.Value;
        }
    }
}
