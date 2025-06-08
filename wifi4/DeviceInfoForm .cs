using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;
using System.Linq;
using System.Threading.Tasks;

#nullable enable

namespace wifi4
{
    public partial class DeviceInfoForm : Form
    {
        private Dictionary<string, string> customDeviceNames = new Dictionary<string, string>();
        private string customNamesFilePath = "custom_device_names.txt";
        private string deviceMac;
        private string deviceName;
        private string deviceIp;
        private Panel? cardPanel;
        private Label? lblPortsValue;
        private Button? btnScanPorts;

        public DeviceInfoForm(string deviceName, string ip, string mac, string vendor, string hostname, string connectionType, string deviceType)
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string dir = Path.Combine(appData, "wifi4");
            Directory.CreateDirectory(dir);
            customNamesFilePath = Path.Combine(dir, "custom_device_names.txt");
            InitializeComponent();

            LoadCustomDeviceNames();

            this.deviceMac = mac;
            this.deviceName = deviceName;
            this.deviceIp = ip;
            this.Text = deviceName + " - Detaylar";
            this.BackColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Size = new Size(355, 618);
            this.MinimumSize = new Size(355, 618);

            string normMac = deviceMac.Replace(":", "-").ToUpper();
            if (customDeviceNames.TryGetValue(normMac, out var savedName))
            {
                this.deviceName = savedName;
            }
            CreateVCardPanel(deviceName, ip, mac, vendor, hostname, connectionType, deviceType);
            if (cardPanel == null)
            {
                throw new InvalidOperationException("Card panel was not initialized.");
            }
        }

        private void CreateVCardPanel(string deviceName, string ip, string mac, string vendor, string hostname, string connectionType, string deviceType)
        {
            int formWidth = this.ClientSize.Width;
            int formHeight = this.ClientSize.Height;
            int cardMargin = 20;
            int cardWidth = formWidth - 2 * cardMargin;
            int cardHeight = formHeight - 40;

            cardPanel = new Panel
            {
                Size = new Size(cardWidth, cardHeight),
                Location = new Point(cardMargin, 20),
                BackColor = Color.White,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };
            this.Controls.Add(cardPanel);

            cardPanel.Paint += (sender, e) =>
            {
                Rectangle rect = new Rectangle(0, 0, cardPanel.Width, cardPanel.Height);
                using (GraphicsPath path = CreateRoundedRectangle(rect, 10))
                {
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                    for (int i = 1; i <= 5; i++)
                    {
                        using (Pen shadowPen = new Pen(Color.FromArgb(20, 0, 0, 0), 1))
                        {
                            e.Graphics.DrawPath(shadowPen, path);
                        }
                    }

                    using (SolidBrush brush = new SolidBrush(Color.White))
                    {
                        e.Graphics.FillPath(brush, path);
                    }

                    using (Pen pen = new Pen(Color.FromArgb(200, 200, 200), 1))
                    {
                        e.Graphics.DrawPath(pen, path);
                    }
                }
            };

            Panel headerPanel = new Panel
            {
                Size = new Size(cardWidth, 100),
                Location = new Point(0, 0),
                BackColor = Color.FromArgb(0, 123, 255)
            };
            headerPanel.Paint += (sender, e) =>
            {
                Rectangle rect = new Rectangle(0, 0, headerPanel.Width, headerPanel.Height);
                using (GraphicsPath path = CreateRoundedRectanglePath(rect, 10, true, true, false, false))
                {
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                    using (LinearGradientBrush brush = new LinearGradientBrush(
                        rect,
                        Color.FromArgb(0, 123, 255),
                        Color.FromArgb(0, 80, 200),
                        LinearGradientMode.ForwardDiagonal))
                    {
                        e.Graphics.FillPath(brush, path);
                    }
                }
            };
            cardPanel.Controls.Add(headerPanel);

            // Avatar Panel
            Panel avatarPanel = new Panel
            {
                Size = new Size(70, 70),
                Location = new Point(15, 15),
                BackColor = Color.Transparent
            };
            avatarPanel.Paint += (sender, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                Rectangle rect = new Rectangle(0, 0, avatarPanel.Width, avatarPanel.Height);
                using (GraphicsPath path = CreateRoundedRectangle(rect, 8))
                {
                    e.Graphics.FillPath(Brushes.White, path);
                    using (Font iconFont = new Font("Segoe UI Symbol", 30))
                    {
                        string iconChar = "📱";
                        if (deviceType.Contains("Router") || deviceType.Contains("Modem"))
                            iconChar = "📶";
                        else if (deviceType.Contains("PC") || deviceType.Contains("Bilgisayar"))
                            iconChar = "💻";
                        else if (deviceType.Contains("TV") || deviceType.Contains("Television"))
                            iconChar = "📺";
                        else if (deviceType.Contains("Printer") || deviceType.Contains("Yazıcı"))
                            iconChar = "🖨️";

                        SizeF size = e.Graphics.MeasureString(iconChar, iconFont);
                        e.Graphics.DrawString(iconChar, iconFont, Brushes.DodgerBlue,
                            (avatarPanel.Width - size.Width) / 2,
                            (avatarPanel.Height - size.Height) / 2);
                    }
                }
            };
            headerPanel.Controls.Add(avatarPanel);

            // Device Name Panel
            Panel deviceNamePanel = new Panel
            {
                Size = new Size(cardWidth - 140, 30),
                Location = new Point(95, 20),
                BackColor = Color.Transparent
            };
            deviceNamePanel.Paint += (sender, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                Rectangle rect = new Rectangle(0, 0, deviceNamePanel.Width, deviceNamePanel.Height);
                using (GraphicsPath path = CreateRoundedRectangle(rect, 5))
                {
                    e.Graphics.FillPath(Brushes.White, path);
                }
            };
            headerPanel.Controls.Add(deviceNamePanel);

            Label lblDeviceName = new Label
            {
                Text = this.deviceName,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                AutoSize = false,
                Size = deviceNamePanel.Size,
                Location = new Point(0, 0),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent,
                Name = "lblName"
            };
            deviceNamePanel.Controls.Add(lblDeviceName);

            // MAC Address Panel
            Panel macPanel = new Panel
            {
                Size = new Size(cardWidth - 140, 25),
                Location = new Point(95, 55),
                BackColor = Color.Transparent
            };
            macPanel.Paint += (sender, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                Rectangle rect = new Rectangle(0, 0, macPanel.Width, macPanel.Height);
                using (GraphicsPath path = CreateRoundedRectangle(rect, 5))
                {
                    e.Graphics.FillPath(Brushes.White, path);
                }
            };
            headerPanel.Controls.Add(macPanel);

            Label lblMacSubtitle = new Label
            {
                Text = mac,
                ForeColor = Color.FromArgb(80, 80, 80),
                Font = new Font("Segoe UI", 9),
                AutoSize = false,
                Size = macPanel.Size,
                Location = new Point(0, 0),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };
            macPanel.Controls.Add(lblMacSubtitle);

            Panel contentPanel = new Panel
            {
                Size = new Size(cardWidth, cardHeight - 180),
                Location = new Point(0, 100),
                BackColor = Color.White,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };
            cardPanel.Controls.Add(contentPanel);

            CreateInfoRow(contentPanel, "📡", "IP Adresi", ip, 20);
            CreateInfoRow(contentPanel, "🏭", "Üretici", vendor, 55);
            CreateInfoRow(contentPanel, "📛", "Hostname", hostname, 90);
            CreateInfoRow(contentPanel, "🌐", "Bağlantı", connectionType, 125);
            CreateInfoRow(contentPanel, "📱", "Cihaz Türü", deviceType, 160);

            // Açık Portlar butonu ve sonuç label'ı üstte OLMAYACAK, bunun yerine alt satırda olacak
            // Alt butonlar ve port tarama butonu için ortak değişkenler
            int buttonWidth = 90;
            int buttonHeight = 36;
            int buttonSpacing = 20;
            int totalButtonWidth = buttonWidth * 3 + buttonSpacing * 2;
            int startX = (cardWidth - totalButtonWidth) / 2;
            int buttonY = cardPanel.Height - buttonHeight - 20;

            // Ping info row
            Label lblPingValue = null;
            int pingRowY = 195;
            {
                Label lblIcon = new Label
                {
                    Text = "📶",
                    Font = new Font("Segoe UI Symbol", 14),
                    ForeColor = Color.FromArgb(0, 123, 255),
                    AutoSize = true,
                    Location = new Point(20, pingRowY)
                };
                contentPanel.Controls.Add(lblIcon);

                Label lblLabel = new Label
                {
                    Text = "Ping:",
                    Font = new Font("Segoe UI", 9, FontStyle.Bold),
                    ForeColor = Color.FromArgb(100, 100, 100),
                    AutoSize = true,
                    Location = new Point(50, pingRowY + 3)
                };
                contentPanel.Controls.Add(lblLabel);

                lblPingValue = new Label
                {
                    Text = "-",
                    Font = new Font("Segoe UI", 9),
                    ForeColor = Color.FromArgb(60, 60, 60),
                    AutoSize = true,
                    Location = new Point(155, pingRowY + 3)
                };
                contentPanel.Controls.Add(lblPingValue);
            }

            // Açık Portlar satırı (ping satırının hemen altında)
            int portsRowY = pingRowY + 35;
            Label lblPortsValueLocal = new Label
            {
                Text = "-",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(60, 60, 60),
                AutoSize = true,
                Location = new Point(155, portsRowY + 3)
            };
            // Icon
            Label lblPortsIcon = new Label
            {
                Text = "🛡️",
                Font = new Font("Segoe UI Symbol", 14),
                ForeColor = Color.FromArgb(0, 123, 255),
                AutoSize = true,
                Location = new Point(20, portsRowY)
            };
            contentPanel.Controls.Add(lblPortsIcon);
            // Label
            Label lblPortsLabel = new Label
            {
                Text = "Açık Portlar:",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.FromArgb(100, 100, 100),
                AutoSize = true,
                Location = new Point(50, portsRowY + 3)
            };
            contentPanel.Controls.Add(lblPortsLabel);
            contentPanel.Controls.Add(lblPortsValueLocal);
            lblPortsValue = lblPortsValueLocal;

            // Alt butonlar (Ping, İsim Değiştir, Açık Portlar)
            var btnPing = new Button
            {
                Text = "Ping",
                Size = new Size(buttonWidth, buttonHeight),
                Location = new Point(startX, buttonY),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                BackColor = Color.FromArgb(0, 123, 255),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnPing.FlatAppearance.BorderSize = 0;
            cardPanel.Controls.Add(btnPing);

            var btnRename = new Button
            {
                Text = "İsim Değiştir",
                Size = new Size(buttonWidth, buttonHeight),
                Location = new Point(startX + buttonWidth + buttonSpacing, buttonY),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 123, 255),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnRename.FlatAppearance.BorderSize = 0;
            cardPanel.Controls.Add(btnRename);

            var btnScanPorts = new Button
            {
                Text = "Açık Portlar",
                Size = new Size(buttonWidth, buttonHeight),
                Location = new Point(startX + 2 * (buttonWidth + buttonSpacing), buttonY),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 123, 255),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnScanPorts.FlatAppearance.BorderSize = 0;
            cardPanel.Controls.Add(btnScanPorts);
            this.btnScanPorts = btnScanPorts;

            btnRename.Click += BtnRename_Click;

            btnPing.Click += async (s, e) =>
            {
                lblPingValue.Text = "Ping atılıyor...";
                await Task.Delay(400);
                try
                {
                    using (var ping = new System.Net.NetworkInformation.Ping())
                    {
                        var reply = await ping.SendPingAsync(ip, 1000);
                        if (reply.Status == System.Net.NetworkInformation.IPStatus.Success)
                        {
                            lblPingValue.Text = $"Başarılı: {reply.RoundtripTime} ms";
                        }
                        else
                        {
                            lblPingValue.Text = "Başarısız";
                        }
                    }
                }
                catch
                {
                    lblPingValue.Text = "Başarısız";
                }
            };

            btnScanPorts.Click += async (s, e) =>
            {
                var result = MessageBox.Show(
                    "Port tarama yöntemini seçin:\n\n" +
                    "Evet = En yaygın 30 portu tara\n" +
                    "Hayır = Özel port aralığı belirle",
                    "Port Tarama",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    btnScanPorts.Enabled = false;
                    if (lblPortsValue != null)
                        lblPortsValue.Text = "Taranıyor...";
                    try
                    {
                        var openPorts = await ScanCommonPortsAsync(ip);
                        if (lblPortsValue != null)
                        {
                            if (openPorts.Count == 0)
                                lblPortsValue.Text = "Açık port bulunamadı.";
                            else
                                lblPortsValue.Text = string.Join(", ", openPorts);
                        }
                    }
                    catch (Exception ex)
                    {
                        if (lblPortsValue != null)
                            lblPortsValue.Text = "Hata: " + ex.Message;
                    }
                    finally
                    {
                        btnScanPorts.Enabled = true;
                    }
                }
                else if (result == DialogResult.No)
                {
                    ShowPortRangeDialog(ip);
                }
            };
        }

        private T? FindControlRecursive<T>(Control parent, string name) where T : Control
        {
            foreach (Control ctrl in parent.Controls)
            {
                if (ctrl is T t && ctrl.Name == name)
                    return t;
                var child = FindControlRecursive<T>(ctrl, name);
                if (child != null)
                    return child;
            }
            return null;
        }

        private void CreateInfoRow(Panel parent, string icon, string label, string value, int yPosition)
        {
            Label lblIcon = new Label
            {
                Text = icon,
                Font = new Font("Segoe UI Symbol", 14),
                ForeColor = Color.FromArgb(0, 123, 255),
                AutoSize = true,
                Location = new Point(20, yPosition)
            };
            parent.Controls.Add(lblIcon);

            Label lblLabel = new Label
            {
                Text = label + ":",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.FromArgb(100, 100, 100),
                AutoSize = true,
                Location = new Point(50, yPosition + 3)
            };
            parent.Controls.Add(lblLabel);

            Label lblValue = new Label
            {
                Text = value,
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(60, 60, 60),
                AutoSize = true,
                Location = new Point(155, yPosition + 3)
            };
            parent.Controls.Add(lblValue);
        }

        private GraphicsPath CreateRoundedRectangle(Rectangle rect, int radius)
        {
            return CreateRoundedRectanglePath(rect, radius, true, true, true, true);
        }

        private GraphicsPath CreateRoundedRectanglePath(Rectangle rect, int radius, bool topLeft, bool topRight, bool bottomRight, bool bottomLeft)
        {
            GraphicsPath path = new GraphicsPath();
            int diameter = radius * 2;

            if (topLeft)
            {
                path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            }
            else
            {
                path.AddLine(rect.X, rect.Y, rect.X, rect.Y);
            }

            if (topRight)
            {
                path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            }
            else
            {
                path.AddLine(rect.Right, rect.Y, rect.Right, rect.Y);
            }

            if (bottomRight)
            {
                path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            }
            else
            {
                path.AddLine(rect.Right, rect.Bottom, rect.Right, rect.Bottom);
            }

            if (bottomLeft)
            {
                path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
            }
            else
            {
                path.AddLine(rect.X, rect.Bottom, rect.X, rect.Bottom);
            }

            path.CloseFigure();
            return path;
        }

        private void BtnRename_Click(object? sender, EventArgs e)
        {
            var connectedDevicesForm = Application.OpenForms.OfType<ConnectedDevicesForm>().FirstOrDefault();
            if (connectedDevicesForm != null)
            {
                connectedDevicesForm.ShowDeviceEditDialog(deviceMac);
                this.Close(); // Close the device info form after opening rename dialog
            }
            else
            {
                MessageBox.Show("Cihaz listesi formu bulunamadı.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void RenameDevice(string mac, string currentName)
        {
            var inputBox = new TextBox
            {
                Text = currentName,
                Font = new Font("Segoe UI", 10),
                Width = 300,
                Margin = new Padding(5),
                Location = new Point(20, 20)
            };

            var dialog = new Form
            {
                Width = 360,
                Height = 150,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent,
                Text = "Cihaz İsmi Değiştir"
            };

            var btnSave = new Button
            {
                Text = "Kaydet",
                Width = 100,
                Height = 40,
                Margin = new Padding(5),
                Location = new Point(130, 60),
                DialogResult = DialogResult.OK
            };

            dialog.Controls.Add(inputBox);
            dialog.Controls.Add(btnSave);
            dialog.AcceptButton = btnSave;

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string newName = inputBox.Text.Trim();
                if (!string.IsNullOrWhiteSpace(newName) && newName != currentName)
                {
                    string normMac = mac.Replace(":", "-").ToUpper();
                    customDeviceNames[normMac] = newName;
                    SaveCustomDeviceNames();
                    if (Owner is ConnectedDevicesForm mainForm)
                    {
                        mainForm.LoadCustomDeviceNames();
                        mainForm.ShowFilteredDevices();
                    }
                    deviceName = newName;

                    var lblName = FindControlRecursive<Label>(this, "lblName");
                    if (lblName != null)
                    {
                        lblName.Text = this.deviceName;  // Başında "Cihaz İsmi:" yok, sadece isim
                    }

                    MessageBox.Show("Cihaz adı başarıyla güncellendi!");
                }
            }
        }

        private void SaveCustomDeviceNames()
        {
            var lines = customDeviceNames.Select(kvp => $"{kvp.Key}|{kvp.Value}");
            File.WriteAllLines(customNamesFilePath, lines);
        }

        private void LoadCustomDeviceNames()
        {
            customDeviceNames.Clear();
            if (File.Exists(customNamesFilePath))
            {
                foreach (var line in File.ReadAllLines(customNamesFilePath))
                {
                    var parts = line.Split('|');
                    if (parts.Length == 2)
                        customDeviceNames[parts[0]] = parts[1];
                }
            }
        }

        // Port tarama fonksiyonu
        private async Task<List<int>> ScanCommonPortsAsync(string ip)
        {
            // En yaygın 30 port
            int[] commonPorts = {
                20, 21, 22, 23, 25, 53, 80, 110, 139, 143, 443, 445, 3389, 8080,
                3306, 5432, 27017, 1433, 1521, 6379, 9200, 9300, 11211, 2181,
                5000, 5001, 8443, 8888, 9000, 9090
            };
            return await ScanPortsAsync(ip, commonPorts);
        }

        private async Task<List<int>> ScanPortRangeAsync(string ip, int startPort, int endPort)
        {
            var ports = Enumerable.Range(startPort, endPort - startPort + 1).ToArray();
            return await ScanPortsAsync(ip, ports);
        }

        private async Task<List<int>> ScanPortsAsync(string ip, int[] ports)
        {
            var openPorts = new List<int>();
            var tasks = new List<Task>();

            foreach (int port in ports)
            {
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        using (var client = new System.Net.Sockets.TcpClient())
                        {
                            var connectTask = client.ConnectAsync(ip, port);
                            var timeoutTask = Task.Delay(400);
                            var completed = await Task.WhenAny(connectTask, timeoutTask);
                            if (completed == connectTask && client.Connected)
                            {
                                lock (openPorts)
                                    openPorts.Add(port);
                            }
                        }
                    }
                    catch { }
                }));
            }

            await Task.WhenAll(tasks);
            openPorts.Sort();
            return openPorts;
        }

        private void ShowPortRangeDialog(string ip)
        {
            var dialog = new Form
            {
                Width = 360,
                Height = 200,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent,
                Text = "Port Aralığı Tarama"
            };

            var lblStartPort = new Label
            {
                Text = "Başlangıç Portu:",
                Location = new Point(20, 20),
                AutoSize = true
            };

            var txtStartPort = new TextBox
            {
                Location = new Point(120, 17),
                Width = 200
            };

            var lblEndPort = new Label
            {
                Text = "Bitiş Portu:",
                Location = new Point(20, 50),
                AutoSize = true
            };

            var txtEndPort = new TextBox
            {
                Location = new Point(120, 47),
                Width = 200
            };

            var btnScan = new Button
            {
                Text = "Taramayı Başlat",
                Width = 120,
                Height = 40,
                Location = new Point(120, 90),
                DialogResult = DialogResult.OK
            };

            dialog.Controls.AddRange(new Control[] { lblStartPort, txtStartPort, lblEndPort, txtEndPort, btnScan });
            dialog.AcceptButton = btnScan;

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                if (int.TryParse(txtStartPort.Text, out int startPort) && 
                    int.TryParse(txtEndPort.Text, out int endPort))
                {
                    if (startPort > 0 && endPort <= 65535 && startPort <= endPort)
                    {
                        if (btnScanPorts != null)
                            btnScanPorts.Enabled = false;
                        if (lblPortsValue != null)
                            lblPortsValue.Text = "Taranıyor...";

                        Task.Run(async () =>
                        {
                            try
                            {
                                var openPorts = await ScanPortRangeAsync(ip, startPort, endPort);
                                this.Invoke((MethodInvoker)delegate
                                {
                                    if (lblPortsValue != null)
                                    {
                                        if (openPorts.Count == 0)
                                            lblPortsValue.Text = "Açık port bulunamadı.";
                                        else
                                            lblPortsValue.Text = string.Join(", ", openPorts);
                                    }
                                    if (btnScanPorts != null)
                                        btnScanPorts.Enabled = true;
                                });
                            }
                            catch (Exception ex)
                            {
                                this.Invoke((MethodInvoker)delegate
                                {
                                    if (lblPortsValue != null)
                                        lblPortsValue.Text = "Hata: " + ex.Message;
                                    if (btnScanPorts != null)
                                        btnScanPorts.Enabled = true;
                                });
                            }
                        });
                    }
                    else
                    {
                        MessageBox.Show("Geçersiz port aralığı! Portlar 1-65535 arasında olmalı ve başlangıç portu bitiş portundan küçük olmalıdır.",
                            "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Lütfen geçerli port numaraları girin!",
                        "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
