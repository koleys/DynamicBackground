namespace DynamicBackground.Infrastructure
{
    /// <summary>
    /// Application-wide constants for configuration values, URLs, and magic strings.
    /// </summary>
    public static class AppConstants
    {
        // Settings Keys
        public const string SETTINGS_KEY_IMAGE_SAVE_LOCATION = "ImgSaveLoc";
        public const string SETTINGS_KEY_UPDATE_INTERVAL = "Interval";
        public const string SETTINGS_KEY_STARTUP_DELAY = "StartupDelay";

        // Default Settings Values
        public const int DEFAULT_UPDATE_INTERVAL_MINUTES = 720; // 12 hours
        public const int DEFAULT_STARTUP_DELAY_SECONDS = 300; // 5 minutes

        // Validation Ranges
        public const int MIN_STARTUP_DELAY_SECONDS = 0;
        public const int MAX_STARTUP_DELAY_SECONDS = 300; // 5 minutes

        // Bing Background URLs
        public const string BING_IMAGE_ARCHIVE_API_URL = "https://www.bing.com/HPImageArchive.aspx";
        public const string BING_BASE_IMAGE_URL = "https://www.bing.com";
        public const string BING_DEFAULT_RESOLUTION_EXTENSION = "_1920x1080.jpg";

        // Image Download Configuration
        public const int DOWNLOAD_TIMEOUT_SECONDS = 30;
        public const int MAX_DOWNLOAD_RETRIES = 3;
        public const int DOWNLOAD_RETRY_BACKOFF_SECONDS = 2;

        // File System Paths (relative to AppData)
        public const string APP_FOLDER_NAME = "DynamicBackground";
        public const string SETTINGS_FILE_NAME = "DynamicBackground.settings.json";
        public const string LOG_FILE_NAME = "logs.txt";

        // Default Image Save Location (relative to Pictures)
        public const string DEFAULT_BING_IMAGES_FOLDER = "Bing Backgrounds";

        // UI Constants
        public const string APP_TITLE = "Dynamic Background";
        public const string TRAY_ICON_FILE = "TrayIcon.ico";
    }
}
