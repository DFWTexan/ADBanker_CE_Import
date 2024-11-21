using Azure.Core;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace ADBanker_CE_Import.Services
{
    internal class ApiService : IApiService
    {
        private readonly IConfiguration _config;
        private readonly string? _baseUrl;
        private readonly string? _clientId;
        private readonly string? _clientSecret;

        public ApiService(IConfiguration config)
        {
            _config = config;
            _baseUrl = _config["AdBankerApi:BaseUrl"] ?? string.Empty;
            _clientId = _config["AdBankerApi:ClientId"] ?? string.Empty;
            _clientSecret = _config["AdBankerApi:ClientSecret"] ?? string.Empty;
        }

        public async Task<string> GetAccessToken()
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_baseUrl);
                var authURL = _baseUrl + "v1/Auth/Tokens";

                // We want the response to be JSON.
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // Add Basic Auth header
                string clientIdAndSecret = $"{_clientId}:{_clientSecret}";
                var authValue = Convert.ToBase64String(Encoding.ASCII.GetBytes(clientIdAndSecret));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authValue);

                // Build up the data to POST.
                List<KeyValuePair<string, string>> postData = new List<KeyValuePair<string, string>>();
                postData.Add(new KeyValuePair<string, string>("grant_type", "client_credentials"));

                FormUrlEncodedContent content = new FormUrlEncodedContent(postData);

                // Post to the Server and parse the response.
                HttpResponseMessage response = await client.PostAsync(authURL, content);
                string jsonString = await response.Content.ReadAsStringAsync();
                object responseData = JsonConvert.DeserializeObject(jsonString);

                // return the Access Token.
                return ((dynamic)responseData).access_token;
            }

        }

        public async Task<dynamic> GetCompletedCourses(string? accessToken, int? page, string? tags)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_baseUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);

                string url = string.Format("v1/CETrackingOverview/GetAll", page, HttpUtility.UrlEncode(tags));

                var requestBody = new
                {
                    Limit = 150,
                    Page = 0,
                    SortField = "CourseCompletionDate",
                    SortType = "asc",
                    FilterContainer = new List<object>
                    {
                        new
                        {
                            Filters = new List<object>
                            {
                                new
                                {
                                    FieldName = "CourseCompletionDate",
                                    Value = DateTime.Now.AddDays(-14).ToString(),
                                    Comparator = "greaterThan"
                                }
                            }
                        }
                    }
                };

                var json = JsonConvert.SerializeObject(requestBody);
                var data = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(url, data);

                string jsonString = await response.Content.ReadAsStringAsync();
                object? responseData = JsonConvert.DeserializeObject(jsonString);

                return (dynamic)responseData;
            }
        }
    }
}
