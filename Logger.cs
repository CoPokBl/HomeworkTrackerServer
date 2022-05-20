/*
 
 Credit to Calcilore (https://github.com/Calcilore) for this file
 Original: https://github.com/Calcilore/RayKeys/blob/main/Misc/Logger.cs
 
 */

using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace HomeworkTrackerServer; 

public static class Logger {
    public static LogLevel LoggingLevel { get; set; } = LogLevel.Debug;
    private static FileStream _logFile;
    private static StreamWriter _streamWriter;
    private static Task _writeTask = Task.CompletedTask;
    private static string _typeText;
        
    public static void Log(object logObj, LogLevel level) {
        if (LoggingLevel < level) { return; }
            
        string log = $"[{DateTime.Now.ToLongTimeString()}] [{level}]: {logObj}\n";
        Console.Write(log);
            
        _typeText += log;

        if (!_writeTask.IsCompleted) { return; }
        _writeTask = _streamWriter.WriteAsync(_typeText);
        _typeText = "";
    }

    public static void WaitFlush() {
        _writeTask.Wait();

        _streamWriter.Write(_typeText);
        _typeText = "";
    }

    public static void Init(LogLevel logLevel) {
        LoggingLevel = logLevel;

        if (!Directory.Exists("Logs")) { Directory.CreateDirectory("Logs"); }
            
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
        }

        string logFileName = $"Logs/{DateTime.Now:yyyy-MM-dd}-";

        int i = 1;
        while (File.Exists(logFileName + i + ".log.gz")) { i++; }  // Get a unique number for the name

        logFileName += i + ".log";
            
        _logFile = File.OpenWrite("Logs/latest.log");
        _streamWriter = new StreamWriter(_logFile);
        _streamWriter.AutoFlush = true;
        _typeText = "";
        Info($"Logging to: {logFileName}");
    }
        
    public static void Error(object log) => Log(log, LogLevel.Error);
    public static void Info(object log) => Log(log, LogLevel.Info);
    public static void Debug(object log) => Log(log, LogLevel.Debug);
}

public enum LogLevel { Error, Info, Debug }