using ADBanker_CE_Import.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADBanker_CE_Import
{
    public class ImportProcessor
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ImportProcessor> _logger;
        private readonly IApiService _apiService;
        private readonly IUtilityService _utilityService;

        public ImportProcessor(IConfiguration configuration, ILogger<ImportProcessor> logger, IUtilityService utilityService, IApiService apiService)
        {
            _configuration = configuration;
            _logger = logger;
            _apiService = apiService;
            _utilityService = utilityService;
        }

        public void Run()
        {
            _logger.LogInformation("Import Processor Started");

            // Get the configuration values.
            string baseUrl = _configuration["Api:BaseUrl"];
            string clientId = _configuration["Api:ClientId"];
            string clientSecret = _configuration["Api:ClientSecret"];
            string tags = _configuration["Api:Tags"];

            // Get the Access Token.
            string accessToken = _apiService.GetAccessToken(baseUrl, clientId, clientSecret).Result;

            // Get the Articles.
            int page = 1;
            dynamic articles = _apiService.GetCompletedCourses(baseUrl, accessToken, page, tags).Result;

            // Process the Articles.
            //foreach (var article in articles)
            //{
            //    _logger.LogInformation($"Processing Article: {article.Title}");
            //    _utilityService.ProcessArticle(article);
            //}

            _logger.LogInformation("Import Processor Finished");
        }
    }
}
