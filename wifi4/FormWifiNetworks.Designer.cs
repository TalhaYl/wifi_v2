namespace wifi4
{
    partial class FormWifiNetworks
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormWifiNetworks));
            btnWifiScan = new Button();
            btnback = new Button();
            labelNetworkCount = new Label();
            listViewWifiNetworks = new ListView();
            ssıd = new ColumnHeader();
            signal = new ColumnHeader();
            security = new ColumnHeader();
            lblScan = new Label();
            lblHomePage = new Label();
            SuspendLayout();
            // 
            // btnWifiScan
            // 
            btnWifiScan.BackgroundImage = (Image)resources.GetObject("btnWifiScan.BackgroundImage");
            btnWifiScan.BackgroundImageLayout = ImageLayout.Center;
            btnWifiScan.FlatAppearance.BorderSize = 0;
            btnWifiScan.FlatStyle = FlatStyle.Flat;
            btnWifiScan.Location = new Point(22, 500);
            btnWifiScan.Name = "btnWifiScan";
            btnWifiScan.Size = new Size(73, 86);
            btnWifiScan.TabIndex = 0;
            btnWifiScan.UseVisualStyleBackColor = true;
            btnWifiScan.Click += btnScanWifi_Click;
            // 
            // btnback
            // 
            btnback.BackgroundImage = (Image)resources.GetObject("btnback.BackgroundImage");
            btnback.BackgroundImageLayout = ImageLayout.Center;
            btnback.FlatAppearance.BorderSize = 0;
            btnback.FlatStyle = FlatStyle.Flat;
            btnback.Location = new Point(260, 500);
            btnback.Name = "btnback";
            btnback.Size = new Size(73, 86);
            btnback.TabIndex = 1;
            btnback.UseVisualStyleBackColor = true;
            btnback.Click += btnBack_Click;
            // 
            // labelNetworkCount
            // 
            labelNetworkCount.AutoSize = true;
            labelNetworkCount.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point);
            labelNetworkCount.Location = new Point(22, 20);
            labelNetworkCount.Name = "labelNetworkCount";
            labelNetworkCount.Size = new Size(200, 28);
            labelNetworkCount.TabIndex = 3;
            labelNetworkCount.Text = "Toplam Ağ: 0";
            // 
            // listViewWifiNetworks
            // 
            listViewWifiNetworks.BackColor = Color.White;
            listViewWifiNetworks.Columns.AddRange(new ColumnHeader[] { ssıd, signal, security });
            listViewWifiNetworks.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            listViewWifiNetworks.FullRowSelect = true;
            listViewWifiNetworks.GridLines = true;
            listViewWifiNetworks.Location = new Point(22, 60);
            listViewWifiNetworks.Name = "listViewWifiNetworks";
            listViewWifiNetworks.Size = new Size(311, 434);
            listViewWifiNetworks.TabIndex = 4;
            listViewWifiNetworks.UseCompatibleStateImageBehavior = false;
            listViewWifiNetworks.View = View.Details;
            // 
            // ssıd
            // 
            ssıd.Text = "Ağ Adı";
            ssıd.Width = 150;
            // 
            // signal
            // 
            signal.Text = "Sinyal";
            signal.Width = 80;
            // 
            // security
            // 
            security.Text = "Güvenlik";
            security.Width = 80;
            // 
            // lblScan
            // 
            lblScan.AutoSize = true;
            lblScan.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            lblScan.Location = new Point(24, 589);
            lblScan.Name = "lblScan";
            lblScan.Size = new Size(71, 20);
            lblScan.TabIndex = 5;
            lblScan.Text = "Ağları Tara";
            // 
            // lblHomePage
            // 
            lblHomePage.AutoSize = true;
            lblHomePage.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            lblHomePage.Location = new Point(260, 589);
            lblHomePage.Name = "lblHomePage";
            lblHomePage.Size = new Size(86, 20);
            lblHomePage.TabIndex = 6;
            lblHomePage.Text = "Ana Sayfa";
            // 
            // FormWifiNetworks
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(240, 240, 240);
            ClientSize = new Size(355, 618);
            Controls.Add(lblHomePage);
            Controls.Add(lblScan);
            Controls.Add(listViewWifiNetworks);
            Controls.Add(labelNetworkCount);
            Controls.Add(btnback);
            Controls.Add(btnWifiScan);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "FormWifiNetworks";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "WiFi Ağları";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnWifiScan;
        private Button btnback;
        private Label labelNetworkCount;
        private ListView listViewWifiNetworks;
        private ColumnHeader ssıd;
        private ColumnHeader signal;
        private ColumnHeader security;
        private Label lblScan;
        private Label lblHomePage;
    }
}