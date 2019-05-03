using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
//个人引用
using SocketServer;
using System.Net;
using System.Net.Sockets;

namespace Intelligent_dormitory_integrated_control_system_PC_
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private bool isInitialed = false;

        private const int port = 50000;
        string testHost = "";

        public MainPage()
        {
            this.InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            ToastController toastController;
            string userName = UserNameTextBox.Text;
            string passWord = PassWordTextBox.Password;

            if (!isInitialed)
            {
                IPAddress[] iPAddresses = Dns.GetHostAddresses(Dns.GetHostName());
                foreach (IPAddress iP in iPAddresses)
                {
                    if (iP.AddressFamily == AddressFamily.InterNetwork)
                    {
                        testHost = iP.ToString();
                        break;
                    }
                }
                toastController = new ToastController("TestHost:" + testHost);
                toastController.Show();
                isInitialed = true;
            }
            Sock sock = new Sock(userName, passWord, testHost, port);
            Status status = Status.NONE;
            status = sock.Login(userName, passWord);
            if (status == Status.LOGIN_AC)
            {
                Frame root = Window.Current.Content as Frame;
                root.Navigate(typeof(CommunicatePage),sock);
                //sock.start();
            }
            else if (status == Status.WRONG_PASSWORD)
            {
                toastController = new ToastController("Login Failure!Please check the password");
                toastController.Show();
            }
            else if (status == Status.NO_MEMSHIP)
            {
                toastController = new ToastController("No Memship!Please register and try again!");
                toastController.Show();
            }

        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            ToastController toastController;
            if (!isInitialed)
            {
                IPAddress[] iPAddresses = Dns.GetHostAddresses(Dns.GetHostName());
                foreach (IPAddress iP in iPAddresses)
                {
                    if (iP.AddressFamily == AddressFamily.InterNetwork)
                    {
                        testHost = iP.ToString();
                        break;
                    }
                }
                toastController = new ToastController("TestHost:" + testHost);
                toastController.Show();
                isInitialed = true;
            }
            Sock sock = new Sock(testHost, port);

            Frame root = Window.Current.Content as Frame;
            root.Navigate(typeof(RegisterPage),sock);
        }
    }
}
