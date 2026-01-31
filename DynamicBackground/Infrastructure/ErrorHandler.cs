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
        /// Handles and logs an error, optionally showing a user message.
        /// </summary>
        public static void HandleError(
            string message,
            Exception? exception,
            ILogger logger,
            bool showUserMessage = true,
            string windowTitle = "Error")
        {
            if (exception != null)
            {
                logger.LogError(message, exception);
            }
            else
            {
                logger.LogError(message);
            }

            if (showUserMessage)
            {
                MessageBox.Show(
                    message,
                    windowTitle,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Handles a warning with optional user notification.
        /// </summary>
        public static void HandleWarning(
            string message,
            ILogger logger,
            bool showUserMessage = true,
            string windowTitle = "Warning")
        {
            logger.LogWarning(message);

            if (showUserMessage)
            {
                MessageBox.Show(
                    message,
                    windowTitle,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// Handles validation failure with user notification.
        /// </summary>
        public static bool ValidateInput(
            string input,
            string errorMessage,
            ILogger logger)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                HandleWarning(errorMessage, logger, true, "Validation");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Safely executes an operation with error handling.
        /// </summary>
        public static bool SafeExecute(
            Action operation,
            string operationName,
            ILogger logger,
            bool showUserMessage = true)
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
                    logger,
                    showUserMessage);
                return false;
            }
        }

        /// <summary>
        /// Safely executes an operation with error handling and result.
        /// </summary>
        public static T? SafeExecute<T>(
            Func<T> operation,
            string operationName,
            ILogger logger,
            T? defaultValue = default,
            bool showUserMessage = false)
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
                    logger,
                    showUserMessage);
                return defaultValue;
            }
        }
    }
}
