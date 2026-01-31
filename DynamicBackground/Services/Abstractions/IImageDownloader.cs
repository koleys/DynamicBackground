using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DynamicBackground.Services.Abstractions
{
    /// <summary>
    /// Service for downloading images from URLs.
    /// </summary>
    public interface IImageDownloader
    {
        /// <summary>
        /// Downloads image stream from URL.
        /// </summary>
        Task<Stream> DownloadImageStreamAsync(string imageUrl, CancellationToken cancellationToken = default);

        /// <summary>
        /// Downloads and saves image from URL.
        /// </summary>
        Task<string> DownloadAndSaveImageAsync(string imageUrl, string savePath, CancellationToken cancellationToken = default);
    }
}
