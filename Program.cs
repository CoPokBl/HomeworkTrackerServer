/*

Welcome To Homework Tracker Server's Source Code

Things to do TODO:

- Push notifications using OneSignal
- More robust error handling

 */

using System;
using System.Collections.Generic;
using System.IO;
using HomeworkTrackerServer.Storage;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace HomeworkTrackerServer; 

public static class Program {
    public static IStorageMethod Storage { get; set; }
    public static readonly Version Ver = new(0, 9, 0);
    public static readonly string WwwAuthHeader = "Bearer realm=\"HomeworkAccounts\"";
    public static bool Debug { get; private set; }
    public static bool StorageInitialized { get; set; }
    public static Dictionary<string, string> Config { get; private set; }

    public static void Main(string[] args) {
        
        Console.WriteLine("Starting Log Initialization");

        // Initialize logging (LogLevel gets updated once config is loaded)
        Logger.Init(LogLevel.Debug);
        
        // Apply args
        int argCounter = 0;
        while (argCounter < args.Length) {
            switch (args[argCounter]) {
                
                case "--debug":
                    Debug = true;
                    Logger.Info("Debug mode enabled");
                    break;
                
                case "--directory":
                    try {
                        Directory.SetCurrentDirectory(args[argCounter+1]);
                    }
                    catch (Exception) {
                        Logger.Error("Invalid directory");
                        return;
                    }
                    Logger.Info("Set active directory to " + Directory.GetCurrentDirectory());
                    argCounter++;  // Skip next arg because it's the directory
                    break;
                
                case "--config":
                    ConfigManager.ConfigFileName = args[argCounter+1];
                    Logger.Info("Set config file to: " + args[argCounter+1]);
                    argCounter++;  // Skip next arg because it's the config file
                    break;
                
                default:
                    Logger.Error($"Invalid Argument: {args[argCounter]}");
                    Logger.Info("Exiting...");
                    return;

            }

            argCounter++;
        }

        // Print info
        Logger.Info($"Starting Homework Tracker Server v{Ver}");
        Logger.Info($"Active Directory: {Directory.GetCurrentDirectory()}");
            
        // Custom config because ASP.NET's config system is stupid
        Logger.Info("Loading config...");
        try {
            Config = ConfigManager.LoadConfig();  // Attempt to load config and correct any errors
        }
        catch (InvalidConfigException) {
            Logger.Error("Failed to load config, config is invalid!");
            throw;
        }
        Logger.Info("Loaded config");

        Logger.LoggingLevel = (LogLevel) int.Parse(Config["LoggingLevel"]);

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
                Logger.Error("Storage deinit failed: ");
                Logger.Error(e);
            }
        } else { Logger.Info("Storage not initialized, skipping deinit"); }
            
        Logger.Info("Bye!");
        Logger.WaitFlush();  // Flush all logs
    }

    // Yes I'm aware that http is insecure but you should just use a reverse proxy for https
    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder => {
                webBuilder.UseStartup<Startup>().UseUrls(Config.ContainsKey("bind_address") ? Config["bind_address"] : "http://*:9898");
            });

}