using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace DynamicBackground
{
    //https://github.com/josueespinosa/BingBackground/blob/master/BingBackground/BingBackground/App.config
    public class BingBackground
    {
        public string GetDownloadedImagePath()
        {
            string urlBase = GetBackgroundUrlBase();
            Image background = DownloadBackground(urlBase + GetResolutionExtension(urlBase));
            return SaveBackground(background);
            //SetBackground(GetPosition());
        }

        private dynamic DownloadJson()
        {
            using (WebClient webClient = new WebClient())
            {
                Console.WriteLine("Downloading JSON...");
                try
                {
                    string jsonString = webClient.DownloadString("https://www.bing.com/HPImageArchive.aspx?format=js&idx=0&n=1&mkt=en-US");
                    return JsonConvert.DeserializeObject<dynamic>(jsonString);
                }
                catch (Exception ex)
                {

                    throw ex;
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
            return copyrightText.Substring(0, copyrightText.IndexOf(" ("));
        }

        private bool WebsiteExists(string url)
        {
            try
            {
                WebRequest request = WebRequest.Create(url);
                request.Method = "HEAD";
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                return response.StatusCode == HttpStatusCode.OK;
            }
            catch
            {
                return false;
            }
        }

        private string GetResolutionExtension(string url)
        {
            Rectangle resolution = Screen.PrimaryScreen.Bounds;
            string widthByHeight = resolution.Width + "x" + resolution.Height;
            string potentialExtension = "_" + widthByHeight + ".jpg";
            if (WebsiteExists(url + potentialExtension))
            {
                Console.WriteLine("Background for " + widthByHeight + " found.");
                return potentialExtension;
            }
            else
            {
                Console.WriteLine("No background for " + widthByHeight + " was found.");
                Console.WriteLine("Using 1920x1080 instead.");
                return "_1920x1080.jpg";
            }
        }        

        private Image DownloadBackground(string url)
        {
            Console.WriteLine("Downloading background...");
            //SetProxy();
            WebRequest request = WebRequest.Create(url);
            WebResponse reponse = request.GetResponse();
            Stream stream = reponse.GetResponseStream();
            return Image.FromStream(stream);
        }

        public string GetBackgroundImagePath()
        {
            string directoryPath;
            directoryPath = GetSetting("ImgSaveLoc");
            if(string.IsNullOrEmpty(directoryPath))
            {
                directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "Bing Backgrounds", DateTime.Now.Year.ToString());
            }            
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath); 
            }
            string filename = GetBackgroundTitle();
            if(string.IsNullOrEmpty(filename))
            {
                filename = DateTime.Now.ToString("M-d-yyyy") ;
            }
            else
            {
               filename= Regex.Replace(filename, @"[^0-9a-zA-Z]+", "_");
            }
            filename += ".jpg";
            return Path.Combine(directoryPath, filename);
        }

        private string SaveBackground(Image background)
        {
            Console.WriteLine("Saving background...");
            string imagepath = GetBackgroundImagePath();
            background.Save(imagepath, System.Drawing.Imaging.ImageFormat.Bmp);
            return imagepath;
        }
        private  string GetSetting(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }

        public  void SetSetting(string key, string value)
        {
            Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            configuration.AppSettings.Settings[key].Value = value;
            configuration.Save(ConfigurationSaveMode.Full, true);
            ConfigurationManager.RefreshSection("appSettings");
        }
        #region commented
        //private static void SetProxy()
        //{
        //    string proxyUrl = Properties.Settings.Default.Proxy;
        //    if (proxyUrl.Length > 0)
        //    {
        //        var webProxy = new WebProxy(proxyUrl, true);
        //        webProxy.Credentials = CredentialCache.DefaultCredentials;
        //        WebRequest.DefaultWebProxy = webProxy;
        //    }
        //}

        //private enum PicturePosition
        //{
        //    Tile,
        //    Center,
        //    Stretch,
        //    Fit,
        //    Fill
        //}

        //private static PicturePosition GetPosition()
        //{
        //    PicturePosition position = PicturePosition.Fit;
        //    switch (Properties.Settings.Default.Position)
        //    {
        //        case "Tile":
        //            position = PicturePosition.Tile;
        //            break;
        //        case "Center":
        //            position = PicturePosition.Center;
        //            break;
        //        case "Stretch":
        //            position = PicturePosition.Stretch;
        //            break;
        //        case "Fit":
        //            position = PicturePosition.Fit;
        //            break;
        //        case "Fill":
        //            position = PicturePosition.Fill;
        //            break;
        //    }
        //    return position;
        //}

        //internal sealed class NativeMethods
        //{
        //    [DllImport("user32.dll", CharSet = CharSet.Auto)]
        //    internal static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);
        //}

        //private static void SetBackground(PicturePosition style)
        //{
        //    Console.WriteLine("Setting background...");
        //    using (RegistryKey key = Registry.CurrentUser.OpenSubKey(Path.Combine("Control Panel", "Desktop"), true))
        //    {
        //        switch (style)
        //        {
        //            case PicturePosition.Tile:
        //                key.SetValue("PicturePosition", "0");
        //                key.SetValue("TileWallpaper", "1");
        //                break;
        //            case PicturePosition.Center:
        //                key.SetValue("PicturePosition", "0");
        //                key.SetValue("TileWallpaper", "0");
        //                break;
        //            case PicturePosition.Stretch:
        //                key.SetValue("PicturePosition", "2");
        //                key.SetValue("TileWallpaper", "0");
        //                break;
        //            case PicturePosition.Fit:
        //                key.SetValue("PicturePosition", "6");
        //                key.SetValue("TileWallpaper", "0");
        //                break;
        //            case PicturePosition.Fill:
        //                key.SetValue("PicturePosition", "10");
        //                key.SetValue("TileWallpaper", "0");
        //                break;
        //        }
        //    }
        //    const int SetDesktopBackground = 20;
        //    const int UpdateIniFile = 1;
        //    const int SendWindowsIniChange = 2;
        //    NativeMethods.SystemParametersInfo(SetDesktopBackground, 0, GetBackgroundImagePath(), UpdateIniFile | SendWindowsIniChange);
        //} 
        #endregion

    }

}