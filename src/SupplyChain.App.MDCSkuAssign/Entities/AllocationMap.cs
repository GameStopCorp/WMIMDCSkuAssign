using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SupplyChain.App.MDCSkuAssign.Entities
{
    public class AllocationMap : ClassMap<Allocation>
    {
        public AllocationMap()
        {
            Map(m => m.Warehouse).Index(0);
            Map(m => m.AllocNum).Index(1);
            Map(m => m.Store).Index(2);
            Map(m => m.Sku).Index(3);
            Map(m => m.Quantity).Index(4);
            Map(m => m.Price).Index(5);
            Map(m => m.Item).Index(6);

            Map(m => m.Parstype).Index(7);
            Map(m => m.Rfm).Index(8);
            Map(m => m.Margin).Index(9);
            Map(m => m.CarryForwardDays).Index(10);
            Map(m => m.OutofStock).Index(11);
            Map(m => m.Velocity).Index(12);
            Map(m => m.Weight).Index(13);
            Map(m => m.AType == 11);
            Map(m => m.isProcessed == false);
        }
    }
}