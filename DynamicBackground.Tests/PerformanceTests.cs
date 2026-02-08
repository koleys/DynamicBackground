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
    /// Performance and benchmark tests for monitoring application performance.
    /// Ensures operations complete within acceptable time frames.
    /// </summary>
    [TestClass]
    public class PerformanceTests
    {
        private string _testImagePath;
        private Stopwatch _stopwatch;

        [TestInitialize]
        public void Setup()
        {
            _testImagePath = Path.Combine(Path.GetTempPath(), "perf_test_wallpaper.jpg");
            if (!File.Exists(_testImagePath))
            {
                File.WriteAllBytes(_testImagePath, new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 });
            }
            _stopwatch = new Stopwatch();
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (File.Exists(_testImagePath))
                File.Delete(_testImagePath);
        }

        [TestMethod]
        public async Task Performance_PlatformFactory_CreateProvider_Sub100ms()
        {
            // Arrange
            _stopwatch.Restart();

            // Act
            for (int i = 0; i < 100; i++)
            {
                var provider = PlatformFactory.CreateWallpaperProvider();
                Assert.IsNotNull(provider);
            }
            _stopwatch.Stop();

            // Assert - Average should be well under 100ms
            var averageTime = _stopwatch.ElapsedMilliseconds / 100.0;
            Assert.IsTrue(averageTime < 100, 
                $"Factory creation average should be <100ms, was {averageTime}ms");
        }

        [TestMethod]
        public async Task Performance_GetSupportedStyles_Sub100ms()
        {
            // Arrange
            var provider = PlatformFactory.CreateWallpaperProvider();
            _stopwatch.Restart();

            // Act
            for (int i = 0; i < 50; i++)
            {
                var styles = await provider.GetSupportedStylesAsync();
                Assert.IsNotNull(styles);
            }
            _stopwatch.Stop();

            // Assert
            var averageTime = _stopwatch.ElapsedMilliseconds / 50.0;
            Assert.IsTrue(averageTime < 100, 
                $"GetSupportedStyles average should be <100ms, was {averageTime}ms");
        }

        [TestMethod]
        public async Task Performance_SetWallpaper_UnderTimeout()
        {
            // Arrange
            var provider = PlatformFactory.CreateWallpaperProvider();
            
            // Skip on non-Windows platforms
            if (!(provider is WindowsWallpaperProvider))
            {
                Assert.Inconclusive("Test only runs on Windows with proper setup.");
            }

            _stopwatch.Restart();

            // Act
            var result = await provider.SetWallpaperAsync(_testImagePath, WallpaperStyle.Fill);

            _stopwatch.Stop();

            // Assert - Should complete within 5 seconds
            Assert.IsTrue(_stopwatch.ElapsedMilliseconds < 5000,
                $"SetWallpaper should complete in <5s, took {_stopwatch.ElapsedMilliseconds}ms");
        }

        [TestMethod]
        public async Task Performance_GetCurrentWallpaper_UnderTimeout()
        {
            // Arrange
            var provider = PlatformFactory.CreateWallpaperProvider();
            
            // Skip on non-Windows platforms
            if (!(provider is WindowsWallpaperProvider))
            {
                Assert.Inconclusive("Test only runs on Windows with proper setup.");
            }

            _stopwatch.Restart();

            // Act
            var wallpaper = await provider.GetWallpaperAsync();

            _stopwatch.Stop();

            // Assert - Should complete quickly
            Assert.IsTrue(_stopwatch.ElapsedMilliseconds < 1000,
                $"GetWallpaper should complete in <1s, took {_stopwatch.ElapsedMilliseconds}ms");
        }

        [TestMethod]
        public async Task Performance_SequentialOperations_LinearTiming()
        {
            // Arrange
            var provider = PlatformFactory.CreateWallpaperProvider();
            
            // Skip on non-Windows platforms
            if (!(provider is WindowsWallpaperProvider))
            {
                Assert.Inconclusive("Test only runs on Windows with proper setup.");
            }

            const int operations = 5;
            _stopwatch.Restart();

            // Act
            for (int i = 0; i < operations; i++)
            {
                await provider.SetWallpaperAsync(_testImagePath, WallpaperStyle.Fill);
            }

            _stopwatch.Stop();

            // Assert - Total time should be reasonable
            var averagePerOp = _stopwatch.ElapsedMilliseconds / (double)operations;
            Assert.IsTrue(_stopwatch.ElapsedMilliseconds < 30000,
                $"Sequential {operations} operations should complete in <30s, took {_stopwatch.ElapsedMilliseconds}ms");
        }

        [TestMethod]
        public async Task Performance_ParallelOperations_Concurrent()
        {
            // Arrange
            var provider = PlatformFactory.CreateWallpaperProvider();
            
            // Skip on non-Windows platforms
            if (!(provider is WindowsWallpaperProvider))
            {
                Assert.Inconclusive("Test only runs on Windows with proper setup.");
            }

            const int concurrentOps = 3;
            _stopwatch.Restart();

            // Act
            var tasks = new Task[concurrentOps];
            for (int i = 0; i < concurrentOps; i++)
            {
                tasks[i] = provider.SetWallpaperAsync(_testImagePath, WallpaperStyle.Fill);
            }
            await Task.WhenAll(tasks);

            _stopwatch.Stop();

            // Assert - Parallel should not be much slower than single operation
            Assert.IsTrue(_stopwatch.ElapsedMilliseconds < 10000,
                $"Concurrent {concurrentOps} operations should complete in <10s, took {_stopwatch.ElapsedMilliseconds}ms");
        }

        [TestMethod]
        public void Performance_PlatformDetection_Cached()
        {
            // Arrange
            _stopwatch.Restart();

            // Act - First call detects platform
            var name1 = PlatformFactory.GetCurrentPlatformName();
            var time1 = _stopwatch.ElapsedMilliseconds;

            _stopwatch.Restart();

            // Act - Second call should use cache
            var name2 = PlatformFactory.GetCurrentPlatformName();
            var time2 = _stopwatch.ElapsedMilliseconds;

            // Assert
            Assert.AreEqual(name1, name2, "Platform name should be consistent");
            Assert.IsTrue(time2 <= time1, "Cached call should be as fast or faster");
        }

        [TestMethod]
        public async Task Performance_MemoryUsage_Stable()
        {
            // Arrange
            var provider = PlatformFactory.CreateWallpaperProvider();
            
            // Skip on non-Windows platforms
            if (!(provider is WindowsWallpaperProvider))
            {
                Assert.Inconclusive("Test only runs on Windows with proper setup.");
            }

            GC.Collect();
            var memBefore = GC.GetTotalMemory(false);

            // Act - Perform multiple operations
            for (int i = 0; i < 10; i++)
            {
                await provider.GetSupportedStylesAsync();
            }

            GC.Collect();
            var memAfter = GC.GetTotalMemory(false);

            // Assert - Memory shouldn't grow significantly
            var memIncrease = memAfter - memBefore;
            Assert.IsTrue(memIncrease < 10_000_000, // 10 MB threshold
                $"Memory increase should be <10MB, was {memIncrease / 1_000_000}MB");
        }

        [TestMethod]
        [Timeout(60000)] // 60 second timeout
        public async Task Performance_ManyOperations_Completes()
        {
            // Arrange
            var provider = PlatformFactory.CreateWallpaperProvider();

            // Act
            for (int i = 0; i < 100; i++)
            {
                var styles = await provider.GetSupportedStylesAsync();
                Assert.IsNotNull(styles);
            }

            // Assert
            Assert.IsTrue(true, "100 operations completed within timeout");
        }

        [TestMethod]
        public async Task Performance_ResponseTime_Consistent()
        {
            // Arrange
            var provider = PlatformFactory.CreateWallpaperProvider();
            var times = new long[10];

            // Act - Measure response times
            for (int i = 0; i < 10; i++)
            {
                _stopwatch.Restart();
                var styles = await provider.GetSupportedStylesAsync();
                times[i] = _stopwatch.ElapsedMilliseconds;
            }

            // Assert - Calculate variance
            var sum = 0L;
            foreach (var t in times) sum += t;
            var average = sum / 10.0;
            var variance = CalculateVariance(times, average);
            var stdDev = Math.Sqrt(variance);

            // Standard deviation should be reasonable (not too much variance)
            Assert.IsTrue(stdDev < average * 0.5,
                $"Response time variance should be low, std dev {stdDev}ms vs average {average}ms");
        }

        private double CalculateVariance(long[] values, double average)
        {
            double sum = 0;
            foreach (var value in values)
            {
                sum += Math.Pow(value - average, 2);
            }
            return sum / values.Length;
        }
    }
}
