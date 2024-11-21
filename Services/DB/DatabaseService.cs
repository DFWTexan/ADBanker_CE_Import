using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;


namespace ADBanker_CE_Import.Services
{
    public class DatabaseService : IDatabaseService
    {
        private readonly IConfiguration _config;
        private readonly IUtilityService _utilityService;
        private readonly string? _connectionString;

        public DatabaseService(IConfiguration config, IUtilityService utilityService)
        {
            _config = config;
            _utilityService = utilityService;
            var environment = _config["Environment"] ?? "PROD";
            _connectionString = _config[$"EnvironmentSettings:{environment}:DefaultConnection"] ?? string.Empty;
        }

        public bool CheckImport(ViewModels.StudentCourseInfo vInput)
        {
            if (vInput == null)
                return false;

            if (vInput.EmployeeID == null || vInput.EmployeeID == 0)
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    string query = @"
                        SELECT a.TeamMemberID 
                        FROM stg_ADBankerImport a 
                        WHERE a.CourseState = @CourseState
                            AND a.StudentName = @StudentName
                            AND a.CourseTitle = @CourseTitle
                            AND a.CompletionDate = @CompletionDate
                            AND a.ReportedDate = @ReportedDate 
                            AND a.TotalCredits = @TotalCredits";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.Add(new SqlParameter("@CourseState", vInput.CourseState));
                        cmd.Parameters.Add(new SqlParameter("@StudentName", vInput.StudentName));
                        cmd.Parameters.Add(new SqlParameter("@CourseTitle", vInput.CourseTitle));
                        cmd.Parameters.Add(new SqlParameter("@CompletionDate", vInput.CompletionDate));
                        cmd.Parameters.Add(new SqlParameter("@ReportedDate", vInput.ReportedDate));
                        cmd.Parameters.Add(new SqlParameter("@TotalCredits", vInput.TotalCredits));

                        conn.Open();
                        var result = cmd.ExecuteScalar();
                        return result != null;
                    }
                }
            }
            else
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    string query = @"
                        SELECT a.TeamMemberID 
                        FROM stg_ADBankerImport a 
                        WHERE a.TeamMemberID = @EmployeeID
                            AND a.CourseState = @CourseState
                            AND a.StudentName = @StudentName
                            AND a.CourseTitle = @CourseTitle
                            AND a.CompletionDate = @CompletionDate
                            AND a.ReportedDate = @ReportedDate 
                            AND a.TotalCredits = @TotalCredits";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.Add(new SqlParameter("@EmployeeID", vInput.EmployeeID.ToString()));
                        cmd.Parameters.Add(new SqlParameter("@CourseState", vInput.CourseState));
                        cmd.Parameters.Add(new SqlParameter("@StudentName", vInput.StudentName));
                        cmd.Parameters.Add(new SqlParameter("@CourseTitle", vInput.CourseTitle));
                        cmd.Parameters.Add(new SqlParameter("@CompletionDate", vInput.CompletionDate));
                        cmd.Parameters.Add(new SqlParameter("@ReportedDate", vInput.ReportedDate));
                        cmd.Parameters.Add(new SqlParameter("@TotalCredits", vInput.TotalCredits));

                        conn.Open();
                        var result = cmd.ExecuteScalar();
                        return result != null;
                    }
                }
            }


        }

        public bool InsertEmployeeCE(ViewModels.StudentCourseInfo vInput)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("uspAgentContEducationTakenInsert", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add(new SqlParameter("@ContEducationID", 1));
                        cmd.Parameters.Add(new SqlParameter("@ContEducationRequirementID", vInput.ContEducationRequirementID));
                        cmd.Parameters.Add(new SqlParameter("@ContEducationTakenDate", vInput.ContEducationTakenDate));
                        cmd.Parameters.Add(new SqlParameter("@CreditHoursTaken", vInput.CreditHoursTaken));
                        cmd.Parameters.Add(new SqlParameter("@AdditionalNotes", vInput.AdditionalNotes));
                        cmd.Parameters.Add(new SqlParameter("@UserSOEID", "ADBanker-Api"));

                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }

                return true;

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                _utilityService.LogError(ex.Message, "ADBankerImport-Api");
                return false;
            }
        }
    }
}
