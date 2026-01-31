using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DynamicBackground.Services.Abstractions;

namespace DynamicBackground.ViewModels
{
    /// <summary>
    /// ViewModel for the main application window, encapsulating business logic.
    /// Handles wallpaper operations, settings, and auto-update scheduling.
    /// </summary>
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private readonly IBackgroundService _backgroundService;
        private readonly IWallpaperService _wallpaperService;
        private readonly ISettingsService _settingsService;
        private readonly IImageDownloader _imageDownloader;
        private readonly ILogger _logger;

        private string _currentImagePath = string.Empty;
        private WallpaperStyle _currentStyle = WallpaperStyle.Fill;
        private bool _autoUpdateEnabled = true;
        private int _updateInterval = 720;
        private bool _isProcessing = false;
        private string _lastError = string.Empty;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string CurrentImagePath
        {
            get => _currentImagePath;
            set
            {
                if (_currentImagePath != value)
                {
                    _currentImagePath = value;
                    OnPropertyChanged(nameof(CurrentImagePath));
                }
            }
        }

        public WallpaperStyle CurrentStyle
        {
            get => _currentStyle;
            set
            {
                if (_currentStyle != value)
                {
                    _currentStyle = value;
                    OnPropertyChanged(nameof(CurrentStyle));
                }
            }
        }

        public bool AutoUpdateEnabled
        {
            get => _autoUpdateEnabled;
            set
            {
                if (_autoUpdateEnabled != value)
                {
                    _autoUpdateEnabled = value;
                    OnPropertyChanged(nameof(AutoUpdateEnabled));
                }
            }
        }

        public int UpdateInterval
        {
            get => _updateInterval;
            set
            {
                if (_updateInterval != value)
                {
                    _updateInterval = value;
                    OnPropertyChanged(nameof(UpdateInterval));
                }
            }
        }

        public bool IsProcessing
        {
            get => _isProcessing;
            set
            {
                if (_isProcessing != value)
                {
                    _isProcessing = value;
                    OnPropertyChanged(nameof(IsProcessing));
                }
            }
        }

        public string LastError
        {
            get => _lastError;
            private set
            {
                if (_lastError != value)
                {
                    _lastError = value;
                    OnPropertyChanged(nameof(LastError));
                }
            }
        }

        public MainWindowViewModel(
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

            LoadSettings();
        }

        /// <summary>
        /// Sets wallpaper from file path with error handling.
        /// </summary>
        public async Task<bool> SetWallpaperAsync(string filePath, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                LastError = "Please select a file";
                return false;
            }

            try
            {
                IsProcessing = true;
                LastError = string.Empty;

                // Check if URL or local file
                if (Uri.IsWellFormedUriString(filePath, UriKind.RelativeOrAbsolute))
                {
                    var savedPath = await _imageDownloader.DownloadAndSaveImageAsync(filePath, 
                        Path.Combine(Path.GetTempPath(), Path.GetFileName(filePath)), cancellationToken);
                    
                    if (!string.IsNullOrEmpty(savedPath))
                    {
                        _wallpaperService.SilentSet(savedPath, CurrentStyle);
                        CurrentImagePath = savedPath;
                        return true;
                    }
                    return false;
                }
                else
                {
                    // Local file
                    _wallpaperService.SilentSet(filePath, CurrentStyle);
                    CurrentImagePath = filePath;
                    return true;
                }
            }
            catch (Exception ex)
            {
                LastError = $"Error: {ex.Message}";
                _logger.LogError($"Failed to set wallpaper from {filePath}", ex);
                return false;
            }
            finally
            {
                IsProcessing = false;
            }
        }

        /// <summary>
        /// Downloads and sets the current Bing wallpaper.
        /// </summary>
        public async Task<bool> DownloadAndSetBingWallpaperAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                IsProcessing = true;
                LastError = string.Empty;

                var imagePath = await _backgroundService.GetDownloadedImagePathAsync(cancellationToken);
                if (!string.IsNullOrEmpty(imagePath))
                {
                    _wallpaperService.SilentSet(imagePath, CurrentStyle);
                    CurrentImagePath = imagePath;
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                LastError = $"Error downloading Bing wallpaper: {ex.Message}";
                _logger.LogError("Failed to download and set Bing wallpaper", ex);
                return false;
            }
            finally
            {
                IsProcessing = false;
            }
        }

        /// <summary>
        /// Handles file browse dialog and returns selected path.
        /// </summary>
        public string BrowseFile()
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = BuildImageFileFilter();
                openFileDialog.DefaultExt = ".png";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    return openFileDialog.FileName;
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// Handles folder browse dialog and returns selected path.
        /// </summary>
        public string BrowseFolder()
        {
            using (var folderBrowserDialog = new FolderBrowserDialog())
            {
                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                {
                    return folderBrowserDialog.SelectedPath;
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// Updates the Bing image save location.
        /// </summary>
        public bool SetImageSaveLocation(string folderPath)
        {
            if (string.IsNullOrEmpty(folderPath))
            {
                return false;
            }

            try
            {
                _settingsService.SetSetting("ImgSaveLoc", folderPath);
                LastError = string.Empty;
                return true;
            }
            catch (Exception ex)
            {
                LastError = "Failed to save location setting";
                _logger.LogError("Failed to set image save location", ex);
                return false;
            }
        }

        /// <summary>
        /// Updates the auto-update interval.
        /// </summary>
        public bool SetUpdateInterval(int minutes)
        {
            if (minutes <= 0)
            {
                LastError = "Interval must be greater than 0";
                return false;
            }

            try
            {
                UpdateInterval = minutes;
                _settingsService.SetSetting("Interval", minutes.ToString());
                LastError = string.Empty;
                return true;
            }
            catch (Exception ex)
            {
                LastError = "Failed to save interval setting";
                _logger.LogError("Failed to set update interval", ex);
                return false;
            }
        }

        /// <summary>
        /// Loads settings from persistent storage.
        /// </summary>
        private void LoadSettings()
        {
            try
            {
                var intervalStr = _settingsService.GetSetting("Interval");
                if (int.TryParse(intervalStr, out var interval) && interval > 0)
                {
                    UpdateInterval = interval;
                }

                AutoUpdateEnabled = true;
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to load settings", ex);
            }
        }

        /// <summary>
        /// Builds image file filter for OpenFileDialog.
        /// </summary>
        private string BuildImageFileFilter()
        {
            var filters = new System.Collections.Generic.List<string> { "All Files (*.*)|*.*" };
            
            var codecs = System.Drawing.Imaging.ImageCodecInfo.GetImageEncoders();
            foreach (var codec in codecs)
            {
                var codecName = codec.CodecName.Substring(8).Replace("Codec", "Files").Trim();
                filters.Add($"{codecName} ({codec.FilenameExtension})|{codec.FilenameExtension}");
            }

            return string.Join("|", filters);
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

