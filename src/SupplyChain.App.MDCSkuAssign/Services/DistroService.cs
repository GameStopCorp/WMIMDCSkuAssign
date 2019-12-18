using SupplyChain.App.MDCSkuAssign.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;
using SupplyChain.App.MDCSkuAssign.Entities;
using Microsoft.Extensions.Logging;
using SupplyChain.App.MDCSkuAssign.Data;
using Microsoft.Extensions.Options;

namespace SupplyChain.App.MDCSkuAssign.Services
{
    public class DistroService : IDistroService<Allocation>
    {
        #region [ Declarations ]

        private readonly ILogger<PickTicketService> _logger;
        private AppSettings _options { get; }

        private IRepository<Allocation> _db;

        #endregion

        #region [ Instantiation ]

        public DistroService(ILogger<PickTicketService> logger, IOptions<AppSettings> options, IRepository<Allocation> db)
        {
            _logger = logger;
            _options = options.Value;
            _db = db;
        }

        #endregion

        public void Create(List<Allocation> allocations, string whse)
        {
            if (allocations.Any())
            {
                var allocationCount = allocations.Count;

                _logger.LogInformation($"Initializing 'CreateDistro' on {allocationCount} allocations for {whse}");

                foreach (var allocation in allocations)
                {
                    _logger.LogInformation($"Starting distro service queue for allocation number: {allocation.AllocNum} and SKU: {allocation.Sku}.");

                    var previousRecords = _db.HasPreviousRecord(allocation);

                    if (previousRecords)
                    {
                        _logger.LogWarning($"Found duplicate insert on restart allocnum/store/sku:  {allocation.AllocNum} /{ allocation.Store} /{allocation.Sku}");

                        //skip since it's already in there.
                        allocation.isProcessed = true;
                    }
                    else
                    {
                        allocation.isProcessed = _db.AddDistro(allocation, whse);
                    }

                    _logger.LogInformation($"Distro created for allocation number: {allocation.AllocNum} and SKU: {allocation.Sku}.");
                }

                _logger.LogInformation("Processing Completed");
            }
        }
    }
}
