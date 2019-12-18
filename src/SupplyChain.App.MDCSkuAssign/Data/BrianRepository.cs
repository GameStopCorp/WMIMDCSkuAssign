using GameStop.Common.Services.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SupplyChain.App.MDCSkuAssign.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;

namespace SupplyChain.App.MDCSkuAssign.Data
{
    public class BrianRepository : IBrianRepository<MasterAllocDetail>
    {
        #region [ Declarations ]

        private ILogger<BrianRepository> _logger;
        private AppSettings _options;
        private string _connectionString = string.Empty;

        #endregion

        #region [ Instantiation ]

        public BrianRepository(ILogger<BrianRepository> logger, IOptions<AppSettings> options)
        {
            _logger = logger;
            _options = options.Value;

            Setup();
        }

        /// <summary>
        /// Set up security.. prob needs a factory for this but for now, just hook it up
        /// </summary>
        public void Setup()
        {
            //var endpoint = _options.PervasiveSecEndpoint;

            //var wsBinding = new System.ServiceModel.WSHttpBinding();
            //var wsEndpoint = new System.ServiceModel.EndpointAddress(endpoint);

            //var serviceChannelFactory = new System.ServiceModel.ChannelFactory<ISecurityService>(wsBinding, wsEndpoint);
            ////var serviceChannelFactory = new System.ServiceModel.ChannelFactory<ISecurityService>(bscBinding, wsEndpoint);

            //ISecurityService client = null;

            try
            {
                var resource = new SQLResource
                {
                    DatabaseName = _options.PervasiveDBName,
                    ServerName = _options.PervasiveServer,
                    UserID = _options.PervasiveUser                   
                };
                //using UserID and pwd from config as of now.Since, WSHttpBinding not supported in .Netcore 3.0
                var PervasiveUserId = _options.PervasiveUserId;
                var PervasivePwd = _options.PervasivePwd;

                //client = serviceChannelFactory.CreateChannel(wsEndpoint);
                //GetCredentialResponse credentials = client.GetCredentials(new GetCredentialRequest("MDCSKU", resource.ServerName, _options.PervasiveIP, resource.UserID, resource));
                //((System.ServiceModel.ICommunicationObject)client).Close();

                //_connectionString = $"ServerName={resource.ServerName};Dsn={_options.PervasiveDns};uid={credentials.Credential.UserID};pwd={credentials.Credential.Password};";
                _connectionString = $"ServerName={resource.ServerName};Dsn={_options.PervasiveDns};uid={PervasiveUserId};pwd={PervasivePwd};";
            }
            catch (Exception ex)
            {
                //if (client != null)
                //{
                //    ((System.ServiceModel.ICommunicationObject)client).Abort();
                //}
                _logger.LogError(ex.Message);
                throw ex;
            }
        }

        #endregion

        #region [ Public Methods ]

        /// <summary>
        /// Pulls next batch number from Pervasive
        /// </summary>
        /// <returns></returns>
        public string GetNextBatchNumber()
        {
            var batchNumber = string.Empty;

            try
            {
                _logger.LogInformation($"Calling 'GetNextBatchNumber' from Pervavive DB");

                using (var con = new OdbcConnection(_connectionString))
                {
                    con.Open();

                    var cmd = new OdbcCommand($"CALL {_options.BatchNumCommand}()", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    batchNumber = cmd.ExecuteScalar().ToString();
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                throw;
            }

            return batchNumber;
        }

        /// <summary>
        /// Returns a collection of MasterAllocDetails
        /// </summary>
        /// <param name="whse"></param>
        /// <param name="allocNum"></param>
        /// <returns></returns>
        public List<MasterAllocDetail> GetAllocationDetails(string whse, string allocNum)
        {
            var details = new List<MasterAllocDetail>();

            try
            {
                _logger.LogInformation($"Calling 'GetAllocationDetails' from Pervavive DB");

                using (var con = new OdbcConnection(_connectionString))
                {
                    con.Open();

                    var cmd = new OdbcCommand($"CALL {_options.AllocDetailCommand}({allocNum}, '{whse}')", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        details.Add(new MasterAllocDetail
                        {
                            AllocNum = reader.GetInt32(0),
                            Store = reader.GetInt32(1),
                            Quantity = reader.GetInt32(2),
                            CombinedWeight = reader.GetInt32(3)
                        });
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                throw;
            }

            return details;
        }

        #endregion
    }
}
