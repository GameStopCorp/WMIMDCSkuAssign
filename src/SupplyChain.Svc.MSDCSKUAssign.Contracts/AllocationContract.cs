using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SupplyChain.Svc.MSDCSKUAssign.Contracts
{
    /// <summary>
    /// Output allocation contract;
    /// Contract will be converted to CSV  andpresented to Warehouse Allocation service
    /// </summary>
    [Serializable]
    public class AllocationContract
    {
        public string Warehouse { get; set; }

        public string AllocNum { get; set; }

        public int Store;

        public string Sku { get; set; }

        public int Quantity { get; set; }

        public int Price { get; set; }

        public int Item { get; set; }

        public int Parstype { get; set; }

        public int Rfm { get; set; }

        public int Margin { get; set; }

        public int CarryForwardDays { get; set; }

        public string OutofStock { get; set; }

        public string Velocity { get; set; }

        public double Weight { get; set; }

    }
}
