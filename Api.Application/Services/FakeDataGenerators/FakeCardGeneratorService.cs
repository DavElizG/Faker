using System;
using System.Collections.Generic;
using Bogus;
using Api.Domain.Entities;
using Api.Domain.Enums;
using Api.Domain.Interfaces.Generators;

namespace Api.Application.Services.FakeDataGenerators
{
    public class FakeCardGeneratorService : ICardGeneratorService
    {
        private readonly List<Card> _cards = new List<Card>();

        public FakeCardGeneratorService() { }
        public void GenerateCards(int count)
        {
            var faker = new Faker<Card>()
                .RuleFor(c => c.Id, f => Guid.NewGuid())
                .RuleFor(c => c.Name, f => f.Name.FullName())
                .RuleFor(c => c.Email, f => f.Internet.Email())
                .RuleFor(c => c.CreditCardNumber, f => f.Finance.CreditCardNumber())
                .RuleFor(c => c.ExpiryDate, f => f.Random.Double() < 0.05
                    ? f.Date.Past(1)  // 5% de las tarjetas estarán expiradas
                    : f.Date.Future(3)) // 95% con fecha futura
                .RuleFor(c => c.Funds, f => f.Random.Decimal(0, 5000)) // Ajuste de rango para errores de fondos insuficientes
                .RuleFor(c => c.Brand, f => f.PickRandom("Visa", "MasterCard", "Amex", "Discover", "JCB", "Diners Club", "UnionPay"))
                .RuleFor(c => c.CVV, (f, c) => c.Brand == "Amex" || c.Brand == "Diners Club"
                    ? f.Random.Int(1000, 9999).ToString()
                    : f.Random.Int(100, 999).ToString())
                .RuleFor(c => c.Status, f => f.Random.Double() < 0.8
                    ? CardStatus.Active     // 80% de las tarjetas serán activas
                    : CardStatus.Inactive); // 20% serán inactivas

            _cards.AddRange(faker.Generate(count));
        }

        public List<Card> GetCards() => _cards;

        public void ForceGenerateCard() => GenerateCards(1);
    }
}
