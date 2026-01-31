using System;
using System.Diagnostics;
using System.IO;
using DynamicBackground.Services.Abstractions;

namespace DynamicBackground.Services.Logging
{
    /// <summary>
    /// Logger that tries Windows Event Log first, falls back to file logging.
    /// </summary>
    public class DualModeLogger : ILogger
    {
        private readonly ILogger _primaryLogger;
        private readonly ILogger _fallbackLogger;
        private bool _usesFallback = false;

        public DualModeLogger(string logFilePath)
        {
            _primaryLogger = new WindowsEventLogger();
            _fallbackLogger = new FileLogger(logFilePath);
        }

        public void LogError(string message, Exception? exception = null)
        {
            try
            {
                if (!_usesFallback)
                {
                    try
                    {
                        _primaryLogger.LogError(message, exception);
                        return;
                    }
                    catch
                    {
                        _usesFallback = true;
                    }
                }

                _fallbackLogger.LogError(message, exception);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Logging failed: {ex}");
            }
        }

        public void LogWarning(string message)
        {
            try
            {
                if (!_usesFallback)
                {
                    try
                    {
                        _primaryLogger.LogWarning(message);
                        return;
                    }
                    catch
                    {
                        _usesFallback = true;
                    }
                }

                _fallbackLogger.LogWarning(message);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Logging failed: {ex}");
            }
        }

        public void LogInfo(string message)
        {
            try
            {
                if (!_usesFallback)
                {
                    try
                    {
                        _primaryLogger.LogInfo(message);
                        return;
                    }
                    catch
                    {
                        _usesFallback = true;
                    }
                }

                _fallbackLogger.LogInfo(message);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Logging failed: {ex}");
            }
        }
    }

    /// <summary>
    /// Windows Event Log logger implementation.
    /// </summary>
    public class WindowsEventLogger : ILogger
    {
        private const string SOURCE = "DynamicBackground";

        public WindowsEventLogger()
        {
            try
            {
                if (!EventLog.SourceExists(SOURCE))
                    EventLog.CreateEventSource(SOURCE, "Application");
            }
            catch
            {
                // Ignore if no admin rights
            }
        }

        public void LogError(string message, Exception? exception = null)
        {
            try
            {
                var fullMessage = exception != null
                    ? $"{message}\n{exception}"
                    : message;

                EventLog.WriteEntry(SOURCE, fullMessage, EventLogEntryType.Error);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"EventLog write failed: {ex}");
                throw;
            }
        }

        public void LogWarning(string message)
        {
            try
            {
                EventLog.WriteEntry(SOURCE, message, EventLogEntryType.Warning);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"EventLog write failed: {ex}");
                throw;
            }
        }

        public void LogInfo(string message)
        {
            try
            {
                EventLog.WriteEntry(SOURCE, message, EventLogEntryType.Information);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"EventLog write failed: {ex}");
                throw;
            }
        }
    }

    /// <summary>
    /// File-based logger implementation (fallback).
    /// </summary>
    public class FileLogger : ILogger
    {
        private readonly string _logFilePath;

        public FileLogger(string logFilePath)
        {
            _logFilePath = logFilePath;
            EnsureDirectory();
        }

        public void LogError(string message, Exception? exception = null)
        {
            var fullMessage = exception != null
                ? $"[ERROR] {DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}\n{exception}\n"
                : $"[ERROR] {DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}\n";

            WriteToFile(fullMessage);
        }

        public void LogWarning(string message)
        {
            var fullMessage = $"[WARN] {DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}\n";
            WriteToFile(fullMessage);
        }

        public void LogInfo(string message)
        {
            var fullMessage = $"[INFO] {DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}\n";
            WriteToFile(fullMessage);
        }

        private void WriteToFile(string message)
        {
            try
            {
                File.AppendAllText(_logFilePath, message);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"File log write failed: {ex}");
                throw;
            }
        }

        private void EnsureDirectory()
        {
            try
            {
                var directory = Path.GetDirectoryName(_logFilePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    Directory.CreateDirectory(directory);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating log directory: {ex}");
            }
        }
    }
}
