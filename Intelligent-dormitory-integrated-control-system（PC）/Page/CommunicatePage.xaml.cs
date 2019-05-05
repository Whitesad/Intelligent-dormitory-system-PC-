using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using System.Threading;
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
    public sealed partial class CommunicatePage : Page
    {
        Sock sock;
        ChatBoxTool _chat_box_tool;

        public CommunicatePage()
        {
            this.InitializeComponent();
            _chat_box_tool = new ChatBoxTool(ChatBox);
            //ChatBox.ScriptNotify += ChatBox_ScriptNotify; //js 与 C#通信
        }
        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            sock.UserInputText = TextSend.Text;
            sock.send();
            _chat_box_tool.Send("http://www.jf258.com/uploads/2013-07-21/073810328.jpg", "whitesad", TextSend.Text, DateTime.Now.ToString());
            TextSend.Text = "";
        }
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            //这个e.Parameter是获取传递过来的参数，应该再次之前判断这个参数是否为null的，我偷懒了
            this.sock = (Sock)e.Parameter;
            //sock.SetTextOutput(TextReceiver);
            sock.SetTextOutput(ChatBox);
            sock.start();
        }
    }
}
