using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SupplyChain.Svc.MSDCSKUAssign.Contracts;
using SupplyChain.Svc.MSDCSKUAssign.Repositories.Extensions;
using SupplyChain.Svc.MSDCSKUAssign.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace SupplyChain.Svc.MSDCSKUAssign.Repositories.Repos
{
    public class PurchaseOrderRepository : IRepositorySvc<PurchaseOrderContract>
    {
        #region [ Declarations ]

        private IConfiguration _config;
        private ILogger<PurchaseOrderRepository> _log;
        private const string _connectionStringName = "POConnectionString";
        private IDbFactory _db;

        #endregion

        #region [ Setup and Instantiation ]

        public PurchaseOrderRepository(IConfiguration config, ILogger<PurchaseOrderRepository> log, IDbFactory db)
        {
            _config = config;
            _log = log;
            _db = db;
        }

        #endregion

        #region [ Public Methods ]

        /// <summary>
        /// Returns all open POs that have an ETA within the next {x} days 
        /// </summary>
        /// <returns></returns>
        public List<PurchaseOrderContract> GetContracts()
        {
            var contracts = new List<PurchaseOrderContract>();

            var SP_contractsCommand = _config.GetValue<string>("GSMStoredProc");
            var etaDayOffset = Int32.Parse(_config.GetValue<string>("PO_ETADayOffset"));
            var reverseEtaDayOffset = Int32.Parse(_config.GetValue<string>("Reverse_PO_ETADayOffset"));

            var tempCmd = GetTempCommand();

            try
            {
                using (var db = _db.GetDatabase(_connectionStringName))
                {
                    //using (var cmd = db.GetStoredProcCommand(SP_contractsCommand))
                    using (var cmd = db.GetStringCommand(tempCmd))
                    {
                        cmd.CommandType = CommandType.Text;

                        var reader = cmd.ExecuteReader();

                        while (reader.Read())
                        {
                            var po = new PurchaseOrderContract
                            {
                                PONum = reader.Get<int>("PONum"),
                                Vendor = reader.Get<int>("VendorNum"),
                                POStatus = reader.Get<int>("PO_Status"),
                                SKUStatus = reader.Get<int>("SKU_Status"),
                                PO_ETA = reader.Get<DateTime>("PO_ETA"),
                                SKU_ETA = reader.Get<DateTime>("SKU_ETA"),
                                DateOrdered = reader.Get<DateTime>("OrderDate"),
                                PODateCanceled = reader.Get<DateTime>("PO_Cancel_Date"),
                                SKUDateCanceled = reader.Get<DateTime>("SKU_Cancel_Date"),
                                PurchaseWarehouse = reader.Get<string>("PurchWH"),
                                POLoadDate = reader.Get<DateTime>("PO_Load_Date"),
                                SKULoadDate = reader.Get<DateTime>("SKU_Load_Date"),
                                LineItem = reader.Get<int>("LnItem"),
                                SKU = reader.Get<int>("SKU").ToString(),
                                Ship = reader.Get<Int16>("Ship"),
                                Ordered = reader.Get<int>("Ordered"),
                                Stocked = reader.Get<int>("Stocked"),
                                Price = reader.Get<decimal>("Price"),
                                FirstRcvd = reader.Get<DateTime>("FirstRcvd"),
                                LastRcvd = reader.Get<DateTime>("LastRcvd"),
                            };

                            if (po.PO_ETA <= DateTime.Now.Date.AddDays(etaDayOffset) && po.PO_ETA >= DateTime.Now.Date.AddDays(reverseEtaDayOffset * -1))
                            {
                                _log.LogDebug($"Captured PO for SKU {po.SKU} that has ETA of {po.SKU_ETA}");
                                contracts.Add(po);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _log.LogError("Error receiving PO headers from GSDM", null);
                throw ex;
            }

            return contracts;
        }

        /// <summary>
        /// Temporary until the SP is added to GSDM
        /// </summary>
        /// <returns></returns>
        private string GetTempCommand()
        {
            return "SELECT    H.[PONum] " +
                      ",[VendorNum] " +
                      ",H.[Status] AS PO_Status " +
                      ", D.[Status] AS SKU_Status " +
                      ", H.[ETA] AS PO_ETA " +
                      ", D.[ETA] AS SKU_ETA " +
                      ",[OrderDate] " +
                      ", H.[CancelDate] AS PO_Cancel_Date " +
                      ", D.[CancelDate] AS SKU_Cancel_Date " +
                      ",[PurchWH] " +
                      ", H.[LoadDate] AS PO_Load_Date " +
                      ",D.[LoadDate] AS SKU_Load_Date " +
                      ",[LnItem] " +
                      ",[SKU] " +
                      ",[Ship] " +
                      ",[Ordered] " +
                      ",[Stocked] " +
                      ",[Price] " +
                      ",[FirstRcvd] " +
                      ",[LastRcvd] " +
               " FROM[DM].[PO_Header] " +
               "H" +
               " INNER JOIN [DM].[PO_Detail] " +
                " D" +
             " ON H.PONum = D.PONum " +
            " WHERE H.[Status] IN(3, 4, 7) AND H.[ETA] >= GETDATE()";
        }

        #endregion

        #region [ Not Implemented Methods ]

        public List<PurchaseOrderContract> GetContractsByAllocNum(int allocationNumber)
        {
            throw new NotImplementedException();
        }

        public List<PurchaseOrderContract> GetContractsByStore(int allocationNumber, int itemNumber)
        {
            throw new NotImplementedException();
        }

        public List<PurchaseOrderContract> GetSummaries()
        {
            throw new NotImplementedException();
        }

        public PurchaseOrderContract Find()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
