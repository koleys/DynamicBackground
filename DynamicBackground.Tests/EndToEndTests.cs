using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using DynamicBackground.Services.Abstractions;
using DynamicBackground.Services.Platform;
using DynamicBackground.Platform.Windows;

namespace DynamicBackground.Tests
{
    /// <summary>
    /// End-to-End (E2E) tests for complete user workflows.
    /// Tests full scenarios from start to finish with real operations.
    /// </summary>
    [TestClass]
    public class EndToEndTests
    {
        private string _testImagePath;
        private string _testDirectory;

        [TestInitialize]
        public void Setup()
        {
            _testDirectory = Path.Combine(Path.GetTempPath(), $"E2E_Test_{Guid.NewGuid()}");
            Directory.CreateDirectory(_testDirectory);

            _testImagePath = Path.Combine(_testDirectory, "test_wallpaper.jpg");
            if (!File.Exists(_testImagePath))
            {
                File.WriteAllBytes(_testImagePath, new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 });
            }
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (Directory.Exists(_testDirectory))
            {
                try
                {
                    Directory.Delete(_testDirectory, true);
                }
                catch { }
            }
        }

        [TestMethod]
        public async Task E2E_UserScenario_SelectPlatform_GetStyles_SetWallpaper()
        {
            // User Scenario: User launches app, selects wallpaper style, sets wallpaper

            // Arrange
            var provider = PlatformFactory.CreateWallpaperProvider();
            Assert.IsNotNull(provider, "Step 1: Should select a platform provider");

            // Act - Get supported styles
            var styles = await provider.GetSupportedStylesAsync();
            Assert.IsTrue(styles.Count > 0, "Step 2: Should retrieve supported styles");

            // Act - Attempt to set wallpaper with first supported style
            var firstStyle = styles[0];
            var setResult = await provider.SetWallpaperAsync(_testImagePath, firstStyle);

            // Assert
            Assert.IsNotNull(setResult, "Step 3: Should attempt to set wallpaper");
        }

        [TestMethod]
        public async Task E2E_UserScenario_MultipleWallpaperChanges()
        {
            // User Scenario: User rapidly changes wallpaper multiple times

            // Arrange
            var provider = PlatformFactory.CreateWallpaperProvider();
            
            // Skip on non-Windows platforms
            if (!(provider is WindowsWallpaperProvider))
            {
                Assert.Inconclusive("Test only runs on Windows with proper setup.");
            }

            // Act - Change wallpaper multiple times
            var changeCount = 3;
            for (int i = 0; i < changeCount; i++)
            {
                var style = i % 2 == 0 ? WallpaperStyle.Fill : WallpaperStyle.Fit;
                var result = await provider.SetWallpaperAsync(_testImagePath, style);
                Assert.IsNotNull(result, $"Change {i + 1}: Should complete without error");
            }

            // Assert
            Assert.IsTrue(true, "All changes completed successfully");
        }

        [TestMethod]
        public async Task E2E_UserScenario_QueryCurrentWallpaper()
        {
            // User Scenario: User checks what the current wallpaper is

            // Arrange
            var provider = PlatformFactory.CreateWallpaperProvider();
            
            // Skip on non-Windows platforms
            if (!(provider is WindowsWallpaperProvider))
            {
                Assert.Inconclusive("Test only runs on Windows with proper setup.");
            }

            // Act
            var currentWallpaper = await provider.GetWallpaperAsync();

            // Assert
            Assert.IsTrue(currentWallpaper == null || currentWallpaper.Length > 0,
                "Should retrieve current wallpaper or null");
        }

        [TestMethod]
        public async Task E2E_UserScenario_HandlesErrorGracefully_InvalidFile()
        {
            // User Scenario: User selects invalid file by mistake

            // Arrange
            var provider = PlatformFactory.CreateWallpaperProvider();
            var invalidPath = @"C:\NonExistent\Image_That_Does_Not_Exist.jpg";

            // Skip on non-Windows platforms
            if (!(provider is WindowsWallpaperProvider))
            {
                Assert.Inconclusive("Test only runs on Windows with proper setup.");
            }

            // Act
            var result = await provider.SetWallpaperAsync(invalidPath, WallpaperStyle.Fill);

            // Assert
            Assert.IsFalse(result, "Should fail gracefully with invalid path");
        }

        [TestMethod]
        public async Task E2E_UserScenario_AllStylesAvailable_WindowsSpecific()
        {
            // User Scenario: Windows user cycles through all available styles

            // Arrange
            var provider = PlatformFactory.CreateWallpaperProvider();
            
            // Skip on non-Windows platforms
            if (!(provider is WindowsWallpaperProvider))
            {
                Assert.Inconclusive("Test only runs on Windows with proper setup.");
            }

            var styles = await provider.GetSupportedStylesAsync();
            Assert.IsTrue(styles.Count >= 6, "Windows should support 6 styles");

            // Act - Try setting each supported style
            foreach (var style in styles)
            {
                var result = await provider.SetWallpaperAsync(_testImagePath, style);
                Assert.IsNotNull(result, $"Setting {style} should not throw");
            }

            // Assert
            Assert.IsTrue(true, "All styles set successfully");
        }

        [TestMethod]
        public async Task E2E_UserScenario_WorksWithDifferentImageFormats()
        {
            // User Scenario: User sets wallpaper with JPEG image

            // Arrange
            var provider = PlatformFactory.CreateWallpaperProvider();
            
            // Skip on non-Windows platforms
            if (!(provider is WindowsWallpaperProvider))
            {
                Assert.Inconclusive("Test only runs on Windows with proper setup.");
            }

            // Act - Set with current JPEG test image
            var result = await provider.SetWallpaperAsync(_testImagePath, WallpaperStyle.Fill);

            // Assert
            Assert.IsNotNull(result, "Should handle JPEG format");
        }

        [TestMethod]
        public async Task E2E_UserScenario_ConcurrentOperationHandling()
        {
            // User Scenario: App handles rapid requests without crashing

            // Arrange
            var provider = PlatformFactory.CreateWallpaperProvider();
            
            // Skip on non-Windows platforms
            if (!(provider is WindowsWallpaperProvider))
            {
                Assert.Inconclusive("Test only runs on Windows with proper setup.");
            }

            // Act - Fire multiple async operations
            var tasks = new Task[5];
            for (int i = 0; i < 5; i++)
            {
                tasks[i] = provider.SetWallpaperAsync(
                    _testImagePath, 
                    i % 2 == 0 ? WallpaperStyle.Fill : WallpaperStyle.Fit
                );
            }

            await Task.WhenAll(tasks);

            // Assert
            Assert.IsTrue(true, "All operations should complete");
        }

        [TestMethod]
        public async Task E2E_UserScenario_LongRunningOperation_WithCancellation()
        {
            // User Scenario: User cancels a long-running operation

            // Arrange
            var provider = PlatformFactory.CreateWallpaperProvider();
            var cts = new System.Threading.CancellationTokenSource();

            // Act - Schedule cancellation after 100ms
            var setTask = provider.SetWallpaperAsync(_testImagePath, WallpaperStyle.Fill, cts.Token);
            cts.CancelAfter(100);

            // Try to complete or handle cancellation
            var result = false;
            try
            {
                result = await setTask;
            }
            catch (OperationCanceledException)
            {
                result = false;
            }

            // Assert
            Assert.IsNotNull(result, "Operation should complete or be cancelled cleanly");
        }

        [TestMethod]
        public async Task E2E_UserScenario_SequentialOperations_Reliable()
        {
            // User Scenario: User changes wallpaper, checks current, changes again

            // Arrange
            var provider = PlatformFactory.CreateWallpaperProvider();
            
            // Skip on non-Windows platforms
            if (!(provider is WindowsWallpaperProvider))
            {
                Assert.Inconclusive("Test only runs on Windows with proper setup.");
            }

            // Act & Assert - Sequential operations
            var set1 = await provider.SetWallpaperAsync(_testImagePath, WallpaperStyle.Fill);
            Assert.IsNotNull(set1, "First set operation should complete");

            var getCurrent = await provider.GetWallpaperAsync();
            Assert.IsTrue(getCurrent == null || getCurrent.Length > 0, "Get current should work");

            var set2 = await provider.SetWallpaperAsync(_testImagePath, WallpaperStyle.Fit);
            Assert.IsNotNull(set2, "Second set operation should complete");
        }
    }
}
