using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Text.RegularExpressions;
using SupplyChain.App.MDCSkuAssign.Entities;
using System.Collections.Generic;
using CsvHelper;
using SupplyChain.App.MDCSkuAssign.Services.Interfaces;

namespace SupplyChain.App.MDCSkuAssign.Services
{
    public class FileService : IFileService
    {
        #region [ Declarations ]

        private readonly ILogger _logger;
        private AppSettings _options { get; }

        #endregion

        #region [ Instantiation ]

        public FileService(ILogger<FileService> logger, IOptions<AppSettings> options)
        {
            _logger = logger;
            _options = options.Value;
        }

        #endregion

        #region [ Public Methods ]

        /// <summary>
        /// Generates file based off data passed in
        /// </summary>
        /// <param name="csv"></param>
        /// <returns></returns>
        public bool GenerateFile(string csv)
        {
            bool isFileComplete = false;

            try
            {
                var fileName = _options.WMIWarehouseAllocFilePattern.Replace("{date}", DateTime.Now.ToString("YYYYMMdd"));

                _logger.LogInformation(string.Concat("Creating WM WarehouseAllocation file: ", _options.WMIWarehouseAllocFilePattern));

                File.WriteAllText(Path.Combine(_options.FileDestinationPath, fileName), csv);
                _logger.LogInformation($"WM WarehouseAllocation file {_options.WMIWarehouseAllocFilePattern} written.");

            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
            }

            return isFileComplete;
        }

        public bool GenerateFile(string whse, List<Allocation> allocations)
        {
            bool isFileComplete = false;

            try
            {
                StringWriter csvStringWriter = new StringWriter();
                var csv = new CsvWriter(csvStringWriter);
                csv.WriteRecords(allocations);

                var fileName = _options.WMIWarehouseAllocFilePattern
                    .Replace("{date}", DateTime.Now.ToString("YYYYMMdd"))
                    .Replace("{whse}", whse);

                _logger.LogInformation(string.Concat("Creating WM WarehouseAllocation file: ", _options.WMIWarehouseAllocFilePattern));

                File.WriteAllText(Path.Combine(_options.FileDestinationPath, fileName), csv.ToString());
                _logger.LogInformation($"WM WarehouseAllocation file {_options.WMIWarehouseAllocFilePattern} written.");

            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
            }

            return isFileComplete;
        }

        #endregion
    }
}
