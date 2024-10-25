using Api.Application.Services.FakeDataGenerators;
using Api.Domain.Entities;
using Api.Domain.Enums;
using Api.Domain.Interfaces;
using Api.Domain.Interfaces.Generators;
using Api.Domain.Interfaces.Infraestructure;
using Bogus;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Application.Services
{
    public class PurchaseSimulationService : IPurchaseSimulationService
    {
        private readonly List<Purchase> _purchases = new List<Purchase>();
        private readonly IErrorHandlingService _errorHandlingService;
        private readonly IEventSource _eventSource;
        private readonly ICardGeneratorService _cardGeneratorService;
        private readonly IProductGeneratorService _productGeneratorService;

        public PurchaseSimulationService(
            IErrorHandlingService errorHandlingService,
            IEventSource eventSource,
            ICardGeneratorService cardGeneratorService,
            IProductGeneratorService productGeneratorService)
        {
            _errorHandlingService = errorHandlingService;
            _eventSource = eventSource;
            _cardGeneratorService = cardGeneratorService;
            _productGeneratorService = productGeneratorService;
        }

        public void GeneratePurchases(List<Product> products, List<Affiliate> affiliates, List<Card> cards, int count)
        {
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
        }

        public List<Purchase> GetPurchases() => _purchases;

        public Purchase GeneratePurchase()
        {
            if (_purchases.Count == 0)
            {
                throw new InvalidOperationException("No purchases available. Please generate purchases first.");
            }

            return _purchases[new Random().Next(_purchases.Count)];
        }

        public async Task SimulatePurchaseAsync()
        {
            // Generar una compra aleatoria
            var purchase = GeneratePurchase();
            var card = _cardGeneratorService.GetCards().FirstOrDefault(c => c.Id == purchase.CardId);

            if (card == null)
            {
                throw new InvalidOperationException("No card available for the purchase.");
            }

            // Procesar la compra
            await ProcessPurchaseAsync(purchase, card);
        }

        public async Task<bool> ProcessPurchaseAsync(Purchase purchase, Card card)
        {
            try
            {
                // Calcular el total de la compra
                decimal totalPurchaseAmount = purchase.Product.Price;

                // Verificar si la tarjeta tiene fondos suficientes
                if (card.Funds < totalPurchaseAmount)
                {
                    // Fondos insuficientes, manejar el error
                    _errorHandlingService.HandleNoFundsError(card.Id);
                    return false;
                }

                // Verificar si la tarjeta está inactiva
                if (card.Status == CardStatus.Inactive)
                {
                    // Tarjeta inactiva, manejar el error
                    _errorHandlingService.HandleInactiveCardError(card.Id);
                    return false;
                }

                // Si todo está bien, procesar la compra
                card.Funds -= totalPurchaseAmount;
                await _eventSource.SendPurchaseEventAsync(purchase, true);
                return true;
            }
            catch (Exception ex)
            {
                // Manejar cualquier excepción que ocurra durante el proceso
                _errorHandlingService.HandleError(ex);
                return false;
            }
        }

        // Implementación del método sincrónico para cumplir con la interfaz
        public bool ProcessPurchase(Purchase purchase, Card card)
        {
            return ProcessPurchaseAsync(purchase, card).GetAwaiter().GetResult();
        }

        public async Task SimulatePurchases()
        {
            // Generar afiliados
            var affiliateGeneratorService = new FakeAffiliateGeneratorService();
            affiliateGeneratorService.ForceGenerateAffiliate(); // Generar un afiliado inmediatamente
            var affiliates = affiliateGeneratorService.GetAffiliates();

            // Esperar un momento para asegurar que los afiliados se generen
            await Task.Delay(1000);

            // Generar productos
            var productGeneratorService = new FakeProductGeneratorService(affiliates);
            var products = productGeneratorService.GetProducts();

            // Generar tarjetas
            var cardGeneratorService = new FakeCardGeneratorService();
            cardGeneratorService.ForceGenerateCard(); // Generar una tarjeta inmediatamente
            var cards = cardGeneratorService.GetCards();

            // Esperar un momento para asegurar que las tarjetas se generen
            await Task.Delay(1000);

            // Generar compras
            GeneratePurchases(products, affiliates, cards, 10);

            // Simular una compra
            await SimulatePurchaseAsync();
        }
    }
}
