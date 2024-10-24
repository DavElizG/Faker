using Api.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Application.Commands
{
    // Comando para generar compras
    public class GeneratePurchaseCommand : IRequest<Unit>
    {
        // Lista de productos disponibles para la compra
        public List<Product> Products { get; set; }

        // Lista de afiliados disponibles
        public List<Affiliate> Affiliates { get; set; }

        // Lista de tarjetas disponibles para la compra
        public List<Card> Cards { get; set; }

        // Número de compras a generar
        public int Count { get; set; }

        // Constructor que inicializa el comando con los datos necesarios
        public GeneratePurchaseCommand(List<Product> products, List<Affiliate> affiliates, List<Card> cards, int count)
        {
            Products = products;
            Affiliates = affiliates;
            Cards = cards;
            Count = count;
        }
    }
}
