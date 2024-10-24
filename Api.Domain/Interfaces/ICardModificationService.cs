using Api.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Domain.Interfaces
{
    public interface ICardModificationService
    {
        Task ModifyCardAsync(Card card);
        Task AddFundsAsync(Guid cardId, decimal amount);
        Task UpdateCardStatusAsync(Guid cardId, string status);
    }
}
