using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADBanker_CE_Import.Services
{
    public interface IDatabaseService
    {
        public void PermissionTest();
        public bool CheckImport(ViewModels.StudentCourseInfo vInput);
        public bool InsertEmployeeCE(ViewModels.StudentCourseInfo vInput);
    }
}
