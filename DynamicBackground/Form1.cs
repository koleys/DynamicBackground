using System;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace DynamicBackground
{
    public partial class DynamicBackgroundUI : Form
    {
        private Picture _picture = new Picture();
        BingBackground bingobj = new BingBackground();

        public DynamicBackgroundUI()
        {
            InitializeComponent();
        }

        private void Browse_Click(object sender, EventArgs e)
        {
            Filepath.Text = GetFileName();
        }

        private void DynamicBackgroundUI_Load(object sender, EventArgs e)
        {
            Style.DataSource = Enum.GetValues(typeof(WallpaperStyle));
        }

        private void Filepath_TextChanged(object sender, EventArgs e)
        {
            //if (string.IsNullOrEmpty(Filepath.Text))
            //{
            //    Set.Enabled = false;
            //}
            //else
            //{
            //    Set.Enabled = true;
            //}
        }

        private string GetFileName()
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();
            string sep = string.Empty;

            foreach (var c in codecs)
            {
                string codecName = c.CodecName.Substring(8).Replace("Codec", "Files").Trim();
                openFileDialog1.Filter = String.Format("{0}{1}{2} ({3})|{3}", openFileDialog1.Filter, sep, codecName, c.FilenameExtension);
                sep = "|";
            }
            openFileDialog1.Filter = String.Format("{0}{1}{2} ({3})|{3}", openFileDialog1.Filter, sep, "All Files", "*.*");

            openFileDialog1.DefaultExt = ".png"; // Default file extension

            DialogResult result = openFileDialog1.ShowDialog(); // Show the dialog.
            if (result == DialogResult.OK) // Test result.
            {
                return openFileDialog1.FileName;
            }
            else
            {
                return "";
            }
        }

        private void Set_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(Filepath.Text))
            {
                MessageBox.Show("please select a file");
                return;
            }
            
            if (Uri.IsWellFormedUriString(Filepath.Text, UriKind.RelativeOrAbsolute))
            {
                //string folder = @"C:\Users\skoley\Pictures\Saved Pictures\";
                //string filename = DateTime.Now.ToString("dd_MM_yyyy_hh_mm_ss") + ".png";
                try
                {
                    string savedFilePath = _picture.DownloadImage(Filepath.Text);//,folder,filename);
                    //string savedFilePath = _picture.DownloadImage(Filepath.Text,folder,filename);
                    if (!string.IsNullOrEmpty(savedFilePath))
                    {
                        WallpaperStyle _style = (WallpaperStyle)Style.SelectedItem;
                        Wallpaper.SilentSet(savedFilePath, _style);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                WallpaperStyle _style = (WallpaperStyle)Style.SelectedItem;
                Wallpaper.SilentSet(Filepath.Text, _style);
            }
        }

        private void setBingImage_Click(object sender, EventArgs e)
        {
            
            try
            {
                string savedFilePath = bingobj.GetDownloadedImagePath();
                //string savedFilePath = BingBackground.GetDownloadedImagePath();
                if (!string.IsNullOrEmpty(savedFilePath))
                {
                    WallpaperStyle _style = (WallpaperStyle)Style.SelectedItem;
                    Wallpaper.SilentSet(savedFilePath, _style);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void downloadLoc_Click(object sender, EventArgs e)
        {
            string folderpath = Browsefolder();
            if(!string.IsNullOrEmpty(folderpath))               
            {
                bingobj.SetSetting("ImgSaveLoc", folderpath);
            }
        }

        private string Browsefolder()
        {
            string folderPath="";
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                folderPath = folderBrowserDialog.SelectedPath;
            }
            return folderPath;
        }
    }
}