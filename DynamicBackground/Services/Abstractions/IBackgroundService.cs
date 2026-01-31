using System.Threading;
using System.Threading.Tasks;

namespace DynamicBackground.Services.Abstractions
{
    /// <summary>
    /// Service for downloading and managing Bing background images.
    /// </summary>
    public interface IBackgroundService
    {
        /// <summary>
        /// Gets the downloaded Bing image path.
        /// </summary>
        Task<string> GetDownloadedImagePathAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the background image title/copyright.
        /// </summary>
        Task<string> GetBackgroundTitleAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the resolution-specific extension for the image URL.
        /// </summary>
        string GetResolutionExtension();
    }
}
