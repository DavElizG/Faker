using Api.Domain.Entities;
using Api.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public Task AddFundsAsync(Guid cardId, decimal amount)
        {
            throw new NotImplementedException();
        }

        public Task ModifyCardAsync(Card card)
        {
            throw new NotImplementedException();
        }

        // Método para establecer los fondos de una tarjeta en cero
        public async Task SetFundsToZeroAsync(Guid cardId)
        {
            var card = _cards.FirstOrDefault(c => c.Id == cardId);
            if (card == null)
            {
                throw new ArgumentException("Card not found");
            }

            // Establecer los fondos de la tarjeta en 0
            card.Funds = 0;
            await Task.CompletedTask;
        }

        public Task UpdateCardStatusAsync(Guid cardId, string status)
        {
            throw new NotImplementedException();
        }
    }
}
