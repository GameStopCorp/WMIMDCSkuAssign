using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SupplyChain.Svc.MSDCSKUAssign.Contracts;
using SupplyChain.Svc.MSDCSKUAssign.Repositories.Interfaces;
using SupplyChain.Svc.MSDCSKUAssign.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SupplyChain.Svc.MSDCSKUAssign.Services.Entities
{
    public class DecisionTreeFactory : IDecisionTreeFactory<SkuContract, TNTContract, AllocationContract>
    {
        #region [ Declarations ]

        private IConfiguration _config;
        private ILogger<DecisionTreeBase> _log;
        private IControlRepository<ControlContract, SkuContract> _controlRepository;

        #endregion

        #region [ Instantiation ]

        public DecisionTreeFactory(IConfiguration config, ILogger<DecisionTreeBase> log, IControlRepository<ControlContract, SkuContract> controlRepository)
        {
            _config = config;
            _log = log;
            _controlRepository = controlRepository;
        }

        #endregion

        #region [ Implementation ]

        public T Create<T>(List<SkuContract> skus, List<TNTContract> transits, AllocationContract allocation, bool isReporting) where T : DecisionTreeBase
        {
            return new DecisionTree(_config, _log, _controlRepository, skus, transits, allocation, isReporting) as T;
        }

        #endregion
    }
}
