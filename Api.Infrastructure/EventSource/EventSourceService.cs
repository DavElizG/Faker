using Api.Domain.Entities;
using Api.Domain.Interfaces.Infraestructure;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
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
            // Crear el mensaje completo con todos los datos necesarios
            var eventMessage = new
            {
                Id = purchase.Id,
                ProductId = purchase.ProductId,
                Product = purchase.Product == null ? null : new
                {
                    purchase.Product.Id,
                    purchase.Product.Name,
                    purchase.Product.Description,
                    purchase.Product.Price,
                    purchase.Product.Category,
                    purchase.Product.Stock,
                    purchase.Product.AffiliateId,
                    Affiliate = purchase.Product.Affiliate == null ? null : new
                    {
                        purchase.Product.Affiliate.Id,
                        purchase.Product.Affiliate.Name,
                        purchase.Product.Affiliate.Address,
                        purchase.Product.Affiliate.PhoneNumber,
                        purchase.Product.Affiliate.Email,
                        purchase.Product.Affiliate.Website
                    }
                },
                AffiliateId = purchase.AffiliateId,
                Affiliate = purchase.Affiliate == null ? null : new
                {
                    purchase.Affiliate.Id,
                    purchase.Affiliate.Name,
                    purchase.Affiliate.Address,
                    purchase.Affiliate.PhoneNumber,
                    purchase.Affiliate.Email,
                    purchase.Affiliate.Website
                },
                CardId = purchase.CardId,
                Card = purchase.Card == null ? null : new
                {
                    purchase.Card.Id,
                    purchase.Card.Name,
                    purchase.Card.Email,
                    purchase.Card.CreditCardNumber,
                    purchase.Card.ExpiryDate,
                    purchase.Card.Funds,
                    purchase.Card.Brand,
                    purchase.Card.CVV,
                    purchase.Card.Status
                },
                PurchaseDate = purchase.PurchaseDate,
                Amount = purchase.Amount,
                Status = purchase.Status,
                IsSuccess = isSuccess // Indica si la compra fue exitosa o no
            };

            // Serializar el mensaje completo a JSON, ignorando propiedades nulas
            var messageContent = JsonSerializer.Serialize(eventMessage, new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                WriteIndented = false // Cambia a true si prefieres una salida más legible
            });

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




