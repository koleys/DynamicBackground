using System;
using System.Runtime.InteropServices;
using DynamicBackground.Services.Abstractions;
using DynamicBackground.Platform.Windows;
using DynamicBackground.Platform.MacOS;
using DynamicBackground.Platform.Linux;

namespace DynamicBackground.Services.Platform
{
    /// <summary>
    /// Factory for creating platform-specific wallpaper providers.
    /// Detects the runtime operating system and returns the appropriate provider.
    /// </summary>
    public static class PlatformFactory
    {
        /// <summary>
        /// Creates a platform-specific wallpaper provider based on the runtime OS.
        /// </summary>
        /// <returns>An IWallpaperProvider implementation for the current platform.</returns>
        public static IWallpaperProvider CreateWallpaperProvider()
        {
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    Logger.LogInfo("Creating Windows wallpaper provider.");
                    return new WindowsWallpaperProvider();
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Logger.LogInfo("Creating macOS wallpaper provider.");
                    return new MacOSWallpaperProvider();
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Logger.LogInfo("Creating Linux wallpaper provider.");
                    return new LinuxWallpaperProvider();
                }
                else
                {
                    Logger.LogWarning($"Unknown platform detected: {RuntimeInformation.OSDescription}. Defaulting to Windows provider.");
                    return new WindowsWallpaperProvider();
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Failed to create wallpaper provider. Defaulting to Windows.", ex);
                return new WindowsWallpaperProvider();
            }
        }

        /// <summary>
        /// Gets the name of the current platform.
        /// </summary>
        /// <returns>Platform name: "Windows", "macOS", "Linux", or "Unknown".</returns>
        public static string GetCurrentPlatformName()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return "Windows";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                return "macOS";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return "Linux";

            return "Unknown";
        }

        /// <summary>
        /// Checks if the current platform is supported.
        /// </summary>
        /// <returns>True if the platform is Windows, macOS, or Linux; false otherwise.</returns>
        public static bool IsPlatformSupported()
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ||
                   RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ||
                   RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
        }
    }
}
