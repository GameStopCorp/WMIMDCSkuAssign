using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SupplyChain.App.MDCSkuAssign.Entities
{
    public class MasterAllocDetail
    {
        /// <summary>
        /// Allocation Number
        /// </summary>
        public int AllocNum { get; set; }

        /// <summary>
        /// Store
        /// </summary>
        public int Store { get; set; }

        /// <summary>
        /// Total quantity for the store allocation
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Total estimated combined weight for the store allocation
        /// </summary>
        public int CombinedWeight { get; set; }
    }
}
