using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZedGraph;
using System.IO;
using System.Threading;
using System.Xml.Serialization;


namespace Scanner
{
    public partial class Form_Main : Form
    {

        Form_Log form_Log;
        Form_2DScan form_2DScan;

        #region vme
        const int EH = 3;
        readonly int[] nCh = { 23, 47, 39, 31 };
        internal VmeControl vme = new VmeControl();
        const int sleep = 100;

        #endregion

        #region feedbackstage
        internal Class_FeedbackStage fs1nmX;
        internal Class_FeedbackStage fs1nmZ;
        internal Class_FeedbackStage fs5nm;

        internal int Axis5nmX = 1;
        internal int Axis5nmZ = 2;

        bool flagFromMouseLS = false;
        bool flagFromMouseSS = false;

        string[] pos1nmX = new string[2];
        string[] pos1nmZ = new string[2];
        string[] pos5nmX = new string[2];
        string[] pos5nmZ = new string[2];

        const int hspd = 100000;    //100um/sec
        const int lspd = 10000;      //10um/sec

        #endregion

        #region graph

        string[] graphFilePath = null;

        //filelist
        System.IO.FileSystemWatcher watcher;

        public bool GraphLog = false;
        #endregion

        #region Scan
        Scan scan;
        int PeakX;

        List<ScanReserve> listScanReserve = new List<ScanReserve>();
        class ScanReserve
        {
            internal enum ScanMethod { Coordinate, FromPeak }
            internal ScanMethod scanMethod { get; set; }
            internal int iScanStart { get; set; }
            internal int iScanPitch { get; set; }
            internal int iScanMax { get; set; }
            internal int iNoPt { get; set; }
            internal double dExposTime { get; set; }
            internal double dWaitTime { get; set; }
            
            internal bool crossScan { get; set; }
            internal int crossScanPitch { get; set; }
        }

        #endregion

        public Form_Main()
        {
            InitializeComponent();

            form_Log = new Form_Log();
            this.form_Log.Show();

            System.Reflection.Assembly asm = System.Reflection.Assembly.GetExecutingAssembly();
            System.Version ver = asm.GetName().Version;

            this.Text = "Scanner " + ver;

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            #region feedbackstage
            string[] pList = System.IO.Ports.SerialPort.GetPortNames();

            foreach (string port in pList)
            {
                this.comboBox_1nmX_Port.Items.Add(port);
                this.comboBox_1nmZ_Port.Items.Add(port);
                this.comboBox_5nm_Port.Items.Add(port);
            }

            fs1nmX = new Class_FeedbackStage("1nmStageX");
            fs1nmZ = new Class_FeedbackStage("1nmStageZ");
            fs5nm = new Class_FeedbackStage("5nmStage");

            fs1nmX.logSet += new LogEventHandler(fs1nmX_logSet);
            fs1nmZ.logSet += new LogEventHandler(fs1nmZ_logSet);
            fs5nm.logSet += new LogEventHandler(fs5nm_logSet);

            fs1nmX.statSet += new StatusEventHandler(fs1nmX_statSet);
            fs1nmZ.statSet += new StatusEventHandler(fs1nmZ_statSet);
            fs5nm.statSet += new StatusEventHandler(fs5nm_statSet);
            #endregion

            #region VME
            SetVmeCh(this.comboBox_F1hAngle_VmeCh);
            SetVmeCh(this.comboBox_F1vAngle_VmeCh);
            SetVmeCh(this.comboBox_F1Y_VmeCh);
            SetVmeCh(this.comboBox_F2hAngle_VmeCh);
            SetVmeCh(this.comboBox_F2vAngle_VmeCh);
            SetVmeCh(this.comboBox_F2Y_VmeCh);
            #endregion

            #region Scan
            scan = new Scan();
            scan.graphSet += new ScanDataEventHandler(scan_graphSet);
            scan.scanStatusSet += new ScanStatusEventHandler(scan_scanStatusSet);
            #endregion
        }

        void scan_scanStatusSet(string ScanStatus)
        {
            BeginInvoke((Action)(() => this.toolStripStatusLabel_Status.Text = ScanStatus));
        }

        #region feedbackstage

        #region Connect/Disconnect
        private void button_5nm_Com_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.fs5nm.IsOpen)
                {
                    if (this.fs5nm.Close())
                    {
                        this.button_5nm_Com.Text = "Connect";
                        this.textBox_5nmX_Status.Text = "Disconnect";
                        this.textBox_5nmZ_Status.Text = "Disconnect";
                        this.groupBox_5nmX.Enabled = false;
                        this.groupBox_5nmZ.Enabled = false;

                        this.comboBox_5nm_Port.Enabled = true;
                    }
                }
                else
                {
                    if (this.fs5nm.Open(this.comboBox_5nm_Port.Text))
                    {
                        this.fs5nm.Status();

                        this.button_5nm_Com.Text = "Disconnect";
                        this.groupBox_5nmX.Enabled = true;
                        this.groupBox_5nmZ.Enabled = true;
                        this.comboBox_5nm_Port.Enabled = false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button_1nmX_Com_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.fs1nmX.IsOpen)
                {
                    if (this.fs1nmX.Close())
                    {
                        this.button_1nmX_Com.Text = "Connect";
                        this.textBox_1nmX_Status.Text = "Disconnect";
                        this.groupBox_1nmX.Enabled = false;
                        this.comboBox_1nmX_Port.Enabled = true;
                    }

                }
                else
                {
                    if (this.fs1nmX.Open(this.comboBox_1nmX_Port.Text))
                    {
                        this.fs1nmX.Status();

                        this.button_1nmX_Com.Text = "Disconnect";
                        this.groupBox_1nmX.Enabled = true;

                        this.comboBox_1nmX_Port.Enabled = false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button_1nmZ_Com_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.fs1nmZ.IsOpen)
                {
                    if (this.fs1nmZ.Close())
                    {
                        this.button_1nmZ_Com.Text = "Connect";
                        this.textBox_1nmZ_Status.Text = "Disconnect";
                        this.groupBox_1nmZ.Enabled = false;

                        this.comboBox_1nmZ_Port.Enabled = true;
                    }
                }
                else
                {
                    if (this.fs1nmZ.Open(this.comboBox_1nmZ_Port.Text))
                    {
                        this.fs1nmZ.Status();

                        this.button_1nmZ_Com.Text = "Disconnect";
                        this.groupBox_1nmZ.Enabled = true;

                        this.comboBox_1nmZ_Port.Enabled = false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        #endregion

        #region eventhandler
        void fs5nm_statSet(string Pos1, string Pos2, string Error, string Axis1, string Axis2, string Ready)
        {
            BeginInvoke((Action)(() =>
            {

                if (Axis5nmX == 1)
                {
                    this.textBox_5nmX_Position.Text = Pos1;
                    this.textBox_5nmZ_Position.Text = Pos2;

                    this.textBox_5nmX_Status.Text = Axis1;
                    this.textBox_5nmZ_Status.Text = Axis2;

                    pos5nmX[0] = Axis1.Substring(0, 1);
                    pos5nmX[1] = Pos1;

                    pos5nmZ[0] = Axis2.Substring(0, 1);
                    pos5nmZ[1] = Pos2;

                    if (pos5nmX[0] == "M")
                    {
                        this.button_5nmX_Go.Text = "Stop";
                    }
                    else
                    {
                        this.button_5nmX_Go.Text = "Go";
                    }

                    if (pos5nmZ[0] == "M")
                    {
                        this.button_5nmZ_Go.Text = "Stop";
                    }
                    else
                    {
                        this.button_5nmZ_Go.Text = "Go";
                    }
                }
                else
                {
                    this.textBox_5nmX_Position.Text = Pos2;
                    this.textBox_5nmZ_Position.Text = Pos1;

                    this.textBox_5nmX_Status.Text = Axis2;
                    this.textBox_5nmZ_Status.Text = Axis1;

                    pos5nmX[0] = Axis2.Substring(0, 1);
                    pos5nmX[1] = Pos2;

                    pos5nmZ[0] = Axis1.Substring(0, 1);
                    pos5nmZ[1] = Pos1;

                    if (Axis2.Substring(0, 1) == "M")
                    {
                        this.button_5nmX_Go.Text = "Stop";
                    }
                    else
                    {
                        this.button_5nmX_Go.Text = "Go";
                    }

                    if (Axis1.Substring(0, 1) == "M")
                    {
                        this.button_5nmZ_Go.Text = "Stop";
                    }
                    else
                    {
                        this.button_5nmZ_Go.Text = "Go";
                    }

                }
            }));

        }

        void fs1nmX_statSet(string Pos1, string Pos2, string Error, string Axis1, string Axis2, string Ready)
        {
            BeginInvoke((Action)(() =>
            {
                this.textBox_1nmX_Position.Text = Pos1;
                this.textBox_1nmX_Status.Text = Axis1;

                pos1nmX[0] = Axis1.Substring(0, 1);
                pos1nmX[1] = Pos1;

                if (pos1nmX[0] == "M")
                {
                    this.button_1nmX_Go.Text = "Stop";
                }
                else
                {
                    this.button_1nmX_Go.Text = "Go";
                }

            }));


        }

        void fs1nmZ_statSet(string Pos1, string Pos2, string Error, string Axis1, string Axis2, string Ready)
        {
            BeginInvoke((Action)(() =>
            {
                this.textBox_1nmZ_Position.Text = Pos1;
                this.textBox_1nmZ_Status.Text = Axis1;

                pos1nmZ[0] = Axis1.Substring(0, 1);
                pos1nmZ[1] = Pos1;

                if (Axis1.Substring(0, 1) == "M")
                {
                    this.button_1nmZ_Go.Text = "Stop";
                }
                else
                {
                    this.button_1nmZ_Go.Text = "Go";
                }
            }));

        }

        void fs1nmX_logSet(string log)
        {
            this.form_Log.logFSX(log);
        }

        void fs1nmZ_logSet(string log)
        {
            this.form_Log.logFSX(log);
        }

        void fs5nm_logSet(string log)
        {
            this.form_Log.logFSX(log);
        }
        #endregion

        private void button_5nmZ_Axis_Click(object sender, EventArgs e)
        {
            if (this.button_5nmZ_Axis.Text == "1")
            {
                this.button_5nmZ_Axis.Text = "2";
                Axis5nmX = 1;
                Axis5nmZ = 2;
            }
            else
            {
                this.button_5nmZ_Axis.Text = "1";
                Axis5nmX = 2;
                Axis5nmZ = 1;
            }

        }

        void button_Mode(object sender, EventArgs e)
        {
            try
            {
                Button button = (Button)sender;
                if (button.Text == "ABS")
                {
                    button.Text = "INC";
                    button.BackColor = Color.Aqua;
                }
                else
                {
                    button.Text = "ABS";
                    button.BackColor = Color.Pink;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        #region Go/Stop
        private async void button_5nmX_Go_Click(object sender, EventArgs e)
        {
            if (this.button_5nmX_Go.Text == "Go")
            {
                this.button_5nmX_Go.Text = "Stop";
                this.fs5nm.Speed(hspd, Axis5nmX);

                int move = Convert.ToInt32(this.textBox_5nmX_Move.Text);
                if (this.button_5nmX_Mode.Text == "ABS")
                    await Task.Run(() => this.fs5nm.MoveAbs(move, Axis5nmX));
                else
                    await Task.Run(() => this.fs5nm.MoveInc(move, Axis5nmX));
            }
            else
            {
                //Stop
                this.fs5nm.Stop(Axis5nmX);

                this.button_5nmX_Go.Text = "Go";
            }
        }

        private async void button_5nmZ_Go_Click(object sender, EventArgs e)
        {
            if (this.button_5nmZ_Go.Text == "Go")
            {
                this.button_5nmZ_Go.Text = "Stop";
                this.fs5nm.Speed(hspd, Axis5nmZ);

                int move = Convert.ToInt32(this.textBox_5nmZ_Move.Text);
                if (this.button_5nmZ_Mode.Text == "ABS")
                    await Task.Run(() => this.fs5nm.MoveAbs(move, Axis5nmZ));
                else
                    await Task.Run(() => this.fs5nm.MoveInc(move, Axis5nmZ));
            }
            else
            {
                //Stop
                this.fs5nm.Stop(Axis5nmZ);

                this.button_5nmZ_Go.Text = "Go";
            }
        }

        private async void button_1nmX_Go_Click(object sender, EventArgs e)
        {
            if (this.button_1nmX_Go.Text == "Go")
            {
                this.button_1nmX_Go.Text = "Stop";
                this.fs1nmX.Speed(hspd);

                int move = Convert.ToInt32(this.textBox_1nmX_Move.Text);
                if (this.button_1nmX_Mode.Text == "ABS")
                    await Task.Run(() => this.fs1nmX.MoveAbs(move));
                else
                    await Task.Run(() => this.fs1nmX.MoveInc(move));
            }
            else
            {
                //Stop
                this.fs1nmX.Stop();

                this.button_1nmX_Go.Text = "Go";
            }
        }

        private async void button_1nmZ_Go_Click(object sender, EventArgs e)
        {
            if (this.button_1nmZ_Go.Text == "Go")
            {
                this.button_1nmZ_Go.Text = "Stop";
                this.fs1nmZ.Speed(hspd);

                int move = Convert.ToInt32(this.textBox_1nmZ_Move.Text);
                if (this.button_1nmZ_Mode.Text == "ABS")
                    await Task.Run(() => this.fs1nmZ.MoveAbs(move));
                else
                    await Task.Run(() => this.fs1nmZ.MoveInc(move));
            }
            else
            {
                //Stop
                this.fs1nmZ.Stop();

                this.button_1nmZ_Go.Text = "Go";
            }
        }
        #endregion

        #region コマンド送り
        private void button_5nm_Send_Click(object sender, EventArgs e)
        {
            this.fs5nm.Send(this.textBox_5nm_Send.Text);
        }

        private void button_1nmX_Send_Click(object sender, EventArgs e)
        {
            this.fs1nmX.Send(this.textBox_1nmX_Send.Text);
        }

        private void button_1nmZ_Send_Click(object sender, EventArgs e)
        {
            this.fs1nmZ.Send(this.textBox_1nmZ_Send.Text);
        }

        #endregion


        #endregion

        #region VME
        private void toolStripMenuItem_VMEConnect_Click(object sender, EventArgs e)
        {
            try
            {
                this.vme = new VmeControl();
                if (this.vme.Connect())
                {
                    this.toolStripMenuItem_VMEConnect.Enabled = false;
                    this.toolStripMenuItem_VMEDisconnect.Enabled = true;

                    this.groupBox_F1hAngle.Enabled = true;
                    this.groupBox_F1vAngle.Enabled = true;
                    this.groupBox_F1Y.Enabled = true;

                    this.groupBox_F2hAngle.Enabled = true;
                    this.groupBox_F2vAngle.Enabled = true;
                    this.groupBox_F2Y.Enabled = true;

                    this.vme.logSet += new LogEventHandler(vme_logSet);
                }
            }
            catch (Exception ex)
            { MessageBox.Show(ex.Message); }
        }

        private void toolStripMenuItem_VMEDisconnect_Click(object sender, EventArgs e)
        {
            try
            {
                this.vme = null;
                this.toolStripMenuItem_VMEConnect.Enabled = true;
                this.toolStripMenuItem_VMEDisconnect.Enabled = false;

                this.groupBox_F1hAngle.Enabled = false;
                this.groupBox_F1vAngle.Enabled = false;
                this.groupBox_F1Y.Enabled = false;

                this.groupBox_F2hAngle.Enabled = false;
                this.groupBox_F2vAngle.Enabled = false;
                this.groupBox_F2Y.Enabled = false;
            }
            catch { }
        }

        void vme_logSet(string log)
        {
            this.form_Log.logVME(log);
        }

        void SetVmeCh(ComboBox comboBox, int _EH = EH)
        {
            comboBox.Items.Clear();
            for (int i = 0; i < nCh[_EH - 1]; i++)
            {
                comboBox.Items.Add(i.ToString());
            }
            comboBox.SelectedIndex = 0;
        }


        #region button
        private async void button_F1hAngle_Go_Click(object sender, EventArgs e)
        {
            int Ch = Convert.ToInt32(this.comboBox_F1hAngle_VmeCh.SelectedItem);

            if (this.button_F1hAngle_Go.Text == "Go")
            {

                this.button_F1hAngle_Go.Text = "Stop";
                this.comboBox_F1hAngle_VmeCh.Enabled = false;

                int Pulse = Convert.ToInt32(this.textBox_F1hAngle_Move.Text);

                bool abs = this.button_F1hAngle_Mode.Text == "ABS" ? true : false;

                await Task.Run(() => VmeGo(EH, Ch, Pulse, this.textBox_F1hAngle_Pulse, abs));

                this.button_F1hAngle_Go.Text = "Go";
                this.comboBox_F1hAngle_VmeCh.Enabled = true;
            }
            else
            {
                this.vme.PutMotorStop(EH, Ch);
            }
        }
        private async void button_F1vAngle_Go_Click(object sender, EventArgs e)
        {
            int Ch = Convert.ToInt32(this.comboBox_F1vAngle_VmeCh.SelectedItem);

            if (this.button_F1vAngle_Go.Text == "Go")
            {

                this.button_F1vAngle_Go.Text = "Stop";
                this.comboBox_F1vAngle_VmeCh.Enabled = false;

                int Pulse = Convert.ToInt32(this.textBox_F1vAngle_Move.Text);

                bool abs = this.button_F1vAngle_Mode.Text == "ABS" ? true : false;

                await Task.Run(() => VmeGo(EH, Ch, Pulse, this.textBox_F1vAngle_Pulse, abs));

                this.button_F1vAngle_Go.Text = "Go";
                this.comboBox_F1vAngle_VmeCh.Enabled = true;
            }
            else
            {
                this.vme.PutMotorStop(EH, Ch);
            }

        }

        private async void button_F1Y_Go_Click(object sender, EventArgs e)
        {
            int Ch = Convert.ToInt32(this.comboBox_F1Y_VmeCh.SelectedItem);

            if (this.button_F1Y_Go.Text == "Go")
            {

                this.button_F1Y_Go.Text = "Stop";
                this.comboBox_F1Y_VmeCh.Enabled = false;

                int Pulse = Convert.ToInt32(this.textBox_F1Y_Move.Text);

                bool abs = this.button_F1Y_Mode.Text == "ABS" ? true : false;

                await Task.Run(() => VmeGo(EH, Ch, Pulse, this.textBox_F1Y_Pulse, abs));

                this.button_F1Y_Go.Text = "Go";
                this.comboBox_F1Y_VmeCh.Enabled = true;
            }
            else
            {
                this.vme.PutMotorStop(EH, Ch);
            }


        }

        private async void button_F2hAngle_Go_Click(object sender, EventArgs e)
        {
            int Ch = Convert.ToInt32(this.comboBox_F2hAngle_VmeCh.SelectedItem);

            if (this.button_F2hAngle_Go.Text == "Go")
            {

                this.button_F2hAngle_Go.Text = "Stop";
                this.comboBox_F2hAngle_VmeCh.Enabled = false;

                int Pulse = Convert.ToInt32(this.textBox_F2hAngle_Move.Text);

                bool abs = this.button_F2hAngle_Mode.Text == "ABS" ? true : false;

                await Task.Run(() => VmeGo(EH, Ch, Pulse, this.textBox_F2hAngle_Pulse, abs));

                this.button_F2hAngle_Go.Text = "Go";
                this.comboBox_F2hAngle_VmeCh.Enabled = true;
            }
            else
            {
                this.vme.PutMotorStop(EH, Ch);
            }

        }

        private async void button_F2vAngle_Go_Click(object sender, EventArgs e)
        {
            int Ch = Convert.ToInt32(this.comboBox_F2vAngle_VmeCh.SelectedItem);

            if (this.button_F2vAngle_Go.Text == "Go")
            {

                this.button_F2vAngle_Go.Text = "Stop";
                this.comboBox_F1vAngle_VmeCh.Enabled = false;

                int Pulse = Convert.ToInt32(this.textBox_F2vAngle_Move.Text);

                bool abs = this.button_F2vAngle_Mode.Text == "ABS" ? true : false;

                await Task.Run(() => VmeGo(EH, Ch, Pulse, this.textBox_F2vAngle_Pulse, abs));

                this.button_F2vAngle_Go.Text = "Go";
                this.comboBox_F2vAngle_VmeCh.Enabled = true;
            }
            else
            {
                this.vme.PutMotorStop(EH, Ch);
            }

        }

        private async void button_F2Y_Go_Click(object sender, EventArgs e)
        {
            int Ch = Convert.ToInt32(this.comboBox_F2Y_VmeCh.SelectedItem);

            if (this.button_F2Y_Go.Text == "Go")
            {

                this.button_F2Y_Go.Text = "Stop";
                this.comboBox_F2Y_VmeCh.Enabled = false;

                int Pulse = Convert.ToInt32(this.textBox_F1Y_Move.Text);

                bool abs = this.button_F2Y_Mode.Text == "ABS" ? true : false;

                await Task.Run(() => VmeGo(EH, Ch, Pulse, this.textBox_F2Y_Pulse, abs));

                this.button_F2Y_Go.Text = "Go";
                this.comboBox_F2Y_VmeCh.Enabled = true;
            }
            else
            {
                this.vme.PutMotorStop(EH, Ch);
            }
        }

        void VmeGo(int EH, int Ch, int Pulse, TextBox textBoxPulseNow, bool abs = true)
        {
            int pulsenow;
            while (true)
            {
                System.Threading.Thread.Sleep(sleep);

                string[] responce = this.vme.GetMotorQuery(EH, Ch);
                pulsenow = Convert.ToInt32(responce[1]);
                if (!responce[0].Contains("rotating"))
                    break;
            }

            if (abs)
                this.vme.PutMotorPulse(EH, Ch, Pulse);
            else
                this.vme.PutMotorPulse(EH, Ch, Pulse + pulsenow);

            while (true)
            {
                System.Threading.Thread.Sleep(sleep);

                string[] responce = this.vme.GetMotorQuery(EH, Ch);

                BeginInvoke((Action)(() =>
                {
                    textBoxPulseNow.Text
                        = responce[0] == "fail"
                        ? "N/A"
                        : responce[1];
                }));

                if (!responce[0].Contains("rotating"))
                    break;
            }

        }
        #endregion

        #endregion

        //log
        private void toolStripMenuItem_Log_Click(object sender, EventArgs e)
        {
            if (this.form_Log.WindowState == FormWindowState.Minimized)
                this.form_Log.WindowState = FormWindowState.Normal;
            else if (!this.form_Log.Focus())
                this.form_Log.Show();
        }

        #region Scan設定



        #region スタート位置の設定
        //グラフからスタート位置設定
        private void button_LS_StartFromGraph_Click(object sender, EventArgs e)
        {
            Click_button_StargFromGraph((Button)sender, ref this.flagFromMouseLS);
        }

        void Click_button_StargFromGraph(Button button, ref bool flag)
        {
            if (!flag)
            {
                button.Text = "Cancel";
                button.ForeColor = Color.Red;

                flag = true;
            }
            else
            {
                button.Text = "Graph";
                button.ForeColor = Color.Black;

                flag = false;
            }

        }

        private void zgc_Profile_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (flagFromMouseLS)
                {
                    this.zgc_Profile.Invalidate();


                    double x = e.Location.X;
                    double y = e.Location.Y;
                    double left = (double)this.zgc_Profile.GraphPane.Chart.Rect.Left;
                    double right = (double)this.zgc_Profile.GraphPane.Chart.Rect.Right;
                    double bottom = (double)this.zgc_Profile.GraphPane.Chart.Rect.Bottom;
                    double top = (double)this.zgc_Profile.GraphPane.Chart.Rect.Top;
                    double width = (double)this.zgc_Profile.GraphPane.Chart.Rect.Width;

                    int a = 0;

                    if (left < x && x < right
                        && top < y && y < bottom)
                    {
                        double xMax = this.zgc_Profile.GraphPane.XAxis.Scale.Max;
                        double xMin = this.zgc_Profile.GraphPane.XAxis.Scale.Min;

                        double xWidth = xMax - xMin;
                        a = (int)(xMin + xWidth * (x - left) / (right - left)) / 100;

                        this.textBox_S1_ScanStart.Text = a.ToString() + "00";
                        this.button_S1_StartFromGraph.Text = "Graph";
                        this.button_S1_StartFromGraph.ForeColor = Color.Black;

                        this.flagFromMouseLS = false;
                    }
                }

            }

        }

        //今のステージ位置
        private async void button_S1_StartCurrentPos_Click(object sender, EventArgs e)
        {
            string[] strPos;
            //1nmor5nm
            if (this.tabControl.SelectedTab == this.tabPage_Focus1)
            {
                if (this.radioButton_Setting_ScanX.Checked)
                    strPos = await Task.Run(() => fs5nm.Position(Axis5nmX));
                else
                    strPos = await Task.Run(() => fs5nm.Position(Axis5nmZ));
            }
            else
            {
                if (this.radioButton_Setting_ScanX.Checked)
                    strPos = await Task.Run(() => fs1nmX.Position());
                else
                    strPos = await Task.Run(() => fs1nmZ.Position());
            }

            this.textBox_S1_ScanStart.Text = strPos[1];
        }

        #endregion

        //スキャンスタート
        private async void button_S1_ScanStart_Click(object sender, EventArgs e)
        {
            if (scan.IsScan)
            {
                MessageBox.Show("スキャン中です");
                return;
            }
            if(!this.vme.IsConnect)
            {
                MessageBox.Show("VMEサーバ未接続");
                return;
            }

            try
            {
                this.button_S1_ScanStart.Enabled = false;
                this.button_S1_ScanStop.Enabled = true;
                //this.button_ScanBackwardSkip.Enabled = true;
                //this.button_ScanForwardSkip.Enabled = true;
                this.groupBox_Setting.Enabled = false;

                #region 宣言
                string strComment;

                int ChIn;
                int ChOut;

                int iScanStart;
                int iScanPitch;
                int iScanMax;
                double dExposTime;
                double dWaitTime;
                int iNoPt;

                Scan.ScanField field;
                Class_FeedbackStage fs;
                int axis;
                #endregion

                do
                {
                    #region 前処理

                    strComment = "";

                    ChIn = Convert.ToInt32(this.textBox_Setting_ChIn.Text);
                    ChOut = Convert.ToInt32(this.textBox_Setting_ChOut.Text);

                    #region 明視野暗視野
                    if (radioButton_Setting_BrightField.Checked)
                    {
                        field = Scan.ScanField.bright;
                        strComment += "暗視野スキャン\t";
                    }
                    else
                    {
                        field = Scan.ScanField.dark;
                        strComment += "明視野スキャン\t";
                    }
                    #endregion

                    #region Stage
                    if (this.tabControl.SelectedTab == this.tabPage_Focus1)
                    {
                        fs = fs5nm;

                        strComment = "5nmStage";
                        if (this.radioButton_Setting_ScanX.Checked)
                        {
                            //5nmStageX
                            axis = Axis5nmX;
                            strComment += "Focus1Xスキャン\tZ座標[nm]:";
                            //5nmStageZの座標をコメント
                            strComment += await Task.Run(() => this.fs5nm.Position(Axis5nmZ)[1]);
                        }
                        else
                        {
                            //5nmStageZ
                            axis = Axis5nmZ;
                            strComment += "Focus1Zスキャン\tX座標[nm]:";
                            //5nmStageXの座標をコメント
                            strComment += await Task.Run(() => this.fs5nm.Position(Axis5nmX)[1]);
                        }
                    }
                    else
                    {
                        strComment = "1nmStage";
                        if (this.radioButton_Setting_ScanX.Checked)
                        {
                            //1nmStageX
                            fs = fs1nmX;
                            strComment += "Focus2Xスキャン\tZ座標[nm]:";
                            //1nmStageZの座標をコメント
                            strComment += await Task.Run(() => this.fs1nmZ.Position()[1]);
                        }
                        else
                        {
                            //1nmStageZ
                            fs = fs1nmZ;
                            strComment += "Focus2Zスキャン\tX座標[nm]:";
                            //1nmStageXの座標をコメント
                            strComment += await Task.Run(() => this.fs1nmX.Position()[1]);
                        }
                        axis = 1;

                    }

                    if (!fs.IsOpen)
                    {
                        MessageBox.Show("フィードバックステージが接続されていません");
                        return;
                    }

                    //ステージ速度の設定(10um/sec)
                    fs.Speed(lspd, axis);
                    #endregion

                    //ScanReserveに予約がないとき，直接スキャン
                    if (listScanReserve.Count == 0)
                    {
                        iScanStart = Convert.ToInt32(this.textBox_S1_ScanStart.Text);
                        iScanPitch = Convert.ToInt32(this.textBox_S1_ScanPitch.Text);
                        iScanMax = Convert.ToInt32(this.textBox_S1_ScanMax.Text);
                        dExposTime = Convert.ToDouble(this.textBox_S1_ExposureTime.Text);
                        dWaitTime = Convert.ToDouble(this.textBox_S1_WaitTime.Text);
                        iNoPt = Convert.ToInt32(this.nud_S1_NoPt.Value);

                    }
                    #region reserve
                    //ScanReserveに予約があるとき，予約リストから読み込んでスキャン
                    else
                    {
                        #region 直交ステージの動作
                        //crossScanPitch
                        if (listScanReserve[0].crossScan)
                        {
                            Class_FeedbackStage fs2 = fs5nm;
                            int crossAxis = 1;
                            if (fs == fs1nmX)
                                fs2 = fs1nmZ;
                            else if (fs == fs1nmZ)
                                fs2 = fs1nmX;
                            else
                                crossAxis = axis == 1 ? 2 : 1;
                            await Task.Run(() => fs2.MoveInc(listScanReserve[0].crossScanPitch));
                            strComment += "+" + Convert.ToString(listScanReserve[0].crossScanPitch);
                        }
                        #endregion

                        //開始位置指定orピーク前の設定
                        if (listScanReserve[0].scanMethod == ScanReserve.ScanMethod.Coordinate)
                        {
                            iScanStart = listScanReserve[0].iScanStart;
                        }
                        else
                        {
                            iScanStart = PeakX + listScanReserve[0].iScanStart;
                        }


                        iScanPitch = listScanReserve[0].iScanPitch;
                        iScanMax = listScanReserve[0].iScanMax;
                        iNoPt = listScanReserve[0].iNoPt;
                        dExposTime = listScanReserve[0].dExposTime;
                        dWaitTime = listScanReserve[0].dWaitTime;

                        listScanReserve.RemoveAt(0);
                        this.listBox_ScanReserve.Items.RemoveAt(0);
                    }
                    #endregion

                    strComment += "\t露光時間[sec]:" + dExposTime.ToString()
                                + "\tステージ送り量[nm]:" + iScanPitch.ToString()
                                + "\t" + DateTime.Now.ToString("F")
                                + "\tスキャン開始位置[nm]:" + iScanStart.ToString()
                                + "\t一点あたりの計測回数:" + iNoPt.ToString();

                    #endregion

                    //保存先設定
                    string f = this.tabControl.SelectedTab == this.tabPage_Focus1 ? "F1" : "F2";
                    string xz = this.radioButton_Setting_ScanX.Checked ? "X" : "Z";
                    string bd = this.radioButton_Setting_BrightField.Checked ? "b" : "d";
                    string path = this.fbd.SelectedPath + "\\" + f + "\\" + xz
                        + "\\" + this.textBox_FileName.Text + ((int)this.numericUpDown_FileNum.Value).ToString("D4") + f + xz + bd;

                    //スキャンの初期化
                    scan.Initialize(fs, axis, vme, EH, ChIn, ChOut, iScanStart, iScanPitch, iScanMax, iNoPt, dExposTime, dWaitTime, path, field, strComment);
                    double bg;
                    if (double.TryParse(this.textBox_Background.Text, out bg))
                        scan.scanData.bg = bg;

                    //別スレッドでスキャン開始
                    await Task.Run(() => scan.TaskScan());

                    scan.IsScan = false;

                    //終了処理
                    this.button_S1_ScanStop.Text = "Scan Stop";
                    this.numericUpDown_FileNum.Value++;
                }
                //ScanReserveの予約があるときは続ける
                while (this.listBox_ScanReserve.Items.Count > 0);

            }
            finally
            {
                //終了後処理
                this.button_S1_ScanStart.Enabled = true;
                this.button_S1_ScanStop.Enabled = false;
                //this.button_ScanBackwardSkip.Enabled = false;
                //this.button_ScanForwardSkip.Enabled = false;
                this.groupBox_Setting.Enabled = true;
            }

        }

        #endregion

        //スキャンストップ
        private void button_S1_ScanStop_Click(object sender, EventArgs e)
        {
            if (scan.IsScan)
            {
                this.button_S1_ScanStop.Text = "停止処理中";
                scan.Finish();
            }
        }

        #region ScanReserve
        //スキャン予約
        private void button_ScanReserveAdd_Click(object sender, EventArgs e)
        {
            ScanReserve scanReserve = new ScanReserve();
            scanReserve.scanMethod = this.radioButton_Coordinate.Checked ? ScanReserve.ScanMethod.Coordinate : ScanReserve.ScanMethod.FromPeak;
            scanReserve.iScanStart = this.radioButton_Coordinate.Checked ? Convert.ToInt32(this.textBox_S1_ScanStart.Text) : Convert.ToInt32(this.textBox_S1_StartFromPeak.Text);
            scanReserve.iScanPitch = Convert.ToInt32(this.textBox_S1_ScanPitch.Text);
            scanReserve.iScanMax = Convert.ToInt32(this.textBox_S1_ScanMax.Text);
            scanReserve.iNoPt = Convert.ToInt32(this.nud_S1_NoPt.Value);
            scanReserve.dExposTime = Convert.ToDouble(this.textBox_S1_ExposureTime.Text);
            scanReserve.dWaitTime = Convert.ToDouble(this.textBox_S1_WaitTime.Text);

            scanReserve.crossScan = this.checkBox_S1_CrossStagePitch.Checked;
            scanReserve.crossScanPitch = Convert.ToInt32(this.textBox_S1_CrossStagePitch.Text);

            listScanReserve.Add(scanReserve);

            string strListBox = this.radioButton_Coordinate.Checked ? "座標:" : "ピークから:";
            strListBox += scanReserve.iScanStart.ToString();
            strListBox += " ピッチ:" + this.textBox_S1_ScanPitch.Text
            + " 回数:" + this.textBox_S1_ScanMax.Text
            + " 露光" + this.textBox_S1_ExposureTime.Text;
            
            this.listBox_ScanReserve.Items.Add(strListBox);

        }

        //予約を消す
        private void button_ScanReserveDelete_Click(object sender, EventArgs e)
        {
            this.listScanReserve.RemoveAt(this.listBox_ScanReserve.SelectedIndex);
            this.listBox_ScanReserve.Items.RemoveAt(this.listBox_ScanReserve.SelectedIndex);

        }

        //全予約取り消し
        private void button_ScanReserveAllDelete_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("予約を全て取り消します", "", MessageBoxButtons.OKCancel) == DialogResult.OK)
            {
                this.listScanReserve.Clear();
                this.listBox_ScanReserve.Items.Clear();
            }
        }

        #endregion

        #region graph

        public void scan_graphSet(ScanData _scanData)
        {
            this.zgc_Profile.GraphPane.CurveList.Clear();
            this.zgc_Profile.GraphPane.XAxis.Title.Text = "ステージ座標";

            Tuple<double, double> peakPosCount;

            this.zgc_Profile.GraphPane.Title.IsVisible = false;

            #region 明視野
            if (this.radioButton_Setting_BrightField.Checked)
            {
                //ピーク位置計算
                peakPosCount = ScanData.PeakPosCount(_scanData.Position.ToArray(), _scanData.CountND.ToArray());
                //暗視野計測の場合
                //グラフにデータを追加 (強度と微分値)
                LineItem lineDif = this.zgc_Profile.GraphPane.AddCurve("Differential", _scanData.Position.ToArray(), _scanData.CountND.ToArray(), Color.Red);
                LineItem lineIntensity = this.zgc_Profile.GraphPane.AddCurve("Intensity", _scanData.Position.ToArray(), _scanData.CountNT.ToArray(), Color.Purple);
                lineIntensity.IsY2Axis = true;

                this.zgc_Profile.GraphPane.YAxis.Title.Text = "Differential";
                this.zgc_Profile.GraphPane.YAxis.Type = AxisType.Linear;

                this.zgc_Profile.GraphPane.Y2Axis.Title.Text = "Intensity";
                this.zgc_Profile.GraphPane.Y2Axis.IsVisible = true;
                if (this.GraphLog)
                    this.zgc_Profile.GraphPane.YAxis.Type = AxisType.Log;
                else
                    this.zgc_Profile.GraphPane.YAxis.Type = AxisType.Linear;

                //FWHM計算表示
                Tuple<double, double> FWHM = scan.minusPeak? _scanData.FWHM_NDmin : _scanData.FWHM_ND;
                Tuple<double, double> FWHM2 = scan.minusPeak ? _scanData.FWHM_ND2min : _scanData.FWHM_ND2;
                BeginInvoke((Action)(() =>
                {
                    this.textBox_FWHM1.Text = FWHM.Item1.ToString("F0");
                    this.textBox_FWHM2.Text = FWHM.Item2.ToString("F0");
                    this.textBox_FWHM1_2.Text = FWHM2.Item1.ToString("F0");
                    this.textBox_FWHM2_2.Text = FWHM2.Item2.ToString("F0");
                    this.toolStripStatusLabel_FWHM1.Text = "FWHM1: " + FWHM.Item1.ToString("F0") + " [nm] |";
                    this.toolStripStatusLabel_FWHM2.Text = "FWHM2: " + FWHM.Item2.ToString("F0") + " [nm] |";

                }));

            }
            #endregion

            #region 暗視野
            else
            {
                //ピーク位置計算
                peakPosCount = ScanData.PeakPosCount(_scanData.Position.ToArray(), _scanData.CountNT.ToArray());
                //明視野計測の場合
                //グラフにデータを追加 (強度のみ)
                LineItem lineIntensity = this.zgc_Profile.GraphPane.AddCurve("Intensity", _scanData.Position.ToArray(), _scanData.CountNT.ToArray(), Color.Purple);
                double[] back_x = new double[] { _scanData.Position[0], _scanData.Position[_scanData.iScanCount-1] };
                double[] back_y = new double[] { _scanData.bg, _scanData.bg };
                LineItem lineBack = this.zgc_Profile.GraphPane.AddCurve("Background", back_x, back_y, Color.Green, SymbolType.None);
                
                this.zgc_Profile.GraphPane.YAxis.Title.Text = "Intensity";
                if (this.GraphLog)
                    this.zgc_Profile.GraphPane.YAxis.Type = AxisType.Log;
                else
                    this.zgc_Profile.GraphPane.YAxis.Type = AxisType.Linear;
                this.zgc_Profile.GraphPane.Y2Axis.IsVisible = false;
                
                //FWHM計算表示
                Tuple<double, double> FWHM = _scanData.FWHM_NT;
                BeginInvoke((Action)(() =>
                {
                    this.textBox_FWHM1.Text = FWHM.Item1.ToString("F0");
                    this.textBox_FWHM2.Text = FWHM.Item2.ToString("F0");
                    this.textBox_FWHM1_2.Text = "N/A";
                    this.textBox_FWHM2_2.Text = "N/A";
                    this.toolStripStatusLabel_FWHM1.Text = "FWHM1: " + FWHM.Item1.ToString("F0") + " [nm] |";
                    this.toolStripStatusLabel_FWHM2.Text = "FWHM2: " + FWHM.Item2.ToString("F0") + " [nm] |";
                }));
            }
            #endregion
            
            //ピーク位置を格納
            PeakX = (int)peakPosCount.Item1;

            //選択したファイルの描画
            if (graphFilePath != null)
                GraphFromFile(graphFilePath, peakPosCount.Item1, peakPosCount.Item2);

            this.zgc_Profile.GraphPane.YAxis.Type = this.GraphLog ? AxisType.Log : AxisType.Linear;
            this.zgc_Profile.GraphPane.AxisChange();
            BeginInvoke((Action)(() =>
            {
                this.toolStripProgressBar_ScanNum.Value = (int)(_scanData.iScanCount / (double)scan.iScanMax);
                this.toolStripStatusLabel_PeakX.Text = "ピーク位置: " + PeakX.ToString("F0") + " [nm] |";
                this.toolStripStatusLabel_ScanNum.Text = "スキャン: " + _scanData.iScanCount.ToString() + "/" + scan.iScanMax.ToString() + " 回 |";

                this.zgc_Profile.Refresh();
            }));
        }

        void GraphFromFile(string[] _filePath, double xc = 0.0, double yc = 1.0)
        {
            double[][] x = new double[_filePath.Length][];
            double[][] y = new double[_filePath.Length][];
            int[] iMax = new int[_filePath.Length];

            if (this.radioButton_Setting_BrightField.Checked)
            {
                this.zgc_Profile.GraphPane.YAxis.Title.Text = "Differential";
                this.zgc_Profile.GraphPane.Y2Axis.Title.Text = "Intensity";
                this.zgc_Profile.GraphPane.Y2Axis.IsVisible = true;
            }
            else
            {
                this.zgc_Profile.GraphPane.YAxis.Title.Text = "Intensity";
                this.zgc_Profile.GraphPane.Y2Axis.IsVisible = false;
            }
            LineItem[] lineItem = new LineItem[_filePath.Length];

            //Color設定
            int seed=Environment.TickCount;

            for (int n = 0; n < _filePath.Length; n++)
            {
                //ファイル読み込み
                double[][] dbleData;
                

                if (ClsNk.FileIO.IsFileLocked(_filePath[n]))
                {
                    MessageBox.Show(Path.GetFileNameWithoutExtension(_filePath[n]) + "\r\nはロックされています．");
                    continue;
                }
                ClsNk.FileIO.readFile(_filePath[n], out dbleData);

                x[n] = new double[dbleData.GetLength(0) - 1];
                y[n] = new double[dbleData.GetLength(0) - 1];

                double max = 0.0;

                Regex r = new Regex(@"(\w+)(\d{4})(F1|F2)([XZ])([bd])");
                Match m = r.Match(Path.GetFileNameWithoutExtension(_filePath[n]));

                for (int i = 0; i < dbleData.GetLength(0) - 1; i++)
                {
                    x[n][i] = dbleData[i + 1][0];
                    y[n][i] = Convert.ToString(m.Groups[5]) == "b" ? dbleData[i + 1][6] : dbleData[i + 1][5];
                    if (max < y[n][i])
                    {
                        max = y[n][i];
                        iMax[n] = i;
                    }
                }

                if (this.checkBox_GraphPeakX.Checked)
                {
                    double xMax = x[n][iMax[n]];
                    for (int i = 0; i < dbleData.GetLength(0) - 1; i++)
                    {
                        x[n][i] -= xMax + xc;
                    }
                }

                if (this.checkBox_GraphPeakY.Checked)
                {
                    double yMax = y[n][iMax[n]];
                    for (int i = 0; i < dbleData.GetLength(0) - 1; i++)
                    {
                        y[n][i] /= yMax * yc;
                    }
                }

                //Color設定
                Random rdm = new Random(seed++);
                int R = rdm.Next(256);
                int G = rdm.Next(256);
                int B = rdm.Next(256);
                
                lineItem[n] = this.zgc_Profile.GraphPane.AddCurve(System.IO.Path.GetFileNameWithoutExtension(_filePath[n]) + ":" + this.zgc_Profile.GraphPane.YAxis.Title.Text,
                    x[n], y[n], Color.FromArgb(R,G,B), SymbolType.None);
            }

        }

        #endregion

        #region FileList

        private void button_fbd_Click(object sender, EventArgs e)
        {
            if (this.fbd.ShowDialog() == DialogResult.OK)
            {
                fbdselected();
            }
        }
        private void fbdselected()
        {
            this.textBox_fbd.Text = fbd.SelectedPath;
            string[] filePath = Directory.GetFiles(this.fbd.SelectedPath, this.textBox_FileName.Text + "*.csv", SearchOption.AllDirectories);
            Array.Sort(filePath);
            Array.Reverse(filePath);

            this.listBox_FileList.Items.Clear();
            for (int i = 0; i < filePath.Length; i++)
                this.listBox_FileList.Items.Add(Path.GetFileName(filePath[i]));

            GetFileNum();

            watcherSetting();

        }

        //ディレクトリ監視
        void watcherSetting()
        {
            watcher = new System.IO.FileSystemWatcher();
            watcher.Path = this.fbd.SelectedPath;
            watcher.NotifyFilter =
                (System.IO.NotifyFilters.FileName
                | System.IO.NotifyFilters.LastAccess
                | System.IO.NotifyFilters.LastWrite
                | System.IO.NotifyFilters.DirectoryName);
            watcher.IncludeSubdirectories = true;
            watcher.Filter = this.textBox_FileName.Text + "*.csv";
            watcher.SynchronizingObject = this;
            watcher.Created += new System.IO.FileSystemEventHandler(watcher_Changed);
            watcher.Renamed += new System.IO.RenamedEventHandler(watcher_Changed);
            watcher.Deleted += new System.IO.FileSystemEventHandler(watcher_Changed);
            watcher.EnableRaisingEvents = true;

            watcherChanged();
        }

        //ディレクトリ監視
        void watcher_Changed(object sender, System.IO.FileSystemEventArgs e)
        {
            watcherChanged();
        }

        void watcherChanged()
        {
            var listitems = this.listBox_FileList.SelectedItems;

            this.textBox_fbd.Text = fbd.SelectedPath;
            string[] filePath = Directory.GetFiles(this.fbd.SelectedPath, this.textBox_FileName.Text + "*.csv", SearchOption.AllDirectories);

            //並べ替え用Key
            string[] filePathKey = new string[filePath.Length];
            for (int i = 0; i < filePathKey.Length; i++)
            { filePathKey[i] = Path.GetFileName(filePath[i]); }

            Array.Sort(filePathKey, filePath);
            Array.Reverse(filePath);

            this.listBox_FileList.Items.Clear();
            for (int i = 0; i < filePath.Length; i++)
                this.listBox_FileList.Items.Add(Path.GetFileName(filePath[i]));

        }

        //ファイルナンバー取得
        private void button_GetFileNum_Click(object sender, EventArgs e)
        {
            GetFileNum();
        }

        //ファイルナンバー取得
        void GetFileNum()
        {
            string[] filename = Directory.GetFiles(this.fbd.SelectedPath, "*.csv",SearchOption.AllDirectories);
            if (filename.Length != 0)
            {
                int max = 0;
                foreach (string _filename in filename)
                {

                    //"設定したファイル名"以降の文字列を抽出
                    string subFileName = Path.GetFileNameWithoutExtension(_filename);

                    Regex reg = new Regex(@"(\w+)(\d{4})");
                    Match m = reg.Match(subFileName);

                    
                    //"設定したファイル名"が含まれない場合は次のファイル名へ
                    if (m.Groups[1].Value.IndexOf(this.textBox_FileName.Text) < 0)
                    { continue; }
                    //ファイルナンバーを抽出
                    subFileName = subFileName.Substring(subFileName.IndexOf(this.textBox_FileName.Text) + this.textBox_FileName.TextLength);

                    ////FWHM記入の"("を探す
                    ////無いときは次のファイルへ
                    //if (subFileName.IndexOf("(") < 0)
                    //{ continue; }
                    //"("以前のファイルナンバーを抽出
                    int fileNumber = Convert.ToInt32(m.Groups[2].Value);

                    max = max < fileNumber ? fileNumber : max;
                }
                this.numericUpDown_FileNum.Value = max + 1;
            }
            else
                this.numericUpDown_FileNum.Value = 0;
        }

        //グラフ更新
        private void button_ReloadGraphFromFile_Click(object sender, EventArgs e)
        {
            GraphReload();
        }

        void GraphReload()
        {
            graphFilePath = new string[this.listBox_FileList.SelectedItems.Count];
            Regex r = new Regex(@"(\w+)(\d{4})(\w{2})(\w{1})");
            for (int i = 0; i < graphFilePath.Length; i++)
            {
                Match m = r.Match((string)this.listBox_FileList.SelectedItems[i]);
                string pathplus = "";
                pathplus += Convert.ToString(m.Groups[3]) == "F1" ? "F1\\" : "F2\\";
                pathplus += Convert.ToString(m.Groups[4]) == "X" ? "X\\" : "Z\\";
                graphFilePath[i] = this.fbd.SelectedPath + "\\"+pathplus + (string)this.listBox_FileList.SelectedItems[i];
            }
            this.zgc_Profile.GraphPane.CurveList.Clear();
            GraphFromFile(graphFilePath);
            this.zgc_Profile.GraphPane.AxisChange();
            this.zgc_Profile.Refresh();

        }

        #endregion


        #region setting
        public class Setting
        {
            public string Port5nm, Port1nmX, Port1nmZ;
            public int Axis5nmZ;

            public string F1v, F1h, F1Y;
            public string F2v, F2h, F2Y;

            public int ChIn, ChOut;
            public bool Bright, ScanX;
            public string SaveDir, FileName;
            public string ScanStart, StartFromPeak, ScanPitch;
            public string ScanMax;
            public string ExposTime, WaitTime;

            public string background;
        }

        XmlSerializer serializer = new XmlSerializer(typeof(Setting));
        FileStream fs;

        private void toolStripMenuItem_LoadSetting_Click(object sender, EventArgs e)
        {
            this.ofd_setting.Filter = "Scanner Setting(*Setting_Scanner.xml)|*Setting_Scanner.xml";

            if (this.ofd_setting.ShowDialog() == DialogResult.OK)
            {
                fs = new FileStream(this.ofd_setting.FileName, FileMode.Open);
                Setting setting = (Setting)serializer.Deserialize(fs);
                fs.Close();

                this.comboBox_5nm_Port.Text = setting.Port5nm;
                this.button_5nmZ_Axis.Text = Convert.ToString(setting.Axis5nmZ);
                this.Axis5nmZ = setting.Axis5nmZ;
                this.Axis5nmX = this.Axis5nmZ == 1 ? 2 : 1;

                this.comboBox_1nmX_Port.Text = setting.Port1nmX;
                this.comboBox_1nmZ_Port.Text = setting.Port1nmZ;

                this.comboBox_F1vAngle_VmeCh.Text = setting.F1v;
                this.comboBox_F1hAngle_VmeCh.Text = setting.F1h;
                this.comboBox_F1Y_VmeCh.Text = setting.F1Y;
                this.comboBox_F2vAngle_VmeCh.Text = setting.F2v;
                this.comboBox_F2hAngle_VmeCh.Text = setting.F2h;
                this.comboBox_F2Y_VmeCh.Text = setting.F2Y;

                this.textBox_Setting_ChIn.Text = setting.ChIn.ToString();
                this.textBox_Setting_ChOut.Text = setting.ChOut.ToString();
                this.radioButton_Setting_BrightField.Checked = setting.Bright;
                this.radioButton_Setting_ScanX.Checked = setting.ScanX;
                this.radioButton_Setting_ScanZ.Checked = !setting.ScanX;
                
                this.fbd.SelectedPath = setting.SaveDir;
                this.textBox_FileName.Text = setting.FileName;
                fbdselected();

                this.textBox_S1_ScanStart.Text = setting.ScanStart;
                this.textBox_S1_StartFromPeak.Text = setting.StartFromPeak;
                this.textBox_S1_ScanPitch.Text = setting.ScanPitch;
                this.textBox_S1_ScanMax.Text = setting.ScanMax;
                this.textBox_S1_ExposureTime.Text = setting.ExposTime;
                this.textBox_S1_WaitTime.Text = setting.WaitTime;

                this.textBox_Background.Text = setting.background;
            }

        }

        private void toolStripMenuItem_SaveSetting_Click(object sender, EventArgs e)
        {
            this.sfd_setting.FileName = DateTime.Now.ToString("yyyyMMdd_HHmm") + "Setting_Scanner.xml";
            this.sfd_setting.Filter = "Scanner Setting(*Setting_Scanner.xml)|*Setting_Scanner.xml";
            if (this.sfd_setting.ShowDialog() == DialogResult.OK)
            {

                Setting setting = new Setting();

                setting.Port5nm = this.comboBox_5nm_Port.Text;
                setting.Axis5nmZ = Convert.ToInt16(this.button_5nmZ_Axis.Text);

                setting.Port1nmX = this.comboBox_1nmX_Port.Text;
                setting.Port1nmZ = this.comboBox_1nmZ_Port.Text;

                setting.F1v = this.comboBox_F1vAngle_VmeCh.Text;
                setting.F1h = this.comboBox_F1hAngle_VmeCh.Text;
                setting.F1Y = this.comboBox_F1Y_VmeCh.Text;
                setting.F2v = this.comboBox_F2vAngle_VmeCh.Text;
                setting.F2h = this.comboBox_F2hAngle_VmeCh.Text;
                setting.F2Y = this.comboBox_F2Y_VmeCh.Text;

                setting.ChIn = Convert.ToInt16(this.textBox_Setting_ChIn.Text);
                setting.ChOut = Convert.ToInt16(this.textBox_Setting_ChOut.Text);
                setting.Bright = this.radioButton_Setting_BrightField.Checked;
                setting.ScanX = this.radioButton_Setting_ScanX.Checked;
                setting.SaveDir = this.fbd.SelectedPath;
                setting.FileName = this.textBox_FileName.Text;

                setting.ScanStart = this.textBox_S1_ScanStart.Text;
                setting.StartFromPeak = this.textBox_S1_StartFromPeak.Text;
                setting.ScanPitch = this.textBox_S1_ScanPitch.Text;
                setting.ScanMax = this.textBox_S1_ScanMax.Text;
                setting.ExposTime = this.textBox_S1_ExposureTime.Text;
                setting.WaitTime = this.textBox_S1_WaitTime.Text;

                setting.background = this.textBox_Background.Text;

                fs = new FileStream(this.sfd_setting.FileName, FileMode.Create);
                serializer.Serialize(fs, setting);
                fs.Close();
            }
        }

        #endregion

        #region control
        private void checkBox_S1_CrossStagePitch_CheckedChanged(object sender, EventArgs e)
        {
            if(this.checkBox_S1_CrossStagePitch.Checked)
            { this.textBox_S1_CrossStagePitch.Enabled = true; }
            else
            { this.textBox_S1_CrossStagePitch.Enabled = false; }
        }

        private void checkBox_minusFWHM_CheckedChanged(object sender, EventArgs e)
        {
            if (this.checkBox_minusFWHM.Checked)
                scan.minusPeak = true;
            else
                scan.minusPeak = false;
        }

        private void checkBox_GraphLog_CheckedChanged(object sender, EventArgs e)
        {
            if (this.checkBox_GraphLog.Checked)
                this.GraphLog = true;
            else
                this.GraphLog = false;
        }

        private void button_background_Click(object sender, EventArgs e)
        {
            try
            {
                double bg;
                if (double.TryParse(this.textBox_Background.Text, out bg))
                    scan.scanData.bg = bg;
            }
            catch { }
        }

        private void button_bgAuto_Click(object sender, EventArgs e)
        {
            double[] count = scan.scanData.CountNT.ToArray();
            double bg = 0.0;
            for(int i=0;i<count.Length;i++)
            {
                bg += count[i];
            }
            bg /= count.Length;
            this.textBox_Background.Text = bg.ToString("F4");
            
        }
        #endregion

        private void toolStripMenuItem_2DScan_Click(object sender, EventArgs e)
        {
            if (this.form_2DScan == null || this.form_2DScan.IsDisposed) 
                this.form_2DScan = new Form_2DScan(this);

            if (this.form_2DScan.WindowState == FormWindowState.Minimized)
                this.form_2DScan.WindowState = FormWindowState.Normal;
            else if (!this.form_2DScan.Visible)
                this.form_2DScan.Show();
            else
                this.form_2DScan.Focus();
        }


    }
}







