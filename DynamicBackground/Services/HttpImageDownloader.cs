using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Polly;
using DynamicBackground.Services.Abstractions;

namespace DynamicBackground.Services
{
    /// <summary>
    /// HTTP image downloader service with resilience policies (retry with exponential backoff).
    /// </summary>
    public class HttpImageDownloader : IImageDownloader
    {
        private readonly ILogger _logger;
        private readonly IAsyncPolicy<Stream?> _downloadStreamPolicy;
        private readonly IAsyncPolicy _downloadFilePolicy;
        private const int MAX_RETRIES = 3;
        private const int TIMEOUT_SECONDS = 30;

        public HttpImageDownloader(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _downloadStreamPolicy = CreateStreamDownloadPolicy();
            _downloadFilePolicy = CreateFileDownloadPolicy();
        }

        public async Task<Stream> DownloadImageStreamAsync(
            string imageUrl, CancellationToken cancellationToken = default)
        {
            try
            {
                var stream = await _downloadStreamPolicy.ExecuteAsync(
                    async () => await DownloadStreamInternalAsync(imageUrl, cancellationToken));
                
                if (stream == null)
                    throw new Exception("Failed to download stream after retries");
                
                return stream;
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

                await _downloadFilePolicy.ExecuteAsync(
                    async () => await DownloadFileInternalAsync(imageUrl, savePath, cancellationToken));

                return savePath;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to download and save image from {imageUrl} to {savePath}", ex);
                throw;
            }
        }

        private async Task<Stream?> DownloadStreamInternalAsync(
            string imageUrl, CancellationToken cancellationToken)
        {
            using (var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
            {
                cts.CancelAfter(TimeSpan.FromSeconds(TIMEOUT_SECONDS));
                using (var webClient = new WebClient())
                {
                    var data = await Task.Run(() => webClient.DownloadData(imageUrl), cts.Token);
                    return new MemoryStream(data);
                }
            }
        }

        private async Task DownloadFileInternalAsync(
            string imageUrl, string savePath, CancellationToken cancellationToken)
        {
            using (var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
            {
                cts.CancelAfter(TimeSpan.FromSeconds(TIMEOUT_SECONDS));
                using (var webClient = new WebClient())
                {
                    await Task.Run(() => webClient.DownloadFile(imageUrl, savePath), cts.Token);
                }
            }
        }

        private IAsyncPolicy<Stream?> CreateStreamDownloadPolicy()
        {
            return Policy
                .Handle<Exception>()
                .OrResult<Stream?>(r => r == null)
                .WaitAndRetryAsync(
                    retryCount: MAX_RETRIES,
                    sleepDurationProvider: retryAttempt =>
                        TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (outcome, timespan, retryCount, context) =>
                    {
                        _logger.LogWarning(
                            $"Retry {retryCount} after {timespan.TotalSeconds}s for stream download");
                    });
        }

        private IAsyncPolicy CreateFileDownloadPolicy()
        {
            return Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(
                    retryCount: MAX_RETRIES,
                    sleepDurationProvider: retryAttempt =>
                        TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (outcome, timespan, retryCount, context) =>
                    {
                        _logger.LogWarning(
                            $"Retry {retryCount} after {timespan.TotalSeconds}s for file download");
                    });
        }
    }
}
