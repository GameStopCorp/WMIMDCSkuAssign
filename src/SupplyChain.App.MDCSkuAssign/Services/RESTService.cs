using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using SupplyChain.App.MDCSkuAssign.Services.Interfaces;

namespace SupplyChain.App.MDCSkuAssign.Services
{
    public class RESTService : IRESTService
    {
        #region [ Declarations ]

        private readonly ILogger _logger;
        private AppSettings _options { get; }

        #endregion

        #region [ Instantiation ]

        public RESTService(ILogger<RESTService> logger, IOptions<AppSettings> options)
        {
            _logger = logger;
            _options = options.Value;
        }

        #endregion

        #region [ Public Methods ]


        /// <summary>
        /// Http get request for rest api
        /// </summary>
        /// <param name="url"></param>
        /// <param name="httpClientTimeout">Overwite default httpClientTimeout defined in appSettings.json</param>
        /// <returns>Http response message</returns>
        public HttpResponseMessage GET(string url, int? httpClientTimeout = null)
        {
            var httpResponse = new HttpResponseMessage();
            if (string.IsNullOrEmpty(url))
            {
                url = _options.WMIServiceURL;
            }

            try
            {
                if (Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute))
                {
                    var httpClient = new HttpClient();

                    if (string.IsNullOrEmpty(_options.httpClientTimeout.ToString()))
                    {
                        throw new Exception("ERROR: Missing httpClientTimeout setting in appSetting.json");
                    }
                    
                    if (httpClientTimeout.HasValue)
                    {
                        httpClient.Timeout = TimeSpan.FromSeconds(httpClientTimeout.Value);
                    }
                    else
                    {
                        httpClient.Timeout = TimeSpan.FromSeconds(_options.httpClientTimeout);
                    }

                    httpResponse = httpClient.GetAsync(url).Result;
                    switch (httpResponse.StatusCode)
                    {
                        case System.Net.HttpStatusCode.InternalServerError:
                            _logger.LogError(string.Concat("Service Error: ", httpResponse.StatusCode.ToString()));
                            break;
                        case System.Net.HttpStatusCode.NoContent:
                            _logger.LogInformation("Response received: NO CONTENT");
                            break;
                        default:
                            _logger.LogInformation(string.Concat("Response received: ", httpResponse.Content.ReadAsStringAsync().Result));
                            break;
                    }
                }
                else
                {
                    _logger.LogInformation(string.Concat("Service Error:", httpResponse.StatusCode.ToString()));
                    httpResponse.StatusCode = System.Net.HttpStatusCode.BadRequest;
                }
            }
            catch (Exception e)
            {
                _logger.LogError(string.Concat("Service Error: ", e.ToString()));
                throw e;

            }
            return httpResponse;
        }


        /// <summary>
        /// Http post request for rest api
        /// </summary>
        /// <param name="url"></param>
        /// <param name="json"></param>
        /// <param name="httpClientTimeout">Overwite default httpClientTimeout defined in appSettings.json</param>
        /// <returns></returns>
        public HttpResponseMessage POST(string url = "", string json = "", int? httpClientTimeout = null)
        {
            HttpResponseMessage httpResponse = new HttpResponseMessage();

            if (string.IsNullOrEmpty(url))
            {
                url = _options.WMIServiceURL;
            }

            try
            {
                if (Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute))
                {
                    _logger.LogInformation(string.Concat("Posting to URL: ", url, " Data: ", json));

                    var httpClient = new HttpClient();

                    if (string.IsNullOrEmpty(_options.httpClientTimeout.ToString()))
                    {
                        throw new Exception("ERROR: Missing httpClientTimeout setting in appSetting.json");
                    }

                    if (httpClientTimeout.HasValue)
                    {
                        httpClient.Timeout = TimeSpan.FromSeconds(httpClientTimeout.Value);
                    }
                    else
                    {
                        httpClient.Timeout = TimeSpan.FromSeconds(_options.httpClientTimeout);
                    }

                    httpResponse = httpClient.PostAsync(url, new StringContent(json, System.Text.Encoding.UTF8, "application/json")).Result;

                    if (httpResponse.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        _logger.LogInformation(string.Concat("Response received:", httpResponse.Content.ReadAsStringAsync().Result));
                    }
                    else
                    {
                        _logger.LogInformation(string.Concat("Service Error:", httpResponse.StatusCode.ToString()));
                    }
                }
                else
                {
                    _logger.LogInformation(string.Concat("Service Error:", httpResponse.StatusCode.ToString()));
                    httpResponse.StatusCode = System.Net.HttpStatusCode.BadRequest;
                }
            }
            catch (Exception e)
            {
                if (e.InnerException == null)
                {
                    _logger.LogError(string.Concat("Service Error: ", e.Message));
                }
                else
                {
                    _logger.LogError(string.Concat("Service Error: ", e.InnerException.Message));
                }
            }

            return httpResponse;
        }
    }

    #endregion
}
