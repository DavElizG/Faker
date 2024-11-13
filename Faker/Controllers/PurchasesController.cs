using Api.Domain.Entities;
using Api.Domain.Interfaces;
using Api.Domain.Interfaces.Infraestructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
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
        private readonly IFailedPurchaseStore _failedPurchaseStore;

        public PurchasesController(
            IEventSource eventSource,
            IErrorLogService errorLogService,
            ICardModificationService cardModificationService,
            IPurchaseSimulationService purchaseSimulationService,
            IErrorHandlingService errorHandlingService,
            IPurchaseRetryService purchaseRetryService,
            IFailedPurchaseStore failedPurchaseStore,
            ILogger<PurchasesController> logger)
        {
            _eventSource = eventSource;
            _errorLogService = errorLogService;
            _cardModificationService = cardModificationService;
            _purchaseSimulationService = purchaseSimulationService;
            _errorHandlingService = errorHandlingService;
            _purchaseRetryService = purchaseRetryService;
            _logger = logger;
            _failedPurchaseStore = failedPurchaseStore;
        }

        // Endpoint para ver las compras fallidas
        [HttpGet("failed-purchases")]
        public IActionResult GetFailedPurchases()
        {
            var failedPurchases = _purchaseRetryService.GetAllFailedPurchases();
            if (failedPurchases == null || !failedPurchases.Any())
            {
                _logger.LogInformation("No se encontraron compras fallidas.");
                return NotFound("No se encontraron compras fallidas.");
            }
            else
            {
                _logger.LogInformation($"Se encontraron {failedPurchases.Count} compras fallidas.");
                return Ok(failedPurchases);
            }
        }

        // Endpoint para reintentar una compra fallida por ID
        [HttpPost("retry/{purchaseId}")]
        public async Task<IActionResult> RetryFailedPurchase([FromRoute] Guid purchaseId)
        {
            try
            {
                var result = await _purchaseRetryService.RetryFailedPurchaseByIdAsync(purchaseId);
                if (result)
                {
                    _logger.LogInformation($"Compra {purchaseId} reintentada exitosamente.");
                    return Ok($"Compra {purchaseId} reintentada exitosamente.");
                }
                else
                {
                    _logger.LogWarning($"Reintento fallido para la compra {purchaseId}.");
                    return BadRequest($"Reintento fallido para la compra {purchaseId}.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al reintentar la compra: {ex.Message}");
                return StatusCode(500, $"Error al reintentar la compra: {ex.Message}");
            }
        }

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