using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SupplyChain.Svc.MSDCSKUAssign.Services.Managers.Interfaces;
using SupplyChain.Svc.MSDCSKUAssign.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using SupplyChain.Svc.MSDCSKUAssign.Services.Entities;
using System.IO;
using SupplyChain.Svc.MSDCSKUAssign.Services.Interfaces;
using SupplyChain.Svc.MSDCSKUAssign.Repositories.Interfaces;
using SupplyChain.Svc.MSDCSKUAssign.Services.Extensions;
using System.Runtime.InteropServices;

namespace SupplyChain.Svc.MSDCSKUAssign.Services.Managers
{
    public class AllocationManager : IAllocationManager<AllocationContract>
    {
        #region [ Private Declarations ]

        private ILogger<AllocationManager> _log;
        private IConfiguration _config;
        private IRepositorySvc<AllocationContract> _allocationRepository;
        private IControlRepository<ControlContract, SkuContract> _controlRepository;
        private IDecisionTreeFactory<SkuContract, TNTContract, AllocationContract> _factory;
        private bool _isPerformanceTestEnabled;

        #endregion

        #region [ Setup and Instantiation ]

        public AllocationManager(IConfiguration config, ILogger<AllocationManager> log, IRepositorySvc<AllocationContract> allocationRepository,
            IControlRepository<ControlContract, SkuContract> controlRepository, IDecisionTreeFactory<SkuContract, TNTContract, AllocationContract> factory)
        {
            _config = config;
            _log = log;
            _allocationRepository = allocationRepository;
            _controlRepository = controlRepository;
            _factory = factory;
            _isPerformanceTestEnabled = Convert.ToBoolean(_config.GetValue<string>("EnableBetaPerformanceEnhancment"));
            _log.LogInformation($"_isPerformanceTestEnabled = " + _isPerformanceTestEnabled);
            _isPerformanceTestEnabled = true;
        }

        #endregion

        #region [ Implementations ]

        /// <summary>
        /// Creates a list of allocation contracts by running validation tree on on all passed in SKUs and combined TNT list
        /// </summary>
        /// <param name="batchId"></param>
        /// <param name="skus"></param>
        /// <param name="tnt"></param>
        /// <param name="isReporting"></param>
        /// <returns></returns>
        public List<AllocationContract> GetAllocationContracts(Guid batchId, List<SkuContract> skus, List<TNTContract> tnt, bool isReporting)
        {
            //all should have the same allocation number
            var allocationNumber = tnt.First().AllocationNumber;

            _log.LogInformation($"Beginning store allocation pull for allocation {allocationNumber}...");
            var allocations = new List<AllocationContract>();

            IEnumerable<IGrouping<int, SkuContract>> skuGroups = skus.GroupBy(i => i.ItemNumber);
            var watch = System.Diagnostics.Stopwatch.StartNew();
            List<AllocationContract> contracts = new List<AllocationContract>();

            LogPerformance();
            if (_isPerformanceTestEnabled)
            {
                contracts = _allocationRepository.GetContractsByAllocNum(9806).OrderByDescending(a => a.Quantity).ToList();
                LogPerformance();
            }

            foreach (var skuGroup in skuGroups)
            {
                var skuHeader = skuGroup.First();

                //get allocations per SKU and pull transits based off item
                //_log.LogInformation($"Pull allocation detail and running decision tree for SKU { skuHeader.SkuNumber} and Item number { skuHeader.ItemNumber}");
                List<AllocationContract> allocationsByStore = new List<AllocationContract>();
                if (_isPerformanceTestEnabled)
                {
                    allocationsByStore = contracts.Where(x => x.Item == skuHeader.ItemNumber).ToList();
                }
                else
                {
                    allocationsByStore = _allocationRepository.GetContractsByStore(allocationNumber, skuHeader.ItemNumber)
                      .OrderByDescending(a => a.Quantity).ToList();
                }

                allocationsByStore.ForEach(a =>
                {
                    using (var decisionTree = _factory.Create<DecisionTree>(skuGroup.ToList(), tnt, a, isReporting))
                    {
                        var sku = decisionTree.Validate(batchId);

                        if (sku != null)
                        {
                            a.Warehouse = sku.WHLocation;
                            sku.WOHNorm -= a.Quantity;

                            //added for weight for pick tickets 8/2/2018
                            a.Weight = sku.Weight;

                            _log.LogInformation($"Selecting warehouse {a.Warehouse} for SKU {a.Sku} and store {a.Store} and allocation Qty {a.Quantity}.");

                            allocations.Add(a);
                        }
                        else
                        {
                            //sku is null
                            if (skuGroup.Sum(s => s.WOHNorm) >= a.Quantity && decisionTree.IsAllocatable)
                            {
                                foreach (var split in skuGroup)
                                {
                                    var alternates = skuGroup.Where(s => s.WHLocation != split.WHLocation);
                                    if (alternates.Any())
                                    {
                                        decisionTree.CreateReport(batchId, split, alternates.FirstOrDefault());
                                    }

                                    //set warehouse location for allocation
                                    a.Warehouse = split.WHLocation;

                                    //clone allocation first
                                    var alloc = Extensions.Extensions.DeepClone<AllocationContract>(a);

                                    //set rolling allocation qty to actual warehouse value
                                    alloc.Quantity = split.WOHNorm;

                                    _log.LogInformation($"Split shipment for SKU {alloc.Sku} for store {alloc.Store}. Allocation Qty: {a.Quantity} split to " +
                                   $"warehouse location: {split.WHLocation}, split quantity of: [{split.WOHNorm}].");

                                    //decrement actual allocation by what's in the warehouse and what was allocated
                                    a.Quantity -= split.WOHNorm;
                                    split.WOHNorm -= alloc.Quantity;

                                    allocations.Add(alloc);
                                }
                            }
                        }
                    }
                });
            };
            watch.Stop();
            LogPerformance();
            _log.LogWarning("@@@VS- Get Allocation Contracts: Total elapsed time for execution for 'GetAllocationContracts' is:{0} in TotalMinutes and {1} in TotalSeconds, " +
                                             " and total allocations count returned is:{2}", watch.Elapsed.TotalMinutes, watch.Elapsed.TotalSeconds, allocations.Count);

            return allocations;
        }

        /// <summary>
        /// Returns a lookup for all store allocations for GISMO
        /// </summary>
        /// <returns>List of tuples containing store allocation files from WMIStoreAllocation job</returns>
        public List<Tuple<int, int>> GetStoreAllocations()
        {
            _log.LogInformation("Calling store allocation files:  Reading all allocation files..");

            var storeDictionary = new List<Tuple<int, int>>();
            var t3 = DateTime.Now;

            //get allocation directory
            var searchDirectory = new DirectoryInfo(@_config.GetValue<string>("StoreFileSearchDirectory"));

            try
            {
                var currentDate = DateTime.Now.Date.ToString("yyyyMMdd");
                var searchPattern = $"{_config.GetValue<string>("StoreAllocationFilePrefix")}{currentDate}*";
                //var files = searchDirectory.GetFiles(searchPattern);
                var files = searchDirectory.GetFiles("wmistoreallocation20191204*");


                for (int i = 0; i < files.Length; i++)
                {
                    using (var fs = new FileStream(files[i].FullName, FileMode.Open, FileAccess.Read))
                    {
                        using (var reader = new StreamReader(fs))
                        {
                            reader.ReadLine();
                            while (!reader.EndOfStream)
                            {
                                var line = reader.ReadLine();
                                var values = line.Split(',');

                                storeDictionary.Add(new Tuple<int, int>(Convert.ToInt32(values[0]), Convert.ToInt32(values[1])));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            LoggerExtension.LogTimeDiff(t3, DateTime.Now, _log,
                        string.Format("@@@VS- Time taken to fetch all allocations for GISMO:"));
            return storeDictionary;
        }

        /// <summary>
        /// Returns value if control repository for Offsite INV has been updated
        /// </summary>
        /// <returns></returns>
        public bool IsOffsiteDailyUpdated()
        {
            var t5 = DateTime.Now;
            var isUpdated = _controlRepository.IsUpdatedData(DateTime.Now);
            LoggerExtension.LogTimeDiff(t5, DateTime.Now, _log,
                        string.Format("@@@VS- Time taken to check if INV has been updated:"));
            if (isUpdated)
                _log.LogInformation("Offsite Daily was updated current date from stage INV1");

            return isUpdated;
        }

        /// <summary>
        /// Updateds INV records in stage database from GISMO data
        /// </summary>
        /// <param name="invRecords"></param>
        /// <returns></returns>
        public bool IsDataStaged(List<SkuContract> invRecords)
        {
            _log.LogInformation("");

            for (int i = 0; i < invRecords.Count; i++)
            {
                var staged = invRecords[i];
                var t7 = DateTime.Now;
                var pervasive = _controlRepository.GetPervasiveItemBySku(staged.SkuNumber);
                LoggerExtension.LogTimeDiff(t7, DateTime.Now, _log,
                        string.Format("@@@VS- Time taken to Get pervasive item by SKU:"));
                if (!staged.WOHNorm.Equals(pervasive.WOHNorm))
                {
                    var t9 = DateTime.Now;
                    _controlRepository.IsUpsertedStageInvData(pervasive);
                    LoggerExtension.LogTimeDiff(t9, DateTime.Now, _log,
                        string.Format("@@@VS- Time taken to check upserted stage INV data:"));
                    _log.LogInformation($"SKU {staged.SkuNumber} on hand was updated in stage INV1 from Pervasive");
                }
            }

            return IsOffsiteDailyUpdated();
        }

        #endregion

        #region PrivateMethods

        private void LogPerformance()
        {
            Int64 phav = PerformanceInfo.GetPhysicalAvailableMemoryInMiB();
            Int64 tot = PerformanceInfo.GetTotalMemoryInMiB();
            decimal percentFree = ((decimal)phav / (decimal)tot) * 100;
            decimal percentOccupied = 100 - percentFree;
            _log.LogInformation("@@@VS-Available Physical Memory (MiB): {0} Total Memory (MiB): {1} Free (%): {2} Occupied (%): {3}",
                                  phav.ToString(), tot.ToString() , Math.Round(percentFree, 2), Math.Round(percentOccupied, 2));
        }

        private static class PerformanceInfo
        {
            [DllImport("psapi.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool GetPerformanceInfo([Out] out PerformanceInformation PerformanceInformation, [In] int Size);

            [StructLayout(LayoutKind.Sequential)]
            public struct PerformanceInformation
            {
                public int Size;
                public IntPtr CommitTotal;
                public IntPtr CommitLimit;
                public IntPtr CommitPeak;
                public IntPtr PhysicalTotal;
                public IntPtr PhysicalAvailable;
                public IntPtr SystemCache;
                public IntPtr KernelTotal;
                public IntPtr KernelPaged;
                public IntPtr KernelNonPaged;
                public IntPtr PageSize;
                public int HandlesCount;
                public int ProcessCount;
                public int ThreadCount;
            }

            public static Int64 GetPhysicalAvailableMemoryInMiB()
            {
                PerformanceInformation pi = new PerformanceInformation();
                if (GetPerformanceInfo(out pi, Marshal.SizeOf(pi)))
                {
                    return Convert.ToInt64((pi.PhysicalAvailable.ToInt64() * pi.PageSize.ToInt64() / 1048576));
                }
                else
                {
                    return -1;
                }

            }

            public static Int64 GetTotalMemoryInMiB()
            {
                PerformanceInformation pi = new PerformanceInformation();
                if (GetPerformanceInfo(out pi, Marshal.SizeOf(pi)))
                {
                    return Convert.ToInt64((pi.PhysicalTotal.ToInt64() * pi.PageSize.ToInt64() / 1048576));
                }
                else
                {
                    return -1;
                }

            }
        }
        #endregion
    }
}