using SupplyChain.Svc.MSDCSKUAssign.Contracts;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;
using SupplyChain.Svc.MSDCSKUAssign.Repositories.Interfaces;
using SupplyChain.Svc.MSDCSKUAssign.Repositories.Extensions;

namespace SupplyChain.Svc.MSDCSKUAssign.Repositories.Repos
{
    public class AllocationRepository : IRepositorySvc<AllocationContract>
    {
        #region [ Private Declarations ]

        private IConfiguration _config;
        private ILogger<AllocationRepository> _log;
        private readonly string _connectionStringName;
        private bool _isPervasive;
        private IDbFactory _db;

        #endregion

        #region [ Setup and Instantiation ]

        public AllocationRepository(IConfiguration config, ILogger<AllocationRepository> log, IDbFactory db)
        {
            _config = config;
            _log = log;
            _db = db;

            _isPervasive = Convert.ToBoolean(_config.GetValue<string>("IsPervasiveActiveFlag"));
            _connectionStringName = _isPervasive ? "Pervasive" : "SkuConnectionString";
        }

        #endregion

        #region [ Public Methods ]     

        public List<AllocationContract> GetContractsByStore(int allocationNumber, int itemNumber)
        {
            var contracts = new List<AllocationContract>();

            var commandSp = _config.GetValue<string>("AllocationDetailStoredProc");

            var SP_contractsCommand = _isPervasive ? $"{commandSp} ('{allocationNumber}', '{itemNumber}')" : commandSp;

            try
            {
                using (var db = _db.GetDatabase(_connectionStringName))
                {
                    using (var cmd = db.GetStoredProcCommand(SP_contractsCommand))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        db.AddInParameter(cmd, "allocnum", DbType.Int32, allocationNumber);
                        db.AddInParameter(cmd, "item", DbType.Int32, itemNumber);

                        var reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            contracts.Add(new AllocationContract
                            {
                                Sku = reader.Get<string>("sku"),
                                Item = reader.Get<int>("item"),
                                AllocNum = allocationNumber.ToString(),
                                Quantity = reader.Get<Int16>("aqty"),
                                Price = reader.Get<int>("rprice"),
                                Parstype = reader.Get<int>("parstype"),
                                Rfm = reader.Get<int>("rfm"),
                                Margin = reader.Get<int>("margin"),
                                CarryForwardDays = reader.Get<int>("carryforwarddays"),
                                OutofStock = reader.Get<string>("outofstock"),
                                Velocity = reader.IsDBNull(9) ? string.Empty : reader.Get<string>("velocity"),
                                Store = reader.Get<Int16>("store")
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _log.LogError("Error in retrieving Pervasive allocation data", null);
                throw ex;
            }

            return contracts;
        }

        public List<AllocationContract> GetContractsByAllocNum(int allocationNumber)
        {
            var contracts = new List<AllocationContract>();
            var commandSp = "SCS_Get_AllocationData_SPI_WMIMDCSkuAssignAll";
            var SP_contractsCommand = _isPervasive ? $"{commandSp} ('{allocationNumber}')" : commandSp;

            try
            {
                using (var db = _db.GetDatabase(_connectionStringName))
                {
                    using (var cmd = db.GetStoredProcCommand(SP_contractsCommand))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        db.AddInParameter(cmd, "allocnum", DbType.Int32, allocationNumber);

                        var reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            contracts.Add(new AllocationContract
                            {
                                Sku = reader.Get<string>("sku"),
                                Item = reader.Get<int>("item"),
                                AllocNum = allocationNumber.ToString(),
                                Quantity = reader.Get<Int16>("aqty"),
                                Price = reader.Get<int>("rprice"),
                                Parstype = reader.Get<int>("parstype"),
                                Rfm = reader.Get<int>("rfm"),
                                Margin = reader.Get<int>("margin"),
                                CarryForwardDays = reader.Get<int>("carryforwarddays"),
                                OutofStock = reader.Get<string>("outofstock"),
                                Velocity = reader.IsDBNull(9) ? string.Empty : reader.Get<string>("velocity"),
                                Store = reader.Get<Int16>("store")
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _log.LogError("Error in retrieving Pervasive allocation data", null);
                throw ex;
            }

            return contracts;
        }

        #endregion

        #region [ Not Implemented Methods ]

        /// <summary>
        /// Returns a list of SKU contracts
        /// </summary>
        /// <returns></returns>
        public List<AllocationContract> GetContracts()
        {
            throw new NotImplementedException();
        }

        public List<AllocationContract> GetSummaries()
        {
            throw new NotImplementedException();
        }

        public AllocationContract Find()
        {
            throw new NotImplementedException();
        }

        #endregion
    }

}
