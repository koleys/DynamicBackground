using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Reflection;
using DynamicBackground;
using System.Drawing;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DynamicBackground.Tests
{
    [TestClass]
    public class TestSuiteInit
    {
        public static Dictionary<string, string> SettingsCache;
        public static string SettingsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DynamicBackground.settings.json");

        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            if (File.Exists(SettingsFilePath))
            {
                var json = File.ReadAllText(SettingsFilePath);
                SettingsCache = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            }
            else
            {
                SettingsCache = new Dictionary<string, string>();
            }
        }
    }

    [TestClass]
    public class PictureTests
    {
        [TestMethod]
        [ExpectedException(typeof(Exception), AllowDerivedTypes = true)]
        public void DownloadImage_InvalidUrl_ThrowsException()
        {
            var picture = new Picture();
            picture.DownloadImage("http://invalid-url-for-test");
        }

        [TestMethod]
        public void DownloadImage_ValidUrl_SavesImage()
        {
            // Skip this test if network is unavailable
            try
            {
                System.Net.Dns.GetHostEntry("raw.githubusercontent.com"); // Throws if no network
            }
            catch
            {
                Assert.Inconclusive("Network unavailable or cannot reach test image host.");
                return;
            }
            try
            {
                var picture = new Picture();
                string url = @"https://raw.githubusercontent.com/koleys/DynamicBackground/refs/heads/main/TestImage.jpg";
                string path = picture.DownloadImage(url);
                Assert.IsTrue(File.Exists(path));
                File.Delete(path);
            }
            catch (Exception ex)
            {
                if (ex.InnerException is System.Net.Sockets.SocketException ||
                    (ex.InnerException != null && ex.InnerException.Message.Contains("No such host")))
                {
                    Assert.Inconclusive("Network unavailable or cannot reach test image host.");
                }
                else
                {
                    throw;
                }
            }
        }
    }

    [TestClass]
    public class BingBackgroundTests
    {
        [TestMethod]
        public void GetBackgroundImagePath_ReturnsValidPath()
        {
            var bing = new BingBackground();
            // Use cached settings if available
            var settings = TestSuiteInit.SettingsCache;
            string path = bing.GetBackgroundImagePath();
            Assert.IsFalse(string.IsNullOrEmpty(path));
            Assert.IsTrue(path.EndsWith(".jpg"));
        }

        [TestMethod]
        public void SetAndGetSetting_Works()
        {
            var bing = new BingBackground();
            string key = "TestKey";
            string value = Guid.NewGuid().ToString();
            bing.SetSetting(key, value);
            string result = bing.GetSetting(key);
            Assert.AreEqual(value, result);
        }

        [TestMethod]
        public void EnsureSettingsFile_CreatesFile()
        {
            string settingsPath = TestSuiteInit.SettingsFilePath;
            if (File.Exists(settingsPath)) File.Delete(settingsPath);
            var bing = new BingBackground();
            Assert.IsTrue(File.Exists(settingsPath));
        }

        [TestMethod]
        public void GetSetting_ReturnsNullIfFileMissing()
        {
            string settingsPath = TestSuiteInit.SettingsFilePath;
            if (File.Exists(settingsPath)) File.Delete(settingsPath);
            var bing = new BingBackground();
            File.Delete(settingsPath);
            Assert.IsNull(bing.GetSetting("NonExistentKey"));
        }
    }

    [TestClass]
    public class LoggerTests
    {
        [TestMethod]
        public void LogError_WritesToFileOnFailure()
        {
            // Simulate EventLog failure by using an invalid source name
            string logFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DynamicBackground.log");
            if (File.Exists(logFile)) File.Delete(logFile);
            Logger.LogError("Test error", new Exception("Test exception"));
            Assert.IsTrue(File.Exists(logFile));
        }
    }

    [TestClass]
    public class WallpaperTests
    {
        [TestMethod]
        public void BackupAndRestoreState_DoesNotThrow()
        {
            Wallpaper.BackupState();
            Wallpaper.RestoreState();
        }

        [TestMethod]
        [ExpectedException(typeof(Exception), AllowDerivedTypes = true)]
        public void RestoreState_WithoutBackup_Throws()
        {
            var field = typeof(Wallpaper).GetField("_backupState", BindingFlags.Static | BindingFlags.NonPublic);
            field.SetValue(null, null);
            Wallpaper.RestoreState();
        }
    }
}
