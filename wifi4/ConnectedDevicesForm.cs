using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text;

namespace wifi4
{
    public class DeviceInfo
    {
        public string CompanyName { get; set; } = "Bilinmeyen Üretici";
        public string CompanyAddress { get; set; } = "";
        public string CountryCode { get; set; } = "";
        public string BlockSize { get; set; } = "";
        public string AssignmentBlockSize { get; set; } = "";
        public string DateCreated { get; set; } = "";
        public string DateUpdated { get; set; } = "";
        public List<string> Applications { get; set; } = new List<string>();
        public string TransmissionType { get; set; } = "";
        public string AdministrationType { get; set; } = "";
        public string WiresharkNotes { get; set; } = "";
        public string Comment { get; set; } = "";
        public DateTime LastUpdated { get; set; } = DateTime.Now;
    }

    public partial class ConnectedDevicesForm : Form
    {
        #region Fields and Properties
        private Dictionary<string, string> customDeviceNames = new Dictionary<string, string>();
        private string customNamesFilePath;
        private int deviceCount = 0;
        private CheckBox chkLocalOnly;
        private List<(string ip, string mac, string hostname, string vendor, string connectionType, string deviceType)> allDevices = new();
        private string mac = "";
        private SemaphoreSlim pingSemaphore = new SemaphoreSlim(20, 20); // Aynı anda max 20 ping
        private Label lblCurrentNetwork; // Mevcut WiFi ağını göstermek için
        private string currentSSID = ""; // Mevcut WiFi SSID'si
        private EnhancedMacLookup macLookup; // Yeni MAC lookup sınıfı
        private Dictionary<string, string> macVendorCache = new Dictionary<string, string>();
        private string macVendorCacheFile = "mac_vendors.txt";
        private bool isScanning = false;
        private CancellationTokenSource scanCancellationSource;
        #endregion

        #region Constructor and Initialization
        public ConnectedDevicesForm()
        {
            InitializeComponent();

            // Buton özelliklerini ayarla
            btnScan.FlatStyle = FlatStyle.Flat;
            btnScan.FlatAppearance.BorderSize = 0;
            btnScan.Size = new Size(50, 50);
            btnScan.BackColor = Color.Transparent;
            btnScan.BackgroundImageLayout = ImageLayout.Center;

            btnBack.FlatStyle = FlatStyle.Flat;
            btnBack.FlatAppearance.BorderSize = 0;
            btnBack.Size = new Size(50, 50);
            btnBack.BackColor = Color.Transparent;
            btnBack.BackgroundImageLayout = ImageLayout.Center;

            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string dir = Path.Combine(appData, "wifi4");
            Directory.CreateDirectory(dir);
            customNamesFilePath = Path.Combine(dir, "custom_device_names.txt");

            LoadCustomDeviceNames();
            LoadMacVendorCache();
            SetHoverBorder(btnScan);
            SetHoverBorder(btnBack);

            // Buton resimlerini yükle
            LoadResources();

            // MAC lookup sınıfını başlat
            macLookup = new EnhancedMacLookup();

            // WiFi ağ bilgisi için yeni label
            lblCurrentNetwork = new Label
            {
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 122, 204),
                AutoSize = true,
                Location = new Point(12, 15),
                Text = "Bağlı WiFi: Yükleniyor..."
            };
            this.Controls.Add(lblCurrentNetwork);

            // Yerel ağ filtresi için checkbox
            chkLocalOnly = new CheckBox
            {
                Text = "Sadece Yerel Ağ",
                Location = new Point(12, lblCurrentNetwork.Bottom + 5),
                AutoSize = true,
                Font = new Font("Segoe UI", 10),
                Checked = false
            };
            chkLocalOnly.CheckedChanged += (s, e) => ShowFilteredDevices();
            this.Controls.Add(chkLocalOnly);

            // Diğer kontrollerin konumlarını ayarla
            flowDevices.Location = new Point(12, chkLocalOnly.Bottom + 10);

            // WiFi ağ bilgisini güncelle
            UpdateCurrentNetworkInfo();

            try
            {
                // Loading spinner ayarları
                loadingSpinner.Image = Image.FromFile(Path.Combine(Application.StartupPath, "Resources", "g2.gif"));
                loadingSpinner.SizeMode = PictureBoxSizeMode.Zoom;
                loadingSpinner.Visible = true;
                loadingSpinner.Size = new Size(100, 100);

                // Count spinner ayarları
                countSpinner.Image = Image.FromFile(Path.Combine(Application.StartupPath, "Resources", "g2.gif"));
                countSpinner.SizeMode = PictureBoxSizeMode.Zoom;
                countSpinner.Visible = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Loading gif yüklenirken hata oluştu: " + ex.Message);
            }

            // Başlangıçta spinner'ları gizle
            HideLoadingSpinner();
            HideCountSpinner();
        }

        private void ShowLoadingSpinner()
        {
            if (loadingSpinner != null)
            {
                loadingSpinner.Visible = true;
                loadingSpinner.Location = new Point((this.Width - loadingSpinner.Width) / 2, (this.Height - loadingSpinner.Height) / 2);
                loadingSpinner.BringToFront();
            }
        }

        private void HideLoadingSpinner()
        {
            if (loadingSpinner != null)
            {
                loadingSpinner.Visible = false;
            }
        }

        private void ShowCountSpinner()
        {
            if (countSpinner != null)
            {
                countSpinner.Visible = true;
                countSpinner.Location = new Point(labelCount.Right + 10, labelCount.Top + 2);
                countSpinner.BringToFront();
            }
        }

        private void HideCountSpinner()
        {
            if (countSpinner != null)
            {
                countSpinner.Visible = false;
            }
        }
        private void SetHoverBorder(Button button)
        {
            button.MouseEnter += (s, e) =>
            {
                button.FlatAppearance.BorderSize = 2;
                button.FlatAppearance.BorderColor = Color.SteelBlue;
                button.BackColor = Color.FromArgb(240, 240, 240);
            };

            button.MouseLeave += (s, e) =>
            {
                button.FlatAppearance.BorderSize = 0;
                button.FlatAppearance.BorderColor = this.BackColor;
                button.BackColor = Color.Transparent;
            };
        }

        private bool IsConnectedToWiFi()
        {
            try
            {
                NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
                foreach (NetworkInterface ni in interfaces)
                {
                    if (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 &&
                        ni.OperationalStatus == OperationalStatus.Up)
                    {
                        IPInterfaceProperties ipProps = ni.GetIPProperties();
                        foreach (UnicastIPAddressInformation ip in ipProps.UnicastAddresses)
                        {
                            if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                            {
                                return true;
                            }
                        }
                    }
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        private async void btnScan_Click(object sender, EventArgs e)
        {
            if (isScanning)
            {
                StopScan();
                return;
            }

            StartScan();
            await ScanNetworkAsync();
        }

        private void UpdateCurrentNetworkInfo()
        {
            try
            {
                string output = RunCommand("netsh", "wlan show interfaces");
                string[] lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string line in lines)
                {
                    if (line.Contains("SSID") && !line.Contains("BSSID"))
                    {
                        string ssid = line.Split(':')[1].Trim();
                        if (!string.IsNullOrEmpty(ssid) && ssid != "0")
                        {
                            currentSSID = ssid;
                            lblCurrentNetwork.Text = $"Bağlı WiFi: {ssid}";
                            break;
                        }
                    }
                }
            }
            catch
            {
                lblCurrentNetwork.Text = "Bağlı WiFi: Tespit edilemedi";
            }
        }

        private async Task<List<(string ip, string mac)>> GetArpTableAsync()
        {
            return await Task.Run(() =>
            {
                var arpDevices = new List<(string ip, string mac)>();
                string arpOutput = RunCommand("arp", "-a");
                string[] arpLines = arpOutput.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                // Mevcut WiFi ağının IP aralığını al
                string localIp = GetLocalIPAddress();
                string[] ipParts = localIp.Split('.');
                string networkPrefix = $"{ipParts[0]}.{ipParts[1]}.{ipParts[2]}.";

                foreach (string line in arpLines)
                {
                    var match = Regex.Match(line, @"(?<ip>\d+\.\d+\.\d+\.\d+)\s+([\-\w]+)?\s+(?<mac>([0-9A-Fa-f]{2}[:-]){5}[0-9A-Fa-f]{2})");
                    if (match.Success)
                    {
                        string ip = match.Groups["ip"].Value;
                        // Sadece aynı ağdaki cihazları ekle
                        if (ip.StartsWith(networkPrefix))
                        {
                            string mac = match.Groups["mac"].Value.ToUpper().Replace(":", "-");
                            arpDevices.Add((ip, mac));
                        }
                    }
                }

                return arpDevices;
            });
        }

        // Her cihazı ayrı ayrı işle
        private async Task ProcessDeviceAsync(string ip, string mac)
        {
            try
            {
                // Önce bu cihazın daha önce eklenip eklenmediğini kontrol et
                lock (allDevices)
                {
                    if (allDevices.Any(d => d.mac == mac))
                    {
                        return; // Bu cihaz zaten eklenmiş, işlemi sonlandır
                    }
                }

                string vendor = "Bilinmeyen Üretici";
                string hostname = "Çözümlenemedi";
                string deviceType = "Bilinmeyen Cihaz";

                // Hostname çözümleme
                try
                {
                    var hostEntry = await Dns.GetHostEntryAsync(ip);
                    if (hostEntry != null && !string.IsNullOrEmpty(hostEntry.HostName))
                    {
                        hostname = hostEntry.HostName;
                    }
                }
                catch { }

                // Önce cache'den kontrol et
                if (macVendorCache.ContainsKey(mac))
                {
                    vendor = macVendorCache[mac];
                }
                else
                {
                    // Yeni MAC lookup sınıfını kullan
                    var detailedDeviceInfo = await macLookup.GetDetailedDeviceInfoAsync(mac);
                    vendor = detailedDeviceInfo.CompanyName;
                    deviceType = detailedDeviceInfo.DeviceCategory;

                    // Cache'e ekle
                    macVendorCache[mac] = vendor;
                    SaveMacVendorCache();
                }

                string connectionType = GetConnectionType(ip);

                // Eğer deviceType hala bilinmiyorsa, tekrar kontrol et
                if (deviceType == "Bilinmeyen Cihaz")
                {
                    var deviceDetails = await macLookup.GetDetailedDeviceInfoAsync(mac);
                    deviceType = deviceDetails.DeviceCategory;
                }

                var deviceTuple = (ip, mac, hostname, vendor, connectionType, deviceType);

                lock (allDevices)
                {
                    allDevices.Add(deviceTuple);
                }

                // Filtreleme kontrolü
                if (!chkLocalOnly.Checked || (connectionType != null && connectionType.Trim().ToLower() == "yerel ağ"))
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        AddDeviceCard(ip, mac, hostname, vendor, connectionType, deviceType);
                        deviceCount++;
                        if (deviceCount == 1)
                            HideLoadingSpinner();
                        labelCount.Text = $"Taranan cihaz sayısı: {deviceCount}";
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Cihaz işlenirken hata: {ex.Message}");
            }
        }

        // Ek ping taraması - sadece gerekirse
        private async Task PerformAdditionalPingScan(HashSet<string> seenMacs)
        {
            string localIp = GetLocalIPAddress();
            string baseIp = string.Join(".", localIp.Split('.').Take(3)) + ".";

            var pingTasks = new List<Task>();

            for (int i = 1; i <= 254; i++)
            {
                string ip = baseIp + i;
                pingTasks.Add(PingAndProcessAsync(ip, seenMacs));
            }

            await Task.WhenAll(pingTasks);
        }

        private async Task PingAndProcessAsync(string ip, HashSet<string> seenMacs)
        {
            await pingSemaphore.WaitAsync();
            try
            {
                using (var ping = new System.Net.NetworkInformation.Ping())
                {
                    var reply = await ping.SendPingAsync(ip, 500); // 500ms timeout
                    if (reply.Status == System.Net.NetworkInformation.IPStatus.Success)
                    {
                        // ARP tablosunu tekrar kontrol et
                        await Task.Delay(100); // ARP tablosunun güncellenmesi için kısa bekleme

                        string arpOutput = RunCommand("arp", $"-a {ip}");
                        var match = System.Text.RegularExpressions.Regex.Match(arpOutput, @"([0-9A-Fa-f]{2}[:-]){5}[0-9A-Fa-f]{2}");
                        if (match.Success)
                        {
                            string mac = match.Value.ToUpper().Replace(":", "-");
                            lock (seenMacs)
                            {
                                if (!seenMacs.Contains(mac))
                                {
                                    seenMacs.Add(mac);
                                    _ = ProcessDeviceAsync(ip, mac);
                                }
                            }
                        }
                    }
                }
            }
            catch { }
            finally
            {
                pingSemaphore.Release();
            }
        }

        // Vendor bilgisini güncellemek için yardımcı method
        private async void UpdateDeviceVendor(string mac, string vendor)
        {
            foreach (Panel panel in flowDevices.Controls.OfType<Panel>())
            {
                var macLabel = panel.Controls.OfType<Label>().FirstOrDefault(l => l.Text.Contains(mac));
                if (macLabel != null)
                {
                    var vendorLabel = panel.Controls.OfType<Label>().FirstOrDefault(l => l.Text.StartsWith("🏭 Üretici:"));
                    if (vendorLabel != null)
                    {
                        vendorLabel.Text = $"🏭 Üretici: {vendor}";
                    }

                    // Device type'ı da güncelle
                    var deviceInfo = await GetDetailedDeviceInfoFromMacAsync(mac);
                    string newDeviceType = GetDeviceType(vendor, deviceInfo);
                    var typeLabel = panel.Controls.OfType<Label>().FirstOrDefault(l => l.Text.Contains("Cihazı") || l.Text.Contains("Diğer"));
                    if (typeLabel != null)
                    {
                        typeLabel.Text = newDeviceType;
                    }

                    var iconLabel = panel.Controls.OfType<Label>().FirstOrDefault(l => l.Font.Name == "Segoe UI Symbol");
                    if (iconLabel != null)
                    {
                        iconLabel.Text = GetDeviceIcon(newDeviceType);
                    }
                    break;
                }
            }
        }

        private async Task<string> GetVendorFromMacAsync(string mac)
        {
            try
            {
                // MAC adresinin ilk 6 karakterini al (OUI - Organizationally Unique Identifier)
                string oui = mac.Substring(0, 8).Replace("-", "").ToUpper();

                // Önce cache'den kontrol et
                if (macVendorCache.ContainsKey(oui))
                {
                    return macVendorCache[oui];
                }

                // MAC veritabanından sorgula
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(5);
                    string url = $"https://api.macvendors.com/{oui}";
                    try
                    {
                        string vendor = await client.GetStringAsync(url);
                        if (!string.IsNullOrWhiteSpace(vendor))
                        {
                            // Cache'e ekle
                            macVendorCache[oui] = vendor;
                            SaveMacVendorCache();
                            return vendor;
                        }
                    }
                    catch (HttpRequestException)
                    {
                        // API'den yanıt alınamadı, alternatif kaynakları dene
                        return await GetVendorFromAlternativeSource(oui);
                    }
                }
            }
            catch
            {
                // Hata durumunda bilinmeyen üretici döndür
                return "Bilinmeyen Üretici";
            }
            return "Bilinmeyen Üretici";
        }

        private async Task<DeviceInfo> GetDetailedDeviceInfoFromMacAsync(string mac)
        {
            try
            {
                string oui = mac.Substring(0, 8).Replace("-", "").ToUpper();
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(5);
                    string url = $"https://api.macaddress.io/v1?apiKey=at_8Qpuw07fVV1S1b3fuR7JfZf1ZXp5a&output=json&search={mac}";
                    var response = await client.GetStringAsync(url);
                    var json = JsonDocument.Parse(response);
                    var root = json.RootElement;

                    var deviceInfo = new DeviceInfo();

                    if (root.TryGetProperty("vendorDetails", out var vendorDetails))
                    {
                        deviceInfo.CompanyName = vendorDetails.GetProperty("companyName").GetString() ?? "Bilinmeyen Üretici";
                        deviceInfo.CompanyAddress = vendorDetails.GetProperty("companyAddress").GetString() ?? "";
                        deviceInfo.CountryCode = vendorDetails.GetProperty("countryCode").GetString() ?? "";
                    }

                    if (root.TryGetProperty("blockDetails", out var blockDetails))
                    {
                        deviceInfo.BlockSize = blockDetails.GetProperty("blockSize").GetString() ?? "";
                        deviceInfo.AssignmentBlockSize = blockDetails.GetProperty("assignmentBlockSize").GetString() ?? "";
                        deviceInfo.DateCreated = blockDetails.GetProperty("dateCreated").GetString() ?? "";
                        deviceInfo.DateUpdated = blockDetails.GetProperty("dateUpdated").GetString() ?? "";
                    }

                    if (root.TryGetProperty("macAddressDetails", out var macDetails))
                    {
                        if (macDetails.TryGetProperty("applications", out var applications))
                        {
                            foreach (var app in applications.EnumerateArray())
                            {
                                deviceInfo.Applications.Add(app.GetString() ?? "");
                            }
                        }
                        deviceInfo.TransmissionType = macDetails.GetProperty("transmissionType").GetString() ?? "";
                        deviceInfo.AdministrationType = macDetails.GetProperty("administrationType").GetString() ?? "";
                        deviceInfo.WiresharkNotes = macDetails.GetProperty("wiresharkNotes").GetString() ?? "";
                        deviceInfo.Comment = macDetails.GetProperty("comment").GetString() ?? "";
                    }

                    return deviceInfo;
                }
            }
            catch
            {
                return new DeviceInfo();
            }
        }

        private async Task<string> GetVendorFromAlternativeSource(string oui)
        {
            try
            {
                var deviceInfo = await GetDetailedDeviceInfoFromMacAsync(oui);
                if (!string.IsNullOrWhiteSpace(deviceInfo.CompanyName))
                {
                    // Cache'e ekle
                    macVendorCache[oui] = deviceInfo.CompanyName;
                    SaveMacVendorCache();
                    return deviceInfo.CompanyName;
                }
            }
            catch
            {
                // Hata durumunda bilinmeyen üretici döndür
                return "Bilinmeyen Üretici";
            }
            return "Bilinmeyen Üretici";
        }

        private void SaveMacVendorCache()
        {
            try
            {
                string cachePath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "wifi4",
                    macVendorCacheFile
                );

                Directory.CreateDirectory(Path.GetDirectoryName(cachePath));
                File.WriteAllLines(
                    cachePath,
                    macVendorCache.Select(kvp => $"{kvp.Key}={kvp.Value}")
                );
            }
            catch
            {
                // Cache kaydetme hatası - sessizce devam et
            }
        }

        private string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            return "192.168.1.1";
        }

        private string GetConnectionType(string ip)
        {
            string[] parts = ip.Split('.');
            if (parts.Length == 4)
            {
                int firstOctet = int.Parse(parts[0]);
                if (firstOctet == 192 && parts[1] == "168")
                    return "Yerel Ağ";
                else if (firstOctet == 10)
                    return "Yerel Ağ";
                else if (firstOctet == 172 && int.Parse(parts[1]) >= 16 && int.Parse(parts[1]) <= 31)
                    return "Yerel Ağ";
            }
            return "Harici Ağ";
        }

        private string GetDeviceType(string vendor, DeviceInfo deviceInfo = null)
        {
            vendor = vendor.ToLower();

            if (deviceInfo != null)
            {
                if (deviceInfo.Applications.Any(a => a.ToLower().Contains("virtual machine")))
                    return "Sanal Makine";
                if (deviceInfo.Applications.Any(a => a.ToLower().Contains("mobile")))
                    return "Mobil Cihaz";
                if (deviceInfo.TransmissionType.ToLower().Contains("wireless"))
                    return "Kablosuz Cihaz";
            }

            if (vendor.Contains("apple"))
                return "Apple Cihazı";
            if (vendor.Contains("vmware") || vendor.Contains("virtualbox"))
                return "Sanal Makine";
            if (vendor.Contains("samsung"))
                return "Samsung Cihazı";

            return "Diğer Cihaz";
        }

        private string RunCommand(string command, string args)
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo(command, args)
                {
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process process = Process.Start(psi))
                {
                    process.WaitForExit();
                    return process.StandardOutput.ReadToEnd();
                }
            }
            catch
            {
                return "";
            }
        }

        private void LoadMacVendorCache()
        {
            if (File.Exists(macVendorCacheFile))
            {
                foreach (var line in File.ReadAllLines(macVendorCacheFile))
                {
                    var parts = line.Split('|');
                    if (parts.Length == 2)
                        macVendorCache[parts[0]] = parts[1];
                }
            }
        }

        private string GetWiFiInterface(string ip)
        {
            try
            {
                string output = RunCommand("netsh", "wlan show interfaces");
                string[] lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string line in lines)
                {
                    if (line.Contains("Name") && !line.Contains("Description"))
                    {
                        return line.Split(':')[1].Trim();
                    }
                }
            }
            catch { }
            return "Bilinmiyor";
        }

        private void AddDeviceCard(string ip, string mac, string hostname, string vendor, string connectionType, string deviceType)
        {
            var panel = new Panel
            {
                Width = 260,
                Height = 180,
                Margin = new Padding(10),
                Padding = new Padding(15),
                BackColor = Color.White,
                Tag = false
            };

            // Hover efekti için event handler'lar
            panel.MouseEnter += (s, e) =>
            {
                panel.BackColor = Color.FromArgb(245, 245, 245);
                panel.BorderStyle = BorderStyle.FixedSingle;
            };

            panel.MouseLeave += (s, e) =>
            {
                panel.BackColor = Color.White;
                panel.BorderStyle = BorderStyle.None;
            };

            // MAC adresini normalize et
            string normMac = mac.Replace(":", "-").ToUpper();
            string displayName = customDeviceNames.ContainsKey(normMac)
                ? customDeviceNames[normMac]
                : hostname != "Çözümlenemedi" ? hostname : "Bilinmeyen Cihaz";

            var iconLabel = new Label
            {
                Text = GetDeviceIcon(deviceType),
                Font = new Font("Segoe UI Symbol", 32),
                Location = new Point(10, 10),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            var nameLabel = new Label
            {
                Text = displayName,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(iconLabel.Right + 5, 15),
                Size = new Size(panel.Width - (iconLabel.Right + 20), 25),
                AutoSize = false,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleLeft
            };

            var typeLabel = new Label
            {
                Text = deviceType,
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray,
                Location = new Point(iconLabel.Right + 5, 40),
                Size = new Size(panel.Width - (iconLabel.Right + 20), 20),
                AutoSize = false,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleLeft
            };

            var connectionLabel = new Label
            {
                Text = $"🌐 {connectionType}",
                Font = new Font("Segoe UI", 9),
                Location = new Point(15, 80),
                Size = new Size(panel.Width - 30, 20),
                AutoSize = false,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleLeft
            };

            var ipLabel = new Label
            {
                Text = $"📡 IP: {ip}",
                Font = new Font("Segoe UI", 9),
                Location = new Point(15, 105),
                Size = new Size(panel.Width - 30, 20),
                AutoSize = false,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleLeft
            };

            var macLabel = new Label
            {
                Text = $"🔑 MAC: {mac}",
                Font = new Font("Segoe UI", 9),
                Location = new Point(15, 130),
                Size = new Size(panel.Width - 30, 20),
                AutoSize = false,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleLeft
            };

            var vendorLabel = new Label
            {
                Text = $"🏭 Üretici: {vendor}",
                Font = new Font("Segoe UI", 9),
                Location = new Point(15, 155),
                Size = new Size(panel.Width - 30, 20),
                AutoSize = false,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleLeft,
                Tag = mac
            };

            // Cihaz detaylarını almak için tıklama olayı
            vendorLabel.Click += async (s, e) =>
            {
                try
                {
                    var deviceInfo = await macLookup.GetDetailedDeviceInfoAsync(mac);
                    MessageBox.Show(macLookup.GenerateDetailedReport(deviceInfo),
                        "Cihaz Detayları",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Cihaz detayları alınırken hata oluştu: " + ex.Message);
                }
            };

            vendorLabel.Cursor = Cursors.Hand;

            panel.Controls.AddRange(new Control[] {
                iconLabel, nameLabel, typeLabel, connectionLabel,
                ipLabel, macLabel, vendorLabel
            });

            panel.DoubleClick += (sender, e) =>
            {
                var deviceInfoForm = new DeviceInfoForm(
                    displayName, ip, mac, vendor, hostname, connectionType, deviceType
                );
                deviceInfoForm.ShowDialog();
            };

            flowDevices.Controls.Add(panel);
        }

        private string GetDeviceIcon(string deviceType)
        {
            switch (deviceType)
            {
                // Mobil Cihazlar
                case "iPhone": return "📱";
                case "iPad": return "📱";
                case "iPod": return "🎵";
                case "Samsung Galaxy": return "📱";
                case "Samsung Note": return "📱";
                case "Xiaomi Cihazı": return "📱";
                case "Huawei Cihazı": return "📱";
                case "OPPO/Vivo Cihazı": return "📱";
                case "OnePlus Cihazı": return "📱";
                case "Google Pixel": return "📱";

                // Bilgisayarlar
                case "Microsoft Surface": return "💻";
                case "Windows Bilgisayar": return "💻";
                case "Lenovo ThinkPad": return "💻";
                case "Lenovo Yoga": return "💻";
                case "Dell Bilgisayar": return "💻";
                case "HP Laptop": return "💻";
                case "HP Bilgisayar": return "💻";
                case "ASUS ROG": return "💻";
                case "ASUS ZenBook": return "💻";
                case "Acer Bilgisayar": return "💻";
                case "MSI Bilgisayar": return "💻";

                // Ağ Cihazları
                case "Cisco Router": return "🌐";
                case "Netgear Router": return "🌐";
                case "TP-Link Router": return "🌐";
                case "ASUS Router": return "🌐";
                case "D-Link Router": return "🌐";
                case "Huawei Router": return "🌐";
                case "ZTE Router": return "🌐";
                case "Router": return "🌐";
                case "Modem": return "🌐";

                // Yazıcılar
                case "HP Yazıcı": return "🖨️";
                case "Canon Yazıcı": return "🖨️";
                case "Epson Yazıcı": return "🖨️";
                case "Brother Yazıcı": return "🖨️";
                case "Yazıcı": return "🖨️";

                // TV ve Medya Cihazları
                case "Samsung TV": return "📺";
                case "LG TV": return "📺";
                case "Sony TV": return "📺";
                case "Philips TV": return "📺";
                case "TV": return "📺";
                case "Roku Cihazı": return "📺";
                case "Chromecast": return "📺";
                case "Amazon Fire TV": return "📺";

                // Oyun Konsolları
                case "PlayStation": return "🎮";
                case "Xbox": return "🎮";
                case "Nintendo": return "🎮";

                // Akıllı Ev Cihazları
                case "Google Nest": return "🏠";
                case "Ring Cihazı": return "🏠";
                case "Philips Hue": return "💡";
                case "Amazon Echo": return "🔊";

                // Donanım Üreticileri
                case "Intel Cihazı": return "💻";
                case "AMD Cihazı": return "💻";
                case "NVIDIA Cihazı": return "💻";

                default: return "❓";
            }
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            if (isScanning)
            {
                StopScan();
            }

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
                if (isScanning)
                {
                    StopScan();
                }

                // Ana menü formunu bul ve göster
                foreach (Form form in Application.OpenForms)
                {
                    if (form is MainMenuForm)
                    {
                        form.Show();
                        break;
                    }
                }

                // Formu tamamen kapat
                this.Dispose();
            }
            base.OnFormClosing(e);
        }

        // Form yeniden boyutlandırıldığında spinner'ları ortala
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (loadingSpinner != null && loadingSpinner.Visible)
            {
                loadingSpinner.Location = new Point((this.Width - loadingSpinner.Width) / 2, (this.Height - loadingSpinner.Height) / 2);
            }
            if (countSpinner != null && countSpinner.Visible)
            {
                countSpinner.Location = new Point(labelCount.Right + 8);
            }
        }

        public void ShowFilteredDevices()
        {
            flowDevices.Controls.Clear();
            deviceCount = 0;
            var filteredDevices = chkLocalOnly.Checked
                ? allDevices.Where(d => d.connectionType != null && d.connectionType.Trim().ToLower() == "yerel ağ").ToList()
                : allDevices;
            foreach (var device in filteredDevices)
            {
                AddDeviceCard(device.ip, device.mac, device.hostname, device.vendor, device.connectionType, device.deviceType);
                deviceCount++;
            }
            labelCount.Text = $"Toplam cihaz sayısı: {deviceCount}";
        }

        public void LoadCustomDeviceNames()
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

        public void SaveCustomDeviceNames()
        {
            var lines = customDeviceNames.Select(kvp => $"{kvp.Key}|{kvp.Value}");
            File.WriteAllLines(customNamesFilePath, lines);
        }

        public void ShowDeviceEditDialog(string mac)
        {
            string normMac = mac.Replace(":", "-").ToUpper();
            string currentName = customDeviceNames.ContainsKey(normMac) ? customDeviceNames[normMac] : "";
            Form dialog = new Form { Width = 300, Height = 120, Text = "Cihaz İsmini Değiştir" };
            TextBox txt = new TextBox { Text = currentName, Left = 10, Top = 10, Width = 260 };
            Button btn = new Button { Text = "Kaydet", Left = 100, Width = 80, Top = 40, DialogResult = DialogResult.OK };
            dialog.Controls.Add(txt);
            dialog.Controls.Add(btn);
            dialog.AcceptButton = btn;
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string newName = txt.Text.Trim();
                if (!string.IsNullOrEmpty(newName))
                {
                    customDeviceNames[normMac] = newName;
                    SaveCustomDeviceNames();
                    LoadCustomDeviceNames();
                    ShowFilteredDevices();
                }
            }
        }

        private void StartScan()
        {
            isScanning = true;
            scanCancellationSource = new CancellationTokenSource();
            btnScan.Text = "Durdur";
            ShowLoadingSpinner();
            flowDevices.Controls.Clear();
            allDevices.Clear();
            deviceCount = 0;
            labelCount.Text = "Taranan cihaz sayısı: 0";
            ShowCountSpinner();
        }

        private void StopScan()
        {
            isScanning = false;
            scanCancellationSource?.Cancel();
            btnScan.Text = "Tara";
            HideLoadingSpinner();
            HideCountSpinner();
            labelCount.Text = $"Toplam cihaz sayısı: {deviceCount}";
        }

        private async Task ScanNetworkAsync()
        {
            try
            {
                var localIP = GetLocalIPAddress();
                if (string.IsNullOrEmpty(localIP))
                {
                    MessageBox.Show("Yerel IP adresi bulunamadı!");
                    return;
                }

                var baseIP = localIP.Substring(0, localIP.LastIndexOf('.') + 1);
                var tasks = new List<Task>();

                for (int i = 1; i <= 254; i++)
                {
                    if (scanCancellationSource.Token.IsCancellationRequested)
                        break;

                    string ip = baseIP + i.ToString();
                    tasks.Add(ScanIPAsync(ip));

                    if (tasks.Count >= 10)
                    {
                        await Task.WhenAny(tasks);
                        tasks.RemoveAll(t => t.IsCompleted);
                    }
                }

                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Tarama sırasında hata oluştu: {ex.Message}");
            }
            finally
            {
                StopScan();
            }
        }

        private async Task ScanIPAsync(string ip)
        {
            try
            {
                using (var ping = new Ping())
                {
                    var reply = await ping.SendPingAsync(ip, 1000);
                    if (reply.Status == IPStatus.Success)
                    {
                        var mac = GetMacAddress(ip);
                        if (!string.IsNullOrEmpty(mac))
                        {
                            await ProcessDeviceAsync(ip, mac);
                        }
                    }
                }
            }
            catch { }
        }

        private string GetMacAddress(string ip)
        {
            try
            {
                var macAddr = new byte[6];
                var destIP = BitConverter.ToInt32(IPAddress.Parse(ip).GetAddressBytes(), 0);

                SendARP(destIP, 0, macAddr, new int[] { 6 });
                return BitConverter.ToString(macAddr).Replace("-", ":");
            }
            catch
            {
                return string.Empty;
            }
        }

        [System.Runtime.InteropServices.DllImport("iphlpapi.dll", ExactSpelling = true)]
        private static extern int SendARP(int destIP, int srcIP, byte[] macAddr, int[] physicalAddrLen);

        private void LoadResources()
        {
            try
            {
                // Buton resimlerini yükle
                btnScan.BackgroundImage = Image.FromFile(@"C:\Users\TALHA\Documents\GitHub\wifiv2\wifi4\Resources\icons8-multiple-devices-50.png");
                btnScan.BackgroundImageLayout = ImageLayout.Center;
                btnScan.FlatStyle = FlatStyle.Flat;
                btnScan.FlatAppearance.BorderSize = 0;
                btnScan.Size = new Size(50, 50);
                btnScan.BackColor = Color.Transparent;

                btnBack.BackgroundImage = Image.FromFile(@"C:\Users\TALHA\Documents\GitHub\wifiv2\wifi4\Resources\icons8-home-page-50.png");
                btnBack.BackgroundImageLayout = ImageLayout.Center;
                btnBack.FlatStyle = FlatStyle.Flat;
                btnBack.FlatAppearance.BorderSize = 0;
                btnBack.Size = new Size(50, 50);
                btnBack.BackColor = Color.Transparent;

                // Loading animasyonunu yükle
                string loadingPath = @"C:\Users\TALHA\Documents\GitHub\wifiv2\wifi4\Resources\g2.gif";
                if (File.Exists(loadingPath))
                {
                    loadingSpinner.Image = Image.FromFile(loadingPath);
                    loadingSpinner.SizeMode = PictureBoxSizeMode.Zoom;
                    loadingSpinner.Size = new Size(100, 100);
                    loadingSpinner.Visible = false;

                    countSpinner.Image = Image.FromFile(loadingPath);
                    countSpinner.SizeMode = PictureBoxSizeMode.Zoom;
                    countSpinner.Size = new Size(20, 20);
                    countSpinner.Visible = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kaynak dosyaları yüklenirken hata oluştu: " + ex.Message);
            }
        }
    }

    public class EnhancedMacLookup
    {
        private static readonly HttpClient httpClient = new HttpClient();
        private Dictionary<string, DeviceInfo> deviceCache = new Dictionary<string, DeviceInfo>();
        private string cacheFilePath;

        public class DetailedDeviceInfo
        {
            public string CompanyName { get; set; } = "Bilinmeyen Üretici";
            public string CompanyAddress { get; set; } = "";
            public string CountryCode { get; set; } = "";
            public string DeviceType { get; set; } = "Bilinmeyen Cihaz";
            public string DeviceCategory { get; set; } = "Diğer";
            public bool IsVirtualMachine { get; set; } = false;
            public bool IsMobile { get; set; } = false;
            public bool IsComputer { get; set; } = false;
            public bool IsNetworkDevice { get; set; } = false;
            public bool IsAppleDevice { get; set; } = false;
            public bool IsSamsungDevice { get; set; } = false;
            public List<string> PossibleDevices { get; set; } = new List<string>();
            public string OUI { get; set; } = "";
            public DateTime LastUpdated { get; set; } = DateTime.Now;
            public string SpecificModel { get; set; } = "";
            public string OperatingSystem { get; set; } = "";
            public List<string> TechnicalSpecs { get; set; } = new List<string>();
        }

        public EnhancedMacLookup()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string dir = Path.Combine(appData, "wifi4");
            Directory.CreateDirectory(dir);
            cacheFilePath = Path.Combine(dir, "device_info_cache.json");
            LoadDeviceCache();
        }

        // Birden fazla API'den veri topla
        public async Task<DetailedDeviceInfo> GetDetailedDeviceInfoAsync(string macAddress)
        {
            try
            {
                string normalizedMac = NormalizeMacAddress(macAddress);
                string oui = normalizedMac.Substring(0, 8).Replace("-", "").ToUpper();

                // Cache kontrolü
                if (deviceCache.ContainsKey(oui) &&
                    (DateTime.Now - deviceCache[oui].LastUpdated).TotalDays < 7)
                {
                    return ConvertToDetailedInfo(deviceCache[oui]);
                }

                var detailedInfo = new DetailedDeviceInfo { OUI = oui };

                // 1. MacVendors API - En güvenilir
                await GetDataFromMacVendorsAsync(oui, detailedInfo);

                // 2. MacAddress.io API - Detaylı bilgi
                await GetDataFromMacAddressIoAsync(normalizedMac, detailedInfo);

                // 3. IEEE OUI Database - Resmi kaynak
                await GetDataFromIEEE(oui, detailedInfo);

                // 4. Maclookup.app - Alternatif kaynak
                await GetDataFromMacLookupAsync(oui, detailedInfo);

                // 5. Yerel OUI veritabanı kontrolü
                EnhanceWithLocalDatabase(oui, detailedInfo);

                // Cihaz tipini analiz et
                AnalyzeDeviceType(detailedInfo);

                // Cache'e kaydet
                var cacheInfo = ConvertFromDetailedInfo(detailedInfo);
                deviceCache[oui] = cacheInfo;
                SaveDeviceCache();

                return detailedInfo;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"MAC lookup error: {ex.Message}");
                return new DetailedDeviceInfo();
            }
        }

        private async Task GetDataFromMacVendorsAsync(string oui, DetailedDeviceInfo detailedInfo)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(3);
                    string url = $"https://api.macvendors.com/{oui}";
                    string vendor = await client.GetStringAsync(url);

                    if (!string.IsNullOrWhiteSpace(vendor))
                    {
                        detailedInfo.CompanyName = CleanVendorName(vendor);
                    }

                    // Rate limiting için bekle
                    await Task.Delay(100);
                }
            }
            catch { }
        }

        private async Task GetDataFromMacAddressIoAsync(string macAddress, DetailedDeviceInfo detailedInfo)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(5);
                    // Ücretsiz API key - günlük 1000 sorgu limiti
                    string url = $"https://api.macaddress.io/v1?apiKey=at_8Qpuw07fVV1S1b3fuR7JfZf1ZXp5a&output=json&search={macAddress}";

                    var response = await client.GetStringAsync(url);
                    var json = JsonDocument.Parse(response);
                    var root = json.RootElement;

                    if (root.TryGetProperty("vendorDetails", out var vendorDetails))
                    {
                        if (vendorDetails.TryGetProperty("companyName", out var companyName))
                            detailedInfo.CompanyName = companyName.GetString() ?? detailedInfo.CompanyName;

                        if (vendorDetails.TryGetProperty("companyAddress", out var address))
                            detailedInfo.CompanyAddress = address.GetString() ?? "";

                        if (vendorDetails.TryGetProperty("countryCode", out var country))
                            detailedInfo.CountryCode = country.GetString() ?? "";
                    }

                    if (root.TryGetProperty("macAddressDetails", out var macDetails))
                    {
                        if (macDetails.TryGetProperty("applications", out var applications))
                        {
                            foreach (var app in applications.EnumerateArray())
                            {
                                var appName = app.GetString();
                                if (!string.IsNullOrEmpty(appName))
                                    detailedInfo.TechnicalSpecs.Add($"Uygulama: {appName}");
                            }
                        }

                        if (macDetails.TryGetProperty("transmissionType", out var transmission))
                        {
                            var transmissionType = transmission.GetString();
                            if (!string.IsNullOrEmpty(transmissionType))
                                detailedInfo.TechnicalSpecs.Add($"İletim Tipi: {transmissionType}");
                        }
                    }
                }
            }
            catch { }
        }

        private async Task GetDataFromIEEE(string oui, DetailedDeviceInfo detailedInfo)
        {
            try
            {
                // IEEE'nin resmi OUI veritabanından bilgi al
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(3);
                    string url = $"http://standards-oui.ieee.org/oui/oui.txt";

                    // Bu büyük bir dosya, cache'lenmeli
                    // Alternatif olarak sadece OUI lookup yapabiliriz
                    string lookupUrl = $"https://www.wireshark.org/tools/oui-lookup.html";
                    // Bu kısım daha karmaşık parsing gerektirir
                }
            }
            catch { }
        }

        private async Task GetDataFromMacLookupAsync(string oui, DetailedDeviceInfo detailedInfo)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(3);
                    string url = $"https://maclookup.app/api/v2/macs/{oui}";

                    var response = await client.GetStringAsync(url);
                    var json = JsonDocument.Parse(response);

                    if (json.RootElement.TryGetProperty("company", out var company))
                    {
                        var companyName = company.GetString();
                        if (!string.IsNullOrEmpty(companyName) && detailedInfo.CompanyName == "Bilinmeyen Üretici")
                        {
                            detailedInfo.CompanyName = CleanVendorName(companyName);
                        }
                    }
                }
            }
            catch { }
        }

        private void EnhanceWithLocalDatabase(string oui, DetailedDeviceInfo detailedInfo)
        {
            // Yerel OUI veritabanı - bilinen üreticiler ve cihaz tipleri
            var knownOUIs = new Dictionary<string, (string vendor, string deviceType, string category)>
            {
                // Apple
                {"00:1B:63", ("Apple", "iPhone/iPad", "Mobile")},
                {"00:23:12", ("Apple", "iPhone", "Mobile")},
                {"A4:B1:C1", ("Apple", "iPhone", "Mobile")},
                {"00:25:BC", ("Apple", "iPhone/iPad", "Mobile")},
                {"3C:15:C2", ("Apple", "iPhone", "Mobile")},
                {"00:26:08", ("Apple", "iPhone", "Mobile")},
                {"00:17:F2", ("Apple", "Mac", "Computer")},
                {"00:1E:C2", ("Apple", "Mac", "Computer")},
                {"00:25:00", ("Apple", "Mac", "Computer")},
                {"AC:DE:48", ("Apple", "Mac", "Computer")},
                {"00:23:DF", ("Apple", "Mac", "Computer")},
                
                // Samsung
                {"28:E3:47", ("Samsung", "Galaxy Phone", "Mobile")},
                {"34:BE:00", ("Samsung", "Galaxy Phone", "Mobile")},
                {"78:F8:82", ("Samsung", "Galaxy Phone", "Mobile")},
                {"5C:0A:5B", ("Samsung", "Galaxy Phone", "Mobile")},
                {"E8:50:8B", ("Samsung", "Galaxy Phone", "Mobile")},
                {"00:12:FB", ("Samsung", "Galaxy Phone", "Mobile")},
                {"00:15:B9", ("Samsung", "Galaxy Phone", "Mobile")},
                
                // Xiaomi
                {"28:6D:CD", ("Xiaomi", "Mi/Redmi Phone", "Mobile")},
                {"34:CE:00", ("Xiaomi", "Mi/Redmi Phone", "Mobile")},
                {"64:09:80", ("Xiaomi", "Mi/Redmi Phone", "Mobile")},
                {"78:11:DC", ("Xiaomi", "Mi/Redmi Phone", "Mobile")},
                
                // Huawei
                {"00:E0:FC", ("Huawei", "Honor/P Series", "Mobile")},
                {"4C:54:99", ("Huawei", "Honor/P Series", "Mobile")},
                {"AC:E2:D3", ("Huawei", "Mate/P Series", "Mobile")},
                {"50:8F:4C", ("Huawei", "Router/Modem", "Network")},
                
                // Router/Modem üreticileri
                {"00:1A:2B", ("TP-Link", "Router", "Network")},
                {"00:27:19", ("TP-Link", "Router", "Network")},
                {"C4:E9:84", ("TP-Link", "Router", "Network")},
                {"00:50:56", ("VMware", "Virtual Machine", "Virtual")},
                {"00:0C:29", ("VMware", "Virtual Machine", "Virtual")},
                {"00:05:69", ("VMware", "Virtual Machine", "Virtual")},
                {"08:00:27", ("VirtualBox", "Virtual Machine", "Virtual")},
                {"00:15:5D", ("Microsoft", "Hyper-V VM", "Virtual")},
                
                // Yazıcılar
                {"00:17:C8", ("HP", "LaserJet Printer", "Printer")},
                {"00:1F:29", ("HP", "OfficeJet Printer", "Printer")},
                {"00:25:B3", ("Canon", "Pixma Printer", "Printer")},
                {"00:00:48", ("Canon", "Printer", "Printer")},
                
                // TV ve Medya
                {"E8:F2:E2", ("Samsung", "Smart TV", "TV")},
                {"00:26:37", ("Samsung", "Smart TV", "TV")},
                {"3C:A9:F4", ("Samsung", "Smart TV", "TV")},
                {"00:1C:62", ("LG", "Smart TV", "TV")},
                {"B8:17:C2", ("LG", "Smart TV", "TV")},
                
                // Oyun Konsolları
                {"00:19:C5", ("Sony", "PlayStation", "Gaming")},
                {"7C:ED:8D", ("Sony", "PlayStation 4", "Gaming")},
                {"CC:FB:65", ("Sony", "PlayStation 5", "Gaming")},
                {"00:50:F2", ("Microsoft", "Xbox", "Gaming")},
                {"98:B6:E9", ("Microsoft", "Xbox One", "Gaming")},
                {"00:09:BF", ("Nintendo", "Wii/Switch", "Gaming")},
                
                // Laptop üreticileri
                {"00:21:CC", ("Lenovo", "ThinkPad", "Computer")},
                {"00:26:2D", ("Lenovo", "Laptop", "Computer")},
                {"54:EE:75", ("Lenovo", "Laptop", "Computer")},
                {"00:15:E9", ("Dell", "Laptop", "Computer")},
                {"18:03:73", ("Dell", "Laptop", "Computer")},
                {"B8:CA:3A", ("Dell", "Laptop", "Computer")},
                {"00:1B:38", ("ASUS", "Laptop", "Computer")},
                {"1C:B7:2C", ("ASUS", "ROG Laptop", "Computer")},
                {"08:62:66", ("ASUS", "Laptop", "Computer")},
                {"00:1F:16", ("HP", "Laptop", "Computer")},
                {"94:57:A5", ("HP", "Laptop", "Computer")},
                {"6C:62:6D", ("HP", "Laptop", "Computer")},
            };

            if (knownOUIs.ContainsKey(oui))
            {
                var info = knownOUIs[oui];
                if (detailedInfo.CompanyName == "Bilinmeyen Üretici")
                    detailedInfo.CompanyName = info.vendor;
                detailedInfo.DeviceType = info.deviceType;
                detailedInfo.DeviceCategory = info.category;
                detailedInfo.SpecificModel = info.deviceType;
            }
        }

        private void AnalyzeDeviceType(DetailedDeviceInfo detailedInfo)
        {
            string vendor = detailedInfo.CompanyName.ToLower();
            string deviceType = detailedInfo.DeviceType.ToLower();

            // Sanal makine kontrolü
            if (vendor.Contains("vmware") || vendor.Contains("virtualbox") ||
                vendor.Contains("hyper-v") || vendor.Contains("parallels") ||
                deviceType.Contains("virtual"))
            {
                detailedInfo.IsVirtualMachine = true;
                detailedInfo.DeviceCategory = "Sanal Makine";
                detailedInfo.OperatingSystem = "Çeşitli (VM)";
            }

            // Apple cihazları
            if (vendor.Contains("apple"))
            {
                detailedInfo.IsAppleDevice = true;
                if (deviceType.Contains("iphone"))
                {
                    detailedInfo.IsMobile = true;
                    detailedInfo.DeviceCategory = "iPhone";
                    detailedInfo.OperatingSystem = "iOS";
                }
                else if (deviceType.Contains("ipad"))
                {
                    detailedInfo.IsMobile = true;
                    detailedInfo.DeviceCategory = "iPad";
                    detailedInfo.OperatingSystem = "iPadOS";
                }
                else if (deviceType.Contains("mac"))
                {
                    detailedInfo.IsComputer = true;
                    detailedInfo.DeviceCategory = "Mac";
                    detailedInfo.OperatingSystem = "macOS";
                }
                else
                {
                    detailedInfo.PossibleDevices.AddRange(new[]
                    { "iPhone", "iPad", "MacBook", "iMac", "Mac mini", "Apple TV", "AirPods" });
                }
            }

            // Samsung cihazları
            else if (vendor.Contains("samsung"))
            {
                detailedInfo.IsSamsungDevice = true;
                if (deviceType.Contains("galaxy") || deviceType.Contains("phone"))
                {
                    detailedInfo.IsMobile = true;
                    detailedInfo.DeviceCategory = "Samsung Galaxy";
                    detailedInfo.OperatingSystem = "Android";
                    detailedInfo.PossibleDevices.AddRange(new[]
                    { "Galaxy S Series", "Galaxy Note", "Galaxy A Series", "Galaxy M Series" });
                }
                else if (deviceType.Contains("tv"))
                {
                    detailedInfo.DeviceCategory = "Samsung Smart TV";
                    detailedInfo.OperatingSystem = "Tizen OS";
                }
                else
                {
                    detailedInfo.PossibleDevices.AddRange(new[]
                    { "Galaxy Phone", "Galaxy Tablet", "Smart TV", "Galaxy Watch", "Galaxy Buds" });
                }
            }

            // Diğer mobil cihazlar
            else if (vendor.Contains("xiaomi") || vendor.Contains("redmi"))
            {
                detailedInfo.IsMobile = true;
                detailedInfo.DeviceCategory = "Xiaomi/Redmi";
                detailedInfo.OperatingSystem = "MIUI (Android)";
                detailedInfo.PossibleDevices.AddRange(new[] { "Mi Series", "Redmi Series", "Poco Series" });
            }

            else if (vendor.Contains("huawei") || vendor.Contains("honor"))
            {
                if (deviceType.Contains("router") || deviceType.Contains("modem"))
                {
                    detailedInfo.IsNetworkDevice = true;
                    detailedInfo.DeviceCategory = "Huawei Router/Modem";
                }
                else
                {
                    detailedInfo.IsMobile = true;
                    detailedInfo.DeviceCategory = "Huawei/Honor";
                    detailedInfo.OperatingSystem = "HarmonyOS/Android";
                    detailedInfo.PossibleDevices.AddRange(new[] { "P Series", "Mate Series", "Honor Series" });
                }
            }

            // Bilgisayar üreticileri
            else if (vendor.Contains("lenovo"))
            {
                detailedInfo.IsComputer = true;
                detailedInfo.DeviceCategory = "Lenovo";
                detailedInfo.OperatingSystem = "Windows/Linux";
                detailedInfo.PossibleDevices.AddRange(new[] { "ThinkPad", "IdeaPad", "Legion", "Yoga" });
            }

            else if (vendor.Contains("dell"))
            {
                detailedInfo.IsComputer = true;
                detailedInfo.DeviceCategory = "Dell";
                detailedInfo.OperatingSystem = "Windows/Linux";
                detailedInfo.PossibleDevices.AddRange(new[] { "Inspiron", "XPS", "Latitude", "Alienware" });
            }

            else if (vendor.Contains("hp") || vendor.Contains("hewlett"))
            {
                if (deviceType.Contains("printer"))
                {
                    detailedInfo.DeviceCategory = "HP Yazıcı";
                    detailedInfo.PossibleDevices.AddRange(new[] { "LaserJet", "OfficeJet", "DeskJet" });
                }
                else
                {
                    detailedInfo.IsComputer = true;
                    detailedInfo.DeviceCategory = "HP";
                    detailedInfo.OperatingSystem = "Windows";
                    detailedInfo.PossibleDevices.AddRange(new[] { "Pavilion", "EliteBook", "ProBook", "Envy" });
                }
            }

            else if (vendor.Contains("asus"))
            {
                if (deviceType.Contains("router"))
                {
                    detailedInfo.IsNetworkDevice = true;
                    detailedInfo.DeviceCategory = "ASUS Router";
                }
                else
                {
                    detailedInfo.IsComputer = true;
                    detailedInfo.DeviceCategory = "ASUS";
                    detailedInfo.OperatingSystem = "Windows/Linux";
                    detailedInfo.PossibleDevices.AddRange(new[] { "ROG", "ZenBook", "VivoBook", "TUF Gaming" });
                }
            }

            // Network cihazları
            else if (vendor.Contains("tp-link") || vendor.Contains("netgear") ||
                     vendor.Contains("d-link") || vendor.Contains("cisco"))
            {
                detailedInfo.IsNetworkDevice = true;
                detailedInfo.DeviceCategory = "Ağ Cihazı";
                detailedInfo.PossibleDevices.AddRange(new[] { "Router", "Access Point", "Switch", "Modem" });
            }

            // Oyun konsolları
            else if (vendor.Contains("sony") && deviceType.Contains("playstation"))
            {
                detailedInfo.DeviceCategory = "PlayStation";
                detailedInfo.OperatingSystem = "PlayStation OS";
                detailedInfo.PossibleDevices.AddRange(new[] { "PS4", "PS5", "PS4 Pro" });
            }

            else if (vendor.Contains("microsoft") && deviceType.Contains("xbox"))
            {
                detailedInfo.DeviceCategory = "Xbox";
                detailedInfo.OperatingSystem = "Xbox OS";
                detailedInfo.PossibleDevices.AddRange(new[] { "Xbox One", "Xbox Series X", "Xbox Series S" });
            }

            else if (vendor.Contains("nintendo"))
            {
                detailedInfo.DeviceCategory = "Nintendo";
                detailedInfo.OperatingSystem = "Nintendo OS";
                detailedInfo.PossibleDevices.AddRange(new[] { "Switch", "Switch Lite", "Wii U" });
            }
        }

        private string CleanVendorName(string vendor)
        {
            // Vendor ismini temizle ve standardize et
            vendor = vendor.Trim();

            // Yaygın eklentileri kaldır
            vendor = vendor.Replace(" Inc.", "")
                          .Replace(" Ltd.", "")
                          .Replace(" Corp.", "")
                          .Replace(" Co.", "")
                          .Replace(" LLC", "")
                          .Replace(", Inc", "")
                          .Replace(", Ltd", "")
                          .Trim();

            return vendor;
        }

        private string NormalizeMacAddress(string mac)
        {
            return mac.Replace(":", "-").Replace(".", "-").ToUpper();
        }

        private DetailedDeviceInfo ConvertToDetailedInfo(DeviceInfo cacheInfo)
        {
            return new DetailedDeviceInfo
            {
                CompanyName = cacheInfo.CompanyName,
                CompanyAddress = cacheInfo.CompanyAddress,
                CountryCode = cacheInfo.CountryCode,
                LastUpdated = DateTime.Parse(cacheInfo.DateUpdated ?? DateTime.Now.ToString())
            };
        }

        private DeviceInfo ConvertFromDetailedInfo(DetailedDeviceInfo detailed)
        {
            return new DeviceInfo
            {
                CompanyName = detailed.CompanyName,
                CompanyAddress = detailed.CompanyAddress,
                CountryCode = detailed.CountryCode,
                DateUpdated = detailed.LastUpdated.ToString(),
                Applications = detailed.TechnicalSpecs
            };
        }

        private void LoadDeviceCache()
        {
            try
            {
                if (File.Exists(cacheFilePath))
                {
                    string json = File.ReadAllText(cacheFilePath);
                    deviceCache = JsonSerializer.Deserialize<Dictionary<string, DeviceInfo>>(json) ?? new Dictionary<string, DeviceInfo>();
                }
            }
            catch { }
        }

        private void SaveDeviceCache()
        {
            try
            {
                string json = JsonSerializer.Serialize(deviceCache, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(cacheFilePath, json);
            }
            catch { }
        }

        // Detaylı rapor oluştur
        public string GenerateDetailedReport(DetailedDeviceInfo deviceInfo)
        {
            var report = new StringBuilder();

            report.AppendLine("=== DETAYLI CİHAZ BİLGİSİ ===");
            report.AppendLine($"🏭 Üretici: {deviceInfo.CompanyName}");
            report.AppendLine($"📱 Cihaz Tipi: {deviceInfo.DeviceCategory}");

            if (!string.IsNullOrEmpty(deviceInfo.SpecificModel))
                report.AppendLine($"🔧 Model: {deviceInfo.SpecificModel}");

            if (!string.IsNullOrEmpty(deviceInfo.OperatingSystem))
                report.AppendLine($"💻 İşletim Sistemi: {deviceInfo.OperatingSystem}");

            report.AppendLine($"🌍 Ülke: {deviceInfo.CountryCode}");
            report.AppendLine($"📍 Adres: {deviceInfo.CompanyAddress}");

            // Cihaz özellikleri
            report.AppendLine("\n=== CİHAZ ÖZELLİKLERİ ===");
            report.AppendLine($"📱 Mobil Cihaz: {(deviceInfo.IsMobile ? "Evet" : "Hayır")}");
            report.AppendLine($"💻 Bilgisayar: {(deviceInfo.IsComputer ? "Evet" : "Hayır")}");
            report.AppendLine($"🌐 Ağ Cihazı: {(deviceInfo.IsNetworkDevice ? "Evet" : "Hayır")}");
            report.AppendLine($"🖥️ Sanal Makine: {(deviceInfo.IsVirtualMachine ? "Evet" : "Hayır")}");
            report.AppendLine($"🍎 Apple Cihazı: {(deviceInfo.IsAppleDevice ? "Evet" : "Hayır")}");
            report.AppendLine($"📱 Samsung Cihazı: {(deviceInfo.IsSamsungDevice ? "Evet" : "Hayır")}");

            // Olası cihazlar
            if (deviceInfo.PossibleDevices.Any())
            {
                report.AppendLine("\n=== OLASI CİHAZLAR ===");
                foreach (var device in deviceInfo.PossibleDevices)
                {
                    report.AppendLine($"• {device}");
                }
            }

            // Teknik özellikler
            if (deviceInfo.TechnicalSpecs.Any())
            {
                report.AppendLine("\n=== TEKNİK ÖZELLİKLER ===");
                foreach (var spec in deviceInfo.TechnicalSpecs)
                {
                    report.AppendLine($"• {spec}");
                }
            }

            report.AppendLine($"\n🔍 OUI: {deviceInfo.OUI}");
            report.AppendLine($"🕐 Son Güncelleme: {deviceInfo.LastUpdated:dd.MM.yyyy HH:mm}");

            return report.ToString();
        }
    }
}

// Add this line to close the last #region block in the file
#endregion
