using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using DynamicBackground.Services;
using DynamicBackground.Services.Abstractions;
using DynamicBackground.ViewModels;
using DynamicBackground.Infrastructure;
using DynamicBackground.Services.Logging;

namespace DynamicBackground
{
    public partial class DynamicBackgroundUI : Form
    {
        private Picture _picture;
        private BingBackground bingobj;
        private readonly IServiceProvider _serviceProvider;
        private MainWindowViewModel? _viewModel;
        private readonly ISettingsService _settingsService;
        private readonly ILogger _logger;
        
        public DynamicBackgroundUI(IServiceProvider serviceProvider = null)
        {
            InitializeComponent();
            this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;

            // Support both DI and legacy initialization
            _serviceProvider = serviceProvider;
            _settingsService = new SettingsService("settings.json");
            _logger = new DualModeLogger("DynamicBackground.log");

            if (_serviceProvider != null)
            {
                // Initialize ViewModel from DI
                var backgroundService = _serviceProvider.GetService(typeof(IBackgroundService)) as IBackgroundService;
                var wallpaperService = _serviceProvider.GetService(typeof(IWallpaperService)) as IWallpaperService;
                var settingsService = _serviceProvider.GetService(typeof(ISettingsService)) as ISettingsService;
                var imageDownloader = _serviceProvider.GetService(typeof(IImageDownloader)) as IImageDownloader;
                var logger = _serviceProvider.GetService(typeof(ILogger)) as ILogger;

                if (backgroundService != null && wallpaperService != null &&
                    settingsService != null && imageDownloader != null && logger != null)
                {
                    _viewModel = new MainWindowViewModel(backgroundService, wallpaperService,
                        settingsService, imageDownloader, logger);
                }
            }

            // Fallback to legacy initialization for backward compatibility
            _picture = new Picture();
            bingobj = new BingBackground();
        }

        private void Browse_Click(object sender, EventArgs e)
        {
            if (_viewModel != null)
            {
                Filepath.Text = _viewModel.BrowseFile();
            }
            else
            {
                Filepath.Text = GetFileName();
            }
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
            if (_viewModel != null)
            {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                _viewModel.SetWallpaperAsync(Filepath.Text);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                if (!string.IsNullOrEmpty(_viewModel.LastError))
                {
                    MessageBox.Show(_viewModel.LastError, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                if (string.IsNullOrEmpty(Filepath.Text))
                {
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
                        _logger.LogError("Failed to set wallpaper from URL", ex);
                    }
                }
                else
                {
                    WallpaperStyle _style = (WallpaperStyle)Style.SelectedItem;
                    Wallpaper.SilentSet(Filepath.Text, _style);
                }
            }
        }

        private void setBingImage_Click(object sender, EventArgs e)
        {
            SetBingBackground();
        }

        private async void SetBingBackground()
        {
            if (_viewModel != null)
            {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                _viewModel.DownloadAndSetBingWallpaperAsync();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                if (!string.IsNullOrEmpty(_viewModel.LastError))
                {
                    MessageBox.Show(_viewModel.LastError, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                try
                {
                    // Get startup delay setting
                    int startupDelay = _settingsService.GetSettingAsInt(AppConstants.SETTINGS_KEY_STARTUP_DELAY, AppConstants.DEFAULT_STARTUP_DELAY_SECONDS);

                    // Apply startup delay if enabled
                    if (startupDelay > 0)
                    {
                        await Task.Delay(startupDelay * 1000);
                    }

                    string savedFilePath = bingobj.GetDownloadedImagePath();
                    if (!string.IsNullOrEmpty(savedFilePath))
                    {
                        WallpaperStyle _style = (WallpaperStyle)Style.SelectedItem;
                        Wallpaper.SilentSet(savedFilePath, _style);
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError("Failed to download and set Bing wallpaper", ex);
                }
            }
        }

        private void downloadLoc_Click(object sender, EventArgs e)
        {
            string folderpath;
            
            if (_viewModel != null)
            {
                folderpath = _viewModel.BrowseFolder();
                if (!string.IsNullOrEmpty(folderpath))
                {
                    _viewModel.SetImageSaveLocation(folderpath);
                }
            }
            else
            {
                folderpath = Browsefolder();
                if(!string.IsNullOrEmpty(folderpath))               
                {
                    bingobj.SetSetting("ImgSaveLoc", folderpath);
                }
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
                int interval_val = int.Parse(bingobj.GetSetting("Interval") ?? "720");
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
            
            if (_viewModel != null)
            {
                _viewModel.SetUpdateInterval((int)interval.Value);
            }
            else
            {
                bingobj.SetSetting("Interval", ((int)interval.Value).ToString());
            }
        }
    }
}