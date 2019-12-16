using SupplyChain.App.MDCSkuAssign.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SupplyChain.App.MDCSkuAssign.Services.Interfaces
{
    public interface IFileService
    {
        /// <summary>
        /// Generate file based off csv data passed in
        /// </summary>
        /// <param name="csv"></param>
        /// <returns></returns>
        bool GenerateFile(string csv);

        /// <summary>
        /// Generate file based off csv data passed in
        /// </summary>
        /// <param name="csv"></param>
        /// <returns></returns>
        bool GenerateFile(string whse, List<Allocation> allocations);
    }
}
