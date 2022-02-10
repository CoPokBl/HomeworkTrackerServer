using System;
using System.Threading.Tasks;
using HomeworkTrackerServer.Storage;

namespace HomeworkTrackerServer {
    internal static class Program {
        private const int LoggingLevel = 3;
        public static readonly IStorageMethod Storage = new RamStorage();

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
            await HttpServer.Start();
            Info("HTTP server stopped");
            Info("Exiting...");
            return 69;
        }

        public static void Debug(string msg, Severity severity = Severity.Debug) {
            if (LoggingLevel > (int) severity) {
                Console.WriteLine($"[{DateTime.Now}] [{severity}]: {msg}");
            }
        }
        
        public static void Info(string msg) { Debug(msg, Severity.Info); }
        public static void Error(string msg) { Debug(msg, Severity.Error); }


    }
    
    public enum Severity {
        Error,
        Info,
        Debug
    }
}