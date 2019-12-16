using SupplyChain.Svc.MSDCSKUAssign.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SupplyChain.Svc.MSDCSKUAssign.Services.Managers.Interfaces
{
    public interface IAllocationManager<T>
    {
        List<T> GetAllocationContracts(Guid batchId, List<SkuContract> sku, List<TNTContract> tnt, bool isReporting);

        /// <summary>
        /// Returns a dictionary of store allocations
        /// </summary>
        /// <returns></returns>
        List<Tuple<int, int>> GetStoreAllocations();

        /// <summary>
        /// Returns value if Offsite Daily Updated
        /// </summary>
        /// <returns></returns>
        bool IsOffsiteDailyUpdated();

        /// <summary>
        /// Stages INV1 and OffsiteDaily for Mock Allocation
        /// </summary>
        /// <returns></returns>
        bool IsDataStaged(List<SkuContract> invRecords);

    }

}
