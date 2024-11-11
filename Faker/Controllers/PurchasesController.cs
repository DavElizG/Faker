using Api.Domain.Entities;
using Api.Domain.Interfaces.Infraestructure;
using Api.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Faker.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PurchasesController : ControllerBase
    {
        private readonly IEventSource _eventSource;
        private readonly IErrorLogService _errorLogService;
        private readonly ICardModificationService _cardModificationService;
        private readonly IPurchaseSimulationService _purchaseSimulationService;
        private readonly IErrorHandlingService _errorHandlingService;
        private readonly IPurchaseRetryService _purchaseRetryService;
        private readonly ILogger<PurchasesController> _logger;

        public PurchasesController(
            IEventSource eventSource,
            IErrorLogService errorLogService,
            ICardModificationService cardModificationService,
            IPurchaseSimulationService purchaseSimulationService,
            IErrorHandlingService errorHandlingService,
            IPurchaseRetryService purchaseRetryService,
            ILogger<PurchasesController> logger) // Agregar ILogger aquí
        {
            _eventSource = eventSource;
            _errorLogService = errorLogService;
            _cardModificationService = cardModificationService;
            _purchaseSimulationService = purchaseSimulationService;
            _errorHandlingService = errorHandlingService;
            _purchaseRetryService = purchaseRetryService;
            _logger = logger; // Asignar al campo _logger
        }

        // Endpoint para ver las compras fallidas
        [HttpGet("failed-purchases")]
        public IActionResult GetFailedPurchases()
        {
            var failedPurchases = _purchaseRetryService.GetAllFailedPurchases();
            if (failedPurchases == null || !failedPurchases.Any())
            {
                _logger.LogInformation("No se encontraron compras fallidas.");
            }
            else
            {
                _logger.LogInformation($"Se encontraron {failedPurchases.Count} compras fallidas.");
            }
            return Ok(failedPurchases);
        }


        // Endpoint para generar y enviar compras al Event Source
        [HttpPost("generate")]
        public async Task<IActionResult> GeneratePurchase([FromBody] Purchase purchase)
        {
            try
            {
                await _eventSource.SendPurchaseEventAsync(purchase, true);
                return Ok("Compra generada y enviada al Event Source.");
            }
            catch (Exception ex)
            {
                bool isRetriable = _errorHandlingService.IsRetriableError("GeneralError");
                await _errorLogService.LogFailedPurchaseAsync(purchase, $"Error al generar la compra: {ex.Message}", isRetriable);
                return StatusCode(500, $"Error al generar la compra: {ex.Message}");
            }
        }

        // Endpoint para retry de compras fallidas
        [HttpPost("retry")]
        public async Task<IActionResult> RetryPurchase([FromBody] Guid purchaseId)
        {
            try
            {
                await _purchaseRetryService.RetryFailedPurchaseByIdAsync(purchaseId);
                return Ok("Compra reintentada y procesada exitosamente.");
            }
            catch (Exception ex)
            {
                await _errorLogService.LogFailedPurchaseAsync(new Purchase { Id = purchaseId }, $"Error al reintentar la compra: {ex.Message}", false);
                return StatusCode(500, $"Error al reintentar la compra: {ex.Message}");
            }
        }

        // Endpoint para generar múltiples compras simuladas
        [HttpPost("generate-purchases")]
        public async Task<IActionResult> GeneratePurchases()
        {
            try
            {
                await _purchaseSimulationService.SimulatePurchases();
                return Ok("Simulación de compras iniciada exitosamente.");
            }
            catch (Exception ex)
            {
                bool isRetriable = _errorHandlingService.IsRetriableError("GeneratePurchasesError");
                await _errorLogService.LogFailedPurchaseAsync(new Purchase(), $"Ocurrió un error al generar las compras: {ex.Message}", isRetriable);
                return StatusCode(500, $"Ocurrió un error al generar las compras: {ex.Message}");
            }
        }

        // Endpoint para editar una tarjeta
        [HttpPut("edit-card")]
        public async Task<IActionResult> EditCard([FromBody] Card card)
        {
            try
            {
                await _cardModificationService.ModifyCardAsync(card);
                return Ok("Tarjeta modificada exitosamente.");
            }
            catch (Exception ex)
            {
                bool isRetriable = _errorHandlingService.IsRetriableError("EditCardError");
                await _errorLogService.LogFailedPurchaseAsync(new Purchase(), $"Error al modificar la tarjeta: {ex.Message}", isRetriable);
                return StatusCode(500, $"Error al modificar la tarjeta: {ex.Message}");
            }
        }
    }
}
