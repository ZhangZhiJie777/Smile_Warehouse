using RabbitMQ.Client;
using RabbitMQ.Client.Framing.Impl;
using Slac_DataCollect.DatabaseSql.DBModel;
using Slac_DataCollect.DatabaseSql.DBOper;
using Slac_DataCollect.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Slac_DataCollect.Modules
{
    public class Slac_DataReceiveToMQModule
    {

        private static Timer checkConnectTimer;                   // 定时器，检查连接状态
        private static readonly object lockObject = new object(); // 线程锁

        private ConcurrentQueue<Tuple<IPEndPoint, byte[], int>> dataQueue; // 数据队列

        public static event Func<string, string, bool> DataReturnEvent;           // 用于返回数据，更新界面
        public static event Action<string, bool> UpdateConnectStateEvent;  // 用于返回连接状态，更新界面
        public event Action<string, int> UpdateDataStateEvent;             // 用于返回数据状态，更新界面

        private static bool PlcConnectState;              // PLC连接状态
        private static bool MQConnectState;               // MQ连接状态

        private static bool isRunAcceptData;      // 是否正在发送数据到MQ

        private static DataComm data_receiver;

        private static string LocalIP;     // UDP通讯IP
        private static string Port;        // UDP通讯端口2000

        private static string FilePath;    // 用于获取文件路径，文件名（保存数据）
        public static string isToRemoteMQ; // 是否发送到远程MQ

        #region RabbitMQ参数
        private static string LineID;      // 线体ID（消息队列名称）
        public static string MQserver;     // RabbitMQ服务器IP
        private static string MQexchange;  // RabbitMQ交换机名称                
        private static string Password;    // RabbitMQ密码

        private static IConnection conn = null;            // RabbitMQ连接
        private static IModel channel = null;              // RabbitMQ通道
        private static IBasicProperties properties = null; // RabbitMQ消息属性
        ///现场应用
        private static ConnectionFactory factory = new ConnectionFactory() { HostName = MQserver, Port = 5672, UserName = MQexchange, Password = "slac1028", AutomaticRecoveryEnabled = true };

        //本地调试
        //private static ConnectionFactory factory = new ConnectionFactory() { HostName = "localhost", Port = 5672, UserName = "guest", Password = "guest", AutomaticRecoveryEnabled = true };

        #endregion

        public Slac_DataReceiveToMQModule()
        {

        }

        /// <summary>
        /// 获取配置参数
        /// </summary>
        public static void GetParamConfig()
        {
            try
            {
                DBSystemConfig dbSystemConfig = new DBSystemConfig();   //配置类
                DBOper.Init();
                DBOper db = new DBOper();
                List<DBSystemConfig> list = db.QueryList(dbSystemConfig);

                //UDP
                LocalIP = list.Find(e => e.Name.Trim() == "RcvToMQ_LocalIP&Port").Value.Trim().Split('|')[0];
                Port = list.Find(e => e.Name.Trim() == "RcvToMQ_LocalIP&Port").Value.Trim().Split('|')[1];

                //RabbitMQ
                MQserver = list.Find(e => e.Name.Trim() == "RcvToMQ_MQserver").Value.Trim().Split('|')[0];
                MQexchange = list.Find(e => e.Name.Trim() == "RcvToMQ_MQserver").Value.Trim().Split('|')[1];
                LineID = list.Find(e => e.Name.Trim() == "RcvToMQ_MQserver").Value.Trim().Split('|')[2];
                Password = list.Find(e => e.Name.Trim() == "RcvToMQ_MQserver").Value.Trim().Split('|')[3];

                FilePath = list.Find(e => e.Name.Trim() == "RcvToMQ_FilePath").Value.Trim();
                isToRemoteMQ = list.Find(e => e.Name.Trim() == "RcvToMQ_isToRemoteMQ").Value.Trim();
            }
            catch (Exception ex)
            {
                LogConfig.WriteErrLog("ReceiveMQ", $"获取数据库配置参数异常Error：{ex.ToString()}");
                DataReturnEvent?.Invoke("label3", $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff")}  获取数据库配置参数失败，请检查数据库配置！");

            }
        }

        /// <summary>
        /// 启动数据接收服务
        /// </summary>
        public void StartCollectData()
        {
            try
            {
                LogConfig.WriteRunLog("ReceiveMQ", "———— ———— ———— ———— ———— ———— ————\r\n");
                LogConfig.WriteRunLog("ReceiveMQ", "ReceiveMQ模块开始");
                //MQ服务设置                
                bool MQstate = SettingMQ(MQexchange, "line" + LineID);                

                dataQueue = new ConcurrentQueue<Tuple<IPEndPoint, byte[], int>>();
                data_receiver = new DataComm(dataQueue);
                isRunAcceptData = true; // 将发送MQ状态置为true                               

                bool isconnect = data_receiver.StartDATAServer(LocalIP, Convert.ToInt32(Port)); //启动数据接收服务
                                                                                                //data_receiver.EventAcceptData += EventAcceptData;                                                                                                

                #region 每隔2秒定时监控PLC、MQ连接状态、数据状态

                // 初始化PLC、MQ连接状态
                if (MQstate)
                {
                    MQConnectState = (conn.IsOpen && channel.IsOpen);
                    UpdateConnectStateEvent?.Invoke("RcvMQ", MQConnectState);
                }
                else { UpdateConnectStateEvent?.Invoke("RcvMQ", false); }
                PlcConnectState = DataComm.isRun;
                UpdateConnectStateEvent?.Invoke("PLCState", PlcConnectState);


                // 设置定时器，每隔2秒监控一次连接状态，数据状态
                checkConnectTimer = new Timer(CheckConnection, null, TimeSpan.Zero, TimeSpan.FromSeconds(2));

                LogConfig.WriteRunLog("ReceiveMQ", $"监听服务启动成功");
                #endregion                

                while (true)
                {
                    if (dataQueue.TryDequeue(out Tuple<IPEndPoint, byte[], int> Tuple))
                    {
                        EventAcceptData(Tuple);   // 循环，集合中有数据时，调用数据接收事件
                    }
                    else
                    {
                        Thread.Sleep(100);        // 集合为空时，等待100ms
                        if (!DataComm.isRun)      // UDP通信停止，等待集合发送清空，把接收发送MQ的状态置为 false,跳出循环
                        {
                            isRunAcceptData = false;
                            break;
                        }
                    }
                }
                LogConfig.WriteRunLog("ReceiveMQ", "ReceiveMQ模块结束");
                
            }
            catch (Exception ex)
            {
                UpdateConnectStateEvent?.Invoke("RcvMQ", false);
                UpdateConnectStateEvent?.Invoke("PLCState", false);
                LogConfig.WriteErrLog("ReceiveMQ", $"ReceiveMQ模块启动数据采集异常：{ex.Message}\r\n");                
            }

        }

        /// <summary>
        /// 数据接收事件：接收数据后发送到消息队列
        /// </summary>
        /// <param name="t"></param>
        private void EventAcceptData(Tuple<System.Net.IPEndPoint, byte[], int> t)
        {
            // 报存log
            string exepath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            if (Properties.Settings.Default.IsSaveLog)
            {
                string ipstr = t.Item1.Address.ToString();
                System.IO.File.AppendAllText(System.IO.Path.GetDirectoryName(exepath) + "\\Rcvlog.txt", "from  " + ipstr + " @ " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "\r\n");
            }

            DateTime time_Plc_Receive = DateTime.Now;
            // 发送到消息队列
            // mq_send.Send(t.Item2, time_Plc_Receive.ToString("yyyy-MM-dd HH:mm:ss.fff") + "@" + t.Item1.Address.ToString());
            byte[] buffer_body = t.Item2;
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1)); // 当地时区
            double RcvTimeMs = Math.Round((time_Plc_Receive - startTime).TotalMilliseconds);
            Byte[] RcvTimeBytes = BitConverter.GetBytes(RcvTimeMs);

            byte[] msgBody = new byte[buffer_body.Length + RcvTimeBytes.Length];
            Buffer.BlockCopy(RcvTimeBytes, 0, msgBody, 0, RcvTimeBytes.Length);
            Buffer.BlockCopy(buffer_body, 0, msgBody, RcvTimeBytes.Length, buffer_body.Length);

            if (Properties.Settings.Default.IsSendMQ) // 是否直接远端传输
            {
                SendtoMQ(MQexchange, "line" + LineID + "_bak", msgBody);
            }
            else
            {
                SendtoMQ(MQexchange, "line" + LineID, msgBody);
            }

            //  --2024.6.15 cwz 发送到bak的队列。停用，在数据解析阶段再传输到远端 。
            //if(checkBox4.Checked)
            //{
            //    SendtoMQ_2(MQexchange, "line" + LineID + "_bak", msgBody);
            //}

            // 显示日志
            if (Properties.Settings.Default.IsShowLog)
            {
                string showData = "收到数据： " + t.Item2.Length.ToString() + "  @  " + time_Plc_Receive.ToString("yyyy-MM-dd HH:mm:ss.fff") + "  @  " + t.Item1.Address.ToString();
                DataReturnEvent?.Invoke("label1", showData);
            }
            // 保存bin文件
            if (Properties.Settings.Default.IsSaveBin)
            {
                if (FilePath == "0")
                {
                    FilePath = System.IO.Path.GetDirectoryName(exepath);
                }
                string sPath = FilePath + @"\" + DateTime.Now.ToString("yyyyMMdd");
                if (!Directory.Exists(sPath))
                {
                    Directory.CreateDirectory(sPath);
                }

                string fileName = sPath + @"\" + LineID + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss_") + RcvTimeMs.ToString() + ".sbin";
                bool saveRet = FileHelper.ByteToFile(msgBody, fileName);
            }
        }

        /// <summary>
        /// 发送到MQ
        /// </summary>
        /// <param name="MQ_exchange">交换机名称</param>
        /// <param name="line_ID">队列名称（路由键）</param>
        /// <param name="SendMsg">发送数据（byte数组)</param>
        /// <returns></returns>
        private void SendtoMQ(string MQ_exchange, string line_ID, byte[] SendMsg)
        {
            try
            {
                if (conn.IsOpen)
                {
                    channel.BasicPublish(MQ_exchange, line_ID, properties, SendMsg);
                    if (Properties.Settings.Default.IsShowLog)
                    {
                        string showData = "传输成功！ @ " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                        DataReturnEvent?.Invoke("label2", showData);
                    }
                    //return "0";
                }
                else
                {
                    string showData = "传输失败！ @ " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    DataReturnEvent?.Invoke("label2", showData);
                    //return "error:100";
                }
            }
            catch (Exception ex)
            {
                string showData = "异常" + ex.ToString() + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                DataReturnEvent?.Invoke("label2", showData);
                LogConfig.WriteRunLog("ReceiveMQ", $"RabbitMQ发送异常：{ex.Message}\r\n");
                //return "error:101";
            }
        }

        /// <summary>
        /// MQ服务设置：建立连接，创建通道，声明交换机，队列，绑定路由键
        /// </summary>
        /// <param name="MQ_exchange">交换机名称</param>
        /// <param name="line_ID">队列名称（路由键）</param>
        /// <returns></returns>
        public static bool SettingMQ(string MQ_exchange, string line_ID)
        {
            try
            {
                if (!string.IsNullOrEmpty(MQ_exchange) && !string.IsNullOrEmpty(line_ID))
                {
                    factory = new ConnectionFactory() { HostName = MQserver, Port = 5672, UserName = MQexchange, Password = Password, AutomaticRecoveryEnabled = true };
                    LogConfig.WriteRunLog("ReceiveMQ", $"RabbitMQ服务：IP：{MQserver}，端口：{5672}，用户名：{MQexchange}，密码：{Password}");

                    conn = factory.CreateConnection();
                    channel = conn.CreateModel();
                    properties = channel.CreateBasicProperties();
                    properties.DeliveryMode = 2; // 持久化

                    // 声明交换机
                    channel.ExchangeDeclare(MQexchange, ExchangeType.Direct, true, false, null);

                    // 声明队列
                    channel.QueueDeclare(MQexchange + "_line" + LineID, true, false, false, null); //队列名称：交换机名称+line+线体ID

                    // 绑定队列和交换机
                    channel.QueueBind(MQexchange + "_line" + LineID, MQexchange, "line" + LineID); // 第三个参数为交换机的路由键                    
                    
                    LogConfig.WriteRunLog("ReceiveMQ", $"RabbitMQ服务启动成功：MQexchange：{MQexchange}，消息队列名称：{MQexchange + "_line" + LineID}，路由键：{"line" + LineID}");
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception EX)
            {
                LogConfig.WriteErrLog("ReceiveMQ", $"RabbitMQ服务启动异常：{EX.Message}\r\n");
                throw;
            }
        }

        /// <summary>
        /// 定时监控连接状态:PLC、MQ，若状态位变化，则更新状态
        /// </summary>
        public async void CheckConnection(object obj)
        {

            try
            {
                #region 监控连接状态

                // 更新 PLC连接状态
                if (PlcConnectState != DataComm.isRun)
                {
                    PlcConnectState = DataComm.isRun;
                    UpdateConnectStateEvent?.Invoke("PLCState", PlcConnectState); // 更新 PLC连接状态
                }

                // 更新 MQ连接状态
                if (MQConnectState != (conn.IsOpen && channel.IsOpen))
                {
                    MQConnectState = (conn.IsOpen && channel.IsOpen);
                    UpdateConnectStateEvent?.Invoke("RcvMQ", MQConnectState);     // 更新 MQ连接状态
                }

                #endregion

                #region 监控数据状态

                // 更新 PLC数据接收状态
                if (PlcConnectState && dataQueue.Count > 0)
                {
                    UpdateDataStateEvent?.Invoke("PlcCollectState", 1); // 若PLC通讯成功，且数据队列不为空
                }
                else if (PlcConnectState && dataQueue.Count == 0)
                {
                    UpdateDataStateEvent?.Invoke("PlcCollectState", 2); // 若PLC通讯成功，且数据队列为空
                }
                else if (!PlcConnectState)
                {
                    UpdateDataStateEvent?.Invoke("PlcCollectState", 0); // 若PLC通讯失败
                }

                // 更新 MQ数据发送状态
                if (MQConnectState && dataQueue.Count > 0)
                {
                    UpdateDataStateEvent?.Invoke("MQSendState", 1); // 若MQ连接成功，且数据队列不为空
                }
                else if (MQConnectState && dataQueue.Count == 0)
                {
                    UpdateDataStateEvent?.Invoke("MQSendState", 2); // 若MQ连接成功，且数据队列为空
                }
                else if (!MQConnectState)
                {
                    UpdateDataStateEvent?.Invoke("MQSendState", 0); // 若MQ连接失败
                }

                #endregion

            }
            catch (Exception ex)
            {
                UpdateConnectStateEvent?.Invoke("PLCState", false); // PLC连接状态
                UpdateConnectStateEvent?.Invoke("RcvMQ", false);    // MQ连接状态

                UpdateDataStateEvent?.Invoke("PlcCollectState", 3); // PLC数据异常
                UpdateDataStateEvent?.Invoke("MQSendState", 3);     // MQ数据发送异常

                LogConfig.WriteErrLog("ReceiveMQ", $"监控PLC、MQ连接状态异常：{ex.Message}\r\n");                
            }


        }

        /// <summary>
        /// 关闭RabbitMQ服务
        /// </summary>
        public static void StopMQ()
        {
            try
            {
                if (channel != null && channel.IsOpen)
                {
                    channel.Close();
                    channel.Dispose();
                    channel = null;
                    LogConfig.WriteRunLog("ReceiveMQ", "RabbitMQ通道关闭成功");
                }
                if (conn != null && conn.IsOpen)
                {
                    conn.Close();
                    conn.Dispose();
                    conn = null;
                    LogConfig.WriteRunLog("ReceiveMQ", "RabbitMQ连接关闭成功");
                }
            }
            catch (Exception ex)
            {
                LogConfig.WriteRunLog("ReceiveMQ", $"RabbitMQ服务关闭异常：{ex.Message}\r\n");
            }


        }

        /// <summary>
        /// 关闭所有服务
        /// </summary>
        public static bool StopService()
        {
            try
            {
                lock (lockObject)
                {
                    // 关闭定时器
                    if (checkConnectTimer != null)
                    {
                        using (ManualResetEvent waitHandle = new ManualResetEvent(false)) // 声明一个 WaitHandle，用来等待回调方法执行完成
                        {
                            checkConnectTimer.Dispose(waitHandle);
                            waitHandle.WaitOne(TimeSpan.FromSeconds(3));                  // 等待回调方法执行完成，最多等待3秒
                            checkConnectTimer = null;
                            LogConfig.WriteRunLog("ReceiveMQ", "定时器监听关闭成功");
                        }

                    }

                    data_receiver.StopDATAServer();                     // 关闭UDP通信服务

                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();
                    do
                    {
                        // 关闭RabbitMQ服务
                        if (!DataComm.isRun && !isRunAcceptData)        // 如果UDP服务已经关闭，则关闭MQ服务
                        {
                            StopMQ();
                            break;
                        }
                    } while (stopwatch.ElapsedMilliseconds < 5000);     // 5秒内,等待UDP服务关闭,等待集合数据清空上传 MQ

                    StopMQ();                                           // 5秒后若集合没清空，直接关闭RabbitMQ服务
                    stopwatch.Stop();

                    UpdateConnectStateEvent?.Invoke("PLCState", false); // 更新PLC连接状态
                    UpdateConnectStateEvent?.Invoke("RcvMQ", false);    // 更新MQ连接状态

                    return true;
                }

            }
            catch (Exception ex)
            {
                LogConfig.WriteRunLog("ReceiveMQ", $"关闭所有服务异常：{ex.Message}\r\n"); 
                return false;
            }
        }
    }
}