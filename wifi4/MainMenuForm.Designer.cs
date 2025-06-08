namespace wifi4
{
    partial class MainMenuForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainMenuForm));
            btnWifiScan = new Button();
            btnConnectedDevices = new Button();
            labelWifi = new Label();
            labelDevices = new Label();
            SuspendLayout();
            // 
            // btnWifiScan
            // 
            btnWifiScan.BackColor = Color.FromArgb(240, 240, 240);
            btnWifiScan.FlatAppearance.BorderSize = 0;
            btnWifiScan.FlatStyle = FlatStyle.Flat;
            btnWifiScan.Location = new Point(22, 70);
            btnWifiScan.Name = "btnWifiScan";
            btnWifiScan.Size = new Size(311, 60);
            btnWifiScan.TabIndex = 0;
            btnWifiScan.UseVisualStyleBackColor = true;
            btnWifiScan.Click += btnWifiScan_Click;
            // 
            // btnConnectedDevices
            // 
            btnConnectedDevices.BackColor = Color.FromArgb(240, 240, 240);
            btnConnectedDevices.FlatAppearance.BorderSize = 0;
            btnConnectedDevices.FlatStyle = FlatStyle.Flat;
            btnConnectedDevices.Location = new Point(22, 150);
            btnConnectedDevices.Name = "btnConnectedDevices";
            btnConnectedDevices.Size = new Size(311, 60);
            btnConnectedDevices.TabIndex = 1;
            btnConnectedDevices.UseVisualStyleBackColor = true;
            btnConnectedDevices.Click += btnConnectedDevices_Click;
            // 
            // labelWifi
            // 
            labelWifi.AutoSize = true;
            labelWifi.Font = new Font("Segoe UI", 9F);
            labelWifi.Location = new Point(22, 47);
            labelWifi.Name = "labelWifi";
            labelWifi.Size = new Size(82, 20);
            labelWifi.TabIndex = 2;
            labelWifi.Text = "WiFi Ağları";
            // 
            // labelDevices
            // 
            labelDevices.AutoSize = true;
            labelDevices.Font = new Font("Segoe UI", 9F);
            labelDevices.Location = new Point(22, 127);
            labelDevices.Name = "labelDevices";
            labelDevices.Size = new Size(100, 20);
            labelDevices.TabIndex = 3;
            labelDevices.Text = "Bağlı Cihazlar";
            // 
            // MainMenuForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(240, 240, 240);
            ClientSize = new Size(355, 400);
            Controls.Add(labelDevices);
            Controls.Add(labelWifi);
            Controls.Add(btnConnectedDevices);
            Controls.Add(btnWifiScan);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "MainMenuForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "WiFi Yönetimi";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnWifiScan;
        private Button btnConnectedDevices;
        private Label labelWifi;
        private Label labelDevices;
    }
}