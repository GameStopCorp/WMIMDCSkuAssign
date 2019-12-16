using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SupplyChain.Svc.MSDCSKUAssign.Services.Interfaces
{
    public interface IDecisionTree<T>
    {
        T Validate(Guid batchId);

        T GetSkuContractByTnt();

        T GetSkuContractByEqualInventory();

        T GetSkuContractByHigherInventory();

        void Normalize();

        void CreateReport(Guid batchId, T selectedSku, T alternateSku);

        bool SkipAllocationByValidatePOReserve();
    }
}
