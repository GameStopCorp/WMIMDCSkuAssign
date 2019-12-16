using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SupplyChain.App.MDCSkuAssign.Data
{
    /// <summary>
    /// Repository for PTL database
    /// </summary>
    public interface IPtlRepository<T>
    {
        List<T> GetTimeInTransit();

        List<KeyValuePair<int, int>> AllocationOrderTypeLookup();
    }
}
