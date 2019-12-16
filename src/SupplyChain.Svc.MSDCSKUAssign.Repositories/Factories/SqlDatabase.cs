using SupplyChain.Svc.MSDCSKUAssign.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;

namespace SupplyChain.Svc.MSDCSKUAssign.Repositories.Factories
{
    public class SqlDatabase : IDb
    {
        #region [ Declarations ]

        private string _connectionString = string.Empty;
        private IDbConnection _dbConnection;
        private IConfiguration _config;

        #endregion

        #region [ Instantiation ]

        public SqlDatabase(IConfiguration config)
           : this(config, string.Empty)
        {
        }

        public SqlDatabase(IConfiguration config, string connectionName)
        {
            _config = config;
            _connectionString = _config.GetValue<string>(connectionName).Replace("999",_config.GetValue<string>("ConnectionTimeout"));
            if (connectionName == "SkuConnectionString")
                _connectionString = "Data Source=GV1HQQDB50SQL01.testgs.pvt\\INST01;Initial Catalog=DCSystems;Integrated Security=True";

            CreateConnection();
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
            return new SqlDataAdapter();
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
            command.CommandText = storedProcedureName;
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
            var sqlDbCommand = dbCommand as SqlCommand;

            if (sqlDbCommand == null) return;

            var parameter = sqlDbCommand.Parameters.AddWithValue($"@{name}", value);
            parameter.SqlDbType = SqlDbType.Structured;
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
            command.Parameters.Add(new SqlParameter($"@{name}", value) { DbType = dbType, Direction = ParameterDirection.Input });
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
            command.Parameters.Add(new SqlParameter($"@{name}", value) { DbType = dbType, Size = size, Direction = ParameterDirection.Input });
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
            var parameter = new SqlParameter { ParameterName = $"@{name}", DbType = dbType, Direction = ParameterDirection.Output };
            command.Parameters.Add(parameter);
            return parameter;
        }

        public IDbDataParameter AddReturnValueParameter(IDbCommand command, string name, DbType dbType)
        {
            var parameter = new SqlParameter { ParameterName = $"@{name}", DbType = dbType, Direction = ParameterDirection.ReturnValue };
            command.Parameters.Add(parameter);
            return parameter;
        }

        public IDbDataParameter AddInOutParameter(IDbCommand command, string name, DbType dbType, object value)
        {
            var parameter = new SqlParameter($"@{name}", value) { DbType = dbType, Direction = ParameterDirection.InputOutput };
            command.Parameters.Add(parameter);
            return parameter;
        }

        public IDataAdapter GetDataAdapter(IDbCommand command)
        {
            var sqlCommand = command as SqlCommand;

            if (sqlCommand == null)
            {
                throw new ArgumentException("command is not type of System.Data.SqlClient.SqlCommand");
            }

            return new SqlDataAdapter(sqlCommand);
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
                _dbConnection = new SqlConnection(_connectionString);
            }

            if (_dbConnection.State != ConnectionState.Open)
            {
                _dbConnection.Open();
            }
        }

        #endregion
    }
}
