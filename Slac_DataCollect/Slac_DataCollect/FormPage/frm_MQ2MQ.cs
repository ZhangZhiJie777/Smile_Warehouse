using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Messaging;
using System.Timers;
using System.Net;
using System.Threading;
using System.Configuration;
using System.IO;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Slac_DataCollect.Utils;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Slac_DataCollect.Modules;

namespace Slac_DataCollect.FormPage
{
    public partial class frm_MQ2MQ : Form
    {
        private Slac_DataMQ2MQModule slac_DataMQ2MQModule; // MQ2MQ模块
        private Thread threadMQ2MQ = null;                 // MQ2MQ线程（每次关闭重启，注意控制生命周期）
        public static ManualResetEvent StopEvent;          // 用于停止线程的事件
        public event Action<string> CloseForm;             // 窗体关闭事件

        public frm_MQ2MQ()
        {
            InitializeComponent();

            this.TopLevel = false;                       //窗体不作为顶级窗体
            this.Dock = DockStyle.Fill;                  //窗体填充父容器
            this.FormBorderStyle = FormBorderStyle.None; //隐藏窗体边框
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
        private void frm_MQ2MQ_Load(object sender, EventArgs e)
        {

            Slac_DataMQ2MQModule.isShowLog = checkBox3.Checked;  // 是否显示日志

            slac_DataMQ2MQModule = new Slac_DataMQ2MQModule();

            slac_DataMQ2MQModule.DataReturnEvent += ShowDataMsg; // 订阅数据返回事件
            slac_DataMQ2MQModule.GetParamConfig();               // 获取参数配置

            if (threadMQ2MQ == null)
            {
                StopEvent = new ManualResetEvent(false); // 手动重置事件
                threadMQ2MQ = new Thread(slac_DataMQ2MQModule.StartService);
                threadMQ2MQ.IsBackground = true;
                threadMQ2MQ.Start();
            }

            // timer1.Enabled = true;
        }

        /// <summary>
        /// 图标右键菜单：显示窗体
        /// </summary>
        private void btn_show_Click(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }

        /// <summary>
        /// 图标右键菜单：退出程序，关服务，关线程，关窗体（异步执行）
        /// </summary>
        public async void btn_closeme_Click(object sender, EventArgs e)
        {
            notifyIcon1.Visible = false;
            bool result = false;

            this.Invoke(new Action(() =>
            {
                result = MessageBox.Show(this, "确定要关闭传输数据模块吗？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes;
            }));

            if (result)
            {
                Task task = Task.Run(() =>
                {
                    slac_DataMQ2MQModule.StopService(); // 停止服务
                });
                await task;
                StopEvent.Set();                        // 设置停止事件以通知MQ2CH线程停止

                if (!threadMQ2MQ.Join(2000))            // 等待线程终止,2秒未终止后强制终止
                {
                    threadMQ2MQ.Abort();
                }

                if (threadMQ2MQ != null && !threadMQ2MQ.IsAlive)
                {
                    threadMQ2MQ = null;                 // 线程终止后，将线程对象置为null
                }

                LogConfig.WriteRunLog("MQ2MQ", "MQ2MQ线程关闭成功\r\n");
                LogConfig.WriteRunLog("MQ2MQ", "———— ———— ———— ———— ———— ———— ————");
                CloseForm?.Invoke("frm_MQ2MQ");         // 触发关闭事件，关闭窗体

                //System.Environment.Exit(0);
            }
            else
            {
                notifyIcon1.Visible = true;
            }
        }

        /// <summary>
        /// 窗体关闭：退出程序，关服务，关线程，关窗体（同步执行）
        /// </summary>
        private void frm_MQ2MQ_FormClosing(object sender, FormClosingEventArgs e)
        {
            notifyIcon1.Visible = false;
            slac_DataMQ2MQModule.StopService();              // 停止服务

            StopEvent.Set();                                 // 设置停止事件以通知MQ2CH线程停止

            if (threadMQ2MQ!= null)                     // 等待线程终止,2秒未终止后强制终止
            {
                threadMQ2MQ.Join(2000);
                if (threadMQ2MQ.IsAlive)
                {
                    threadMQ2MQ.Abort();
                }
                threadMQ2MQ = null;
                LogConfig.WriteRunLog("MQ2MQ", "MQ2MQ线程关闭成功\r\n");
                LogConfig.WriteRunLog("MQ2MQ", "———— ———— ———— ———— ———— ———— ————");
            }

            //if (threadMQ2MQ != null && !threadMQ2MQ.IsAlive) // 关闭线程
            //{
            //    threadMQ2MQ.Abort();
            //    threadMQ2MQ = null;
            //    LogConfig.WriteRunLog("MQ2MQ", "MQ2MQ线程关闭成功\r\n");
            //    LogConfig.WriteRunLog("MQ2MQ", "———— ———— ———— ———— ———— ———— ————");
            //}
        }

        /// <summary>
        /// 窗体图标双击
        /// </summary>
        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            this.Show(); // 窗体显现
            this.WindowState = FormWindowState.Normal; //窗体回复正常大小
        }

        /// <summary>
        /// 窗体大小改变时，窗体自动调整大小
        /// </summary>
        private void frm_MQ2MQ_Resize(object sender, EventArgs e)
        {
            this.Dock = DockStyle.Fill;//窗体填充所在容器控件
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Hide();
            }

            textBox1.Width = textBox2.Width = label3.Width = groupBox2.Width - 20;

            textBox1.Height = textBox2.Height = (groupBox2.Height - 40) * 3 / 8;

            label3.Height = (groupBox2.Height - 40) / 4;

            textBox1.Location = new Point(groupBox2.Location.X + 12, groupBox2.Location.Y - 40);
            textBox2.Location = new Point(textBox1.Location.X, textBox1.Location.Y + textBox1.Height + 10);
            label3.Location = new Point(textBox1.Location.X, textBox2.Location.Y + textBox2.Height + 10);



        }

        /// <summary>
        /// 返回数据更新界面，更新界面日志
        /// </summary>
        /// <param name="location"></param>
        /// <param name="msg"></param>
        public bool ShowDataMsg(string location, string msg)
        {
            try
            {
                this.BeginInvoke(new Action(() =>
                {
                    if (!string.IsNullOrEmpty(location))
                    {
                        if (location.Trim().Equals("textBox1"))
                        {
                            textBox1.Text = msg;
                            //textBox1.AppendText(msg + Environment.NewLine);
                            //textBox1.SelectionStart = textBox1.Text.Length;  // 将光标位置设置为文本末尾
                            //textBox1.ScrollToCaret();                        // 滚动到最新一行
                        }
                        else if (location.Trim().Equals("textBox2"))
                        {
                            textBox2.Text = msg;
                            //textBox2.AppendText(msg + Environment.NewLine);
                            //textBox2.SelectionStart = textBox2.Text.Length;
                            //textBox2.ScrollToCaret();
                        }
                    }
                }));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.ToString()}");
                return false;
                throw;
            }
            return true;


        }

        /// <summary>
        /// 是否显示日志复选框改变
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            Slac_DataMQ2MQModule.isShowLog = checkBox3.Checked;
        }


    }
}
