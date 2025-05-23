using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Collections.Generic;

namespace DynamicBackground
{
    public class BingBackground
    {
        private dynamic _jsonCache;
        private static readonly string SettingsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DynamicBackground.settings.json");
        private static readonly Dictionary<string, string> DefaultSettings = new()
        {
            { "ImgSaveLoc", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "Bing Backgrounds", DateTime.Now.Year.ToString()) },
            { "Interval", "30" }
        };

        public BingBackground()
        {
            EnsureSettingsFile();
        }

        private void EnsureSettingsFile()
        {
            try
            {
                if (!File.Exists(SettingsFilePath))
                {
                    File.WriteAllText(SettingsFilePath, JsonConvert.SerializeObject(DefaultSettings, Formatting.Indented));
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Failed to create initial settings file.", ex);
                // Do not throw, allow app to continue
            }
        }

        public string GetDownloadedImagePath()
        {
            try
            {
                string urlBase = GetBackgroundUrlBase();
                Image backgroundImage = DownloadBackground(urlBase + GetResolutionExtension(urlBase));
                return SaveBackground(backgroundImage);
            }
            catch (Exception ex)
            {
                Logger.LogError("Failed to get Bing downloaded image path.", ex);
                throw;
            }
        }

        private dynamic DownloadJson()
        {
            if (_jsonCache != null) return _jsonCache;
            using (var webClient = new WebClient())
            {
                try
                {
                    string jsonString = webClient.DownloadString("https://www.bing.com/HPImageArchive.aspx?format=js&idx=0&n=1&mkt=en-US");
                    _jsonCache = JsonConvert.DeserializeObject<dynamic>(jsonString);
                    return _jsonCache;
                }
                catch (Exception ex)
                {
                    Logger.LogError("Failed to download Bing JSON.", ex);
                    throw new Exception("Failed to download Bing JSON.", ex);
                }
            }
        }

        private string GetBackgroundUrlBase()
        {
            dynamic jsonObject = DownloadJson();
            return "https://www.bing.com" + jsonObject.images[0].urlbase;
        }

        private string GetBackgroundTitle()
        {
            dynamic jsonObject = DownloadJson();
            string copyrightText = jsonObject.images[0].copyright;
            int idx = copyrightText.IndexOf(" (");
            return idx > 0 ? copyrightText.Substring(0, idx) : copyrightText;
        }

        private bool WebsiteExists(string url)
        {
            try
            {
                var request = WebRequest.Create(url);
                request.Method = "HEAD";
                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    return response.StatusCode == HttpStatusCode.OK;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Website check failed for {url}", ex);
                return false;
            }
        }

        private string GetResolutionExtension(string url)
        {
            var resolution = Screen.PrimaryScreen.Bounds;
            string widthByHeight = $"{resolution.Width}x{resolution.Height}";
            string potentialExtension = $"_{widthByHeight}.jpg";
            if (WebsiteExists(url + potentialExtension))
            {
                return potentialExtension;
            }
            return "_1920x1080.jpg";
        }

        private Image DownloadBackground(string url)
        {
            try
            {
                var request = WebRequest.Create(url);
                using (var response = request.GetResponse())
                using (var stream = response.GetResponseStream())
                {
                    return Image.FromStream(stream);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to download background from {url}", ex);
                throw;
            }
        }

        public string GetBackgroundImagePath()
        {
            try
            {
                string directoryPath = GetSetting("ImgSaveLoc");
                if (string.IsNullOrEmpty(directoryPath))
                {
                    directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "Bing Backgrounds", DateTime.Now.Year.ToString());
                }
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }
                string fileName = GetBackgroundTitle();
                if (string.IsNullOrEmpty(fileName))
                {
                    fileName = DateTime.Now.ToString("M-d-yyyy");
                }
                else
                {
                    fileName = Regex.Replace(fileName, @"[^0-9a-zA-Z]+", "_");
                }
                fileName += ".jpg";
                return Path.Combine(directoryPath, fileName);
            }
            catch (Exception ex)
            {
                Logger.LogError("Failed to get Bing background image path.", ex);
                throw;
            }
        }

        private string SaveBackground(Image backgroundImage)
        {
            try
            {
                string imagePath = GetBackgroundImagePath();
                backgroundImage.Save(imagePath, System.Drawing.Imaging.ImageFormat.Bmp);
                return imagePath;
            }
            catch (Exception ex)
            {
                Logger.LogError("Failed to save Bing background image.", ex);
                throw;
            }
        }

        public string GetSetting(string key)
        {
            try
            {
                if (!File.Exists(SettingsFilePath))
                    return null;
                var json = File.ReadAllText(SettingsFilePath);
                var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                if (dict != null && dict.TryGetValue(key, out var value))
                    return value;
                return null;
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to get setting: {key}", ex);
                throw;
            }
        }

        public void SetSetting(string key, string value)
        {
            try
            {
                Dictionary<string, string> dict = new();
                if (File.Exists(SettingsFilePath))
                {
                    var json = File.ReadAllText(SettingsFilePath);
                    dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(json) ?? new();
                }
                dict[key] = value;
                File.WriteAllText(SettingsFilePath, JsonConvert.SerializeObject(dict, Formatting.Indented));
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to set setting: {key}", ex);
                throw;
            }
        }
    }
}