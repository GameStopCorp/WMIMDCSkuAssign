using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SupplyChain.Svc.MSDCSKUAssign.Contracts;
using SupplyChain.Svc.MSDCSKUAssign.Repositories.Interfaces;
using SupplyChain.Svc.MSDCSKUAssign.Services.Managers.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SupplyChain.Svc.MSDCSKUAssign.Services.Managers
{
    public class TnTManager : ITnTManager<TNTContract>
    {
        #region [ Declarations ]

        private IConfiguration _config;
        private ILogger<TnTManager> _log;
        private IRepositorySvc<TNTContract> _tntRepository;

        #endregion

        public TnTManager(IConfiguration config, ILogger<TnTManager> log, IRepositorySvc<TNTContract> tntRepository)
        {
            _config = config;
            _log = log;
            _tntRepository = tntRepository;
        }

        /// <summary>
        /// Returns time in transit data for the allocation stores of interest
        /// </summary>
        /// <param name="stores"></param>
        /// <returns></returns>
        public List<TNTContract> GetTimeInTransitData(List<Tuple<int, int>> stores)
        {
            var transitStores = new List<TNTContract>();

            //get contracts
            var contracts = _tntRepository.GetContracts();

            foreach (var store in stores)
            {
                var contract = contracts.ToList().Where(c => c.StoreNumber.Equals(store.Item2));

                if (contract == null)
                {
                    _log.LogWarning($"Missing TNT data for store number {store}.");
                }
                else
                {
                    contract.ToList().ForEach(c => c.AllocationNumber = store.Item1);
                    transitStores.AddRange(contract);
                }
            }

            return transitStores.ToList();
        }
    }
}
