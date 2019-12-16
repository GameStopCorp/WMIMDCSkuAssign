using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SupplyChain.Svc.MSDCSKUAssign.Configurations.ConfigRepositories
{
    public interface IRepository
    {
        /// <summary>
        /// Returns a dictionary of setting values
        /// </summary>
        /// <returns></returns>
        IDictionary<string, string> GetSettingValues();

        /// <summary>
        /// Determines if connection is valid
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        bool IsConnectionValid(string connectionString);
    }
}
