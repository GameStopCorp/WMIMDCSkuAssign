using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SupplyChain.Svc.MSDCSKUAssign.Contracts;
using SupplyChain.Svc.MSDCSKUAssign.Services.Interfaces;
using SupplyChain.Svc.MSDCSKUAssign.Services.Managers.Interfaces;
using SupplyChain.Svc.MSDCSKUAssign.Services.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SupplyChain.Svc.MSDCSKUAssign.Services
{
    public class MsdcService : IMsdcService
    {
        #region [ Declarations ]

        private IConfiguration _config;
        private ILogger<MsdcService> _log;
        private readonly IAllocationManager<AllocationContract> _allocMgr;
        private readonly ISkuManager<SkuContract> _skuMgr;
        private readonly ITnTManager<TNTContract> _tntMgr;
        private readonly IPurchaseOrderManager<PurchaseOrderContract> _purchMgr;

        #endregion

        #region [ Instantiation ]

        public MsdcService(IConfiguration config, ILogger<MsdcService> log, IAllocationManager<AllocationContract> allocMgr,
            ISkuManager<SkuContract> skuMgr, ITnTManager<TNTContract> tntMgr, IPurchaseOrderManager<PurchaseOrderContract> purchMgr)
        {
            _config = config;
            _log = log;
            _allocMgr = allocMgr;
            _tntMgr = tntMgr;
            _skuMgr = skuMgr;
            _purchMgr = purchMgr;
        }

        #endregion

        #region [ Public Methods ]

        /// <summary>
        /// Returns a list of multi DC allocations for current date
        /// </summary>
        /// <returns></returns>
        public List<AllocationContract> GetMultiSkuAllocations(Guid batchId, bool isReporting)
        {
            try
            {
                var transitAllocations = new List<AllocationContract>();

                //gets a list of INV1 records that are MultiDC SKUS;
                var skuSummaries = _skuMgr.GetSummarySkuRecords();
                if (skuSummaries.Any())
                {
                    _log.LogInformation($"Capturing {skuSummaries.Count} SKUS slated for assignment.");

                    //get current store allocations
                    var t1 = DateTime.Now;
                    var storeAllocations = _allocMgr.GetStoreAllocations();
                    LoggerExtension.LogTimeDiff(t1, DateTime.Now, _log,
                        string.Format("@@@VS- Time taken to get all store allocations"));

                    if (!storeAllocations.Any())
                    {
                        throw new ApplicationException("Missing store allocations:  No store allocation files found from WMIMasterAllocation directory");
                    }

                    //get Time in Transits based off store allocations
                    var t3 = DateTime.Now;
                    var transits = _tntMgr.GetTimeInTransitData(storeAllocations);
                    LoggerExtension.LogTimeDiff(t3, DateTime.Now, _log,
                        string.Format("@@@VS- Time taken to get TnT data: "));
                    _log.LogInformation($"Capturing {transits.Count} Time-In-Transits for all current warehouse to store for current allocation.");

                    var t5 = DateTime.Now;
                    var skuDailies = _skuMgr.GetOffsiteDailyRecords();
                    LoggerExtension.LogTimeDiff(t5, DateTime.Now, _log,
                        string.Format("@@@VS- Time taken to get offsite daily records after time stamp: "));

                    var skus = from d in skuDailies
                               join s in skuSummaries on d.ItemNumber equals s.ItemNumber
                               select d;

                    if (!skus.Any() || !transits.Any())
                    {
                        throw new ApplicationException("Missing SKU info from Pervasive and Time In Transit data.  Please check sources");
                    }

                    _log.LogInformation($"Starting SKU-Warehouse assignment process for current allocation.");

                    //returns transit allocations based off store allocations
                    var t7 = DateTime.Now;
                    transitAllocations = _allocMgr.GetAllocationContracts(batchId, skus.ToList(), transits, isReporting);
                    LoggerExtension.LogTimeDiff(t7, DateTime.Now, _log,
                        string.Format("@@@VS- Time taken to get allocation contracts from allocation manager:"));
                }

                return transitAllocations;
            }
            catch (Exception ex)
            {
                _log.LogError(ex.Message);
                throw ex;
            }
        }

        /// <summary>
        /// Stages data for current allocation process
        /// </summary>
        /// <returns></returns>
        public bool IsDailyInventoryUpdated()
        {
            //get current SKUS
            var currentSkus = _skuMgr.GetSummarySkuRecords();

            return _allocMgr.IsDataStaged(currentSkus);
        }

        #endregion
    }
}
