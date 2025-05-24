
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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DynamicBackgroundUI));
            Set = new Button();
            Browse = new Button();
            Filepath = new TextBox();
            openFileDialog1 = new OpenFileDialog();
            Style = new ComboBox();
            label1 = new Label();
            label2 = new Label();
            setBingImage = new Button();
            downloadLoc = new Button();
            checkBox1 = new CheckBox();
            timer1 = new System.Windows.Forms.Timer(components);
            interval = new NumericUpDown();
            label3 = new Label();
            btnSetInterval = new Button();
            notifyIcon = new NotifyIcon(components);
            ((System.ComponentModel.ISupportInitialize)interval).BeginInit();
            SuspendLayout();
            // 
            // Set
            // 
            Set.Location = new Point(471, 82);
            Set.Margin = new Padding(4, 3, 4, 3);
            Set.Name = "Set";
            Set.Size = new Size(99, 27);
            Set.TabIndex = 0;
            Set.Text = "Set Wallpaper";
            Set.UseVisualStyleBackColor = true;
            Set.Click += Set_Click;
            // 
            // Browse
            // 
            Browse.Location = new Point(364, 82);
            Browse.Margin = new Padding(4, 3, 4, 3);
            Browse.Name = "Browse";
            Browse.Size = new Size(99, 27);
            Browse.TabIndex = 1;
            Browse.Text = "Browse";
            Browse.UseVisualStyleBackColor = true;
            Browse.Click += Browse_Click;
            // 
            // Filepath
            // 
            Filepath.Location = new Point(121, 85);
            Filepath.Margin = new Padding(4, 3, 4, 3);
            Filepath.Name = "Filepath";
            Filepath.Size = new Size(235, 23);
            Filepath.TabIndex = 2;
            Filepath.TextChanged += Filepath_TextChanged;
            // 
            // openFileDialog1
            // 
            openFileDialog1.FileName = "openFileDialog1";
            // 
            // Style
            // 
            Style.FormattingEnabled = true;
            Style.Location = new Point(121, 51);
            Style.Margin = new Padding(4, 3, 4, 3);
            Style.Name = "Style";
            Style.Size = new Size(236, 23);
            Style.TabIndex = 3;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(10, 91);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new Size(94, 15);
            label1.TabIndex = 4;
            label1.Text = "IMG Path or URL";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(10, 55);
            label2.Margin = new Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new Size(88, 15);
            label2.TabIndex = 5;
            label2.Text = "Wallpaper Style";
            // 
            // setBingImage
            // 
            setBingImage.Location = new Point(471, 14);
            setBingImage.Margin = new Padding(4, 3, 4, 3);
            setBingImage.Name = "setBingImage";
            setBingImage.Size = new Size(99, 27);
            setBingImage.TabIndex = 6;
            setBingImage.Text = "Set From Bing";
            setBingImage.UseVisualStyleBackColor = true;
            setBingImage.Click += setBingImage_Click;
            // 
            // downloadLoc
            // 
            downloadLoc.Location = new Point(365, 47);
            downloadLoc.Margin = new Padding(4, 3, 4, 3);
            downloadLoc.Name = "downloadLoc";
            downloadLoc.Size = new Size(205, 27);
            downloadLoc.TabIndex = 7;
            downloadLoc.Text = "Set Image Download Location";
            downloadLoc.UseVisualStyleBackColor = true;
            downloadLoc.Click += downloadLoc_Click;
            // 
            // checkBox1
            // 
            checkBox1.AutoSize = true;
            checkBox1.Location = new Point(13, 2);
            checkBox1.Margin = new Padding(4, 3, 4, 3);
            checkBox1.Name = "checkBox1";
            checkBox1.Size = new Size(150, 19);
            checkBox1.TabIndex = 8;
            checkBox1.Text = "Auto change from Bing";
            checkBox1.UseVisualStyleBackColor = true;
            checkBox1.CheckedChanged += checkBox1_CheckedChanged;
            // 
            // timer1
            // 
            timer1.Tick += timer1_Tick;
            // 
            // interval
            // 
            interval.Increment = new decimal(new int[] { 30, 0, 0, 0 });
            interval.Location = new Point(312, 22);
            interval.Margin = new Padding(4, 3, 4, 3);
            interval.Maximum = new decimal(new int[] { 500, 0, 0, 0 });
            interval.Minimum = new decimal(new int[] { 30, 0, 0, 0 });
            interval.Name = "interval";
            interval.Size = new Size(44, 23);
            interval.TabIndex = 9;
            interval.Value = new decimal(new int[] { 180, 0, 0, 0 });
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(138, 24);
            label3.Margin = new Padding(4, 0, 4, 0);
            label3.Name = "label3";
            label3.Size = new Size(166, 15);
            label3.TabIndex = 10;
            label3.Text = "Image change Interval in mins";
            // 
            // btnSetInterval
            // 
            btnSetInterval.Location = new Point(365, 14);
            btnSetInterval.Margin = new Padding(4, 3, 4, 3);
            btnSetInterval.Name = "btnSetInterval";
            btnSetInterval.Size = new Size(99, 27);
            btnSetInterval.TabIndex = 11;
            btnSetInterval.Text = "Set Interval";
            btnSetInterval.UseVisualStyleBackColor = true;
            btnSetInterval.Click += btnSetInterval_Click;
            // 
            // notifyIcon
            // 
            notifyIcon.Text = "Dynamic Background";
            notifyIcon.MouseDoubleClick += notifyIcon_MouseDoubleClick;
            // 
            // DynamicBackgroundUI
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(577, 120);
            Controls.Add(btnSetInterval);
            Controls.Add(label3);
            Controls.Add(interval);
            Controls.Add(checkBox1);
            Controls.Add(downloadLoc);
            Controls.Add(setBingImage);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(Style);
            Controls.Add(Filepath);
            Controls.Add(Browse);
            Controls.Add(Set);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(4, 3, 4, 3);
            MaximizeBox = false;
            Name = "DynamicBackgroundUI";
            Text = "Dynamic Background UI";
            Load += DynamicBackgroundUI_Load;
            Resize += DynamicBackgroundUI_Resize;
            ((System.ComponentModel.ISupportInitialize)interval).EndInit();
            ResumeLayout(false);
            PerformLayout();

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

