using Api.Domain.Entities;
using Bogus;
using System.Collections.Generic;
using System.Timers;

namespace Api.Application.Services.FakeDataGenerators
{
    public class FakeCardGeneratorService
    {
        private readonly List<Card> _cards = new List<Card>();
        private readonly System.Timers.Timer _timer; // Fully qualify Timer

        public FakeCardGeneratorService()
        {
            _timer = new System.Timers.Timer(15000); // Every 15 seconds
            _timer.Elapsed += GenerateCard;
            _timer.Start();
        }

        private void GenerateCard(object sender, ElapsedEventArgs e)
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
                    ? f.Random.Int(1000, 9999).ToString() // 4-digit CVV for Amex and Diners Club
                    : f.Random.Int(100, 999).ToString());  // 3-digit CVV for other brands

            var card = faker.Generate();
            _cards.Add(card);
        }

        public List<Card> GetCards() => _cards;

        public void ForceGenerateCard()
        {
            GenerateCard(this, null);
        }
    }
}
