using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SupplyChain.Svc.MSDCSKUAssign.Services.Managers.Interfaces
{
    public interface ITnTManager<T>
    {
        List<T> GetTimeInTransitData(List<Tuple<int, int>> stores);

    }
}
