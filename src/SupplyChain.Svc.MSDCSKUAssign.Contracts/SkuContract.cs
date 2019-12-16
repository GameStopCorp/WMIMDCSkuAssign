using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SupplyChain.Svc.MSDCSKUAssign.Contracts
{
    [Serializable]
    /// <summary>
    /// SKU Contract for INV1 and Offsite Daily records
    /// </summary>
    public class SkuContract
    {
        /// <summary>
        /// Sku Number
        /// </summary>
        public string SkuNumber { get; set; }

        /// <summary>
        /// GISMO item Number
        /// </summary>
        public int ItemNumber { get; set; }

        /// <summary>
        /// Warehouse OnHand Quantity
        /// </summary>
        public int WOHNorm { get; set; }

        /// <summary>
        /// Warehouse On Order quantity
        /// </summary>
        public int OnOrderWh { get; set; }

        /// <summary>
        /// Warehouse SKU location
        /// </summary>
        public string WHLocation { get; set; }

        /// <summary>
        /// Daily Offsite On Hand Record
        /// </summary>
        public DateTime? DailyRecord { get; set; }

        /// <summary>
        /// Title of SKU [Description]
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Purchase Order Total
        /// </summary>
        public int POQty { get; set; }

        /// <summary>
        /// Estimated weight of SKU
        /// </summary>
        public double Weight { get; set; }

        /// <summary>
        /// Warehouse TNT contract for SKU
        /// </summary>
        public TNTContract WhseTntContract { get; set; }

    }
}
