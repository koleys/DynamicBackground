using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Reflection;
using DynamicBackground; // Add this for access to main project classes

namespace DynamicBackground.Tests
{
    [TestClass]
    public class PictureTests
    {
        [TestMethod]
        [ExpectedException(typeof(Exception), AllowDerivedTypes = true)]
        public void DownloadImage_InvalidUrl_ThrowsException()
        {
            var picture = new DynamicBackground.Picture();
            picture.DownloadImage("http://invalid-url-for-test");
        }
    }

    [TestClass]
    public class BingBackgroundTests
    {
        [TestMethod]
        public void GetBackgroundImagePath_ReturnsValidPath()
        {
            var bing = new DynamicBackground.BingBackground();
            string path = bing.GetBackgroundImagePath();
            Assert.IsFalse(string.IsNullOrEmpty(path));
            Assert.IsTrue(path.EndsWith(".jpg"));
        }

        [TestMethod]
        public void SetAndGetSetting_Works()
        {
            var bing = new DynamicBackground.BingBackground();
            string key = "TestKey";
            string value = Guid.NewGuid().ToString();
            bing.SetSetting(key, value);
            string result = bing.GetSetting(key);
            Assert.AreEqual(value, result);
        }
    }

    [TestClass]
    public class WallpaperTests
    {
        [TestMethod]
        public void BackupAndRestoreState_DoesNotThrow()
        {
            DynamicBackground.Wallpaper.BackupState();
            DynamicBackground.Wallpaper.RestoreState();
        }

        [TestMethod]
        [ExpectedException(typeof(Exception), AllowDerivedTypes = true)]
        public void RestoreState_WithoutBackup_Throws()
        {
            var field = typeof(DynamicBackground.Wallpaper).GetField("_backupState", BindingFlags.Static | BindingFlags.NonPublic);
            field.SetValue(null, null);
            DynamicBackground.Wallpaper.RestoreState();
        }
    }
}
