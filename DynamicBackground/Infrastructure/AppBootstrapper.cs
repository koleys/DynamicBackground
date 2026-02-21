using Microsoft.Extensions.DependencyInjection;
using DynamicBackground.Services;
using DynamicBackground.Services.Abstractions;
using DynamicBackground.Services.Logging;
using DynamicBackground.Services.Platform;
using DynamicBackground.Platform.Windows;
using DynamicBackground.ViewModels;

namespace DynamicBackground.Infrastructure
{
    /// <summary>
    /// Bootstraps dependency injection container for the application.
    /// </summary>
    public static class AppBootstrapper
    {
        public static IServiceProvider CreateServiceProvider()
        {
            var services = new ServiceCollection();

            // Get settings file path using AppConstants
            var settingsFilePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                AppConstants.APP_FOLDER_NAME,
                AppConstants.SETTINGS_FILE_NAME);

            // Register log file manager
            services.AddSingleton<ILogFileManager, LogFileManager>();

            // Register logging service with configurable log file location
            var logFileManager = new LogFileManager(new SettingsService(
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    AppConstants.APP_FOLDER_NAME,
                    AppConstants.SETTINGS_FILE_NAME)));
            var logFilePath = logFileManager.GetLogFilePath();
            services.AddSingleton<ILogger>(new DualModeLogger(logFilePath));

            // Register core services
            services.AddSingleton<ISettingsService>(new SettingsService(settingsFilePath));
            services.AddSingleton<IImageDownloader, HttpImageDownloader>();
            services.AddSingleton<IBackgroundService, BackgroundService>();
            services.AddSingleton<IStartupDelayManager, StartupDelayManager>();

            // Register platform-specific services (Windows only for testing)
            services.AddSingleton<IWallpaperProvider>(sp => new SimpleWallpaperProvider());
            services.AddSingleton<IWallpaperService, WindowsWallpaperService>();

            // Register ViewModel
            services.AddSingleton<MainWindowViewModel>();

            // Register AppController
            services.AddSingleton<AppController>();

            return services.BuildServiceProvider();
        }
    }
}
