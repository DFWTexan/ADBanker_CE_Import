using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADBanker_CE_Import.ViewModels
{
    public class StudentCourseInfo
    {
        public int? EmployeeID { get; set; }
        public string? CourseState { get; set; }
        public string? StudentName { get; set; }
        public string? CourseTitle { get; set; }
        public DateTime CompletionDate { get; set; }
        public DateTime ReportedDate { get; set; }
        public double TotalCredits { get; set; }


        public int ContEducationRequirementID { get; set; }
        public DateTime ContEducationTakenDate { get; set; }
        public double CreditHoursTaken { get; set; }
        public string? AdditionalNotes { get; set; }
    }
}
