using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using HomeworkTrackerServer.Storage;
using Newtonsoft.Json;

// TODO: Add to homepage.html

namespace HomeworkTrackerServer {
    internal static class Program {
        private const int LoggingLevel = 3;
        public static readonly IStorageMethod Storage = new MySQLStorage();
        public static readonly Version Ver = new Version(0, 4, 2);
        public static Dictionary<string, string> Config;

        private static int Main(string[] args) {
            Info("Loading Config...");
            if (!File.Exists("config.json")) {
                Error("Config file (config.json) doesn't exist, please create it or rebuild project");
                return 1;
            }
            Debug("Found file!");
            Debug("Deserializing text...");
            try {
                Config = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText("config.json"));
                if (Config == null) {
                    throw new System.Text.Json.JsonException("Empty file");
                }
            } catch (JsonException) { Error("Invalid config file"); return 1; }
            
            Debug("Loaded config file successfully!\nValues:\n---------------------");
            foreach (var (key, value) in Config) { Debug($"{key}, {value}"); }
            Debug("---------------------");
            
            Debug("Starting Async Method...");
            return AsyncMain(args).GetAwaiter().GetResult();
            // Unreachable 
        }

        private static async Task<int> AsyncMain(string[] args) {
            // init storage
            
            Info("Initialising Storage");
            try {
                Storage.Init();
            }
            catch (Exception e) {
                Error("Failed to initialize storage: " + e.Message);
                Debug(e.ToString());  // Debug whole error
                return 1;
            }
            Info("Initialised Storage");
            
            Info("Attempting to start HTTP server...");
            while (true) {  // If it crashes then restart because who cares
                try {
                    await HttpServer.Start();
                }
                catch (Exception e) {
                    Error(e.ToString());  // Say error
                    HttpServer._listener.Prefixes.Clear(); // Remove all prefixes
                }
            }
        }

#pragma warning disable CS0162
        public static void Debug(string msg, Severity severity = Severity.Debug) {
            if (LoggingLevel > (int) severity) {
                
                ConsoleColor textColour = severity switch {
                    Severity.Error => ConsoleColor.Red,
                    Severity.Info => ConsoleColor.Green,
                    _ => ConsoleColor.White
                };
                Console.ForegroundColor = textColour;
                Console.WriteLine($"[{DateTime.Now}] [{severity}]: {msg}");
                Console.ForegroundColor = ConsoleColor.White;
            }
        }
        
#pragma warning restore CS0162
        
        public static void Info(string msg) { Debug(msg, Severity.Info); }
        public static void Error(string msg) { Debug(msg, Severity.Error); }


    }
    
    public enum Severity {
        Error,
        Info,
        Debug
    }
}