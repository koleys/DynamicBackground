namespace DynamicBackground
{
    using Microsoft.Win32;
    using System;
    using System.Runtime.InteropServices;

    public enum WallpaperStyle
    {
        Fill,
        Fit,
        Stretch,
        Tile,
        Center,
        Span,
    }

    public static class Wallpaper
    {
        private const string DesktopRegistryPath = @"Control Panel\Desktop";
        private const int HistoryMaxEntries = 5;
        private const string HistoryRegistryPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Wallpapers";
        private const int SpiSetDeskWallpaper = 20;
        private const int SpifSendWinIniChange = 0x02;
        private const int SpifUpdateIniFile = 0x01;
        private const string TileWallpaperRegistryKey = "TileWallpaper";
        private const string WallpaperStyleRegistryKey = "WallpaperStyle";
        private static State? _backupState;
        private static bool _historyRestored;

        public static void BackupState()
        {
            try
            {
                var history = new string[HistoryMaxEntries];
                using (var key = Registry.CurrentUser.OpenSubKey(HistoryRegistryPath, true))
                {
                    for (var i = 0; i < history.Length; i++)
                        history[i] = (string)key.GetValue($"BackgroundHistoryPath{i}");
                }
                _backupState = new State
                {
                    Config = GetWallpaperConfig(),
                    History = history,
                    Wallpaper = history[0],
                };
                _historyRestored = false;
            }
            catch (Exception ex)
            {
                Logger.LogError("Failed to backup wallpaper state.", ex);
                throw;
            }
        }

        public static void RestoreState()
        {
            try
            {
                if (!_backupState.HasValue)
                    throw new Exception("You must call BackupState() before.");
                SetWallpaperConfig(_backupState.Value.Config);
                ChangeWallpaper(_backupState.Value.Wallpaper);
                RestoreHistory();
                _backupState = null;
            }
            catch (Exception ex)
            {
                Logger.LogError("Failed to restore wallpaper state.", ex);
                throw;
            }
        }

        public static void Set(string filePath)
        {
            try
            {
                BackupState();
                ChangeWallpaper(filePath);
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to set wallpaper: {filePath}", ex);
                throw;
            }
        }

        public static void Set(string filePath, WallpaperStyle style)
        {
            try
            {
                BackupState();
                SetStyle(style);
                ChangeWallpaper(filePath);
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to set wallpaper with style: {filePath}, {style}", ex);
                throw;
            }
        }

        public static void SilentSet(string filePath)
        {
            try
            {
                Set(filePath);
                RestoreHistory();
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to silent set wallpaper: {filePath}", ex);
                throw;
            }
        }

        public static void SilentSet(string filePath, WallpaperStyle style)
        {
            try
            {
                Set(filePath, style);
                RestoreHistory();
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to silent set wallpaper with style: {filePath}, {style}", ex);
                throw;
            }
        }

        private static void ChangeWallpaper(string filePath)
        {
            try
            {
                SystemParametersInfo(SpiSetDeskWallpaper, 0, filePath, SpifUpdateIniFile | SpifSendWinIniChange);
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to change wallpaper: {filePath}", ex);
                throw;
            }
        }

        private static int GetRegistryValue(RegistryKey key, string name, int defaultValue)
        {
            return int.Parse((string)key.GetValue(name) ?? defaultValue.ToString());
        }

        private static bool GetRegistryValue(RegistryKey key, string name, bool defaultValue)
        {
            return ((string)key.GetValue(name) ?? (defaultValue ? "1" : "0")) == "1";
        }

        private static Config GetWallpaperConfig()
        {
            using (var key = Registry.CurrentUser.OpenSubKey(DesktopRegistryPath, true))
            {
                return new Config
                {
                    Style = GetRegistryValue(key, WallpaperStyleRegistryKey, 0),
                    IsTile = GetRegistryValue(key, TileWallpaperRegistryKey, false),
                };
            }
        }

        private static void RestoreHistory()
        {
            try
            {
                if (_historyRestored) return;
                if (!_backupState.HasValue)
                    throw new Exception("You must call BackupState() before.");
                var backupState = _backupState.Value;
                using (var key = Registry.CurrentUser.OpenSubKey(HistoryRegistryPath, true))
                {
                    for (var i = 0; i < HistoryMaxEntries; i++)
                        if (backupState.History[i] != null)
                            key.SetValue($"BackgroundHistoryPath{i}", backupState.History[i], RegistryValueKind.String);
                }
                _historyRestored = true;
            }
            catch (Exception ex)
            {
                Logger.LogError("Failed to restore wallpaper history.", ex);
                throw;
            }
        }

        private static void SetRegistryValue(RegistryKey key, string name, int value)
        {
            key.SetValue(name, value.ToString());
        }

        private static void SetRegistryValue(RegistryKey key, string name, bool value)
        {
            key.SetValue(name, value ? "1" : "0");
        }

        private static void SetStyle(WallpaperStyle style)
        {
            switch (style)
            {
                case WallpaperStyle.Fill:
                    SetWallpaperConfig(new Config { Style = 10, IsTile = false });
                    break;
                case WallpaperStyle.Fit:
                    SetWallpaperConfig(new Config { Style = 6, IsTile = false });
                    break;
                case WallpaperStyle.Stretch:
                    SetWallpaperConfig(new Config { Style = 2, IsTile = false });
                    break;
                case WallpaperStyle.Tile:
                    SetWallpaperConfig(new Config { Style = 0, IsTile = true });
                    break;
                case WallpaperStyle.Center:
                    SetWallpaperConfig(new Config { Style = 0, IsTile = false });
                    break;
                case WallpaperStyle.Span:
                    SetWallpaperConfig(new Config { Style = 22, IsTile = false });
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(style));
            }
        }

        private static void SetWallpaperConfig(Config config)
        {
            using (var key = Registry.CurrentUser.OpenSubKey(DesktopRegistryPath, true))
            {
                SetRegistryValue(key, WallpaperStyleRegistryKey, config.Style);
                SetRegistryValue(key, TileWallpaperRegistryKey, config.IsTile);
            }
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

        private struct Config
        {
            public bool IsTile;
            public int Style;
        }

        private struct State
        {
            public Config Config;
            public string[] History;
            public string Wallpaper;
        }
    }
}