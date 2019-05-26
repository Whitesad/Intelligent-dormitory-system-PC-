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
using System.Text.RegularExpressions;
using Windows.UI.Core;
using FtpExplorer;

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
            if (TextSend.Text == string.Empty)
                return;
            sock.UserInputText = TextSend.Text;
            sock.send();
            _chat_box_tool.Send("http://www.jf258.com/uploads/2013-07-21/073810328.jpg", sock.getuserName(), TextSend.Text, DateTime.Now.ToString());
            TextSend.Text = "";
        }

        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            MySplitView.IsPaneOpen = !MySplitView.IsPaneOpen;
        }
        private void IconsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ShareListBoxItem.IsSelected) {

            }
            else if (FavoritesListBoxItem.IsSelected) {

            }
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

        /// <summary>         
        /// 获取Img的路径         
        /// </summary>         
        /// <param name="htmlText">Html字符串文本</param>        
        /// <returns>以数组形式返回图片路径</returns>        
        private static string[] GetHtmlImageUrlList(string htmlText)
        {
            Regex regImg = new Regex(@"((http|ftp|https)://)(([a-zA-Z0-9\._-]+\.[a-zA-Z]{2,6})|([0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}))(:[0-9]{1,4})*(/[a-zA-Z0-9\&%_\./-~-]*)?", RegexOptions.IgnoreCase);
            //新建一个matches的MatchCollection对象 保存 匹配对象个数(img标签) 
            MatchCollection matches = regImg.Matches(htmlText);
            int i = 0;
            string[] sUrlList = new string[matches.Count];
            //遍历所有的img标签对象            
            foreach (Match match in matches)
            {
                //获取所有Img的路径src,并保存到数组中 
                sUrlList[i++] = match.ToString();
                //sUrlList[i++] = match.Groups[i].Value;
            }
            return sUrlList;
        }

        private void TextSend_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if(e.Key== Windows.System.VirtualKey.Enter)
            {
                SendButton_Click(TextSend, e);
            }
            
        }
    }
}
