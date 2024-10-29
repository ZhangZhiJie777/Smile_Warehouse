namespace Slac_DataCollect.FormPage
{
    partial class frm_mq2ch_V0
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frm_mq2ch_V0));
            this.btn_Save2DB = new System.Windows.Forms.Button();
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.menu_icon = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.btn_show = new System.Windows.Forms.ToolStripMenuItem();
            this.btn_closeme = new System.Windows.Forms.ToolStripMenuItem();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.checkBox2 = new System.Windows.Forms.CheckBox();
            this.checkBox3 = new System.Windows.Forms.CheckBox();
            this.checkBox4 = new System.Windows.Forms.CheckBox();
            this.textBox_ShowMsg = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.menu_icon.SuspendLayout();
            this.SuspendLayout();
            // 
            // btn_Save2DB
            // 
            this.btn_Save2DB.Font = new System.Drawing.Font("宋体", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btn_Save2DB.Location = new System.Drawing.Point(12, 12);
            this.btn_Save2DB.Name = "btn_Save2DB";
            this.btn_Save2DB.Size = new System.Drawing.Size(161, 52);
            this.btn_Save2DB.TabIndex = 6;
            this.btn_Save2DB.Text = "开始数据解析";
            this.btn_Save2DB.UseVisualStyleBackColor = true;
            this.btn_Save2DB.Click += new System.EventHandler(this.btn_Save2DB_Click);
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.BalloonTipTitle = "Slac_MQ2CH";
            this.notifyIcon1.ContextMenuStrip = this.menu_icon;
            this.notifyIcon1.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon1.Icon")));
            this.notifyIcon1.Text = "Slac_MQ2CH";
            this.notifyIcon1.Visible = true;
            this.notifyIcon1.DoubleClick += new System.EventHandler(this.notifyIcon1_DoubleClick);
            // 
            // menu_icon
            // 
            this.menu_icon.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.menu_icon.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btn_show,
            this.btn_closeme});
            this.menu_icon.Name = "menu_icon";
            this.menu_icon.Size = new System.Drawing.Size(166, 56);
            // 
            // btn_show
            // 
            this.btn_show.Image = ((System.Drawing.Image)(resources.GetObject("btn_show.Image")));
            this.btn_show.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btn_show.Name = "btn_show";
            this.btn_show.Size = new System.Drawing.Size(165, 26);
            this.btn_show.Text = "显示主窗口";
            this.btn_show.Click += new System.EventHandler(this.btn_show_Click);
            // 
            // btn_closeme
            // 
            this.btn_closeme.Image = ((System.Drawing.Image)(resources.GetObject("btn_closeme.Image")));
            this.btn_closeme.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btn_closeme.Name = "btn_closeme";
            this.btn_closeme.Size = new System.Drawing.Size(165, 26);
            this.btn_closeme.Text = "退出";
            this.btn_closeme.Click += new System.EventHandler(this.btn_closeme_Click);
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Enabled = false;
            this.checkBox1.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.checkBox1.Location = new System.Drawing.Point(207, 30);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(161, 18);
            this.checkBox1.TabIndex = 8;
            this.checkBox1.Text = "发送到远端消息队列";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.SystemColors.Control;
            this.label1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label1.Font = new System.Drawing.Font("宋体", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.Location = new System.Drawing.Point(12, 266);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(927, 79);
            this.label1.TabIndex = 9;
            this.label1.Text = "数据发送：";
            // 
            // checkBox2
            // 
            this.checkBox2.AutoSize = true;
            this.checkBox2.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.checkBox2.Location = new System.Drawing.Point(382, 30);
            this.checkBox2.Name = "checkBox2";
            this.checkBox2.Size = new System.Drawing.Size(146, 18);
            this.checkBox2.TabIndex = 10;
            this.checkBox2.Text = "是否显示发送日志";
            this.checkBox2.UseVisualStyleBackColor = true;
            this.checkBox2.CheckedChanged += new System.EventHandler(this.checkBox2_CheckedChanged);
            // 
            // checkBox3
            // 
            this.checkBox3.AutoSize = true;
            this.checkBox3.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.checkBox3.Location = new System.Drawing.Point(563, 30);
            this.checkBox3.Name = "checkBox3";
            this.checkBox3.Size = new System.Drawing.Size(120, 18);
            this.checkBox3.TabIndex = 11;
            this.checkBox3.Text = "保存packetID";
            this.checkBox3.UseVisualStyleBackColor = true;
            // 
            // checkBox4
            // 
            this.checkBox4.AutoSize = true;
            this.checkBox4.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.checkBox4.Location = new System.Drawing.Point(724, 30);
            this.checkBox4.Name = "checkBox4";
            this.checkBox4.Size = new System.Drawing.Size(140, 18);
            this.checkBox4.TabIndex = 12;
            this.checkBox4.Text = "保存bin（调试）";
            this.checkBox4.UseVisualStyleBackColor = true;
            this.checkBox4.CheckedChanged += new System.EventHandler(this.checkBox4_CheckedChanged);
            // 
            // textBox_ShowMsg
            // 
            this.textBox_ShowMsg.Font = new System.Drawing.Font("宋体", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.textBox_ShowMsg.Location = new System.Drawing.Point(12, 79);
            this.textBox_ShowMsg.Multiline = true;
            this.textBox_ShowMsg.Name = "textBox_ShowMsg";
            this.textBox_ShowMsg.ReadOnly = true;
            this.textBox_ShowMsg.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_ShowMsg.Size = new System.Drawing.Size(929, 172);
            this.textBox_ShowMsg.TabIndex = 13;
            // 
            // label2
            // 
            this.label2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label2.Font = new System.Drawing.Font("宋体", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label2.Location = new System.Drawing.Point(12, 359);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(927, 207);
            this.label2.TabIndex = 14;
            this.label2.Text = "数据监控中：";
            // 
            // frm_mq2ch_V0
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.GradientActiveCaption;
            this.ClientSize = new System.Drawing.Size(951, 575);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBox_ShowMsg);
            this.Controls.Add(this.checkBox4);
            this.Controls.Add(this.checkBox3);
            this.Controls.Add(this.checkBox2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.btn_Save2DB);
            this.Font = new System.Drawing.Font("宋体", 10F);
            this.Name = "frm_mq2ch_V0";
            this.Text = "SLAC数据采集系统2.3-解析";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frm_mq2ch_V0_FormClosing);
            this.Load += new System.EventHandler(this.frm_mq2ch_Load);
            this.Resize += new System.EventHandler(this.frm_mq2ch_Resize);
            this.menu_icon.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button btn_Save2DB;
        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox checkBox2;
        private System.Windows.Forms.CheckBox checkBox3;
        private System.Windows.Forms.CheckBox checkBox4;
        private System.Windows.Forms.ContextMenuStrip menu_icon;
        private System.Windows.Forms.ToolStripMenuItem btn_show;
        private System.Windows.Forms.ToolStripMenuItem btn_closeme;
        private System.Windows.Forms.TextBox textBox_ShowMsg;
        private System.Windows.Forms.Label label2;
    }
}

