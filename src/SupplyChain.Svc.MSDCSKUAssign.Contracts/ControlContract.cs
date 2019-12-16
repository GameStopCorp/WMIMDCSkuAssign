using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SupplyChain.Svc.MSDCSKUAssign.Contracts
{
    [Serializable]
    public class ControlContract
    {
        /// <summary>
        /// Allocation Run Time
        /// </summary>
        public DateTime AllocationRunTime { get; set; }

        /// <summary>
        /// Batch ID for allocation run for grouping allocation runs 
        /// </summary>
        public Guid BatchId { get; set; }

        /// <summary>
        /// Allocation number
        /// </summary>
        public string AllocationNumber { get; set; }

        /// <summary>
        /// SKU number
        /// </summary>
        public string SKU { get; set; }

        /// <summary>
        /// Store number
        /// </summary>
        public int StoreNumber { get; set; }

        /// <summary>
        /// Lowest TNT value
        /// </summary>
        public int SelectedTnTValue { get; set; }

        /// <summary>
        /// Selected TNT Warehouse
        /// </summary>
        public string SelectedTnTWarehouse { get; set; }

        /// <summary>
        /// Selected warehouse on hand quantity
        /// </summary>
        public int SelectedWarehouseOHQty { get; set; }


        /// <summary>
        /// Alternate warehouse TNT value
        /// </summary>
        public int AlternateTnTValue { get; set; }

        /// <summary>
        /// Alternate warehouse TNT value
        /// </summary>
        public string AlternateTnTWarehouse { get; set; }

        /// <summary>
        /// Alternate warehouse on hand quantity
        /// </summary>
        public int AlternateWarehouseOHQty { get; set; }

        /// <summary>
        /// Allocation Quantity
        /// </summary>
        public int AllocationQuantity { get; set; }

        /// <summary>
        /// Allocation Quantity
        /// </summary>
        public int DecisionTreeIndex { get; set; }

        /// <summary>
        /// Actual LOU TNT Value
        /// </summary>
        public int LouTntValue { get; set; }

        /// <summary>
        /// Actual GV1 TNT Value
        /// </summary>
        public int GV1TntValue { get; set; }

        /// <summary>
        /// Selected TNT Zone
        /// </summary>
        public int SelectedTnTZone { get; set; }

        /// <summary>
        /// Alternate TNT Zone
        /// </summary>
        public int AlternateTntZone { get; set; }

        /// <summary>
        /// Selected warehouse purchase order quantity
        /// </summary>
        public int SelectedWarehousePOQty { get; set; }

        /// <summary>
        /// Alternate warehouse purchase order quantity
        /// </summary>
        public int AlternateWarehousePOQty { get; set; }

    }
}
