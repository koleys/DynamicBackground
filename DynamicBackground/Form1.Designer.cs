
namespace DynamicBackground
{
    partial class DynamicBackgroundUI
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DynamicBackgroundUI));
            this.Set = new System.Windows.Forms.Button();
            this.Browse = new System.Windows.Forms.Button();
            this.Filepath = new System.Windows.Forms.TextBox();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.Style = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.setBingImage = new System.Windows.Forms.Button();
            this.downloadLoc = new System.Windows.Forms.Button();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.interval = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.btnSetInterval = new System.Windows.Forms.Button();
            this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.interval)).BeginInit();
            this.SuspendLayout();
            // 
            // Set
            // 
            this.Set.Location = new System.Drawing.Point(404, 40);
            this.Set.Name = "Set";
            this.Set.Size = new System.Drawing.Size(85, 23);
            this.Set.TabIndex = 0;
            this.Set.Text = "Set Wallpaper";
            this.Set.UseVisualStyleBackColor = true;
            this.Set.Click += new System.EventHandler(this.Set_Click);
            // 
            // Browse
            // 
            this.Browse.Location = new System.Drawing.Point(404, 74);
            this.Browse.Name = "Browse";
            this.Browse.Size = new System.Drawing.Size(85, 23);
            this.Browse.TabIndex = 1;
            this.Browse.Text = "Browse";
            this.Browse.UseVisualStyleBackColor = true;
            this.Browse.Click += new System.EventHandler(this.Browse_Click);
            // 
            // Filepath
            // 
            this.Filepath.Location = new System.Drawing.Point(104, 74);
            this.Filepath.Name = "Filepath";
            this.Filepath.Size = new System.Drawing.Size(294, 20);
            this.Filepath.TabIndex = 2;
            this.Filepath.TextChanged += new System.EventHandler(this.Filepath_TextChanged);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // Style
            // 
            this.Style.FormattingEnabled = true;
            this.Style.Location = new System.Drawing.Point(104, 40);
            this.Style.Name = "Style";
            this.Style.Size = new System.Drawing.Size(159, 21);
            this.Style.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 79);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(89, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "IMG Path or URL";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 48);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(81, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Wallpaper Style";
            // 
            // setBingImage
            // 
            this.setBingImage.Location = new System.Drawing.Point(404, 12);
            this.setBingImage.Name = "setBingImage";
            this.setBingImage.Size = new System.Drawing.Size(85, 23);
            this.setBingImage.TabIndex = 6;
            this.setBingImage.Text = "Set From Bing";
            this.setBingImage.UseVisualStyleBackColor = true;
            this.setBingImage.Click += new System.EventHandler(this.setBingImage_Click);
            // 
            // downloadLoc
            // 
            this.downloadLoc.Location = new System.Drawing.Point(269, 40);
            this.downloadLoc.Name = "downloadLoc";
            this.downloadLoc.Size = new System.Drawing.Size(129, 23);
            this.downloadLoc.TabIndex = 7;
            this.downloadLoc.Text = "Set Download Loc";
            this.downloadLoc.UseVisualStyleBackColor = true;
            this.downloadLoc.Click += new System.EventHandler(this.downloadLoc_Click);
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(12, 21);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(134, 17);
            this.checkBox1.TabIndex = 8;
            this.checkBox1.Text = "Auto change from Bing";
            this.checkBox1.UseVisualStyleBackColor = true;
            this.checkBox1.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // interval
            // 
            this.interval.Increment = new decimal(new int[] {
            30,
            0,
            0,
            0});
            this.interval.Location = new System.Drawing.Point(245, 14);
            this.interval.Maximum = new decimal(new int[] {
            500,
            0,
            0,
            0});
            this.interval.Minimum = new decimal(new int[] {
            30,
            0,
            0,
            0});
            this.interval.Name = "interval";
            this.interval.Size = new System.Drawing.Size(55, 20);
            this.interval.TabIndex = 9;
            this.interval.Value = new decimal(new int[] {
            180,
            0,
            0,
            0});
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(162, 21);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(77, 13);
            this.label3.TabIndex = 10;
            this.label3.Text = "Interval in mins";
            // 
            // btnSetInterval
            // 
            this.btnSetInterval.Location = new System.Drawing.Point(313, 12);
            this.btnSetInterval.Name = "btnSetInterval";
            this.btnSetInterval.Size = new System.Drawing.Size(85, 23);
            this.btnSetInterval.TabIndex = 11;
            this.btnSetInterval.Text = "Set Interval";
            this.btnSetInterval.UseVisualStyleBackColor = true;
            this.btnSetInterval.Click += new System.EventHandler(this.btnSetInterval_Click);
            // 
            // notifyIcon
            // 
            this.notifyIcon.Text = "Dynamic Background";
            this.notifyIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.notifyIcon_MouseDoubleClick);
            // 
            // DynamicBackgroundUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(498, 104);
            this.Controls.Add(this.btnSetInterval);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.interval);
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.downloadLoc);
            this.Controls.Add(this.setBingImage);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.Style);
            this.Controls.Add(this.Filepath);
            this.Controls.Add(this.Browse);
            this.Controls.Add(this.Set);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "DynamicBackgroundUI";
            this.Text = "Dynamic Background UI";
            this.Load += new System.EventHandler(this.DynamicBackgroundUI_Load);
            this.Resize += new System.EventHandler(this.DynamicBackgroundUI_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.interval)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button Set;
        private System.Windows.Forms.Button Browse;
        private System.Windows.Forms.TextBox Filepath;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.ComboBox Style;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button setBingImage;
        private System.Windows.Forms.Button downloadLoc;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.NumericUpDown interval;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnSetInterval;
        private System.Windows.Forms.NotifyIcon notifyIcon;
    }
}

