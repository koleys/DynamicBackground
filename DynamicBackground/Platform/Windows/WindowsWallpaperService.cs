using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using DynamicBackground.Services.Abstractions;
using Microsoft.Win32;

namespace DynamicBackground.Platform.Windows
{
    /// <summary>
    /// Windows-specific wallpaper service using Registry and P/Invoke.
    /// </summary>
    public class WindowsWallpaperService : IWallpaperService
    {
        private const uint SPI_SETDESKWALLPAPER = 20;
        private const uint SPIF_UPDATEINIFILE = 0x01;
        private const uint SPIF_SENDCHANGE = 0x02;
        private string? _backupImagePath;
        private WallpaperStyle _backupStyle;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SystemParametersInfo(
            uint uAction, uint uParam, string lpvParam, uint fsWinIni);

        public void Set(string filePath, WallpaperStyle style)
        {
            try
            {
                BackupState();
                ApplyWallpaper(filePath, style);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting wallpaper: {ex}");
                throw;
            }
        }

        public void SilentSet(string filePath, WallpaperStyle style)
        {
            try
            {
                ApplyWallpaper(filePath, style);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error silently setting wallpaper: {ex}");
                throw;
            }
        }

        public void RestoreState()
        {
            try
            {
                if (!string.IsNullOrEmpty(_backupImagePath) && File.Exists(_backupImagePath))
                {
                    ApplyWallpaper(_backupImagePath, _backupStyle);
                    _backupImagePath = null;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error restoring wallpaper: {ex}");
            }
        }

        public void BackupState()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(
                    @"Control Panel\Desktop", false))
                {
                    if (key != null)
                    {
                        _backupImagePath = key.GetValue("Wallpaper")?.ToString();
                        var styleValue = key.GetValue("WallpaperStyle")?.ToString() ?? "0";
                        _backupStyle = (WallpaperStyle)int.Parse(styleValue);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error backing up wallpaper state: {ex}");
            }
        }

        private void ApplyWallpaper(string filePath, WallpaperStyle style)
        {
            try
            {
                // Set Registry values
                using (var key = Registry.CurrentUser.OpenSubKey(
                    @"Control Panel\Desktop", true))
                {
                    if (key != null)
                    {
                        key.SetValue("Wallpaper", filePath);
                        key.SetValue("WallpaperStyle", ((int)style).ToString());
                        key.SetValue("TileWallpaper", style == WallpaperStyle.Tile ? "1" : "0");
                    }
                }

                // Apply wallpaper
                SystemParametersInfo(
                    SPI_SETDESKWALLPAPER, 0, filePath,
                    SPIF_UPDATEINIFILE | SPIF_SENDCHANGE);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error applying wallpaper: {ex}");
                throw;
            }
        }
    }
}
