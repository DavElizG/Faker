using Api.Domain.Entities;
using Api.Domain.Enums;
using Api.Domain.Interfaces;
using Api.Domain.Interfaces.Generators;
using Api.Domain.Interfaces.Infraestructure;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bogus;

namespace Api.Application.Services
{
    public class PurchaseSimulationService : IPurchaseSimulationService
    {
        private readonly IFailedPurchaseStore _failedPurchaseStore;
        private readonly List<Purchase> _purchases = new List<Purchase>();
        private readonly List<object> _errorLogs = new List<object>();
        private readonly IErrorHandlingService _errorHandlingService;
        private readonly IEventSource _eventSource;
        private readonly IAffiliateGeneratorService _affiliateGeneratorService;
        private readonly ICardGeneratorService _cardGeneratorService;
        private readonly IProductGeneratorService _productGeneratorService;
        private readonly ILogger<PurchaseSimulationService> _logger;
        private readonly IErrorLogService _errorLogService;

        public PurchaseSimulationService(
            IErrorHandlingService errorHandlingService,
            IEventSource eventSource,
            IAffiliateGeneratorService affiliateGeneratorService,
            ICardGeneratorService cardGeneratorService,
            IProductGeneratorService productGeneratorService,
            ILogger<PurchaseSimulationService> logger,
            IErrorLogService errorLogService,
            IFailedPurchaseStore failedPurchaseStore)
        {
            _errorHandlingService = errorHandlingService;
            _eventSource = eventSource;
            _affiliateGeneratorService = affiliateGeneratorService;
            _cardGeneratorService = cardGeneratorService;
            _productGeneratorService = productGeneratorService;
            _logger = logger;
            _errorLogService = errorLogService;
            _failedPurchaseStore = failedPurchaseStore;
            Console.WriteLine($"PurchaseSimulationService instance created. FailedPurchaseStore hash code: {_failedPurchaseStore.GetHashCode()}");
        }

        public List<object> GetErrorLogs() => _errorLogs;

        public void GeneratePurchases(List<Product> products, List<Affiliate> affiliates, List<Card> cards, int count)
        {
            _logger.LogInformation("Iniciando generación de compras.");

            var faker = new Faker<Purchase>()
                .RuleFor(p => p.Id, f => Guid.NewGuid())
                .RuleFor(p => p.Products, f => f.PickRandom(products, f.Random.Int(1, 5)).ToList())
                .RuleFor(p => p.AffiliateId, (f, p) => p.Products.First().AffiliateId)
                .RuleFor(p => p.Affiliate, (f, p) => p.Products.First().Affiliate)
                .RuleFor(p => p.CardId, f => f.PickRandom(cards).Id)
                .RuleFor(p => p.Card, f => f.PickRandom(cards))
                .RuleFor(p => p.PurchaseDate, f => f.Date.Past(1))
                .RuleFor(p => p.Amount, (f, p) => p.Products.Sum(product => product.Price))
                .RuleFor(p => p.Status, PurchaseStatus.Pending);

            _purchases.AddRange(faker.Generate(count));
            _logger.LogInformation("Se generaron {Count} compras.", count);
        }


        public List<Purchase> GetPurchases() => _purchases;

        public Purchase GeneratePurchase()
        {
            if (_purchases.Count == 0)
            {
                _logger.LogError("No hay compras disponibles. Por favor, genera compras primero.");
                throw new InvalidOperationException("No hay compras disponibles. Por favor, genera compras primero.");
            }

            return _purchases[new Random().Next(_purchases.Count)];
        }

        public async Task SimulatePurchaseAsync()
        {
            _logger.LogInformation("Simulando una compra individual.");
            var purchase = GeneratePurchase();
            var card = _cardGeneratorService.GetCards().FirstOrDefault(c => c.Id == purchase.CardId);

            if (card == null)
            {
                _logger.LogError("No se encontró una tarjeta válida para la compra.");
                throw new InvalidOperationException("No se encontró una tarjeta válida para la compra.");
            }

            await ProcessPurchaseAsync(purchase, card);
        }

        public async Task<bool> ProcessPurchaseAsync(Purchase purchase, Card card)
        {
            bool isSuccess = true;
            string errorMessage = null;
            string errorType = null;
            bool isRetriable = false;

            try
            {
                _logger.LogInformation("Procesando compra {PurchaseId}.", purchase.Id);
                decimal totalPurchaseAmount = purchase.Products.Sum(product => product.Price);
                var random = new Random();

                purchase.Status = PurchaseStatus.Pending;

                // Condiciones de error con probabilidades
                if (card.Funds < totalPurchaseAmount && random.NextDouble() < 0.3)
                {
                    _logger.LogWarning("Fondos insuficientes para la tarjeta {CardId}.", card.Id);
                    _errorHandlingService.HandleNoFundsError(card.Id, purchase);

                    errorMessage = "Fondos insuficientes";
                    errorType = "NoFunds";
                    isSuccess = false;
                }
                else if (card.Status == CardStatus.Inactive && random.NextDouble() < 0.25)
                {
                    _logger.LogWarning("La tarjeta {CardId} está inactiva.", card.Id);
                    _errorHandlingService.HandleInactiveCardError(card.Id, purchase);

                    errorMessage = "La tarjeta está inactiva";
                    errorType = "InactiveCard";
                    isSuccess = false;
                }
                else if (card.ExpiryDate < DateTime.UtcNow && random.NextDouble() < 0.2)
                {
                    _logger.LogWarning("La tarjeta {CardId} ha expirado.", card.Id);
                    _errorHandlingService.HandleCardExpiredError(card.Id, purchase);

                    errorMessage = "La tarjeta ha expirado";
                    errorType = "CardExpired";
                    isSuccess = false;
                }
                else if (totalPurchaseAmount > 1000 && random.NextDouble() < 0.15)
                {
                    _logger.LogWarning("Límite de transacción excedido para la tarjeta {CardId}.", card.Id);
                    _errorHandlingService.HandleTransactionLimitExceededError(card.Id, totalPurchaseAmount, purchase);

                    errorMessage = "Límite de transacción excedido";
                    errorType = "TransactionLimitExceeded";
                    isSuccess = false;
                }
                else if (DetectFraud(card) && random.NextDouble() < 0.1)
                {
                    _logger.LogWarning("Actividad fraudulenta detectada para la tarjeta {CardId}.", card.Id);
                    _errorHandlingService.HandleFraudDetectedError(card.Id, purchase);

                    errorMessage = "Actividad fraudulenta detectada";
                    errorType = "FraudDetected";
                    isSuccess = false;
                }
                else if (random.NextDouble() < 0.05) // Probabilidad de fallo no controlado
                {
                    throw new InvalidOperationException("Fallo no controlado simulado.");
                }

                if (isSuccess)
                {
                    purchase.Status = PurchaseStatus.Completed;
                    card.Funds -= totalPurchaseAmount;

                    // Verificar si estaba en la lista de fallidas y enviarlo como exitoso si es así
                    if (_failedPurchaseStore.GetFailedPurchaseById(purchase.Id) != null)
                    {
                        _failedPurchaseStore.RemoveFailedPurchase(purchase);
                        _logger.LogInformation($"Compra {purchase.Id} procesada exitosamente y eliminada de la lista de fallidas.");
                    }

                    // Enviar el evento de éxito al Service Bus
                    await _eventSource.SendPurchaseEventAsync(purchase, true);
                    _logger.LogInformation("Evento de compra {PurchaseId} enviado al Event Storage con estado IsSuccess = {IsSuccess}.", purchase.Id, isSuccess);
                }
                else
                {
                    purchase.Status = PurchaseStatus.Failed;
                    isRetriable = _errorHandlingService.IsRetriableError(errorType);
                    _logger.LogInformation("Procesando error: {ErrorMessage}, Reintentable: {IsRetriable}", errorMessage, isRetriable);

                    await _errorLogService.LogFailedPurchaseAsync(purchase, errorMessage, isRetriable);

                    if (isRetriable)
                    {
                        if (_failedPurchaseStore.GetFailedPurchaseById(purchase.Id) == null)
                        {
                            _failedPurchaseStore.AddFailedPurchase(purchase);
                            _logger.LogInformation($"Compra fallida añadida al FailedPurchaseStore: {purchase.Id}");
                            _logger.LogInformation($"Total de compras fallidas almacenadas: {_failedPurchaseStore.GetAllFailedPurchases().Count}");
                        }
                        else
                        {
                            _logger.LogInformation($"La compra {purchase.Id} ya está en el FailedPurchaseStore.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar la compra {PurchaseId}.", purchase.Id);
                _errorHandlingService.HandleError(ex, purchase);

                errorMessage = $"Error al procesar la compra: {ex.Message}";
                isRetriable = false;
                await _errorLogService.LogFailedPurchaseAsync(purchase, errorMessage, isRetriable);
                purchase.Status = PurchaseStatus.Failed;
                isSuccess = false;
            }

            return isSuccess;
        }

        public bool ProcessPurchase(Purchase purchase, Card card)
        {
            return ProcessPurchaseAsync(purchase, card).GetAwaiter().GetResult();
        }

        public async Task SimulatePurchases()
        {
            _logger.LogInformation("Iniciando simulación de compras...");

            var affiliates = _affiliateGeneratorService.GetAffiliates();
            if (!affiliates.Any())
            {
                _affiliateGeneratorService.GenerateAffiliates(5);
                affiliates = _affiliateGeneratorService.GetAffiliates();
            }

            var products = _productGeneratorService.GetProducts();
            if (!products.Any())
            {
                foreach (var affiliate in affiliates)
                {
                    _productGeneratorService.GenerateProducts(affiliate.Id, 5);
                }
                products = _productGeneratorService.GetProducts();
            }

            var cards = _cardGeneratorService.GetCards();
            if (!cards.Any())
            {
                _cardGeneratorService.GenerateCards(10);
                cards = _cardGeneratorService.GetCards();
            }

            if (!affiliates.Any() || !products.Any() || !cards.Any())
            {
                _logger.LogError("No se generaron datos suficientes para completar la simulación de compras.");
                throw new InvalidOperationException("Datos insuficientes para la simulación de compras.");
            }

            GeneratePurchases(products, affiliates, cards, 10);
            foreach (var purchase in _purchases)
            {
                var card = cards.FirstOrDefault(c => c.Id == purchase.CardId);
                if (card != null)
                {
                    await ProcessPurchaseAsync(purchase, card);
                    await Task.Delay(TimeSpan.FromSeconds(10)); 
                }
            }

            _logger.LogInformation("Simulación de compras completada.");
        }

        private bool DetectFraud(Card card)
        {
            if (card.Brand == "FraudulentBrand")
            {
                return true;
            }

            if (card.Status == CardStatus.Inactive)
            {
                return true;
            }

            var recentPurchases = _purchases.Where(p => p.CardId == card.Id && (DateTime.UtcNow - p.PurchaseDate).TotalMinutes < 30).ToList();
            if (recentPurchases.Count > 5)
            {
                return true;
            }

            var distinctLocations = recentPurchases
                .SelectMany(p => p.Products.Select(product => product.Affiliate.Address))
                .Distinct()
                .Count();
            if (distinctLocations > 3)
            {
                return true;
            }

            return false;
        }

        public void AddPurchase(Purchase purchase)
        {
            _purchases.Add(purchase);
        }

        public async Task<Purchase> GetPurchaseByIdAsync(Guid purchaseId)
        {
            return await Task.FromResult(_purchases.FirstOrDefault(p => p.Id == purchaseId));
        }
    }
}
