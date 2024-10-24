using Api.Domain.Entities;
using Api.Domain.Interfaces.Infraestructure;
using Api.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Faker.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PurchasesController : ControllerBase
    {
        private readonly IEventSource _eventSource;
        private readonly IErrorLogService _errorLogService;
        private readonly ICardModificationService _cardModificationService;

        public PurchasesController(IEventSource eventSource, IErrorLogService errorLogService, ICardModificationService cardModificationService)
        {
            _eventSource = eventSource;
            _errorLogService = errorLogService;
            _cardModificationService = cardModificationService;
        }

        // Endpoint para generar y enviar compras al Event Source
        [HttpPost("generate")]
        public async Task<IActionResult> GeneratePurchase([FromBody] Purchase purchase)
        {
            try
            {
                // Lógica para generar la compra
                // Aquí puedes agregar lógica adicional para validar y procesar la compra

                // Enviar la compra al Event Source
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
                // Aquí puedes agregar lógica adicional para validar y procesar la compra

                // Enviar la compra al Event Source
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
    }
}
