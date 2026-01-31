using Microsoft.Extensions.DependencyInjection;
using DynamicBackground.Services;
using DynamicBackground.Services.Abstractions;
using DynamicBackground.Services.Logging;
using DynamicBackground.Platform.Windows;

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

            // Get settings file path
            var settingsFilePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "DynamicBackground",
                "DynamicBackground.settings.json");

            // Register logging service
            var logFilePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "DynamicBackground",
                "logs.txt");
            services.AddSingleton<ILogger>(new DualModeLogger(logFilePath));

            // Register core services
            services.AddSingleton<ISettingsService>(new SettingsService(settingsFilePath));
            services.AddSingleton<IImageDownloader, HttpImageDownloader>();
            services.AddSingleton<IBackgroundService, BackgroundService>();

            // Register platform-specific services
            services.AddSingleton<IWallpaperService, WindowsWallpaperService>();

            return services.BuildServiceProvider();
        }
    }
}
