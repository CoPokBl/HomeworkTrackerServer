using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace RayKeys.Misc {
    public static class Logger {
        private static LogLevel _loggingLevel;
        private static FileStream _logFile;
        private static StreamWriter _streamWriter;
        private static Task _writeTask;
        private static string _typeText;
        
        public static void Log(string log, LogLevel level) {
            if (_loggingLevel < level) return;
            
            log = $"[{DateTime.Now.ToLongTimeString()}] [{level}]: {log}\n";
            Console.Write(log);
            
            _typeText += log;

            if (_writeTask.IsCompleted) {
                _writeTask = _streamWriter.WriteAsync(_typeText);
                _typeText = "";
            }
        }

        public static void WaitFlush() {
            _writeTask.Wait();

            _streamWriter.Write(_typeText);
            _typeText = "";
        }

        public static void Init(LogLevel logLevel) {
            _loggingLevel = logLevel;

            if (!Directory.Exists("Logs")) Directory.CreateDirectory("Logs");
            
            if (File.Exists("Logs/latest.log")) {
                using FileStream originalFileStream = File.Open("Logs/latest.log", FileMode.Open);
                string gzFileLoc = new StreamReader(originalFileStream).ReadLine();

                try {
                    gzFileLoc = "Logs" + gzFileLoc[gzFileLoc.LastIndexOf('/')..] + ".gz";
                }
                catch (Exception) {
                    gzFileLoc = "Logs/Unknown-" + 
                                (int)(new Random().Next()*1000000*3.141592653589793238462643383279502884197169) + ".log.gz";
                }

                originalFileStream.Seek(0, SeekOrigin.Begin);

                using FileStream compressedFileStream = File.Create(gzFileLoc);
                using GZipStream compressor = new GZipStream(compressedFileStream, CompressionMode.Compress);
                originalFileStream.CopyTo(compressor);
                
                File.Delete("Logs/latest.log");
            }

            string logFileName = $"Logs/{DateTime.Now:yyyy-MM-dd}-";

            int i;
            for (i = 1; File.Exists(logFileName + i + ".log.gz"); i++) { }

            logFileName += i + ".log";
            
            _logFile = File.Create("Logs/latest.log");
            _streamWriter = new StreamWriter(_logFile);
            _streamWriter.AutoFlush = true;
            _writeTask = Task.CompletedTask;
            _typeText = "";
            Logger.Info($"Logging to: {logFileName}");
        }
        
        public static void Error(string log) {
            Log(log, LogLevel.Error);
        }
        
        public static void Info(string log) {
            Log(log, LogLevel.Info);
        }
        
        public static void Debug(string log) {
            Log(log, LogLevel.Debug);
        }
    }

    public enum LogLevel {
        Error,
        Info,
        Debug
    }
}