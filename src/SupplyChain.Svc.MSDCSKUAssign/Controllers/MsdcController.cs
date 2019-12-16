using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using SupplyChain.Svc.MSDCSKUAssign.Services.Interfaces;
using SupplyChain.Svc.MSDCSKUAssign.Services.Extensions;
using System.Text;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace SupplyChain.Svc.MSDCSKUAssign.Controllers
{
    [Route("api/[controller]")]
    public class MsdcController : Controller
    {
        #region [ Private Declarations ]

        /// <summary>
        /// Injected logger factory
        /// </summary>
        private ILogger<MsdcController> _logger;

        /// <summary>
        /// Injected application config
        /// </summary>
        private IConfiguration _config;

        /// <summary>
        /// SKU Service pilot service
        /// </summary>
        //private IService<SkuContract, TNTContract> _skuService;
        private IMsdcService _msdcService;

        public static volatile int instanceCount = 0;

        #endregion

        #region [ Setup and Instantiation ]

        /// <summary>
        /// DI constructor for MSDC controller
        /// </summary>
        /// <param name="config"></param>
        /// <param name="logger"></param>
        public MsdcController(IConfiguration config, IMsdcService msdcService, ILogger<MsdcController> logger)
        {
            _logger = logger;
            _config = config;
            _msdcService = msdcService;

            if (instanceCount == 0)
            {
                _logger.LogInformation($"Machine Name:{Environment.MachineName}");
                _logger.LogInformation($"Build Version:{Assembly.GetEntryAssembly().GetName().Version}");
                instanceCount++;
            }
        }

        #endregion

        #region [ Public Actions ]

        // GET: api/values
        [HttpGet]
        public string Get()
        {
            _logger.LogInformation("HTTP GET Invoked @ {requestTime}", DateTime.Now);

            return "Web service is AVAILABLE";
        }

        [HttpGet]
        [Route("[action]")]
        public IActionResult ProcessMultiDCSkus(bool isReporting)
        {
            try
            {
                var batchId = Guid.NewGuid();
                var result = _msdcService.GetMultiSkuAllocations(batchId, isReporting);
                var statusCode = (int)System.Net.HttpStatusCode.OK;
                object json = null;

                if (result == null)
                {
                    statusCode = (int)System.Net.HttpStatusCode.NoContent;
                }

                json = new
                {
                    BatchId = batchId,
                    Allocations = result
                };

                return new JsonResult(json) { ContentType = "application/json", StatusCode = statusCode };
            }
            catch (Exception ex)
            {
                _logger.LogError(getExceptionMessage(ex));
                return new JsonResult("Failed") { StatusCode = (int)System.Net.HttpStatusCode.InternalServerError };
            }
        }

        [HttpGet]
        [Route("[action]")]
        public IActionResult UpdateInvDailyRecords()
        {
            try
            {
                var t1 = DateTime.Now;
                var result = _msdcService.IsDailyInventoryUpdated();
                LoggerExtension.LogTimeDiff(t1, DateTime.Now, _logger,
                        string.Format("@@@VS- Time taken to check if daily inventory is updated or not:"));
                var statusCode = (int)System.Net.HttpStatusCode.OK;
                if (!result)
                {
                    statusCode = (int)System.Net.HttpStatusCode.InternalServerError;
                }

                return new JsonResult(result) { StatusCode = statusCode };

            }
            catch (Exception ex)
            {
                _logger.LogError(getExceptionMessage(ex));
                return new JsonResult("Failed") { StatusCode = (int)System.Net.HttpStatusCode.InternalServerError };
            }
        }

        #endregion

        #region [ Private Methods ]

        /// <summary>
        /// Returns a string for capturing full exception in log
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        private string getExceptionMessage(Exception ex)
        {
            var errorMessageToLog = new StringBuilder();
            errorMessageToLog.AppendFormat("{0}{1}Exception", Environment.NewLine, Environment.NewLine);
            errorMessageToLog.AppendFormat(" Type: {0}{1}", ex.GetType(), Environment.NewLine);
            errorMessageToLog.AppendFormat(" Message: {0}{1}", ex.Message, Environment.NewLine);
            if (ex.InnerException != null)
            {
                errorMessageToLog.AppendLine("Inner Exception");
                errorMessageToLog.AppendFormat(" Type: {0}{1}", ex.InnerException.GetType(), Environment.NewLine);
                errorMessageToLog.AppendFormat(" Message: {0}{1}", ex.InnerException.Message, Environment.NewLine);
            }
            errorMessageToLog.AppendFormat("Stack Trace: {0}{1}{2}", ex.StackTrace, Environment.NewLine, Environment.NewLine);
            return errorMessageToLog.ToString();
        }

        #endregion

        #region [ Not Implemented Actions ]

        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }

        #endregion

    }
}
