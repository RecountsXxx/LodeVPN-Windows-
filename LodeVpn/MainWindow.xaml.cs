using FireSharp.Config;
using FireSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Windows.Media.Animation;
using MessageBox = System.Windows.Forms.MessageBox;
using System.Security.Cryptography;
using System.Reflection;
using Microsoft.Win32;
using System.Xml.Linq;
using System.Diagnostics.Contracts;
using FireSharp.Response;

namespace LodeVpn
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>



    public partial class MainWindow : Window
    {

        private bool autoRun;
        private bool saveMe;

        private static string FolderPath => string.Concat(Directory.GetCurrentDirectory(), "\\VPN");
        private string host = "PL226.vpnbook.com";

        private static MainWindow Window;

        private System.Windows.Forms.ContextMenuStrip contextMenu = new System.Windows.Forms.ContextMenuStrip();
        private System.Windows.Forms.NotifyIcon notify = new NotifyIcon();

        private DispatcherTimer timer = new DispatcherTimer(DispatcherPriority.Send);
        private DispatcherTimer timerInternet = new DispatcherTimer(DispatcherPriority.Send);
        private DateTime dateTime = new DateTime(0, 0);

        public User user = new User();


        private FirebaseConfig config = new FirebaseConfig
        {
            AuthSecret = "TJBURMVXjCkdLPnhQoqOLkx2K8vG8fIdWxZmcm5K",
            BasePath = "https://vpnusersdb-default-rtdb.europe-west1.firebasedatabase.app/"
        };
        private FirebaseClient client;




        public MainWindow(User user)
        {
            //когда кто то покупает премиум то isPremium = true, daysubribe = сколько дней подписка, а daybuysubcribe = день покупки 



            client = new FirebaseClient(config);

            this.user = user;

            InitializeComponent();

            #region Other

            Window = this;
            comboBoxHostName.SelectedIndex = 3;
            #endregion

            #region Timer interval
            timer.Interval = new TimeSpan(0, 0, 0, 1);
            timer.Tick += Timer_Tick;

            timerInternet.Interval = new TimeSpan(0, 1, 0);
            timerInternet.Tick += Timer_TickInternet;
            timerInternet.Start();
            #endregion

            #region Get ip-adress
            string pubIp = new System.Net.WebClient().DownloadString("https://api.ipify.org");
            ipAdressText.Text = pubIp;
            #endregion

            #region ContextMenu
            contextMenu.Items.Add("Show", System.Drawing.Image.FromFile(@"..\..\Images\visual.png"), new EventHandler(showMenuItem));
            contextMenu.Items.Add("Disconnect", System.Drawing.Image.FromFile(@"..\..\\Images\disconnect.png"), new EventHandler(disconnectMenuItem));
            contextMenu.Items.Add("Exit", System.Drawing.Image.FromFile(@"..\..\Images\exitTray.png"), new EventHandler(exitMenuItem));
            notify.ContextMenuStrip = contextMenu;
            #endregion

            #region AutoRun per windows
            if (Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run\", "systems", null) == null)
                LaunchAppWitStart.IsChecked = false;
            else
                LaunchAppWitStart.IsChecked = true;
            #endregion

            #region Save Me
            using (StreamReader reader = new StreamReader("LogFiles.txt", false))
            {
                string boolean = reader.ReadLine();
                string name = reader.ReadLine();
                string password = reader.ReadLine();
                if (boolean == "True")
                {
                    try
                    {
                        var response = client.GetAsync((@"vpnusersdb/" + name));
                        if (user.Password == password)
                        {
                            SaveMeToggleButton.IsChecked = true;
                        }      
                    }
                    catch
                    {

                    }
                }
                else
                {
                    SaveMeToggleButton.IsChecked = false ;
                }

            }
            #endregion

            #region Loading data per account




            loginAccount.Text = user.Gmail;

            DateTime expires = new DateTime();

            expires = user.Created;
            if (user.IsPremium == true)
            {
                expires = user.DayBuySubcribe;
                expires = expires.AddDays(user.DaysSubscibe);
            }
            else
                expires = expires.AddDays(10);
            user.DaysForFreePlan = expires;
            expiresAccount.Text = "Expiries on: " + user.DaysForFreePlan.ToShortDateString();
            usedAccount.Text = "You used " + user.UsedInternet + "MB out of 10GB internet.";
            var result = client.Update(@"vpnusersdb/" + user.Name, user);

            if (user.IsPremium == true)
            {
                planAccount.Text = "Plan: " + "Premium Plan";
                premiumHost0.IsEnabled = true;
                premiumHost1.IsEnabled = true;
                premiumHost2.IsEnabled = true;
            }
            else
            {
                planAccount.Text = "Plan: " + "Trial period";
                premiumHost0.IsEnabled = false;
                premiumHost1.IsEnabled = false;
                premiumHost2.IsEnabled = false;
            }


            if (user.IsPremium == true)
            {
                if (DateTime.Now >= user.DaysForFreePlan)
                {
                    //перекидыввать на страницу покупки
                    expiresAccount.Text = "Subscription ended.";
                    MessageBox.Show("Unfortunately, your subscription is over.", "Information", MessageBoxButtons.OK, (MessageBoxIcon)MessageBoxImage.Information); ;
                    planAccount.Text = "Plan: " + "Subscription ended.";
                    expiresAccount.Text = "Subscription ended.";
                    trailTextBlock.Visibility = Visibility.Visible;
                    onOffButton.IsEnabled = false;
                    premiumHost0.IsEnabled = false;
                    premiumHost1.IsEnabled = false;
                    premiumHost2.IsEnabled = false;
                }

            }
            else
            {
                if (DateTime.Now >= user.DaysForFreePlan)
                {
                    //перекидыввать на страницу покупки
                    MessageBox.Show("Unfortunately, your trial period is over.", "Information", MessageBoxButtons.OK, (MessageBoxIcon)MessageBoxImage.Information); ;
                    tabControl.SelectedIndex = 2;
                    planAccount.Text = "Plan: " + "Trial period is over.";
                    expiresAccount.Text = "Trail period ended.";
                    trailTextBlock.Visibility = Visibility.Visible;
                    onOffButton.IsEnabled = false;
                    premiumHost0.IsEnabled = false;
                    premiumHost1.IsEnabled = false;
                    premiumHost2.IsEnabled = false;
                }
                if (user.UsedInternet >= 10000)
                {
                    //перекидыввать на страницу покупки
                    MessageBox.Show("Unfortunately, you spent all the Internet available to you in the trial period.", "Information", MessageBoxButtons.OK, (MessageBoxIcon)MessageBoxImage.Information); ;
                    tabControl.SelectedIndex = 2;
                    planAccount.Text = "Plan: " + "The Internet in use has ended.";
                    expiresAccount.Text = "Trail period ended.";
                    trailTextBlock.Visibility = Visibility.Visible;
                    onOffButton.IsEnabled = false;
                }
            }
            #endregion
        }

        #region Timer Tick
        private void Timer_Tick(object sender, EventArgs e)
        {
            dateTime = dateTime.AddSeconds(1);
            timerLabel.Content = "Duration: " + dateTime.ToLongTimeString();
            timerLabel.Foreground = Brushes.White;
        }
        private void Timer_TickInternet(object sender, EventArgs e)
        {
            //PerformanceCounterCategory performanceCounterCategory = new PerformanceCounterCategory("Network Interface");
            //string instance = "WAN [PPP_SLIP] Interface"; //либо так
            //PerformanceCounter performanceCounterSent = new PerformanceCounter("Network Interface", "Bytes Sent/sec", instance);
            //PerformanceCounter performanceCounterReceived = new PerformanceCounter("Network Interface", "Bytes Received/sec", instance);

            //    Console.WriteLine("bytes sent: {0}k\tbytes received: {1}k", performanceCounterSent.NextValue() / 1024, performanceCounterReceived.NextValue() / 1024);
        }
        #endregion 

        #region VPN Functions

        public void Connect()
        {


            if (!Directory.Exists(FolderPath))
                Directory.CreateDirectory(FolderPath);

            var sb = new StringBuilder();
            sb.AppendLine("[VPN]");
            sb.AppendLine("MEDIA=rastapi");
            sb.AppendLine("Port=VPN2-0");
            sb.AppendLine("Device=WAN Miniport (IKEv2)");
            sb.AppendLine("DEVICE=vpn");
            sb.AppendLine("PhoneNumber=" + host);

            File.WriteAllText(FolderPath + "\\VpnConnection.pbk", sb.ToString());

            sb = new StringBuilder();
            sb.AppendLine("rasdial \"VPN\" " + "vpnbook" + " " + "rxtasfh" + " /phonebook:\"" + FolderPath +
                          "\\VpnConnection.pbk\"");

            File.WriteAllText(FolderPath + "\\VpnConnection.bat", sb.ToString());

            var newProcess = new Process
            {
                StartInfo =
                {
                    FileName = FolderPath + "\\VpnConnection.bat",
                    WindowStyle = ProcessWindowStyle.Normal
                }
            };
            newProcess.Start();
            newProcess.WaitForExit();

        }
        public void Disconnect()
        {
            File.WriteAllText(FolderPath + "\\VpnDisconnect.bat", "rasdial /d");

            var newProcess = new Process
            {
                StartInfo =
                {
                    FileName = FolderPath + "\\VpnDisconnect.bat",
                    WindowStyle = ProcessWindowStyle.Hidden
                }
            };

            newProcess.Start();
            newProcess.WaitForExit();
        }

        private void funcComboBox()
        {
            if (comboBoxHostName.SelectedIndex == 0)
            {
                ipAdressText.Text = "51.68.152.226";
                ipAdressTextTwo.Text = "Protected IP: ";

            }
            if (comboBoxHostName.SelectedIndex == 1)
            {
                ipAdressText.Text = "94.23.57.8";
                ipAdressTextTwo.Text = "Protected IP: ";
            }
            if (comboBoxHostName.SelectedIndex == 2)
            {
                ipAdressText.Text = "198.7.62.204";
                ipAdressTextTwo.Text = "Protected IP: ";
            }
            if (comboBoxHostName.SelectedIndex == 3)
            {
                ipAdressText.Text = "198.7.58.147";
                ipAdressTextTwo.Text = "Protected IP: ";
            }
            if (comboBoxHostName.SelectedIndex == 4)
            {
                ipAdressText.Text = "192.99.37.222";
                ipAdressTextTwo.Text = "Protected IP: ";
            }
            if (comboBoxHostName.SelectedIndex == 5)
            {
                ipAdressText.Text = "198.27.69.198";
                ipAdressTextTwo.Text = "Protected IP: ";
            }
            if (comboBoxHostName.SelectedIndex == 6)
            {
                ipAdressText.Text = "37.187.158.97";
                ipAdressTextTwo.Text = "Protected IP: ";
            }
            if (comboBoxHostName.SelectedIndex == 7)
            {
                ipAdressText.Text = "94.23.57.8";
                ipAdressTextTwo.Text = "Protected IP: ";
            }
        }

        private void comboBoxHostName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //TYT POMENYAT
            if (comboBoxHostName.SelectedIndex == 7)
            {
                host = "PL226.vpnbook.com";

            }
            if (comboBoxHostName.SelectedIndex == 3)
            {
                host = "us1.vpnbook.com";
            }
            if (comboBoxHostName.SelectedIndex == 4)
            {
                host = "us2.vpnbook.com";
            }
            if (comboBoxHostName.SelectedIndex == 5)
            {
                host = "ca222.vpnbook.com";
            }
            if (comboBoxHostName.SelectedIndex == 6)
            {
                host = "ca198.vpnbook.com";
            }


            if (comboBoxHostName.SelectedIndex == 0)
            {
                host = "DE4.vpnbook.com";
            }
            if (comboBoxHostName.SelectedIndex == 1)
            {
                host = "fr1.vpnbook.com";
            }
            if (comboBoxHostName.SelectedIndex == 2)
            {
                host = "fr8.vpnbook.com";
            }

        }
        #endregion

        #region On Off VPN
        [DllImport("wininet.dll")]
        private extern static bool InternetGetConnectedState(out int Description, int ReservedValue);

        public static bool IsConnectedToInternet()
        {
            int Desc;
            return InternetGetConnectedState(out Desc, 0);
        }

        private void OnOffVpn_Click(object sender, RoutedEventArgs e)
        {
            if (IsConnectedToInternet())
            {
                if (onOffButton.IsChecked == true)
                {
                    dateTime = new DateTime(0, 0);
                    connectRadioBtn.Background = new SolidColorBrush(Color.FromRgb(5, 196, 33));
                    disconnectRadioBtn.Background = new SolidColorBrush(Color.FromRgb(56, 56, 56));
                    disconConnecLabel.Content = "Connected";
                    disconConnecLabel.Foreground = new SolidColorBrush(Color.FromRgb(5, 196, 33));

                    Task.Factory.StartNew(new Action(Connect));
                    timer.Start();
                    comboBoxHostName.IsReadOnly = true;
                    funcComboBox();
                    timerInternet.Start();

                }
                else
                {
                    connectRadioBtn.Background = new SolidColorBrush(Color.FromRgb(56, 56, 56));
                    disconnectRadioBtn.Background = Brushes.Red;
                    disconConnecLabel.Content = "Disconnected";
                    disconConnecLabel.Foreground = Brushes.Red;
                    Task.Factory.StartNew(new Action(Disconnect));
                    timer.Stop();
                    string pubIp = new System.Net.WebClient().DownloadString("https://api.ipify.org");
                    ipAdressText.Text = pubIp;
                    ipAdressTextTwo.Text = "    Public IP: ";
                    comboBoxHostName.IsReadOnly = false;
                    timerLabel.Content = "";
                    timerInternet.Stop();
                }
            }
            else
            {
                onOffButton.IsChecked = false;
                timerLabel.Content = "   No internet.";
                timerLabel.Foreground = Brushes.Red;
                connectRadioBtn.Background = new SolidColorBrush(Color.FromRgb(56, 56, 56));
                ipAdressTextTwo.Text = "    Public IP: ";
                disconnectRadioBtn.Background = Brushes.Red;
                disconConnecLabel.Content = "Disconnected";
                disconConnecLabel.Foreground = Brushes.Red;
                timer.Stop();
                comboBoxHostName.IsReadOnly = false;
                timerInternet.Stop();
            }
           
        }
        #endregion

        #region MenuItem Task bar
        private void showMenuItem(object sender, EventArgs e)
        {
            ShowInTaskbar = true;
            Show();
        }
        private void disconnectMenuItem(object sender, EventArgs e)
        {

        }
        private void exitMenuItem(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }
        private void Notify_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Show();
                ShowInTaskbar = true;

            }
        }
        #endregion

        #region Window buttons
        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            notify.Visible = false;
            Environment.Exit(0);
        }

        private void hideButton_Click(object sender, RoutedEventArgs e)
        {

            //help
            notify.Icon = new System.Drawing.Icon(@"..\..\Images\LogoIcons.ico");
            notify.Visible = true;
            notify.Text = "Free VPN";
            ShowInTaskbar = false;
            notify.MouseClick += Notify_MouseClick; ;
            notify.ShowBalloonTip(2, "Free VPN", "VPN will run in the background", ToolTipIcon.Info);
            Hide();
        }

        private void Drag(object sender, RoutedEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                MainWindow.Window.DragMove();
            }
        }
        private void Window_Closed(object sender, EventArgs e)
        {
            Disconnect();
        }
        #endregion

        #region Links
        private void TabItem_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            LoginForm form = new LoginForm(true);
            form.Show();
            Close();

        }

        private void TextBlock_Gmail(object sender, MouseButtonEventArgs e)
        {

        }
        private void TextBlock_Telegram(object sender, MouseButtonEventArgs e)
        {
            Process.Start("https://t.me/+xqhnK555QPo1YTYy");
        }
        #endregion

        #region settings


        private void LaunchAppWitStart_Click(object sender, RoutedEventArgs e)
        {

            if (autoRun == false)
            {
                setAutoRun(true);
                autoRun = true;
            }
            else
            {
                setAutoRun(false);
                autoRun = false;
            }
        }
        public bool setAutoRun(bool autorun)
        {
            const string name = "systems";
            string ExePath = Assembly.GetExecutingAssembly().Location;
            RegistryKey registry;

            try
            {
                registry = Registry.CurrentUser.CreateSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run\\");
                if (autorun)
                {
                    registry.SetValue(name, ExePath);
                }
                else
                {
                    registry.DeleteValue(name);
                }
                registry.Flush();
                registry.Close();
            }
            catch
            {
                return false;
            }
            return true;
        }
        public void setSaveMe(bool saveME)
        {
            if (saveMe)
            {
                using (StreamWriter writer = new StreamWriter("LogFiles.txt", false))
                {
                    writer.WriteLine("True");
                    writer.WriteLine(user.Name);
                    writer.WriteLine(user.Password);
                }
            }
            else
            {
                using (StreamWriter writer = new StreamWriter("LogFiles.txt", false))
                {
                    writer.WriteLine("qwe");
                }
            }
        }
        private void SaveMeToggleButton_Click(object sender, RoutedEventArgs e)
        {


            if (saveMe == true)
            {
                setSaveMe(false);
                saveMe = false;
                MessageBox.Show("true");
            }
            else
            {
                setSaveMe(true);
                saveMe = true;
                MessageBox.Show("false");
            }
        }
        #endregion


        private void upgradeBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        #region Encrypt and decrypt
        public static byte[] RSAEncrypt(byte[] DataToEncrypt, RSAParameters RSAKeyInfo, bool DoOAEPPadding)
        {
            try
            {
                byte[] encryptedData;
                using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider())
                {
                    RSA.ImportParameters(RSAKeyInfo);
                    encryptedData = RSA.Encrypt(DataToEncrypt, DoOAEPPadding);
                }
                return encryptedData;
            }
            catch (CryptographicException e)
            {
                Console.WriteLine(e.Message);

                return null;
            }
        }

        public static byte[] RSADecrypt(byte[] DataToDecrypt, RSAParameters RSAKeyInfo, bool DoOAEPPadding)
        {
            try
            {
                byte[] decryptedData;
                using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider())
                {
                    RSA.ImportParameters(RSAKeyInfo);
                    decryptedData = RSA.Decrypt(DataToDecrypt, DoOAEPPadding);
                }
                return decryptedData;
            }
            catch (CryptographicException e)
            {
                Console.WriteLine(e.ToString());

                return null;
            }
        }
        #endregion


    
    }
}
