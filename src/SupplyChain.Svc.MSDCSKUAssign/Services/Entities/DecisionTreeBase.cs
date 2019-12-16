using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SupplyChain.Svc.MSDCSKUAssign.Contracts;
using SupplyChain.Svc.MSDCSKUAssign.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SupplyChain.Svc.MSDCSKUAssign.Services.Entities
{
    public abstract class DecisionTreeBase : IDisposable
    {
        #region [ Declarations ]

        /// <summary>
        /// Configuration from intance
        /// </summary>
        protected IConfiguration _config;

        /// <summary>
        /// Logger instantiated from DI
        /// </summary>
        protected ILogger<DecisionTreeBase> _log;

        /// <summary>
        /// Repository for committing results 
        /// </summary>
        protected IControlRepository<ControlContract, SkuContract> _controlRepository;

        /// <summary>
        /// All time in transits for all warehouses to all stores
        /// </summary>
        protected List<TNTContract> _transits;

        /// <summary>
        /// SKU contracts for determining differences
        /// </summary>
        protected List<SkuContract> _skus;

        /// <summary>
        /// Allocation quantities 
        /// </summary>
        protected AllocationContract _allocation;

        /// <summary>
        /// Switch if reporting is activated
        /// </summary>
        protected bool _isReporting;

        #endregion

        #region [ Protected Inherited ]

        protected DecisionTreeBase(IConfiguration config, ILogger<DecisionTreeBase> log, IControlRepository<ControlContract, SkuContract> controlRepository,
   List<SkuContract> skus, List<TNTContract> transits, AllocationContract allocation, bool isReporting)
        {
            _config = config;
            _log = log;
            _controlRepository = controlRepository;
            _transits = transits;
            _skus = skus;
            _allocation = allocation;
            _isReporting = isReporting;
        }

        protected bool IsEqual(int value1, int value2)
        {
            return value1.Equals(value2);
        }

        protected TNTContract GetLowestTntContract()
        {
            return _transits
         .Where(s => s.StoreNumber == _allocation.Store)
         .OrderByDescending(t => t.CarrierZoneId).LastOrDefault();
        }

        protected TNTContract GetAlternateTntContract()
        {
            return _transits
         .Where(s => s.StoreNumber == _allocation.Store)
         .OrderByDescending(t => t.CarrierZoneId).FirstOrDefault();
        }

        protected bool IsResultCommitted(ControlContract results)
        {
            return _controlRepository.Commit(results);
        }

        #endregion

        #region [ IDisposable Support ]
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    //_transits = null;
                    //_skus = null;
                }

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);

        }

        #endregion
    }
}
