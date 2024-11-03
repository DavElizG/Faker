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
        private readonly List<Purchase> _purchases = new List<Purchase>();
        private readonly IErrorHandlingService _errorHandlingService;
        private readonly IEventSource _eventSource;
        private readonly IAffiliateGeneratorService _affiliateGeneratorService;
        private readonly ICardGeneratorService _cardGeneratorService;
        private readonly IProductGeneratorService _productGeneratorService;
        private readonly ILogger<PurchaseSimulationService> _logger;

        public PurchaseSimulationService(
            IErrorHandlingService errorHandlingService,
            IEventSource eventSource,
            IAffiliateGeneratorService affiliateGeneratorService,
            ICardGeneratorService cardGeneratorService,
            IProductGeneratorService productGeneratorService,
            ILogger<PurchaseSimulationService> logger)
        {
            _errorHandlingService = errorHandlingService;
            _eventSource = eventSource;
            _affiliateGeneratorService = affiliateGeneratorService;
            _cardGeneratorService = cardGeneratorService;
            _productGeneratorService = productGeneratorService;
            _logger = logger;
        }

        public void GeneratePurchases(List<Product> products, List<Affiliate> affiliates, List<Card> cards, int count)
        {
            _logger.LogInformation("Iniciando generación de compras.");

            var faker = new Faker<Purchase>()
                .RuleFor(p => p.Id, f => Guid.NewGuid())
                .RuleFor(p => p.ProductId, f => f.PickRandom(products).Id)
                .RuleFor(p => p.Product, f => f.PickRandom(products))
                .RuleFor(p => p.AffiliateId, (f, p) => p.Product.AffiliateId)
                .RuleFor(p => p.Affiliate, (f, p) => p.Product.Affiliate)
                .RuleFor(p => p.CardId, f => f.PickRandom(cards).Id)
                .RuleFor(p => p.Card, f => f.PickRandom(cards))
                .RuleFor(p => p.PurchaseDate, f => f.Date.Past(1))
                .RuleFor(p => p.Amount, (f, p) => p.Product.Price)
                .RuleFor(p => p.Status, f => f.PickRandom<PurchaseStatus>());

            _purchases.AddRange(faker.Generate(count));
            _logger.LogInformation("Se generaron {Count} compras.", count);
        }

        public List<Purchase> GetPurchases() => _purchases;

        public Purchase GeneratePurchase()
        {
            if (_purchases.Count == 0)
            {
                _logger.LogError("No hay compras disponibles. Por favor, genera compras primero.");
                throw new InvalidOperationException("No purchases available. Please generate purchases first.");
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
                throw new InvalidOperationException("No card available for the purchase.");
            }

            await ProcessPurchaseAsync(purchase, card);
        }

        public async Task<bool> ProcessPurchaseAsync(Purchase purchase, Card card)
        {
            bool isSuccess = true;


            try
            {
                _logger.LogInformation("Procesando compra {PurchaseId}.", purchase.Id);
                decimal totalPurchaseAmount = purchase.Product.Price;

                // Validación de fondos insuficientes
                if (card.Funds < totalPurchaseAmount)
                {
                    _logger.LogWarning("Fondos insuficientes para la tarjeta {CardId}.", card.Id);
                    _errorHandlingService.HandleNoFundsError(card.Id);
                    isSuccess = false; // Marca la compra como fallida
                }

                // Validación de tarjeta inactiva
                if (card.Status == CardStatus.Inactive)
                {
                    _logger.LogWarning("La tarjeta {CardId} está inactiva.", card.Id);
                    _errorHandlingService.HandleInactiveCardError(card.Id);
                    isSuccess = false; // Marca la compra como fallida
                }

                if (isSuccess)
                {
                    // Si la compra es exitosa, deduce los fondos
                    card.Funds -= totalPurchaseAmount;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar la compra {PurchaseId}.", purchase.Id);
                _errorHandlingService.HandleError(ex);
                isSuccess = false; // En caso de error inesperado, marca la compra como fallida
            }

            // Envía el evento con todos los datos de la compra, indicando si fue exitosa o no
            await _eventSource.SendPurchaseEventAsync(purchase, isSuccess);
            _logger.LogInformation("Evento de compra {PurchaseId} enviado con estado IsSuccess = {IsSuccess}.", purchase.Id, isSuccess);

            return isSuccess;
        }
        public bool ProcessPurchase(Purchase purchase, Card card)
        {
            return ProcessPurchaseAsync(purchase, card).GetAwaiter().GetResult();
        }

        public async Task SimulatePurchases()
        {
            _logger.LogInformation("Iniciando simulación de compras...");

            // Generar afiliados si no existen
            var affiliates = _affiliateGeneratorService.GetAffiliates();
            if (!affiliates.Any())
            {
                _affiliateGeneratorService.GenerateAffiliates(5);  // Genera 5 afiliados
                affiliates = _affiliateGeneratorService.GetAffiliates();
            }

            // Generar productos si no existen
            var products = _productGeneratorService.GetProducts();
            if (!products.Any())
            {
                foreach (var affiliate in affiliates)
                {
                    _productGeneratorService.GenerateProducts(affiliate.Id, 5); // Genera 5 productos por afiliado
                }
                products = _productGeneratorService.GetProducts();
            }

            // Generar tarjetas si no existen
            var cards = _cardGeneratorService.GetCards();
            if (!cards.Any())
            {
                _cardGeneratorService.GenerateCards(10); // Genera 10 tarjetas
                cards = _cardGeneratorService.GetCards();
            }

            // Verificar que todos los datos necesarios estén disponibles
            if (!affiliates.Any() || !products.Any() || !cards.Any())
            {
                _logger.LogError("No se generaron datos suficientes para completar la simulación de compras.");
                throw new InvalidOperationException("Datos insuficientes para la simulación de compras.");
            }

            _logger.LogInformation("Datos generados: {AffiliatesCount} afiliados, {ProductsCount} productos, {CardsCount} tarjetas.",
                                    affiliates.Count, products.Count, cards.Count);

            // Generar y procesar compras
            GeneratePurchases(products, affiliates, cards, 10);
            _logger.LogInformation("Compras generadas. Simulando una compra...");

            await SimulatePurchaseAsync();
            _logger.LogInformation("Simulación de compras completada.");
        }
    }
}