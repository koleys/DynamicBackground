using System;
using System.Threading;
using System.Threading.Tasks;
using DynamicBackground.Services.Abstractions;

namespace DynamicBackground.Infrastructure
{
    /// <summary>
    /// Application controller that orchestrates services and business logic flow.
    /// Acts as a mediator between UI and services.
    /// </summary>
    public class AppController
    {
        private readonly IBackgroundService _backgroundService;
        private readonly IWallpaperService _wallpaperService;
        private readonly ISettingsService _settingsService;
        private readonly IImageDownloader _imageDownloader;
        private readonly ILogger _logger;

        public AppController(
            IBackgroundService backgroundService,
            IWallpaperService wallpaperService,
            ISettingsService settingsService,
            IImageDownloader imageDownloader,
            ILogger logger)
        {
            _backgroundService = backgroundService ?? throw new ArgumentNullException(nameof(backgroundService));
            _wallpaperService = wallpaperService ?? throw new ArgumentNullException(nameof(wallpaperService));
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            _imageDownloader = imageDownloader ?? throw new ArgumentNullException(nameof(imageDownloader));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Downloads the current Bing wallpaper and applies it.
        /// </summary>
        public async Task<bool> ApplyBingWallpaperAsync(WallpaperStyle style, CancellationToken cancellationToken = default)
        {
            try
            {
                var imagePath = await _backgroundService.GetDownloadedImagePathAsync(cancellationToken);
                if (!string.IsNullOrEmpty(imagePath))
                {
                    _wallpaperService.SilentSet(imagePath, style);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to apply Bing wallpaper", ex);
                return false;
            }
        }

        /// <summary>
        /// Applies a wallpaper from a given file path.
        /// </summary>
        public bool ApplyLocalWallpaper(string filePath, WallpaperStyle style)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(filePath))
                    return false;

                _wallpaperService.SilentSet(filePath, style);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to apply local wallpaper from {filePath}", ex);
                return false;
            }
        }

        /// <summary>
        /// Saves the background image download location.
        /// </summary>
        public bool SetBackgroundSaveLocation(string folderPath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(folderPath))
                    return false;

                _settingsService.SetSetting("ImgSaveLoc", folderPath);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to set background save location", ex);
                return false;
            }
        }

        /// <summary>
        /// Updates the auto-update interval setting.
        /// </summary>
        public bool SetAutoUpdateInterval(int minutes)
        {
            try
            {
                if (minutes <= 0)
                    return false;

                _settingsService.SetSetting("Interval", minutes.ToString());
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to set auto-update interval", ex);
                return false;
            }
        }

        /// <summary>
        /// Gets the current auto-update interval in minutes.
        /// </summary>
        public int GetAutoUpdateInterval()
        {
            try
            {
                return _settingsService.GetSettingAsInt("Interval", 720);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to get auto-update interval", ex);
                return 720;
            }
        }
    }
}
