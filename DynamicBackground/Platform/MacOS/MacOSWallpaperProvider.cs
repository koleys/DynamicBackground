using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using DynamicBackground.Services.Abstractions;

namespace DynamicBackground.Platform.MacOS
{
    /// <summary>
    /// macOS-specific wallpaper provider implementation.
    /// Uses AppleScript and osascript command-line tool to manage wallpapers.
    /// </summary>
    public class MacOSWallpaperProvider : IWallpaperProvider
    {
        /// <summary>
        /// Gets the name of this platform.
        /// </summary>
        public string PlatformName => "macOS";

        /// <summary>
        /// Sets the wallpaper on macOS using AppleScript via osascript.
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

                        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                        {
                            Logger.LogWarning("macOS wallpaper provider called on non-macOS platform.");
                            return false;
                        }

                        // AppleScript to set desktop wallpaper on all screens
                        var appleScript = $@"
tell application ""System Events""
    set desktopCount to count of desktops
    repeat with desktopNumber from 1 to desktopCount
        tell desktop desktopNumber
            set picture to POSIX file ""{imagePath}""
        end tell
    end repeat
end tell
";
                        return ExecuteAppleScript(appleScript);
                    }
                    catch (OperationCanceledException)
                    {
                        Logger.LogWarning("macOS wallpaper operation cancelled.");
                        return false;
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError($"Failed to set wallpaper on macOS: {imagePath}", ex);
                        return false;
                    }
                }, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                Logger.LogWarning("macOS wallpaper operation cancelled.");
                return false;
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to set wallpaper on macOS: {imagePath}", ex);
                return false;
            }
        }

        /// <summary>
        /// Gets the current wallpaper path on macOS using AppleScript.
        /// </summary>
        public async Task<string> GetWallpaperAsync(CancellationToken cancellationToken = default)
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    {
                        Logger.LogWarning("macOS wallpaper provider called on non-macOS platform.");
                        return (string)null;
                    }

                    const string appleScript = @"
tell application ""System Events""
    tell desktop 1
        return POSIX path of picture
    end tell
end tell
";
                    var result = ExecuteAppleScriptWithOutput(appleScript);
                    return string.IsNullOrEmpty(result) ? null : result.Trim();
                }
                catch (Exception ex)
                {
                    Logger.LogError("Failed to get wallpaper on macOS.", ex);
                    return (string)null;
                }
            }, cancellationToken);
        }

        /// <summary>
        /// Gets the wallpaper styles supported on macOS.
        /// macOS supports Fill and Fit styles (controlled via System Preferences).
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
        /// Executes an AppleScript command and returns success status.
        /// </summary>
        private bool ExecuteAppleScript(string script)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "/usr/bin/osascript",
                    Arguments = $"-e \"{script.Replace("\"", "\\\"")}\"",
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
                Logger.LogError("AppleScript execution failed.", ex);
                return false;
            }
        }

        /// <summary>
        /// Executes an AppleScript command and returns its output.
        /// </summary>
        private string ExecuteAppleScriptWithOutput(string script)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "/usr/bin/osascript",
                    Arguments = $"-e \"{script.Replace("\"", "\\\"")}\"",
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
                Logger.LogError("AppleScript output execution failed.", ex);
                return null;
            }
        }
    }
}
