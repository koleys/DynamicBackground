using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using DynamicBackground.Services.Abstractions;

namespace DynamicBackground.Services
{
    /// <summary>
    /// HTTP image downloader service.
    /// </summary>
    public class HttpImageDownloader : IImageDownloader
    {
        private readonly ILogger _logger;

        public HttpImageDownloader(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<Stream> DownloadImageStreamAsync(
            string imageUrl, CancellationToken cancellationToken = default)
        {
            try
            {
                using (var webClient = new WebClient())
                {
                    var data = await Task.Run(() => webClient.DownloadData(imageUrl), cancellationToken);
                    return new MemoryStream(data);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to download image stream from {imageUrl}", ex);
                throw;
            }
        }

        public async Task<string> DownloadAndSaveImageAsync(
            string imageUrl, string savePath, CancellationToken cancellationToken = default)
        {
            try
            {
                var directory = Path.GetDirectoryName(savePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                using (var webClient = new WebClient())
                {
                    await Task.Run(() => webClient.DownloadFile(imageUrl, savePath), cancellationToken);
                }

                return savePath;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to download and save image from {imageUrl} to {savePath}", ex);
                throw;
            }
        }
    }
}
