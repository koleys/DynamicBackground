using System;
using System.Threading.Tasks;
using DynamicBackground.Services.Abstractions;

namespace DynamicBackground.Platform.Windows
{
    /// <summary>
    /// Windows-specific wallpaper service that delegates to WindowsWallpaperProvider.
    /// Maintains backward compatibility with IWallpaperService interface.
    /// </summary>
    public class WindowsWallpaperService : IWallpaperService
    {
        private readonly IWallpaperProvider _provider;

        /// <summary>
        /// Initializes a new instance of the WindowsWallpaperService.
        /// </summary>
        public WindowsWallpaperService()
        {
            _provider = new WindowsWallpaperProvider();
        }

        /// <summary>
        /// Sets the wallpaper with the specified style.
        /// Synchronous wrapper around the async provider.
        /// </summary>
        public void Set(string filePath, WallpaperStyle style)
        {
            try
            {
                var task = _provider.SetWallpaperAsync(filePath, style);
                task.GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error setting wallpaper: {filePath}", ex);
                throw;
            }
        }

        /// <summary>
        /// Sets the wallpaper silently without updating history.
        /// Synchronous wrapper around the async provider.
        /// </summary>
        public void SilentSet(string filePath, WallpaperStyle style)
        {
            try
            {
                var task = _provider.SetWallpaperAsync(filePath, style);
                task.GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error silently setting wallpaper: {filePath}", ex);
                throw;
            }
        }

        /// <summary>
        /// Sets the wallpaper silently without updating history.
        /// Asynchronous wrapper around the async provider.
        /// </summary>
        public async Task SilentSetAsync(string filePath, WallpaperStyle style, CancellationToken cancellationToken = default)
        {
            try
            {
                await _provider.SetWallpaperAsync(filePath, style, cancellationToken);
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error silently setting wallpaper: {filePath}", ex);
                throw;
            }
        }

        /// <summary>
        /// Restores the wallpaper to previous state (not implemented in provider).
        /// </summary>
        public void RestoreState()
        {
            Logger.LogWarning("RestoreState is deprecated. Use Wallpaper.RestoreState() for backward compatibility.");
        }

        /// <summary>
        /// Backs up current wallpaper state (not implemented in provider).
        /// </summary>
        public void BackupState()
        {
            Logger.LogWarning("BackupState is deprecated. Use Wallpaper.BackupState() for backward compatibility.");
        }
    }
}
