using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SupplyChain.App.MDCSkuAssign.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace SupplyChain.App.MDCSkuAssign.Data
{
    public class PtlRepository : IPtlRepository<TntItem>
    {
        #region [ Declarations ]

        private ILogger<PtlRepository> _logger;
        private AppSettings _options;

        #endregion

        #region [ Instantiation ]

        public PtlRepository(ILogger<PtlRepository> logger, IOptions<AppSettings> options)
        {
            _logger = logger;
            _options = options.Value;
        }

        #endregion

        #region [ Public Methods ]

        /// <summary>
        /// Returns a list of Time In Transits
        /// </summary>
        /// <returns></returns>
        public List<TntItem> GetTimeInTransit()
        {
            var Tnts = new List<TntItem>();

            try
            {
                _logger.LogInformation($"Calling 'GetTimeInTransit' from PTL DB");

                using (var con = new SqlConnection(_options.PtlConnectionString))
                {
                    con.Open();

                    var cmd = new SqlCommand(_options.TNTCommand, con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    var reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        Tnts.Add(new TntItem
                        {
                            Store = reader.GetInt32(0),
                            Origin = reader.GetString(1),
                            CarrierId = reader.GetInt32(2),
                            TimeInTransit = reader.GetInt32(3),
                            CarrierZone = reader.GetInt32(4),
                            CarrierZoneId = reader.GetInt32(5)
                        });
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                throw;
            }

            return Tnts;
        }

        /// <summary>
        /// Returns a lookup list of KeyValue pairs based off AllocIFace Parent to assign order types for pick tickets.
        /// </summary>
        /// <returns></returns>
        public List<KeyValuePair<int, int>> AllocationOrderTypeLookup()
        {
            var orderTypes = new List<KeyValuePair<int, int>>();

            ///change this to an SP
            var commandStr = "SELECT [Store], [AType] FROM [dbo].[WMIAllocIFace]";

            try
            {
                _logger.LogInformation($"Calling 'WMIAllocIFaceParentData' from PTL DB");

                //using (var con = new SqlConnection(_options.PtlConnectionString))
                using(var con = new SqlConnection("Data Source = GV1HQPDB35A\\INST03, 5514; Initial Catalog = PTL; User ID = ptl; Password = ptl; Persist Security Info = True;"))
                {
                    con.Open();

                    var cmd = new SqlCommand(commandStr, con);
                    cmd.CommandType = CommandType.Text;

                    var reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        orderTypes.Add(new KeyValuePair<int, int>(key: reader.GetInt16(0), value: reader.GetByte(1)));
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                throw;
            }

            return orderTypes;
        }

        #endregion

    }
}
