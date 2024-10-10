using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bogus;
using System.Threading;
using Api.Domain.Entities;
using Microsoft.Extensions.Hosting;
using System.Timers;
using Timer = System.Timers.Timer;


namespace Api.Application.Services
{
    public class FakeAffiliateGeneratorService
    {
        private readonly List<Affiliate> _affiliates = new List<Affiliate>();
        private readonly Timer _timer;

        public FakeAffiliateGeneratorService()
        {
            _timer = new Timer(10000); // Cada 10 segundos
            _timer.Elapsed += GenerateAffiliate;
            _timer.Start();
        }

        private void GenerateAffiliate(object sender, ElapsedEventArgs e)
        {
            var faker = new Faker<Affiliate>()
                .RuleFor(a => a.Id, f => Guid.NewGuid())
                .RuleFor(a => a.Name, f => f.Company.CompanyName())
                .RuleFor(a => a.Address, f => f.Address.FullAddress())
                .RuleFor(a => a.PhoneNumber, f => f.Phone.PhoneNumber())
                .RuleFor(a => a.Email, f => f.Internet.Email())
                .RuleFor(a => a.Website, f => f.Internet.Url());

            var affiliate = faker.Generate();
            _affiliates.Add(affiliate);
        }

        public List<Affiliate> GetAffiliates() => _affiliates;

        public void ForceGenerateAffiliate()
        {
            GenerateAffiliate(this, null);
        }
    }
}

