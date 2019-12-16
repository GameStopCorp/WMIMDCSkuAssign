using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SupplyChain.App.MDCSkuAssign.Exceptions
{
    public class WebServiceAvaliableException : Exception
    {

        public WebServiceAvaliableException() : base()
        {

        }

        public WebServiceAvaliableException(string message) : base(message)
        {

        }
    }
}
