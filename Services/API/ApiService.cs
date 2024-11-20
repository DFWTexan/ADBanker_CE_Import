using Azure.Core;
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


        /// <summary>
        /// This method uses the OAuth Client Credentials Flow to get an Access Token to provide
        /// Authorization to the APIs.
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetAccessToken(string baseUrl, string clientId, string clientSecret)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(baseUrl);
                var authURL = baseUrl + "v1/Auth/Tokens";

                // We want the response to be JSON.
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // Add Basic Auth header
                string clientIdAndSecret = $"{clientId}:{clientSecret}";
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

        /// <summary>
        /// Gets the page of Articles.
        /// </summary>
        /// <param name="page">The page to get.</param>
        /// <param name="tags">The tags to filter the articles with.</param>
        /// <returns>The page of articles.</returns>
        public async Task<dynamic> GetCompletedCourses(string baseUrl, string accessToken, int page, string tags)
        {
            //DateTime getAfterDate = DateTime.Now.AddDays(-14);
            //using (SqlConnection conn = new SqlConnection(_connectionString))
            //{
            //    string query = @"
            //                    SELECT Top(1) CreateDate 
            //                    FROM stgADBankerImport 
            //                    ORDER BY CreateDate ";

            //    using (SqlCommand cmd = new SqlCommand(query, conn))
            //    {
            //        //cmd.Parameters.Add(new SqlParameter("@GEID", courseInfo.Student.EmployeeId.ToString()));

            //        conn.Open();
            //        getAfterDate = (DateTime)cmd.ExecuteScalar();
            //    }
            //}

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(baseUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // Add the Authorization header with the AccessToken.
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);

                // create the URL string.
                string url = string.Format("v1/CETrackingOverview/GetAll", page, HttpUtility.UrlEncode(tags));

                // Create the request body.
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


                // Convert the request body to JSON and then to StringContent.
                var json = JsonConvert.SerializeObject(requestBody);
                var data = new StringContent(json, Encoding.UTF8, "application/json");

                // make the request
                //HttpResponseMessage response = await client.GetAsync(url);
                HttpResponseMessage response = await client.PostAsync(url, data);

                // parse the response and return the data.
                string jsonString = await response.Content.ReadAsStringAsync();
                object responseData = JsonConvert.DeserializeObject(jsonString);

                return (dynamic)responseData;
            }
        }
    }
}
