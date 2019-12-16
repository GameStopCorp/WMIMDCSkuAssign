using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SupplyChain.Svc.MSDCSKUAssign.Contracts;
using SupplyChain.Svc.MSDCSKUAssign.Repositories.Interfaces;
using SupplyChain.Svc.MSDCSKUAssign.Services.Managers.Interfaces;
using SupplyChain.Svc.MSDCSKUAssign.Services.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SupplyChain.Svc.MSDCSKUAssign.Services.Managers
{
    public class SkuManager : ISkuManager<SkuContract>
    {
        #region [ Declarations ]

        private IConfiguration _config;
        private ILogger<SkuManager> _log;
        private IRepositorySvc<SkuContract> _skuRepository;
        private IPurchaseOrderManager<PurchaseOrderContract> _purchMgr;

        #endregion

        public SkuManager(IConfiguration config, ILogger<SkuManager> log, IRepositorySvc<SkuContract> skuRepository, IPurchaseOrderManager<PurchaseOrderContract> purchMgr)
        {
            _config = config;
            _log = log;
            _skuRepository = skuRepository;
            _purchMgr = purchMgr;
        }

        /// <summary>
        /// Returns all MUL summary records from INV1
        /// </summary>
        /// <returns></returns>
        public List<SkuContract> GetSummarySkuRecords()
        {
            _log.LogInformation("Pulling all INV1 records from GISMO... ");
            return _skuRepository.GetSummaries();
        }

        /// <summary>
        /// Returns offsite daily records for allocations, along with PO ASN data
        /// </summary>
        /// <returns></returns>
        public List<SkuContract> GetOffsiteDailyRecords()
        {
            _log.LogInformation("Pulling all Offsite Daily INV1 records from GISMO... ");

            var t3 = DateTime.Now;
            var currentSkus = _skuRepository.GetContracts();
            LoggerExtension.LogTimeDiff(t3, DateTime.Now, _log,
                        string.Format("@@@VS- Time taken to get contracts from SKU repository:"));

            if (Convert.ToBoolean(_config.GetValue<string>("IsPOActive")))
            {
                //returns all active purchase orders within date range
                var t5 = DateTime.Now;
                var purchOrders = _purchMgr.GetActivePurchaseOrders();
                LoggerExtension.LogTimeDiff(t5, DateTime.Now, _log,
                        string.Format("@@@VS- Time taken to get active purchase orders:"));

                if (purchOrders?.Any() == true)
                {
                    var relevantPurchOrders = from p in purchOrders
                                              join s in currentSkus on new { SKU = p.SKU, WH = p.PurchaseWarehouse }
                                              equals new { SKU = s.SkuNumber, WH = s.WHLocation }
                                              select new { p.PurchaseWarehouse, p.Ordered, p.PONum, s };

                    if (relevantPurchOrders.Any())
                    {
                        foreach (var p in relevantPurchOrders)
                        {
                            _log.LogInformation($"Adding PO# {p.PONum} Quantity {p.Ordered} to OnHand {p.s.WOHNorm} for SKU {p.s.SkuNumber} at whse: {p.PurchaseWarehouse}");

                            //p.s.WOHNorm += p.Ordered;
                            p.s.POQty += p.Ordered;
                        }
                    }
                    else
                    {
                        _log.LogInformation($"No pending purchase orders for relevant Multi-DC SKUs in any warehouse.");
                    }
                }
                else
                {
                    _log.LogInformation($"No purchase orders for any warehouses.");
                }
            }

            return currentSkus;
        }
    }
}
