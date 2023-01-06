
using Firebase.Auth;
using FireSharp;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
namespace LodeVpn
{
    /// <summary>
    /// Логика взаимодействия для LoginForm.xaml
    /// </summary>
    public partial class LoginForm : Window
    {
        private MainWindow mainWindow;
        private DownloadWindow downloadWindow = new DownloadWindow();


        private DispatcherTimer timer = new DispatcherTimer(DispatcherPriority.Send);
        private DispatcherTimer timerLoadingLogin = new DispatcherTimer(DispatcherPriority.Send);

        public static int Key = 102;

        private static LoginForm form;

        private FireSharp.Config.FirebaseConfig config = new FireSharp.Config.FirebaseConfig
        {
            AuthSecret = "TJBURMVXjCkdLPnhQoqOLkx2K8vG8fIdWxZmcm5K",
            BasePath = "https://vpnusersdb-default-rtdb.europe-west1.firebasedatabase.app/"
        };
        private FirebaseClient client;
        private FirebaseAuthProvider provider = new FirebaseAuthProvider(new FirebaseConfig("4mdEj7Nj6jotaEuulLIJb7YKIHRZY"));


        public LoginForm()
        {

            timer.Interval = TimeSpan.FromSeconds(6);
            timer.Tick += Timer_Tick;

            timerLoadingLogin.Interval = TimeSpan.FromSeconds(2);
            timerLoadingLogin.Tick += TimerLoading_Tick;

            form = this;

            client = new FirebaseClient(config);

            InitializeComponent();

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
        }

        #region Timers
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
        #endregion

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
            else
            {
                txtPass.BorderBrush = System.Windows.Media.Brushes.DarkGray;
                txtUser.BorderBrush = System.Windows.Media.Brushes.DarkGray;
                textInvalid.Visibility = Visibility.Visible;
                textInvalid.Text = "No internet connection!";
            }
        }
        #endregion

        //SDELAT
        #region TextBlock click
        private void ForgotPassword_MouseEnter(object sender, MouseEventArgs e)
        {
            Process.Start("http://lodevpn.zzz.com.ua/forgotPassword.html");
        }
        private void CreateAccount_MouseEnter(object sender, MouseEventArgs e)
        {
            Process.Start("http://lodevpn.zzz.com.ua/register.html");
        }
        #endregion
    }

}
