using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Application.Exceptions
{
    public class CardException : Exception
    {
        public CardException() { }

        public CardException(string message) : base(message) { }

        public CardException(string message, Exception inner) : base(message, inner) { }
    }

    public class PurchaseStatusException : Exception
    {
        public PurchaseStatusException() { }

        public PurchaseStatusException(string message) : base(message) { }

        public PurchaseStatusException(string message, Exception inner) : base(message, inner) { }
    }
}
