using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Slac_DataCollect
{
    internal static class Program
    {

        static System.Threading.Mutex mutex;
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            bool isRuning;
            mutex = new System.Threading.Mutex(true, "OnlyRunOneInstance_001", out isRuning); // 如果互斥锁成功创建，isRuning 将被设置为 false；如果互斥锁已经存在，isRuning 将被设置为 true

            if (isRuning)
            {
                //处理未捕获的异常   
                Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
                //处理UI线程异常   
                Application.ThreadException += Application_ThreadException;
                //处理非UI线程异常   
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                Application.Run(new MainForm());                
            }
            else
            {
                MessageBox.Show("程序已经在运行中！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }            
        }

        /// <summary>
        /// 释放互斥锁
        /// </summary>
        public static void ReleaseMutex()
        {
            if (mutex != null)
            {
                mutex.ReleaseMutex();
                mutex.Dispose();
                mutex = null;
            }
        }

        /// <summary>
        /// UI异常
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            string strCurTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff");
            string msg = @"D:\crash.txt";
            WriteFile(msg, strCurTime + " " + e.Exception.StackTrace + "******\r\n");
            MessageBox.Show("线程异常,关闭程序,错误详见路径D:\\crash.txt");
            System.Environment.Exit(1);
        }

        /// <summary>
        /// 异常捕获
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            string strCurTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff");
            string msg = @"D:\crash.txt";
            WriteFile(msg, strCurTime + " " + e.ExceptionObject.ToString() + "-----\r\n");
            //WriteFile(msg, strCurTime + " " + e.IsTerminating.ToString() + "-----\r\n");
            MessageBox.Show("未知异常,关闭程序,错误详见路径D\\crash.txt");
            System.Environment.Exit(1);
        }

        /// <summary>
        /// 写文件
        /// </summary>
        /// <param name="path"></param>
        /// <param name="strValue"></param>
        static void WriteFile(string path, string strValue)
        {
            try
            {
                if (!File.Exists(path))
                {
                    using (FileStream fileStream = new FileStream(path, FileMode.Create, FileAccess.Write))
                    {
                        fileStream.Close();
                    }
                }
                using (StreamWriter file = new StreamWriter(path, true))
                {
                    file.Write(strValue);//直接追加文件末尾，不换行
                    file.Flush();
                    file.Close();
                    file.Dispose();
                }
            }
            catch
            {
            }
        }
    }
}
