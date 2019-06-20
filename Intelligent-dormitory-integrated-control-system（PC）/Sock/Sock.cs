using System.Net.Sockets;
using System.Net;
using System.Threading;
using System;
using System.Collections.Generic;

using Windows.UI.Xaml.Controls;

using Intelligent_dormitory_integrated_control_system_PC_;
using Intelligent_dormitory_integrated_control_system_PC_.ConstantVariable;
using System.Threading.Tasks;

using RSA;
namespace SocketServer
{
    using dict = Dictionary<String, String>;


    
    class Sock
    {
        private Socket sockServer;//所创建的socketServer
        private String hostIp;//目标IP
        private int port;//目标端口
        private Dict DictMaker = new Dict();//字典创造工具
        private RSA.RSA RsaMaker = new RSA.RSA(1024);


        private String local_ip, local_name;//本机IP、计算机名
        private string userName, passWord;
        private string PublicKey;

        public string UserInputText;
        private bool isSending = false;
        private TextBlock TextOutput;
        private ChatBoxTool _chat_box_tool;

        bool isConnect = false;
        
        public string getuserName()
        {
            return this.userName;
        }

        public Sock(string userName, string passWord, String hostIp, int port)
        {
            this.userName = userName;
            this.passWord = passWord;

            this.PublicKey = this.RsaMaker.ToPEM_PKCS1(true);
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
            this.PublicKey = this.RsaMaker.ToPEM_PKCS1(true);

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
            this.PublicKey = this.RsaMaker.ToPEM_PKCS1(true);

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

            try
            {
                Send(RegisterDict);
                sockServer.Receive(dict_bytes);
                dict_dict = DictMaker.MakeDict(dict_bytes);
                if (dict_dict["type"] == "REGISTER_MES")
                {
                    if (dict_dict["status"] == "AC")
                    {
                        
                        return Status.REGISTER_AC;
                    }
                    else if (dict_dict["status"] == "SAME_NAME")
                    {
                        return Status.SAME_NAME;
                    }
                    else if (dict_dict["status"] == "REGISTER_ERROR")
                    {
                        return Status.REGISTER_ERROR;
                    }
                }
            }
            catch
            {
                sockServer.Close();
                Toast("Server Connect Error!");
                return Status.CONNECT_ERROR;
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
            dict LoginRequestDict = DictMaker.MakeLoginRequestDict(this.RsaMaker.ToPEM_PKCS1(true));
            byte[] dict_bytes = new byte[2048];
            dict dict_dict = new dict();
            
            try
            {
                Send(LoginRequestDict);
                dict_dict = Recieve();

                if (dict_dict["type"] == "publickey")
                {
                    this.DictMaker.SetServerPublicKey(dict_dict["publickey"]);
                }

                dict LoginDict = DictMaker.MakeLoginDict(userName, passWord, local_ip, local_name);
                Send(LoginDict);

                dict_dict = Recieve();

                if (dict_dict["type"] == "LOGIN_MES")
                {
                    if (dict_dict["status"] == "AC")
                    {
                        ConstantVariable.FTPUsername = dict_dict["ftpusername"];
                        ConstantVariable.FTPPassword = dict_dict["ftppassword"];
                        return Status.LOGIN_AC;
                    }
                    else if (dict_dict["status"] == "NO_MEMSHIP")
                    {
                        return Status.NO_MEMSHIP;
                    }
                    else if (dict_dict["status"] == "WRONG_PASSWORD")
                    {
                        return Status.WRONG_PASSWORD;
                    }
                }
            }
            catch
            {
                sockServer.Close();
                Toast("Server Connect Error!");
                return Status.CONNECT_ERROR;

            }
            return Status.CONNECT_ERROR;
        }
        

        private void Send(dict dict_dict)
        {
            byte[] dict_bytes = new byte[2048];
            dict_bytes = DictMaker.MakeBytesDict(dict_dict);
            sockServer.Send(dict_bytes);
        }

        private void DecryptDict(dict dict_mes)
        {
            if (dict_mes.ContainsKey("content"))
                dict_mes["content"] = RsaMaker.DecodeOrNull(dict_mes["content"]);
            if (dict_mes.ContainsKey("ftpusername"))
                dict_mes["ftpusername"] = RsaMaker.DecodeOrNull(dict_mes["ftpusername"]);
            if (dict_mes.ContainsKey("ftppassword"))
                dict_mes["ftppassword"] = RsaMaker.DecodeOrNull(dict_mes["ftppassword"]);
        }
        private dict Recieve()//类型待议
        {
            byte[] dict_bytes = new byte[2048];
            sockServer.Receive(dict_bytes);
            dict dict_mes = DictMaker.MakeDict(dict_bytes);
            DecryptDict(dict_mes);
            return dict_mes;
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
                    //byte[] dict_bytes = new byte[2048];
                    //sockServer.Receive(dict_bytes);
                    //Console.WriteLine("before make");
                    //receive_dict = DictMaker.MakeDict(dict_bytes);
                    receive_dict = Recieve();
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
                    Thread.Sleep(1);
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
    
}