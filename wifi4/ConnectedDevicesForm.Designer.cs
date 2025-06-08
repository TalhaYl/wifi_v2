namespace wifi4
{
    partial class ConnectedDevicesForm
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
            btnScan = new Button();
            flowDevices = new FlowLayoutPanel();
            labelCount = new Label();
            btnBack = new Button();
            lblDevicesScan = new Label();
            lblHomePage = new Label();
            loadingSpinner = new PictureBox();
            countSpinner = new PictureBox();
            ((System.ComponentModel.ISupportInitialize)loadingSpinner).BeginInit();
            ((System.ComponentModel.ISupportInitialize)countSpinner).BeginInit();
            SuspendLayout();
            // 
            // btnScan
            // 
            btnScan.BackgroundImageLayout = ImageLayout.Center;
            btnScan.FlatAppearance.BorderSize = 0;
            btnScan.FlatStyle = FlatStyle.Flat;
            btnScan.Location = new Point(12, 530);
            btnScan.Name = "btnScan";
            btnScan.Size = new Size(92, 76);
            btnScan.TabIndex = 0;
            btnScan.UseVisualStyleBackColor = true;
            btnScan.Click += btnScan_Click;
            // 
            // flowDevices
            // 
            flowDevices.AutoScroll = true;
            flowDevices.BackColor = Color.FromArgb(240, 240, 240);
            flowDevices.Location = new Point(12, 58);
            flowDevices.Name = "flowDevices";
            flowDevices.Padding = new Padding(10);
            flowDevices.Size = new Size(337, 466);
            flowDevices.TabIndex = 1;
            // 
            // labelCount
            // 
            labelCount.AutoSize = true;
            labelCount.Font = new Font("Segoe UI", 12F);
            labelCount.Location = new Point(12, 18);
            labelCount.Name = "labelCount";
            labelCount.Size = new Size(207, 28);
            labelCount.TabIndex = 2;
            labelCount.Text = "Toplam Cihaz Sayısı : 0";
            // 
            // btnBack
            // 
            btnBack.BackgroundImageLayout = ImageLayout.Center;
            btnBack.FlatAppearance.BorderSize = 0;
            btnBack.FlatStyle = FlatStyle.Flat;
            btnBack.Location = new Point(207, 530);
            btnBack.Name = "btnBack";
            btnBack.Size = new Size(92, 76);
            btnBack.TabIndex = 3;
            btnBack.UseVisualStyleBackColor = true;
            btnBack.Click += btnBack_Click;
            // 
            // lblDevicesScan
            // 
            lblDevicesScan.AutoSize = true;
            lblDevicesScan.Font = new Font("Segoe UI", 9F);
            lblDevicesScan.Location = new Point(7, 609);
            lblDevicesScan.Name = "lblDevicesScan";
            lblDevicesScan.Size = new Size(97, 20);
            lblDevicesScan.TabIndex = 4;
            lblDevicesScan.Text = "Cihazları Tara";
            // 
            // lblHomePage
            // 
            lblHomePage.AutoSize = true;
            lblHomePage.Font = new Font("Segoe UI", 9F);
            lblHomePage.Location = new Point(224, 609);
            lblHomePage.Name = "lblHomePage";
            lblHomePage.Size = new Size(75, 20);
            lblHomePage.TabIndex = 5;
            lblHomePage.Text = "Ana Sayfa";
            // 
            // loadingSpinner
            // 
            loadingSpinner.BackColor = Color.Transparent;
            loadingSpinner.Location = new Point(20, 380);
            loadingSpinner.Name = "loadingSpinner";
            loadingSpinner.Size = new Size(40, 40);
            loadingSpinner.SizeMode = PictureBoxSizeMode.Zoom;
            loadingSpinner.TabIndex = 6;
            loadingSpinner.TabStop = false;
            // 
            // countSpinner
            // 
            countSpinner.BackColor = Color.Transparent;
            countSpinner.Location = new Point(224, -7);
            countSpinner.Name = "countSpinner";
            countSpinner.Size = new Size(74, 59);
            countSpinner.SizeMode = PictureBoxSizeMode.Zoom;
            countSpinner.TabIndex = 7;
            countSpinner.TabStop = false;
            // 
            // ConnectedDevicesForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(240, 240, 240);
            ClientSize = new Size(361, 638);
            Controls.Add(lblHomePage);
            Controls.Add(lblDevicesScan);
            Controls.Add(btnScan);
            Controls.Add(labelCount);
            Controls.Add(btnBack);
            Controls.Add(flowDevices);
            Controls.Add(loadingSpinner);
            Controls.Add(countSpinner);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "ConnectedDevicesForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Bağlı Cihazlar";
            ((System.ComponentModel.ISupportInitialize)loadingSpinner).EndInit();
            ((System.ComponentModel.ISupportInitialize)countSpinner).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnScan;
        private FlowLayoutPanel flowDevices;
        private Label labelCount;
        private Button btnBack;
        private Label lblDevicesScan;
        private Label lblHomePage;
        private System.Windows.Forms.PictureBox loadingSpinner;
        private System.Windows.Forms.PictureBox countSpinner;
    }
}