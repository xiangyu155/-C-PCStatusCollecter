using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Threading;
using System.Runtime.InteropServices;
using System.Net;
using System.IO;

namespace ConsoleApp1
{

    class Program
    {
        public class CheckComputerFreeState
        {
            /// <summary>
            /// 创建结构体用于返回捕获时间
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            struct LASTINPUTINFO
            {
                /// <summary>
                /// 设置结构体块容量
                /// </summary>
                [MarshalAs(UnmanagedType.U4)]
                public int cbSize;

                /// <summary>
                /// 抓获的时间
                /// </summary>
                [MarshalAs(UnmanagedType.U4)]
                public uint dwTime;
            }

            [DllImport("user32.dll")]
            private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);
            /// <summary>
            /// 获取键盘和鼠标没有操作的时间
            /// </summary>
            /// <returns>用户上次使用系统到现在的时间间隔，单位为秒</returns>
            public static long GetLastInputTime()
            {
                LASTINPUTINFO vLastInputInfo = new LASTINPUTINFO();
                vLastInputInfo.cbSize = Marshal.SizeOf(vLastInputInfo);
                if (!GetLastInputInfo(ref vLastInputInfo))
                {
                    return 0;
                }
                else
                {
                    var count = Environment.TickCount - (long)vLastInputInfo.dwTime;
                    var icount = count / 1000;
                    return icount;
                }
            }

        }

        public static string GetLocalIp()
        {
            ///获取本地的IP地址
            string AddressIP = string.Empty;
            foreach (IPAddress _IPAddress in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            {
                if (_IPAddress.AddressFamily.ToString() == "InterNetwork")
                {
                    AddressIP = _IPAddress.ToString();
                }
            }
            return AddressIP;
        }
        //读取主机自定义配置信息的函数
        public static string loadHostInfo()
        {
            try
            {
                string ss = "";
                string url = "hostInfo.txt";//目标文件路径
                using (StreamReader sr = new StreamReader(url))
                {
                    string line;
                    // 从文件读取并显示行，直到文件的末尾 
                    while ((line = sr.ReadLine()) != null)
                    {
                        Console.WriteLine(line);
                        ss = line;
                    }
                }
                return ss;
            }
            catch (Exception ex)
            {
                return "UnknownHost";
            }
        }
        //读取主机自定义配置信息的函数
        public static long loadFreeTime()
        {
            try
            {
                string ss = "";
                string url = "freeTime.txt";//目标文件路径
                using (StreamReader sr = new StreamReader(url))
                {
                    string line;
                    // 从文件读取并显示行，直到文件的末尾 
                    while ((line = sr.ReadLine()) != null)
                    {
                        Console.WriteLine(line);
                        ss = line;
                    }
                }
                return long.Parse(ss);
            }
            catch (Exception ex)
            {
                return 5;
            }
        }
        //读取主机自定义配置信息的函数
        public static string loadServerURL()
        {
            try
            {
                string ss = "";
                string url = "serverURL.txt";//目标文件路径
                using (StreamReader sr = new StreamReader(url))
                {
                    string line;
                    // 从文件读取并显示行，直到文件的末尾 
                    while ((line = sr.ReadLine()) != null)
                    {
                        Console.WriteLine(line);
                        ss = line;
                    }
                }
                return ss;
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        //保存log的txt文档函数
        public static string saveToTxt(string str)
        {
            StreamWriter sw = File.AppendText("log.txt");
            try
            {
                sw.WriteLine(str);
                sw.Close();
                return "";
            }
            catch (Exception ex)
            {
                return ex.Message.ToString();
            }

        }
        //写入监控数据库D:\IIS\PCStatusCollecter\updateRecords.asp
        public static string saveToServer(string str1, string str2, string str3, string str4, string str5, string str6)
        { 
            try
            {
                String requestStr = "";
                requestStr = str1 + "updateRecords.asp?recordTime=";
                requestStr += str2+ "&operatedTime=";
                requestStr += str3 + "&hostInfo=";
                requestStr += str4 + "&hostName=";
                requestStr += str5 + "&hostIP=";
                requestStr += str6;
                //Get请求中请求参数等直接拼接在url中
                WebRequest request = WebRequest.Create(requestStr);
                //返回对Internet请求的响应
                WebResponse resp = request.GetResponse();
                return requestStr;
            }
            catch (Exception ex)
            {
                StreamWriter sw = File.AppendText("errorLog.txt");
                sw.WriteLine(DateTime.Now.ToString("yyyyMMddHHmmss")+" "+ex.Message.ToString());
                sw.Close();
                return ex.Message.ToString();
            }
        }
        //
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        /// 设置窗体的显示与隐藏  
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool ShowWindow(IntPtr hWnd, uint nCmdShow);
        // 隐藏控制台  
        /// </summary>  
        /// <param name="ConsoleTitle">控制台标题(可为空,为空则取默认值)</param>  
        public static void hideConsole(string ConsoleTitle = "")
        {
            ConsoleTitle = String.IsNullOrEmpty(ConsoleTitle) ? Console.Title : ConsoleTitle;
            IntPtr hWnd = FindWindow("ConsoleWindowClass", ConsoleTitle);
            if (hWnd != IntPtr.Zero)
            {
                ShowWindow(hWnd, 0);
            }
        }

        static void Main(string[] args)
        {
            //隐藏
            hideConsole();
            //初始化主机信息和判断参数
            String hostInfoStr = loadHostInfo();
            String hostNameStr = Dns.GetHostName();//获取主机名字
            String hostIPStr = GetLocalIp();//获取主机IP
            String serverURL = loadServerURL();//获取服务端地址   updateRecords.asp?recordTime=20210912152226&spareTime=3&hostInfo=xiangyu155&hostName=DESKTOP-E9K4VEO&hostIP=192.168.1.100
            String tempTime = "";
            String logStr = "";//log
            long freeTime = loadFreeTime();//无键盘鼠标事件动作的判断时长
            Boolean isExcuted = false;//初始化执行锁
            long eclipsedSecond = 0;//计时器已经执行的时间
            Timer t = null;//初始化计时器
            t = new Timer((o) =>
            {
                try {
                    var result = CheckComputerFreeState.GetLastInputTime();
                    //显示空闲秒数
                    Console.WriteLine(result.ToString() + " "+ eclipsedSecond.ToString());
                    //判断是否在freeTime时间内有键鼠按下或移动
                    if (result < freeTime && eclipsedSecond > freeTime)
                    {
                        tempTime = DateTime.Now.ToString("yyyyMMddHHmmss");
                        logStr = tempTime;
                        logStr += ",";
                        logStr += freeTime.ToString();
                        logStr += ",";
                        logStr += hostInfoStr;
                        logStr += ",";
                        logStr += hostNameStr;
                        logStr += ",";
                        logStr += hostIPStr;
                        Console.WriteLine(logStr);
                        //写入日志
                        saveToTxt(logStr);
                        try
                        {
                            //写入监控数据库 
                            saveToServer(serverURL, tempTime, freeTime.ToString(), hostInfoStr, hostNameStr, hostIPStr);
                        }
                        catch (Exception ex1)
                        {
                            //显示报错信息
                            Console.WriteLine(ex1.ToString());
                        }
                        eclipsedSecond = 0;
                    }
                    eclipsedSecond++;//记录逝去的秒数
                    
                }
                catch (Exception ex)
                {
                    //显示报错信息
                    Console.WriteLine(ex.ToString());
                }
                
                //t.Dispose();
                
            }, null, 1000, 1000);

            Console.ReadLine();
        }
    }
}
