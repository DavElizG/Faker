using System;
using System.Collections.Generic;
using Bogus;
using Api.Domain.Entities;
using Api.Domain.Interfaces.Generators;

namespace Api.Application.Services.FakeDataGenerators
{
    public class FakeAffiliateGeneratorService : IAffiliateGeneratorService
    {
        private readonly List<Affiliate> _affiliates = new List<Affiliate>();

        public FakeAffiliateGeneratorService() { }

        public void GenerateAffiliates(int count)
        {
            var faker = new Faker<Affiliate>()
                .RuleFor(a => a.Id, f => Guid.NewGuid())
                .RuleFor(a => a.Name, f => f.Company.CompanyName())
                .RuleFor(a => a.Address, f => f.Address.FullAddress())
                .RuleFor(a => a.PhoneNumber, f => f.Phone.PhoneNumber())
                .RuleFor(a => a.Email, f => f.Internet.Email())
                .RuleFor(a => a.Website, f => f.Internet.Url());

            _affiliates.AddRange(faker.Generate(count));
        }

        public List<Affiliate> GetAffiliates() => _affiliates;

        public void ForceGenerateAffiliate() => GenerateAffiliates(1);
    }
}
