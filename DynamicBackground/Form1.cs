using System;
using System.Diagnostics;
using System.Drawing;
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
            this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;
        }

        private void Browse_Click(object sender, EventArgs e)
        {
            Filepath.Text = GetFileName();
        }

        private void DynamicBackgroundUI_Load(object sender, EventArgs e)
        {
            Style.DataSource = Enum.GetValues(typeof(WallpaperStyle));
            checkBox1.Checked = true;
        }
        private void DynamicBackgroundUI_Resize(object sender, EventArgs e)
        {
            //if the form is minimized  
            //hide it from the task bar  
            //and show the system tray icon (represented by the NotifyIcon control)  
            if (this.WindowState == FormWindowState.Minimized)
            {
                Hide();
                string apppath = System.IO.Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
                notifyIcon.Icon= new Icon(apppath + "\\Icons\\TrayIcon.ico");
                notifyIcon.Visible = true;
            }
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
                //string folder = @"C:\Users\Pictures\Saved Pictures\";
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
            SetBingBackground();
        }

        private void SetBingBackground()
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

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if(checkBox1.Checked)
            {
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