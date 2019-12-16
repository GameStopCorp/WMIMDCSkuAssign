using SupplyChain.Svc.MSDCSKUAssign.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SupplyChain.Svc.MSDCSKUAssign.Services.Interfaces
{
    public interface IMsdcService
    {
        List<AllocationContract> GetMultiSkuAllocations(Guid batchId, bool isReporting);

        bool IsDailyInventoryUpdated();

    }
}
