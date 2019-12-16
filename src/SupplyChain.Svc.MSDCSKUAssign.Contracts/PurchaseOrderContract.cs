using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SupplyChain.Svc.MSDCSKUAssign.Contracts
{
    [Serializable]
    /// <summary>
    /// Purchase Order Contract;  Used to structure data from GSDM
    /// </summary>
    public class PurchaseOrderContract
    {
        /// <summary>
        /// Purchase Order Number
        /// </summary>
        public int PONum { get; set; }

        /// <summary>
        /// Vendor
        /// </summary>
        public int Vendor { get; set; }

        /// <summary>
        /// Status of Purchase Order
        /// </summary>
        public int POStatus { get; set; }

        /// <summary>
        /// Purchase order Item status
        /// </summary>
        public int SKUStatus { get; set; }

        /// <summary>
        /// Purchase Warehouse
        /// </summary>
        public string PurchaseWarehouse { get; set; }

        /// <summary>
        /// PO Order date
        /// </summary>
        public DateTime DateOrdered { get; set; }

        /// <summary>
        /// PO Cancel Date
        /// </summary>
        public DateTime PODateCanceled { get; set; }

        /// <summary>
        /// SKU Item Cancel Date
        /// </summary>
        public DateTime SKUDateCanceled { get; set; }

        /// <summary>
        /// Purchase Order ETA
        /// </summary>
        public DateTime PO_ETA { get; set; }

        /// <summary>
        /// Line Item ETA on Purchase Order
        /// </summary>
        public DateTime SKU_ETA { get; set; }

        /// <summary>
        /// PO Load Date
        /// </summary>
        public DateTime POLoadDate { get; set; }

        /// <summary>
        /// SKU Item Load Date
        /// </summary>
        public DateTime SKULoadDate { get; set; }

        /// <summary>
        /// Line Item on PO
        /// </summary>
        public int LineItem { get; set; }

        /// <summary>
        /// SKU on PO
        /// </summary>
        public string SKU { get; set; }

        /// <summary>
        /// Ship 
        /// </summary>
        public Int16 Ship { get; set; }

        /// <summary>
        /// Is Ordered
        /// </summary>
        public int Ordered { get; set; }

        /// <summary>
        /// Is stocked 
        /// </summary>
        public int Stocked { get; set; }

        /// <summary>
        /// Date first received on PO
        /// </summary>
        public DateTime FirstRcvd { get; set; }

        /// <summary>
        /// Date Last received on PO
        /// </summary>
        public DateTime LastRcvd { get; set; }

        /// <summary>
        /// Price of item
        /// </summary>
        public decimal Price { get; set; }
    }

}
