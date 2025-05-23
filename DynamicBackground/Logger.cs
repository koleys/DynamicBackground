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

        public static void LogError(string message, Exception ex)
        {
            string errorMessage = $"{DateTime.Now:u} | {message}\nException: {ex}\n";
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
    }
}
