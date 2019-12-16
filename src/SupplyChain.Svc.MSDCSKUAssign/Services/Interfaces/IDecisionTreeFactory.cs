using SupplyChain.Svc.MSDCSKUAssign.Services.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SupplyChain.Svc.MSDCSKUAssign.Services.Interfaces
{
    public interface IDecisionTreeFactory<T1, T2, T3>
    {
        T Create<T>(List<T1> skus, List<T2> transits, T3 allocation, bool isReporting) where T : DecisionTreeBase;
    }
}
