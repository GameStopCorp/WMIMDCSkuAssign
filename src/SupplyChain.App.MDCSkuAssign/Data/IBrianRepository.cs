using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SupplyChain.App.MDCSkuAssign.Data
{
    /// <summary>
    /// Repository Interface for Pervasive
    /// </summary>
    public interface IBrianRepository<T>
    {
        string GetNextBatchNumber();

        List<T> GetAllocationDetails(string whse, string allocNum);
    }
}
