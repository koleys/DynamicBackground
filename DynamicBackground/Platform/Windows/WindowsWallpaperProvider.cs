using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32;
using DynamicBackground.Services.Abstractions;

namespace DynamicBackground.Platform.Windows
{
    /// <summary>
    /// Windows-specific wallpaper provider implementation.
    /// Handles wallpaper management on Windows platforms using Registry and P/Invoke.
    /// </summary>
    public class WindowsWallpaperProvider : IWallpaperProvider
    {
        private const uint SPI_SETDESKWALLPAPER = 20;
        private const uint SPIF_UPDATEINIFILE = 0x01;
        private const uint SPIF_SENDCHANGE = 0x02;
        private const string DesktopRegistryPath = @"Control Panel\Desktop";
        private const string WallpaperStyleRegistryKey = "WallpaperStyle";
        private const string TileWallpaperRegistryKey = "TileWallpaper";

        private string? _backupImagePath;
        private WallpaperStyle _backupStyle;

        /// <summary>
        /// Gets the name of this platform.
        /// </summary>
        public string PlatformName => "Windows";

        /// <summary>
        /// Sets the wallpaper with the specified style on Windows.
        /// </summary>
        public async Task<bool> SetWallpaperAsync(string imagePath, WallpaperStyle style, CancellationToken cancellationToken = default)
        {
            try
            {
                // Use a timeout for the operation
                using (var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
                {
                    timeoutCts.CancelAfter(30000); // 30 second timeout

                    return await Task.Run(() =>
                    {
                        try
                        {
                            timeoutCts.Token.ThrowIfCancellationRequested();

                            // Check if we have registry access permissions
                            if (!HasRegistryAccess())
                            {
                                Logger.LogWarning("Insufficient permissions to access Windows Registry.");
                                return false;
                            }

                            BackupState();
                            ApplyWallpaper(imagePath, style);
                            return true;
                        }
                        catch (OperationCanceledException)
                        {
                            Logger.LogWarning("Wallpaper operation cancelled or timed out.");
                            return false;
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError($"Failed to set wallpaper: {imagePath}", ex);
                            return false;
                        }
                    }, timeoutCts.Token);
                }
            }
            catch (OperationCanceledException)
            {
                Logger.LogWarning("Wallpaper operation cancelled.");
                return false;
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to set wallpaper: {imagePath}", ex);
                return false;
            }
        }

        /// <summary>
        /// Checks if the application has sufficient permissions to access the Windows Registry.
        /// </summary>
        private bool HasRegistryAccess()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(DesktopRegistryPath, false))
                {
                    return key != null;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the current wallpaper path from Windows Registry.
        /// </summary>
        public async Task<string> GetWallpaperAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await Task.Run(() =>
                {
                    try
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        using (var key = Registry.CurrentUser.OpenSubKey(DesktopRegistryPath, false))
                        {
                            return key?.GetValue("Wallpaper")?.ToString();
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        Logger.LogWarning("Get wallpaper operation cancelled.");
                        return null;
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError("Failed to get current wallpaper.", ex);
                        return null;
                    }
                }, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                Logger.LogWarning("Get wallpaper operation cancelled.");
                return null;
            }
            catch (Exception ex)
            {
                Logger.LogError("Failed to get current wallpaper.", ex);
                return null;
            }
        }

        /// <summary>
        /// Gets the wallpaper styles supported on Windows.
        /// Windows supports all 6 styles: Fill, Fit, Stretch, Tile, Center, and Span.
        /// </summary>
        public async Task<IList<WallpaperStyle>> GetSupportedStylesAsync()
        {
            try
            {
                return await Task.FromResult(new List<WallpaperStyle>
                {
                    WallpaperStyle.Fill,
                    WallpaperStyle.Fit,
                    WallpaperStyle.Stretch,
                    WallpaperStyle.Tile,
                    WallpaperStyle.Center,
                    WallpaperStyle.Span,
                });
            }
            catch (Exception ex)
            {
                Logger.LogError("Failed to get supported wallpaper styles.", ex);
                return new List<WallpaperStyle>();
            }
        }

        /// <summary>
        /// Backs up the current wallpaper state for potential restoration.
        /// </summary>
        private void BackupState()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(DesktopRegistryPath, false))
                {
                    if (key != null)
                    {
                        _backupImagePath = key.GetValue("Wallpaper")?.ToString();
                        var styleValue = key.GetValue(WallpaperStyleRegistryKey)?.ToString() ?? "0";
                        if (int.TryParse(styleValue, out var styleInt))
                        {
                            _backupStyle = StyleValueToEnum(styleInt, key);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Failed to backup wallpaper state.", ex);
            }
        }

        /// <summary>
        /// Applies the wallpaper to the desktop with the specified style.
        /// </summary>
        private void ApplyWallpaper(string filePath, WallpaperStyle style)
        {
            try
            {
                SetWallpaperStyle(style);
                ChangeWallpaper(filePath);
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to apply wallpaper: {filePath}", ex);
                throw;
            }
        }

        /// <summary>
        /// Changes the wallpaper image using Windows API.
        /// </summary>
        private void ChangeWallpaper(string filePath)
        {
            try
            {
                SystemParametersInfo(
                    SPI_SETDESKWALLPAPER, 0, filePath,
                    SPIF_UPDATEINIFILE | SPIF_SENDCHANGE);
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to change wallpaper via SystemParametersInfo: {filePath}", ex);
                throw;
            }
        }

        /// <summary>
        /// Sets the wallpaper style in the Windows Registry.
        /// </summary>
        private void SetWallpaperStyle(WallpaperStyle style)
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(DesktopRegistryPath, true))
                {
                    if (key != null)
                    {
                        switch (style)
                        {
                            case WallpaperStyle.Fill:
                                key.SetValue(WallpaperStyleRegistryKey, "10");
                                key.SetValue(TileWallpaperRegistryKey, "0");
                                break;
                            case WallpaperStyle.Fit:
                                key.SetValue(WallpaperStyleRegistryKey, "6");
                                key.SetValue(TileWallpaperRegistryKey, "0");
                                break;
                            case WallpaperStyle.Stretch:
                                key.SetValue(WallpaperStyleRegistryKey, "2");
                                key.SetValue(TileWallpaperRegistryKey, "0");
                                break;
                            case WallpaperStyle.Tile:
                                key.SetValue(WallpaperStyleRegistryKey, "0");
                                key.SetValue(TileWallpaperRegistryKey, "1");
                                break;
                            case WallpaperStyle.Center:
                                key.SetValue(WallpaperStyleRegistryKey, "0");
                                key.SetValue(TileWallpaperRegistryKey, "0");
                                break;
                            case WallpaperStyle.Span:
                                key.SetValue(WallpaperStyleRegistryKey, "22");
                                key.SetValue(TileWallpaperRegistryKey, "0");
                                break;
                            default:
                                throw new ArgumentOutOfRangeException(nameof(style), $"Unknown wallpaper style: {style}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to set wallpaper style in Registry: {style}", ex);
                throw;
            }
        }

        /// <summary>
        /// Converts Windows Registry style value to WallpaperStyle enum.
        /// </summary>
        private WallpaperStyle StyleValueToEnum(int styleValue, RegistryKey key)
        {
            var tileValue = key.GetValue(TileWallpaperRegistryKey)?.ToString() ?? "0";
            var isTile = tileValue == "1";

            return styleValue switch
            {
                10 => WallpaperStyle.Fill,
                6 => WallpaperStyle.Fit,
                2 => WallpaperStyle.Stretch,
                0 => isTile ? WallpaperStyle.Tile : WallpaperStyle.Center,
                22 => WallpaperStyle.Span,
                _ => WallpaperStyle.Fill,
            };
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SystemParametersInfo(uint uAction, uint uParam, string lpvParam, uint fuWinIni);
    }
}
