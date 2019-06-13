using System.Net.Sockets;
using System.Net;
using System;
namespace SocketServer
{
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