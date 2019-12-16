using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SupplyChain.Svc.MSDCSKUAssign.Repositories.Interfaces
{
    public interface IRepositorySvc<T>
    {
        List<T> GetContracts();

        List<T> GetContractsByAllocNum(int allocationNumber);

        List<T> GetContractsByStore(int allocationNumber, int itemNumber);

        T Find();

        List<T> GetSummaries();
    }
}
