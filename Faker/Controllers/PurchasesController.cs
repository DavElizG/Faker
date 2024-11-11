using Api.Domain.Entities;
using Api.Domain.Interfaces;
using Api.Domain.Interfaces.Infraestructure;
using Microsoft.AspNetCore.Mvc;
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

        public PurchasesController(
            IEventSource eventSource,
            IErrorLogService errorLogService,
            ICardModificationService cardModificationService,
            IPurchaseSimulationService purchaseSimulationService)
        {
            _eventSource = eventSource;
            _errorLogService = errorLogService;
            _cardModificationService = cardModificationService;
            _purchaseSimulationService = purchaseSimulationService;
        }

        // Endpoint para generar y enviar compras al Event Source
        [HttpPost("generate")]
        public async Task<IActionResult> GeneratePurchase([FromBody] Purchase purchase)
        {
            try
            {
                // Lógica para generar la compra
                await _eventSource.SendPurchaseEventAsync(purchase, true);
                return Ok("Compra generada y enviada al Event Source.");
            }
            catch (Exception ex)
            {
                // En caso de error, registrar la compra fallida
                await _errorLogService.LogFailedPurchaseAsync(purchase);
                return StatusCode(500, $"Error al generar la compra: {ex.Message}");
            }
        }

        // Endpoint para retry de compras fallidas
        [HttpPost("retry")]
        public async Task<IActionResult> RetryPurchase([FromBody] Purchase purchase)
        {
            try
            {
                // Lógica para reintentar la compra
                await _eventSource.SendPurchaseEventAsync(purchase, true);
                return Ok("Retry de compra exitoso y enviado al Event Source.");
            }
            catch (Exception ex)
            {
                // En caso de error, registrar la compra fallida
                await _errorLogService.LogFailedPurchaseAsync(purchase);
                return StatusCode(500, $"Error al reintentar la compra: {ex.Message}");
            }
        }

        // Endpoint para iniciar la simulación y generación de compras
        [HttpPost("generate-purchases")]
        public async Task<IActionResult> GeneratePurchases()
        {
            try
            {
                // Inicia la simulación y envío de compras manualmente
                await _purchaseSimulationService.SimulatePurchases();
                return Ok("Simulación de compras iniciada exitosamente.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ocurrió un error al generar las compras: {ex.Message}");
            }
        }

        // Endpoint para editar una tarjeta
        [HttpPut("edit-card")]
        public async Task<IActionResult> EditCard([FromBody] Card card)
        {
            try
            {
                // Lógica para modificar la tarjeta
                await _cardModificationService.ModifyCardAsync(card);
                return Ok("Tarjeta modificada exitosamente.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al modificar la tarjeta: {ex.Message}");
            }
        }

        // Endpoint para obtener todos los errores registrados en el proceso de simulación de compras
        [HttpGet("all-errors")]
        public IActionResult GetAllErrors()
        {
            var errors = _purchaseSimulationService.GetErrorLogs();
            return Ok(errors);
        }
    }
}
