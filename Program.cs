using System.Data;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using ADBanker_CE_Import.Services;

namespace ADBanker_CE_Import
{
    class Program
    {
        /// The client information used to get the OAuth Access Token from the server.
        //static string clientId = "7ce15f9cbc7462eb56f4cbabe1fd7f6c844c1e8010c5a9603ffdd6efc7a7d99c";
        //static string clientSecret = "905a23f9bf06105e69cc7981e232bdcf58a3d8e2bea76baa712db1da486f02647c07227f4b58cb036a86f70b11eff1a18481c43ca4ce93f4ea576e83ad4866c21b7c5a1e47832aebf21a6958882167064eab99616cfaafc8f43b9e7bd3b01d359258ba5c8d1e1121cf9150bf999c7f259d9e6b29a7315f0510bccd8c93aa0eb8";
        //private static IConfiguration Configuration { get; set; }

        // The connection string to the database.
        //static string _connectionString = "Server=FTSQLD201;Database=License;Integrated Security=True";

        // The server base address
        //static string baseUrl = "https://api.adbanker.com/";

        // this will hold the Access Token returned from the server.
        //static string accessToken = null;

        static void Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.SetBasePath(Directory.GetCurrentDirectory());
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton<ImportProcessor>();
                    services.AddSingleton<IApiService, ApiService>();
                    services.AddSingleton<IDatabaseService, DatabaseService>();
                    services.AddSingleton<IUtilityService, UtilityService>();
                })
                .ConfigureLogging(logging =>
                {
                    //logging.ClearProviders();
                    //logging.AddConsole();
                })
                .Build();

            var processor = host.Services.GetRequiredService<ImportProcessor>();
            processor.Run();

            //LogInfo("Starting Import of CE Completion.");

            //DoIt().Wait();
            processor.Run();

            //LogInfo("ADBanker Import of CE Completed...");

        }

        //private static async Task<int> DoIt()
        //{
        //    int studentCount = 0;
        //    // Get the Access Token.
        //    accessToken = await GetAccessToken();
        //    Console.WriteLine(accessToken != null ? "Got Token" : "No Token found");

        //    // Get the Student Courses.
        //    Console.WriteLine();
        //    Console.WriteLine("------ New C# Student Courses ------");
        //    dynamic response = await GetCompletedCourses(1, "c#");

        //    //var studentCourses = (dynamic)response.Results;


        //    if (response.Results != null)
        //    {
        //        //studentCount = response.Results.Count;
        //        // LOG - Date of Import and Count of Records
        //        var studentCourses = (dynamic)response.Results;
        //        foreach (dynamic courseInfo in studentCourses)
        //        {
        //            if (courseInfo.Student.EmployeeId == null || string.IsNullOrEmpty(courseInfo.Student.EmployeeId.ToString()))
        //            {
        //                Console.WriteLine("TeamMemberID is null or empty. Skipping this record.");
        //                LogInfo(string.Format("TeamMemberID is null or empty for - Name: {0} {1} state: {2}. Skipping this record.", courseInfo.Student.FirstName, courseInfo.Student.LastName, courseInfo.Student.StateCode));
                        
        //                var isFound = CheckImport(new ViewModels.StudentCourseInfo
        //                {
        //                    EmployeeID = null,
        //                    CourseState = courseInfo.Student.StateCode,
        //                    StudentName = courseInfo.Student.FirstName + " " + courseInfo.Student.LastName,
        //                    CourseTitle = courseInfo.Course.CourseTitle,
        //                    CompletionDate = Convert.ToDateTime((string)courseInfo.StudentCourse.CourseCompletionDate),
        //                    ReportedDate = Convert.ToDateTime((string)courseInfo.StudentCourse.ReportedDate),
        //                    TotalCredits = (double)courseInfo.Course.Credits
        //                });

        //                if (isFound)  // Do not insert if the record is already in the system
        //                    continue;

        //                using (SqlConnection conn = new SqlConnection(_connectionString))
        //                {
        //                    using (SqlCommand cmd = new SqlCommand("INSERT INTO [dbo].[stg_ADBankerImport] ([TeamMemberID], [CourseState], [StudentName], [CourseTitle], [CompletionDate], [ReportedDate], [TotalCredits], [CreateDate], [IsImportComplete], [SortOrder]) VALUES (@TeamMemberID, @CourseState, @StudentName, @CourseTitle, @CompletionDate, @ReportedDate, @TotalCredits, @CreateDate, @IsImportComplete, @SortOrder)", conn))
        //                    {
        //                        cmd.Parameters.Add(new SqlParameter("@TeamMemberID", (string)courseInfo.Student.EmployeeId));
        //                        cmd.Parameters.Add(new SqlParameter("@CourseState", (string)courseInfo.Student.StateCode));
        //                        cmd.Parameters.Add(new SqlParameter("@StudentName", (string)courseInfo.Student.FirstName + " " + (string)courseInfo.Student.LastName));
        //                        cmd.Parameters.Add(new SqlParameter("@CourseTitle", (string)courseInfo.Course.CourseTitle));
        //                        cmd.Parameters.Add(new SqlParameter("@CompletionDate", Convert.ToDateTime((string)courseInfo.StudentCourse.CourseCompletionDate)));
        //                        cmd.Parameters.Add(new SqlParameter("@ReportedDate", Convert.ToDateTime((string)courseInfo.StudentCourse.ReportedDate)));
        //                        cmd.Parameters.Add(new SqlParameter("@TotalCredits", (double)courseInfo.Course.Credits));
        //                        cmd.Parameters.Add(new SqlParameter("@CreateDate", DateTime.Now));
        //                        bool isImportComplete = false;
        //                        cmd.Parameters.Add(new SqlParameter("@IsImportComplete", isImportComplete ? 1 : 0));
        //                        cmd.Parameters.Add(new SqlParameter("@SortOrder", 1));

        //                        conn.Open();
        //                        cmd.ExecuteNonQuery();
        //                    }
        //                }
        //                continue; // Skip this iteration if EmployeeId is null or empty
        //            }

        //            Console.WriteLine("  {0} - Team #: {1} - Name: {2} {3} - NP#: {4} - Title: {5} - CompDate: {6} - RptDate: {7} - Credits: {8}",
        //                courseInfo.Student.StateCode,
        //                courseInfo.Student.EmployeeId,
        //                courseInfo.Student.FirstName,
        //                courseInfo.Student.LastName,
        //                courseInfo.Student.NationalProducerNumber > 0 ? courseInfo.Student.NationalProducerNumber : "N/A",
        //                courseInfo.Course.CourseTitle,
        //                courseInfo.StudentCourse.CourseCompletionDate.ToString("MM/dd/yyyy"),
        //                courseInfo.StudentCourse.ReportedDate.ToString("MM/dd/yyyy") != "" ? courseInfo.StudentCourse.ReportedDate.ToString("MM/dd/yyyy") : "N/A",
        //                courseInfo.Course.Credits);

        //            int conEduReqID;
        //            try
        //            {
        //                using (SqlConnection conn = new SqlConnection(_connectionString))
        //                {
        //                    string query = @"
        //                                    SELECT c.ContEducationRequirementID 
        //                                    FROM Employee a 
        //                                    JOIN Employment b ON a.EmployeeID = b.EmployeeID
        //                                    JOIN ContEducationRequirement c ON b.EmploymentID = c.EmploymentID
        //                                    WHERE a.GEID = @GEID";

        //                    using (SqlCommand cmd = new SqlCommand(query, conn))
        //                    {
        //                        cmd.Parameters.Add(new SqlParameter("@GEID", courseInfo.Student.EmployeeId.ToString()));

        //                        conn.Open();
        //                        //conEduReqID = (int)cmd.ExecuteScalar();
        //                        var result = cmd.ExecuteScalar();
        //                        conEduReqID = result != null ? (int)result : -1; // Handle null by assigning -1
        //                    }
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                Console.WriteLine("Error: " + ex.Message);
        //                conEduReqID = -1;
        //                LogError(ex.Message, "ADBankerImport-Api");
        //            }

        //            if (conEduReqID == -1)
        //            {
        //                Console.WriteLine("Employee not found in the system. Skipping this record.");
        //                LogInfo(string.Format("Employee not found in the system. Skipping this record. EmployeeID: {0}", courseInfo.Student.EmployeeId), null);
        //                continue;
        //            }

        //            var studentCourseInfo = new ViewModels.StudentCourseInfo
        //            {
        //                EmployeeID = courseInfo.Student.EmployeeId,
        //                CourseState = courseInfo.Student.StateCode,
        //                StudentName = courseInfo.Student.FirstName + " " + courseInfo.Student.LastName,
        //                CourseTitle = courseInfo.Course.CourseTitle,
        //                CompletionDate = Convert.ToDateTime((string)courseInfo.StudentCourse.CourseCompletionDate),
        //                ReportedDate = Convert.ToDateTime((string)courseInfo.StudentCourse.ReportedDate),
        //                TotalCredits = (double)courseInfo.Course.Credits,

        //                ContEducationRequirementID = conEduReqID,
        //                ContEducationTakenDate = courseInfo.StudentCourse.CourseCompletionDate,
        //                CreditHoursTaken = courseInfo.Course.Credits,
        //                AdditionalNotes = string.Format("ADBnkApi_{0} - {1}", DateTime.Now.ToString("yyyyMMdd"), courseInfo.Course.CourseTitle)
        //            };

        //            try
        //            {
        //                if (!CheckImport(studentCourseInfo)) {
        //                    studentCount++;
        //                    using (SqlConnection conn = new SqlConnection(_connectionString))
        //                    {
        //                        using (SqlCommand cmd = new SqlCommand("INSERT INTO [dbo].[stg_ADBankerImport] ([TeamMemberID], [CourseState], [StudentName], [CourseTitle], [CompletionDate], [ReportedDate], [TotalCredits], [CreateDate], [IsImportComplete], [SortOrder]) VALUES (@TeamMemberID, @CourseState, @StudentName, @CourseTitle, @CompletionDate, @ReportedDate, @TotalCredits, @CreateDate, @IsImportComplete, @SortOrder)", conn))
        //                        {
        //                            cmd.Parameters.Add(new SqlParameter("@TeamMemberID", (string)courseInfo.Student.EmployeeId));
        //                            cmd.Parameters.Add(new SqlParameter("@CourseState", (string)courseInfo.Student.StateCode));
        //                            cmd.Parameters.Add(new SqlParameter("@StudentName", (string)courseInfo.Student.FirstName + " " + (string)courseInfo.Student.LastName));
        //                            cmd.Parameters.Add(new SqlParameter("@CourseTitle", (string)courseInfo.Course.CourseTitle));
        //                            cmd.Parameters.Add(new SqlParameter("@CompletionDate", Convert.ToDateTime((string)courseInfo.StudentCourse.CourseCompletionDate)));
        //                            cmd.Parameters.Add(new SqlParameter("@ReportedDate", Convert.ToDateTime((string)courseInfo.StudentCourse.ReportedDate)));
        //                            cmd.Parameters.Add(new SqlParameter("@TotalCredits", (double)courseInfo.Course.Credits));
        //                            cmd.Parameters.Add(new SqlParameter("@CreateDate", DateTime.Now));
        //                            bool isImportComplete = InsertEmployeeCE(studentCourseInfo);
        //                            cmd.Parameters.Add(new SqlParameter("@IsImportComplete", isImportComplete ? 1 : 0));
        //                            cmd.Parameters.Add(new SqlParameter("@SortOrder", 2));

        //                            conn.Open();
        //                            cmd.ExecuteNonQuery();
        //                        }
        //                    }
        //                };
                        
        //            }
        //            catch (Exception ex)
        //            {
        //                Console.WriteLine("Error: " + ex.Message);
        //                LogError(ex.Message, "ADBankerImport-Api");
        //            }

        //        }
        //        // LOG - Date of Import and Count of Records
        //        LogInfo(string.Format(" ==> ADBanker {0} Imported..", studentCount), null);
        //    }

        //    return 0;
        //}

        //private static async Task<string> GetAccessToken()
        //{
        //    using (var client = new HttpClient())
        //    {
        //        client.BaseAddress = new Uri(baseUrl);
        //        var authURL = baseUrl + "v1/Auth/Tokens";

        //        // We want the response to be JSON.
        //        client.DefaultRequestHeaders.Accept.Clear();
        //        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        //        // Add Basic Auth header
        //        string clientIdAndSecret = $"{clientId}:{clientSecret}";
        //        var authValue = Convert.ToBase64String(Encoding.ASCII.GetBytes(clientIdAndSecret));
        //        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authValue);

        //        // Build up the data to POST.
        //        List<KeyValuePair<string, string>> postData = new List<KeyValuePair<string, string>>();
        //        postData.Add(new KeyValuePair<string, string>("grant_type", "client_credentials"));

        //        FormUrlEncodedContent content = new FormUrlEncodedContent(postData);

        //        // Post to the Server and parse the response.
        //        HttpResponseMessage response = await client.PostAsync(authURL, content);
        //        string jsonString = await response.Content.ReadAsStringAsync();
        //        object responseData = JsonConvert.DeserializeObject(jsonString);

        //        // return the Access Token.
        //        return ((dynamic)responseData).access_token;
        //    }

        //}

        //private static async Task<dynamic> GetCompletedCourses(int page, string tags)
        //{
        //    //DateTime getAfterDate = DateTime.Now.AddDays(-14);
        //    //using (SqlConnection conn = new SqlConnection(_connectionString))
        //    //{
        //    //    string query = @"
        //    //                    SELECT Top(1) CreateDate 
        //    //                    FROM stgADBankerImport 
        //    //                    ORDER BY CreateDate ";

        //    //    using (SqlCommand cmd = new SqlCommand(query, conn))
        //    //    {
        //    //        //cmd.Parameters.Add(new SqlParameter("@GEID", courseInfo.Student.EmployeeId.ToString()));

        //    //        conn.Open();
        //    //        getAfterDate = (DateTime)cmd.ExecuteScalar();
        //    //    }
        //    //}

        //    using (var client = new HttpClient())
        //    {
        //        client.BaseAddress = new Uri(baseUrl);
        //        client.DefaultRequestHeaders.Accept.Clear();
        //        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        //        // Add the Authorization header with the AccessToken.
        //        client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);

        //        // create the URL string.
        //        string url = string.Format("v1/CETrackingOverview/GetAll", page, HttpUtility.UrlEncode(tags));

        //        // Create the request body.
        //        var requestBody = new
        //        {
        //            Limit = 150,
        //            Page = 0,
        //            SortField = "CourseCompletionDate",
        //            SortType = "asc",
        //            FilterContainer = new List<object>
        //            {
        //                new
        //                {
        //                    Filters = new List<object>
        //                    {
        //                        new
        //                        {
        //                            FieldName = "CourseCompletionDate",
        //                            Value = DateTime.Now.AddDays(-14).ToString(),
        //                            Comparator = "greaterThan"
        //                        }
        //                    }
        //                }
        //            }
        //        };


        //        // Convert the request body to JSON and then to StringContent.
        //        var json = JsonConvert.SerializeObject(requestBody);
        //        var data = new StringContent(json, Encoding.UTF8, "application/json");

        //        // make the request
        //        //HttpResponseMessage response = await client.GetAsync(url);
        //        HttpResponseMessage response = await client.PostAsync(url, data);

        //        // parse the response and return the data.
        //        string jsonString = await response.Content.ReadAsStringAsync();
        //        object responseData = JsonConvert.DeserializeObject(jsonString);

        //        return (dynamic)responseData;
        //    }
        //}

        //private static bool CheckImport(ViewModels.StudentCourseInfo vInput)
        //{
        //    if (vInput == null)
        //        return false;

        //    if (vInput.EmployeeID == null || vInput.EmployeeID == 0)
        //    {
        //        using (SqlConnection conn = new SqlConnection(_connectionString))
        //        {
        //            string query = @"
        //                SELECT a.TeamMemberID 
        //                FROM stg_ADBankerImport a 
        //                WHERE a.CourseState = @CourseState
        //                    AND a.StudentName = @StudentName
        //                    AND a.CourseTitle = @CourseTitle
        //                    AND a.CompletionDate = @CompletionDate
        //                    AND a.ReportedDate = @ReportedDate 
        //                    AND a.TotalCredits = @TotalCredits";

        //            using (SqlCommand cmd = new SqlCommand(query, conn))
        //            {
        //                cmd.Parameters.Add(new SqlParameter("@CourseState", vInput.CourseState));
        //                cmd.Parameters.Add(new SqlParameter("@StudentName", vInput.StudentName));
        //                cmd.Parameters.Add(new SqlParameter("@CourseTitle", vInput.CourseTitle));
        //                cmd.Parameters.Add(new SqlParameter("@CompletionDate", vInput.CompletionDate));
        //                cmd.Parameters.Add(new SqlParameter("@ReportedDate", vInput.ReportedDate));
        //                cmd.Parameters.Add(new SqlParameter("@TotalCredits", vInput.TotalCredits));

        //                conn.Open();
        //                var result = cmd.ExecuteScalar();
        //                return result != null;
        //            }
        //        }
        //    }
        //    else
        //    {
        //        using (SqlConnection conn = new SqlConnection(_connectionString))
        //        {
        //            string query = @"
        //                SELECT a.TeamMemberID 
        //                FROM stg_ADBankerImport a 
        //                WHERE a.TeamMemberID = @EmployeeID
        //                    AND a.CourseState = @CourseState
        //                    AND a.StudentName = @StudentName
        //                    AND a.CourseTitle = @CourseTitle
        //                    AND a.CompletionDate = @CompletionDate
        //                    AND a.ReportedDate = @ReportedDate 
        //                    AND a.TotalCredits = @TotalCredits";

        //            using (SqlCommand cmd = new SqlCommand(query, conn))
        //            {
        //                cmd.Parameters.Add(new SqlParameter("@EmployeeID", vInput.EmployeeID.ToString()));
        //                cmd.Parameters.Add(new SqlParameter("@CourseState", vInput.CourseState));
        //                cmd.Parameters.Add(new SqlParameter("@StudentName", vInput.StudentName));
        //                cmd.Parameters.Add(new SqlParameter("@CourseTitle", vInput.CourseTitle));
        //                cmd.Parameters.Add(new SqlParameter("@CompletionDate", vInput.CompletionDate));
        //                cmd.Parameters.Add(new SqlParameter("@ReportedDate", vInput.ReportedDate));
        //                cmd.Parameters.Add(new SqlParameter("@TotalCredits", vInput.TotalCredits));

        //                conn.Open();
        //                var result = cmd.ExecuteScalar();
        //                return result != null;
        //            }
        //        }
        //    }

            
        //}

        ///// <summary>
        ///// Gets the page of Questions.
        ///// </summary>
        ///// <param name="page">The page to get.</param>
        ///// <param name="tags">The tags to filter the articles with.</param>
        ///// <returns>The page of articles.</returns>
        //private static bool InsertEmployeeCE(ViewModels.StudentCourseInfo vInput)
        //{
        //    try
        //    {
        //        using (SqlConnection conn = new SqlConnection(_connectionString))
        //        {
        //            using (SqlCommand cmd = new SqlCommand("uspAgentContEducationTakenInsert", conn))
        //            {
        //                cmd.CommandType = CommandType.StoredProcedure;

        //                cmd.Parameters.Add(new SqlParameter("@ContEducationID", 1));
        //                cmd.Parameters.Add(new SqlParameter("@ContEducationRequirementID", vInput.ContEducationRequirementID));
        //                cmd.Parameters.Add(new SqlParameter("@ContEducationTakenDate", vInput.ContEducationTakenDate));
        //                cmd.Parameters.Add(new SqlParameter("@CreditHoursTaken", vInput.CreditHoursTaken));
        //                cmd.Parameters.Add(new SqlParameter("@AdditionalNotes", vInput.AdditionalNotes));
        //                cmd.Parameters.Add(new SqlParameter("@UserSOEID", "ADBanker-Api"));

        //                conn.Open();
        //                cmd.ExecuteNonQuery();
        //            }
        //        }

        //        return true;

        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("Error: " + ex.Message);
        //        LogError(ex.Message, "ADBankerImport-Api");
        //        return false;
        //    }
        //}

        //private static void CreateLog(string strApplication, string strMsg, string? strAdditionalInfo = null, string msgType = "ERROR")
        //{
        //    // Build the configuration
        //    var configuration = new ConfigurationBuilder()
        //        .SetBasePath(Directory.GetCurrentDirectory())
        //        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        //        .Build();

        //    // Get the environment
        //    var environment = configuration["Environment"];

        //    // Get the OneTrakV2LogPath for the current environment
        //    var oneTrakV2LogPath = configuration[$"EnvironmentSettings:{environment}:OneTrakV2LogPath"];

        //    try
        //    {
        //        string path = oneTrakV2LogPath + strApplication + "_" + System.DateTime.Today.ToString("yyyy-MM-dd") + ".log";

        //        if (!File.Exists(path))
        //        {
        //            File.Create(path).Dispose();

        //            using (TextWriter tw = new StreamWriter(path))
        //            {
        //                tw.WriteLine(strApplication + " ***** " + System.DateTime.Now.ToString() + Environment.NewLine);
        //                //tw.WriteLine(msgType == "ERROR" ? strAdditionalInfo : "INFO" + " => " + strMsg + Environment.NewLine);
        //                string strLog = string.Format("{0} => {1}", msgType == "ERROR" ? strAdditionalInfo : "INFO", strMsg);

        //                tw.WriteLine(strLog + Environment.NewLine);
        //                //tw.WriteLine(strAdditionalInfo + Environment.NewLine);
        //                tw.WriteLine(Environment.NewLine);
        //            }

        //        }
        //        else if (File.Exists(path))
        //        {
        //            using (StreamWriter w = File.AppendText(path))
        //            {
        //                w.WriteLine(strApplication + " ***** " + System.DateTime.Now.ToString() + Environment.NewLine);
        //                //w.WriteLine(msgType == "ERROR" ? strAdditionalInfo : "INFO" + " => " + strMsg + Environment.NewLine);
        //                string strLog = string.Format("{0} => {1}", msgType == "ERROR" ? strAdditionalInfo : "INFO", strMsg);

        //                w.WriteLine(strLog + Environment.NewLine);
        //                //w.WriteLine(strAdditionalInfo + Environment.NewLine);
        //                w.WriteLine(Environment.NewLine);
        //            }
        //        }

        //    }

        //    catch (Exception ex)
        //    {
        //        //SendEmail("wcfOneTrak Error", myex.Message.ToString() + Environment.NewLine + myex.StackTrace.ToString() + Environment.NewLine + myex.TargetSite.Name.ToString());
        //        LogError(ex.Message, "ADBankerImport-Api");
        //    }

        //}

        //public static void LogInfo(string vInfoText, string? vInfoSource = null)
        //{
        //    CreateLog("ADBankerImport-Info", vInfoText, vInfoSource, "INFO");
        //}

        //private static void LogError(string vErrorText, string vErrorSource, object? errorObject = null, string? vUserSOEID = null)
        //{
        //    CreateLog("ADBankerImport-Error", vErrorText, vErrorSource, "ERROR");
        //}
    }

    //public class StudentCourseInfo
    //{
    //    public int? EmployeeID { get; set; }
    //    public string? CourseState { get; set; }
    //    public string? StudentName { get; set; }
    //    public string? CourseTitle { get; set; }
    //    public DateTime CompletionDate { get; set; }
    //    public DateTime ReportedDate { get; set; }
    //    public double TotalCredits { get; set; }


    //    public int ContEducationRequirementID { get; set; }
    //    public DateTime ContEducationTakenDate { get; set; }
    //    public double CreditHoursTaken { get; set; }
    //    public string? AdditionalNotes { get; set; }
    //}
}
