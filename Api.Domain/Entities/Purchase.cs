using Api.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Domain.Entities
{
    public class Purchase
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public Product Product { get; set; }
        public Guid AffiliateId { get; set; }
        public Affiliate Affiliate { get; set; }
        public Guid CardId { get; set; }
        public Card Card { get; set; }
        public DateTime PurchaseDate { get; set; }
        public decimal Amount { get; set; }
        public PurchaseStatus Status { get; set; }
    }
}
