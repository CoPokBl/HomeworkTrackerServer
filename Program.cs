using System;
using System.Collections.Generic;
using HomeworkTrackerServer.Storage;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using RayKeys.Misc;

namespace HomeworkTrackerServer {
    public static class Program {
        
        public static LogLevel LoggingLevel = LogLevel.Debug;
        public static IStorageMethod Storage;
        public static readonly Version Ver = new Version(0, 8, 0);
        public const string WwwAuthHeader = "Bearer realm=\"HomeworkAccounts\"";
        public static bool Debug;
        public static Dictionary<string, string> Config;
        
        public static void Main(string[] args) {
            
            // Debug option
            if (args.Length > 0 && args[0] == "debug") {
                Debug = true;
            }
            
            try {
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception e) {
                Logger.Error("Program exited with an error: " + e.Message);
                Logger.Debug(e.ToString());
            }
            Logger.Info("Exiting");
            try {
                Storage.Deinit();
            }
            catch (Exception e) {
                Logger.Error("Storage deinit failed: " + e.Message);
                Logger.Debug(e.ToString());
            }
            Logger.WaitFlush();
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => {
                    webBuilder.UseStartup<Startup>().UseUrls("http://*:9898");
                });

    }
    
}
