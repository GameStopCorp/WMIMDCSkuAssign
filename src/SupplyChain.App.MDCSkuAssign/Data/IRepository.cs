using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SupplyChain.App.MDCSkuAssign.Data
{
    public interface IRepository<T>
    {
        /// <summary>
        ///  Add to GS_IGINPT00_INSERT
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="whse"></param>
        bool AddDistro(T obj, string whse);

        /// <summary>
        /// Add to GS_I1INPT00_INSERT
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="whse"></param>
        ///<param name="tnt"></param>
        bool AddHeader(T obj, string whse, int tntValue, string batchNum, int estWeight);

        /// <summary>
        /// Add to GS_I2INPT00_INSERT
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="whse"></param>
        bool AddDetail(T obj, string whse, string batchNum, int pickLineNum);

        /// <summary>
        /// Returns previous records in DTStor for cross-referencing 
        /// </summary>
        /// <returns></returns>
        bool HasPreviousRecord(T obj);

        /// <summary>
        /// On completion of allocation, update all non-multi pick tickets
        /// </summary>
        /// <returns></returns>
        bool UpdateWmPickTicketStatuses(string whse);
    }
}
