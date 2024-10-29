using Slac_DataCollect.DatabaseSql.DBModel;
using Slac_DataCollect.DatabaseSql.DBOper;
using Slac_DataCollect.FormPage;
using Slac_DataCollect.Modules;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Slac_DataCollect
{
    public partial class MainForm : Form
    {

        private frm_RcvToMQ frm_RcvToMQ = null;    // RcvToMQ
        private frm_mq2ch_V0 frm_mq2ch_V0 = null;  // MQ2CH
        private frm_MQ2MQ frm_MQ2MQ = null;        // MQ2MQ

        private static String RcvToMQ_Permissions; // RcvToMQ模块权限
        private static String MQ2CH_Permissions;   // MQ2CH模块权限
        private static String MQ2MQ_Permissions;   // MQ2MQ模块权限

        private System.Windows.Forms.ToolTip toolTip; // ToolTip控件实例,用于显示鼠标悬停提示信息

        public MainForm()
        {
            InitializeComponent();

        }

        /// <summary>
        /// 主页面加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_Load(object sender, EventArgs e)
        {
            try
            {
                #region 模块权限管理
                GetParam();
                if (RcvToMQ_Permissions != "1")
                {
                    panel2.Enabled = false;
                    panel2.BackColor = Color.Gray;
                    btn_form1.BackColor = Color.Gray;
                    Btn_PlcState.BackColor = Color.Gray;
                    Btn_RcvMQ.BackColor = Color.Gray;
                    Btn_RevMQStop.BackColor = Color.Gray;
                }
                if (MQ2CH_Permissions != "1")
                {
                    panel3.Enabled = false;
                    panel3.BackColor = Color.Gray;
                    btn_form2.BackColor = Color.Gray;
                    Btn_MQ2CHMQ.BackColor = Color.Gray;
                    Btn_MQ2CHClickHouse.BackColor = Color.Gray;
                    Btn_MQ2CHRedis.BackColor = Color.Gray;
                    Btn_MQ2CHStop.BackColor = Color.Gray;
                }
                if (MQ2MQ_Permissions != "1")
                {
                    panel4.Enabled = false;
                    panel4.BackColor = Color.Gray;
                    btn_form3.BackColor = Color.Gray;
                    Btn_MQLocal.BackColor = Color.Gray;
                    Btn_RemoteMQServer.BackColor = Color.Gray;
                    Btn_MQ2MQStop.BackColor = Color.Gray;
                } 
                #endregion

                ChangePanel();
                Slac_DataReceiveToMQModule.UpdateConnectStateEvent += UpdateConnectStateColor; // ReceiveToMQ订阅更新连接状态事件
                Slac_DataMQ2CHModule.UpdateConnectStateEvent += UpdateConnectStateColor;       // MQ2CH订阅更新连接状态事件
                Slac_DataMQ2MQModule.UpdateConnectStateEvent += UpdateConnectStateColor;       // MQ2MQ订阅更新连接状态事件
            }
            catch (Exception ex)
            {
                this.Dispose();
                Environment.Exit(0);
            }
            
        }

        #region 界面切换
        /// <summary>
        /// 切换采集数据模块
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_form1_Click(object sender, EventArgs e)
        {
            if (frm_RcvToMQ != null)
            {
                frm_RcvToMQ.Show();
                panel1.Controls.Clear();
                panel1.Controls.Add(frm_RcvToMQ);
            }
            else
            {
                frm_RcvToMQ = new frm_RcvToMQ();
                frm_RcvToMQ.Show();
                panel1.Controls.Clear();
                panel1.Controls.Add(frm_RcvToMQ);
                frm_RcvToMQ.CloseForm += CloseForm;
                btn_form1.BackColor = Color.Green;
            }
        }

        /// <summary>
        /// 切换存储数据模块
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_form2_Click(object sender, EventArgs e)
        {
            if (frm_mq2ch_V0 != null)
            {
                frm_mq2ch_V0.Show();
                panel1.Controls.Clear();
                panel1.Controls.Add(frm_mq2ch_V0);
            }
            else
            {
                frm_mq2ch_V0 = new frm_mq2ch_V0();
                frm_mq2ch_V0.Show();
                panel1.Controls.Clear();
                panel1.Controls.Add(frm_mq2ch_V0);
                frm_mq2ch_V0.CloseForm += CloseForm;
                btn_form2.BackColor = Color.Green;

            }
        }
        /// <summary>
        /// 切换传输数据模块
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_from3_Click(object sender, EventArgs e)
        {
            if (frm_MQ2MQ != null)
            {
                frm_MQ2MQ.Show();
                panel1.Controls.Clear();
                panel1.Controls.Add(frm_MQ2MQ);
            }
            else
            {
                frm_MQ2MQ = new frm_MQ2MQ();
                frm_MQ2MQ.Show();
                panel1.Controls.Clear();
                panel1.Controls.Add(frm_MQ2MQ);
                frm_MQ2MQ.CloseForm += CloseForm; //订阅关闭事件
                btn_form3.BackColor = Color.Green;

            }
        }
        #endregion

        /// <summary>
        /// 获取数据库配置参数
        /// </summary>
        public static void GetParam()
        {
            try
            {
                DBSystemConfig dbSystemConfig = new DBSystemConfig();   //配置类
                DBOper.Init();
                DBOper db = new DBOper();
                List<DBSystemConfig> list = db.QueryList(dbSystemConfig);

                RcvToMQ_Permissions = list.Find(e => e.Name.Trim() == "RcvToMQ_Permissions").Value.Trim();
                MQ2CH_Permissions = list.Find(e => e.Name.Trim() == "MQ2CH_Permissions").Value.Trim();
                MQ2MQ_Permissions = list.Find(e => e.Name.Trim() == "MQ2MQ_Permissions").Value.Trim();
                
            }
            catch (Exception ex)
            {
                MessageBox.Show($"获取数据库权限参数参数异常，请检查数据库");
                System.IO.File.AppendAllText("output\\" + DateTime.Now.ToString("yyyyMMdd_") + ".log", "Error:" + ex.ToString() + " @ " + DateTime.Now.ToString() + "\r\n\r\n");
                Console.WriteLine($"获取配置参数异常：{ex}");
                throw;
            }
        }

        /// <summary>
        /// 主窗体尺寸发生变化时触发
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_Resize(object sender, EventArgs e)
        {
            ChangePanel();
        }

        /// <summary>
        /// 改变panel的位置和大小
        /// </summary>
        public void ChangePanel()
        {
            double widthPanel = 0.8;
            double heightPanel = 1;

            //计算panel1的新尺寸
            int newWidth = (int)(this.ClientSize.Width * widthPanel) - 5;
            int newHeight = (int)(this.ClientSize.Height * heightPanel) - 20;

            //设置panel1的新尺寸
            panel1.Size = new Size(newWidth, newHeight);

            //设置panel1的新位置
            panel1.Location = new Point((int)(this.ClientSize.Width * 0.2), 10);

            //设置显示文本尺寸、位置（提示选择模块）
            textBox_MainShowMsg.Height = panel1.Height / 9;
            textBox_MainShowMsg.Location = new Point(panel1.Width / 3, panel1.Height * 4 / 9);
            textBox_MainShowMsg.SelectionLength = 0;

            #region 设置左侧菜单尺寸（按比例自适应）
            double widthPanelMenu = 0.2;
            double heightPanelMenu = 1 / 3;

            int newWidthMenu = (int)((this.ClientSize.Width) * widthPanelMenu) - 10;
            int newHeightMenu = (int)((this.ClientSize.Height - 30) / 3);

            panel2.Size = new Size(newWidthMenu, (this.ClientSize.Height - 30) / 3);
            panel2.Location = new Point(5, 10);

            panel3.Size = new Size(newWidthMenu, newHeightMenu);
            panel3.Location = new Point(5, panel2.Location.Y + newHeightMenu + 5);
            panel4.Size = new Size(newWidthMenu, newHeightMenu);
            panel4.Location = new Point(5, panel3.Location.Y + newHeightMenu + 5);
            #endregion
        }

        #region 窗体关闭
        /// <summary>
        /// 关闭子窗体,并释放资源
        /// </summary>
        /// <param name="param"></param>
        public void CloseForm(string param)
        {
            this.Invoke(new Action(() =>
            {
                try
                {
                    if (param == "frm_RcvToMQ" && frm_RcvToMQ != null)
                    {
                        frm_RcvToMQ.Close();
                        frm_RcvToMQ.Dispose();
                        frm_RcvToMQ = null;
                        btn_form1.BackColor = Color.White;

                        if (panel1.Controls.Count == 0)
                        {
                            this.BeginInvoke(new Action(() =>
                            {
                                textBox_MainShowMsg.Show();
                                panel1.Controls.Clear();
                                panel1.Controls.Add(textBox_MainShowMsg);
                            }));
                        }
                    }
                    else if (param == "frm_mq2ch_V0" && frm_mq2ch_V0 != null)
                    {
                        frm_mq2ch_V0.Close();
                        frm_mq2ch_V0.Dispose();
                        frm_mq2ch_V0 = null;
                        btn_form2.BackColor = Color.White;

                        if (panel1.Controls.Count == 0)
                        {
                            this.BeginInvoke(new Action(() =>
                            {
                                textBox_MainShowMsg.Show();
                                panel1.Controls.Clear();
                                panel1.Controls.Add(textBox_MainShowMsg);
                            }));
                        }
                    }
                    else if (param == "frm_MQ2MQ" && frm_MQ2MQ != null)
                    {
                        frm_MQ2MQ.Close();
                        frm_MQ2MQ.Dispose();
                        frm_MQ2MQ = null;
                        btn_form3.BackColor = Color.White;

                        if (panel1.Controls.Count == 0)
                        {
                            this.BeginInvoke(new Action(() =>
                            {
                                textBox_MainShowMsg.Show();
                                panel1.Controls.Clear();
                                panel1.Controls.Add(textBox_MainShowMsg);
                            }));
                        }
                    }
                    if (param == "MainForm")
                    {
                        this.Dispose();
                        System.Environment.Exit(0);
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"关闭子窗体异常：{ex.Message}");
                }
            }));



        }


        private bool isFormClosing = false;  // 标志变量，表示是否已经在关闭主窗体

        /// <summary>
        /// 主窗体关闭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (isFormClosing) return;  // 如果已经在关闭窗体，直接返回

                bool result = false;
                this.Invoke(new Action(() =>
                {
                    if (MessageBox.Show(this, "确定要关闭PLC数据采集系统吗？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                    {
                        result = true;
                        //CloseForm("frm_RcvToMQ");
                        //CloseForm("frm_mq2ch_V0");
                        //CloseForm("frm_ch2ch_V0");
                        //CloseForm("MainForm");
                    }
                    else
                    {
                        e.Cancel = true;
                    }
                }));

                if (result)
                {
                    isFormClosing = true;

                    // 取消关闭操作，手动关闭窗体
                    e.Cancel = true;

                    // 异步同时关闭三个子窗体
                    Task task1 = RevClose();

                    Task task2 = MQ2CHClose();

                    Task task3 = MQ2MQClose();

                    await Task.WhenAll(task1, task2, task3); // 等待所有任务完成

                    this.Close();       // 手动关闭窗体会重新触发FormClosing事件，所以需要添加标志变量isFormClosing来避免重复关闭窗体
                    this.Dispose();
                    System.Environment.Exit(0);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"主窗体关闭异常：{ex.Message}");
            }

        }

        /// <summary>
        /// 主窗体完全关闭后
        /// </summary>
        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Program.ReleaseMutex(); // 释放互斥锁
        }

        /// <summary>
        /// 关闭子窗体:frm_RcvToMQ(异步同时关闭三个子窗体，服务，以及线程)
        /// </summary>
        /// <returns></returns>
        public async Task RevClose()
        {
            try
            {
                await Task.Delay(1);
                if (frm_RcvToMQ != null)
                {
                    frm_RcvToMQ.Close();
                    frm_RcvToMQ.Dispose();
                    frm_RcvToMQ = null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"关闭子窗体异常：{ex.Message}");
            }

        }

        /// <summary>
        /// 关闭子窗体:frm_mq2ch_V0(异步同时关闭三个子窗体，服务，以及线程)
        /// </summary>
        /// <returns></returns>
        public async Task MQ2CHClose()
        {
            try
            {
                await Task.Delay(1);
                if (frm_mq2ch_V0 != null)
                {
                    frm_mq2ch_V0.Close();
                    frm_mq2ch_V0.Dispose();
                    frm_mq2ch_V0 = null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"关闭子窗体异常：{ex.Message}");
            }
        }

        /// <summary>
        /// 关闭子窗体:frm_MQ2MQ(异步同时关闭三个子窗体，服务，以及线程)
        /// </summary>
        /// <returns></returns>
        public async Task MQ2MQClose()
        {
            try
            {
                await Task.Delay(1);
                if (frm_MQ2MQ != null)
                {
                    frm_MQ2MQ.Close();
                    frm_MQ2MQ.Dispose();
                    frm_MQ2MQ = null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"关闭子窗体异常：{ex.Message}");
            }
        }
        #endregion

        #region 鼠标悬停显示提示信息
        /// <summary>
        /// 设置鼠标悬停提示属性
        /// </summary>
        private void SettingToolTip()
        {
            // 创建ToolTip控件实例  
            toolTip = new System.Windows.Forms.ToolTip();

            // 设置ToolTip的属性  
            toolTip.AutoPopDelay = 10000; // 提示信息的可见时间（毫秒）  
            toolTip.InitialDelay = 500;   // 事件触发多久后出现提示（毫秒）  
            toolTip.ReshowDelay = 500;    // 指针从一个控件移向另一个控件时，经过多久才会显示下一个提示框（毫秒）  
            toolTip.ShowAlways = true;    // 是否总是显示提示框（即使控件没有焦点）  
        }

        /// <summary>
        /// 鼠标悬停提示：Rcv:PLC连接状态
        /// </summary>
        private void Btn_PlcState_MouseHover(object sender, EventArgs e)
        {
            this.BeginInvoke(new Action(() =>
            {
                SettingToolTip();
                // 设置提示信息  
                toolTip.SetToolTip(Btn_PlcState, "绿：PLC连接正常\r\n" +
                                                 "红：PLC未连接\r\n");
            }));
        }

        /// <summary>
        /// 鼠标悬停提示：Rcv:MQ连接状态
        /// </summary>
        private void Btn_RcvMQ_MouseHover(object sender, EventArgs e)
        {
            this.BeginInvoke(new Action(() =>
            {
                SettingToolTip();
                // 设置提示信息  
                toolTip.SetToolTip(Btn_RcvMQ, "绿：MQ服务连接正常\r\n" +
                                              "红：MQ服务未连接\r\n");
            }));
            
        }

        /// <summary>
        /// 鼠标悬停提示：MQ2MQ:本地RabbitMQ连接状态
        /// </summary>
        private void Btn_MQLocal_MouseHover(object sender, EventArgs e)
        {
            SettingToolTip();
            // 设置提示信息  
            toolTip.SetToolTip(Btn_MQLocal, "绿：本地RabbitMQ连接正常\r\n" +
                                            "红：本地RabbitMQ未连接\r\n");
        }

        /// <summary>
        /// 鼠标悬停提示：MQ2MQ:服务器MQ连接状态
        /// </summary>
        private void Btn_MQServer_MouseHover(object sender, EventArgs e)
        {
            SettingToolTip();
            // 设置提示信息  
            toolTip.SetToolTip(Btn_RemoteMQServer, "绿：远端RabbitMQ连接正常\r\n" +
                                                   "红：远端RabbitMQ未连接\r\n");
        }

        /// <summary>
        /// 鼠标悬停提示：MQ2CH:Click House连接状态
        /// </summary>
        private void Btn_MQ2CHClickHouse_MouseHover(object sender, EventArgs e)
        {
            SettingToolTip();
            // 设置提示信息  
            toolTip.SetToolTip(Btn_MQ2CHClickHouse, "绿：Click House连接正常\r\n" +
                                                    "红：Click House未连接\r\n");
        }

        /// <summary>
        /// 鼠标悬停提示：MQ2CH:Redis连接状态
        /// </summary>
        private void Btn_MQ2CHRedis_MouseHover(object sender, EventArgs e)
        {
            SettingToolTip();
            // 设置提示信息  
            toolTip.SetToolTip(Btn_MQ2CHRedis, "绿：Redis连接连接正常\r\n" +
                                               "红：Redis连接未连接\r\n");

        }

        /// <summary>
        /// 鼠标悬停提示：MQ2CH:MQ连接状态
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_MQ2CHMQ_MouseHover(object sender, EventArgs e)
        {
            SettingToolTip();
            // 设置提示信息  
            toolTip.SetToolTip(Btn_MQ2CHMQ, "绿：RabbitMQ连接连接正常\r\n" +
                                            "红：RabbitMQ连接未连接\r\n");

        }
        #endregion

        /// <summary>
        /// 监控各种服务连接状态：更新状态颜色，绿色：正常，红色：异常
        /// </summary>
        public void UpdateConnectStateColor(string updateLocation, bool state)
        {
            this.Invoke(new Action(() =>
            {
                if (!string.IsNullOrEmpty(updateLocation))
                {
                    switch (updateLocation.Trim())
                    {
                        case "PLCState":
                            Btn_PlcState.BackColor = state ? Color.Green : Color.Red;
                            break;
                        case "RcvMQ":
                            Btn_RcvMQ.BackColor = state ? Color.Green : Color.Red;
                            break;
                        case "LocalMQServer":
                            Btn_MQLocal.BackColor = state ? Color.Green : Color.Red;
                            break;
                        case "RemoteMQServer":
                            Btn_RemoteMQServer.BackColor = state ? Color.Green : Color.Red;
                            break;
                        case "MQ2CHMQ":
                            Btn_MQ2CHMQ.BackColor = state ? Color.Green : Color.Red;
                            break;
                        case "MQ2CHClickHouse":
                            Btn_MQ2CHClickHouse.BackColor = state ? Color.Green : Color.Red;
                            break;
                        case "MQ2CHRedis":
                            Btn_MQ2CHRedis.BackColor = state ? Color.Green : Color.Red;
                            break;
                    }
                }

            }));
        }

        /// <summary>
        /// RevMQ_停止按钮
        /// </summary>
        private async void Btn_RevMQStop_Click(object sender, EventArgs e)
        {
            if (frm_RcvToMQ != null)
            {
                await Task.Run(() =>
                {
                    frm_RcvToMQ.btn_closeme_Click(null, null);
                });
            }
        }

        /// <summary>
        /// MQ2CH_停止按钮
        /// </summary>
        private async void Btn_MQ2CHStop_Click(object sender, EventArgs e)
        {
            if (frm_mq2ch_V0 != null)
            {
                await Task.Run(() =>
                {
                    frm_mq2ch_V0.btn_closeme_Click(null, null);
                });
            }
        }

        /// <summary>
        /// MQ2CH_停止按钮
        /// </summary>
        private async void Btn_MQ2MQStop_Click(object sender, EventArgs e)
        {
            if (frm_MQ2MQ != null)
            {
                await Task.Run(() =>
                {
                    frm_MQ2MQ.btn_closeme_Click(null, null);
                });
            }
        }

        
    }
}