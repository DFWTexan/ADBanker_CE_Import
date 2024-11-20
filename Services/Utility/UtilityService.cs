using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADBanker_CE_Import.Services
{
    public class UtilityService : IUtilityService
    {
        public void CreateLog(string strApplication, string strMsg, string? strAdditionalInfo = null, string msgType = "ERROR")
        {
            // Build the configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            // Get the environment
            var environment = configuration["Environment"];

            // Get the OneTrakV2LogPath for the current environment
            var oneTrakV2LogPath = configuration[$"EnvironmentSettings:{environment}:OneTrakV2LogPath"];

            try
            {
                string path = oneTrakV2LogPath + strApplication + "_" + System.DateTime.Today.ToString("yyyy-MM-dd") + ".log";

                if (!File.Exists(path))
                {
                    File.Create(path).Dispose();

                    using (TextWriter tw = new StreamWriter(path))
                    {
                        tw.WriteLine(strApplication + " ***** " + System.DateTime.Now.ToString() + Environment.NewLine);
                        //tw.WriteLine(msgType == "ERROR" ? strAdditionalInfo : "INFO" + " => " + strMsg + Environment.NewLine);
                        string strLog = string.Format("{0} => {1}", msgType == "ERROR" ? strAdditionalInfo : "INFO", strMsg);

                        tw.WriteLine(strLog + Environment.NewLine);
                        //tw.WriteLine(strAdditionalInfo + Environment.NewLine);
                        tw.WriteLine(Environment.NewLine);
                    }

                }
                else if (File.Exists(path))
                {
                    using (StreamWriter w = File.AppendText(path))
                    {
                        w.WriteLine(strApplication + " ***** " + System.DateTime.Now.ToString() + Environment.NewLine);
                        //w.WriteLine(msgType == "ERROR" ? strAdditionalInfo : "INFO" + " => " + strMsg + Environment.NewLine);
                        string strLog = string.Format("{0} => {1}", msgType == "ERROR" ? strAdditionalInfo : "INFO", strMsg);

                        w.WriteLine(strLog + Environment.NewLine);
                        //w.WriteLine(strAdditionalInfo + Environment.NewLine);
                        w.WriteLine(Environment.NewLine);
                    }
                }

            }

            catch (Exception ex)
            {
                //SendEmail("wcfOneTrak Error", myex.Message.ToString() + Environment.NewLine + myex.StackTrace.ToString() + Environment.NewLine + myex.TargetSite.Name.ToString());
                LogError(ex.Message, "ADBankerImport-Api");
            }

        }

        public void LogInfo(string vInfoText, string? vInfoSource = null)
        {
            CreateLog("ADBankerImport-Info", vInfoText, vInfoSource, "INFO");
        }

        public void LogError(string vErrorText, string vErrorSource, object? errorObject = null, string? vUserSOEID = null)
        {
            CreateLog("ADBankerImport-Error", vErrorText, vErrorSource, "ERROR");
        }
    }
}
