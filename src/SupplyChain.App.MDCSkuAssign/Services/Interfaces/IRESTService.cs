using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace SupplyChain.App.MDCSkuAssign.Services.Interfaces
{
    public interface IRESTService
    {
        HttpResponseMessage GET(string url, int? httpClientTimeout = null);
    }
}
