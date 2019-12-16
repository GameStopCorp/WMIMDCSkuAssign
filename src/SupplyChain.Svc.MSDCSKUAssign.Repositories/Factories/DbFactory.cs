using GameStop.Common.Services.Security;
using Microsoft.Extensions.Configuration;
using SupplyChain.Svc.MSDCSKUAssign.Repositories.Interfaces;
using System;

namespace SupplyChain.Svc.MSDCSKUAssign.Repositories.Factories
{
    public class DbFactory : IDbFactory
    {
        #region [ Private Properties ]

        private readonly IConfiguration _config;
        private readonly bool _isPervasiveActiveFlag;
        private string _pervasiveConnnectionString;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="appSettingsManager"></param>
        /// <param name="connectionManager"></param>
        public DbFactory(IConfiguration config)
        {
            if (config == null)
                throw new ArgumentNullException("configuration");

            _config = config;
            _isPervasiveActiveFlag = Convert.ToBoolean(_config.GetValue<string>("IsPervasiveActiveFlag"));

            SetupPervasive();
        }

        #endregion

        #region [ Private Methods ]

        private void SetupPervasive()
        {
            //var endpoint = _config.GetValue<string>("PervasiveSecEndpoint");

            //var wsBinding = new System.ServiceModel.WSHttpBinding();
            //var wsEndpoint = new System.ServiceModel.EndpointAddress(endpoint);
            //var serviceChannelFactory = new System.ServiceModel.ChannelFactory<ISecurityService>(wsBinding, wsEndpoint);

            //ISecurityService client = null;

            try
            {
                var resource = new SQLResource
                {
                    DatabaseName = _config.GetValue<string>("PervasiveDBName"),
                    ServerName = _config.GetValue<string>("PervasiveServer"),
                    UserID = _config.GetValue<string>("PervasiveUser")
                    
                };
                //using UserID and pwd from config as of now.Since, WSHttpBinding not supported in .Netcore 3.0
                var PervasiveUserId = _config.GetValue<string>("PervasiveUserId") ?? "AppUser";
                var PervasivePwd = _config.GetValue<string>("PervasivePwd") ?? "Password";

                //client = serviceChannelFactory.CreateChannel();
                //GetCredentialResponse credentials = client.GetCredentials(new GetCredentialRequest("MDCSKU", resource.ServerName, _config.GetValue<string>("PervasiveIP"), resource.UserID, resource));
                ////GetCredentialResponse credentials = client.GetCredentials(new GetCredentialRequest("MDCSKU", "gv1hqddb01.testgs.pvt", "172.22.5.138", "AppUser", resource));
                //((System.ServiceModel.ICommunicationObject)client).Close();

                //_pervasiveConnnectionString = $"ServerName={resource.ServerName};Dsn={_config.GetValue<string>("PervasiveDns")};uid={credentials.Credential.UserID};pwd={credentials.Credential.Password};";
                _pervasiveConnnectionString = $"ServerName={resource.ServerName};Dsn={_config.GetValue<string>("PervasiveDns")};uid={PervasiveUserId};pwd={PervasivePwd};";

            }
            catch (Exception ex)
            {
                //if (client != null)
                //{
                //    ((System.ServiceModel.ICommunicationObject)client).Abort();
                //}
                throw ex;
            }
        }

        #endregion

        #region [ IDbFactory Members ]

        public IDb GetDatabase()
        {
            if (_isPervasiveActiveFlag)
            {
                return new PervasiveDatabase(_config);
            }

            return new SqlDatabase(_config);
        }

        public IDb GetDatabase(string name)
        {
            if (name.Contains("Pervasive"))
            {
                return new PervasiveDatabase(_config, _pervasiveConnnectionString);
            }

            return new SqlDatabase(_config, name);
        }

        #endregion
    }
}
