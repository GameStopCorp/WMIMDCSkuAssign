using SupplyChain.App.MDCSkuAssign.Entities;
using System.Collections.Generic;

namespace SupplyChain.App.MDCSkuAssign.Services.Interfaces
{
    public interface IProcessor
    {
        bool Process();

        List<Allocation> GetAllocationData();

        void SendEmail(KeyValuePair<string, List<Allocation>> allocations);

    }
}
