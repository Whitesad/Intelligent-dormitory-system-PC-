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
using SocketServer;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Intelligent_dormitory_integrated_control_system_PC_
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    /// 
    
    public sealed partial class RegisterPage : Page
    {
        private Sock sock;
        public RegisterPage()
        {
            this.InitializeComponent();
        }
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            //这个e.Parameter是获取传递过来的参数，其实大家应该再次之前判断这个参数是否为null的，我偷懒了
            this.sock = (Sock)e.Parameter;
        }

        private void RegisterSubmit_Click(object sender, RoutedEventArgs e)
        {
            ToastController toast;
            string userName = RegisterUserNameTextBox.Text;
            string passWord01 = RegisterPassWordTextBox01.Password;
            string passWord02 = RegisterPassWordTextBox02.Password;
            if (passWord01 != passWord02)
            {
                toast = new ToastController("PassWord is not exactly the same!");
                toast.Show();
            }
            else if(passWord01.Length>=32)
            {
                toast = new ToastController("PassWord is too long!");
                toast.Show();
            }
            else
            {
                Status status = sock.Register(userName, passWord01);
                if (status == Status.REGISTER_AC)
                {
                    toast = new ToastController("Register Accepted!");
                    toast.Show();
                }
                else if (status == Status.SAME_NAME)
                {
                    toast = new ToastController("Register Failure!The username is existed!");
                    toast.Show();
                }
                else if (status == Status.REGISTER_ERROR)
                {
                    toast = new ToastController("REGISTER ERROR!");
                    toast.Show();
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Frame root = Window.Current.Content as Frame;
            root.Navigate(typeof(MainPage));
        }
    }
}
