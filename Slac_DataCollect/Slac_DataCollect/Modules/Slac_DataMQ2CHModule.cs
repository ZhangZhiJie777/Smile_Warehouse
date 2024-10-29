using MySqlX.XDevAPI;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Framing.Impl;
using ServiceStack.Messaging;
//using ServiceStack.ServiceInterface.ServiceModel;
using Slac_DataCollect.Utils;
using SlacDataCollect;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Primitives;
using System.Data;
using System.IO;
using System.Linq;
using System.Messaging;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ServiceStack.Redis;
using Slac_DataCollect.Common;
using SqlSugar;
using System.Net;
using Slac_DataCollect.FormPage;
using static Slac_DataCollect.Common.Parameter;
using Slac_DataCollect.DatabaseSql.DBModel;
using Slac_DataCollect.DatabaseSql.DBOper;
using System.Runtime.Remoting.Messaging;
using System.Globalization;
//using static ServiceStack.Diagnostics.Events;
using HttpClient = System.Net.Http.HttpClient;
using System.Security.Cryptography;

namespace Slac_DataCollect.Modules
{
    /// <summary>
    /// 用于将MQ队列的消息解析并存储到CH数据库
    /// </summary>
    public class Slac_DataMQ2CHModule
    {
        private static bool rabbitMQState = false;   // rabbitMQ连接状态
        private static bool redisState = false;      // redis连接状态
        private static bool clickHouseState = false; // clickhouse连接状态
        public static event Func<string, string, bool> DataReturnEvent; //用于返回数据，更新界面
        public static event Action<string, bool> UpdateConnectStateEvent;  // 用于返回连接状态，更新界面

        public static bool IsSaveBin = false;
        public static bool IsShowSendLog = false;

        private readonly object lockObj = new object();

        public static string MQ2CH_TimeVersion; // 时间戳版本

        #region click house参数
        private static string isCluster;    // 是否分布式
        private static string CHpwd;        // click house 密码 slac1028# 或 slac1028
        private static string CHport;       // click house 端口
        private static string CHserver;     // click house IP

        private static HttpClient httpClient; // click house的连接的监听对象（不是上传数据，专门用来监听连接）

        public static int ListCount = 0;    // 累计数据量
        public static int LogListCount = 0; // 累计日志数据量
        private static int ListCountLimit;  // 每次提交处理数据量，累计数据超过该值，才上传一次 clickhouse数据库

        #endregion        

        public static bool isPause_save2db = true;
        public static string is2MQbak;     // 是否保存到备份消息队列
        private static string dataVer;     // 数据包版本  

        #region RabbitMQ参数
        private static string MQserver;     // IP  
        public static string MQexchange;    // 交换机 
        public static string lineID;        // 消息队列名      
        public static string Password;      // 密码

        private static IConnection connection = null;
        public static IModel channel = null; // 消费消息队列的通道
        //现场使用
        private static ConnectionFactory factory = new ConnectionFactory() { HostName = MQserver, Port = 5672, UserName = MQexchange, Password = "slac1028", AutomaticRecoveryEnabled = true };
        //本地调试
        //private static ConnectionFactory factory = new ConnectionFactory() { HostName = "localhost", Port = 5672, UserName = "guest", Password = "guest", AutomaticRecoveryEnabled = true };

        public static IModel channel2 = null; // 用于发送消息到备份队列(该队列用于后续远端传输到苏州斯莱克本地服务器)
        private static IBasicProperties properties2 = null;

        public static EventingBasicConsumer consumer; // 消费者

        private static string QueueName = MQexchange + "_line" + lineID;// "slac_line1003" //消费的消息队列名称

        #endregion

        private static System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1)); // 当地时区
        private static string packetID = "";
        public static string RcvTime = "";


        private static StringBuilder SqlString = new StringBuilder("");
        private static StringBuilder LogSqlString = new StringBuilder("");
        private static string sqlhead = "insert into " + MQexchange + ".line" + lineID + " (eventtime,device_id,msg_id,data) values ";                // 1001临湖,1002巴西
        private static string Logsqlhead = "insert into " + MQexchange + ".log" + lineID + " (rcv_time,send_time,device_id,packet_id,data) values ";  // 1001临湖,1002巴西

        #region Redis参数
        //--- 2024-06-09  Add by CWZ 发送给Redis
        private static string is2RDSmsg;          // 是否保存到Redis
        private static string RdsIP;              // RedisIP
        private static string RedisPassword;      // Redis密码
        private static int RedisDbIndex;       // Redis数据库索引
        private readonly DataTable dt_api_list = new DataTable();
        // 本地调试
        private readonly RedisClient client /*= new RedisClient("127.0.0.1", 6379)*/;  // "127.0.0.1"

        // 创建RedisManagerPool连接池实例
        private static PooledRedisClientManager redisManagerPool = null;
        // Redis连接字符串        
        private static string redisConnectionString;

        //现场使用
        //private readonly RedisClient client = new RedisClient(RdsIP, 6379);  // "127.0.0.1"

        #endregion

        #region MSMQ参数-暂时不用
        /*//-2024-06-18 发送到msmq-slacmsg
        private static string is2MQmsg; // 是否保存到备份消息队列
        private static string MQPath_boardMsg = @".\private$\slacmsg";
        private static MessageQueue myQueue_boardMsg;        
        private DataTable dt_dashboard_list = new DataTable();
        */
        #endregion

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

                MQ2CH_TimeVersion = list.Find(e => e.Name.Trim() == "MQ2CH_TimeVersion").Value.Trim();
                is2MQbak = list.Find(e => e.Name.Trim() == "MQ2CH_is2MQbak").Value.Trim();
                /*is2MQmsg = list.Find(e => e.Name.Trim() == "MQ2CH_is2MQmsg").Value.Trim();*/
                dataVer = list.Find(e => e.Name.Trim() == "MQ2CH_dataVer").Value.Trim();
                ListCountLimit = Convert.ToInt32(list.Find(e => e.Name.Trim() == "MQ2CH_ListCountLimit").Value.Trim());

                //click house
                isCluster = list.Find(e => e.Name.Trim() == "isCluster").Value.Trim();
                CHpwd = list.Find(e => e.Name.Trim() == "CHserver").Value.Trim().Split('|')[0];
                CHserver = list.Find(e => e.Name.Trim() == "CHserver").Value.Trim().Split('|')[1];
                CHport = list.Find(e => e.Name.Trim() == "CHserver").Value.Trim().Split('|')[2];

                //RabbitMQ
                MQserver = list.Find(e => e.Name.Trim() == "MQ2CH_MQserver").Value.Trim().Split('|')[0];
                MQexchange = list.Find(e => e.Name.Trim() == "MQ2CH_MQserver").Value.Trim().Split('|')[1];
                lineID = list.Find(e => e.Name.Trim() == "MQ2CH_MQserver").Value.Trim().Split('|')[2];
                Password = list.Find(e => e.Name.Trim() == "MQ2CH_MQserver").Value.Trim().Split('|')[3];

                //Redis
                is2RDSmsg = list.Find(e => e.Name.Trim() == "MQ2CH_RDSserver").Value.Trim().Split('|')[0];
                RdsIP = list.Find(e => e.Name.Trim() == "MQ2CH_RDSserver").Value.Trim().Split('|')[1];
                redisConnectionString = list.Find(e => e.Name.Trim() == "MQ2CH_RDSConnectionString").Value.Trim().Split('|')[0];
                RedisPassword = list.Find(e => e.Name.Trim() == "MQ2CH_RDSConnectionString").Value.Trim().Split('|')[1];
                RedisDbIndex = Convert.ToInt32(list.Find(e => e.Name.Trim() == "MQ2CH_RDSConnectionString").Value.Trim().Split('|')[2]);

            }
            catch (Exception ex)
            {
                RichText_InputText("textBox_ShowMsg", $"获取配置参数异常,请检查数据库配置 @{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}");
                LogConfig.WriteErrLog("MQ2CH", $"获取配置参数异常 Error：{ex.Message}\r\n");                
            }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="systemConfig">系统参数配置</param>
        public Slac_DataMQ2CHModule()
        {
            LogConfig.WriteRunLog("MQ2CH", $"———— ———— ———— ———— ———— ———— ————\r\n");
            LogConfig.WriteRunLog("MQ2CH", $"MQ2CH服务开始启动");

            QueueName = MQexchange + "_line" + lineID;// "slac_line1003" //消费的消息队列名称
            sqlhead = "insert into " + MQexchange + ".line" + lineID + " (eventtime,device_id,msg_id,data) values ";                // 1001临湖,1002巴西
            Logsqlhead = "insert into " + MQexchange + ".log" + lineID + " (rcv_time,send_time,device_id,packet_id,data) values ";  // 1001临湖,1002巴西

            SqlString = new StringBuilder(sqlhead);
            LogSqlString = new StringBuilder(Logsqlhead);
            isPause_save2db = false;
            if (isCluster == "1")
            {
                CHpwd = "slac1028#";
                CHport = "10280";
            }

            try
            {
                //dt_dashboard_list = ConfigSQL.Get_dashboard_list();
                dt_api_list = ConfigSQL.Get_data2api_list();
                LogConfig.WriteRunLog("MQ2CH", $"读取sqlite数据文件(System.Data.Sql.db)成功");
            }
            catch (Exception ex)
            {
                LogConfig.WriteErrLog("MQ2CH", $"读取sqlite数据文件异常，Error：{ex.Message}\r\n");
                RichText_InputText("textBox_ShowMsg", "Error:读取sqlite数据文件异常，请检查文件：System.Data.Sql.db" + "@" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));                
            }

            try
            {
                // 实例化Rsdis连接池
                if (redisManagerPool == null)
                {
                    redisManagerPool = new PooledRedisClientManager(redisConnectionString);
                }

                // 初始化监听一次连接,更新界面状态
                using (var redisClient = redisManagerPool.GetClient())
                {
                    redisClient.Password = RedisPassword; //密码
                    redisClient.Db = RedisDbIndex;        //选择第1个数据库，0-15
                    // 获取 INFO 信息以测试连接状态
                    var info = redisClient.Info;
                    if (info != null)
                    {
                        UpdateConnectStateEvent?.Invoke("MQ2CHRedis", true);
                        LogConfig.WriteRunLog("MQ2CH", $"Redis连接池实例化成功");
                    }
                    else
                    {
                        UpdateConnectStateEvent?.Invoke("MQ2CHRedis", false);
                        LogConfig.WriteErrLog("MQ2CH", $"Redis连接池实例化，获取连接Info信息失败");
                    }
                }

                // 初始化clickhouse 监听HttpClient连接对象
                httpClient = new HttpClient(); // 初始化clickhouse 监听HttpClient连接对象
            }
            catch (Exception ex)
            {
                LogConfig.WriteErrLog("MQ2CH", $"Redis和clickhouse连接异常，Error：{ex.Message}\r\n");
                RichText_InputText("textBox_ShowMsg", "Error：Redis和clickhouse连接异常，请检查配置" + "@" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));                
            }

            #region 注释
            //if (MessageQueue.Exists(MQPath_boardMsg))
            //{
            //    myQueue_boardMsg = new MessageQueue(MQPath_boardMsg);
            //}
            //else
            //{
            //    myQueue_boardMsg = MessageQueue.Create(MQPath_boardMsg);
            //} 
            #endregion
        }

        /// <summary>
        /// 开始服务
        /// </summary>
        public void StartService()
        {
            try
            {                                
                SettingMQ(QueueName); // 设置消息队列
                Run_ReciveInfo();     // 接收消息，进行后续操作                

                // 重置界面连接状态，避免监控冲突
                UpdateConnectStateEvent?.Invoke("MQ2CHMQ", rabbitMQState);
                UpdateConnectStateEvent?.Invoke("MQ2CHRedis", redisState);
                UpdateConnectStateEvent?.Invoke("MQ2CHClickHouse", clickHouseState);

                // 使用一个循环来保持线程活跃，直到stopEvent被设置  
                while (!frm_mq2ch_V0.StopEvent.WaitOne(1000)) // 1000毫秒超时，避免完全阻塞线程  
                {
                    ListenService(); // 启动监听
                }
            }
            catch (Exception ex)
            {
                StopMQ();
                UpdateConnectStateEvent?.Invoke("MQ2CHMQ", false);
                UpdateConnectStateEvent?.Invoke("MQ2CHRedis", false);
                UpdateConnectStateEvent?.Invoke("MQ2CHClickHouse", false);
                LogConfig.WriteErrLog("MQ2CH", $"MQ2CH开始服务异常：{ex.ToString()}\r\n");
                RichText_InputText("textBox_ShowMsg", "MQ2CH开始服务异常，请检查日志！"  + "@" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            }
            finally
            {
                if (redisManagerPool != null)
                {
                    // 关闭Redis连接池，释放资源
                    redisManagerPool.Dispose();
                    redisManagerPool = null;
                    LogConfig.WriteRunLog("MQ2CH", $"Redis连接池关闭成功");
                }

                if (httpClient != null)
                {
                    // 关闭HttpClient对象，释放资源
                    httpClient.Dispose();
                    httpClient = null;
                    LogConfig.WriteRunLog("MQ2CH", $"clickhouse连接关闭成功");
                }
            }

        }

        /// <summary>
        /// 从消息队列接收数据，进行数据处理，保存本地Messqge、MQ消息队列，保存数据库，保存Redis
        /// </summary>
        public void Run_ReciveInfo()
        {            
            string nowdevice = "";
            string nowmsg = "";

            try
            {
                /* 这里定义了一个消费者，用于消费服务器接受的消息
                 * C#开发需要注意下这里，在一些非面向对象和面向对象比较差的语言中，是非常重视这种设计模式的。
                 * 比如RabbitMQ使用了生产者与消费者模式，然后很多相关的使用文章都在拿这个生产者和消费者来表述。
                 * 但是，在C#里，生产者与消费者对我们而言，根本算不上一种设计模式，他就是一种最基础的代码编写规则。
                 * 所以，大家不要复杂的名词吓到，其实，并没那么复杂。
                 * 这里，其实就是定义一个EventingBasicConsumer类型的对象，然后该对象有个Received事件，该事件会在服务接收到数据时触发。
                 */
                //消费者 
                consumer = null;                                //消费者重新绑定，防止重复绑定
                consumer = new EventingBasicConsumer(channel);
                //消费消息 autoAck参数为消费后是否删除
                channel.BasicConsume(QueueName, false, consumer);   //true:自动删除  false: 不自动删除
                //自动接收消息事件：被动读取事件，只要客户端推送消息 ，这边被动接收

                consumer.Received += (model, ea) =>
                {
                    StringBuilder errorLog = new StringBuilder();//日志记录
                    try
                    {
                        if (!isPause_save2db)
                        {
                            var MQmsg = ea.Body;
                            byte[] bufferMQ = (byte[])MQmsg; // MQ消息队列数据

                            /*
                             * byte 数组 MQmsg：
                             * 0—7：接收时间（8个字节）
                             * 8—23：包头（16个字节）0通讯版本、1通讯码、2机器、3发送时、4分、5秒、6毫秒 、7包ID、8传输包序号、9传输包大小, 10传输包数量
                             * 24— ：数据体，解析为int数组：包ID、信息ID、时、分、秒
                            */

                            byte[] byte_rcvtime = new byte[8];                 // 接收时间（8个字节）
                            byte[] buffer_all = new byte[bufferMQ.Length - 8]; // 数据体（队列数据总长度-8个字节）

                            try
                            {
                                // 前八个字节为接收时间，后边的为数据体
                                Buffer.BlockCopy(bufferMQ, 0, byte_rcvtime, 0, 8);
                                Buffer.BlockCopy(bufferMQ, 8, buffer_all, 0, bufferMQ.Length - 8);
                            }
                            catch (Exception ex)
                            {
                                errorLog.AppendLine($"{DateTime.Now}@Error1:{ex.ToString()}");
                                throw ex;
                            }
                            byte[] byte_head = new byte[16];

                            try
                            {
                                // 将数据体的前16个字节作为包头
                                Buffer.BlockCopy(buffer_all, 0, byte_head, 0, 16);
                            }
                            catch (Exception ex)
                            {
                                errorLog.AppendLine($"{DateTime.Now}@Error2:{ex.ToString()}");
                                throw ex;
                            }

                            // 解析包头，结果存在 packet_head_list 数组中
                            // 依次是0通讯版本、1通讯码、2机器、3发送时、4分、5秒、6毫秒 、7包ID、8传输包序号、9传输包大小, 10传输包数量
                            int[] packet_head_list = dataVer == "1" ? SlacPlcDataV1.packet_head(byte_head) : SlacPlcDataV0.packet_head(byte_head);
                            byte[] byte_data = new byte[packet_head_list[9]];              //获取传输包大小 2021-2-3不再减去头大小-16
                            double rcv_timestamp = BitConverter.ToDouble(byte_rcvtime, 0); //接收时间戳

                            try
                            {
                                // 将数据体的第17个字节开始的数据作为数据体，长度为传输包大小，拷贝到 byte_data字节数组中
                                Buffer.BlockCopy(buffer_all, 16, byte_data, 0, packet_head_list[9]); //2021-2-3不再减去头大小 -16
                            }
                            catch (Exception ex)
                            {
                                errorLog.AppendLine($"{DateTime.Now}@Error3:数据包标记大小与实际不匹配！标记大小：{packet_head_list[9]}-实际大小：{bufferMQ.Length},异常信息：{ex.ToString()}");

                                if (IsSaveBin)
                                {
                                    // 保存异常数据包，位置程序exe运行目录，文件名为当前日期时间加上 rcv_timestamp 和 lineID。
                                    string exepath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                                    string FilePath = System.IO.Path.GetDirectoryName(exepath);
                                    string sPath = FilePath + @"\" + DateTime.Now.ToString("yyyyMMdd");
                                    if (!Directory.Exists(sPath))
                                    {
                                        Directory.CreateDirectory(sPath);
                                    }
                                    string fileName = sPath + @"\" + lineID + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss_") + rcv_timestamp.ToString() + ".sbin";
                                    bool saveRet = FileHelper.ByteToFile(bufferMQ, fileName); // 将字节数组保存为文件
                                }
                                throw ex;
                            }

                            try
                            {
                                DateTime time_Plc_Receive = startTime.AddMilliseconds(rcv_timestamp); // 根据接收时间戳及时区，得出接收PLC数据的具体时间
                                // 根据包头解析的数据，以及PLC接收时间，计算出PLC发送数据的具体时间
                                DateTime time_Plc_send = time_Plc_Receive.Date.AddHours(packet_head_list[3]).AddMinutes(packet_head_list[4]).AddSeconds(packet_head_list[5]).AddMilliseconds(packet_head_list[6]);

                                // 获取传输包数据，存储到 int_datalist int数组中
                                int[] int_datalist = dataVer == "1" ? SlacPlcDataV1.SetByte2Int(byte_data) : SlacPlcDataV0.SetByte2Int(byte_data);
                                packetID = packet_head_list[8].ToString(); // 传输包序号
                                RcvTime = time_Plc_Receive.ToString("yyyy-MM-dd HH:mm:ss.fff");

                                int logNumCount = ListCount;


                                /* 数据时间版本：0:PLC时间 1:PLC接收时间 2:PLC时间+事件触发拆位*/
                                if (MQ2CH_TimeVersion == "0" || MQ2CH_TimeVersion == "1" || MQ2CH_TimeVersion == "2")
                                {
                                    for (int i = 0; i < int_datalist.Count(); i++)
                                    {
                                        int packet_id = dataVer == "1" ? SlacPlcDataV1.GetPacket_id(int_datalist[i]) : SlacPlcDataV0.GetPacket_id(int_datalist[i]); // >> 29 & 7;

                                        int[] datahead = dataVer == "1" ? SlacPlcDataV1.data_head(int_datalist[i]) : SlacPlcDataV0.data_head(int_datalist[i]);  //依次是：包ID、信息ID、时、分、秒
                                        DateTime time_data = time_Plc_Receive.Date.AddHours(datahead[2]).AddMinutes(datahead[3]).AddSeconds(datahead[4]);

                                        double plctime_delta = 0; //PLC数据延迟时间

                                        if (MQ2CH_TimeVersion == "0" || MQ2CH_TimeVersion == "2")
                                        {
                                            // 解决跨天问题
                                            plctime_delta = (DateTime.Now - time_data).TotalMilliseconds;
                                            if (plctime_delta < -43200000)
                                            {
                                                plctime_delta = -86400000;
                                                time_data = time_data.AddMilliseconds(plctime_delta); // 误差超12小时，数据时间往前推一天
                                            }
                                        }
                                        else if (MQ2CH_TimeVersion == "1")
                                        {
                                            // 消息体前八个字节（接收时间）解析的发送时间减去数据时间差
                                            plctime_delta = (time_Plc_send.TimeOfDay - time_data.TimeOfDay).TotalMilliseconds;
                                            if (plctime_delta < -43200000)
                                            {
                                                plctime_delta = plctime_delta + 86400000; // 误差超12小时，PLC数据延迟时间往后推一天（跨天）
                                            }
                                        }

                                        if (packet_id == 0) // 000普通数据 
                                        {
                                            int msg_id = datahead[1];                   // 信息ID
                                            nowdevice = packet_head_list[2].ToString(); // 机器
                                            nowmsg = msg_id.ToString();

                                            int data_value = int_datalist[i + 1];
                                            int enDatavalue = dataVer == "1" ? SlacPlcDataV1.GetEnIntV1(data_value, packet_head_list[2], msg_id) : SlacPlcDataV0.GetEnIntV1(data_value, packet_head_list[2], msg_id);

                                            if (MQ2CH_TimeVersion == "0" || MQ2CH_TimeVersion == "2")
                                            {
                                                // V0和V2版本使用PLC数据时间
                                                SqlString.Append("('" + time_data.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss.fff") + "','" + packet_head_list[2].ToString() + "','" + msg_id.ToString() + "','" + enDatavalue.ToString() + "'),");
                                            }
                                            else if (MQ2CH_TimeVersion == "1")
                                            {
                                                // V1版本使用PLC接收时间
                                                SqlString.Append("('" + time_Plc_Receive.AddMilliseconds(-plctime_delta).ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss.fff") + "','" + packet_head_list[2].ToString() + "','" + msg_id.ToString() + "','" + enDatavalue.ToString() + "'),");
                                            }

                                            CheckAndSend2MQ(nowdevice, msg_id.ToString(), data_value);
                                            ListCount++;
                                            i++;

                                        }
                                        else if (packet_id == 1) // 001高速数据
                                        {
                                            int[] datahead_highspeed2 = dataVer == "1" ? SlacPlcDataV1.data_head_highspeed2(int_datalist[i + 1]) : SlacPlcDataV0.data_head_highspeed2(int_datalist[i + 1]);
                                            // 依次是：数据量、采样周期毫秒、起始时间毫秒
                                            int data_lenth_hspeed = datahead_highspeed2[0];  // 数据量---32位数值个数
                                            int milli_second = datahead_highspeed2[1];       // 采样周期毫秒
                                            int start_milli_second = datahead_highspeed2[2]; // 起始时间毫秒

                                            nowdevice = packet_head_list[2].ToString();
                                            nowmsg = datahead[1].ToString();

                                            int[] datahead_highspeed3 = dataVer == "1" ? SlacPlcDataV1.data_head_highspeed3(int_datalist[i + 2]) : SlacPlcDataV0.data_head_highspeed3(int_datalist[i + 2]);
                                            // 数据大小、功能3bit
                                            int data_type = datahead_highspeed3[0];      // 000-bit  001-8bit  010-16bit  011-32bit
                                            int highspeed_type = datahead_highspeed3[1]; // 功能 

                                            // 2020-12-31不按浮点数存储，存成int，用的时候再转为浮点数
                                            for (int m = 0; m < data_lenth_hspeed; m++)
                                            {
                                                int add_millisecond = start_milli_second;
                                                int msgID = datahead[1];
                                                int data_value = Convert.ToInt32(int_datalist[i + 3 + m]);

                                                if (data_type == 0)       // 00--1bit
                                                {
                                                    data_value = Convert.ToInt32(int_datalist[i + 3 + m]);
                                                }
                                                else if (data_type == 1)  // 01--8bit
                                                {
                                                    data_value = Convert.ToInt32(int_datalist[i + 3 + m]);
                                                }
                                                else if (data_type == 2)  // 10--16bit
                                                {
                                                    data_value = Convert.ToInt32(int_datalist[i + 3 + m]);
                                                }
                                                else if (data_type == 3)  // 11--32bit
                                                {
                                                    data_value = Convert.ToInt32(int_datalist[i + 3 + m]);
                                                }

                                                if (highspeed_type == 0) // 功能=000： 同一个ID采集多条记录
                                                {
                                                    msgID = datahead[1];
                                                    add_millisecond = start_milli_second + milli_second * m; // 此处如果怕因为时间相同覆盖数据的话，可以+m把时间区分开
                                                }
                                                else if (highspeed_type == 1) // 功能=001：ID递增，每条ID一行记录
                                                {
                                                    msgID = datahead[1] + m;
                                                    add_millisecond = start_milli_second; // 此处如果怕因为时间相同覆盖数据的话，可以+m把时间区分开
                                                }

                                                int enDatavalue = dataVer == "1" ? SlacPlcDataV1.GetEnIntV1(data_value, packet_head_list[2], msgID) : SlacPlcDataV0.GetEnIntV1(data_value, packet_head_list[2], msgID);

                                                if (MQ2CH_TimeVersion == "0" || MQ2CH_TimeVersion == "2")
                                                {
                                                    // V0和V2版本使用PLC数据时间
                                                    SqlString.Append("('" + time_data.AddMilliseconds(add_millisecond).AddTicks(m * 2).ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss.fff") + "','" + packet_head_list[2].ToString() + "','" + msgID.ToString() + "','" + enDatavalue.ToString() + "'),");
                                                }
                                                else if (MQ2CH_TimeVersion == "1")
                                                {
                                                    // V1版本使用接收时间。
                                                    SqlString.Append("('" + time_Plc_Receive.AddMilliseconds(add_millisecond - plctime_delta).AddTicks(m * 2).ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss.fff") + "','" + packet_head_list[2].ToString() + "','" + msgID.ToString() + "','" + enDatavalue.ToString() + "'),");
                                                }

                                                CheckAndSend2MQ(nowdevice, msgID.ToString(), data_value);
                                                ListCount++;
                                            }
                                            i = i + 2 + data_lenth_hspeed;

                                        }
                                        else if (packet_id == 2)  //010事件触发
                                        {
                                            int local_milli_second = 0;
                                            if (MQ2CH_TimeVersion == "2")
                                            {
                                                int[] datahead_event2 = dataVer == "1" ? SlacPlcDataV2.data_head_event2(int_datalist[i + 1]) : SlacPlcDataV0.data_head_highspeed2(int_datalist[i + 1]);
                                                // 依次是：信息位数3bit、扫描周期毫秒8bit、本地时间毫秒10bit
                                                int msg_bit_type = datahead_event2[0];   // 1、2、4、8、16、32
                                                int scan_second = datahead_event2[1];    // 扫描周期毫秒
                                                local_milli_second = datahead_event2[2]; // 本地时间毫秒
                                            }

                                            int msg_id = datahead[1];
                                            nowdevice = packet_head_list[2].ToString();
                                            nowmsg = msg_id.ToString();
                                            int data_value;
                                            if (MQ2CH_TimeVersion == "2")
                                            {
                                                data_value = int_datalist[i + 2];
                                            }
                                            else { data_value = int_datalist[i + 1]; }

                                            int enDatavalue = dataVer == "1" ? SlacPlcDataV1.GetEnIntV1(data_value, packet_head_list[2], msg_id) : SlacPlcDataV0.GetEnIntV1(data_value, packet_head_list[2], msg_id);

                                            if (MQ2CH_TimeVersion == "0")
                                            {
                                                // V0版本使用PLC数据时间。
                                                SqlString.Append("('" + time_data.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss.fff") + "','" + packet_head_list[2].ToString() + "','" + msg_id.ToString() + "','" + enDatavalue.ToString() + "'),");
                                            }
                                            else if (MQ2CH_TimeVersion == "1")
                                            {
                                                // V1版本使用接收时间。
                                                SqlString.Append("('" + time_Plc_Receive.AddMilliseconds(-plctime_delta).ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss.fff") + "','" + packet_head_list[2].ToString() + "','" + msg_id.ToString() + "','" + enDatavalue.ToString() + "'),");
                                            }
                                            else if (MQ2CH_TimeVersion == "2")
                                            {
                                                // V2版本使用本地时间。
                                                SqlString.Append("('" + time_data.AddMilliseconds(local_milli_second).ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss.fff") + "','" + packet_head_list[2].ToString() + "','" + msg_id.ToString() + "','" + enDatavalue.ToString() + "'),");
                                            }

                                            CheckAndSend2MQ(nowdevice, msg_id.ToString(), data_value);
                                            ListCount++;
                                            if (MQ2CH_TimeVersion == "2")
                                            {
                                                i = i + 2;
                                            }
                                            else
                                            {
                                                i++;
                                            }


                                        }
                                    }
                                }
                                else
                                {
                                    RichText_InputText("textBox_ShowMsg", $"数据保存时间版本配置获取错误，请检查数据库配置！@{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}");
                                }

                                logNumCount = ListCount - logNumCount;
                                //System.IO.File.AppendAllText("output\\packetID_" + DateTime.Now.ToString("yyyyMMdd_") + packet_head_list[2].ToString() + ".log", packetID + " @ " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "\r\n");
                                LogSqlString.Append("('" + time_Plc_Receive.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss.fff") + "','" + time_Plc_send.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss.fff") + "','" + packet_head_list[2].ToString() + "','" + packetID.ToString() + "','" + logNumCount.ToString() + "'),");
                                LogListCount++;



                                if (ListCount >= ListCountLimit)
                                {
                                    saveToCK(RcvTime);
                                    RichText_InputText("textBox_ShowMsg", $"保存数据库： {"  Total: " + ListCountLimit + " rows @   " + time_Plc_Receive}");
                                }


                                if (LogListCount >= ListCountLimit / 10)
                                {
                                    saveLogToCK(RcvTime);
                                    RichText_InputText("textBox_ShowMsg", $"Log保存数据库： {"  Total: " + ListCountLimit / 10 + " rows @   " + time_Plc_Receive}");
                                }

                                //--保存到 bak队列里面
                                if (is2MQbak == "1" && logNumCount > 0)
                                {
                                    SendtoMQ_2(MQexchange, "line" + lineID + "_bak", ea.Body);
                                }
                                //手动向rabbitmq发送消息确认
                                //当294行为false时需要手动ack才能删除消息  //前文的这里： /*channel.BasicConsume(QueueName, false, consumer);*/ 
                                channel.BasicAck(ea.DeliveryTag, false);
                                // /*RichText_InputText($"Received： {packet_head_list[8].ToString()+"  @   "+time_Plc_Receive}");*/
                            }
                            catch (Exception ex)
                            {
                                errorLog.AppendLine($"{DateTime.Now}@Error3:RabbitMq数据解析转换异常:{ex.ToString()}");
                                throw ex;
                            }

                        }

                    }
                    catch (Exception ex)
                    {
                        RichText_InputText("textBox_ShowMsg", errorLog.ToString());
                        LogConfig.WriteErrLog("MQ2CH", $"====RabbitMQ received event Error Begin======{Environment.NewLine}{errorLog.ToString()}{Environment.NewLine}====RabbitMQ received event Error End======{Environment.NewLine}\r\n");                        
                        errorLog.Clear();
                        channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                    }
                };
                LogConfig.WriteRunLog("MQ2CH", "RabbitMQ消费者订阅接收事件，订阅成功");

                #region 注释
                //读取方式2:主动读取一次消息 ，不会定时取
                /*
                BasicGetResult res = channel.BasicGet(queue, false*//*noAck*//*);
                if (res != null)
                {
                    RichText_InputText("22222--" + System.Text.UTF8Encoding.UTF8.GetString(res.Body.ToArray()));
                    ch.BasicAck(res.DeliveryTag, false);

                }
                else
                {
                    RichText_InputText("22222--[external-request-queue] no data");
                }
                */
                //处理时间  
                #endregion
            }
            catch (Exception ex)
            {
                LogConfig.WriteErrLog("MQ2CH", $"RabbitMQ消费者订阅接收事件，订阅失败,异常：{ex.Message}\r\n");
                RichText_InputText("textBox_ShowMsg", $"RabbitMQ消费者订阅接收事件，订阅失败!");
            }
        }

        /// <summary>
        /// 保存到本地Message消息队列、保存到 Redis中
        /// </summary>
        /// <param name="from_device_id"></param>
        /// <param name="msg_id"></param>
        /// <param name="data"></param>
        private void CheckAndSend2MQ(string from_device_id, string msg_id, Int32 data)
        {
            try
            {
                #region MessageQueue-暂时不用
                //if (is2MQmsg == "1")
                //{
                //    string SqlStr = " from_device_id='" + from_device_id + "' and msg_id='" + msg_id + "'";
                //    DataRow[] dr_list = dt_dashboard_list.Select(SqlStr);
                //    for (int i = 0; i < dr_list.Count(); i++)
                //    {
                //        DataRow dr = dr_list[i];
                //        int bit_id = Convert.ToInt32(dr["bit_id"]);
                //        int bit_data = Convert.ToInt32(dr["bit_data"]);
                //        int state_id = Convert.ToInt32(dr["state_id"]);
                //        string device_id = dr["device_id"].ToString();
                //        int state_type = (data >> bit_id) & 1;
                //        if (state_type == bit_data)
                //        {
                //            myQueue_boardMsg = new MessageQueue(MQPath_boardMsg);
                //            myQueue_boardMsg.Formatter = new BinaryMessageFormatter();
                //            myQueue_boardMsg.DefaultPropertiesToSend.Recoverable = true;
                //            var str = "{\"device_id\":\"" + device_id + "\",\"msg_id\":\"" + msg_id + "." + bit_id.ToString() + "\",\"data\":\"" + state_type.ToString() + "\",\"state_id\":\"" + state_id.ToString() + "\"}";
                //            var chararr = Encoding.Default.GetBytes(str);
                //            myQueue_boardMsg.Send(chararr, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                //        }
                //    }
                //} 
                #endregion

                if (is2RDSmsg == "1")
                {
                    string SqlStr_api = " from_device_id=" + from_device_id + " and msg_id=" + msg_id + " and line_id='" + lineID + "'";
                    DataRow[] dr_apilist = dt_api_list.Select(SqlStr_api);
                    for (int j = 0; j < dr_apilist.Count(); j++)
                    {
                        DataRow dr = dr_apilist[j];
                        string device_id = dr["device_id"].ToString();
                        int bit_id = Convert.ToInt32(dr["bit_id"]);
                        int isbitdata = Convert.ToInt32(dr["isbitdata"]);
                        if (redisManagerPool != null)
                        {
                            using (var redisClient = redisManagerPool.GetClient())
                            {
                                redisClient.Password = RedisPassword; //密码
                                redisClient.Db = RedisDbIndex;        //选择第1个数据库，0-15
                                if (isbitdata == 0)
                                {
                                    redisClient.Set("d" + lineID + "." + device_id + "." + msg_id, data);

                                }
                                else
                                {
                                    int bitdata = (data >> bit_id) & 1;
                                    redisClient.Set("d" + lineID + "." + device_id + "." + msg_id + "." + bit_id, bitdata);
                                }
                            }
                        }
                        else
                        {
                            RichText_InputText("textBox_ShowMsg", $"Redis连接已关闭，上传Redis失败： @{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.ffff")}");
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                LogConfig.WriteErrLog("MQ2CH", $"from_device_id：{from_device_id}--msg_id：{msg_id}, 保存Redis异常：{ex.Message}\r\n");
                throw;
            }
        }

        /// <summary>
        /// 保存到备份的消息队列：用来远端传输到斯莱克服务器
        /// </summary>
        /// <param name="MQ_exchange"></param>
        /// <param name="router"></param>
        /// <param name="SendMsg"></param>
        /// <returns></returns>
        private void SendtoMQ_2(string MQ_exchange, string router, byte[] SendMsg)
        {
            try
            {
                if (connection.IsOpen)
                {
                    channel2.BasicPublish(MQ_exchange, router, properties2, SendMsg);
                    if (IsShowSendLog)
                    {
                        RichText_InputText("label1", "传输成功！ @ " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                    }
                }
                else
                {
                    RichText_InputText("label1", "传输失败！ @ " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                }

            }
            catch (Exception ex)
            {
                LogConfig.WriteErrLog("MQ2CH", $"保存bak消息队列异常：{ex.Message}\r\n");
                RichText_InputText("label1", "异常" + ex.ToString() + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            }

        }

        /// <summary>
        /// 更新界面状态
        /// </summary>
        /// <param name="msg"></param>
        protected static void RichText_InputText(string msgLocation, string msg)
        {
            DataReturnEvent?.Invoke(msgLocation, msg);
        }

        /// <summary>
        /// 保存到 click house数据库
        /// </summary>
        /// <param name="RcvTime"></param>
        public void saveToCK(string RcvTime)
        {
            try
            {
                lock (lockObj)
                {
                    //web.Headers.Add("Content-Type", "application/json");
                    StringBuilder ssql = SqlString.Remove(SqlString.Length - 1, 1);
                    byte[] postData = Encoding.ASCII.GetBytes(ssql.ToString());
                    string postUrl = "";
                    bool isUrl = false;
                    if (CHport == "8123")
                    {
                        postUrl = $"http://{CHserver}:8123/";
                        isUrl = true;
                    }
                    else
                    {
                        if (DateTime.TryParseExact(RcvTime, "yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime rcvTime))
                        {
                            int days2port = 10281 + Convert.ToDateTime(rcvTime).Day % 3;
                            postUrl = $"http://{CHserver}:{days2port.ToString()}";
                            isUrl = true;
                        }

                    }
                    if (isUrl)
                    {
                        var strResult = PostResponse("default", CHpwd, postUrl, ssql.ToString());
                        // 上传数据
                        // byte[] responseData = web.UploadData("http://" + CHserver + ":8123/", "POST", postData);
                        //获取返回的二进制数据.
                        // string strResult = Encoding.UTF8.GetString(responseData);
                        if (string.IsNullOrEmpty(strResult))
                        {
                            SqlString = new StringBuilder(sqlhead);
                            ListCount = 0;
                        }
                    }
                    else
                    {
                        RichText_InputText("textBox_ShowMsg", $"保存数据库失败！获取完整请求路径失败，当前路径：{postUrl}");
                    }
                    Thread.Sleep(500);
                }

            }
            catch (Exception ex)
            {

                RichText_InputText("textBox_ShowMsg", $"保存数据库失败！请检查网线是否连接正常...");
                LogConfig.WriteErrLog("MQ2CH", $"保存数据库失败！请检查网线是否连接正常...Error：{ex.Message}\r\n");                
                //return;
            }

        }

        /// <summary>
        /// 保存到 click house数据库日志
        /// </summary>
        /// <param name="RcvTime"></param>
        public void saveLogToCK(string RcvTime)
        {
            try
            {
                lock (lockObj)
                {

                    //web.Headers.Add("Content-Type", "application/json");
                    StringBuilder Logssql = LogSqlString.Remove(LogSqlString.Length - 1, 1);
                    byte[] postData = Encoding.ASCII.GetBytes(Logssql.ToString());
                    string postUrl = "";
                    if (CHport == "8123")
                    {
                        postUrl = $"http://{CHserver}:8123/";
                    }
                    else
                    {
                        int days2port = 10281 + Convert.ToDateTime(RcvTime).Day % 3;
                        postUrl = $"http://{CHserver}:{days2port.ToString()}";
                    }
                    var strResult = PostResponse("default", CHpwd, postUrl, Logssql.ToString());
                    // 上传数据
                    // byte[] responseData = web.UploadData("http://" + CHserver + ":8123/", "POST", postData);
                    //获取返回的二进制数据.
                    // string strResult = Encoding.UTF8.GetString(responseData);
                    if (string.IsNullOrEmpty(strResult))
                    {
                        LogSqlString = new StringBuilder(Logsqlhead);
                        LogListCount = 0;
                    }
                    Thread.Sleep(500);
                }
            }
            catch (Exception ex)
            {
                RichText_InputText("textBox_ShowMsg", $"Log保存数据库失败！请检查网线是否连接正常...");
                LogConfig.WriteErrLog("MQ2CH", $"Log保存数据库失败！请检查网线是否连接正常...Error：{ex.Message}\r\n");
                //return;
            }

        }

        /// <summary>
        /// Post请求发送到click house
        /// </summary>
        /// <param name="user"></param>
        /// <param name="password"></param>
        /// <param name="url"></param>
        /// <param name="postData"></param>
        /// <returns></returns>
        private string PostResponse(string user, string password, string url, string postData)
        {
            AuthenticationHeaderValue authentication = new AuthenticationHeaderValue(
               "Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{user}:{password}")
               ));
            string result = string.Empty;
            HttpContent httpContent = new StringContent(postData);
            httpContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            httpContent.Headers.ContentType.CharSet = "utf-8";
            using (HttpClient httpClient = new HttpClient())
            {
                try
                {
                    httpClient.DefaultRequestHeaders.Authorization = authentication;
                    HttpResponseMessage response = httpClient.PostAsync(url, httpContent).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        Task<string> t = response.Content.ReadAsStringAsync();
                        result = t.Result;
                    }
                }
                catch (Exception ex)
                {
                    LogConfig.WriteErrLog("MQ2CH", $"PostResponse请求发送到ClickHouse异常 Error：{ex.Message}\r\n");
                    throw;
                }
            }
            return result;
        }

        /// <summary>
        /// 设置RabbitMQ服务
        /// </summary>
        public static void SettingMQ(string QueueName)
        {
            try
            {
                if (connection == null)
                {
                    factory = new ConnectionFactory() { HostName = MQserver, Port = 5672, UserName = MQexchange, Password = Password, AutomaticRecoveryEnabled = true };

                    LogConfig.WriteRunLog("MQ2CH", $"RabbitMQ服务：IP：{MQserver}，端口：{5672}，用户名：{MQexchange}，密码：{Password}");

                    connection = factory.CreateConnection();

                    //channel：用来接收消息
                    channel = connection.CreateModel();
                    //每个消费者最多消费一条消息,没返回消息确认之前不再接收消息
                    channel.BasicQos(0, 1, false);
                    channel.QueueDeclare(QueueName, true, false, false, null);
                    LogConfig.WriteRunLog("MQ2CH", $"RabbitMQ服务消费消息队列名称：{QueueName}");

                    //channel2：用来发送到备份消息队列
                    channel2 = connection.CreateModel();
                    properties2 = channel2.CreateBasicProperties();
                    properties2.DeliveryMode = 2;//持久化
                                                 //properties2.Expiration = (3 * 24 * 3600 * 1000).ToString();  //设置消息的TTL生命周期:ms   3 * 24 * 3600 * 1000

                    if (connection.IsOpen && channel.IsOpen && channel2.IsOpen)
                    {
                        UpdateConnectStateEvent?.Invoke("MQ2CHMQ", true);
                    }
                    LogConfig.WriteRunLog("MQ2CH", $"RabbitMQ服务启动成功");
                }

            }
            catch (Exception ex)
            {
                UpdateConnectStateEvent?.Invoke("MQ2CHMQ", false);
                RichText_InputText("textBox_ShowMsg", "RabbitMQ服务启动异常：请检查MQ配置" + "@" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                LogConfig.WriteErrLog("MQ2CH", $"RabbitMQ服务启动异常：请检查MQ配置：{ex.Message}\r\n");                
                throw;
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
                    LogConfig.WriteRunLog("MQ2CH", $"RabbitMQ服务消费消息队列关闭成功");
                }
                if (channel2 != null && channel2.IsOpen)
                {
                    channel2.Close();
                    channel2.Dispose();
                    channel2 = null;
                    LogConfig.WriteRunLog("MQ2CH", $"RabbitMQ服务上传bak备份消息队列关闭成功");
                }
                if (connection != null && connection.IsOpen)
                {
                    connection.Close();
                    connection.Dispose();
                    connection = null;
                    LogConfig.WriteRunLog("MQ2CH", $"RabbitMQ服务关闭成功");
                }
            }
            catch (Exception ex)
            {
                RichText_InputText("textBox_ShowMsg", "RabbitMQ服务关闭异常 " + "@" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                LogConfig.WriteErrLog("MQ2CH", $"RabbitMQ服务关闭异常Error：{ex.Message}\r\n");                
            }


        }

        /// <summary>
        /// 监听服务
        /// </summary>
        public void ListenService()
        {
            #region 监听MQ连接状态
            try
            {
                if (rabbitMQState != (connection != null && connection.IsOpen && channel.IsOpen && channel2.IsOpen))
                {
                    rabbitMQState = (connection != null && connection.IsOpen && channel.IsOpen && channel2.IsOpen);
                    UpdateConnectStateEvent?.Invoke("MQ2CHMQ", rabbitMQState);
                }
            }
            catch (Exception ex)
            {
                //throw;
                rabbitMQState = false;
                UpdateConnectStateEvent?.Invoke("MQ2CHMQ", rabbitMQState);
                LogConfig.WriteErrLog("MQ2CH", $"MQ2CH监听RabbitMQ服务异常：{ex.Message}\r\n");                
                //RichText_InputText("textBox_ShowMsg", "监听MQ连接状态异常：请检查MQ服务配置" + "@" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            }

            #endregion

            #region 监听Redis连接状态          
            try
            {
                if (redisManagerPool != null)
                {
                    using (var redisClient = redisManagerPool.GetClient())
                    {
                        redisClient.Password = RedisPassword; //密码
                        redisClient.Db = RedisDbIndex;        //选择第1个数据库，0-15
                        // 获取 INFO 信息以测试连接状态
                        var info = redisClient.Info;

                        if (redisState != (info != null))
                        {
                            redisState = (info != null);
                            UpdateConnectStateEvent?.Invoke("MQ2CHRedis", redisState);
                        }
                                                
                    }
                }
            }
            catch (Exception ex)
            {
                redisState = false;
                UpdateConnectStateEvent?.Invoke("MQ2CHRedis", redisState);
                LogConfig.WriteErrLog("MQ2CH", $"MQ2CH监听Redis服务异常：{ex.Message}\r\n");               
                //RichText_InputText("textBox_ShowMsg", "监听Redis连接状态Error：异常" + ex.ToString() + "@" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            }

            #endregion

            #region 监听click house数据库连接状态(监控一次 ClickHouse 的 /ping 接口)
            string pingUrl = "";
            bool isurl = false;  // 判断是否是url                    
            if (CHport == "8123")
            {
                pingUrl = $"http://{CHserver}:8123/ping";
                isurl = true;
            }
            else
            {
                if (DateTime.TryParseExact(RcvTime, "yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime rcvTime))
                {
                    int days2port = 10281 + Convert.ToDateTime(rcvTime).Day % 3;
                    pingUrl = $"http://{CHserver}:{days2port.ToString()}/ping";
                    isurl = true;
                }
            }

            //string Url = "http://localhost:8123/ping";
            try
            {
                if (isurl)
                {
                    HttpResponseMessage response = httpClient.GetAsync(pingUrl).Result;
                    if (clickHouseState != response.IsSuccessStatusCode)
                    {
                        clickHouseState = response.IsSuccessStatusCode;
                        UpdateConnectStateEvent?.Invoke("MQ2CHClickHouse", clickHouseState);
                    }
                }
                else
                {
                    clickHouseState = false;
                    UpdateConnectStateEvent?.Invoke("MQ2CHClickHouse", clickHouseState);
                    LogConfig.WriteErrLog("MQ2CH", $"MQ2CH监听click house数据库连接状态Error：请求路径未获取成功" + "\r\n");                    
                    //RichText_InputText("textBox_ShowMsg", "监听click house数据库连接状态Error：请求路径未获取成功" + "@" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                }
            }
            catch (HttpRequestException ex)
            {
                LogConfig.WriteErrLog("MQ2CH", $"MQ2CH监听ClickHouse服务HttpRequestException异常：{ex.Message}\r\n");               
            }
            catch (Exception ex)
            {
                clickHouseState = false;
                UpdateConnectStateEvent?.Invoke("MQ2CHClickHouse", clickHouseState);
                LogConfig.WriteErrLog("MQ2CH", $"MQ2CH监听ClickHouse服务异常：{ex.Message}\r\n");                
            }


            #endregion

        }

        /// <summary>
        /// 关闭所有服务
        /// </summary>
        /// <returns></returns>
        public static bool StopService()
        {
            try
            {
                //关闭RabbitMQ服务
                StopMQ();
                UpdateConnectStateEvent?.Invoke("MQ2CHMQ", false);

                //关闭Redis服务
                if (redisManagerPool != null)
                {
                    redisManagerPool.Dispose();
                    redisManagerPool = null;
                    LogConfig.WriteRunLog("MQ2CH", $"MQ2CH关闭Redis服务成功");
                }
                UpdateConnectStateEvent?.Invoke("MQ2CHRedis", false);

                //关闭click house数据库服务
                if (httpClient != null)
                {
                    httpClient.Dispose();
                    httpClient = null;
                    LogConfig.WriteRunLog("MQ2CH", $"MQ2CH关闭ClickHouse服务成功");
                }
                UpdateConnectStateEvent?.Invoke("MQ2CHClickHouse", false);
                return true;
            }
            catch (Exception ex)
            {
                LogConfig.WriteErrLog("MQ2CH", $"MQ2CH关闭所有服务异常 Error：{ex.Message}\r\n");
                RichText_InputText("textBox_ShowMsg", $"关闭所有服务异常 @{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}");                
                return false;
            }
        }
    }
}
