namespace Scanner
{
    partial class Form_Log
    {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージ リソースが破棄される場合 true、破棄されない場合は false です。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form_Log));
            this.textBox_LogVme = new System.Windows.Forms.TextBox();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripButton_LogDir = new System.Windows.Forms.ToolStripButton();
            this.toolStripTextBox1 = new System.Windows.Forms.ToolStripTextBox();
            this.toolStripLabel2 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripTextBox2 = new System.Windows.Forms.ToolStripTextBox();
            this.toolStripLabel3 = new System.Windows.Forms.ToolStripLabel();
            this.textBox_LogFSX = new System.Windows.Forms.TextBox();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // textBox_LogVme
            // 
            this.textBox_LogVme.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.textBox_LogVme.BackColor = System.Drawing.Color.Black;
            this.textBox_LogVme.Font = new System.Drawing.Font("MS UI Gothic", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.textBox_LogVme.ForeColor = System.Drawing.Color.Lime;
            this.textBox_LogVme.Location = new System.Drawing.Point(0, 28);
            this.textBox_LogVme.Multiline = true;
            this.textBox_LogVme.Name = "textBox_LogVme";
            this.textBox_LogVme.ReadOnly = true;
            this.textBox_LogVme.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_LogVme.Size = new System.Drawing.Size(400, 401);
            this.textBox_LogVme.TabIndex = 1;
            // 
            // toolStrip1
            // 
            this.toolStrip1.Font = new System.Drawing.Font("ＭＳ Ｐゴシック", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_LogDir,
            this.toolStripTextBox1,
            this.toolStripLabel2,
            this.toolStripTextBox2,
            this.toolStripLabel3});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(813, 25);
            this.toolStrip1.TabIndex = 2;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButton_LogDir
            // 
            this.toolStripButton_LogDir.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_LogDir.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_LogDir.Image")));
            this.toolStripButton_LogDir.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_LogDir.Name = "toolStripButton_LogDir";
            this.toolStripButton_LogDir.Size = new System.Drawing.Size(63, 22);
            this.toolStripButton_LogDir.Text = "Log保存先";
            // 
            // toolStripTextBox1
            // 
            this.toolStripTextBox1.Name = "toolStripTextBox1";
            this.toolStripTextBox1.Size = new System.Drawing.Size(100, 25);
            // 
            // toolStripLabel2
            // 
            this.toolStripLabel2.Name = "toolStripLabel2";
            this.toolStripLabel2.Size = new System.Drawing.Size(58, 22);
            this.toolStripLabel2.Text = "ファイル名";
            // 
            // toolStripTextBox2
            // 
            this.toolStripTextBox2.Name = "toolStripTextBox2";
            this.toolStripTextBox2.Size = new System.Drawing.Size(100, 25);
            // 
            // toolStripLabel3
            // 
            this.toolStripLabel3.Name = "toolStripLabel3";
            this.toolStripLabel3.Size = new System.Drawing.Size(39, 22);
            this.toolStripLabel3.Text = "***.txt";
            // 
            // textBox_LogFSX
            // 
            this.textBox_LogFSX.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
            this.textBox_LogFSX.BackColor = System.Drawing.Color.Black;
            this.textBox_LogFSX.Font = new System.Drawing.Font("MS UI Gothic", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.textBox_LogFSX.ForeColor = System.Drawing.Color.Lime;
            this.textBox_LogFSX.Location = new System.Drawing.Point(406, 28);
            this.textBox_LogFSX.Multiline = true;
            this.textBox_LogFSX.Name = "textBox_LogFSX";
            this.textBox_LogFSX.ReadOnly = true;
            this.textBox_LogFSX.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_LogFSX.Size = new System.Drawing.Size(400, 401);
            this.textBox_LogFSX.TabIndex = 1;
            // 
            // Form_Log
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(813, 441);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.textBox_LogFSX);
            this.Controls.Add(this.textBox_LogVme);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "Form_Log";
            this.Text = "Log";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form_Log_FormClosing);
            this.Load += new System.EventHandler(this.Form_Log_Load);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBox_LogVme;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolStripButton_LogDir;
        private System.Windows.Forms.ToolStripTextBox toolStripTextBox1;
        private System.Windows.Forms.ToolStripLabel toolStripLabel2;
        private System.Windows.Forms.ToolStripTextBox toolStripTextBox2;
        private System.Windows.Forms.ToolStripLabel toolStripLabel3;
        private System.Windows.Forms.TextBox textBox_LogFSX;

    }
}