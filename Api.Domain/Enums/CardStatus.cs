using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Domain.Enums
{
    public enum CardStatus
    {
        Active,      // Tarjeta activa y disponible para uso
        NoFunds,     // Tarjeta sin fondos disponibles
        Inactive,    // Tarjeta inactiva, no se puede usar
        Expired      // Tarjeta vencida
    }
}
