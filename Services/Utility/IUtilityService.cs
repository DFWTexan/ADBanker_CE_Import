using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADBanker_CE_Import.Services
{
    public interface IUtilityService
    {
        public void CreateLog(string strApplication, string strMsg, string? strAdditionalInfo = null, string msgType = "ERROR");
        public void LogInfo(string vInfoText, string? vInfoSource = null);
        public void LogError(string vErrorText, string vErrorSource, object? errorObject = null, string? vUserSOEID = null);
    }
}
