using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.IO;
using SupplyChain.App.MDCSkuAssign.Entities;
using SupplyChain.Svc.MSDCSKUAssign.Controllers;
using SupplyChain.App.MDCSkuAssign.Services.Interfaces;
using CsvHelper;
using SupplyChain.DCAutoMailer;
using System.Text;
using SupplyChain.App.MDCSkuAssign.Exceptions;

namespace SupplyChain.App.MDCSkuAssign.Services
{
    public class AppProcessor : IProcessor
    {
        #region [ Declarations ]

        public readonly ILogger _logger;
        public readonly IRESTService _restService;
        public readonly IFileService _fileService;
        public readonly IPickTicketService<Allocation> _pickService;
        public readonly IDistroService<Allocation> _distroService;
        private IDCEMailService _email;
        public IOptions<AppSettings> _options;
        public IMsdcController _msdcController;
        #endregion

        #region [ Instantiation ]

        public AppProcessor(ILogger<Processor> logger, IOptions<AppSettings> options, IRESTService restService, IFileService fileService,
            IPickTicketService<Allocation> pickService, IDistroService<Allocation> distroService, IDCEMailService email, IMsdcController msdcController)
        {
            _logger = logger;
            _options = options;
            _restService = restService;
            _fileService = fileService;
            _pickService = pickService;
            _distroService = distroService;
            _email = email;
            _msdcController = msdcController;
        }

        #endregion

        #region [ Public Methods ]

        public bool Process()
        {
            var fileVersionAttribute = System.Reflection.Assembly.GetEntryAssembly()
                .CustomAttributes.Where(p => p.AttributeType.Equals(typeof(System.Reflection.AssemblyFileVersionAttribute)))
                .FirstOrDefault();

            _logger.LogInformation(string.Format("Machine Name:{0}", Environment.MachineName));
            _logger.LogInformation(string.Format("Build Version:{0}", fileVersionAttribute.ConstructorArguments.FirstOrDefault().Value));

            bool allocationsProcessed = false;

            var allocations = GetAllocationData();

            if (allocations.Any())
            {
                var allocationKvp = allocations.GroupBy(k => k.Warehouse).ToDictionary(g => g.Key, g => g.ToList());

                foreach (var kvp in allocationKvp)
                {
                    if (kvp.Key.Equals("LOU"))
                    {
                        _pickService.Create(kvp.Value, kvp.Key);
                    }
                    else
                    {
                        _distroService.Create(kvp.Value, kvp.Key);
                    }

                    SendEmail(kvp);
                }

                allocationsProcessed = allocations.Any(p => p.isProcessed);
            }
            else
            {
                allocationsProcessed = true;
            }

            ///releases all pick tickets
            if (allocationsProcessed)
            {
                if (_pickService.Release())
                {
                    _logger.LogInformation("All pick tickets have been released to Pick Ticket Apply Server");
                }
            }
            return allocationsProcessed;
        }

        public string GetCsvData()
        {
            string csvString = null;

            try
            {
                _logger.LogInformation("Sending Data Request");

                var url = string.Concat(_options.Value.WMIServiceURL, "/ProcessMultiDCSkus");

                UriBuilder builder = new UriBuilder(url);
                builder.Query = "isReporting=true";


                //Create a new message service             
                HttpResponseMessage httpResponse = _restService.GET(builder.Uri.ToString());

                if (httpResponse.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    if (httpResponse.Content != null)
                    {
                        var contentReceived = httpResponse.Content.ReadAsStringAsync().Result;

                        JObject jsonSearch = JObject.Parse(contentReceived);

                        // get JSON result objects into a list
                        IList<JToken> results = jsonSearch["allocations"].Children().ToList();

                        IList<Allocation> searchResults = new List<Allocation>();
                        foreach (JToken result in results)
                        {
                            // JToken.ToObject is a helper method that uses JsonSerializer internally
                            Allocation searchResult = result.ToObject<Allocation>();
                            searchResults.Add(searchResult);
                        }

                        StringWriter csvStringWriter = new StringWriter();
                        var csv = new CsvWriter(csvStringWriter);
                        csv.WriteRecords(searchResults);

                        csvString = csv.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw ex;
            }

            return csvString;
        }

        public List<Allocation> GetAllocationData()
        {
            var allocations = new List<Allocation>();

            try
            {
                _logger.LogInformation("Sending Data Request");

                int retryCount = _options.Value.RestServiecAvaliableRetryCount;
                for (int i = 0; i < retryCount; i++)
                {
                    try
                    {
                        _logger.LogInformation(string.Format("Get Allocation Data availability check attempt {0} of {1}.", i + 1, retryCount));
                        
                        var httpResponse = _msdcController.ProcessMultiDCSkus(true);

                        if (httpResponse.Allocations.Count > 0)
                        {
                            foreach (var item in httpResponse.Allocations)
                            {
                                item.Warehouse = item.Warehouse == "DAL" ? "GV1" : item.Warehouse;
                                Allocation allocation = ConvertToAllocation(item);
                                allocations.Add(allocation);
                            }
                            _logger.LogInformation(string.Format("Process Multi Sku's Succeded. Allocations count is {0}.",
                                                                allocations.Count));
                            return allocations;
                        }
                        else
                        {
                            throw new Exception();
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex.Message);
                        System.Threading.Thread.Sleep(30000);
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw ex;
            }

            return allocations;
        }

        /// <summary>
        /// Sends e-mail out for called warehouse
        /// </summary>
        /// <param name="allocations"></param>
        public void SendEmail(KeyValuePair<string, List<Allocation>> allocations)
        {
            try
            {
                var whse = allocations.Key;
                var allocationValues = allocations.Value;

                var totalStores = allocationValues.Select(s => s.Store).Distinct().Count();
                var totalLines = allocationValues.Count;
                var quantities = allocationValues.Sum(a => a.Quantity);

                //sort 
                var sortAllocations = allocationValues.OrderBy(a => a.Store);
                var firstAllocation = sortAllocations.FirstOrDefault();
                var lastAllocation = sortAllocations.LastOrDefault();

                var currentDate = DateTime.Now.ToString("MM/dd/yyyy");

                var body = new StringBuilder(Environment.NewLine);
                body.AppendLine(String.Format("{0}{1}{2}", "Date:", '\t', currentDate));
                body.AppendLine(String.Format("{0}{1}{2}", "Warehouse:", '\t', "MULTI DC"));
                body.AppendLine(String.Format("{0}{1}{2}", "Total Number of Stores:", '\t', totalStores));
                body.AppendLine(String.Format("{0}{1}{2}", "Total Number of Lines:", '\t', totalLines));
                body.AppendLine(String.Format("{0}{1}{2}", "Sum of quantities:", '\t', quantities));
                if (whse.Equals("LOU"))
                {
                    var firstPick = string.Format("{0}{1}", firstAllocation.AllocNum, firstAllocation.Store);
                    var lastPick = string.Format("{0}{1}", lastAllocation.AllocNum, lastAllocation.Store);
                    body.AppendLine(String.Format("{0}{1}{2}", "Starting/Ending Pick Tickets:", '\t', $"From {firstPick} to {lastPick}"));
                }
                else
                {
                    body.AppendLine(String.Format("{0}{1}{2}", "Distro:", '\t', firstAllocation.AllocNum));
                }

                body.AppendLine(String.Format("{0}{1}{2}", "Time Orders Available:", '\t', DateTime.Now.ToShortTimeString()));

                var isEmailSent = _email.SendMail(new DCAutoMailer.Models.SendModel
                {
                    Body = body.ToString(),
                    To = whse == "LOU" ? _options.Value.LOURecipient : _options.Value.GV1Recipient,
                    Subject = $"Multi DC Store Allocation Summary Report {currentDate}",
                });

                _logger.LogInformation($"Allocation e-mail sent for {whse}");
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                throw e;
            }
        }

        #endregion

        #region PrivateMethods
        /// <summary>
        /// Convert from AllocationContract to allocation
        /// </summary>
        /// <param name="contract">Allocation contract object</param>
        /// <returns>Allocation object</returns>
        private Allocation ConvertToAllocation(Svc.MSDCSKUAssign.Contracts.AllocationContract contract)
        {
            return new Allocation
            {
                Warehouse = contract.Warehouse,
                Store = contract.Store,
                Sku = contract.Sku,
                Quantity = contract.Quantity,
                Price = contract.Price,
                Item = contract.Item,
                Parstype = contract.Parstype,
                Rfm = contract.Rfm,
                Margin = contract.Margin,
                OutofStock = contract.OutofStock,
                Velocity = contract.Velocity,
                Weight = (decimal)contract.Weight,
                AllocNum = contract.AllocNum,
                CarryForwardDays = contract.CarryForwardDays
            };
        }
        #endregion
    }
}
