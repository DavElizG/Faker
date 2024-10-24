using Api.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Domain.Entities
{
    public class Card
    {
        public Guid Id { get; set; }               // Unique identifier for each card
        public string Name { get; set; }           // Cardholder name
        public string Email { get; set; }          // Email associated with the card
        public string CreditCardNumber { get; set; }
        public DateTime ExpiryDate { get; set; }
        public decimal Funds { get; set; }
        public string Brand { get; set; }
        public string CVV { get; set; }
        public CardStatus Status { get; set; }     // Estado de la tarjeta (usando el enum)
    }
}