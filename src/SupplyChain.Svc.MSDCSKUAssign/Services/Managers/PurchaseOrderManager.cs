using SupplyChain.Svc.MSDCSKUAssign.Services.Managers.Interfaces;
using SupplyChain.Svc.MSDCSKUAssign.Contracts;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using SupplyChain.Svc.MSDCSKUAssign.Repositories.Interfaces;

namespace SupplyChain.Svc.MSDCSKUAssign.Services.Managers
{
    public class PurchaseOrderManager : IPurchaseOrderManager<PurchaseOrderContract>
    {
        #region [ Declarations ]

        private IConfiguration _config;
        private ILogger<PurchaseOrderManager> _log;
        private IRepositorySvc<PurchaseOrderContract> _poRepository;

        #endregion

        #region [ Instantiation ]

        public PurchaseOrderManager(IConfiguration config, ILogger<PurchaseOrderManager> log, IRepositorySvc<PurchaseOrderContract> poRepository)
        {
            _config = config;
            _log = log;
            _poRepository = poRepository;
        }

        #endregion

        #region [ Public Methods ]

        /// <summary>
        /// Returns all p
        /// </summary>
        /// <returns></returns>
        public List<PurchaseOrderContract> GetActivePurchaseOrders()
        {
            _log.LogInformation("Calling Purchase Order data from GSDM...");
            return _poRepository.GetContracts();
        }

        #endregion
    }
}
