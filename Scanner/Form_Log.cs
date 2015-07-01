using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Scanner
{
    public partial class Form_Log : Form
    {

        public Form_Log()
        {
            InitializeComponent();
        }

        public void logVME(string log)
        {
            appendLine(log, this.textBox_LogVme);
        }

        public void logFSX(string log)
        {
            appendLine(log, this.textBox_LogFSX);
        }

        public void appendLine(string text,TextBox textBox)
        {
            BeginInvoke((Action)(() =>
            {
                if (textBox.Text.Length > 32000)
                {
                    //新しいファイルに保存


                    textBox.Text = "";
                }

                textBox.AppendText(DateTime.Now.ToLongTimeString() + " " + text + Environment.NewLine);
                textBox.SelectionStart = textBox.Text.Length;
                textBox.ScrollToCaret();
            }));
        }

        //閉じるを無効
        private void Form_Log_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.WindowState = FormWindowState.Minimized;
            }
        }

        private void Form_Log_Load(object sender, EventArgs e)
        {
            appendLine("Start logging..", this.textBox_LogVme);
            appendLine("Start logging..", this.textBox_LogFSX);
        }
    }
}
