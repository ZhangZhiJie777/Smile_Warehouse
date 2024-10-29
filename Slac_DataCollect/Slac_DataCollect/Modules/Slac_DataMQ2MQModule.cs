using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using ServiceStack.Messaging;
using Slac_DataCollect.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.IO;
using System.Threading;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;
using Slac_DataCollect.DatabaseSql.DBModel;
using Slac_DataCollect.DatabaseSql.DBOper;
using Slac_DataCollect.FormPage;

namespace Slac_DataCollect.Modules
{
    /// <summary>
    /// MQ2MQ模块：接收MQ消息，将数据发送到远端MQ服务器
    /// </summary>
    public class Slac_DataMQ2MQModule
    {
        public event Func<string, string, bool> DataReturnEvent;           // 用于返回数据，更新界面
        public static event Action<string, bool> UpdateConnectStateEvent;  // 用于返回连接状态，更新界面
        public static bool isShowLog;

        #region 本地RabbitMQ
        private static string LineID;           // 线体ID(用于拼接成消息队列名称)
        private static string MQserver;         // IP
        private static string MQexchange;       // 交换机名称
        private static string Password;         // 密码

        private static IConnection conn = null; // 连接
        private static IModel channel = null;   // 通道
        private static string isDurable;        // 消息队列是否持久化

        private static EventingBasicConsumer consumer; // 消费者
        // 消费的消息队列名称
        private static string QueueName = MQexchange + "_line" + LineID + "_bak";// "slac_line1003"; 

        private static string TTL_days; // 消息队列中数据的过期时间(如果修改过期时间，那消息队列要么删除重新声明，要么修改消息队列)

        //现场使用
        private static ConnectionFactory factory = new ConnectionFactory() { HostName = MQserver, Port = 5672, UserName = MQexchange, Password = "slac1028", AutomaticRecoveryEnabled = true };

        //本地调试        
        //private static ConnectionFactory factory = new ConnectionFactory() { HostName = "localhost", Port = 5672, UserName = "guest", Password = "guest", AutomaticRecoveryEnabled = true };

        #endregion

        #region 远端RabbitMQ
        private static string MQserver_132;                    // IP
        private static string MQexchange_132;                  // 交换机名称
        private static string LineID_132;                      // 线体ID(用于拼接成消息队列名称)
        private static string Password_132;                    // 密码
        private static IConnection conn_132 = null;            // 连接
        private static IModel channel_132 = null;              // 通道
        private static IBasicProperties properties_132 = null; // 消息属性
        private static string isDurable_132;                   // 消息的持久属性（不是消息队列，是消息）
        //现场使用
        private static ConnectionFactory factory_132 = new ConnectionFactory() { HostName = MQserver_132, Port = 5672, UserName = MQexchange_132, Password = "slac1028", AutomaticRecoveryEnabled = true };

        //本地调试        
        //private static ConnectionFactory factory_132 = new ConnectionFactory() { HostName = "localhost", Port = 5672, UserName = "guest", Password = "guest", AutomaticRecoveryEnabled = true };

        #endregion

        public Slac_DataMQ2MQModule()
        {

        }

        /// <summary>
        /// 获取配置参数
        /// </summary>
        public void GetParamConfig()
        {
            try
            {
                DBSystemConfig dbSystemConfig = new DBSystemConfig();   //配置类
                DBOper.Init();
                DBOper db = new DBOper();
                List<DBSystemConfig> list = db.QueryList(dbSystemConfig);

                TTL_days = list.Find(e => e.Name.Trim() == "MQ2MQ_TTL_days").Value.Trim();

                //本地 RabbitMQ
                MQserver = list.Find(e => e.Name.Trim() == "MQ2MQ_MQserver").Value.Trim().Split('|')[0];
                MQexchange = list.Find(e => e.Name.Trim() == "MQ2MQ_MQserver").Value.Trim().Split('|')[1];
                LineID = list.Find(e => e.Name.Trim() == "MQ2MQ_MQserver").Value.Trim().Split('|')[2];
                Password = list.Find(e => e.Name.Trim() == "MQ2MQ_MQserver").Value.Trim().Split('|')[3];
                isDurable = list.Find(e => e.Name.Trim() == "MQ2MQ_MQserver").Value.Trim().Split('|')[4];

                //远端 RabbitMQ
                MQserver_132 = list.Find(e => e.Name.Trim() == "MQ2MQ_MQserver_remote").Value.Trim().Split('|')[0];
                MQexchange_132 = list.Find(e => e.Name.Trim() == "MQ2MQ_MQserver_remote").Value.Trim().Split('|')[1];
                LineID_132 = list.Find(e => e.Name.Trim() == "MQ2MQ_MQserver_remote").Value.Trim().Split('|')[2];
                Password_132 = list.Find(e => e.Name.Trim() == "MQ2MQ_MQserver_remote").Value.Trim().Split('|')[3];
                isDurable_132 = list.Find(e => e.Name.Trim() == "MQ2MQ_MQserver_remote").Value.Trim().Split('|')[4];

            }
            catch (Exception ex)
            {
                LogConfig.WriteErrLog("MQ2MQ", $"获取数据库配置参数异常Error：{ex.ToString()}\r\n");
                ShowDataMsg("textBox2", $"请检查数据库配置,读取数据库配置参数异常！");
            }
        }

        /// <summary>
        /// 开始服务
        /// </summary>
        public void StartService()
        {
            LogConfig.WriteRunLog("MQ2MQ", "———— ———— ———— ———— ———— ———— ————\r\n");
            LogConfig.WriteRunLog("MQ2MQ", "MQ2MQ模块开始");
            //连接MQ
            connectMQ();

            //定时器监控MQ连接状态，数据状态
            //每隔一秒监控两个MQ的服务连接和通道状态
            while (!frm_MQ2MQ.StopEvent.WaitOne(1000))
            {
                try
                {
                    if (conn == null || !conn.IsOpen || !channel.IsOpen)
                    {
                        UpdateConnectStateEvent?.Invoke("LocalMQServer", false);
                    }

                    if (conn_132 == null || !conn_132.IsOpen || !channel_132.IsOpen)
                    {
                        UpdateConnectStateEvent?.Invoke("RemoteMQServer", false);
                    }
                }
                catch (Exception ex)
                {
                    LogConfig.WriteErrLog("MQ2MQ", $"MQ2MQ服务监控异常：{ex.Message}\r\n");
                    ShowDataMsg("textBox2", DateTime.Now.ToString() + "MQ2MQ服务监控异常\r\n");
                    UpdateConnectStateEvent?.Invoke("LocalMQServer", false);
                    UpdateConnectStateEvent?.Invoke("RemoteMQServer", false);
                    //break;
                }
            }
            LogConfig.WriteRunLog("MQ2MQ", "MQ2MQ模块结束");
        }

        /// <summary>
        /// 连接MQ
        /// </summary>
        public void connectMQ()
        {
            try
            {
                QueueName = MQexchange + "_line" + LineID + "_bak";// "slac_line1003";
                factory = new ConnectionFactory() { HostName = MQserver, Port = 5672, UserName = MQexchange, Password = Password, AutomaticRecoveryEnabled = true };
                factory_132 = new ConnectionFactory() { HostName = MQserver_132, Port = 5672, UserName = MQexchange_132, Password = Password_132, AutomaticRecoveryEnabled = true };

                LogConfig.WriteRunLog("MQ2MQ", $"本地RabbitMQ服务：IP：{MQserver}，端口：{5672}，用户名：{MQexchange}，密码：{Password}");
                LogConfig.WriteRunLog("MQ2MQ", $"远端RabbitMQ服务：IP：{MQserver_132}，端口：{5672}，用户名：{MQexchange_132}，密码：{Password_132}");

                // 设置本地MQ连接，通道
                // 声明一个持久的、非自动删除的、非排他的队列，并设置预取机制为每次只处理一条消息。
                if (conn == null || !conn.IsOpen)
                {
                    Cleanup();
                    Dictionary<string, object> arguments = null; // 存储声明队列时的额外参数，键值对形式
                    if (TTL_days != "0")
                    {
                        try
                        {
                            int days = Convert.ToInt32(TTL_days);
                            // x-message-ttl是消息队列参数表示消息存活的最大时间，超出将被丢弃，单位毫秒
                            arguments = new Dictionary<string, object> { { "x-message-ttl", days * 24 * 3600 * 1000 } };

                            LogConfig.WriteRunLog("MQ2MQ", $"本地RabbitMQ消息队列存活最大时间(TTL天数)：{days}天");
                        }
                        catch
                        {
                            DataReturnEvent?.Invoke("textBox2", DateTime.Now.ToString() + "TTL天数配置不正确!");
                            LogConfig.WriteErrLog("MQ2MQ", "TTL天数配置不正确!");
                        }
                    }

                    conn = factory.CreateConnection();
                    channel = conn.CreateModel();
                    //每个消费者最多消费一条消息,没返回消息确认之前不再接收消息，设置预取机制为每次只处理一条消息
                    channel.BasicQos(0, 1, false);

                    // 设置消息队列是否持久化
                    if (int.TryParse(isDurable, out int isdurable) && (isdurable == 1 || isdurable == 2))
                    {
                        bool IsDurable = isdurable != 1;
                        // 声明一个持久的、非自动删除的、非排他的队列
                        channel.QueueDeclare(QueueName,  // 队列名称
                                             IsDurable,  // 是否持久化（队列在服务器重启后仍然存在）
                                             false,      // 是否自动删除（无消费者连接，是否删除队列）
                                             false,      // 是否排他（其他连接也可以访问这个队列）
                                             arguments); // 额外参数（如消息的过期时间）
                    }
                    else
                    {
                        LogConfig.WriteErrLog("MQ2MQ", $"本地RabbitMQ消息队列持久化属性配置错误，请检查数据库配置！");
                        ShowDataMsg("textBox2", DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff") + " 本地RabbitMQ消息队列持久化属性配置错误，请检查数据库配置！");
                    }
                    
                    if (conn != null && conn.IsOpen && channel != null && channel.IsOpen)
                    {
                        UpdateConnectStateEvent?.Invoke("LocalMQServer", true); // 更新连接状态
                    }

                    LogConfig.WriteRunLog("MQ2MQ", $"本地RabbitMQ服务启动成功：消费的消息队列名称：{QueueName}");
                    ShowDataMsg("textBox2", DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff") + " 本地MQ服务器连接成功！");
                }

                // 设置远端MQ连接，通道，消息属性持久化,消息会被存储在磁盘上，即使服务器重启也不会丢失
                if (conn_132 == null || conn_132.IsOpen == false)
                {
                    Cleanup132();
                    conn_132 = factory_132.CreateConnection();
                    channel_132 = conn_132.CreateModel();
                    properties_132 = channel_132.CreateBasicProperties();

                    // 设置消息属性是否持久化
                    if(int.TryParse(isDurable_132,out int isdurable_132) && (isdurable_132 == 1 || isdurable_132 == 2))
                    {                        
                        properties_132.DeliveryMode = (byte)isdurable_132; // 1:非持久化 2:持久化
                    }
                    else
                    {
                        LogConfig.WriteErrLog("MQ2MQ", $"远端RabbitMQ消息持久化属性配置错误，请检查数据库配置！");
                        ShowDataMsg("textBox2", DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff") + " 远端RabbitMQ消息队列持久化属性配置错误，请检查数据库配置！");
                    }

                    Run_ReciveInfo();

                    if (conn_132 != null && conn_132.IsOpen && channel_132 != null && channel_132.IsOpen)
                    {
                        UpdateConnectStateEvent?.Invoke("RemoteMQServer", true); // 更新连接状态
                    }

                    LogConfig.WriteRunLog("MQ2MQ", $"远端RabbitMQ服务启动成功：MQexchange：{MQexchange_132}，路由键：{"line" + LineID_132}");
                    ShowDataMsg("textBox2", DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff") + " 远端MQ服务器连接成功！");
                }
            }
            catch (Exception ex)
            {
                LogConfig.WriteErrLog("MQ2MQ", $"本地、远端MQ服务器连接失败：{ex.Message}\r\n");
                ShowDataMsg("textBox2", DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff") + "  MQ服务器连接失败：");
            }
        }

        /// <summary>
        /// 接收本地MQ消息队列数据，发送到远端MQ消息队列
        /// </summary>
        private void Run_ReciveInfo()
        {
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
                consumer = new EventingBasicConsumer(channel);
                //消费消息 autoAck参数为消费后是否删除
                channel.BasicConsume(QueueName, false, consumer);   //true:自动删除  false: 不自动删除
                //自动接收消息事件：被动读取事件，只要客户端推送消息 ，这边被动接收 
                consumer.Received += (model, ea) =>
                {
                    var MQmsg = ea.Body;
                    string result = SendtoMQ(MQmsg);
                    if (result == "0")
                    {
                        //手动向rabbitmq发送消息确认
                        //当246行,为false时需要手动ack才能删除消息  
                        channel.BasicAck(ea.DeliveryTag, false);
                        ShowDataMsg("textBox1", "成功！ @ " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                    }
                    else
                    {
                        channel.BasicNack(ea.DeliveryTag, false, true);  //设置为true，则消息重入队列；如果设置为false，则消息被丢弃或发送到死信Exchange
                        ShowDataMsg("textBox1", "失败！ @ " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                    }
                    Thread.Sleep(0);
                };
            }
            catch (Exception ex)
            {
                LogConfig.WriteErrLog("MQ2MQ", $"接收本地MQ消息队列数据，消费消息队列异常：{ex.Message}\r\n");
            }
        }

        /// <summary>
        /// 传输数据到远端MQ服务器
        /// </summary>
        /// <param name="SendMsg"></param>
        /// <returns></returns>
        private string SendtoMQ(byte[] SendMsg)
        {
            try
            {
                if (!conn_132.IsOpen)
                {
                    connectMQ();
                }

                if (conn_132.IsOpen)
                {
                    channel_132.BasicPublish(MQexchange_132, "line" + LineID_132, properties_132, SendMsg);
                    if (isShowLog)
                    {
                        ShowDataMsg("textBox2", "传输成功！ @ " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                    }
                    return "0";
                }
                else
                {
                    ShowDataMsg("textBox2", "传输失败！ @ " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                    return "error:100";
                }
            }
            catch (Exception ex)
            {
                LogConfig.WriteErrLog("MQ2MQ", $"发送远端MQ消息队列数据异常：{ex.Message}");
                return "error:101";
            }
        }

        /// <summary>
        /// 返回界面，显示数据信息
        /// </summary>
        /// <param name="location"></param>
        /// <param name="msg"></param>
        private void ShowDataMsg(string location, string msg)
        {
            DataReturnEvent?.Invoke(location, msg);
        }

        /// <summary>
        /// 关闭本地MQ连接：每次重连前，关闭之前的连接
        /// </summary>
        private void Cleanup()
        {
            try
            {

                if (channel != null && channel.IsOpen)
                {
                    try
                    {
                        channel.Close();
                    }
                    catch (Exception ex)
                    {
                        LogConfig.WriteErrLog("MQ2MQ", $"关闭本地RabbitMQ通道异常：{ex.Message}\r\n");
                        ShowDataMsg("textBox2", "RabbitMQ重新连接，正在尝试关闭之前的Channel[发送]，但遇到错误");
                    }
                    channel = null;
                    LogConfig.WriteRunLog("MQ2MQ", $"关闭本地RabbitMQ通道成功");
                }



                if (conn != null && conn.IsOpen)
                {
                    try
                    {
                        conn.Close();
                    }
                    catch (Exception ex)
                    {
                        LogConfig.WriteErrLog("MQ2MQ", $"关闭本地RabbitMQ连接异常：{ex.Message}\r\n");
                        ShowDataMsg("textBox2", "RabbitMQ重新连接，正在尝试关闭之前的连接，但遇到错误");
                    }
                    conn = null;
                    LogConfig.WriteRunLog("MQ2MQ", $"关闭本地RabbitMQ连接成功");
                }

            }
            catch (IOException ex)
            {
                LogConfig.WriteErrLog("MQ2MQ", $"关闭本地RabbitMQ服务异常：{ex.Message}\r\n");
                throw ex;
            }
        }

        /// <summary>
        /// 关闭远端MQ连接：每次重连前，关闭之前的连接
        /// </summary>
        private void Cleanup132()
        {
            try
            {



                if (channel_132 != null && channel_132.IsOpen)
                {
                    try
                    {
                        channel_132.Close();
                    }
                    catch (Exception ex)
                    {
                        LogConfig.WriteErrLog("MQ2MQ", $"关闭远端RabbitMQ通道异常：{ex.Message}\r\n");
                        ShowDataMsg("textBox2", "远端RabbitMQ重新连接，正在尝试关闭之前的Channel[接收]，但遇到错误");
                    }
                    channel_132 = null;
                    LogConfig.WriteRunLog("MQ2MQ", $"关闭远端RabbitMQ通道成功");
                }

                if (conn_132 != null && conn_132.IsOpen)
                {
                    try
                    {
                        conn_132.Close();
                    }
                    catch (Exception ex)
                    {
                        LogConfig.WriteErrLog("MQ2MQ", $"关闭远端RabbitMQ连接异常：{ex.Message}\r\n");
                        ShowDataMsg("textBox2", "远端RabbitMQ重新连接，正在尝试关闭之前的连接，但遇到错误");
                    }
                    conn_132 = null;
                    LogConfig.WriteRunLog("MQ2MQ", $"关闭远端RabbitMQ连接成功");
                }
            }
            catch (IOException ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 关闭所有服务
        /// </summary>
        public void StopService()
        {
            try
            {
                Cleanup();    // 关闭本地MQ连接
                Cleanup132(); // 关闭远端MQ连接

                UpdateConnectStateEvent?.Invoke("RemoteMQServer", false); // 通知UI更新本地MQ连接状态
                UpdateConnectStateEvent?.Invoke("LocalMQServer", false);  // 通知UI更新远端MQ连接状态

            }
            catch (Exception)
            {
                ShowDataMsg("textBox2", "停止服务时发生错误");
            }
        }
    }
}
