using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace DynamicBackground
{
    public class Picture
    {
        #region commented
        //public string DownloadimageAsync(string directoryPath, string fileName, Uri uri)
        //{
        //    try
        //    {
        //        await DownloadImageAsync(directoryPath, fileName, new Uri("kkk"));

        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;

        //    }
        //    return directoryPath + fileName;
        //}

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="directoryPath"></param>
        ///// <param name="fileName"></param>
        ///// <param name="uri"></param>
        ///// <returns></returns>
        //private async Task DownloadImageAsync(string directoryPath, string fileName, Uri uri)
        //{
        //    var httpClient = new HttpClient();

        //    // Get the file extension
        //    var uriWithoutQuery = uri.GetLeftPart(UriPartial.Path);
        //    var fileExtension = Path.GetExtension(uriWithoutQuery);

        //    // Create file path and ensure directory exists
        //    var path = Path.Combine(directoryPath, $"{fileName}{fileExtension}");
        //    Directory.CreateDirectory(directoryPath);

        //    // Download the image and write to the file
        //    var imageBytes = await httpClient.GetByteArrayAsync(uri);
        //    File.WriteAllBytes(path, imageBytes);
        //} 
        #endregion

        public string DownloadImage(string imageUrl)//, string directoryPath, string fileName)
        {
            BingBackground bingobj = new BingBackground();
            string imagePath = bingobj.GetBackgroundImagePath();
            System.Drawing.Image image = null;

            try
            {
                System.Net.HttpWebRequest webRequest = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(imageUrl);
                webRequest.AllowWriteStreamBuffering = true;
                webRequest.Timeout = 30000;
                webRequest.ServicePoint.ConnectionLeaseTimeout = 5000;
                webRequest.ServicePoint.MaxIdleTime = 5000;

                using (System.Net.WebResponse webResponse = webRequest.GetResponse())
                {
                    using (System.IO.Stream stream = webResponse.GetResponseStream())
                    {
                        image = System.Drawing.Image.FromStream(stream);
                    }
                }

                webRequest.ServicePoint.CloseConnectionGroup(webRequest.ConnectionGroupName);
                webRequest = null;
                //image.Save(directoryPath + fileName);
                image.Save(imagePath);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }          

            //return directoryPath+fileName;
            return imagePath;
        }
    }
}