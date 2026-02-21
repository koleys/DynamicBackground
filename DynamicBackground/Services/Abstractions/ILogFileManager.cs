using System;

namespace DynamicBackground.Services.Abstractions
{
    /// <summary>
    /// Interface for managing log file paths with configurable location.
    /// </summary>
    public interface ILogFileManager
    {
        /// <summary>
        /// Gets the current log file path based on settings or default.
        /// </summary>
        string GetLogFilePath();

        /// <summary>
        /// Sets the log file location. Can be a directory or full file path.
        /// </summary>
        void SetLogFilePath(string path);

        /// <summary>
        /// Gets the current log file directory.
        /// </summary>
        string GetLogDirectory();

        /// <summary>
        /// Ensures the log file directory exists.
        /// </summary>
        void EnsureLogDirectory();
    }
}