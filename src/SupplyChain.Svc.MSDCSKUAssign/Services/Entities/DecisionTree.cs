using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SupplyChain.Svc.MSDCSKUAssign.Services.Extensions;
using SupplyChain.Svc.MSDCSKUAssign.Contracts;
using SupplyChain.Svc.MSDCSKUAssign.Repositories.Interfaces;
using SupplyChain.Svc.MSDCSKUAssign.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Collections;

namespace SupplyChain.Svc.MSDCSKUAssign.Services.Entities
{
    public class DecisionTree : DecisionTreeBase, IDecisionTree<SkuContract>
    {
        #region [ Private Declarations ]

        private SkuContract _lowestTntSkuContract;
        private SkuContract _alternateTntSkuContract;
        private bool _isAllocatable = false;
        private DecisionTreeIndex _decisionIndex;
        private ControlContract _reportContract;

        #endregion

        #region [ Instantiation ]

        public DecisionTree(IConfiguration config, ILogger<DecisionTreeBase> log, IControlRepository<ControlContract, SkuContract> controlRepository,
      List<SkuContract> skus, List<TNTContract> transits, AllocationContract allocation, bool isReporting) :
      base(config, log, controlRepository, skus, transits, allocation, isReporting)
        {
            _log.LogInformation($"Building decision tree for allocation number  {allocation.Sku} & store {allocation.Store}");
            Normalize();
        }

        #endregion

        #region [ Public Methods ]

        /// <summary>
        /// Normalize SKUs to their local TNT Contracts
        /// </summary>
        public void Normalize()
        {
            //reset all SKUs
            _skus.ForEach(t => t.WhseTntContract = null);

            var transitStores = _transits.Where(s => s.StoreNumber == _allocation.Store);

            if (transitStores.Count() == 2)
            {
                var skus = from t in transitStores
                           join s in _skus on t.Origin equals s.WHLocation
                           select new { SKU = s, TNT = t };

                foreach (var s in skus)
                {
                    s.SKU.WhseTntContract = s.TNT;
                }
            }
            else
            {
                _log.LogWarning($"Missing TNT for store allocation record {_allocation.AllocNum} and store {_allocation.Store}");
            }
        }

        /// <summary>
        /// Validates the current batch data for the SKU and TNT and returns the most suitable contract for the decision
        /// </summary>
        /// <param name="batchId"></param>
        /// <returns></returns>
        public SkuContract Validate(Guid batchId)
        {
            if (!_skus.Any(tnt => tnt.WhseTntContract == null))
            {
                var sortedContracts = _skus.OrderByDescending(t => t.WhseTntContract.CarrierZoneId);

                _lowestTntSkuContract = sortedContracts.LastOrDefault();
                _alternateTntSkuContract = sortedContracts.FirstOrDefault();

                if (_lowestTntSkuContract.WhseTntContract.Equals(_alternateTntSkuContract.WhseTntContract))
                {
                    _log.LogInformation($"Sorting TNT SKU contracts returned same TNT contract. Assigning alternate contract.");
                    _alternateTntSkuContract = _skus.Where(s => s.WhseTntContract != _lowestTntSkuContract.WhseTntContract).FirstOrDefault();
                }

                var t1 = DateTime.Now;
                SkuContract selectedContract = GetSkuContractByTnt()
                   ?? GetSkuContractByEqualInventory()
                   ?? GetSkuContractByHigherInventory()
                   ?? null;
                LoggerExtension.LogTimeDiff(t1, DateTime.Now, _log,
                        string.Format("@@@VS- Time taken to fetch selected contract:"));

                if (selectedContract != null)
                {
                    _isAllocatable = true;
                    var alternateSkuContract = _skus.Where(a => !a.Equals(selectedContract)).FirstOrDefault();

                    if (_isReporting)
                        CreateReport(batchId, selectedContract, alternateSkuContract);
                }
                else
                {
                    _log.LogInformation($"Allocation Qty {_allocation.Quantity} for SKU {_allocation.Sku} not enough" +
                    $" to fulfill store {_allocation.Store} from either warehouse: {_lowestTntSkuContract.WHLocation}[{_lowestTntSkuContract.WOHNorm}], {_alternateTntSkuContract.WHLocation}[{_alternateTntSkuContract.WOHNorm}].");

                    if (_decisionIndex == DecisionTreeIndex.Pending_PO_Fulfillment)
                    {
                        CreateReport(batchId, _lowestTntSkuContract, _alternateTntSkuContract);
                    }
                }

                return selectedContract;
            }
            else
            {
                _decisionIndex = DecisionTreeIndex.Missing_Store_TNT;
                _isAllocatable = false;
            }

            return null;
        }

        /// <summary>
        /// Get data contract by TNT
        /// </summary>
        /// <returns>Single contract if TNT/Zone are lowest and enough inventory for the allocation; otherwise NULL</returns>
        public SkuContract GetSkuContractByTnt()
        {
            _decisionIndex = DecisionTreeIndex.Lowest_TNT_Selection;

            SkuContract selectedSkuContract = _lowestTntSkuContract.WOHNorm >= _allocation.Quantity ? _lowestTntSkuContract : null;

            if (_lowestTntSkuContract.WhseTntContract.TimeInTransit.Equals(_alternateTntSkuContract.WhseTntContract.TimeInTransit))
            {
                if (_lowestTntSkuContract.WhseTntContract.CarrierZone.Equals(_alternateTntSkuContract.WhseTntContract.CarrierZone))
                {
                    _decisionIndex = DecisionTreeIndex.Highest_Inventory_Equal_TNT_And_Zone_Selection;
                    _log.LogInformation($"TNTs and Zones match for store {_allocation.Store}.");
                    return null;
                }
                else
                {
                    _decisionIndex = selectedSkuContract == null
                        ? DecisionTreeIndex.Highest_Inventory_Equal_TNT_Selection
                        : DecisionTreeIndex.Lowest_TNTZone_Selection;

                    _log.LogInformation($"TNTs match for store {_allocation.Store}. Validating on zones.");
                }
            }

            if (selectedSkuContract == null)
            {
                //no reflection here.. that's too slow and too much overhead
                _log.LogInformation("Returned NULL for lowest TNT and adequate QTY validation. Next, run equal warehouse validation..");
            }

            return selectedSkuContract;
        }

        /// <summary>
        /// Get all contracts by equal inventory
        /// </summary>
        /// <returns>Single contract based off Round-Robin if quantities are equal; otherwise NULL</returns>
        public SkuContract GetSkuContractByEqualInventory()
        {
            SkuContract selectedSkuContract = null;

            if (_lowestTntSkuContract.WhseTntContract.CarrierZoneId.Equals(_alternateTntSkuContract.WhseTntContract.CarrierZoneId))
            {
                if (_skus.All(s => s.WOHNorm.Equals(_skus.FirstOrDefault().WOHNorm) && s.WOHNorm >= _allocation.Quantity))
                {
                    selectedSkuContract = _skus.Shuffle().Take(1).FirstOrDefault();
                    if (selectedSkuContract != null)
                        _decisionIndex = DecisionTreeIndex.Equal_TNT_Inventory_Selection;
                }
            }

            if (selectedSkuContract == null)
            {
                //no reflection here.. that's too slow and too much overhead
                _log.LogInformation("Returned NULL for Round Robin equal warehouse validation. Lastly, pull highest QTY validation..");
            }

            return selectedSkuContract;
        }

        /// <summary>
        /// Gets SKU when highest inventory
        /// </summary>
        /// <returns></returns>
        public SkuContract GetSkuContractByHigherInventory()
        {
            SkuContract higherInvContract = null;

            //if not skipped, pick highest
            if (!SkipAllocationByValidatePOReserve())
            {
                if (_decisionIndex != DecisionTreeIndex.Highest_Inventory_Equal_TNT_Selection
                && _decisionIndex != DecisionTreeIndex.Highest_Inventory_Equal_TNT_And_Zone_Selection)

                    _decisionIndex = DecisionTreeIndex.Highest_Inventory_Selection;

                var higherInv = _skus
                    .Where(a => a.WOHNorm >= _allocation.Quantity);

                higherInvContract = higherInv
                    .OrderByDescending(q => q.WOHNorm)
                    .FirstOrDefault();
            }

            return higherInvContract;
        }

        public void CreateReport(Guid batchId, SkuContract selectedSku, SkuContract alternateSku)
        {
            if (_isReporting)
            {
                var lou = _skus.Where(w => w.WHLocation.Equals("LOU")).FirstOrDefault();
                var dal = _skus.Where(w => w.WHLocation.Equals("DAL")).FirstOrDefault();

                var reportableResults = new ControlContract
                {
                    BatchId = batchId,
                    AllocationNumber = _allocation.AllocNum,
                    AllocationQuantity = _allocation.Quantity,
                    AllocationRunTime = DateTime.Now,
                    StoreNumber = _allocation.Store,
                    SKU = selectedSku.SkuNumber,
                    SelectedWarehouseOHQty = selectedSku.WOHNorm,
                    SelectedTnTWarehouse = selectedSku.WHLocation,
                    SelectedTnTValue = selectedSku.WhseTntContract.TimeInTransit,
                    AlternateTnTValue = alternateSku.WhseTntContract.TimeInTransit,
                    AlternateWarehouseOHQty = alternateSku.WOHNorm,
                    DecisionTreeIndex = (int)_decisionIndex,
                    LouTntValue = lou.WhseTntContract.TimeInTransit,
                    GV1TntValue = dal.WhseTntContract.TimeInTransit,
                    SelectedTnTZone = selectedSku.WhseTntContract.CarrierZone,
                    AlternateTntZone = alternateSku.WhseTntContract.CarrierZone,
                    SelectedWarehousePOQty = selectedSku.POQty,
                    AlternateWarehousePOQty = alternateSku.POQty
                };

                Task task = Task.Factory.StartNew(() =>
                {
                    _log.LogInformation($"Start Commmitting allocation results for allocation { _allocation.Sku} and store { _allocation.Store}");
                    IsResultCommitted(reportableResults);
                }).ContinueWith(a =>
                {
                    _log.LogInformation($"Results for allocation { _allocation.Sku} and store { _allocation.Store} committed successfully!");
                });
            }
        }

        /// <summary>
        /// Check pending POs to include when allocation does occur; determines if we should proceed with higher inventory or wait for PO on next day(s) allocation
        /// </summary>
        /// <returns></returns>
        public bool SkipAllocationByValidatePOReserve()
        {
            bool isSkipped = false;

            try
            {
                if (Convert.ToBoolean(_config.GetValue<string>("IsPOActive")))
                {
                    _log.LogInformation("Validating PO reserve quantity for lowest TNT warehouse");

                    if (_lowestTntSkuContract.WOHNorm < _allocation.Quantity)
                    {
                        if ((_lowestTntSkuContract.WOHNorm + _lowestTntSkuContract.POQty) >= _allocation.Quantity)
                        {
                            _log.LogInformation($"Allocation quantity [{_allocation.Quantity}] too much for warehouse {_lowestTntSkuContract.WHLocation} with qty {_lowestTntSkuContract.WOHNorm}." +
                                $" Pending PO qty {_lowestTntSkuContract.POQty} expected.  Allocation to be skipped.");

                            _decisionIndex = DecisionTreeIndex.Pending_PO_Fulfillment;

                            isSkipped = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _log.LogError(ex.Message);
                throw;
            }

            return isSkipped;
        }

        #endregion

        #region [ Public Accessors ]

        /// <summary>
        /// Is allocatable
        /// </summary>
        public bool IsAllocatable
        {
            get
            {
                return _isAllocatable;
            }
        }

        /// <summary>
        /// Alternate SKU TNT Contract
        /// </summary>
        public SkuContract AlternateTntSkuContract
        {
            get
            {
                return _alternateTntSkuContract;
            }
            set
            {
                _alternateTntSkuContract = value;
            }
        }

        /// <summary>
        /// Selected DecisionIndex for reporting
        /// </summary>
        public DecisionTreeIndex DecisionIndex
        {
            get
            {
                return _decisionIndex;
            }
            set
            {
                _decisionIndex = value;
            }
        }

        #endregion
    }
}
