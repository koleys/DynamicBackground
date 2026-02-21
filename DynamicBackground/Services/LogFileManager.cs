using System;
using System.IO;
using DynamicBackground.Services.Abstractions;
using DynamicBackground.Infrastructure;

namespace DynamicBackground.Services
{
    /// <summary>
    /// Service for managing log file paths with configurable location.
    /// Defaults to application running directory but can be configured through settings.
    /// </summary>
    public class LogFileManager : ILogFileManager
    {
        private readonly ISettingsService _settingsService;
        private readonly string _defaultLogFilePath;
        private readonly string _logFileName;

        public LogFileManager(ISettingsService settingsService, string logFileName = AppConstants.LOG_FILE_NAME)
        {
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            _logFileName = logFileName;
            _defaultLogFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, logFileName);
        }

        /// <summary>
        /// Gets the current log file path based on settings or default.
        /// </summary>
        public string GetLogFilePath()
        {
            try
            {
                var configuredPath = _settingsService.GetSetting(AppConstants.SETTINGS_KEY_LOG_FILE_LOCATION);

                if (!string.IsNullOrEmpty(configuredPath))
                {
                    // If path is a directory, append the log file name
                    if (Directory.Exists(configuredPath))
                    {
                        return Path.Combine(configuredPath, _logFileName);
                    }

                    // If path is a full file path, use it directly
                    if (Path.GetFileName(configuredPath) == _logFileName)
                    {
                        return configuredPath;
                    }

                    // If path is a directory without trailing separator, append log file name
                    if (Path.GetDirectoryName(configuredPath) == configuredPath)
                    {
                        return Path.Combine(configuredPath, _logFileName);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting log file location from settings: {ex}");
            }

            return _defaultLogFilePath;
        }

        /// <summary>
        /// Sets the log file location. Can be a directory or full file path.
        /// </summary>
        public void SetLogFilePath(string path)
        {
            try
            {
                // Validate the path
                if (string.IsNullOrEmpty(path))
                {
                    throw new ArgumentException("Log file path cannot be empty", nameof(path));
                }

                // If path is a directory, ensure it exists
                if (Directory.Exists(path))
                {
                    _settingsService.SetSetting(AppConstants.SETTINGS_KEY_LOG_FILE_LOCATION, path);
                    return;
                }

                // If path is a file, validate it's in a valid directory
                var directory = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(directory) && Directory.Exists(directory))
                {
                    _settingsService.SetSetting(AppConstants.SETTINGS_KEY_LOG_FILE_LOCATION, path);
                    return;
                }

                throw new DirectoryNotFoundException($"Log file directory does not exist: {directory}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting log file location: {ex}");
                throw;
            }
        }

        /// <summary>
        /// Gets the current log file directory.
        /// </summary>
        public string GetLogDirectory()
        {
            return Path.GetDirectoryName(GetLogFilePath()) ?? _defaultLogFilePath;
        }

        /// <summary>
        /// Ensures the log file directory exists.
        /// </summary>
        public void EnsureLogDirectory()
        {
            try
            {
                var directory = GetLogDirectory();
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating log directory: {ex}");
            }
        }
    }
}