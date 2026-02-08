using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DynamicBackground.Services.Abstractions
{
    /// <summary>
    /// Platform-specific wallpaper provider abstraction.
    /// Defines contract for setting wallpapers across different operating systems.
    /// </summary>
    public interface IWallpaperProvider
    {
        /// <summary>
        /// Sets wallpaper on the current platform.
        /// </summary>
        /// <param name="imagePath">Full path to the wallpaper image file.</param>
        /// <param name="style">The wallpaper style/fit mode.</param>
        /// <param name="cancellationToken">Cancellation token for async operations.</param>
        /// <returns>True if wallpaper was set successfully; false otherwise.</returns>
        Task<bool> SetWallpaperAsync(string imagePath, WallpaperStyle style, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the current wallpaper path on this platform.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for async operations.</param>
        /// <returns>The path to the current wallpaper image, or null if unable to retrieve.</returns>
        Task<string> GetWallpaperAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the list of wallpaper styles supported by this platform.
        /// </summary>
        /// <returns>Collection of supported wallpaper styles.</returns>
        Task<IList<WallpaperStyle>> GetSupportedStylesAsync();

        /// <summary>
        /// Gets the name of the platform this provider targets.
        /// </summary>
        string PlatformName { get; }
    }
}
