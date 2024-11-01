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
                .RuleFor(c => c.ExpiryDate, f => f.Date.Future(3))
                .RuleFor(c => c.Funds, f => f.Random.Decimal(0, 10000))
                .RuleFor(c => c.Brand, f => f.PickRandom("Visa", "MasterCard", "Amex", "Discover", "JCB", "Diners Club", "UnionPay"))
                .RuleFor(c => c.CVV, (f, c) => c.Brand == "Amex" || c.Brand == "Diners Club"
                    ? f.Random.Int(1000, 9999).ToString()
                    : f.Random.Int(100, 999).ToString())
                .RuleFor(c => c.Status, f => f.PickRandom<CardStatus>());

            _cards.AddRange(faker.Generate(count));
        }

        public List<Card> GetCards() => _cards;

        public void ForceGenerateCard() => GenerateCards(1);
    }
}
