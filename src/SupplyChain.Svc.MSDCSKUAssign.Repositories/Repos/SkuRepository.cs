using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SupplyChain.Svc.MSDCSKUAssign.Contracts;
using SupplyChain.Svc.MSDCSKUAssign.Repositories.Extensions;
using SupplyChain.Svc.MSDCSKUAssign.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Data.SqlClient;
using System.Linq;

namespace SupplyChain.Svc.MSDCSKUAssign.Repositories.Repos
{
    public class SkuRepository : IRepositorySvc<SkuContract>
    {
        #region [ Private Declarations ]

        private IConfiguration _config;
        private ILogger<SkuRepository> _log;
        private readonly IDbFactory _db;
        private string _connectionName;
        private string _SP_summaryCommand;
        private string _SP_contractsCommand;
        private string[] whses = new[] { "DAL", "LOU" };

        #endregion

        #region [ Setup and Instantiation ]

        public SkuRepository(IConfiguration config, ILogger<SkuRepository> log, IDbFactory db)
        {
            _config = config;
            _log = log;
            _db = db;

            var isPervasiveFlag = Convert.ToBoolean(_config.GetValue<string>("IsPervasiveActiveFlag"));
            _connectionName = isPervasiveFlag ? "Pervasive" : "SkuConnectionString";
            _SP_summaryCommand = isPervasiveFlag ? $"{_config.GetValue<string>("SkuStoredProc")}()" : _config.GetValue<string>("SkuStoredProc");
            _SP_contractsCommand = isPervasiveFlag ? $"{ _config.GetValue<string>("OffsiteDailyStoredProc")}()" : _config.GetValue<string>("OffsiteDailyStoredProc");

        }

        #endregion

        #region [ Public Methods ]           

        /// <summary>
        /// Get all offsite daily INV records for MUL SKUs 
        /// </summary>
        /// <returns></returns>
        public List<SkuContract> GetContracts()
        {
            var contracts = new List<SkuContract>();

            try
            {
                using (var db = _db.GetDatabase(_connectionName))
                {
                    using (var cmd = db.GetStoredProcCommand(_SP_contractsCommand))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        var reader = cmd.ExecuteReader();

                        while (reader.Read())
                        {
                            contracts.Add(new SkuContract
                            {
                                SkuNumber = reader.Get<string>("SKU"),
                                ItemNumber = reader.Get<int>("Item"),
                                WOHNorm = reader.Get<int>("OnHand"),
                                WHLocation = reader.Get<string>("WHCode"),
                                DailyRecord = reader.Get<DateTime>("InvDate"),
                                Weight = reader.Get<Double>("Weight")
                            });
                        }
                    }
                }

                //validate full warehouse equality for missing OffsiteInvDaily records
                IEnumerable<IGrouping<int, SkuContract>> skuGroups = contracts.GroupBy(i => i.ItemNumber);
                foreach (var skuGroup in skuGroups)
                {
                    var grp = from w in whses
                              join s in skuGroup on w equals s.WHLocation into missingDailies
                              from s in missingDailies.DefaultIfEmpty()
                              where s == null
                              select new SkuContract
                              {
                                  WHLocation = w,
                                  WOHNorm = 0,
                                  SkuNumber = skuGroup.First().SkuNumber,
                                  ItemNumber = skuGroup.First().ItemNumber,
                                  Title = skuGroup.First().Title,
                                  DailyRecord = skuGroup.First().DailyRecord,
                                  Weight = 0
                              };

                    if (grp.Any())
                    {
                        contracts.AddRange(grp);
                    }
                }
            }
            catch (Exception ex)
            {
                _log.LogError("Error in retrieving UPS routing data", null);
                throw ex;
            }

            return contracts;
        }

        /// <summary>
        /// Returns all INV1 MUL records
        /// </summary>
        /// <returns></returns>
        public List<SkuContract> GetSummaries()
        {
            var contracts = new List<SkuContract>();

            try
            {
                using (var db = _db.GetDatabase(_connectionName))
                {
                    using (var cmd = db.GetStoredProcCommand(_SP_summaryCommand))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        //20190306 BH Added query timeout
                        cmd.CommandTimeout=int.Parse(_config.GetValue<string>("QueryTimeout"));

                        var reader = cmd.ExecuteReader();

                        while (reader.Read())
                        {
                            contracts.Add(new SkuContract
                            {
                                SkuNumber = reader.Get<string>("SKU"),
                                ItemNumber = reader.Get<int>("Item"),
                                WOHNorm = reader.Get<int>("WOHNorm"),
                                WHLocation = reader.Get<string>("WHLocation"),
                                OnOrderWh = reader.Get<int>("OnOrderWh")
                            });
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                _log.LogError("Error in retrieving UPS routing data", null);
                throw ex;
            }

            return contracts;
        }

        #endregion

        #region [ Not Implemented Methods ]

        public List<SkuContract> GetContractsByAllocNum(int allocationNumber)
        {
            throw new NotImplementedException();
        }

        public List<SkuContract> GetContractsByStore(int allocationNumber, int storeNumber)
        {
            throw new NotImplementedException();
        }

        public SkuContract Find()
        {
            throw new NotImplementedException();
        }
       
        #endregion
    }
}
