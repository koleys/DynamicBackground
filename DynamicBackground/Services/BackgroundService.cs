using Newtonsoft.Json;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using DynamicBackground.Services.Abstractions;

namespace DynamicBackground.Services
{
    /// <summary>
    /// Service for Bing background image operations.
    /// </summary>
    public class BackgroundService : IBackgroundService
    {
        private readonly ISettingsService _settingsService;
        private readonly IImageDownloader _imageDownloader;
        private readonly ILogger _logger;
        private dynamic? _jsonCache;
        private DateTime _jsonCacheTime = DateTime.MinValue;
        private const int CACHE_HOURS = 24;

        public BackgroundService(
            ISettingsService settingsService,
            IImageDownloader imageDownloader,
            ILogger logger)
        {
            _settingsService = settingsService;
            _imageDownloader = imageDownloader;
            _logger = logger;
        }

        public async Task<string> GetDownloadedImagePathAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var urlBase = await GetBackgroundUrlBaseAsync(cancellationToken);
                var extension = GetResolutionExtension();
                var fullUrl = urlBase + extension;

                var backgroundImage = await DownloadBackgroundAsync(fullUrl, cancellationToken);
                return await SaveBackgroundAsync(backgroundImage, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to get Bing downloaded image path", ex);
                throw;
            }
        }

        public async Task<string> GetBackgroundTitleAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var json = await DownloadJsonAsync(cancellationToken);
                var copyrightText = json.images[0].copyright;
                int idx = copyrightText.IndexOf(" (");
                return idx > 0 ? copyrightText.Substring(0, idx) : copyrightText;
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to get background title", ex);
                return DateTime.Now.ToString("M-d-yyyy");
            }
        }

        public string GetResolutionExtension()
        {
            try
            {
                var resolution = Screen.PrimaryScreen.Bounds;
                string widthByHeight = $"{resolution.Width}x{resolution.Height}";
                string potentialExtension = $"_{widthByHeight}.jpg";
                return potentialExtension;
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to get resolution extension", ex);
                return "_1920x1080.jpg";
            }
        }

        private async Task<dynamic> DownloadJsonAsync(CancellationToken cancellationToken = default)
        {
            // Use cache if valid
            if (DateTime.Now - _jsonCacheTime < TimeSpan.FromHours(CACHE_HOURS) && _jsonCache != null)
                return _jsonCache;

            try
            {
                using (var webClient = new WebClient())
                {
                    string jsonString = webClient.DownloadString(
                        "https://www.bing.com/HPImageArchive.aspx?format=js&idx=0&n=1&mkt=en-US");
                    _jsonCache = JsonConvert.DeserializeObject<dynamic>(jsonString);
                    _jsonCacheTime = DateTime.Now;
                    return _jsonCache;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to download Bing JSON", ex);
                throw;
            }
        }

        private async Task<string> GetBackgroundUrlBaseAsync(CancellationToken cancellationToken = default)
        {
            var jsonObject = await DownloadJsonAsync(cancellationToken);
            return "https://www.bing.com" + jsonObject.images[0].urlbase;
        }

        private async Task<System.Drawing.Image> DownloadBackgroundAsync(
            string url, CancellationToken cancellationToken = default)
        {
            try
            {
                var stream = await _imageDownloader.DownloadImageStreamAsync(url, cancellationToken);
                return System.Drawing.Image.FromStream(stream);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to download background from {url}", ex);
                throw;
            }
        }

        private async Task<string> SaveBackgroundAsync(
            System.Drawing.Image backgroundImage, CancellationToken cancellationToken = default)
        {
            try
            {
                string imagePath = GetBackgroundImagePath();
                backgroundImage.Save(imagePath, System.Drawing.Imaging.ImageFormat.Bmp);
                return imagePath;
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to save Bing background image", ex);
                throw;
            }
        }

        private string GetBackgroundImagePath()
        {
            try
            {
                string directoryPath = _settingsService.GetSetting("ImgSaveLoc") ??
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                        "Bing Backgrounds", DateTime.Now.Year.ToString());

                if (!Directory.Exists(directoryPath))
                    Directory.CreateDirectory(directoryPath);

                string fileName = GetBackgroundTitleSync();
                if (string.IsNullOrEmpty(fileName))
                    fileName = DateTime.Now.ToString("M-d-yyyy");
                else
                    fileName = Regex.Replace(fileName, @"[^0-9a-zA-Z]+", "_");

                fileName += ".jpg";
                return Path.Combine(directoryPath, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to get background image path", ex);
                throw;
            }
        }

        private string GetBackgroundTitleSync()
        {
            try
            {
                // Using sync version for internal use
                using (var webClient = new WebClient())
                {
                    string jsonString = webClient.DownloadString(
                        "https://www.bing.com/HPImageArchive.aspx?format=js&idx=0&n=1&mkt=en-US");
                    var json = JsonConvert.DeserializeObject<dynamic>(jsonString);
                    var copyrightText = json.images[0].copyright;
                    int idx = copyrightText.IndexOf(" (");
                    return idx > 0 ? copyrightText.Substring(0, idx) : copyrightText;
                }
            }
            catch
            {
                return DateTime.Now.ToString("M-d-yyyy");
            }
        }
    }
}
