using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace wifi4
{
    public partial class MainMenuForm : Form
    {
        private Color borderColor = Color.FromArgb(200, 200, 200);
        private Color hoverBorderColor = Color.FromArgb(0, 122, 204);

        public MainMenuForm()
        {
            InitializeComponent();
            SetupForm();
        }

        private void SetupForm()
        {
            // Form ayarları
            this.BackColor = Color.FromArgb(245, 246, 250);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "WiFi Yönetimi";
            this.Size = new Size(355, 618);

            // WiFi Scan butonu styling
            SetupButton(btnWifiScan, "📶", "WiFi Ağları", "Mevcut WiFi ağlarını tarayın ve bağlanın");
            btnWifiScan.Location = new Point(13, 150);

            // Connected Devices butonu styling
            SetupButton(btnConnectedDevices, "🖥️", "Bağlı Cihazlar", "Ağınıza bağlı cihazları görüntüleyin");
            btnConnectedDevices.Location = new Point(13, 250);

            // Label'ları gizle (artık buton üzerinde text var)
            if (labelWifi != null)
                labelWifi.Visible = false;
            if (labelDevices != null)
                labelDevices.Visible = false;
        }

        private void SetupButton(Button btn, string icon, string title, string description)
        {
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 2;
            btn.FlatAppearance.BorderColor = borderColor;
            btn.BackColor = Color.White;
            btn.Cursor = Cursors.Hand;
            btn.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
            btn.TextAlign = ContentAlignment.MiddleLeft;
            btn.ImageAlign = ContentAlignment.MiddleLeft;
            btn.Padding = new Padding(15, 10, 15, 10);

            // Buton boyutunu ayarla
            btn.Size = new Size(311, 80);

            // Custom paint event for better icon and text layout
            btn.Paint += (sender, e) => DrawCustomButton(sender as Button, e, icon, title, description);

            // Hover effects - sadece border değişimi
            btn.MouseEnter += (s, e) => {
                btn.FlatAppearance.BorderColor = hoverBorderColor;
                btn.Invalidate();
            };

            btn.MouseLeave += (s, e) => {
                btn.FlatAppearance.BorderColor = borderColor;
                btn.Invalidate();
            };
        }

        private void DrawCustomButton(Button btn, PaintEventArgs e, string icon, string title, string description)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            // Icon çizimi
            Font iconFont = new Font("Segoe UI Emoji", 20F, FontStyle.Regular);
            SizeF iconSize = g.MeasureString(icon, iconFont);
            float iconX = 20;
            float iconY = (btn.Height - iconSize.Height) / 2;

            using (SolidBrush iconBrush = new SolidBrush(Color.FromArgb(0, 122, 204)))
            {
                g.DrawString(icon, iconFont, iconBrush, iconX, iconY);
            }

            // Title çizimi
            Font titleFont = new Font("Segoe UI", 12F, FontStyle.Bold);
            float textX = iconX + iconSize.Width + 15;
            float titleY = 20;

            using (SolidBrush titleBrush = new SolidBrush(Color.FromArgb(51, 51, 51)))
            {
                g.DrawString(title, titleFont, titleBrush, textX, titleY);
            }

            // Description çizimi
            Font descFont = new Font("Segoe UI", 9F, FontStyle.Regular);
            float descY = titleY + 25;

            using (SolidBrush descBrush = new SolidBrush(Color.FromArgb(102, 102, 102)))
            {
                g.DrawString(description, descFont, descBrush, textX, descY);
            }

            iconFont.Dispose();
            titleFont.Dispose();
            descFont.Dispose();
        }

        // Form üzerine title ve subtitle ekleyelim
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            // Ana başlık
            string mainTitle = "WiFi Yönetim Merkezi";
            Font titleFont = new Font("Segoe UI", 16F, FontStyle.Bold);
            SizeF titleSize = g.MeasureString(mainTitle, titleFont);
            float titleX = (this.Width - titleSize.Width) / 2;
            float titleY = 50;

            using (SolidBrush titleBrush = new SolidBrush(Color.FromArgb(51, 51, 51)))
            {
                g.DrawString(mainTitle, titleFont, titleBrush, titleX, titleY);
            }

            // Alt başlık
            string subtitle = "Ağ bağlantılarınızı yönetin ve cihazlarınızı kontrol edin";
            Font subtitleFont = new Font("Segoe UI", 9F, FontStyle.Regular);
            SizeF subtitleSize = g.MeasureString(subtitle, subtitleFont);
            float subtitleX = (this.Width - subtitleSize.Width) / 2;
            float subtitleY = titleY + 30;

            using (SolidBrush subtitleBrush = new SolidBrush(Color.FromArgb(102, 102, 102)))
            {
                g.DrawString(subtitle, subtitleFont, subtitleBrush, subtitleX, subtitleY);
            }

            titleFont.Dispose();
            subtitleFont.Dispose();
        }

        private void btnWifiScan_Click(object sender, EventArgs e)
        {
            FormWifiNetworks wifiNetworksForm = new FormWifiNetworks();
            wifiNetworksForm.Show();
            this.Hide();
        }

        private void btnConnectedDevices_Click(object sender, EventArgs e)
        {
            ConnectedDevicesForm devicesForm = new ConnectedDevicesForm();
            devicesForm.Show();
            this.Hide();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (this is MainMenuForm)
            {
                Application.Exit();
            }
            else
            {
                base.OnFormClosing(e);
            }
        }
    }
}