using Api.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Domain.Interfaces.Infraestructure
{
    public interface IEventSource
    {
        void SendPurchaseEvent(Purchase purchase, bool isSuccess);
        Task SendPurchaseEventAsync(Purchase purchase, bool isSuccess);
    }
}
