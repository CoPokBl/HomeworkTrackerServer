using System;
using System.Threading.Tasks;
using HomeworkTrackerServer.Storage;

namespace HomeworkTrackerServer {
    internal static class Program {
        private const int LoggingLevel = 3;
        public static readonly IStorageMethod Storage = new RamStorage();
        public static Version Ver = new Version(0, 2, 1);

        private static int Main(string[] args) {
            Debug("Starting Async Method...");
            return AsyncMain(args).GetAwaiter().GetResult();
            // Unreachable
        }

        private static async Task<int> AsyncMain(string[] args) {
            // init storage
            Info("Initialising Storage");
            Storage.Init();
            Info("Initialised Storage");
            
            Info("Attempting to start HTTP server...");
            while (true) {  // If it crashes then restart because who cares
                try {
                    await HttpServer.Start();
                }
                catch (Exception e) {
                    Error(e.ToString());
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