using Api.Domain.Entities;
using System.Threading.Tasks;

namespace Api.Domain.Interfaces.Infraestructure
{
    public interface IEventSource
    {
      
        /// <param name="purchase">La entidad de compra que representa el evento.</param>
        /// <param name="isSuccess">Indica si el evento de compra fue exitoso.</param>
        void SendPurchaseEvent(Purchase purchase, bool isSuccess);

        /// <summary>
        /// Envía un evento de compra de manera asincrónica.
        /// </summary>
        /// <param name="purchase">La entidad de compra que representa el evento.</param>
        /// <param name="isSuccess">Indica si el evento de compra fue exitoso.</param>
        /// <returns>Una tarea que representa la operación asincrónica de envío de mensaje.</returns>
        Task SendPurchaseEventAsync(Purchase purchase, bool isSuccess);
    }
}
