using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SupplyChain.Svc.MSDCSKUAssign.Repositories.Interfaces
{
    public interface IDbFactory
    {
        /// <summary>
        /// Get standard database based off Pervasive/SQL environment flags
        /// </summary>
        /// <returns></returns>
        IDb GetDatabase();

        /// <summary>
        /// Get databate explicitly by name;  
        /// </summary>
        /// <param name="name">ex.. "Pervasive", "SkuConnectionString", "TntConnectionString" ect..</param>
        /// <returns></returns>
        IDb GetDatabase(string name);
    }
}
