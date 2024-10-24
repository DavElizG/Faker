using Api.Domain.Entities;
using Api.Domain.Interfaces.Infraestructure;
using Azure.Messaging.ServiceBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Api.Infrastructure.EventSource
{
    public class EventSourceService : IEventSource
    {
        private const string ConnectionString = "Cadena-de-Conexion";
        private const string QueueName = "dev-event-source-1";

        private readonly ServiceBusClient _client;
        private readonly ServiceBusSender _sender;

        public EventSourceService()
        {
            _client = new ServiceBusClient(ConnectionString);
            _sender = _client.CreateSender(QueueName);
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
                Console.WriteLine($"Mensaje enviado: {messageContent}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al enviar mensaje: {ex.Message}");
            }
        }
    }
}
