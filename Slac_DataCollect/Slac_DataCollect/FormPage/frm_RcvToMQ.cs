using RabbitMQ.Client;
using Slac_DataCollect.Modules;
using Slac_DataCollect.Utils;
using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;
using ToolTip = System.Windows.Forms.ToolTip;

namespace Slac_DataCollect.FormPage
{
    public partial class frm_RcvToMQ : Form
    {
        
        private Slac_DataReceiveToMQModule slac_DataReceiveToMQModule; // 数据采集模块
        private Thread collectDataThread = null;                       // 采集数据线程
        public event Action<string> CloseForm;                         // 窗体关闭事件

        private System.Windows.Forms.ToolTip toolTip; // ToolTip控件实例,用于显示鼠标悬停提示信息

        private const int CP_NOCLOSE_BUTTON = 0x200;

        public frm_RcvToMQ()
        {            
            InitializeComponent();

            this.TopLevel = false;                       //窗体不作为顶级窗体
            this.Dock = DockStyle.Fill;                  //窗体填充父容器
            this.FormBorderStyle = FormBorderStyle.None; //隐藏窗体边框            
        }

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
        /// </summary>>
        private void frm_RcvToMQ_Load(object sender, EventArgs e)
        {    
            Slac_DataReceiveToMQModule.DataReturnEvent += ShowDataMsg;       // 注册接收数据，更新界面事件
            Slac_DataReceiveToMQModule.GetParamConfig();                     // 获取参数配置

            label1.Text = "开始数据采集！" + DateTime.Now.ToString() + "\r\n";
            checkBox3.Checked = true;
            if (Slac_DataReceiveToMQModule.isToRemoteMQ == "1")
            {
                checkBox4.Checked = true;
            }
            else
            {
                checkBox4.Checked = false;
            }

            Properties.Settings.Default.IsSaveBin = checkBox1.Checked;      // 是否保存bin
            Properties.Settings.Default.IsSaveLog = checkBox2.Checked;      // 是否保存log
            Properties.Settings.Default.IsShowLog = checkBox3.Checked;      // 是否显示日志
            Properties.Settings.Default.IsSendMQ = checkBox4.Checked;       // 是否直接发送远端MQ
            Properties.Settings.Default.Save();

            slac_DataReceiveToMQModule = new Slac_DataReceiveToMQModule();
            slac_DataReceiveToMQModule.UpdateDataStateEvent += UpdateDataStateColor; // 注册更新数据状态事件

            if (collectDataThread == null)
            {
                collectDataThread = new Thread(new ThreadStart(slac_DataReceiveToMQModule.StartCollectData)); // 启动数据采集线程
                collectDataThread.IsBackground = true;
                collectDataThread.Start();
            }

            if (Slac_DataReceiveToMQModule.MQserver == "127.0.0.1")
            {
                this.Text = this.Text + Slac_DataReceiveToMQModule.MQserver;
                notifyIcon1.Text = "Slac_Data2MQ" + Slac_DataReceiveToMQModule.MQserver;
            }
            else
            {
                this.Text = this.Text + "-Server";
                notifyIcon1.Text = "Slac_Data2MQ-Server";
            }
        }

        /// <summary>
        /// 返回数据更新界面
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public bool ShowDataMsg(string msgLocation, string msg)
        {
            try
            {
                if (!string.IsNullOrEmpty(msgLocation))
                {
                    this.BeginInvoke(new Action(() =>
                    {
                        if (msgLocation.Trim().Equals("label1"))
                        {
                            label1.Text = msg;
                        }
                        else if (msgLocation.Trim().Equals("label2"))
                        {
                            label2.Text = msg;
                        }
                        else if (msgLocation.Trim().Equals("label3"))
                        {
                            label3.Text = msg;
                        }
                    }));
                }
                
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 窗体图标右击按钮显示
        /// </summary>
        private void btn_show_Click(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }

        /// <summary>
        /// 窗体图标关闭事件：关闭服务，关闭线程，关闭窗体 (异步执行)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public async void btn_closeme_Click(object sender, EventArgs e)
        {
            bool result = false;
            notifyIcon1.Visible = false;
            this.Invoke(new Action(() =>
            {
                result = MessageBox.Show(this, "确定要关闭采集数据模块吗？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes;
            }));
            
            if (result)
            {
                //ShowDataMsg("label1", $"正在关闭已开启的服务，请稍候！{DateTime.Now.ToString()}\r\n");
                this.notifyIcon1.Visible = false;

                Task task = Task.Run(() =>
                {
                    Slac_DataReceiveToMQModule.StopService();                // 异步关闭所有服务
                });
                await task;

                collectDataThread.Join(2000);
                if (collectDataThread != null && collectDataThread.IsAlive) // 关闭线程
                {                    
                    collectDataThread.Abort();                    
                }
                collectDataThread = null;
                LogConfig.WriteRunLog("ReceiveMQ", "ReceiveMQ线程关闭成功\r\n");
                LogConfig.WriteRunLog("ReceiveMQ", "———— ———— ———— ———— ———— ———— ————");

                //if (collectDataThread != null && !collectDataThread.IsAlive) // 关闭线程
                //{
                //    collectDataThread.Abort();
                //    collectDataThread = null;
                //}

                CloseForm?.Invoke("frm_RcvToMQ");                            // 窗体关闭，释放资源
            }
            else { notifyIcon1.Visible = true; }
        }

        /// <summary>
        /// 窗体关闭事件：关闭服务，关闭线程，关闭窗体（同步执行）
        /// </summary>
        private void frm_RcvToMQ_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.notifyIcon1.Visible = false;

            Slac_DataReceiveToMQModule.StopService();                    // 同步关闭所有服务                        

            if (collectDataThread != null)
            {
                collectDataThread.Join(2000);
                if (collectDataThread.IsAlive)
                {
                    collectDataThread.Abort();
                }
                collectDataThread = null;
                LogConfig.WriteRunLog("ReceiveMQ", "ReceiveMQ线程关闭成功\r\n");
                LogConfig.WriteRunLog("ReceiveMQ", "———— ———— ———— ———— ———— ———— ————");
            }
            

            //if (collectDataThread != null && !collectDataThread.IsAlive) // 关闭线程
            //{
            //    collectDataThread.Abort();
            //    collectDataThread = null;
            //}
        }

        /// <summary>
        /// 窗体图标双击事件
        /// </summary>
        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            this.Show(); // 窗体显现
            this.WindowState = FormWindowState.Normal; //窗体回复正常大小
        }

        /// <summary>
        /// 窗体大小改变时触发
        /// </summary>
        private void frm_RcvToMQ_Resize(object sender, EventArgs e)
        {
            this.Dock = DockStyle.Fill;//窗体填充所在容器控件
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Hide();
            }

            label1.Height = (groupBox2.Height - 40) / 5;
            label2.Height = label3.Height = (int)((groupBox2.Height - 40) / 5);

            label4.Height = ((groupBox2.Height - 40) * 2 / 5 - 10);

            label1.Location = new Point(groupBox2.Location.X + 12, groupBox2.Location.Y - 40);

            label2.Location = new Point(groupBox2.Location.X + 12, (int)(label1.Location.Y + (groupBox2.Height - 40) / 5 + 10));

            label3.Location = new Point(groupBox2.Location.X + 12, (int)(label2.Location.Y + (groupBox2.Height - 40) / 5 + 10));

            label4.Location = new Point(groupBox2.Location.X + 12, (int)(label3.Location.Y + (groupBox2.Height - 40) / 5 + 10));

            label1.Width = this.Width - 30;
            label2.Width = this.Width - 30;
            label3.Width = this.Width - 30;
            label4.Width = this.Width - 30;

            //数据监控控件中，各控件的位置按比例调整
            textBox1.Location = new Point(label4.Location.X + label4.Width / 10, label4.Location.Y + label4.Height / 5);

            textBox2.Location = new Point(label4.Location.X + label4.Width / 10, textBox1.Location.Y + label4.Height / 5);

            Btn_PlcCollectState.Location = new Point(textBox1.Location.X + textBox1.Width + textBox1.Width / 5,
                                                     textBox1.Location.Y + textBox1.Height / 2 - Btn_PlcCollectState.Height / 2);

            Btn_MqSendState.Location = new Point(textBox2.Location.X + textBox2.Width + textBox2.Width / 5,
                                                 textBox2.Location.Y + textBox1.Height / 2 - Btn_MqSendState.Height / 2);
        }

        #region 窗体复选框属性更改时触发

        /*更新 Setting.setting 文件的变量值*/

        /// <summary>
        /// 保存Bin
        /// </summary>
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.IsSaveBin = checkBox1.Checked;
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// 保存Log
        /// </summary>
        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.IsSaveLog = checkBox2.Checked;
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// 显示日志
        /// </summary>
        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.IsShowLog = checkBox3.Checked;
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// 直接发送到远端MQ
        /// </summary>
        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.IsSendMQ = checkBox3.Checked;
            Properties.Settings.Default.Save();
        }

        #endregion 复选框属性更改时触发

        #region 鼠标悬停，显示提示信息

        /// <summary>
        /// 设置鼠标悬停提示属性
        /// </summary>
        private void SettingToolTip()
        {
            // 创建ToolTip控件实例  
            toolTip = new ToolTip();

            // 设置ToolTip的属性  
            toolTip.AutoPopDelay = 15000; // 提示信息的可见时间（毫秒）  
            toolTip.InitialDelay = 500;   // 事件触发多久后出现提示（毫秒）  
            toolTip.ReshowDelay = 500;    // 指针从一个控件移向另一个控件时，经过多久才会显示下一个提示框（毫秒）  
            toolTip.ShowAlways = true;    // 是否总是显示提示框（即使控件没有焦点）  
        }

        /// <summary>
        /// 鼠标悬停，显示提示信息：PLC采集状态
        /// </summary>
        private void Btn_PlcCollectState_MouseHover(object sender, EventArgs e)
        {
            SettingToolTip();
            // 设置提示信息  
            toolTip.SetToolTip(Btn_PlcCollectState, "红：PLC连接，UDP通讯异常\r\n" +
                                                    "绿：PLC数据正常采集中\r\n" +
                                                    "黄：PLC连接正常，未采集到数据\r\n" +
                                                    "灰：PLC数据状态异常，未能获取当前状态");
        }

        /// <summary>
        /// 鼠标悬停，显示提示信息：MQ发送状态
        /// </summary>
        private void Btn_MqSendState_MouseHover(object sender, EventArgs e)
        {
            SettingToolTip();
            // 设置提示信息  
            toolTip.SetToolTip(Btn_MqSendState, "红：MQ服务未连接\r\n" +
                                                "绿：MQ数据正常上传中\r\n" +
                                                "黄：MQ服务连接正常，无数据上传\r\n" +
                                                "灰：MQ数据状态异常，未能获取当前状态");
        }
        #endregion

        /// <summary>
        /// 监控各种PLC采集、MQ上传数据状态：
        /// <param name="updateLocation"></param>
        /// <param name="state"> 
        /// 1：绿色，数据正常处理
        /// 0：红色，无连接
        /// 2：黄色：连接正常，无数据
        /// 3：灰色：数据异常，未能获取当前状态
        /// </param>
        public void UpdateDataStateColor(string updateLocation, int state)
        {
            this.BeginInvoke(new Action(() =>
            {
                if (!string.IsNullOrEmpty(updateLocation))
                {
                    if (updateLocation.Trim() == "PlcCollectState")
                    {
                        switch (state)
                        {
                            case 0:
                                Btn_PlcCollectState.BackColor = Color.Red;
                                break;
                            case 1:
                                Btn_PlcCollectState.BackColor = Color.Green;
                                break;
                            case 2:
                                Btn_PlcCollectState.BackColor = Color.Yellow;
                                break;                            
                            case 3:
                                Btn_PlcCollectState.BackColor = Color.Gray;
                                break;
                        }
                    }
                    else if (updateLocation.Trim() == "MQSendState")
                    {
                        switch (state)
                        {
                            case 1:
                                Btn_MqSendState.BackColor = Color.Green;
                                break;
                            case 2:
                                Btn_MqSendState.BackColor = Color.Yellow;
                                break;
                            case 0:
                                Btn_MqSendState.BackColor = Color.Red;
                                break;
                            case 3:
                                Btn_MqSendState.BackColor = Color.Gray;
                                break;
                        }
                    }
                }
               
            }));
        }

        /*
         --2024.6.15 cwz 发送到bak的队列。停用，在数据解析阶段再传输到远端 。
        private string SendtoMQ_2(string MQ_exchange, string line_ID, byte[] SendMsg)
        {
            try
            {
                if (conn.IsOpen)
                {
                    channel.BasicPublish(MQ_exchange, line_ID, properties, SendMsg);
                    if (checkBox3.Checked)
                    {
                        label3.Text = "传输成功！ @ " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    }
                    return "0";
                }
                else
                {
                    label3.Text = "传输失败！ @ " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    return "error:100";
                }
            }
            catch (Exception ex)
            {
                label3.Text = "异常" + ex.ToString() + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                return "error:101";
            }
        }
        */
    }
}