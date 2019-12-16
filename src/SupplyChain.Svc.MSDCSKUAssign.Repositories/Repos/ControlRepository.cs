using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SupplyChain.Svc.MSDCSKUAssign.Contracts;
using SupplyChain.Svc.MSDCSKUAssign.Repositories.Extensions;
using SupplyChain.Svc.MSDCSKUAssign.Repositories.Interfaces;
using System;
using System.Data;
using System.Data.SqlClient;

namespace SupplyChain.Svc.MSDCSKUAssign.Repositories.Repos
{
    /// <summary>
    /// Repository to control flow of the application, update to flow parameters, percentages and runtimes.
    /// </summary>
    public class ControlRepository : IControlRepository<ControlContract, SkuContract>
    {
        #region [ Declarations ]

        private IConfiguration _config;
        private ILogger<ControlRepository> _log;
        private IDbFactory _db;

        #endregion

        #region [ Instantiation ]

        public ControlRepository(IConfiguration config, ILogger<ControlRepository> log, IDbFactory db)
        {
            _log = log;
            _config = config;
            _db = db;
        }

        #endregion

        #region [ Public Methods ]

        /// <summary>
        /// Updates values in Offsite INV Daily for Mock
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public bool IsUpdatedData(DateTime date)
        {
            int result;

            var SP_UpdateDataCommand = _config.GetValue<string>("OffsiteDailyStageProc");

            try
            {
                using (var db = _db.GetDatabase("SkuConnectionString"))
                {
                    using (var cmd = db.GetStoredProcCommand(SP_UpdateDataCommand))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        db.AddInParameter(cmd, "currDate", DbType.DateTime, date);
                        result = cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                _log.LogError("Error:  Could not update on hand values for OffsiteInvDaily.", null);
                throw ex;
            }

            return result < 0;
        }

        /// <summary>
        /// Commit Results contract back to monitor DB
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public bool Commit(ControlContract contract)
        {
            int result;

            try
            {
                var SP_UpdateDataCommand = _config.GetValue<string>("AllocationResultsProc");

                using (var db = _db.GetDatabase("SkuConnectionString"))
                {
                    using (var cmd = db.GetStoredProcCommand(SP_UpdateDataCommand))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        db.AddInParameter(cmd, "batchId", DbType.Guid, contract.BatchId);
                        db.AddInParameter(cmd, "allocationNum", DbType.Int32, contract.AllocationNumber);
                        db.AddInParameter(cmd, "allocationQty", DbType.Int32, contract.AllocationQuantity);
                        db.AddInParameter(cmd, "allocationRunTime", DbType.DateTime, contract.AllocationRunTime);
                        db.AddInParameter(cmd, "sku", DbType.String, contract.SKU);
                        db.AddInParameter(cmd, "storeNum", DbType.Int32, contract.StoreNumber);
                        db.AddInParameter(cmd, "selectedWhseQty", DbType.Int32, contract.SelectedWarehouseOHQty);
                        db.AddInParameter(cmd, "selectedTnTWhse", DbType.String, contract.SelectedTnTWarehouse);
                        db.AddInParameter(cmd, "selectedTnTVal", DbType.Int32, contract.SelectedTnTValue);
                        db.AddInParameter(cmd, "altTntVal", DbType.Int32, contract.AlternateTnTValue);
                        db.AddInParameter(cmd, "altWhseQty", DbType.Int32, contract.AlternateWarehouseOHQty);
                        db.AddInParameter(cmd, "decisionIndex", DbType.Int32, contract.DecisionTreeIndex);
                        db.AddInParameter(cmd, "louTnt", DbType.Int32, contract.LouTntValue);
                        db.AddInParameter(cmd, "gv1Tnt", DbType.Int32, contract.GV1TntValue);
                        db.AddInParameter(cmd, "selectZone", DbType.Int32, contract.SelectedTnTZone);
                        db.AddInParameter(cmd, "altZone", DbType.Int32, contract.AlternateTntZone);
                        db.AddInParameter(cmd, "poQty", DbType.Int32, contract.SelectedWarehousePOQty);
                        db.AddInParameter(cmd, "altpoQty", DbType.Int32, contract.AlternateWarehousePOQty);

                        result = cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                _log.LogError("Error:  Could not update on hand values for allocation results.", null);
                throw ex;
            }

            return result < 0;
        }

        /// <summary>
        /// Upsert staging data to INV1 from daily allocation
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool IsUpsertedStageInvData(SkuContract contract)
        {
            int result;

            var SP_stageCommand = _config.GetValue<string>("GismoStageProc");

            try
            {
                using (var db = _db.GetDatabase("SkuConnectionString"))
                {
                    using (var cmd = db.GetStoredProcCommand(SP_stageCommand))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        db.AddInParameter(cmd, "item", DbType.Int32, contract.ItemNumber);
                        db.AddInParameter(cmd, "sku", DbType.String, contract.SkuNumber);
                        db.AddInParameter(cmd, "title", DbType.String, contract.Title);
                        db.AddInParameter(cmd, "wOHNorm", DbType.Int32, contract.WOHNorm);

                        result = cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                _log.LogError("Error:  Could not update on hand values for allocation results.", null);
                throw ex;
            }

            return result < 0;
        }

        public SkuContract GetPervasiveItemBySku(string sku)
        {
            var SP_stageCommand = $"{_config.GetValue<string>("GismoStagePullProc")} ('{sku}') ";

            var contract = new SkuContract();

            try
            {
                using (var db = _db.GetDatabase("Pervasive"))
                {
                    using (var cmd = db.GetStoredProcCommand(SP_stageCommand))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        var reader = cmd.ExecuteReader();

                        while (reader.Read())
                        {
                            contract = new SkuContract
                            {
                                SkuNumber = reader.Get<string>("SKU"),
                                ItemNumber = reader.Get<int>("ItemNumber"),
                                WOHNorm = reader.Get<int>("QtyOnHand"),
                                WHLocation = reader.Get<string>("AllocationWarehouse"),
                                Title = reader.Get<string>("Title"),
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _log.LogError($"Error: SKU {sku} could not retrieve values from Pervasive for INV1.", null);
                throw ex;
            }

            return contract;
        }

        #endregion
    }
}


