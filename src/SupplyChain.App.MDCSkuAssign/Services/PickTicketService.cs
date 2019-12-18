using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SupplyChain.App.MDCSkuAssign;
using SupplyChain.App.MDCSkuAssign.Data;
using SupplyChain.App.MDCSkuAssign.Entities;
using SupplyChain.App.MDCSkuAssign.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SupplyChain.App.MDCSkuAssign.Services
{
    public class PickTicketService : IPickTicketService<Allocation>
    {
        #region [ Declarations ]

        private readonly ILogger<PickTicketService> _logger;
        private AppSettings _options { get; }
        private IRepository<Allocation> _db;
        private IPtlRepository<TntItem> _tnt;
        private IBrianRepository<MasterAllocDetail> _pervasive;
        private List<MasterAllocDetail> _storeAllocDetails;

        #endregion

        #region [ Instantiation ]

        public PickTicketService(ILogger<PickTicketService> logger, IOptions<AppSettings> options,
            IRepository<Allocation> db, IPtlRepository<TntItem> tnt, IBrianRepository<MasterAllocDetail> pervasive)
        {
            _logger = logger;
            _options = options.Value;
            _db = db;
            _tnt = tnt;
            _pervasive = pervasive;
        }

        #endregion

        #region [ Public Methods ]

        /// <summary>
        /// Creates pick tickets for allocations
        /// </summary>
        /// <param name="allocations"></param>
        public void Create(List<Allocation> allocations, string whse)
        {
            try
            {
                _logger.LogInformation($"Initializing 'CreatePickTicket' on all allocations for {whse}.");

                var transits = _tnt.GetTimeInTransit();
                int? tntValue = 0;

                var batchNumber = _pervasive.GetNextBatchNumber();

                //group allocations by store
                var allocationByStore = allocations.GroupBy(k => k.Store).ToDictionary(g => g.Key, g => g.ToList());

                foreach (var storeAllocation in allocationByStore)
                {
                    int estWeight = 0;
                    for (int i = 0; i < storeAllocation.Value.Count; i++)
                    {
                        var allocation = storeAllocation.Value[i];
                        estWeight += Convert.ToInt32(allocation.Quantity * allocation.Weight);

                        _logger.LogInformation($"Starting pick ticket service queue for allocation number: {allocation.AllocNum} and SKU: {allocation.Sku} at store {storeAllocation.Key}.");

                        var tnt = transits.Where(t => t.Origin.Equals(whse)
                        && t.Store.Equals(allocation.Store)).FirstOrDefault();

                        if (tnt == null)
                            _logger.LogInformation($"No TNT found for {allocation.Store} from DC {whse}. ");

                        tntValue = tnt?.TimeInTransit;
                        var picklineNum = _options.PickLineSeqNum + (i + 1);

                        allocation.isProcessed = _db.AddDetail(allocation, whse, batchNumber, picklineNum);
                        _logger.LogInformation($"Pick ticket created for allocation number: {allocation.AllocNum} and SKU: {allocation.Sku} for store {storeAllocation.Key}.");
                    }

                    var firstAllocation = storeAllocation.Value.FirstOrDefault();

                   _db.AddHeader(firstAllocation, whse, tntValue.Value, batchNumber, estWeight);
                }

            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                throw e;
            }

            _logger.LogInformation("Updated all picket ticket statuses non-MUL related in I1NPT00 for processing");
        }

        /// <summary>
        /// Release all picktickets from input tables
        /// </summary>
        public bool Release()
        {
            return _db.UpdateWmPickTicketStatuses("LOU");
        }

        #endregion
    }
}
