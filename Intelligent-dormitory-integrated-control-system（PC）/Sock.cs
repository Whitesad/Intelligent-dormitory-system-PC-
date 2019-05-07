﻿using System.Net.Sockets;
using System.Net;
using System.Threading;
using System;
using System.Text;
using System.Collections.Generic;
using Newtonsoft.Json;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using Intelligent_dormitory_integrated_control_system_PC_;
using System.Threading.Tasks;

namespace SocketServer
{
    using dict = Dictionary<String, String>;

    class Dict
    {
        public dict MakeRegisterDict(String userName, String password, String local_ip, String local_name)
        {
            dict RegisterDict = new dict();
            RegisterDict.Add("username", userName);
            RegisterDict.Add("type", "REGISTER_MES");
            RegisterDict.Add("status", "register");
            RegisterDict.Add("ip", local_ip);
            RegisterDict.Add("localname", local_name);
            RegisterDict.Add("password", password);
            return RegisterDict;
        }
        public dict MakeLoginDict(String userName, String password, String local_ip, String local_name)
        {
            dict LoginDict = new dict();
            LoginDict.Add("username", userName);
            LoginDict.Add("type", "LOGIN_MES");
            LoginDict.Add("status", "login");
            LoginDict.Add("ip", local_ip);
            LoginDict.Add("localname", local_name);
            LoginDict.Add("password", password);
            return LoginDict;
        }
        public dict MakeTextDict(string userName, string content, string local_ip, string local_name)
        {
            dict TextDict = new dict();
            TextDict.Add("type", "TEXT_MES");
            TextDict.Add("content", content);
            TextDict.Add("ip", local_ip);
            TextDict.Add("localname", local_name);
            TextDict.Add("username", userName);
            return TextDict;
        }
        public byte[] MakeBytesDict(dict dict_dict)
        {
            String dict_str = "{";
            int count = 1;
            foreach (var item in dict_dict)
            {
                dict_str += (" \"" + item.Key + "\": ");
                dict_str += (" \"" + item.Value);
                if (count != dict_dict.Count)
                {
                    dict_str += "\" ,";
                }
                else
                {
                    dict_str += "\" }";
                }
                count++;
            }
            //Console.WriteLine(dict_str);
            return Encoding.UTF8.GetBytes(dict_str);
        }
        public dict MakeDict(byte[] dict_bytes)
        {
            string str = Encoding.UTF8.GetString(dict_bytes);
            //Console.WriteLine("get dict:" + str);
            dict dict_dict = JsonConvert.DeserializeObject<dict>(Encoding.UTF8.GetString(dict_bytes));
            return dict_dict;
        }
    }

    enum Status
    {
        NONE,
        LOGIN_AC,
        NO_MEMSHIP,
        QUERY_ERROR,
        WRONG_PASSWORD,
        CONNECT_ERROR,
        REGISTER_AC,
        SAME_NAME,
        REGISTER_ERROR
    }
    class Sock
    {
        private Socket sockServer;//所创建的socketServer
        private String hostIp;//目标IP
        private int port;//目标端口
        private Dict DictMaker = new Dict();//字典创造工具

        private String local_ip, local_name;//本机IP、计算机名
        string userName, passWord;

        public string UserInputText;
        private bool isSending = false;
        private TextBlock TextOutput;
        private ChatBoxTool _chat_box_tool;

        bool isConnect = false;


        public Sock(string userName, string passWord, String hostIp, int port)
        {
            this.userName = userName;
            this.passWord = passWord;

            this.sockServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            this.local_name = Dns.GetHostName();
            IPAddress[] iPAddresses = Dns.GetHostAddresses(Dns.GetHostName());
            foreach (IPAddress iP in iPAddresses)
            {
                if (iP.AddressFamily == AddressFamily.InterNetwork)
                {
                    this.local_ip = iP.ToString();
                    break;
                }
            }
            this.hostIp = hostIp;
            this.port = port;
        }
        public Sock()
        {
            this.sockServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            this.local_name = Dns.GetHostName();
            IPAddress[] iPAddresses = Dns.GetHostAddresses(Dns.GetHostName());
            foreach (IPAddress iP in iPAddresses)
            {
                if (iP.AddressFamily == AddressFamily.InterNetwork)
                {
                    this.local_ip = iP.ToString();
                    break;
                }
            }
        }

        public Sock(string hostIp, int port)
        {
            this.sockServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            this.local_name = Dns.GetHostName();
            IPAddress[] iPAddresses = Dns.GetHostAddresses(Dns.GetHostName());
            foreach (IPAddress iP in iPAddresses)
            {
                if (iP.AddressFamily == AddressFamily.InterNetwork)
                {
                    this.local_ip = iP.ToString();
                    break;
                }
            }
            this.hostIp = hostIp;
            this.port = port;
        }

        public void SetHost(String hostIp, int port)
        {
            this.sockServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }
        private void Toast(string text)
        {
            ToastController toast=new ToastController(text);
            this.Invoke(() =>
            {
                toast.Show();
            });
        }
        public Status Register(String userName, String passWord)
        {
            try
            {
                sockServer.Connect(hostIp, port);
                Toast("Successfully Connect the Server:\n" + this.hostIp);
            }
            catch
            {
                sockServer.Close();
                Toast("Server Connect Failure!");
                return Status.CONNECT_ERROR;
            }


            dict RegisterDict = DictMaker.MakeRegisterDict(userName, passWord, local_ip, local_name);
            byte[] dict_bytes = new byte[2048];
            dict dict_dict = new dict();
            for (int i = 1; i <= 200; i++)
            {
                
                Send(RegisterDict);
                try
                {
                    sockServer.Receive(dict_bytes);
                    dict_dict = DictMaker.MakeDict(dict_bytes);
                    if (dict_dict["type"] == "REGISTER_MES")
                    {
                        if (dict_dict["status"] == "AC")
                            return Status.REGISTER_AC;
                        else if (dict_dict["status"] == "SAME_NAME")
                        {
                            return Status.SAME_NAME;
                        }
                        else if (dict_dict["status"] == "REGISTER_ERROR")
                        {
                            return Status.REGISTER_ERROR;
                        }
                    }
                    Thread.Sleep(5);
                }
                catch
                {
                    sockServer.Close();
                    Toast("Server Connect Error!");
                    return Status.CONNECT_ERROR;
                }
                
            }
            return Status.CONNECT_ERROR;
        }

        public Status Login(String userName, String passWord)
        {
            this.userName = userName;
            this.passWord = passWord;
            try
            {
                sockServer.Connect(hostIp, port);
                Toast("Successfully Connect the Server:\n" + this.hostIp);
            }
            catch
            {
                sockServer.Close();
                Toast("Server Connect Failure!");
                return Status.CONNECT_ERROR;
            }
            
            dict LoginDict = DictMaker.MakeLoginDict(userName, passWord, local_ip, local_name);
            byte[] dict_bytes = new byte[2048];
            dict dict_dict = new dict();
            for (int i = 1; i <= 200; i++)
            {
                try
                {
                    Send(LoginDict);
                    sockServer.Receive(dict_bytes);
                    dict_dict = DictMaker.MakeDict(dict_bytes);
                    if (dict_dict["type"] == "LOGIN_MES")
                    {
                        if (dict_dict["status"] == "AC")
                            return Status.LOGIN_AC;
                        else if (dict_dict["status"] == "NO_MEMSHIP")
                        {
                            return Status.NO_MEMSHIP;
                        }
                        else if (dict_dict["status"] == "WRONG_PASSWORD")
                        {
                            return Status.WRONG_PASSWORD;
                        }
                    }
                    Thread.Sleep(5);
                }
                catch
                {
                    sockServer.Close();
                    Toast("Server Connect Error!");
                    return Status.CONNECT_ERROR;

                }
                
            }
            
            return Status.CONNECT_ERROR;
        }
        private void Send(dict dict_dict)
        {
            byte[] dict_bytes = new byte[2048];
            dict_bytes = DictMaker.MakeBytesDict(dict_dict);
            sockServer.Send(dict_bytes);
        }
        private dict Recieve()//类型待议
        {
            byte[] dict_bytes = new byte[2048];
            sockServer.Receive(dict_bytes);
            return DictMaker.MakeDict(dict_bytes);
        }
        public void start()
        {
            Task thread_listen = new Task(Thread_Listen);
            thread_listen.Start();
            Task thread_out = new Task(Thread_Out);
            thread_out.Start();
            //Thread thread_listen = new Thread(Thread_Listen);
            //thread_listen.Start();
            //Thread thread_out = new Thread(Thread_Out);
            //thread_out.Start();
        }

        public async void Invoke(Action action, Windows.UI.Core.CoreDispatcherPriority Priority = Windows.UI.Core.CoreDispatcherPriority.Normal)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Priority, () => { action(); });
        }

        private void Print(dict dict_dict)
        {
            //if (dict_dict["username"] == this.userName)
            //    return;
            if (dict_dict["type"] == "TEXT_MES")
            {
                if (dict_dict.ContainsKey("content"))
                {
                    string Out = "";
                    Out += (dict_dict["localname"] + " " + dict_dict["username"] + " " + dict_dict["time"] + "\n");
                    Out += (dict_dict["content"] + "\n");

                    this.Invoke(() =>
                    {
                        this._chat_box_tool.Receive("http://www.jf258.com/uploads/2013-07-21/073810328.jpg", dict_dict["username"], dict_dict["content"], dict_dict["time"]);
                    });
                    //this.TextOutput.Text += (dict_dict["localname"] + " " + dict_dict["username"] + " " + dict_dict["time"]+"\n");
                    //this.TextOutput.Text += (dict_dict["content"]+"\n");

                }
            }
        }

        public void SetTextOutput(TextBlock textOut)
        {
            this.TextOutput = textOut;
            //textOut.Text = "TextOut更改成功";
        }
        public void SetTextOutput(WebView ChatBox)
        {
            _chat_box_tool = new ChatBoxTool(ChatBox);
        }

        private void Thread_Listen()
        {
            dict receive_dict;
            while (true)
            {
                try
                {
                    byte[] dict_bytes = new byte[2048];
                    sockServer.Receive(dict_bytes);
                    // Console.WriteLine("before make");
                    receive_dict = DictMaker.MakeDict(dict_bytes);
                    Print(receive_dict);
                    Thread.Sleep(20);
                    //Console.WriteLine("after make");
                }
                catch
                {
                    Thread_Listen_Debug();//打印BUG
                }
            }
        }

        private void Thread_Listen_Debug()
        {
            Console.WriteLine("Listen error!");
        }

        public void send()
        {
            this.isSending = true;
        }

        private void Thread_Out()
        {
            while (true)
            {
                try
                {
                    if (isSending)
                    {
                        Send(DictMaker.MakeTextDict(this.userName, this.UserInputText, this.local_ip, this.local_name));
                        isSending = false;
                    }
                }
                catch
                {
                    Thread_Out_Debug();
                }
            }
        }
        private void Thread_Out_Debug()
        {
            Console.WriteLine("Thread Out Error!");
        }
    }
    class Program
    {
        //服务相关常量类
        private static IPAddress ip = IPAddress.Parse("10.1.139.101");
        private static String hostIp = "10.0.139.101";
        private const int port = 50000;
        //用户交互类
        private static String userName;
        private static String passWord;

        static void test()
        {
            //byte[] dict_bytes = new byte[2048];
            //dict_bytes = Encoding.UTF8.GetBytes("{\"type\":\"LOGIN_MES\"}");
            //string str = Encoding.UTF8.GetString(dict_bytes);
            //Console.WriteLine(str);
            //dict dict_dict = JsonConvert.DeserializeObject<dict>(str);
            return;
        }

        static void KMain(string[] args)
        {
            //test();
            string testHost = "";
            IPAddress[] iPAddresses = Dns.GetHostAddresses(Dns.GetHostName());
            foreach (IPAddress iP in iPAddresses)
            {
                if (iP.AddressFamily == AddressFamily.InterNetwork)
                {
                    testHost = iP.ToString();
                    break;
                }
            }
            //Sock sock = new Sock(hostIp, port);
            Status loginStatus = Status.NONE;
            Sock sock = new Sock(userName, passWord, testHost, port);
            string select;
            while (loginStatus != Status.LOGIN_AC)
            {
                Console.WriteLine("Please Select to Login or Register.1)Login   2)Register");
                select = Console.ReadLine();
                if (select == "1")
                {
                    userName = Program.GetUserName();
                    passWord = Program.GetPassWord();
                    loginStatus = sock.Login(userName, passWord);
                    if (loginStatus == Status.LOGIN_AC)
                    {
                        Console.WriteLine("Login Accepted!");
                        sock.start();
                        break;
                    }
                    else if (loginStatus == Status.WRONG_PASSWORD)
                    {
                        Console.WriteLine("Login Failure!Please check the password");
                    }
                    else if (loginStatus == Status.NO_MEMSHIP)
                    {
                        Console.WriteLine("No Memship!Please register and try again!");
                    }
                }
                else if (select == "2")
                {
                    userName = Program.GetUserName();
                    passWord = Program.GetPassWord();
                    loginStatus = sock.Register(userName, passWord);
                    if (loginStatus == Status.REGISTER_AC)
                    {
                        Console.WriteLine("Register Accepted!");
                    }
                    else if (loginStatus == Status.SAME_NAME)
                    {
                        Console.WriteLine("Register Failure!The username is existed!");
                    }
                    else if (loginStatus == Status.REGISTER_ERROR)
                    {
                        Console.WriteLine("REGISTER ERROR!");
                    }
                }
                else
                {
                    Console.WriteLine("Please Check The Select Option.");
                }
            }

            //Program.test();

            /*byte[] b = Encoding.ASCII.GetBytes("whitesad");
            Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);
            serverSocket.Connect(ip, port);
            serverSocket.Listen(10);
            serverSocket.Send(Encoding.ASCII.GetBytes("whitesad"));*/
            return;
        }
        private static String GetUserName()//get username safely
        {
            String userName = "";
            Console.WriteLine("Input The UserName:");
            while (userName.Length < 1 || userName.Length > 32)
            {
                userName = Console.ReadLine();
            }
            return userName;
        }
        private static String GetPassWord()
        {
            String passWord = "";
            Console.WriteLine("Input The PassWord:");

            while (passWord.Length < 1 || passWord.Length > 32)
            {
                passWord = Console.ReadLine();
            }
            return passWord;
        }
    }
}