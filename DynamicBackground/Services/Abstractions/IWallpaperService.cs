using System.Threading;
using System.Threading.Tasks;

namespace DynamicBackground.Services.Abstractions
{
    /// <summary>
    /// Service for managing wallpaper settings.
    /// </summary>
    public interface IWallpaperService
    {
        /// <summary>
        /// Sets the wallpaper with specified style.
        /// </summary>
        void Set(string filePath, WallpaperStyle style);

        /// <summary>
        /// Sets the wallpaper silently without updating history.
        /// </summary>
        void SilentSet(string filePath, WallpaperStyle style);

        /// <summary>
        /// Sets the wallpaper silently without updating history.
        /// </summary>
        Task SilentSetAsync(string filePath, WallpaperStyle style, CancellationToken cancellationToken = default);

        /// <summary>
        /// Restores the wallpaper to previous state.
        /// </summary>
        void RestoreState();

        /// <summary>
        /// Backs up current wallpaper state.
        /// </summary>
        void BackupState();
    }
}
