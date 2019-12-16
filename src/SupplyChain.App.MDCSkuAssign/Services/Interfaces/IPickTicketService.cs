using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SupplyChain.App.MDCSkuAssign.Services.Interfaces
{
    public interface IPickTicketService<T>
    {
        void Create(List<T> allocations, string whse);

        bool Release();
    }
}
