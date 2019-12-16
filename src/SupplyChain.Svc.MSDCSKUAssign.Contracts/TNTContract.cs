using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SupplyChain.Svc.MSDCSKUAssign.Contracts
{
    [Serializable]
    /// <summary>
    /// TNT entity from WCS Routing Data
    /// </summary>
    public class TNTContract
    {
        /// <summary>
        /// Store Number
        /// </summary>
        public int StoreNumber { get; set; }

        /// <summary>
        /// Origin of route path
        /// </summary>
        public string Origin { get; set; }

        /// <summary>
        /// Carrier ID 
        /// </summary>
        public int CarrierId { get; set; }

        /// <summary>
        /// Time in Transit (TNT) interval
        /// </summary>
        public int TimeInTransit { get; set; }

        /// <summary>
        /// Process run allocation number
        /// </summary>
        public int AllocationNumber { get; set; }

        /// <summary>
        /// Carrier Zone
        /// </summary>
        public int CarrierZone { get; set; }

        /// <summary>
        /// Concatenated Carrier Zone and TNT for sorting and equal comparisons
        /// </summary>
        public int CarrierZoneId { get; set; }

    }
}
