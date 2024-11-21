using ADBanker_CE_Import.Services;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;


namespace ADBanker_CE_Import
{
    public class ImportProcessor
    {
        private readonly IConfiguration _config;
        private readonly ILogger<ImportProcessor> _logger;
        private readonly IApiService _apiService;
        private readonly IUtilityService _utilityService;
        private readonly IDatabaseService _databaseService;
        private readonly string? _connectionString;

        public ImportProcessor(IConfiguration configuration, ILogger<ImportProcessor> logger, IUtilityService utilityService, IApiService apiService, IDatabaseService databaseService)
        {
            _config = configuration;
            var environment = _config["Environment"] ?? "PROD";
            _connectionString = _config[$"EnvironmentSettings:{environment}:DefaultConnection"] ?? string.Empty;
            _logger = logger;
            _apiService = apiService;
            _utilityService = utilityService;
            _databaseService = databaseService;
        }

        public void Run()
        {
            _utilityService.LogInfo("Starting Import of CE Completion.");
            _logger.LogInformation("Import Processor Started");

            string? baseUrl = _config["Api:BaseUrl"];
            string? clientId = _config["Api:ClientId"];
            string? clientSecret = _config["Api:ClientSecret"];
            string? tags = _config["Api:Tags"];
            int studentCount = 0;

            // Get the Access Token.
            string accessToken = _apiService.GetAccessToken().Result;

            // Get the Articles.
            int page = 1;
            dynamic response = _apiService.GetCompletedCourses(accessToken, page, tags).Result;

            if (response.Results != null)
            {
                //studentCount = response.Results.Count;
                // LOG - Date of Import and Count of Records
                var studentCourses = (dynamic)response.Results;
                foreach (dynamic courseInfo in studentCourses)
                {
                    if (courseInfo.Student.EmployeeId == null || string.IsNullOrEmpty(courseInfo.Student.EmployeeId.ToString()))
                    {
                        Console.WriteLine("TeamMemberID is null or empty. Skipping this record.");
                        _utilityService.LogInfo(string.Format("TeamMemberID is null or empty for - Name: {0} {1} state: {2}. Skipping this record.", courseInfo.Student.FirstName, courseInfo.Student.LastName, courseInfo.Student.StateCode));

                        var isFound = _databaseService.CheckImport(new ViewModels.StudentCourseInfo
                        {
                            EmployeeID = null,
                            CourseState = courseInfo.Student.StateCode,
                            StudentName = courseInfo.Student.FirstName + " " + courseInfo.Student.LastName,
                            CourseTitle = courseInfo.Course.CourseTitle,
                            CompletionDate = Convert.ToDateTime((string)courseInfo.StudentCourse.CourseCompletionDate),
                            ReportedDate = Convert.ToDateTime((string)courseInfo.StudentCourse.ReportedDate),
                            TotalCredits = (double)courseInfo.Course.Credits
                        });

                        if (isFound)  // Do not insert if the record is already in the system
                            continue;

                        using (SqlConnection conn = new SqlConnection(_connectionString))
                        {
                            using (SqlCommand cmd = new SqlCommand("INSERT INTO [dbo].[stg_ADBankerImport] ([TeamMemberID], [CourseState], [StudentName], [CourseTitle], [CompletionDate], [ReportedDate], [TotalCredits], [CreateDate], [IsImportComplete], [SortOrder]) VALUES (@TeamMemberID, @CourseState, @StudentName, @CourseTitle, @CompletionDate, @ReportedDate, @TotalCredits, @CreateDate, @IsImportComplete, @SortOrder)", conn))
                            {
                                cmd.Parameters.Add(new SqlParameter("@TeamMemberID", (string)courseInfo.Student.EmployeeId));
                                cmd.Parameters.Add(new SqlParameter("@CourseState", (string)courseInfo.Student.StateCode));
                                cmd.Parameters.Add(new SqlParameter("@StudentName", (string)courseInfo.Student.FirstName + " " + (string)courseInfo.Student.LastName));
                                cmd.Parameters.Add(new SqlParameter("@CourseTitle", (string)courseInfo.Course.CourseTitle));
                                cmd.Parameters.Add(new SqlParameter("@CompletionDate", Convert.ToDateTime((string)courseInfo.StudentCourse.CourseCompletionDate)));
                                cmd.Parameters.Add(new SqlParameter("@ReportedDate", Convert.ToDateTime((string)courseInfo.StudentCourse.ReportedDate)));
                                cmd.Parameters.Add(new SqlParameter("@TotalCredits", (double)courseInfo.Course.Credits));
                                cmd.Parameters.Add(new SqlParameter("@CreateDate", DateTime.Now));
                                bool isImportComplete = false;
                                cmd.Parameters.Add(new SqlParameter("@IsImportComplete", isImportComplete ? 1 : 0));
                                cmd.Parameters.Add(new SqlParameter("@SortOrder", 1));

                                conn.Open();
                                cmd.ExecuteNonQuery();
                            }
                        }
                        continue; // Skip this iteration if EmployeeId is null or empty
                    }

                    Console.WriteLine("  {0} - Team #: {1} - Name: {2} {3} - NP#: {4} - Title: {5} - CompDate: {6} - RptDate: {7} - Credits: {8}",
                        courseInfo.Student.StateCode,
                        courseInfo.Student.EmployeeId,
                        courseInfo.Student.FirstName,
                        courseInfo.Student.LastName,
                        courseInfo.Student.NationalProducerNumber > 0 ? courseInfo.Student.NationalProducerNumber : "N/A",
                        courseInfo.Course.CourseTitle,
                        courseInfo.StudentCourse.CourseCompletionDate.ToString("MM/dd/yyyy"),
                        courseInfo.StudentCourse.ReportedDate.ToString("MM/dd/yyyy") != "" ? courseInfo.StudentCourse.ReportedDate.ToString("MM/dd/yyyy") : "N/A",
                        courseInfo.Course.Credits);

                    int conEduReqID;
                    try
                    {
                        using (SqlConnection conn = new SqlConnection(_connectionString))
                        {
                            string query = @"
                                            SELECT c.ContEducationRequirementID 
                                            FROM Employee a 
                                            JOIN Employment b ON a.EmployeeID = b.EmployeeID
                                            JOIN ContEducationRequirement c ON b.EmploymentID = c.EmploymentID
                                            WHERE a.GEID = @GEID";

                            using (SqlCommand cmd = new SqlCommand(query, conn))
                            {
                                cmd.Parameters.Add(new SqlParameter("@GEID", courseInfo.Student.EmployeeId.ToString()));

                                conn.Open();
                                //conEduReqID = (int)cmd.ExecuteScalar();
                                var result = cmd.ExecuteScalar();
                                conEduReqID = result != null ? (int)result : -1; // Handle null by assigning -1
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error: " + ex.Message);
                        conEduReqID = -1;
                        _utilityService.LogError(ex.Message, "ADBankerImport-Api");
                    }

                    if (conEduReqID == -1)
                    {
                        Console.WriteLine("Employee not found in the system. Skipping this record.");
                        _utilityService.LogInfo(string.Format("Employee not found in the system. Skipping this record. EmployeeID: {0}", courseInfo.Student.EmployeeId), null);
                        continue;
                    }

                    var studentCourseInfo = new ViewModels.StudentCourseInfo
                    {
                        EmployeeID = courseInfo.Student.EmployeeId,
                        CourseState = courseInfo.Student.StateCode,
                        StudentName = courseInfo.Student.FirstName + " " + courseInfo.Student.LastName,
                        CourseTitle = courseInfo.Course.CourseTitle,
                        CompletionDate = Convert.ToDateTime((string)courseInfo.StudentCourse.CourseCompletionDate),
                        ReportedDate = Convert.ToDateTime((string)courseInfo.StudentCourse.ReportedDate),
                        TotalCredits = (double)courseInfo.Course.Credits,

                        ContEducationRequirementID = conEduReqID,
                        ContEducationTakenDate = courseInfo.StudentCourse.CourseCompletionDate,
                        CreditHoursTaken = courseInfo.Course.Credits,
                        AdditionalNotes = string.Format("ADBnkApi_{0} - {1}", DateTime.Now.ToString("yyyyMMdd"), courseInfo.Course.CourseTitle)
                    };

                    try
                    {
                        if (!_databaseService.CheckImport(studentCourseInfo))
                        {
                            studentCount++;
                            using (SqlConnection conn = new SqlConnection(_connectionString))
                            {
                                using (SqlCommand cmd = new SqlCommand("INSERT INTO [dbo].[stg_ADBankerImport] ([TeamMemberID], [CourseState], [StudentName], [CourseTitle], [CompletionDate], [ReportedDate], [TotalCredits], [CreateDate], [IsImportComplete], [SortOrder]) VALUES (@TeamMemberID, @CourseState, @StudentName, @CourseTitle, @CompletionDate, @ReportedDate, @TotalCredits, @CreateDate, @IsImportComplete, @SortOrder)", conn))
                                {
                                    cmd.Parameters.Add(new SqlParameter("@TeamMemberID", (string)courseInfo.Student.EmployeeId));
                                    cmd.Parameters.Add(new SqlParameter("@CourseState", (string)courseInfo.Student.StateCode));
                                    cmd.Parameters.Add(new SqlParameter("@StudentName", (string)courseInfo.Student.FirstName + " " + (string)courseInfo.Student.LastName));
                                    cmd.Parameters.Add(new SqlParameter("@CourseTitle", (string)courseInfo.Course.CourseTitle));
                                    cmd.Parameters.Add(new SqlParameter("@CompletionDate", Convert.ToDateTime((string)courseInfo.StudentCourse.CourseCompletionDate)));
                                    cmd.Parameters.Add(new SqlParameter("@ReportedDate", Convert.ToDateTime((string)courseInfo.StudentCourse.ReportedDate)));
                                    cmd.Parameters.Add(new SqlParameter("@TotalCredits", (double)courseInfo.Course.Credits));
                                    cmd.Parameters.Add(new SqlParameter("@CreateDate", DateTime.Now));
                                    bool isImportComplete = _databaseService.InsertEmployeeCE(studentCourseInfo);
                                    cmd.Parameters.Add(new SqlParameter("@IsImportComplete", isImportComplete ? 1 : 0));
                                    cmd.Parameters.Add(new SqlParameter("@SortOrder", 2));

                                    conn.Open();
                                    cmd.ExecuteNonQuery();
                                }
                            }
                        };

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error: " + ex.Message);
                        _utilityService.LogError(ex.Message, "ADBankerImport-Api");
                    }

                }
                // LOG - Date of Import and Count of Records
                _utilityService.LogInfo(string.Format(" ==> ADBanker {0} Imported..", studentCount), null);
            }

            //return 0;
            _utilityService.LogInfo("Import of CE Completion Completed.");

            _logger.LogInformation("Import Processor Finished");
        }
    }
}
