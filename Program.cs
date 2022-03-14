using System;
using HomeworkTrackerServer.Objects;
using HomeworkTrackerServer.Storage;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using RayKeys.Misc;

namespace HomeworkTrackerServer {
    public class Program {
        
        public static LogLevel LoggingLevel = LogLevel.Debug;
        public static IStorageMethod Storage;
        public static TokenHandler TokenHandler;
        public static readonly Version Ver = new Version(0, 7, 0);
        public const string WwwAuthHeader = "Bearer realm=\"HomeworkAccounts\"";
        public static void Main(string[] args) {
            try {
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception e) {
                Logger.Error("Program exited with an error: " + e.Message);
                Logger.Debug(e.ToString());
            }
            Logger.Info("Exiting");
            Logger.WaitFlush();
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => {
                    webBuilder.UseStartup<Startup>();
                });

    }
    
}
