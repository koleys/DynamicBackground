using System.Threading.Tasks;
using DynamicBackground.Infrastructure;
using DynamicBackground.Services.Abstractions;

namespace DynamicBackground.Services
{
    /// <summary>
    /// Manages startup delay behavior for Bing wallpaper operations.
    /// Ensures delays are only applied during application startup, not manual actions.
    /// </summary>
    public class StartupDelayManager : IStartupDelayManager
    {
        private bool _startupComplete = false;
        private readonly ILogger _logger;

        public StartupDelayManager(ILogger logger)
        {
            _logger = logger;
        }

        public bool IsStartupComplete => _startupComplete;

        public void MarkStartupComplete()
        {
            _startupComplete = true;
            _logger.LogInfo("Startup delay phase completed. Manual operations will be immediate.");
        }

        public async Task ApplyStartupDelayAsync()
        {
            if (_startupComplete)
            {
                // Startup already complete, no delay needed
                return;
            }

            int startupDelaySeconds = AppConstants.DEFAULT_STARTUP_DELAY_SECONDS;

            _logger.LogInfo($"Applying startup delay of {startupDelaySeconds} seconds...");

            try
            {
                await Task.Delay(startupDelaySeconds * 1000);
                _logger.LogInfo("Startup delay completed.");
            }
            catch (System.Threading.Tasks.TaskCanceledException)
            {
                _logger.LogWarning("Startup delay was canceled.");
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to apply startup delay", ex);
            }
        }
    }
}