using Api.Domain.Entities;
using Api.Domain.Enums;
using Api.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Application.Services
{
    public class CardModificationService : ICardModificationService
    {
        private readonly List<Card> _cards;

        public CardModificationService(List<Card> cards)
        {
            _cards = cards;
        }

        public async Task ModifyCardAsync(Card card)
        {
            var existingCard = _cards.FirstOrDefault(c => c.Id == card.Id);
            if (existingCard == null)
            {
                throw new ArgumentException("Card not found");
            }

            // Actualizar los campos de la tarjeta existente
            existingCard.Name = card.Name;
            existingCard.Email = card.Email;
            existingCard.CreditCardNumber = card.CreditCardNumber;
            existingCard.ExpiryDate = card.ExpiryDate;
            existingCard.Funds = card.Funds;
            existingCard.Brand = card.Brand;
            existingCard.CVV = card.CVV;

            // Simular una operación asincrónica
            await Task.CompletedTask;
        }

        public async Task AddFundsAsync(Guid cardId, decimal amount)
        {
            var card = _cards.FirstOrDefault(c => c.Id == cardId);
            if (card == null)
            {
                throw new ArgumentException("Card not found");
            }

            card.Funds += amount;
            await Task.CompletedTask;
        }

        public async Task UpdateCardStatusAsync(Guid cardId, string status)
        {
            var card = _cards.FirstOrDefault(c => c.Id == cardId);
            if (card == null)
            {
                throw new ArgumentException("Card not found");
            }

            if (Enum.TryParse(status, out CardStatus cardStatus))
            {
                card.Status = cardStatus;
            }
            else
            {
                throw new ArgumentException("Invalid status value");
            }

            await Task.CompletedTask;
        }
    }
}
