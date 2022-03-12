using System;
using HomeworkTrackerServer.Objects;
using HomeworkTrackerServer.Storage;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace HomeworkTrackerServer {
    public class Program {
        
        public static int LoggingLevel = 3;
        public static IStorageMethod Storage;
        public static TokenHandler TokenHandler;
        public static readonly Version Ver = new Version(0, 6, 3);
        public const string WwwAuthHeader = "Bearer realm=\"HomeworkAccounts\"";
        public static void Main(string[] args) {
            try {
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception e) {
                Error("Program exited with an error: " + e.Message);
                Debug(e.ToString());
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => {
                    webBuilder.UseStartup<Startup>();
                });
        
        
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

        public static void Info(string msg) { Debug(msg, Severity.Info); }
        public static void Error(string msg) { Debug(msg, Severity.Error); }

    }
    
    public enum Severity {
        Error,
        Info,
        Debug
    }
}
