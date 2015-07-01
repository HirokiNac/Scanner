using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace Scanner
{
    public partial class Form_2DScan : Form
    {
        Form_Main form1;
        Scan2D scan2D;
        List<ScanPara2D> qList = new List<ScanPara2D>();

        PictureBox[] picBox;
        TextBox[] txBox;
        ScanPara2D pm;

        bool woZero = true;

        bool flagFromMouse = false;

        readonly string[] EH = new string[] { "EH1", "EH2", "EH3", "EH4" };
        const int nCh = 8;

        int NormalizedCh;

        public Form_2DScan(Form_Main _form1)
        {
            InitializeComponent();

            form1 = _form1;

            #region picturebox
            picBox = new PictureBox[nCh];
            picBox[0] = this.pictureBox0;
            picBox[1] = this.pictureBox1;
            picBox[2] = this.pictureBox2;
            picBox[3] = this.pictureBox3;
            picBox[4] = this.pictureBox4;
            picBox[5] = this.pictureBox5;
            picBox[6] = this.pictureBox6;
            picBox[7] = this.pictureBox7;
            for (int i = 0; i < nCh; i++)
                picBox[i].MouseClick += new MouseEventHandler(picBox_MouseClick);
            #endregion

            #region textbox
            txBox = new TextBox[nCh];
            txBox[0] = this.textBox_Ch0;
            txBox[1] = this.textBox_Ch1;
            txBox[2] = this.textBox_Ch2;
            txBox[3] = this.textBox_Ch3;
            txBox[4] = this.textBox_Ch4;
            txBox[5] = this.textBox_Ch5;
            txBox[6] = this.textBox_Ch6;
            txBox[7] = this.textBox_Ch7;
            #endregion

            scan2D = new Scan2D();
            scan2D.graphSet += new Scan2D.ScanDataEventHandler(scan2D_graphSet);
            scan2D.scanStatusSet += new Scan2D.ScanStatusEventHandler(scan2D_scanStatusSet);

            this.comboBox_EH.Items.AddRange(EH);
            this.comboBox_EH.SelectedIndex = 2;


        }

        private void Form_2DScan_Load(object sender, EventArgs e)
        {
            this.dataGridView_Counter.Rows.Add(nCh);
            this.dataGridView_Counter.AllowUserToAddRows = false;
            for (int j = 0; j < nCh; j++)
                this.dataGridView_Counter["Column_Ch", j].Value = j;

        }

        void scan2D_scanStatusSet(string ScanStatus)
        {
            BeginInvoke((Action)(() =>
            {
                this.toolStripStatusLabel_Status.Text = ScanStatus;
            }));
        }

        void scan2D_graphSet(CountData2D scan2DData)
        {
            double[][,] data = scan2DData.NCountData2D(NormalizedCh);

            BeginInvoke((Action)(() =>
            {
                ClsNac.Graphic.Plot2dPlane Plane;
                for (int k = 0; k < nCh; k++)
                {
                    Plane = new ClsNac.Graphic.Plot2dPlane(picBox[k]);
                    Plane.Draw(data[k], this.woZero);
                    this.dataGridView_Counter["Column_Count", k].Value = scan2DData.count[k];
                }
                
            }));
        }

        private async void button_ScanStart_Click(object sender, EventArgs e)
        {
            this.button_ScanStart.Enabled = false;
            this.button_ScanStop.Enabled = true;

            #region  caution
            if (scan2D.IsScan)
            {
                MessageBox.Show("スキャン中です");
                return;
            }
            if (!this.form1.vme.IsConnect)
            {
                MessageBox.Show("VMEサーバ未接続");
                return;
            }
            #endregion

            try
            {
                do
                {
                    #region Parameterの設定
                    if (qList.Count == 0)
                    {
                        if (!SetParameter(out pm))
                        {
                            MessageBox.Show("");
                            return;
                        }
                    }
                    else
                    {
                        pm = qList[0];
                        qList.RemoveAt(0);
                        this.listBox_ScanReserve.Items.RemoveAt(0);
                    }
                    #endregion

                    #region 保存先設定
                    //fbd\\scan2D0000F2R(\\data.csv)
                    string f = this.radioButton_Focus1.Checked ? "F1" : "F2";
                    string s = this.radioButton_RasterScan.Checked ? "R" : "V";
                    string saveDir = this.fbd.SelectedPath + "\\" + this.textBox_DirName.Text + ((int)this.numericUpDown_DirNum.Value).ToString("D4") + f + s;

                    #endregion

                    #region スキャン開始
                    //スキャン初期化
                    scan2D.Init(pm);
                    await Task.Run(() => scan2D.TaskScan(saveDir));
                    #endregion
                    //設定を保存
                    SaveSetting(saveDir + "\\Setting_2DScan.xml", pm);

                    this.button_ScanStop.Text = "Scan Stop";
                    this.numericUpDown_DirNum.Value++;

                } while (qList.Count > 0);

            }
            finally
            {                    

                this.button_ScanStart.Enabled = true;
                this.button_ScanStop.Enabled = false;
            }

        }

        private void button_ScanStop_Click(object sender, EventArgs e)
        {
            if(scan2D.IsScan)
            {
                this.button_ScanStop.Text = "停止処理中";
                scan2D.Abort();
            }
        }

        bool SetParameter(out ScanPara2D _pm)
        {
            _pm = new ScanPara2D();

            bool success = true;

            success &= Int32.TryParse(this.textBox_XStart.Text, out _pm.XStart);
            success &= Int32.TryParse(this.textBox_XPitch.Text, out _pm.XPitch);
            success &= Int32.TryParse(this.textBox_XNum.Text, out _pm.XNum);

            success &= Int32.TryParse(this.textBox_ZStart.Text, out _pm.ZStart);
            success &= Int32.TryParse(this.textBox_ZPitch.Text, out _pm.ZPitch);
            success &= Int32.TryParse(this.textBox_ZNum.Text, out _pm.ZNum);

            success &= Double.TryParse(this.textBox_ExposureTime.Text, out _pm.dExposTime);
            success &= Double.TryParse(this.textBox_WaitTime.Text, out _pm.dWaitTime);

            _pm.EH = this.comboBox_EH.SelectedIndex+1;

            _pm.vme = this.form1.vme;

            if (this.radioButton_Focus2.Checked)
            {
                _pm.fsX = this.form1.fs1nmX;
                _pm.fsZ = this.form1.fs1nmZ;
                _pm.axisX = 1;
                _pm.axisZ = 1;
            }
            else if (this.radioButton_Focus1.Checked)
            {
                _pm.fsX = this.form1.fs5nm;
                _pm.fsZ = this.form1.fs5nm;
                _pm.axisX = this.form1.Axis5nmX;
                _pm.axisZ = this.form1.Axis5nmZ;
            }
            else success = false;

            if (this.radioButton_VectorScan.Checked) _pm.scanMethod = ScanPara2D.ScanMethod.vector;
            else _pm.scanMethod = ScanPara2D.ScanMethod.raster;

            _pm.strChName=new string[nCh];
            for (int i = 0; i < nCh; i++)
                _pm.strChName[i] = txBox[i].Text;

            _pm.fbd = this.fbd.SelectedPath;
            _pm.savedir = this.textBox_DirName.Text;

            return success;
        }

        #region reserve
        private void button_ScanReserveAdd_Click(object sender, EventArgs e)
        {
            ScanPara2D _pm = new ScanPara2D();
            if (!SetParameter(out _pm)) return;

            qList.Add(_pm);
            this.listBox_ScanReserve.Items.Add(_pm.ToString());
        }

        private void button_ScanReserveDelete_Click(object sender, EventArgs e)
        {
            this.listBox_ScanReserve.Items.RemoveAt(this.listBox_ScanReserve.SelectedIndex);
            qList.RemoveAt(this.listBox_ScanReserve.SelectedIndex);
        }

        private void button_ScanReserveAllDelete_Click(object sender, EventArgs e)
        {
            this.listBox_ScanReserve.Items.Clear();
            qList.Clear();
        }
        #endregion

        #region directory
        private void button_fbd_Click(object sender, EventArgs e)
        {
            if(this.fbd.ShowDialog()==DialogResult.OK)
            {
                this.textBox_fbd.Text = this.fbd.SelectedPath;
                GetDirNum();
            }
        }

        private void button_GetDirNum_Click(object sender, EventArgs e)
        {
            GetDirNum();
        }

        void GetDirNum()
        {
            string[] dirName = Directory.GetDirectories(this.fbd.SelectedPath);
            if (dirName.Length != 0)
            {
                int max = 0;
                int count = 0;
                foreach (string _dirName in dirName)
                {
                    //"設定したdirectory名"以降の文字列を抽出
                    string subDirName = Path.GetFileName(_dirName);

                    Regex reg = new Regex(@"(\w+)(\d{4})");
                    Match m = reg.Match(subDirName);

                    //"設定したdirectory名"が含まれない場合は次のファイル名へ
                    if (m.Groups[1].Value.IndexOf(this.textBox_DirName.Text) < 0)
                    { continue; }

                    //directoryナンバーを抽出
                    int dirNumber = Convert.ToInt32(m.Groups[2].Value);

                    //最大を求める
                    max = max < dirNumber ? dirNumber : max;
                    count++;
                }
                if (count == 0) this.numericUpDown_DirNum.Value = 0;
                else this.numericUpDown_DirNum.Value = max + 1;
            }
            else
                this.numericUpDown_DirNum.Value = 0;
        }
        #endregion

        #region getposition
        private async void button_CurrentPos_Click(object sender, EventArgs e)
        {
            string[] strPosX;
            string[] strPosZ;
            if(this.radioButton_Focus1.Checked)
            {
                strPosX = await Task.Run(() => form1.fs5nm.Position(form1.Axis5nmX));
                strPosZ = await Task.Run(() => form1.fs5nm.Position(form1.Axis5nmZ));
            }
            else
            {
                strPosX = await Task.Run(() => form1.fs1nmX.Position());
                strPosZ = await Task.Run(() => form1.fs1nmZ.Position());
            }
            this.textBox_XStart.Text = strPosX[1];
            this.textBox_ZStart.Text = strPosZ[1];
        }

        private void button_GraphPos_Click(object sender, EventArgs e)
        {
            Click_button_StartFromGraph((Button)sender, ref this.flagFromMouse);

        }

        void Click_button_StartFromGraph(Button button,ref bool flag)
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

        void picBox_MouseClick(object sender, MouseEventArgs e)
        {
            if(e.Button==MouseButtons.Left)
            {
                if(flagFromMouse)
                {
                    PictureBox pb = (PictureBox)sender;
                    double x = pm.XStart + pm.XPitch * (int)(pm.XNum * e.Location.X / pb.Size.Width);
                    double z = pm.ZStart + pm.ZPitch * (int)(pm.ZNum * e.Location.Y / pb.Size.Height);

                    this.textBox_XStart.Text = x.ToString("F0");
                    this.textBox_ZStart.Text = z.ToString("F0");

                    this.button_GraphPos.Text = "Graph";
                    this.button_GraphPos.ForeColor = Color.Black;
                    this.flagFromMouse = false;
                }
            }

        }
        #endregion


        #region setting

        XmlSerializer serializer = new XmlSerializer(typeof(ScanPara2D));
        FileStream fs;

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.sfd_setting.FileName = DateTime.Now.ToString("yyyyMMdd_HHmm") + "Setting_2DScan.xml";
            this.sfd_setting.Filter = "2DScan Setting(*Setting_2DScan.xml)|*Setting_2DScan.xml";
            if (this.sfd_setting.ShowDialog() == DialogResult.OK)
            {
                ScanPara2D setting = new ScanPara2D();

                SetParameter(out setting);

                SaveSetting(this.sfd_setting.FileName, setting);
            }

        }

        private void SaveSetting(string fileName, ScanPara2D setting)
        {
            fs = new FileStream(fileName, FileMode.Create);
            serializer.Serialize(fs, setting);
            fs.Close();

        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.ofd_setting.Filter = "2DScan Setting(*Setting_2DScan.xml)|*Setting_2DScan.xml";
            if(this.ofd_setting.ShowDialog()==DialogResult.OK)
            {
                fs = new FileStream(this.ofd_setting.FileName, FileMode.Open);
                ScanPara2D setting = (ScanPara2D)serializer.Deserialize(fs);
                fs.Close();

                this.fbd.SelectedPath = setting.fbd;
                this.textBox_fbd.Text = setting.fbd;
                this.textBox_DirName.Text = setting.savedir;
                GetDirNum();

                this.textBox_XStart.Text = setting.XStart.ToString();
                this.textBox_XPitch.Text = setting.XPitch.ToString();
                this.textBox_XNum.Text = setting.XNum.ToString();
                this.textBox_ZStart.Text = setting.ZStart.ToString();
                this.textBox_ZPitch.Text = setting.ZPitch.ToString();
                this.textBox_ZNum.Text = setting.ZNum.ToString();

                this.comboBox_EH.SelectedIndex = setting.EH - 1;
                this.textBox_ExposureTime.Text = setting.dExposTime.ToString();
                this.textBox_WaitTime.Text = setting.dWaitTime.ToString();

                for (int i = 0; i < nCh; i++)
                    txBox[i].Text = setting.strChName[i];
            }

        }

        #endregion

        #region checkbox
        private void checkBox_woZero_CheckedChanged(object sender, EventArgs e)
        {
            if (this.checkBox_woZero.Checked)
                this.woZero = true;
            else
                this.woZero = false;
        }

        private void radioButton_2DMapRawData_CheckedChanged(object sender, EventArgs e)
        {
            if(this.radioButton_2DMapRawData.Checked)
            {
                NormalizedCh = -1;
            }
            else
            {
                NormalizedCh = (int)nud_NormalizedCh.Value;
            }
        }

        private void radioButton_2DMapNormalize_CheckedChanged(object sender, EventArgs e)
        {
            if (this.radioButton_2DMapRawData.Checked)
            {
                NormalizedCh = -1;
            }
            else
            {
                NormalizedCh = (int)nud_NormalizedCh.Value;
            }
        }

        #endregion
    }
}
