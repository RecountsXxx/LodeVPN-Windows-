using FireSharp;
using FireSharp.Config;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using static System.Net.Mime.MediaTypeNames;

namespace LodeVpn
{
    /// <summary>
    /// Логика взаимодействия для LoginForm.xaml
    /// </summary>
    public partial class LoginForm : Window
    {
        MainWindow mainWindow;
        DownloadWindow downloadWindow = new DownloadWindow();


        private DispatcherTimer timer = new DispatcherTimer(DispatcherPriority.Send);
        private DispatcherTimer timerLoadingLogin = new DispatcherTimer(DispatcherPriority.Send);

        public static int Key = 102;

        private static LoginForm form;

        private FirebaseConfig config = new FirebaseConfig
        {
            AuthSecret = "TJBURMVXjCkdLPnhQoqOLkx2K8vG8fIdWxZmcm5K",
            BasePath = "https://vpnusersdb-default-rtdb.europe-west1.firebasedatabase.app/"
        };
        private FirebaseClient client;

        public LoginForm(bool initialize)
        {
            form = this;

            InitializeComponent();

            client = new FirebaseClient(config);

        }

        public LoginForm()
        {

            timer.Interval = TimeSpan.FromSeconds(6);
            timer.Tick += Timer_Tick;

            timerLoadingLogin.Interval = TimeSpan.FromSeconds(2);
            timerLoadingLogin.Tick += TimerLoading_Tick;
            form = this;

            InitializeComponent();

            client = new FirebaseClient(config);

            #region Remember me

            if (File.Exists("LogFiles.txt"))
            {
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
                            User user = response.Result.ResultAs<User>();
                            if (user.Password == password)
                            {
                                timer.Start();
                                downloadWindow.Show();
                                mainWindow = new MainWindow(user);
                                Close();
                         
  
         
                            }         
                        }
                        catch
                        {

                        }
                    }
                 
                }
            }
          

            
            #endregion

            //User user = new User();
            //user.Created = DateTime.Now;
            //user.UsedInternet = 1;
            //user.Name = "Bogdanqwe";
            //user.DaysForFreePlan = DateTime.Now;
            //user.IsPremium = true;
            //user.Gmail = "SADASD@gmail.com";
            //user.Password = "1234";
            //user.DaysSubscibe = 30;
            //user.DayBuySubcribe = new DateTime(2022,11,1);
            //var result = client.SetAsync(@"vpnusersdb/" + "prem", user);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            downloadWindow.Close();
            mainWindow.Show();
            timer.Stop();

        }
        private void TimerLoading_Tick(object sender, EventArgs e)
        {
            loginBtn.Visibility = Visibility.Visible;
            progressBarLoading.Visibility = Visibility.Collapsed;
            mainWindow.Show();
            Close();
            timerLoadingLogin.Stop();

        }

        #region Interner connection
        [DllImport("wininet.dll")]
        private extern static bool InternetGetConnectedState(out int Description, int ReservedValue);

        public static bool IsConnectedToInternet()
        {
            int Desc;
            return InternetGetConnectedState(out Desc, 0);
        }
        #endregion

        #region Window buttons
        private void closeButton_Click(object sender, RoutedEventArgs e)
        {

            Environment.Exit(0);
        }

        private void hideButton_Click(object sender, RoutedEventArgs e)
        {

            //help
            WindowState = WindowState.Minimized;
        }


        private void Drag(object sender, RoutedEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                LoginForm.form.DragMove();
            }
        }
        #endregion

        #region Text
        private void txtUser_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtUser.BorderBrush = System.Windows.Media.Brushes.DarkGray;
            textInvalid.Visibility = Visibility.Hidden;
        }

        private void txtPass_PasswordChanged(object sender, RoutedEventArgs e)
        {
            txtPass.BorderBrush = System.Windows.Media.Brushes.DarkGray;
            textInvalid.Visibility = Visibility.Hidden;
        }

        #endregion

        #region Login button
        private void LoginBtn_Click(object sender, RoutedEventArgs e)
        {
            if (IsConnectedToInternet())
            {
                try
                {

                    var response = client.GetAsync((@"vpnusersdb/" + txtUser.Text));
                    User user = response.Result.ResultAs<User>();
                    if (user != null)
                    {
                        if (txtPass.Password == user.Password)
                        {
                            progressBarLoading.Visibility = Visibility.Visible;
                            loginBtn.Visibility = Visibility.Collapsed;
                            mainWindow = new MainWindow(user);
                            timerLoadingLogin.Start();

                            textInvalid.Visibility = Visibility.Collapsed;
                            textInvalid.Foreground = System.Windows.Media.Brushes.LightGreen;
                            textInvalid.Text = "Succefull!";
                            if (checkBoxRememberMe.IsChecked == true)
                            {
                                using (StreamWriter writer = new StreamWriter("LogFiles.txt", false))
                                {
                                    writer.WriteLine("True");
                                    writer.WriteLine(user.Name);
                                    writer.WriteLine(user.Password);
                                }
                            }
                        }
                        else
                        {
                            txtPass.BorderBrush = System.Windows.Media.Brushes.Red;
                            txtUser.BorderBrush = System.Windows.Media.Brushes.Red;
                            textInvalid.Visibility = Visibility;
                        }
                    }
                    else
                    {
                        txtPass.BorderBrush = System.Windows.Media.Brushes.Red;
                        txtUser.BorderBrush = System.Windows.Media.Brushes.Red;
                        textInvalid.Visibility = Visibility;
                    }
                }
                catch
                {

                }
            }
            else
            {
                txtPass.BorderBrush = System.Windows.Media.Brushes.DarkGray;
                txtUser.BorderBrush = System.Windows.Media.Brushes.DarkGray;
                textInvalid.Visibility = Visibility.Visible;
                textInvalid.Text = "No internet connection!";
            }

        }
        #endregion

       
  


    #region Encrypt Decrypt

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

        #region TextBlock click
        private void ForgotPassword_MouseEnter(object sender, MouseEventArgs e)
        {

        }
        private void CreateAccount_MouseEnter(object sender, MouseEventArgs e)
        {

        }
        #endregion
    }

}
