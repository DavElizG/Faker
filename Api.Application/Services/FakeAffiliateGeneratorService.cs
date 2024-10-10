using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bogus;
using System.Threading;
using Api.Domain.Entities;

namespace Api.Application.Services
{
    public class FakeAffiliateGeneratorService : BackgroundService
    {
        private readonly Faker<Affiliate> _faker;
        private readonly List<Affiliate> _affiliates;

        public FakeAffiliateGeneratorService()
        {
            // Configuramos Bogus para generar datos aleatorios para negocios
            _faker = new Faker<Affiliate>()
                .RuleFor(a => a.Id, f => f.IndexFaker + 1)
                .RuleFor(a => a.Name, f => f.Company.CompanyName())
                .RuleFor(a => a.Address, f => f.Address.FullAddress())
                .RuleFor(a => a.Email, f => f.Internet.Email())
                .RuleFor(a => a.Phone, f => f.Phone.PhoneNumber());

            _affiliates = new List<Affiliate>();
        }

        // Método para obtener todos los negocios generados
        public IEnumerable<Affiliate> GetAffiliates() => _affiliates;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // Generar un nuevo negocio y agregarlo a la lista acumulativa
                var newAffiliate = _faker.Generate();
                _affiliates.Add(newAffiliate);

                // Esperar un minuto antes de generar otro negocio
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}

