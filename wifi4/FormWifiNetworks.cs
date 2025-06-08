using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace wifi4
{
    public partial class FormWifiNetworks : Form
    {
        private Color primaryColor = Color.FromArgb(0, 122, 204);
        private Color secondaryColor = Color.FromArgb(240, 240, 240);
        private Color accentColor = Color.FromArgb(0, 122, 204);
        private Font titleFont = new Font("Segoe UI", 12F, FontStyle.Bold);
        private Font normalFont = new Font("Segoe UI", 9F);
        private Font smallFont = new Font("Segoe UI", 9F);

        public FormWifiNetworks()
        {
            InitializeComponent();
            SetupForm();
            SetHoverBorder(btnWifiScan);
            SetHoverBorder(btnback);

        }

        private void SetupForm()
        {
            // Form ayarları
            this.BackColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "WiFi Ağları";
            this.Size = new Size(355, 618);
            this.Padding = new Padding(10);

            // ListView ayarları
            listViewWifiNetworks.View = View.Details;
            listViewWifiNetworks.FullRowSelect = true;
            listViewWifiNetworks.GridLines = false;
            listViewWifiNetworks.BackColor = Color.White;
            listViewWifiNetworks.Font = normalFont;
            listViewWifiNetworks.BorderStyle = BorderStyle.None;
            listViewWifiNetworks.Location = new Point(22, 60);
            listViewWifiNetworks.Size = new Size(311, 434);
            listViewWifiNetworks.Columns[0].Width = 150;
            listViewWifiNetworks.Columns[1].Width = 80;
            listViewWifiNetworks.Columns[2].Width = 80;

            // Özel ListView çizimi
            listViewWifiNetworks.DrawItem += (s, e) => {
                e.DrawDefault = true;
                if (e.Item != null)
                {
                    e.Item.BackColor = e.ItemIndex % 2 == 0 ? Color.White : Color.FromArgb(250, 250, 250);
                }
            };

            // Label ayarları
            labelNetworkCount.Font = titleFont;
            labelNetworkCount.ForeColor = primaryColor;
            labelNetworkCount.Location = new Point(22, 20);
            labelNetworkCount.Text = "Toplam Ağ: 0";

            lblScan.Font = smallFont;
            lblScan.ForeColor = Color.FromArgb(100, 100, 100);
            lblScan.Location = new Point(26, 480);
            lblScan.Text = "Ağları Tara";

            lblHomePage.Font = smallFont;
            lblHomePage.ForeColor = Color.FromArgb(100, 100, 100);
            lblHomePage.Location = new Point(265, 480);
            lblHomePage.Text = "Ana Sayfa";

            // Buton ayarları
            btnWifiScan.BackColor = Color.White;
            btnWifiScan.ForeColor = Color.White;
            btnWifiScan.FlatStyle = FlatStyle.Flat;
            btnWifiScan.FlatAppearance.BorderSize = 0;
            btnWifiScan.Font = normalFont;
            btnWifiScan.Location = new Point(22, 500);
            btnWifiScan.Size = new Size(70, 70);
            btnWifiScan.Cursor = Cursors.Hand;

            btnback.BackColor = Color.White;
            btnback.ForeColor = Color.White;
            btnback.FlatStyle = FlatStyle.Flat;
            btnback.FlatAppearance.BorderSize = 0;
            btnback.Font = normalFont;
            btnback.Location = new Point(260, 500);
            btnback.Size = new Size(70, 70);
            btnback.Cursor = Cursors.Hand;
           }
        private void SetHoverBorder(Button button)
        {
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.BorderColor = this.BackColor;

            button.MouseEnter += (s, e) =>
            {
                button.FlatAppearance.BorderSize = 2;
                button.FlatAppearance.BorderColor = Color.SteelBlue; // İstediğin rengi kullanabilirsin
            };

            button.MouseLeave += (s, e) =>
            {
                button.FlatAppearance.BorderSize = 0;
                button.FlatAppearance.BorderColor = this.BackColor;
            };
        }

        private void btnScanWifi_Click(object sender, EventArgs e)
        {
            listViewWifiNetworks.Items.Clear();
            labelNetworkCount.Text = "WiFi taraması yapılıyor...";

            try
            {
                string output = RunCommand("netsh", "wlan show networks mode=bssid");
                string[] lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                List<string> networks = new List<string>();
                string ssid = "", signal = "", security = "";

                foreach (var raw in lines)
                {
                    string line = raw.Trim();
                    if (line.StartsWith("SSID"))
                    {
                        if (!string.IsNullOrEmpty(ssid))
                        {
                            networks.Add($"{ssid} - {signal} - {security}");
                        }

                        ssid = line.Split(new[] { ':' }, 2)[1].Trim();
                        signal = "";
                        security = "";
                    }
                    else if (line.StartsWith("Signal"))
                    {
                        signal = line.Split(':')[1].Trim();
                    }
                    else if (line.StartsWith("Authentication"))
                    {
                        security = line.Split(':')[1].Trim();
                    }
                }

                if (!string.IsNullOrEmpty(ssid))
                    networks.Add($"{ssid} - {signal} - {security}");

                foreach (var net in networks.Distinct())
                {
                    var parts = net.Split('-');
                    ListViewItem item = new ListViewItem(parts[0].Trim());
                    item.SubItems.Add(parts[1].Trim());
                    item.SubItems.Add(parts[2].Trim());
                    listViewWifiNetworks.Items.Add(item);
                }

                labelNetworkCount.Text = $"Toplam Ağ: {networks.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Tarama hatası: " + ex.Message);
            }
        }

        private string RunCommand(string cmd, string args)
        {
            var psi = new ProcessStartInfo(cmd, args)
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using (var process = Process.Start(psi))
            {
                return process.StandardOutput.ReadToEnd();
            }
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            foreach (Form form in Application.OpenForms)
            {
                if (form is MainMenuForm)
                {
                    form.Show();
                    break;
                }
            }
            this.Close();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                foreach (Form form in Application.OpenForms)
                {
                    if (form is MainMenuForm)
                    {
                        form.Show();
                        break;
                    }
                }
            }
            base.OnFormClosing(e);
        }
    }
}