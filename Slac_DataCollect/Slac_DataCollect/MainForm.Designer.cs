namespace Slac_DataCollect
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.panel1 = new System.Windows.Forms.Panel();
            this.textBox_MainShowMsg = new System.Windows.Forms.TextBox();
            this.btn_form1 = new System.Windows.Forms.Button();
            this.btn_form2 = new System.Windows.Forms.Button();
            this.btn_form3 = new System.Windows.Forms.Button();
            this.Btn_RevMQStop = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.Btn_RcvMQ = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.Btn_PlcState = new System.Windows.Forms.Button();
            this.panel3 = new System.Windows.Forms.Panel();
            this.Btn_MQ2CHMQ = new System.Windows.Forms.Button();
            this.label7 = new System.Windows.Forms.Label();
            this.Btn_MQ2CHStop = new System.Windows.Forms.Button();
            this.Btn_MQ2CHRedis = new System.Windows.Forms.Button();
            this.Btn_MQ2CHClickHouse = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.Btn_RemoteMQServer = new System.Windows.Forms.Button();
            this.Btn_MQLocal = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.panel4 = new System.Windows.Forms.Panel();
            this.Btn_MQ2MQStop = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel4.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panel1.Controls.Add(this.textBox_MainShowMsg);
            this.panel1.ForeColor = System.Drawing.SystemColors.ControlText;
            this.panel1.Location = new System.Drawing.Point(221, 12);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(960, 640);
            this.panel1.TabIndex = 0;
            // 
            // textBox_MainShowMsg
            // 
            this.textBox_MainShowMsg.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.textBox_MainShowMsg.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox_MainShowMsg.Enabled = false;
            this.textBox_MainShowMsg.Font = new System.Drawing.Font("宋体", 26.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.textBox_MainShowMsg.ForeColor = System.Drawing.SystemColors.ControlText;
            this.textBox_MainShowMsg.Location = new System.Drawing.Point(305, 264);
            this.textBox_MainShowMsg.Multiline = true;
            this.textBox_MainShowMsg.Name = "textBox_MainShowMsg";
            this.textBox_MainShowMsg.ReadOnly = true;
            this.textBox_MainShowMsg.Size = new System.Drawing.Size(320, 37);
            this.textBox_MainShowMsg.TabIndex = 0;
            this.textBox_MainShowMsg.Text = "请选择对应的模块";
            // 
            // btn_form1
            // 
            this.btn_form1.BackColor = System.Drawing.Color.White;
            this.btn_form1.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btn_form1.Location = new System.Drawing.Point(7, 21);
            this.btn_form1.Name = "btn_form1";
            this.btn_form1.Size = new System.Drawing.Size(119, 37);
            this.btn_form1.TabIndex = 0;
            this.btn_form1.Text = "采集数据模块";
            this.btn_form1.UseVisualStyleBackColor = false;
            this.btn_form1.Click += new System.EventHandler(this.btn_form1_Click);
            // 
            // btn_form2
            // 
            this.btn_form2.BackColor = System.Drawing.Color.White;
            this.btn_form2.Font = new System.Drawing.Font("新宋体", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btn_form2.Location = new System.Drawing.Point(7, 20);
            this.btn_form2.Name = "btn_form2";
            this.btn_form2.Size = new System.Drawing.Size(119, 37);
            this.btn_form2.TabIndex = 1;
            this.btn_form2.Text = "存储数据模块";
            this.btn_form2.UseVisualStyleBackColor = false;
            this.btn_form2.Click += new System.EventHandler(this.btn_form2_Click);
            // 
            // btn_form3
            // 
            this.btn_form3.BackColor = System.Drawing.Color.White;
            this.btn_form3.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btn_form3.Location = new System.Drawing.Point(7, 23);
            this.btn_form3.Name = "btn_form3";
            this.btn_form3.Size = new System.Drawing.Size(119, 37);
            this.btn_form3.TabIndex = 2;
            this.btn_form3.Text = "传输数据模块";
            this.btn_form3.UseVisualStyleBackColor = false;
            this.btn_form3.Click += new System.EventHandler(this.btn_from3_Click);
            // 
            // Btn_RevMQStop
            // 
            this.Btn_RevMQStop.BackColor = System.Drawing.Color.White;
            this.Btn_RevMQStop.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Btn_RevMQStop.Location = new System.Drawing.Point(166, 21);
            this.Btn_RevMQStop.Name = "Btn_RevMQStop";
            this.Btn_RevMQStop.Size = new System.Drawing.Size(40, 37);
            this.Btn_RevMQStop.TabIndex = 4;
            this.Btn_RevMQStop.Text = "停止";
            this.Btn_RevMQStop.UseVisualStyleBackColor = false;
            this.Btn_RevMQStop.Click += new System.EventHandler(this.Btn_RevMQStop_Click);
            // 
            // panel2
            // 
            this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panel2.Controls.Add(this.Btn_RcvMQ);
            this.panel2.Controls.Add(this.label2);
            this.panel2.Controls.Add(this.label1);
            this.panel2.Controls.Add(this.Btn_PlcState);
            this.panel2.Controls.Add(this.btn_form1);
            this.panel2.Controls.Add(this.Btn_RevMQStop);
            this.panel2.Location = new System.Drawing.Point(4, 12);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(211, 205);
            this.panel2.TabIndex = 5;
            // 
            // Btn_RcvMQ
            // 
            this.Btn_RcvMQ.BackColor = System.Drawing.Color.Yellow;
            this.Btn_RcvMQ.Location = new System.Drawing.Point(179, 140);
            this.Btn_RcvMQ.Name = "Btn_RcvMQ";
            this.Btn_RcvMQ.Size = new System.Drawing.Size(25, 23);
            this.Btn_RcvMQ.TabIndex = 8;
            this.Btn_RcvMQ.UseVisualStyleBackColor = false;
            this.Btn_RcvMQ.MouseHover += new System.EventHandler(this.Btn_RcvMQ_MouseHover);
            // 
            // label2
            // 
            this.label2.Enabled = false;
            this.label2.Font = new System.Drawing.Font("宋体", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label2.Location = new System.Drawing.Point(7, 145);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(161, 22);
            this.label2.TabIndex = 7;
            this.label2.Text = "MQ连接状态:";
            // 
            // label1
            // 
            this.label1.Enabled = false;
            this.label1.Font = new System.Drawing.Font("宋体", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.Location = new System.Drawing.Point(7, 84);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(161, 22);
            this.label1.TabIndex = 6;
            this.label1.Text = "PLC连接状态:";
            // 
            // Btn_PlcState
            // 
            this.Btn_PlcState.BackColor = System.Drawing.Color.Yellow;
            this.Btn_PlcState.Location = new System.Drawing.Point(179, 79);
            this.Btn_PlcState.Name = "Btn_PlcState";
            this.Btn_PlcState.Size = new System.Drawing.Size(25, 23);
            this.Btn_PlcState.TabIndex = 5;
            this.Btn_PlcState.UseVisualStyleBackColor = false;
            this.Btn_PlcState.MouseHover += new System.EventHandler(this.Btn_PlcState_MouseHover);
            // 
            // panel3
            // 
            this.panel3.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panel3.Controls.Add(this.Btn_MQ2CHMQ);
            this.panel3.Controls.Add(this.label7);
            this.panel3.Controls.Add(this.Btn_MQ2CHStop);
            this.panel3.Controls.Add(this.Btn_MQ2CHRedis);
            this.panel3.Controls.Add(this.Btn_MQ2CHClickHouse);
            this.panel3.Controls.Add(this.label6);
            this.panel3.Controls.Add(this.label5);
            this.panel3.Controls.Add(this.btn_form2);
            this.panel3.Location = new System.Drawing.Point(4, 223);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(211, 206);
            this.panel3.TabIndex = 6;
            // 
            // Btn_MQ2CHMQ
            // 
            this.Btn_MQ2CHMQ.BackColor = System.Drawing.Color.Yellow;
            this.Btn_MQ2CHMQ.Location = new System.Drawing.Point(179, 80);
            this.Btn_MQ2CHMQ.Name = "Btn_MQ2CHMQ";
            this.Btn_MQ2CHMQ.Size = new System.Drawing.Size(25, 23);
            this.Btn_MQ2CHMQ.TabIndex = 17;
            this.Btn_MQ2CHMQ.UseVisualStyleBackColor = false;
            this.Btn_MQ2CHMQ.MouseHover += new System.EventHandler(this.Btn_MQ2CHMQ_MouseHover);
            // 
            // label7
            // 
            this.label7.Enabled = false;
            this.label7.Font = new System.Drawing.Font("宋体", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label7.Location = new System.Drawing.Point(7, 83);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(161, 22);
            this.label7.TabIndex = 16;
            this.label7.Text = "MQ连接状态:";
            // 
            // Btn_MQ2CHStop
            // 
            this.Btn_MQ2CHStop.BackColor = System.Drawing.Color.White;
            this.Btn_MQ2CHStop.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Btn_MQ2CHStop.Location = new System.Drawing.Point(164, 20);
            this.Btn_MQ2CHStop.Name = "Btn_MQ2CHStop";
            this.Btn_MQ2CHStop.Size = new System.Drawing.Size(40, 37);
            this.Btn_MQ2CHStop.TabIndex = 15;
            this.Btn_MQ2CHStop.Text = "停止";
            this.Btn_MQ2CHStop.UseVisualStyleBackColor = false;
            this.Btn_MQ2CHStop.Click += new System.EventHandler(this.Btn_MQ2CHStop_Click);
            // 
            // Btn_MQ2CHRedis
            // 
            this.Btn_MQ2CHRedis.BackColor = System.Drawing.Color.Yellow;
            this.Btn_MQ2CHRedis.Location = new System.Drawing.Point(179, 117);
            this.Btn_MQ2CHRedis.Name = "Btn_MQ2CHRedis";
            this.Btn_MQ2CHRedis.Size = new System.Drawing.Size(25, 23);
            this.Btn_MQ2CHRedis.TabIndex = 13;
            this.Btn_MQ2CHRedis.UseVisualStyleBackColor = false;
            this.Btn_MQ2CHRedis.MouseHover += new System.EventHandler(this.Btn_MQ2CHRedis_MouseHover);
            // 
            // Btn_MQ2CHClickHouse
            // 
            this.Btn_MQ2CHClickHouse.BackColor = System.Drawing.Color.Yellow;
            this.Btn_MQ2CHClickHouse.Location = new System.Drawing.Point(179, 155);
            this.Btn_MQ2CHClickHouse.Name = "Btn_MQ2CHClickHouse";
            this.Btn_MQ2CHClickHouse.Size = new System.Drawing.Size(25, 23);
            this.Btn_MQ2CHClickHouse.TabIndex = 12;
            this.Btn_MQ2CHClickHouse.UseVisualStyleBackColor = false;
            this.Btn_MQ2CHClickHouse.MouseHover += new System.EventHandler(this.Btn_MQ2CHClickHouse_MouseHover);
            // 
            // label6
            // 
            this.label6.Enabled = false;
            this.label6.Font = new System.Drawing.Font("宋体", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label6.Location = new System.Drawing.Point(7, 157);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(184, 22);
            this.label6.TabIndex = 10;
            this.label6.Text = "CH数据库连接状态:";
            // 
            // label5
            // 
            this.label5.Enabled = false;
            this.label5.Font = new System.Drawing.Font("宋体", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label5.Location = new System.Drawing.Point(7, 120);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(161, 22);
            this.label5.TabIndex = 9;
            this.label5.Text = "Redis连接状态:";
            // 
            // Btn_RemoteMQServer
            // 
            this.Btn_RemoteMQServer.BackColor = System.Drawing.Color.Yellow;
            this.Btn_RemoteMQServer.Location = new System.Drawing.Point(179, 129);
            this.Btn_RemoteMQServer.Name = "Btn_RemoteMQServer";
            this.Btn_RemoteMQServer.Size = new System.Drawing.Size(25, 23);
            this.Btn_RemoteMQServer.TabIndex = 14;
            this.Btn_RemoteMQServer.UseVisualStyleBackColor = false;
            this.Btn_RemoteMQServer.MouseHover += new System.EventHandler(this.Btn_MQServer_MouseHover);
            // 
            // Btn_MQLocal
            // 
            this.Btn_MQLocal.BackColor = System.Drawing.Color.Yellow;
            this.Btn_MQLocal.Location = new System.Drawing.Point(179, 82);
            this.Btn_MQLocal.Name = "Btn_MQLocal";
            this.Btn_MQLocal.Size = new System.Drawing.Size(25, 23);
            this.Btn_MQLocal.TabIndex = 11;
            this.Btn_MQLocal.UseVisualStyleBackColor = false;
            this.Btn_MQLocal.MouseHover += new System.EventHandler(this.Btn_MQLocal_MouseHover);
            // 
            // label4
            // 
            this.label4.Enabled = false;
            this.label4.Font = new System.Drawing.Font("宋体", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label4.Location = new System.Drawing.Point(7, 132);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(173, 40);
            this.label4.TabIndex = 8;
            this.label4.Text = "远端服务器MQ连接状态:";
            // 
            // label3
            // 
            this.label3.Enabled = false;
            this.label3.Font = new System.Drawing.Font("宋体", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label3.Location = new System.Drawing.Point(7, 85);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(173, 22);
            this.label3.TabIndex = 7;
            this.label3.Text = "本地MQ连接状态:";
            // 
            // panel4
            // 
            this.panel4.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panel4.Controls.Add(this.Btn_RemoteMQServer);
            this.panel4.Controls.Add(this.Btn_MQ2MQStop);
            this.panel4.Controls.Add(this.btn_form3);
            this.panel4.Controls.Add(this.Btn_MQLocal);
            this.panel4.Controls.Add(this.label3);
            this.panel4.Controls.Add(this.label4);
            this.panel4.Location = new System.Drawing.Point(4, 435);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(211, 217);
            this.panel4.TabIndex = 7;
            // 
            // Btn_MQ2MQStop
            // 
            this.Btn_MQ2MQStop.BackColor = System.Drawing.Color.White;
            this.Btn_MQ2MQStop.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Btn_MQ2MQStop.Location = new System.Drawing.Point(164, 23);
            this.Btn_MQ2MQStop.Name = "Btn_MQ2MQStop";
            this.Btn_MQ2MQStop.Size = new System.Drawing.Size(40, 37);
            this.Btn_MQ2MQStop.TabIndex = 16;
            this.Btn_MQ2MQStop.Text = "停止";
            this.Btn_MQ2MQStop.UseVisualStyleBackColor = false;
            this.Btn_MQ2MQStop.Click += new System.EventHandler(this.Btn_MQ2MQStop_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.ClientSize = new System.Drawing.Size(1184, 660);
            this.Controls.Add(this.panel4);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.Text = "PLC数据采集系统";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.Resize += new System.EventHandler(this.MainForm_Resize);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel4.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btn_form1;
        private System.Windows.Forms.Button btn_form2;
        private System.Windows.Forms.Button btn_form3;
        private System.Windows.Forms.Button Btn_RevMQStop;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Button Btn_PlcState;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button Btn_RcvMQ;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button Btn_MQLocal;
        private System.Windows.Forms.Button Btn_MQ2CHClickHouse;
        private System.Windows.Forms.Button Btn_RemoteMQServer;
        private System.Windows.Forms.Button Btn_MQ2CHRedis;
        private System.Windows.Forms.Button Btn_MQ2CHStop;
        private System.Windows.Forms.Button Btn_MQ2MQStop;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button Btn_MQ2CHMQ;
        private System.Windows.Forms.TextBox textBox_MainShowMsg;
    }
}