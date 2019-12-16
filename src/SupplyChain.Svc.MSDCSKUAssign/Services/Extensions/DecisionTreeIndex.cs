using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SupplyChain.Svc.MSDCSKUAssign.Services.Extensions
{
    /// <summary>
    /// Decision Tree Index for reporting levels
    /// </summary>
    public enum DecisionTreeIndex
    {
        /// <summary>
        /// Lowest TNT and Zone
        /// </summary>
        Lowest_TNTZone_Selection = 0,

        /// <summary>
        /// Lowest TNT
        /// </summary>
        Lowest_TNT_Selection = 1,

        /// <summary>
        /// Equal TNT and Inventory
        /// </summary>
        Equal_TNT_Inventory_Selection = 2,

        /// <summary>
        /// Highest Inventory (default)
        /// </summary>
        Highest_Inventory_Selection = 3,

        /// <summary>
        /// Highest inventory with equal TNT
        /// </summary>
        Highest_Inventory_Equal_TNT_Selection = 4,

        /// <summary>
        /// Highest inventory with equal TNT
        /// </summary>
        Highest_Inventory_Equal_TNT_And_Zone_Selection = 5,

        /// <summary>
        /// Missing TNT value for store
        /// </summary>
        Missing_Store_TNT = 6,

        /// <summary>
        /// Allocation Skipped for Pending PO fulfillment
        /// </summary>
        Pending_PO_Fulfillment = 7
    }
}
