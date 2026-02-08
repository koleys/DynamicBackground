using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using DynamicBackground.Services.Abstractions;

namespace DynamicBackground.Platform.Linux
{
    /// <summary>
    /// Linux-specific wallpaper provider implementation.
    /// Detects desktop environment (GNOME, KDE, Xfce, etc.) and sets wallpaper accordingly.
    /// Falls back to generic dconf/xconf methods for unknown environments.
    /// </summary>
    public class LinuxWallpaperProvider : IWallpaperProvider
    {
        private DesktopEnvironment _cachedDE = DesktopEnvironment.Unknown;

        /// <summary>
        /// Gets the name of this platform.
        /// </summary>
        public string PlatformName => "Linux";

        /// <summary>
        /// Sets the wallpaper on Linux using appropriate desktop environment tools.
        /// Supports GNOME, KDE, Xfce, and other common desktop environments.
        /// </summary>
        public async Task<bool> SetWallpaperAsync(string imagePath, WallpaperStyle style, CancellationToken cancellationToken = default)
        {
            try
            {
                return await Task.Run(() =>
                {
                    try
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                        {
                            Logger.LogWarning("Linux wallpaper provider called on non-Linux platform.");
                            return false;
                        }

                        var de = DetectDesktopEnvironment();
                        return de switch
                        {
                            DesktopEnvironment.GNOME => SetWallpaperGNOME(imagePath),
                            DesktopEnvironment.KDE => SetWallpaperKDE(imagePath),
                            DesktopEnvironment.Xfce => SetWallpaperXfce(imagePath),
                            DesktopEnvironment.MATE => SetWallpaperMATE(imagePath),
                            DesktopEnvironment.Cinnamon => SetWallpaperCinnamon(imagePath),
                            _ => SetWallpaperGeneric(imagePath)
                        };
                    }
                    catch (OperationCanceledException)
                    {
                        Logger.LogWarning("Linux wallpaper operation cancelled.");
                        return false;
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError($"Failed to set wallpaper on Linux: {imagePath}", ex);
                        return false;
                    }
                }, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                Logger.LogWarning("Linux wallpaper operation cancelled.");
                return false;
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to set wallpaper on Linux: {imagePath}", ex);
                return false;
            }
        }

        /// <summary>
        /// Gets the current wallpaper path on Linux.
        /// </summary>
        public async Task<string> GetWallpaperAsync(CancellationToken cancellationToken = default)
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    {
                        Logger.LogWarning("Linux wallpaper provider called on non-Linux platform.");
                        return (string)null;
                    }

                    var de = DetectDesktopEnvironment();
                    return de switch
                    {
                        DesktopEnvironment.GNOME => GetWallpaperGNOME(),
                        DesktopEnvironment.KDE => GetWallpaperKDE(),
                        DesktopEnvironment.Xfce => GetWallpaperXfce(),
                        DesktopEnvironment.MATE => GetWallpaperMATE(),
                        DesktopEnvironment.Cinnamon => GetWallpaperCinnamon(),
                        _ => null
                    };
                }
                catch (Exception ex)
                {
                    Logger.LogError("Failed to get wallpaper on Linux.", ex);
                    return (string)null;
                }
            }, cancellationToken);
        }

        /// <summary>
        /// Gets the wallpaper styles supported on Linux.
        /// Linux desktop environments support Fill and Fit styles.
        /// </summary>
        public async Task<IList<WallpaperStyle>> GetSupportedStylesAsync()
        {
            return await Task.FromResult(new List<WallpaperStyle>
            {
                WallpaperStyle.Fill,
                WallpaperStyle.Fit,
            });
        }

        /// <summary>
        /// Detects the running desktop environment.
        /// </summary>
        private DesktopEnvironment DetectDesktopEnvironment()
        {
            if (_cachedDE != DesktopEnvironment.Unknown)
                return _cachedDE;

            var xdgCurrentDesktop = Environment.GetEnvironmentVariable("XDG_CURRENT_DESKTOP")?.ToLower() ?? "";
            var xdgSessionType = Environment.GetEnvironmentVariable("XDG_SESSION_TYPE")?.ToLower() ?? "";

            _cachedDE = xdgCurrentDesktop switch
            {
                var x when x.Contains("gnome") => DesktopEnvironment.GNOME,
                var x when x.Contains("kde") || x.Contains("plasmadesktop") => DesktopEnvironment.KDE,
                var x when x.Contains("xfce") => DesktopEnvironment.Xfce,
                var x when x.Contains("mate") => DesktopEnvironment.MATE,
                var x when x.Contains("cinnamon") => DesktopEnvironment.Cinnamon,
                _ => DesktopEnvironment.Generic
            };

            return _cachedDE;
        }

        /// <summary>
        /// Sets wallpaper using GNOME settings.
        /// </summary>
        private bool SetWallpaperGNOME(string imagePath)
        {
            var commands = new[]
            {
                $"gsettings set org.gnome.desktop.background picture-uri 'file://{imagePath}'",
                $"gsettings set org.gnome.desktop.background picture-uri-dark 'file://{imagePath}'",
                "gsettings set org.gnome.desktop.background picture-options 'zoom'"
            };

            foreach (var cmd in commands)
            {
                if (!ExecuteShellCommand("bash", $"-c \"{cmd}\""))
                    Logger.LogWarning($"GNOME command failed: {cmd}");
            }
            return true;
        }

        /// <summary>
        /// Gets wallpaper using GNOME settings.
        /// </summary>
        private string GetWallpaperGNOME()
        {
            var output = ExecuteShellCommandWithOutput("bash", "-c \"gsettings get org.gnome.desktop.background picture-uri\"");
            return CleanGSettingsPath(output);
        }

        /// <summary>
        /// Sets wallpaper using KDE Plasma.
        /// </summary>
        private bool SetWallpaperKDE(string imagePath)
        {
            var configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ".config/plasmarc");

            try
            {
                // KDE uses plasmarc configuration file
                var command = $@"sed -i 's|wallpaper=.*|wallpaper={imagePath}|g' {configPath}";
                ExecuteShellCommand("bash", $"-c \"{command}\"");

                // Restart plasmashell
                ExecuteShellCommand("bash", "-c \"kquitapp5 plasmashell 2>/dev/null; sleep 1; kstart5 plasmashell >/dev/null 2>&1 &\"");
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError("KDE wallpaper setting failed.", ex);
                return false;
            }
        }

        /// <summary>
        /// Gets wallpaper using KDE Plasma.
        /// </summary>
        private string GetWallpaperKDE()
        {
            var configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ".config/plasmarc");

            try
            {
                if (File.Exists(configPath))
                {
                    var content = File.ReadAllText(configPath);
                    var match = System.Text.RegularExpressions.Regex.Match(content, @"wallpaper=(.*)");
                    if (match.Success)
                        return match.Groups[1].Value.Trim();
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Failed to get KDE wallpaper.", ex);
            }
            return null;
        }

        /// <summary>
        /// Sets wallpaper using Xfce4.
        /// </summary>
        private bool SetWallpaperXfce(string imagePath)
        {
            var commands = new[]
            {
                $"xfconf-query -c xfce4-desktop -p /backdrop/screen0/monitor0/image-path -s '{imagePath}' -n -t string",
                $"xfconf-query -c xfce4-desktop -p /backdrop/screen0/monitor0/image-show -s true -n -t bool"
            };

            foreach (var cmd in commands)
            {
                if (!ExecuteShellCommand("bash", $"-c \"{cmd}\""))
                    Logger.LogWarning($"Xfce command failed: {cmd}");
            }
            return true;
        }

        /// <summary>
        /// Gets wallpaper using Xfce4.
        /// </summary>
        private string GetWallpaperXfce()
        {
            var output = ExecuteShellCommandWithOutput("bash",
                "-c \"xfconf-query -c xfce4-desktop -p /backdrop/screen0/monitor0/image-path\"");
            return output?.Trim();
        }

        /// <summary>
        /// Sets wallpaper using MATE desktop environment.
        /// </summary>
        private bool SetWallpaperMATE(string imagePath)
        {
            var commands = new[]
            {
                $"gsettings set org.mate.background picture-filename '{imagePath}'",
                "gsettings set org.mate.background picture-options 'zoom'"
            };

            foreach (var cmd in commands)
            {
                if (!ExecuteShellCommand("bash", $"-c \"{cmd}\""))
                    Logger.LogWarning($"MATE command failed: {cmd}");
            }
            return true;
        }

        /// <summary>
        /// Gets wallpaper using MATE desktop environment.
        /// </summary>
        private string GetWallpaperMATE()
        {
            var output = ExecuteShellCommandWithOutput("bash",
                "-c \"gsettings get org.mate.background picture-filename\"");
            return CleanGSettingsPath(output);
        }

        /// <summary>
        /// Sets wallpaper using Cinnamon desktop environment.
        /// </summary>
        private bool SetWallpaperCinnamon(string imagePath)
        {
            var commands = new[]
            {
                $"gsettings set org.cinnamon.desktop.background picture-uri 'file://{imagePath}'",
                "gsettings set org.cinnamon.desktop.background picture-options 'zoom'"
            };

            foreach (var cmd in commands)
            {
                if (!ExecuteShellCommand("bash", $"-c \"{cmd}\""))
                    Logger.LogWarning($"Cinnamon command failed: {cmd}");
            }
            return true;
        }

        /// <summary>
        /// Gets wallpaper using Cinnamon desktop environment.
        /// </summary>
        private string GetWallpaperCinnamon()
        {
            var output = ExecuteShellCommandWithOutput("bash",
                "-c \"gsettings get org.cinnamon.desktop.background picture-uri\"");
            return CleanGSettingsPath(output);
        }

        /// <summary>
        /// Generic fallback wallpaper setting method.
        /// </summary>
        private bool SetWallpaperGeneric(string imagePath)
        {
            Logger.LogWarning("Unknown desktop environment; attempting generic wallpaper setting.");
            return SetWallpaperGNOME(imagePath); // Try GNOME first as fallback
        }

        /// <summary>
        /// Executes a shell command and returns success status.
        /// </summary>
        private bool ExecuteShellCommand(string program, string arguments)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = program,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(psi))
                {
                    process?.WaitForExit(5000);
                    return process?.ExitCode == 0;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Shell command execution failed: {program} {arguments}", ex);
                return false;
            }
        }

        /// <summary>
        /// Executes a shell command and returns its output.
        /// </summary>
        private string ExecuteShellCommandWithOutput(string program, string arguments)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = program,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(psi))
                {
                    var output = process?.StandardOutput.ReadToEnd();
                    process?.WaitForExit(5000);
                    return process?.ExitCode == 0 ? output : null;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Shell command output execution failed: {program} {arguments}", ex);
                return null;
            }
        }

        /// <summary>
        /// Cleans gsettings output path (removes quotes and 'file://' prefix).
        /// </summary>
        private string CleanGSettingsPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            var cleaned = path.Trim().Trim('\'', '"');
            if (cleaned.StartsWith("file://"))
                cleaned = cleaned.Substring("file://".Length);

            return string.IsNullOrEmpty(cleaned) ? null : cleaned;
        }

        /// <summary>
        /// Enum for detected desktop environments.
        /// </summary>
        private enum DesktopEnvironment
        {
            Unknown = 0,
            Generic = 1,
            GNOME = 2,
            KDE = 3,
            Xfce = 4,
            MATE = 5,
            Cinnamon = 6
        }
    }
}
