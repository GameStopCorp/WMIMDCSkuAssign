using SupplyChain.Svc.MSDCSKUAssign.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data;
using Microsoft.Extensions.Configuration;
using System.Data.Odbc;
using GameStop.Common.Services.Security;

namespace SupplyChain.Svc.MSDCSKUAssign.Repositories.Factories
{
    public class PervasiveDatabase : IDb
    {
        #region [ Declarations ]

        private string _connectionString = string.Empty;
        private IDbConnection _dbConnection;

        #endregion

        #region [ Instantiation ]

        public PervasiveDatabase(IConfiguration config)
           : this(config, string.Empty)
        {

        }

        public PervasiveDatabase(IConfiguration config, string connectionName)
        {
            _connectionString = connectionName;
            CreateConnection();

            #region Comment out

            /*
           var endpoint = config.GetValue<string>("PervasiveSecEndpoint");

           if (string.IsNullOrEmpty(_connectionString))
           {
               var wsBinding = new System.ServiceModel.WSHttpBinding();
               var wsEndpoint = new System.ServiceModel.EndpointAddress(endpoint);
               var serviceChannelFactory = new System.ServiceModel.ChannelFactory<ISecurityService>(wsBinding, wsEndpoint);

               ISecurityService client = null;

               try
               {
                   var resource = new SQLResource
                   {
                       DatabaseName = config.GetValue<string>("PervasiveDBName"),
                       ServerName = config.GetValue<string>("PervasiveServer"),
                       UserID = config.GetValue<string>("PervasiveUser")
                   };

                   client = serviceChannelFactory.CreateChannel();
                   GetCredentialResponse credentials = client.GetCredentials(new GetCredentialRequest("MDCSKU", "gv1hqddb01.testgs.pvt", "172.22.5.138", "AppUser", resource));
                   ((System.ServiceModel.ICommunicationObject)client).Close();

                   _connectionString = $"ServerName={resource.ServerName};Dsn={config.GetValue<string>("PervasiveDns")};uid={credentials.Credential.UserID};pwd={credentials.Credential.Password};";
               }
               catch (Exception ex)
               {
                   if (client != null)
                   {
                       ((System.ServiceModel.ICommunicationObject)client).Abort();
                   }

                   throw ex;
               }
           }

           CreateConnection();
           */

            #endregion

        }

        #endregion

        public IDbConnection DbConnection
        {
            get { return _dbConnection; }
        }


        public IDbTransaction GetTransaction()
        {
            return _dbConnection.BeginTransaction();
        }

        public IDbDataAdapter GetDataAdapter()
        {
            return new OdbcDataAdapter();
        }

        public IDbCommand GetStringCommand(string query)
        {
            var command = _dbConnection.CreateCommand();
            command.CommandText = query;
            command.CommandType = CommandType.Text;

            return command;
        }

        public IDbCommand GetStoredProcCommand(string storedProcedureName)
        {
            var command = _dbConnection.CreateCommand();
            command.CommandText = $"Call { storedProcedureName}";
            command.CommandType = CommandType.StoredProcedure;

            return command;
        }

        public string ConnectionString
        {
            get
            {
                return _connectionString;
            }
        }

        /// <summary>
        /// Adds the structered parameter.
        /// </summary>
        /// <param name="dbCommand">The database command.</param>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        public void AddInStructuredParameter(IDbCommand dbCommand, string name, object value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Adds the in parameter.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="name">The name.</param>
        /// <param name="dbType">Type of the database.</param>
        /// <param name="value">The value.</param>
        public void AddInParameter(IDbCommand command, string name, DbType dbType, object value)
        {
            command.Parameters.Add(new OdbcParameter($":{name}", value) { DbType = dbType, Direction = ParameterDirection.Input });
        }

        /// <summary>
        /// Adds the in parameter.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="name">The name.</param>
        /// <param name="dbType">Type of the database.</param>
        /// <param name="value">The value.</param>
        public void AddInParameter(IDbCommand command, string name, DbType dbType, int size, object value)
        {
            command.Parameters.Add(new OdbcParameter($":{name}", value) { DbType = dbType, Size = size, Direction = ParameterDirection.Input });
        }

        /// <summary>
        /// Adds the in parameter.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="name">The name.</param>
        /// <param name="dbType">Type of the database.</param>
        /// <param name="value">The value.</param>
        public IDbDataParameter AddOutParameter(IDbCommand command, string name, DbType dbType)
        {
            var parameter = new OdbcParameter { ParameterName = $":{name}", DbType = dbType, Direction = ParameterDirection.Output };
            command.Parameters.Add(parameter);
            return parameter;
        }

        public IDbDataParameter AddReturnValueParameter(IDbCommand command, string name, DbType dbType)
        {
            var parameter = new OdbcParameter { ParameterName = $":{name}", DbType = dbType, Direction = ParameterDirection.ReturnValue };
            command.Parameters.Add(parameter);
            return parameter;
        }

        public IDbDataParameter AddInOutParameter(IDbCommand command, string name, DbType dbType, object value)
        {
            var parameter = new OdbcParameter($":{name}", value) { DbType = dbType, Direction = ParameterDirection.InputOutput };
            command.Parameters.Add(parameter);
            return parameter;
        }

        public IDataAdapter GetDataAdapter(IDbCommand command)
        {
            var odbcCommand = command as OdbcCommand;

            if (odbcCommand == null)
            {
                throw new ArgumentException("command is not type of System.Data.SqlClient.SqlCommand");
            }

            return new OdbcDataAdapter(odbcCommand);
        }

        public void Close()
        {
            if (_dbConnection != null)
            {
                if (_dbConnection.State != ConnectionState.Closed)
                {
                    _dbConnection.Close();
                }
                _dbConnection = null;
            }
        }

        public void Dispose()
        {
            Close();
        }

        #region [ Private Methods ]
        private void CreateConnection()
        {
            if (_dbConnection == null)
            {
                _dbConnection = new OdbcConnection(_connectionString);
            }

            if (_dbConnection.State != ConnectionState.Open)
            {
                _dbConnection.Open();
            }
        }

        #endregion

    }
}
