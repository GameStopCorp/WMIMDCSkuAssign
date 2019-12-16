using Newtonsoft.Json;
using SupplyChain.Svc.MSDCSKUAssign.Configurations.ConfigRepositories;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SupplyChain.Svc.MSDCSKUAssign.Configurations.ConfigRepositories
{
    public class SettingConfigurationRepository : IRepository
    {
        #region [ Private Declarations ]

        private readonly IDictionary<string, string> _data =
        new SortedDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        private string _applicationName = string.Empty;
        private string _connectionString = string.Empty;

        #endregion

        public SettingConfigurationRepository(Stream input)
        {
            var serializer = new JsonSerializer();

            JsonTextReader reader = new JsonTextReader(new StreamReader(input));
            dynamic appSettings = serializer.Deserialize(reader);

            _applicationName = appSettings.AppSettings.ApplicationName.Value;
            _connectionString = appSettings.AppSettings.ConnectionString.Value;

        }

        public IDictionary<string, string> GetSettingValues()
        {
            _data.Clear();

            try
            {
                using (var con = new SqlConnection(_connectionString))
                {
                    using (var cmd = new SqlCommand("spGetAllAppPropertyValues", con))
                    {
                        con.Open();
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@App_Desc", _applicationName);
                        var reader = cmd.ExecuteReader();

                        while (reader.Read())
                        {
                            _data.Add(reader.GetString(0), reader.GetString(1));
                        }
                        
                        con.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return _data;
        }

        /// <summary>
        /// Determines if the connection to the selected connection string is valid
        /// </summary>
        /// <param name="connectionString">DC Property connection string or Exchange connection string</param>
        /// <returns></returns>
        public bool IsConnectionValid(string connectionString)
        {
            bool isDatabaseUp = false;

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    if (con.State != ConnectionState.Closed)
                        isDatabaseUp = true;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return isDatabaseUp;
        }
    }
}
