using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SupplyChain.Svc.MSDCSKUAssign.Repositories.Interfaces
{
    public interface IControlRepository<T1, T2>
    {
        bool IsUpdatedData(DateTime date);

        bool Commit(T1 contract);

        bool IsUpsertedStageInvData(T2 data);

        T2 GetPervasiveItemBySku(string sku);
    }
}
