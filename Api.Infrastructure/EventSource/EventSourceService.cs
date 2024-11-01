using Api.Domain.Entities;
using Api.Domain.Interfaces.Infraestructure;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace Api.Infrastructure.EventSource
{
    public class EventSourceService : IEventSource
    {
        private readonly ServiceBusClient _client;
        private readonly ServiceBusSender _sender;
        private readonly ILogger<EventSourceService> _logger;

        public EventSourceService(IConfiguration configuration, ILogger<EventSourceService> logger)
        {
            // Cargar la cadena de conexión y el nombre de la cola desde la configuración
            var connectionString = configuration["AzureServiceBus:ConnectionString"];
            var queueName = configuration["AzureServiceBus:QueueName"];

            _client = new ServiceBusClient(connectionString);
            _sender = _client.CreateSender(queueName);
            _logger = logger;
        }

        public void SendPurchaseEvent(Purchase purchase, bool isSuccess)
        {
            SendPurchaseEventAsync(purchase, isSuccess).GetAwaiter().GetResult();
        }

        public async Task SendPurchaseEventAsync(Purchase purchase, bool isSuccess)
        {
            var eventMessage = new
            {
                PurchaseId = purchase.Id,
                ProductId = purchase.ProductId,
                Amount = purchase.Amount,
                IsSuccess = isSuccess
            };

            var messageContent = JsonSerializer.Serialize(eventMessage);
            ServiceBusMessage message = new ServiceBusMessage(messageContent);

            try
            {
                await _sender.SendMessageAsync(message);
                _logger.LogInformation("Mensaje enviado: {MessageContent}", messageContent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar mensaje: {MessageContent}", messageContent);
            }
        }
    }
}
