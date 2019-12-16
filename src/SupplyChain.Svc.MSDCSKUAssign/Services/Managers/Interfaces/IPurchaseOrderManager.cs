using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SupplyChain.Svc.MSDCSKUAssign.Services.Managers.Interfaces
{
    public interface IPurchaseOrderManager<T>
    {
        /// <summary>
        /// Gets all active purchase orders
        /// </summary>
        /// <returns></returns>
        List<T> GetActivePurchaseOrders();

    }
}
