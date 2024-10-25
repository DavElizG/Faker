using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bogus;
using Api.Domain.Entities;

namespace Api.Application.Services.FakeDataGenerators
{
    public class FakeProductGeneratorService
    {
        private readonly List<Product> _products = new List<Product>();

        public FakeProductGeneratorService(List<Affiliate> affiliates)
        {
            foreach (var affiliate in affiliates)
            {
                GenerateProducts(affiliate.Id, 5); // Genera 5 productos por afiliado
            }
        }

        // Método para generar productos asociados a un afiliado específico
        public void GenerateProducts(Guid affiliateId, int count)
        {
            var faker = new Faker<Product>()
                .RuleFor(p => p.Id, f => Guid.NewGuid())
                .RuleFor(p => p.Name, f => f.Commerce.ProductName())
                .RuleFor(p => p.Description, f => f.Commerce.ProductDescription())
                .RuleFor(p => p.Price, f => f.Random.Decimal(5, 500))
                .RuleFor(p => p.Category, f => f.Commerce.Categories(1)[0])  // Genera una categoría aleatoria
                .RuleFor(p => p.Stock, f => f.Random.Int(0, 100))             // Cantidad en stock
                .RuleFor(p => p.AffiliateId, affiliateId);

            _products.AddRange(faker.Generate(count));
        }

        // Método para obtener la lista de productos generados
        public List<Product> GetProducts() => _products;
    }
}
