using System;
using System.Diagnostics;
using System.IO;

namespace DynamicBackground
{
    public static class Logger
    {
        private const string Source = "DynamicBackgroundApp";
        private const string LogName = "Application";
        private static readonly string LogFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DynamicBackground.log");

        public static void LogError(string message, Exception ex = null)
        {
            string errorMessage = ex != null
                ? $"{DateTime.Now:u} | [ERROR] {message}\nException: {ex}\n"
                : $"{DateTime.Now:u} | [ERROR] {message}\n";
            
            try
            {
                if (!EventLog.SourceExists(Source))
                {
                    EventLog.CreateEventSource(Source, LogName);
                }
                EventLog.WriteEntry(Source, errorMessage, EventLogEntryType.Error);
            }
            catch
            {
                // Fallback: log to file if Event Viewer logging fails
                try
                {
                    File.AppendAllText(LogFilePath, errorMessage);
                }
                catch
                {
                    // If file logging also fails, do nothing
                }
            }
        }

        public static void LogWarning(string message)
        {
            string warningMessage = $"{DateTime.Now:u} | [WARN] {message}\n";
            try
            {
                if (!EventLog.SourceExists(Source))
                {
                    EventLog.CreateEventSource(Source, LogName);
                }
                EventLog.WriteEntry(Source, warningMessage, EventLogEntryType.Warning);
            }
            catch
            {
                // Fallback: log to file if Event Viewer logging fails
                try
                {
                    File.AppendAllText(LogFilePath, warningMessage);
                }
                catch
                {
                    // If file logging also fails, do nothing
                }
            }
        }

        public static void LogInfo(string message)
        {
            string infoMessage = $"{DateTime.Now:u} | [INFO] {message}\n";
            try
            {
                if (!EventLog.SourceExists(Source))
                {
                    EventLog.CreateEventSource(Source, LogName);
                }
                EventLog.WriteEntry(Source, infoMessage, EventLogEntryType.Information);
            }
            catch
            {
                // Fallback: log to file if Event Viewer logging fails
                try
                {
                    File.AppendAllText(LogFilePath, infoMessage);
                }
                catch
                {
                    // If file logging also fails, do nothing
                }
            }
        }
    }
}
