using System;
using System.Collections.Generic;
using System.IO;
using HomeworkTrackerServer.Storage;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;

namespace HomeworkTrackerServer {
    public static class Program {
        
        public static LogLevel LoggingLevel = LogLevel.Debug;
        public static IStorageMethod Storage;
        public static readonly Version Ver = new Version(0, 8, 0);
        public const string WwwAuthHeader = "Bearer realm=\"HomeworkAccounts\"";
        public static bool Debug;
        public static bool StorageInitialized = false;
        public static Dictionary<string, string> Config;
        
        public static void Main(string[] args) {
            
            // Initialize logging (There are log commands before this but they will still work)
            Logger.Init(LogLevel.Debug);
            
            // Debug option (This option exists because of stupid ASP.NET stuff, I will remove eventually in favour of Logger.Debug)
            if (args.Length > 0 && args[0] == "debug") {
                Debug = true;
            }
            
            // Custom config because ASP.NETs config system is stupid
            try {
                // Don't bother creating a default config because they get it when they build
                if (!File.Exists("config.json")) {
                    throw new FileNotFoundException("config.json not found, please create it or rebuild the project");
                }
                // Get config data
                string data = File.ReadAllText("config.json");
                Dictionary<string, string> configDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(data);
                Config = configDict;
                Logger.Info("Loaded config");
            }
            catch (Exception e) {
                // Failed to load config
                Logger.Error($"Failed to load config {e.Message}");
                throw new Exception("Failed to load config");
            }
            
            Logger._loggingLevel = (LogLevel) int.Parse(Config["LoggingLevel"]);

            // Run actual server (Catch all errors)
            try {
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception e) {
                Logger.Error("Program exited with an error: " + e.Message);
                Logger.Error(e.ToString());
            }
            Logger.Info("Exiting");
            if (StorageInitialized) {
                Logger.Info("Closing storage");
                try {
                    Storage.Deinit();
                }
                catch (Exception e) {
                    Logger.Error("Storage deinit failed: " + e.Message);
                    Logger.Debug(e.ToString());
                }
            } else Logger.Info("Storage not initialized, skipping deinit");
            
            Logger.Info("Bye!");
            Logger.WaitFlush();  // Flush all logs
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => {
                    webBuilder.UseStartup<Startup>().UseUrls(Config.ContainsKey("bind_address") ? Config["bind_address"] : "http://*:9898");
                });

    }
    
}
