namespace DynamicBackground.Services.Abstractions
{
    /// <summary>
    /// Logging service abstraction.
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Logs an error message with optional exception.
        /// </summary>
        void LogError(string message, Exception? exception = null);

        /// <summary>
        /// Logs a warning message.
        /// </summary>
        void LogWarning(string message);

        /// <summary>
        /// Logs an info message.
        /// </summary>
        void LogInfo(string message);
    }
}
