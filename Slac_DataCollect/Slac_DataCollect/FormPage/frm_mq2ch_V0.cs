using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Net;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using SlacDataCollect;
using ServiceStack.Redis;
using Slac_DataCollect.Utils;
using System.Messaging;
using Slac_DataCollect.Common;
using Slac_DataCollect.Modules;
using System.Diagnostics;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;



namespace Slac_DataCollect.FormPage
{
    /// <summary>
    /// 
    /// 用于将MQ队列的消息解析并存储到CH数据库
    /// </summary>
    public partial class frm_mq2ch_V0 : Form
    {
        private Slac_DataMQ2CHModule MQ2CH = null;

        private Thread threadMQ2CH = null;            // MQ2CH线程

        public static ManualResetEvent StopEvent;     // 用于停止线程的事件 

        public event Action<string> CloseForm;        // 窗体关闭       

        public frm_mq2ch_V0()
        {
            InitializeComponent();

            this.TopLevel = false;                       // 窗体不作为顶级窗体
            this.Dock = DockStyle.Fill;                  // 窗体填充父容器
            this.FormBorderStyle = FormBorderStyle.None; // 隐藏窗体边框
        }

        private const int CP_NOCLOSE_BUTTON = 0x200;
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams myCp = base.CreateParams;
                myCp.ClassStyle = myCp.ClassStyle | CP_NOCLOSE_BUTTON;
                return myCp;
            }
        }

        /// <summary>
        /// 加载页面
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void frm_mq2ch_Load(object sender, EventArgs e)
        {
            Slac_DataMQ2CHModule.GetParamConfig();  // 获取MQ2CH配置参数

            if (Slac_DataMQ2CHModule.is2MQbak == "1")
            {
                checkBox1.Checked = true;
            }

            btn_Save2DB.Text = "停止数据解析";
            textBox_ShowMsg.AppendText($"开始数据解析 @ {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}" + Environment.NewLine);
            textBox_ShowMsg.ScrollToCaret();            // 滚动到最新一行

            this.Text = "SLAC数据采集系统2.1-解析:" + Slac_DataMQ2CHModule.MQexchange + "_Line" + Slac_DataMQ2CHModule.lineID;
            this.notifyIcon1.Text = "Slac_DataMQ2CH: " + Slac_DataMQ2CHModule.MQexchange + "_Line" + Slac_DataMQ2CHModule.lineID;

            Slac_DataMQ2CHModule.IsSaveBin = checkBox4.Checked;
            Slac_DataMQ2CHModule.IsShowSendLog = checkBox2.Checked;

            Slac_DataMQ2CHModule.DataReturnEvent += ShowMsgData;        // 绑定数据返回事件

            MQ2CH = new Slac_DataMQ2CHModule();


            if (threadMQ2CH == null)
            {
                StopEvent = new ManualResetEvent(false); // 手动重置事件
                threadMQ2CH = new Thread(new ThreadStart(MQ2CH.StartService)); // 启动线程：接收消息队列，处理数据，保存数据库
                threadMQ2CH.IsBackground = true;
                threadMQ2CH.Start();

            }
            //Console.WriteLine($"页面加载结束{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}");           

        }

        /// <summary>
        /// 返回数据更新界面
        /// </summary>
        /// <param name="msgLocation">数据显示位置</param>
        /// <param name="msg">返回数据</param>
        /// <returns></returns>
        private bool ShowMsgData(string msgLocation, string msg)
        {
            try
            {
                this.BeginInvoke(new Action(() =>
                {
                    if (msgLocation.Trim() == "label1")
                    {
                        label1.Text = msg;
                    }
                    if (msgLocation.Trim() == "textBox_ShowMsg")
                    {
                        if (!string.IsNullOrEmpty(msg))
                        {
                            textBox_ShowMsg.AppendText(msg + Environment.NewLine);
                            textBox_ShowMsg.ScrollToCaret(); // 滚动到最新一行
                        }
                    }

                }));
                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }

        /// <summary>
        /// 窗体大小改变时触发
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void frm_mq2ch_Resize(object sender, EventArgs e)
        {
            this.Dock = DockStyle.Fill;//窗体填充所在容器控件
            if (this.WindowState == FormWindowState.Minimized)
            {
                notifyIcon1.Visible = true;
                this.Hide();
            }

            textBox_ShowMsg.Width = this.Width - 20;
            textBox_ShowMsg.Height = (this.Height - btn_Save2DB.Height - 60) / 2;
            textBox_ShowMsg.Location = new Point(12, btn_Save2DB.Location.Y + btn_Save2DB.Height + 10);

            label1.Width = this.Width - 20;
            label1.Height = (this.Height - btn_Save2DB.Height - 60) / 6;
            label1.Location = new Point(12, textBox_ShowMsg.Location.Y + textBox_ShowMsg.Height + 10);

            label2.Width = this.Width - 20;
            label2.Height = (this.Height - btn_Save2DB.Height - 60) / 3;
            label2.Location = new Point(12, label1.Location.Y + label1.Height + 10);
        }

        /// <summary>
        /// 按钮触发数据解析事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_Save2DB_Click(object sender, EventArgs e)
        {
            try
            {
                if (Slac_DataMQ2CHModule.isPause_save2db)
                {
                    Slac_DataMQ2CHModule.isPause_save2db = false;
                    btn_Save2DB.Text = "停止数据解析";

                    textBox_ShowMsg.AppendText($"开始数据解析 @ {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}" + Environment.NewLine);
                    textBox_ShowMsg.ScrollToCaret();    // 滚动到最新一行 
                    MQ2CH.Run_ReciveInfo();
                }
                else
                {
                    Slac_DataMQ2CHModule.isPause_save2db = true;
                    btn_Save2DB.Text = "开始数据解析";

                    textBox_ShowMsg.AppendText($"停止数据解析 @ {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}" + Environment.NewLine);
                    textBox_ShowMsg.ScrollToCaret();    // 滚动到最新一行

                    Slac_DataMQ2CHModule.channel.BasicCancel(Slac_DataMQ2CHModule.consumer.ConsumerTag); //取消订阅，停止从RabbitMQ接收消息
                    LogConfig.WriteRunLog("MQ2CH", $"停止数据解析，RabbitMQ消费者取消订阅成功");
                    Thread.Sleep(1000);
                    if (MQ2CH != null)
                    {
                        if (Slac_DataMQ2CHModule.ListCount > 0)
                        {
                            MQ2CH.saveToCK(Slac_DataMQ2CHModule.RcvTime);
                        }
                        if (Slac_DataMQ2CHModule.LogListCount > 0)
                        {
                            MQ2CH.saveLogToCK(Slac_DataMQ2CHModule.RcvTime);
                        }
                        LogConfig.WriteRunLog("MQ2CH", $"停止数据解析，数据提交上传ClickHouse数据库成功");
                    }
                    

                }
            }
            catch (Exception ex)
            {
                LogConfig.WriteErrLog("MQ2CH", $"数据解析按钮异常：{ex.Message}");
                throw;
            }

        }

        /// <summary>
        /// 图标双击显示窗体
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            this.Show(); // 窗体显现
            this.WindowState = FormWindowState.Normal; //窗体回复正常大小
        }

        /// <summary>
        /// 显示窗体
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_show_Click(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }

        public bool isFormClosing = false;  // 标志变量，表示是否已经在关闭窗体 

        /// <summary>
        /// 关闭窗体(异步关闭服务，线程)
        /// </summary>
        public async void btn_closeme_Click(object sender, EventArgs e)
        {
            bool result = false;
            this.Invoke(new Action(() =>
            {
                notifyIcon1.Visible = true;
                result = MessageBox.Show(this, "确定要关闭存储数据模块吗？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes;

            }));


            if (result)
            {
                isFormClosing = true;       // 设置标志变量为true，表示正在关闭窗体
                Slac_DataMQ2CHModule.isPause_save2db = true;

                this.BeginInvoke(new Action(() =>
                {
                    btn_Save2DB.Text = "开始数据解析";

                    textBox_ShowMsg.AppendText("暂停数据解析 @ " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "\r\n");
                    textBox_ShowMsg.AppendText("关闭前保存CH数据库 @ " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "\r\n");
                    textBox_ShowMsg.ScrollToCaret();
                }));

                Thread.Sleep(500);
                //关闭时，将数据和日志保存到CH
                if (MQ2CH != null)
                {
                    if (Slac_DataMQ2CHModule.ListCount >= 0)
                    {
                        MQ2CH.saveToCK(Slac_DataMQ2CHModule.RcvTime);
                    }
                    if (Slac_DataMQ2CHModule.LogListCount > 0)
                    {
                        MQ2CH.saveLogToCK(Slac_DataMQ2CHModule.RcvTime);
                    }
                    LogConfig.WriteRunLog("MQ2CH", "关闭前上传ClickHouse数据库成功");
                }

                Task task = Task.Run(() =>
                {
                    Slac_DataMQ2CHModule.StopService(); // 关闭所有服务                    
                });
                await task;

                StopEvent.Set();                        // 设置停止事件以通知MQ2CH线程停止

                if (threadMQ2CH != null)
                {
                    threadMQ2CH.Join(2000);             // 等待MQ2CH线程完成,才会继续执行(若两秒内等待线程未结束，强制关闭线程)
                    if (threadMQ2CH.IsAlive)
                    {
                        threadMQ2CH.Abort();
                    }
                    threadMQ2CH = null;
                    LogConfig.WriteRunLog("MQ2CH", "MQ2CH线程已关闭\r\n");
                    LogConfig.WriteRunLog("MQ2CH", "———— ———— ———— ———— ———— ———— ————");
                }                

                CloseForm?.Invoke("frm_mq2ch_V0");      // 窗体关闭，释放资源
            }
            else
            {
                notifyIcon1.Visible = true;
            }

        }

        /// <summary>
        /// 关闭窗口（同步关闭服务，线程）
        /// </summary>
        private void frm_mq2ch_V0_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!isFormClosing)  // 防止重复关闭
            {
                notifyIcon1.Visible = false;
                if (MQ2CH != null)
                {
                    if (Slac_DataMQ2CHModule.ListCount >= 0)
                    {
                        MQ2CH.saveToCK(Slac_DataMQ2CHModule.RcvTime);
                    }
                    if (Slac_DataMQ2CHModule.LogListCount > 0)
                    {
                        MQ2CH.saveLogToCK(Slac_DataMQ2CHModule.RcvTime);
                    }
                }

                Slac_DataMQ2CHModule.StopService(); // 关闭所有服务

                StopEvent.Set();                    // 设置停止事件以通知MQ2CH线程停止

                if (threadMQ2CH != null)
                {
                    threadMQ2CH.Join(2000);
                    if (threadMQ2CH.IsAlive)
                    {
                        threadMQ2CH.Abort();
                    }
                    threadMQ2CH = null;
                    LogConfig.WriteRunLog("MQ2CH", "MQ2CH线程已关闭\r\n");
                    LogConfig.WriteRunLog("MQ2CH", "———— ———— ———— ———— ———— ———— ————");
                }      
                
            }

        }

        /// <summary>
        /// 是否显示发送日志
        /// </summary>
        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            Slac_DataMQ2CHModule.IsShowSendLog = checkBox2.Checked;
        }

        /// <summary>
        /// 是否保存异常数据
        /// </summary>
        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            Slac_DataMQ2CHModule.IsSaveBin = checkBox4.Checked;
        }
    }
}
