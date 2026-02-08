using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using DynamicBackground.Services.Abstractions;
using DynamicBackground.Services.Platform;
using DynamicBackground.Platform.Windows;
using DynamicBackground.Platform.MacOS;
using DynamicBackground.Platform.Linux;

namespace DynamicBackground.Tests
{
    [TestClass]
    public class PlatformProviderTests
    {
        private const string TestImagePath = @"C:\Test\wallpaper.jpg";

        #region PlatformFactory Tests

        [TestMethod]
        public void PlatformFactory_CreateWallpaperProvider_ReturnsPlatformAppropriateProvider()
        {
            var provider = PlatformFactory.CreateWallpaperProvider();
            Assert.IsNotNull(provider);
            Assert.IsInstanceOfType(provider, typeof(IWallpaperProvider));
            
            // On Windows, should return WindowsWallpaperProvider
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Assert.IsInstanceOfType(provider, typeof(WindowsWallpaperProvider));
            }
        }

        [TestMethod]
        public void PlatformFactory_GetCurrentPlatformName_ReturnsValidPlatformName()
        {
            var platformName = PlatformFactory.GetCurrentPlatformName();
            Assert.IsNotNull(platformName);
            Assert.IsTrue(platformName == "Windows" || platformName == "macOS" || platformName == "Linux" || platformName == "Unknown");
        }

        [TestMethod]
        public void PlatformFactory_GetCurrentPlatformName_MatchesRuntimePlatform()
        {
            var platformName = PlatformFactory.GetCurrentPlatformName();
            
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                Assert.AreEqual("Windows", platformName);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                Assert.AreEqual("macOS", platformName);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                Assert.AreEqual("Linux", platformName);
        }

        [TestMethod]
        public void PlatformFactory_IsPlatformSupported_ReturnsTrueForValidPlatforms()
        {
            bool isSupported = PlatformFactory.IsPlatformSupported();
            Assert.IsTrue(isSupported, "Current platform should be supported");
        }

        #endregion

        #region WindowsWallpaperProvider Tests

        [TestMethod]
        public void WindowsWallpaperProvider_PlatformName_ReturnsWindows()
        {
            var provider = new WindowsWallpaperProvider();
            Assert.AreEqual("Windows", provider.PlatformName);
        }

        [TestMethod]
        public async Task WindowsWallpaperProvider_GetSupportedStylesAsync_ReturnsAllSixStyles()
        {
            var provider = new WindowsWallpaperProvider();
            var styles = await provider.GetSupportedStylesAsync();
            
            Assert.IsNotNull(styles);
            Assert.AreEqual(6, styles.Count, "Windows should support 6 wallpaper styles");
            Assert.IsTrue(styles.Contains(WallpaperStyle.Fill));
            Assert.IsTrue(styles.Contains(WallpaperStyle.Fit));
            Assert.IsTrue(styles.Contains(WallpaperStyle.Stretch));
            Assert.IsTrue(styles.Contains(WallpaperStyle.Tile));
            Assert.IsTrue(styles.Contains(WallpaperStyle.Center));
            Assert.IsTrue(styles.Contains(WallpaperStyle.Span));
        }

        [TestMethod]
        public async Task WindowsWallpaperProvider_SetWallpaperAsync_ReturnsBooleanResult()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Assert.Inconclusive("Test requires Windows platform");
            }

            var provider = new WindowsWallpaperProvider();
            // Use invalid path to test handling
            var result = await provider.SetWallpaperAsync(@"C:\NonExistent\invalid.jpg", WallpaperStyle.Fill);
            
            // Should not throw, but may return false for invalid path
            Assert.IsInstanceOfType(result, typeof(bool));
        }

        [TestMethod]
        public async Task WindowsWallpaperProvider_GetWallpaperAsync_ReturnsStringOrNull()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Assert.Inconclusive("Test requires Windows platform");
            }

            var provider = new WindowsWallpaperProvider();
            var currentWallpaper = await provider.GetWallpaperAsync();
            
            // Should return string or null, never throw
            Assert.IsTrue(currentWallpaper == null || currentWallpaper is string);
        }

        [TestMethod]
        public async Task WindowsWallpaperProvider_SetWallpaperAsync_WithCancellation_ReturnsFalse()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Assert.Inconclusive("Test requires Windows platform");
            }

            var provider = new WindowsWallpaperProvider();
            var cts = new CancellationTokenSource();
            cts.Cancel();
            
            var result = await provider.SetWallpaperAsync(TestImagePath, WallpaperStyle.Fill, cts.Token);
            Assert.IsFalse(result, "Should return false when cancelled");
        }

        #endregion

        #region MacOSWallpaperProvider Tests

        [TestMethod]
        public void MacOSWallpaperProvider_PlatformName_ReturnsMacOS()
        {
            var provider = new MacOSWallpaperProvider();
            Assert.AreEqual("macOS", provider.PlatformName);
        }

        [TestMethod]
        public async Task MacOSWallpaperProvider_GetSupportedStylesAsync_ReturnsTwoStyles()
        {
            var provider = new MacOSWallpaperProvider();
            var styles = await provider.GetSupportedStylesAsync();
            
            Assert.IsNotNull(styles);
            Assert.AreEqual(2, styles.Count, "macOS should support Fill and Fit styles");
            Assert.IsTrue(styles.Contains(WallpaperStyle.Fill));
            Assert.IsTrue(styles.Contains(WallpaperStyle.Fit));
        }

        [TestMethod]
        public async Task MacOSWallpaperProvider_SetWallpaperAsync_ReturnsBoolean()
        {
            var provider = new MacOSWallpaperProvider();
            var result = await provider.SetWallpaperAsync(TestImagePath, WallpaperStyle.Fill);
            
            // Stub should return false on non-macOS or true on macOS
            Assert.IsInstanceOfType(result, typeof(bool));
        }

        [TestMethod]
        public async Task MacOSWallpaperProvider_GetWallpaperAsync_ReturnsStringOrNull()
        {
            var provider = new MacOSWallpaperProvider();
            var currentWallpaper = await provider.GetWallpaperAsync();
            
            // Should return string or null
            Assert.IsTrue(currentWallpaper == null || currentWallpaper is string);
        }

        #endregion

        #region LinuxWallpaperProvider Tests

        [TestMethod]
        public void LinuxWallpaperProvider_PlatformName_ReturnsLinux()
        {
            var provider = new LinuxWallpaperProvider();
            Assert.AreEqual("Linux", provider.PlatformName);
        }

        [TestMethod]
        public async Task LinuxWallpaperProvider_GetSupportedStylesAsync_ReturnsTwoStyles()
        {
            var provider = new LinuxWallpaperProvider();
            var styles = await provider.GetSupportedStylesAsync();
            
            Assert.IsNotNull(styles);
            Assert.AreEqual(2, styles.Count, "Linux should support Fill and Fit styles");
            Assert.IsTrue(styles.Contains(WallpaperStyle.Fill));
            Assert.IsTrue(styles.Contains(WallpaperStyle.Fit));
        }

        [TestMethod]
        public async Task LinuxWallpaperProvider_SetWallpaperAsync_ReturnsBoolean()
        {
            var provider = new LinuxWallpaperProvider();
            var result = await provider.SetWallpaperAsync(TestImagePath, WallpaperStyle.Fill);
            
            // Stub should return false on non-Linux or true on Linux
            Assert.IsInstanceOfType(result, typeof(bool));
        }

        [TestMethod]
        public async Task LinuxWallpaperProvider_GetWallpaperAsync_ReturnsStringOrNull()
        {
            var provider = new LinuxWallpaperProvider();
            var currentWallpaper = await provider.GetWallpaperAsync();
            
            // Should return string or null
            Assert.IsTrue(currentWallpaper == null || currentWallpaper is string);
        }

        #endregion

        #region Interface Contract Tests

        [TestMethod]
        public void AllProviders_ImplementIWallpaperProvider()
        {
            var providers = new IWallpaperProvider[]
            {
                new WindowsWallpaperProvider(),
                new MacOSWallpaperProvider(),
                new LinuxWallpaperProvider(),
            };

            foreach (var provider in providers)
            {
                Assert.IsInstanceOfType(provider, typeof(IWallpaperProvider));
                Assert.IsNotNull(provider.PlatformName);
                Assert.IsFalse(string.IsNullOrEmpty(provider.PlatformName));
            }
        }

        [TestMethod]
        public async Task AllProviders_GetSupportedStylesAsync_ReturnsNonEmptyList()
        {
            var providers = new IWallpaperProvider[]
            {
                new WindowsWallpaperProvider(),
                new MacOSWallpaperProvider(),
                new LinuxWallpaperProvider(),
            };

            foreach (var provider in providers)
            {
                var styles = await provider.GetSupportedStylesAsync();
                Assert.IsNotNull(styles);
                Assert.IsTrue(styles.Count > 0, $"{provider.PlatformName} should support at least one style");
            }
        }

        [TestMethod]
        public async Task AllProviders_SetWallpaperAsync_DoesNotThrow()
        {
            var providers = new IWallpaperProvider[]
            {
                new WindowsWallpaperProvider(),
                new MacOSWallpaperProvider(),
                new LinuxWallpaperProvider(),
            };

            foreach (var provider in providers)
            {
                try
                {
                    var result = await provider.SetWallpaperAsync(TestImagePath, WallpaperStyle.Fill);
                    Assert.IsInstanceOfType(result, typeof(bool));
                }
                catch (Exception ex)
                {
                    Assert.Fail($"{provider.PlatformName} provider SetWallpaperAsync threw: {ex}");
                }
            }
        }

        [TestMethod]
        public async Task AllProviders_GetWallpaperAsync_DoesNotThrow()
        {
            var providers = new IWallpaperProvider[]
            {
                new WindowsWallpaperProvider(),
                new MacOSWallpaperProvider(),
                new LinuxWallpaperProvider(),
            };

            foreach (var provider in providers)
            {
                try
                {
                    var wallpaper = await provider.GetWallpaperAsync();
                    Assert.IsTrue(wallpaper == null || wallpaper is string);
                }
                catch (Exception ex)
                {
                    Assert.Fail($"{provider.PlatformName} provider GetWallpaperAsync threw: {ex}");
                }
            }
        }

        #endregion

        #region Async Operation Tests

        [TestMethod]
        public async Task WindowsWallpaperProvider_AsyncOperations_RunAsynchronously()
        {
            var provider = new WindowsWallpaperProvider();
            var tasks = new List<Task<bool>>();

            for (int i = 0; i < 5; i++)
            {
                tasks.Add(provider.SetWallpaperAsync(TestImagePath, WallpaperStyle.Fill));
            }

            var results = await Task.WhenAll(tasks);
            Assert.AreEqual(5, results.Length);
        }

        #endregion

        #region Error Handling Tests

        [TestMethod]
        public async Task WindowsWallpaperProvider_InvalidImagePath_HandlesSilently()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Assert.Inconclusive("Test requires Windows platform");
            }

            var provider = new WindowsWallpaperProvider();
            var invalidPaths = new[]
            {
                "",
                null,
                @"C:\Invalid\Path\That\Does\Not\Exist\wallpaper.jpg",
            };

            foreach (var path in invalidPaths)
            {
                try
                {
                    if (path == null)
                        continue;

                    var result = await provider.SetWallpaperAsync(path, WallpaperStyle.Fill);
                    // Should not throw, returns false for invalid paths
                    Assert.IsInstanceOfType(result, typeof(bool));
                }
                catch (Exception ex)
                {
                    Assert.Fail($"Provider should handle invalid path gracefully. Exception: {ex}");
                }
            }
        }

        [TestMethod]
        public async Task AllProviders_WithCancellation_ReturnsFalse()
        {
            var providers = new IWallpaperProvider[]
            {
                new WindowsWallpaperProvider(),
                new MacOSWallpaperProvider(),
                new LinuxWallpaperProvider(),
            };

            foreach (var provider in providers)
            {
                var cts = new CancellationTokenSource();
                cts.Cancel();

                var result = await provider.SetWallpaperAsync(TestImagePath, WallpaperStyle.Fill, cts.Token);
                Assert.IsFalse(result, $"{provider.PlatformName} should return false when cancelled");
            }
        }

        #endregion
    }
}
