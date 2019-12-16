using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SupplyChain.App.MDCSkuAssign.Services.Interfaces
{
    public interface IDistroService<T>
    {
        void Create(List<T> allocations, string whse);
    }
}
