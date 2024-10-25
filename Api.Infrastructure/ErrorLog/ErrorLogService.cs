using Api.Domain.Entities;
using Api.Domain.Interfaces.Infraestructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Infrastructure.ErrorLog
{
    public class ErrorLogService : IErrorLogService
    {
        private readonly IEventSource _eventSource;

        public ErrorLogService(IEventSource eventSource)
        {
            _eventSource = eventSource;
        }

        public void LogFailedPurchase(Purchase purchase)
        {
            _eventSource.SendPurchaseEvent(purchase, false);
        }

        public async Task LogFailedPurchaseAsync(Purchase purchase)
        {
            await _eventSource.SendPurchaseEventAsync(purchase, false);
        }
    }
}
