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

namespace LodeVpn
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>



    public partial class MainWindow : Window
    {
        private static string FolderPath => string.Concat(Directory.GetCurrentDirectory(), "\\VPN");
        private string host = "PL226.vpnbook.com";

        private static MainWindow Window;

        private System.Windows.Forms.ContextMenuStrip contextMenu = new System.Windows.Forms.ContextMenuStrip();
        private System.Windows.Forms.NotifyIcon notify = new NotifyIcon();

        private DispatcherTimer timer = new DispatcherTimer(DispatcherPriority.Send);
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
            this.user = user;

            if(DateTime.Now >= user.DaysForFreePlan)
            {
                //Истек пробный период
            }
            if(user.UsedInternet >= 10000)
            {
                //Потратили весь интренет, тоесть и пропадает пробный период
            }
            InitializeComponent();

            #region Other
            client = new FirebaseClient((FireSharp.Interfaces.IFirebaseConfig)client);

            Window = this;
            comboBoxHostName.SelectedIndex = 0;
            #endregion

            #region Timer
            timer.Interval = new TimeSpan(0, 0, 0, 1);
            timer.Tick += Timer_Tick;
            #endregion

            #region IP
            string pubIp = new System.Net.WebClient().DownloadString("https://api.ipify.org");
            ipAdressText.Text =  pubIp;
            #endregion

            #region ContextMenu
            contextMenu.Items.Add("Show", System.Drawing.Image.FromFile(@"..\..\Images\visual.png"), new EventHandler(showMenuItem));
            contextMenu.Items.Add("Disconnect", System.Drawing.Image.FromFile(@"..\..\\Images\disconnect.png"), new EventHandler(disconnectMenuItem));
            contextMenu.Items.Add("Exit", System.Drawing.Image.FromFile(@"..\..\Images\exitTray.png"), new EventHandler(exitMenuItem));
            notify.ContextMenuStrip = contextMenu;
            #endregion

            #region Loading data per account


            loginAccount.Text = user.Gmail;

            if (user.IsPremium == true)
            {
                planAccount.Text = "Plan: " + "Premium Plan";
                planAccount.Foreground = Brushes.Yellow;
            }
            else 
                planAccount.Text = "Plan: " + "Free Plan";
            DateTime expires = new DateTime();
            expires = user.Created;
            expires= expires.AddDays(10);
            user.DaysForFreePlan = expires;
            expiresAccount.Text = "Expiries on: " + expires.ToShortDateString();
            usedAccount.Text = "You used " + user.UsedInternet + "MB out of 10GB internet.";

            #endregion

        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            dateTime = dateTime.AddSeconds(1);
            timerLabel.Content = "Duration: " + dateTime.ToLongTimeString();
        }

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
                    WindowStyle = ProcessWindowStyle.Hidden
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
            if (comboBoxHostName.SelectedIndex == 0)
            {
                host = "PL226.vpnbook.com";

            }
            if (comboBoxHostName.SelectedIndex == 1)
            {
                host = "DE4.vpnbook.com";
            }
            if (comboBoxHostName.SelectedIndex == 2)
            {
                host = "us1.vpnbook.com";
            }
            if (comboBoxHostName.SelectedIndex == 3)
            {
                host = "us2.vpnbook.com";
            }
            if (comboBoxHostName.SelectedIndex == 4)
            {
                host = "ca222.vpnbook.com";
            }
            if (comboBoxHostName.SelectedIndex == 5)
            {
                host = "ca198.vpnbook.com";
            }
            if (comboBoxHostName.SelectedIndex == 6)
            {
                host = "fr1.vpnbook.com";
            }
            if (comboBoxHostName.SelectedIndex == 7)
            {
                host = "fr8.vpnbook.com";
            }

        }
        #endregion

        #region On Off VPN


        private void OnOffVpn_Click(object sender, RoutedEventArgs e)
        {
            if (onOffButton.IsChecked == true)
            {
                connectRadioBtn.Background = new SolidColorBrush(Color.FromRgb(5, 196, 33));
                disconnectRadioBtn.Background = new SolidColorBrush(Color.FromRgb(56, 56, 56));
                disconConnecLabel.Content = "Connected";
                disconConnecLabel.Foreground = new SolidColorBrush(Color.FromRgb(5, 196, 33));

                Task.Factory.StartNew(new Action(Connect));
                timer.Start();
                comboBoxHostName.IsReadOnly = true;
                funcComboBox();


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
                ipAdressText.Text =  pubIp;
                ipAdressTextTwo.Text = "    Public IP: ";
                comboBoxHostName.IsReadOnly = false;
                timerLabel.Content = "";
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

        #region Exit account
    
        #endregion

        private void TabItem_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
                LoginForm loginForm = new LoginForm();
                loginForm.Show();
                Close();
            
        }

        private void TextBlock_Gmail(object sender, MouseButtonEventArgs e)
        {
            
        }
        private void TextBlock_Telegram(object sender, MouseButtonEventArgs e)
        {
            Process.Start("https://t.me/+xqhnK555QPo1YTYy");
        }
    }

}
