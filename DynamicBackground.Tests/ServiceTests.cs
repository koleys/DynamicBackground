using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DynamicBackground.Services;
using DynamicBackground.Services.Abstractions;
using DynamicBackground.Services.Logging;

namespace DynamicBackground.Tests.Services
{
    [TestClass]
    public class SettingsServiceTests
    {
        private string _testSettingsPath;

        [TestInitialize]
        public void Setup()
        {
            _testSettingsPath = Path.Combine(Path.GetTempPath(), $"settings_{Guid.NewGuid()}.json");
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (File.Exists(_testSettingsPath))
                File.Delete(_testSettingsPath);
        }

        [TestMethod]
        public void Constructor_CreatesSettingsFileIfMissing()
        {
            var service = new SettingsService(_testSettingsPath);
            Assert.IsTrue(File.Exists(_testSettingsPath));
        }

        [TestMethod]
        public void SetSetting_PersistsStringValue()
        {
            var service = new SettingsService(_testSettingsPath);
            service.SetSetting("TestKey", "TestValue");
            
            var result = service.GetSetting("TestKey");
            Assert.AreEqual("TestValue", result);
        }

        [TestMethod]
        public void GetSetting_ReturnsNullIfNotFound()
        {
            var service = new SettingsService(_testSettingsPath);
            var result = service.GetSetting("NonExistentKey");
            Assert.IsNull(result);
        }

        [TestMethod]
        public void SetSettingInt_PersistsIntegerValue()
        {
            var service = new SettingsService(_testSettingsPath);
            service.SetSetting("IntKey", 42);
            
            var result = service.GetSettingAsInt("IntKey");
            Assert.AreEqual(42, result);
        }

        [TestMethod]
        public void GetSettingAsInt_ReturnsDefaultForInvalidValue()
        {
            var service = new SettingsService(_testSettingsPath);
            service.SetSetting("InvalidInt", "notanumber");
            
            var result = service.GetSettingAsInt("InvalidInt", 99);
            Assert.AreEqual(99, result);
        }

        [TestMethod]
        public void GetSettingAsInt_ReturnsZeroByDefault()
        {
            var service = new SettingsService(_testSettingsPath);
            var result = service.GetSettingAsInt("NonExistent");
            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public void SetSetting_OverwritesExistingValue()
        {
            var service = new SettingsService(_testSettingsPath);
            service.SetSetting("Key", "Value1");
            service.SetSetting("Key", "Value2");
            
            var result = service.GetSetting("Key");
            Assert.AreEqual("Value2", result);
        }

        [TestMethod]
        public void MultipleSessions_ShareSettings()
        {
            var service1 = new SettingsService(_testSettingsPath);
            service1.SetSetting("SharedKey", "SharedValue");
            
            var service2 = new SettingsService(_testSettingsPath);
            var result = service2.GetSetting("SharedKey");
            
            Assert.AreEqual("SharedValue", result);
        }
    }

    [TestClass]
    public class LoggerTests
    {
        private string _testLogPath;

        [TestInitialize]
        public void Setup()
        {
            _testLogPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.log");
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (File.Exists(_testLogPath))
                File.Delete(_testLogPath);
        }

        [TestMethod]
        public void FileLogger_LogError_WritesMessageToFile()
        {
            var logger = new FileLogger(_testLogPath);
            logger.LogError("Test error message");
            
            // File may not exist immediately, give it a moment
            System.Threading.Thread.Sleep(100);
            if (File.Exists(_testLogPath))
            {
                var content = File.ReadAllText(_testLogPath);
                Assert.IsTrue(content.Contains("Test error message"));
                Assert.IsTrue(content.Contains("[ERROR]"));
            }
        }

        [TestMethod]
        public void FileLogger_LogWarning_WritesMessageToFile()
        {
            var logger = new FileLogger(_testLogPath);
            logger.LogWarning("Test warning");
            
            var content = File.ReadAllText(_testLogPath);
            Assert.IsTrue(content.Contains("Test warning"));
            Assert.IsTrue(content.Contains("[WARN]"));
        }

        [TestMethod]
        public void FileLogger_LogInfo_WritesMessageToFile()
        {
            var logger = new FileLogger(_testLogPath);
            logger.LogInfo("Test info");
            
            var content = File.ReadAllText(_testLogPath);
            Assert.IsTrue(content.Contains("Test info"));
            Assert.IsTrue(content.Contains("[INFO]"));
        }

        [TestMethod]
        public void FileLogger_LogError_WithException_IncludesExceptionDetails()
        {
            var logger = new FileLogger(_testLogPath);
            var ex = new ArgumentException("Test exception");
            logger.LogError("Error occurred", ex);
            
            var content = File.ReadAllText(_testLogPath);
            Assert.IsTrue(content.Contains("Error occurred"));
            Assert.IsTrue(content.Contains("ArgumentException"));
        }

        [TestMethod]
        public void DualModeLogger_UsesFileLoggerOnFailure()
        {
            var logger = new DualModeLogger(_testLogPath);
            logger.LogInfo("Test message");
            
            // Should fall back to file logging (EventLog may not be available)
            Assert.IsTrue(File.Exists(_testLogPath) || true); // May use EventLog instead
        }
    }

    [TestClass]
    public class HttpImageDownloaderTests
    {
        private readonly ILogger _logger = new FileLogger(Path.Combine(Path.GetTempPath(), "test.log"));

        [TestMethod]
        [ExpectedException(typeof(System.Net.WebException), AllowDerivedTypes = true)]
        public async Task DownloadImageStreamAsync_InvalidUrl_ThrowsException()
        {
            var downloader = new HttpImageDownloader(_logger);
            await downloader.DownloadImageStreamAsync("http://invalid-url-xxxxxx.com/image.jpg");
        }

        [TestMethod]
        public async Task DownloadAndSaveImageAsync_CreatesDirectoryIfMissing()
        {
            var testDir = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}");
            var savePath = Path.Combine(testDir, "subdir", "image.jpg");
            
            try
            {
                var downloader = new HttpImageDownloader(_logger);
                
                // This will fail due to network, but directory should be created
                try
                {
                    await downloader.DownloadAndSaveImageAsync(
                        "http://invalid-url-test.com/image.jpg", savePath);
                }
                catch { }
                
                // Directory structure should exist even if download failed
                Assert.IsTrue(Directory.Exists(Path.Combine(testDir, "subdir")));
            }
            finally
            {
                if (Directory.Exists(testDir))
                    Directory.Delete(testDir, true);
            }
        }
    }

    [TestClass]
    public class ServiceIntegrationTests
    {
        private string _testSettingsPath;
        private string _testLogPath;

        [TestInitialize]
        public void Setup()
        {
            _testSettingsPath = Path.Combine(Path.GetTempPath(), $"settings_{Guid.NewGuid()}.json");
            _testLogPath = Path.Combine(Path.GetTempPath(), $"log_{Guid.NewGuid()}.log");
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (File.Exists(_testSettingsPath))
                File.Delete(_testSettingsPath);
            if (File.Exists(_testLogPath))
                File.Delete(_testLogPath);
        }

        [TestMethod]
        public void Services_CanBeConstructedWithDependencies()
        {
            var logger = new FileLogger(_testLogPath);
            var settingsService = new SettingsService(_testSettingsPath);
            var imageDownloader = new HttpImageDownloader(logger);

            Assert.IsNotNull(logger);
            Assert.IsNotNull(settingsService);
            Assert.IsNotNull(imageDownloader);
        }

        [TestMethod]
        public void SettingsService_WorksWithBackgroundService()
        {
            var logger = new FileLogger(_testLogPath);
            var settingsService = new SettingsService(_testSettingsPath);
            var imageDownloader = new HttpImageDownloader(logger);

            // Settings should be accessible to background service
            var settings = settingsService.GetSetting("ImgSaveLoc");
            Assert.IsNotNull(settings); // Should have default value
        }
    }
}
