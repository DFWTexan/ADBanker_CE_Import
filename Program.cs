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
    }
}
