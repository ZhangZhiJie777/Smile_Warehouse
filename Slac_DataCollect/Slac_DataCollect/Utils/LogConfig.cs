using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slac_DataCollect.Utils
{
    /// <summary>
    /// 类功能描述：Log日志
    /// </summary>
    public class LogConfig
    {
        static readonly object o = new object(); // 运行日志锁

        static readonly object objErr = new object(); // 错误日志锁
        /// <summary>
        /// 写运行日志
        /// </summary>
        /// <param name="msglocation">文件位置</param>
        /// <param name="sMsg">日志</param>
        public static void WriteRunLog(string msglocation, string sMsg)
        {
            try
            {
                lock (o)
                {
                    string sFileName, sPath;

                    sPath = AppDomain.CurrentDomain.SetupInformation.ApplicationBase + $"LogFile\\RunLog\\{msglocation.Trim()}";
                    sFileName = "\\" + DateTime.Now.ToString("yyyy-MM-dd") + ".log";
                    if (!Directory.Exists(sPath))
                    {
                        Directory.CreateDirectory(sPath);
                    }
                    using (StreamWriter w = File.AppendText(sPath + sFileName))
                    {
                        w.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff") + "  " + sMsg.ToString());
                        w.Close();
                    }
                }
            }
            catch
            { }
        }

        /// <summary>
        /// 写异常日志
        /// </summary>
        /// <param name="sMsg"></param>
        public static void WriteErrLog(string msglocation, string sMsg)
        {
            try
            {
                lock (objErr)
                {
                    string sFileName, sPath;

                    sPath = AppDomain.CurrentDomain.SetupInformation.ApplicationBase + $"LogFile\\ErrLog\\{msglocation.Trim()}";
                    sFileName = "\\" + DateTime.Now.ToString("yyyy-MM-dd") + ".log";
                    if (!Directory.Exists(sPath))
                    {
                        Directory.CreateDirectory(sPath);
                    }
                    using (StreamWriter w = File.AppendText(sPath + sFileName))
                    {
                        w.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff") + "  " + sMsg.ToString());
                        w.Close();
                    }
                }
            }
            catch
            { }
        }
    }
}
