using Api.Domain.Entities;
using Api.Domain.Interfaces;
using Api.Domain.Interfaces.Infraestructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Application.Services
{
    public class ErrorHandlingService : IErrorHandlingService
    {
        private readonly IErrorLogService _errorLogService;

        public ErrorHandlingService(IErrorLogService errorLogService)
        {
            _errorLogService = errorLogService;
        }

        public void HandleError(Exception ex)
        {
            // Manejar errores generales
            LogError($"Error General: {ex.Message}");
        }

        public void HandleCardError(Exception ex)
        {
            // Manejar errores relacionados con tarjetas
            LogError($"Error de Tarjeta: {ex.Message}");
        }

        public void HandlePurchaseStatusError(Exception ex)
        {
            // Manejar errores relacionados con el estado de las compras
            LogError($"Error de Estado de Compra: {ex.Message}");
        }

        public void LogError(string message)
        {
            // Registrar el mensaje de error en un archivo o cualquier infraestructura de registro
            // Para simplicidad, estamos registrando en un archivo de texto aquí
            File.AppendAllText("error_log.txt", $"{DateTime.Now}: {message}{Environment.NewLine}");
        }

        public void HandleNoFundsError(Guid cardId)
        {
            // Manejar error de tarjeta sin fondos
            var message = $"Error: La tarjeta con ID {cardId} no tiene fondos suficientes.";
            LogError(message);
            _errorLogService.LogFailedPurchase(new Purchase { /* detalles de la compra fallida */ });
        }

        public void HandleInactiveCardError(Guid cardId)
        {
            // Manejar error de tarjeta inactiva
            var message = $"Error: La tarjeta con ID {cardId} está inactiva.";
            LogError(message);
            _errorLogService.LogFailedPurchase(new Purchase { /* detalles de la compra fallida */ });
        }
    }
}
