using System;
using System.Text;
using System.Collections.Generic;
using Newtonsoft.Json;


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
        public dict MakeDict(string str)
        {
            //Console.WriteLine("get dict:" + str);
            dict dict_dict = JsonConvert.DeserializeObject<dict>(str);
            return dict_dict;
        }
    }
}