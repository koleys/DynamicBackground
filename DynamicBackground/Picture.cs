using System;
using System.Drawing;
using System.IO;

namespace DynamicBackground
{
    public class Picture
    {
        public string DownloadImage(string imageUrl)
        {
            var bingBackground = new BingBackground();
            string imagePath = bingBackground.GetBackgroundImagePath();
            try
            {
                var webRequest = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(imageUrl);
                webRequest.AllowWriteStreamBuffering = true;
                webRequest.Timeout = 30000;
                webRequest.ServicePoint.ConnectionLeaseTimeout = 5000;
                webRequest.ServicePoint.MaxIdleTime = 5000;

                using (var webResponse = webRequest.GetResponse())
                using (var stream = webResponse.GetResponseStream())
                using (var downloadedImage = Image.FromStream(stream))
                {
                    downloadedImage.Save(imagePath);
                }
                webRequest.ServicePoint.CloseConnectionGroup(webRequest.ConnectionGroupName);
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to download image from {imageUrl}", ex);
                throw new Exception($"Failed to download image: {ex.Message}", ex);
            }
            return imagePath;
        }
    }
}