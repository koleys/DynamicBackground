using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using DynamicBackground.Services.Abstractions;

namespace DynamicBackground
{
    public partial class DynamicBackgroundUI : Form
    {
        private Picture _picture;
        private BingBackground bingobj;
        private readonly IServiceProvider _serviceProvider;
        private IBackgroundService _backgroundService;
        private IWallpaperService _wallpaperService;
        private ISettingsService _settingsService;
        private IImageDownloader _imageDownloader;

        public DynamicBackgroundUI(IServiceProvider serviceProvider = null)
        {
            InitializeComponent();
            this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;

            // Support both DI and legacy initialization
            _serviceProvider = serviceProvider;
            if (_serviceProvider != null)
            {
                // Initialize from DI
                _backgroundService = _serviceProvider.GetService(typeof(IBackgroundService)) as IBackgroundService;
                _wallpaperService = _serviceProvider.GetService(typeof(IWallpaperService)) as IWallpaperService;
                _settingsService = _serviceProvider.GetService(typeof(ISettingsService)) as ISettingsService;
                _imageDownloader = _serviceProvider.GetService(typeof(IImageDownloader)) as IImageDownloader;
            }

            // Fallback to legacy initialization for backward compatibility
            _picture = new Picture();
            bingobj = new BingBackground();
        }

        private void Browse_Click(object sender, EventArgs e)
        {
            Filepath.Text = GetFileName();
        }

        private void DynamicBackgroundUI_Load(object sender, EventArgs e)
        {
            Style.DataSource = Enum.GetValues(typeof(WallpaperStyle));
            checkBox1.Checked = true; // Ensure auto change from Bing is checked by default
            Set.Enabled = !string.IsNullOrWhiteSpace(Filepath.Text); // Set button state on load
        }

        private void DynamicBackgroundUI_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                Hide();
                string apppath = System.IO.Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
                notifyIcon.Icon = new Icon(apppath + "\\Icons\\TrayIcon.ico");
                notifyIcon.Visible = true;
            }
        }
        
        private void Filepath_TextChanged(object sender, EventArgs e)
        {
            Set.Enabled = !string.IsNullOrWhiteSpace(Filepath.Text);
        }

        private string GetFileName()
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();
            string sep = string.Empty;
            openFileDialog1.Filter = @"All Files (*.*)|*.*";
            foreach (var c in codecs)
            {
                string codecName = c.CodecName.Substring(8).Replace("Codec", "Files").Trim();
                sep = "|";
                openFileDialog1.Filter = String.Format("{0}{1}{2} ({3})|{3}", openFileDialog1.Filter, sep, codecName, c.FilenameExtension);
            }
            openFileDialog1.Filter = String.Format("{0}{1}{2} ({3})|{3}", openFileDialog1.Filter, sep, "All Files", "*.*");
            openFileDialog1.DefaultExt = ".png";

            DialogResult result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK)
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
                try
                {
                    string savedFilePath = _picture.DownloadImage(Filepath.Text);
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
            SetBingBackground();
        }

        private void SetBingBackground()
        {
            try
            {
                string savedFilePath = bingobj.GetDownloadedImagePath();
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

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if(checkBox1.Checked)
            {
                SetBingBackground();
                int interval_val = Convert.ToInt32(bingobj.GetSetting("Interval"));
                timer1.Interval = Convert.ToInt32(interval_val) * 60000;
                timer1.Start();
            }
            else
            {
                timer1.Stop();
                timer1.Dispose();
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            SetBingBackground();
        }

        private void notifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Show();
            this.WindowState = FormWindowState.Normal;
            notifyIcon.Visible = false;
        }

        private void btnSetInterval_Click(object sender, EventArgs e)
        {
            if (interval.Value <= 0)
            {
                interval.Value = 30;
            }
            bingobj.SetSetting("Interval", interval.Value.ToString());
        }
    }
}