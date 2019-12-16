using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SupplyChain.App.MDCSkuAssign.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Linq;

namespace SupplyChain.App.MDCSkuAssign.Data
{
    public class WMRepository : IRepository<Allocation>
    {
        #region [ Declarations ]

        private ILogger<WMRepository> _logger;
        private AppSettings _options;

        #endregion

        #region [ Instantiation ]

        public WMRepository(ILogger<WMRepository> logger, IOptions<AppSettings> options)
        {
            _logger = logger;
            _options = options.Value;
        }

        #endregion

        #region [ Public Methods ]

        /// <summary>
        /// Adds a distro record in IGINPT00
        /// </summary>
        /// <param name="contract"></param>
        /// <param name="whse"></param>
        /// <returns></returns>
        public bool AddDistro(Allocation contract, string whse)
        {
            int rowAffected = 0;
            try
            {
                _logger.LogInformation($"Calling 'AddDistro' for {contract.AllocNum} and {contract.Sku} into IGINPT00 for warehouse {whse}. ");

                if (contract == null)
                    throw new ArgumentNullException("a valid contract must be supplied");

                var connectionString = _options.WmConnectionString.Replace("{WMLib}", $"{whse}DC");
                _logger.LogInformation($"Log connection string for Distros: {connectionString}. ");

                using (OdbcConnection conn = new OdbcConnection(connectionString))
                {
                    conn.Open();

                    var cmd = new OdbcCommand(_options.DistroCommand, conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = _options.DistroCommand;

                    var trans = conn.BeginTransaction();
                    cmd.Transaction = trans;

                    cmd.Parameters.AddWithValue("CreateDate", DateTime.Now.ToString("yyyyMMdd"));
                    cmd.Parameters.AddWithValue("CreateTime", DateTime.Now.ToString("hhmmss"));
                    cmd.Parameters.AddWithValue("UserId", "ALLOC");
                    cmd.Parameters.AddWithValue("WHLocation", contract.Warehouse);
                    cmd.Parameters.AddWithValue("OrderSfx", _options.StoreOrderType);
                    cmd.Parameters.AddWithValue("DistroType", _options.DistroType);
                    cmd.Parameters.AddWithValue("DistroNbr", string.Format("{0}{1}", "000", contract.AllocNum.ToString()));
                    cmd.Parameters.AddWithValue("StoreNbr", contract.Store);
                    cmd.Parameters.AddWithValue("Company", contract.Warehouse);
                    cmd.Parameters.AddWithValue("Division", contract.Warehouse);
                    cmd.Parameters.AddWithValue("Style", contract.Sku);
                    cmd.Parameters.AddWithValue("ShipVia", string.Empty);
                    cmd.Parameters.AddWithValue("SKUAttribute", _options.SKUAttrib1);
                    cmd.Parameters.AddWithValue("InventoryType", _options.InventoryType);
                    cmd.Parameters.AddWithValue("CountryOfOrigin", string.Empty);
                    cmd.Parameters.AddWithValue("Price", _options.WholesalePrice);
                    cmd.Parameters.AddWithValue("RetailPrice", contract.Price / 100);
                    cmd.Parameters.AddWithValue("ReqQuantity", contract.Quantity);
                    cmd.Parameters.AddWithValue("StatusFlag", _options.DistroStatusFlag);
                    cmd.Parameters.AddWithValue("OrderType", string.Empty);
                    cmd.Parameters.AddWithValue("NewFlag", string.Empty);
                    cmd.Parameters.AddWithValue("Func", string.Empty);
                    cmd.Parameters.AddWithValue("ServiceLevel", string.Empty);
                    cmd.Parameters.AddWithValue("ParsType", contract.Parstype);
                    cmd.Parameters.AddWithValue("Rfm", contract.Rfm);//decimal
                    cmd.Parameters.AddWithValue("Margin", contract.Margin); //decimal
                    cmd.Parameters.AddWithValue("CarryForwardDays", contract.CarryForwardDays);//decimal
                    cmd.Parameters.AddWithValue("OutOfStock", contract.OutofStock);
                    cmd.Parameters.AddWithValue("Velocity", contract.Velocity);

                    var c = cmd.ExecuteNonQuery();

                    trans.Commit();
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                throw e;
            }

            return rowAffected != -1;
        }

        /// <summary>
        /// Adds a detail record in I2INPT00
        /// </summary>
        /// <param name="contract"></param>
        /// <param name="whse"></param>
        /// <param name="batchNumber"></param>
        /// <param name="pickLineNum"></param>
        /// <returns></returns>
        public bool AddDetail(Allocation contract, string whse, string batchNumber, int pickLineNum)
        {
            int rowAffected = 0;

            try
            {
                _logger.LogInformation($"Calling 'AddDetail' for {contract.AllocNum} and {contract.Sku} into I2INPT00. ");

                if (contract == null)
                    throw new ArgumentNullException("a valid contract must be supplied");

                var connectionString = _options.WmConnectionString.Replace("{WMLib}", $"{whse}DC");

                using (OdbcConnection conn = new OdbcConnection(connectionString))
                {
                    conn.Open();

                    var cmd = new OdbcCommand(_options.PickTicketDetailCommand, conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = _options.PickTicketDetailCommand;

                    var trans = conn.BeginTransaction();
                    cmd.Transaction = trans;

                    cmd.Parameters.AddWithValue("Company", _options.PickTicketCompany);
                    cmd.Parameters.AddWithValue("Division", _options.DistroDivision);
                    cmd.Parameters.AddWithValue("PickControNbr", string.Format("{0}{1}", contract.AllocNum.PadLeft(6, '0'), contract.Store.ToString().PadLeft(4, '0')));
                    cmd.Parameters.AddWithValue("BatchCtlNum", batchNumber);
                    cmd.Parameters.AddWithValue("PickLineNbr", pickLineNum);
                    cmd.Parameters.AddWithValue("Warehouse", contract.Warehouse);
                    cmd.Parameters.AddWithValue("Style", contract.Sku);
                    cmd.Parameters.AddWithValue("SKUAttribute1", _options.SKUAttrib1);
                    cmd.Parameters.AddWithValue("PickQty", contract.Quantity);
                    cmd.Parameters.AddWithValue("CartonType", _options.CartonType);
                    cmd.Parameters.AddWithValue("InventoryType", _options.InventoryType);

                    rowAffected = cmd.ExecuteNonQuery();

                    trans.Commit();
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                throw e;
            }

            return rowAffected != -1;
        }

        /// <summary>
        /// Adds a header to I1INPT00 in WM
        /// </summary>
        /// <param name="contract"></param>
        /// <param name="whse"></param>
        /// <param name="tntvalue"></param>
        /// <param name="batchNum"></param>
        /// <returns></returns>
        public bool AddHeader(Allocation contract, string whse, int tntvalue, string batchNum, int estWeight)
        {
            int rowAffected = 0;

            try
            {
                _logger.LogInformation($"Calling 'AddHeader' for {contract.AllocNum} and {contract.Sku} into I1INPT00. ");

                if (contract == null)
                    throw new ArgumentNullException("a valid contract must be supplied");

                var connectionString = _options.WmConnectionString.Replace("{WMLib}", $"{whse}DC");

                using (OdbcConnection conn = new OdbcConnection(connectionString))
                {
                    conn.Open();

                    var cmd = new OdbcCommand(string.Empty, conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = _options.PickTicketUpdateCommand;

                    var trans = conn.BeginTransaction();
                    cmd.Transaction = trans;

                    cmd.Parameters.AddWithValue("Company", _options.DistroCompany);
                    cmd.Parameters.AddWithValue("Division", _options.DistroDivision);
                    cmd.Parameters.AddWithValue("PickControNbr", string.Format("{0}{1}", contract.AllocNum.PadLeft(6, '0'), contract.Store.ToString().PadLeft(4, '0')));
                    cmd.Parameters.AddWithValue("BatchCtlNum", batchNum); //comes from BRIAN
                    cmd.Parameters.AddWithValue("Warehouse", whse);
                    cmd.Parameters.AddWithValue("AllocNbrStoreNbr", string.Format("{0}{1}", contract.AllocNum.PadLeft(6, '0'), contract.Store.ToString().PadLeft(4, '0')));
                    cmd.Parameters.AddWithValue("StoreNbr", contract.Store.ToString().PadLeft(4, '0'));
                    cmd.Parameters.AddWithValue("currdate", DateTime.Now.ToString("yyyyMMdd"));
                    cmd.Parameters.AddWithValue("OrderSfx", _options.OrderSfx);
                    cmd.Parameters.AddWithValue("OrderType", _options.OrderTypeRST);
                    cmd.Parameters.AddWithValue("StoreNbr480", contract.Store == 480 ? string.Empty : contract.Store.ToString().PadLeft(4, '0'));
                    cmd.Parameters.AddWithValue("PreStickerCode", _options.PreStickerCode);
                    cmd.Parameters.AddWithValue("PickticketStatus", _options.PickTicketStatus);
                    cmd.Parameters.AddWithValue("EstWeight", estWeight); //is this needed?
                    cmd.Parameters.AddWithValue("BOLType", _options.BOLType);
                    cmd.Parameters.AddWithValue("SKU100PctInventory", _options.SKU100PctInv);
                    cmd.Parameters.AddWithValue("ServiceLevel", string.Empty);
                    cmd.Parameters.AddWithValue("shipvia", string.Empty);
                    cmd.Parameters.AddWithValue("newflag", string.Empty);
                    cmd.Parameters.AddWithValue("MiscIns5Byte2", tntvalue);
                    cmd.Parameters.AddWithValue("MiscIns10Byte1", "");
                    cmd.Parameters.AddWithValue("MiscIns10Byte2", "");
                    cmd.Parameters.AddWithValue("MiscIns20Byte1", "");
                    cmd.Parameters.AddWithValue("MiscIns20Byte2", "");
                    cmd.Parameters.AddWithValue("MiscIns20Byte3", "");
                    cmd.Parameters.AddWithValue("MiscIns20Byte4", "");
                    cmd.Parameters.AddWithValue("SED", _options.SEDPickticket);

                    rowAffected = cmd.ExecuteNonQuery();
                    trans.Commit();
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                throw e;
            }

            return rowAffected != -1;
        }

        /// <summary>
        /// Determines if there's a previous record in IGINPT00
        /// </summary>
        /// <param name="allocation"></param>
        /// <returns></returns>
        public bool HasPreviousRecord(Allocation allocation)
        {
            bool hasRecords = false;
            try
            {
                var distroString = "SELECT DTDSTR, DTSTOR, DTSTYL FROM IGINPT00 WHERE DTDSTR = '" +
                                    string.Concat("000", allocation.AllocNum.ToString()) + "' and " +
                                    " dtstor='" + allocation.Store + "' and dtstyl='" + allocation.Sku + "' and dtwhse='" + allocation.Warehouse + "'";

                var connectionString = _options.WmConnectionString.Replace("{WMLib}", $"{allocation.Warehouse}DB");
                using (OdbcConnection conn = new OdbcConnection(connectionString))
                {
                    conn.Open();

                    var cmd = new OdbcCommand(string.Empty, conn);
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = distroString;

                    var reader = cmd.ExecuteReader();
                    var records = new List<Allocation>();

                    while (reader.Read())
                    {
                        records.Add(new Allocation
                        {
                            AllocNum = reader.GetString(0),
                            Store = reader.GetInt32(1),
                            Sku = reader.GetString(2)
                        });
                    }

                    hasRecords = records.Any();
                }

            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                throw e;
            }

            return hasRecords;
        }

        /// <summary>
        /// On completion of allocation, update all non-multi pick tickets
        /// </summary>
        /// <returns></returns>
        public bool UpdateWmPickTicketStatuses(string whse)
        {
            int rowAffected = 0;

            try
            {
                _logger.LogInformation($"Calling 'UpdateWmPickTicketStatuses'");

                var connectionString = _options.WmConnectionString.Replace("{WMLib}", $"{whse}DC");

                using (OdbcConnection conn = new OdbcConnection(connectionString))
                {
                    conn.Open();

                    var cmd = new OdbcCommand(_options.PickTicketReleaseCommand, conn);
                    cmd.CommandType = CommandType.StoredProcedure;

                    var trans = conn.BeginTransaction();
                    cmd.Transaction = trans;
                    rowAffected = cmd.ExecuteNonQuery();

                    trans.Commit();
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"Error releasing pick tickets: {e.Message}");
                throw e;
            }

            return rowAffected != -1;
        }

        #endregion

    }
}
