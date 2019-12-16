using System;
using System.Collections.Generic;
using SupplyChain.Svc.MSDCSKUAssign.Contracts;

namespace SupplyChain.Svc.MSDCSKUAssign.Controllers
{
    [Serializable]
    public struct ResultProcessMultiDCSkus
    {
        public Guid BatchId;
        public List<AllocationContract> Allocations;
        public bool StatusCode;
    }
}
