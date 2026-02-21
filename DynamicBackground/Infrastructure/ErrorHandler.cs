using System;
using System.Windows.Forms;
using DynamicBackground.Services.Abstractions;

namespace DynamicBackground.Infrastructure
{
    /// <summary>
    /// Utility class for centralized error handling and user notifications.
    /// </summary>
    public static class ErrorHandler
    {
        /// <summary>
        /// Handles and logs an error silently.
        /// </summary>
        public static void HandleError(
            string message,
            Exception? exception,
            ILogger logger)
        {
            if (exception != null)
            {
                logger.LogError(message, exception);
            }
            else
            {
                logger.LogError(message);
            }
        }

        /// <summary>
        /// Handles a warning silently.
        /// </summary>
        public static void HandleWarning(
            string message,
            ILogger logger)
        {
            logger.LogWarning(message);
        }

        /// <summary>
        /// Handles validation failure silently.
        /// </summary>
        public static bool ValidateInput(
            string input,
            string errorMessage,
            ILogger logger)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                HandleWarning(errorMessage, logger);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Safely executes an operation with silent error handling.
        /// </summary>
        public static bool SafeExecute(
            Action operation,
            string operationName,
            ILogger logger)
        {
            try
            {
                operation();
                return true;
            }
            catch (Exception ex)
            {
                HandleError(
                    $"Failed to {operationName}: {ex.Message}",
                    ex,
                    logger);
                return false;
            }
        }

        /// <summary>
        /// Safely executes an operation with silent error handling and result.
        /// </summary>
        public static T? SafeExecute<T>(
            Func<T> operation,
            string operationName,
            ILogger logger,
            T? defaultValue = default)
            where T : class
        {
            try
            {
                return operation();
            }
            catch (Exception ex)
            {
                HandleError(
                    $"Failed to {operationName}: {ex.Message}",
                    ex,
                    logger);
                return defaultValue;
            }
        }
    }
}
