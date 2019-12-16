using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SupplyChain.App.MDCSkuAssign.Entities
{
    /// <summary>
    /// TNT Entity
    /// </summary>
    public class TntItem
    {
        /// <summary>
        /// Store
        /// </summary>
        public int Store { get; set; }

        /// <summary>
        /// Origin Warehouse
        /// </summary>
        public string Origin { get; set; }

        /// <summary>
        /// Carrier ID
        /// </summary>
        public int CarrierId { get; set; }

        /// <summary>
        /// Time In Transit
        /// </summary>
        public int TimeInTransit { get; set; }

        /// <summary>
        /// Carrier Zone
        /// </summary>
        public int CarrierZone { get; set; }

        /// <summary>
        /// Carrier Zone ID
        /// </summary>
        public int CarrierZoneId { get; set; }
    }
}
