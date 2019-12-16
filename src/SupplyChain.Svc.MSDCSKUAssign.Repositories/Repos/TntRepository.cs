using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SupplyChain.Svc.MSDCSKUAssign.Contracts;
using System.Data.SqlClient;
using System.Data;
using SupplyChain.Svc.MSDCSKUAssign.Repositories.Interfaces;
using SupplyChain.Svc.MSDCSKUAssign.Repositories.Extensions;

namespace SupplyChain.Svc.MSDCSKUAssign.Repositories.Repos
{
    public class TntRepository : IRepositorySvc<TNTContract>
    {
        #region [ Private Declarations ]

        private IConfiguration _config;
        private ILogger<TntRepository> _log;
        private IDbFactory _db;

        #endregion

        #region [ Setup and Instantiation ]

        public TntRepository(IConfiguration config, ILogger<TntRepository> log, IDbFactory db)
        {
            _config = config;
            _log = log;
            _db = db;
        }

        #endregion

        #region [ Public Methods ]

        /// <summary>
        /// Returns a list of all zones from PTL database
        /// </summary>
        /// <returns></returns>
        public List<TNTContract> GetContracts()
        {
            var contracts = new List<TNTContract>();

            var SP_contractsCommand = _config.GetValue<string>("TntStoredProc");
            
            try
            {                                
                using (var db = _db.GetDatabase("TntConnectionString"))
                {                    
                    using (var cmd = db.GetStoredProcCommand(SP_contractsCommand))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        //20190306 BH added query timeout
                        cmd.CommandTimeout = int.Parse(_config.GetValue<string>("QueryTimeout"));

                        var reader = cmd.ExecuteReader();

                        while (reader.Read())
                        {
                            contracts.Add(new TNTContract
                            {
                                StoreNumber = reader.Get<int>("storenbr"),
                                Origin = reader.Get<string>("origin"),
                                CarrierId = reader.Get<int>("carrierId"),
                                TimeInTransit = reader.Get<int>("timeintransit"),
                                CarrierZone = reader.Get<int>("carrierzone"),
                                CarrierZoneId = reader.Get<int>("carrierzoneid")
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

        public TNTContract Find()
        {
            throw new NotImplementedException();
        }

        public List<TNTContract> GetContractsByAllocNum(int allocationNumber)
        {
            throw new NotImplementedException();
        }

        public List<TNTContract> GetContractsByStore(int allocationNumber, int storeNumber)
        {
            throw new NotImplementedException();
        }

        public List<TNTContract> GetSummaries()
        {
            throw new NotImplementedException();
        }


        #endregion
    }
}
