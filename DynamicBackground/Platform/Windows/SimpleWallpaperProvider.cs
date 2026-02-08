using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DynamicBackground.Services.Abstractions;

namespace DynamicBackground.Platform.Windows
{
    /// <summary>
    /// Simple wallpaper provider for testing that doesn't use any platform-specific code.
    /// </summary>
    public class SimpleWallpaperProvider : IWallpaperProvider
    {
        /// <summary>
        /// Gets the name of this platform.
        /// </summary>
        public string PlatformName => "Windows";

        /// <summary>
        /// Sets the wallpaper with the specified style on Windows.
        /// </summary>
        public async Task<bool> SetWallpaperAsync(string imagePath, WallpaperStyle style, CancellationToken cancellationToken = default)
        {
            try
            {
                // Simulate wallpaper setting without actual implementation
                await Task.Delay(1000, cancellationToken); // Simulate work
                return true;
            }
            catch (OperationCanceledException)
            {
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the current wallpaper path (simulated).
        /// </summary>
        public async Task<string> GetWallpaperAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await Task.Delay(100, cancellationToken); // Simulate work
                return "C:\\Users\\Public\\Pictures\\Sample Pictures\\Desert.jpg";
            }
            catch (OperationCanceledException)
            {
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the wallpaper styles supported on Windows.
        /// </summary>
        public async Task<IList<WallpaperStyle>> GetSupportedStylesAsync()
        {
            try
            {
                await Task.Delay(100); // Simulate work
                return new List<WallpaperStyle>
                {
                    WallpaperStyle.Fill,
                    WallpaperStyle.Fit,
                    WallpaperStyle.Stretch,
                    WallpaperStyle.Tile,
                    WallpaperStyle.Center,
                    WallpaperStyle.Span,
                };
            }
            catch (Exception ex)
            {
                return new List<WallpaperStyle>();
            }
        }
    }
}