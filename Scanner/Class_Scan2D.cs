using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
namespace Scanner
{
    class CountData2D
    {

        public string[] strCh = new string[] { "Ch0", "Ch1", "Ch2", "Ch3", "Ch4", "Ch5", "Ch6", "Ch7" };

        string saveDir;

        public double[,] PosX;
        public double[,] PosZ;
        public double[][,] dCount;
        public double[] count;

        int XNum;
        int ZNum;
        double dExposTime;

        const int nCh = 8;

        public CountData2D(int _iScanX, int _iScanZ, double _dExposTime)
        {
            XNum = _iScanX;
            ZNum = _iScanZ;
            dExposTime = _dExposTime;
            PosX = new double[XNum, ZNum];
            PosZ = new double[XNum, ZNum];
            dCount = new double[nCh][,];
            for (int i = 0; i < nCh; i++) dCount[i] = new double[XNum, ZNum];
        }

        public void Add(int i, int j, double x, double z, double[] _count, int NormCh = -1)
        {
            PosX[i, j] = x;
            PosZ[i, j] = z;
            count = _count;
            for (int k = 0; k < nCh; k++)
                dCount[k][i, j] = _count[k];
        }

        public void Save(string _saveDir,string[] strChName)
        {
            saveDir = _saveDir;
            if (!System.IO.Directory.Exists(saveDir)) System.IO.Directory.CreateDirectory(saveDir);

            #region 1D data
            //PosX,PosZ,Ch0-7
            //saveDir\AllData.csv
            StringBuilder sb = new StringBuilder();
            //Header
            sb.Append("PosX,PosZ,");
            for (int i = 0; i < strCh.Length; i++)
            {
                sb.Append(strCh[i] + "_" + strChName[i]).Append(",");
            }
            sb.Append("ExposureTime=" + dExposTime.ToString("F3") + "\r\n");

            //data
            for (int j = 0; j < ZNum; j++)
            {
                for (int i = 0; i < XNum; i++)
                {
                    sb.Append(PosX[i, j].ToString()).Append(",");
                    sb.Append(PosZ[i, j].ToString()).Append(",");
                    for (int k = 0; k < strCh.Length; k++)
                    {
                        sb.Append(dCount[k][i, j].ToString()).Append(",");
                    }
                    sb.Remove(sb.Length - 1, 1);
                    sb.Append("\r\n");
                }
            }
            File.WriteAllText(saveDir + "\\AllData.csv", sb.ToString());
            #endregion

            #region 2D data
            //各データ毎にファイル分け
            //PosX,PosZ,Ch0R,Ch0N,Ch0NT,Ch1R,...Ch7NT
            //saveDir\PosX.csv
            for (int k = 0; k < strCh.Length; k++)
            {
                sb.Clear();

                for (int j = 0; j < ZNum; j++)
                {
                    for (int i = 0; i < XNum; i++)
                    {
                        sb.Append(dCount[k][i, j].ToString()).Append(",");
                    }
                    sb.Remove(sb.Length - 1, 1);
                    sb.Append("\r\n");
                }
                File.WriteAllText(saveDir + "\\" + strCh[k] + "_" + strChName[k] + ".csv", sb.ToString());
            }
            #endregion
        }

        public double[][,] NCountData2D(int Ch)
        {
            double[][,] data = dCount;
            if (0 <= Ch || Ch < 8)
            {
                double[,] nData = data[Ch];
                for (int k = 0; k < nCh; k++)
                {
                    for(int i=0;i<XNum;i++)
                    {
                        for(int j=0;j<ZNum;j++)
                        {
                            data[k][i, j] /= nData[i, j];
                        }
                    }
                }
            }
            return data;
        }
    }

    public class ScanPara2D
    {
        public string fbd;
        public string savedir;

        #region feedbackstage
        internal Class_FeedbackStage fsX;
        internal Class_FeedbackStage fsZ;
        public int axisX;
        public int axisZ;
        #endregion

        #region Counter
        public int EH;
        internal VmeControl vme;
        public double dExposTime;
        public double dWaitTime;
        #endregion

        #region ScanParameter
        public int XStart;
        public int XPitch;
        public int XNum;

        public int ZStart;
        public int ZPitch;
        public int ZNum;

        public enum ScanMethod { raster, vector };
        public ScanMethod scanMethod;
        #endregion

        public string[] strChName;

        public override string ToString()
        {
            string strReturn = "";
            strReturn = "X:" + XStart.ToString() + "," + XPitch.ToString() + "," + XNum.ToString();
            strReturn += " Z:" + ZStart.ToString() + "," + ZPitch.ToString() + "," + ZNum.ToString();
            return strReturn;
        }
    }

    class Scan2D
    {
        bool isScan;
        public bool IsScan { get { return isScan; } }
        CountData2D countData2D;
        string saveDir;

        bool flagFinish;

        ScanPara2D pm;

        double PosX;
        double PosZ;
        int ij;

        public Scan2D() { pm = null; }
        
        public Scan2D(Class_FeedbackStage _fsx, Class_FeedbackStage _fsz,int _axisX,int _axisZ,
            VmeControl _vme,int _EH,
            int _scanXStart, int _scanXPitch, int _scanXNum,
            int _scanZStart, int _scanZPitch, int _scanZNum,
            double _dExposTime, double _dWaitTime,
            ScanPara2D.ScanMethod _scanMethod = ScanPara2D.ScanMethod.raster)
        {
            Init(_fsx, _fsz, _axisX, _axisZ, _vme, _EH, _scanXStart, _scanXPitch, _scanXNum, _scanZStart, _scanZPitch, _scanZNum, _dExposTime, _dWaitTime, _scanMethod);
        }

        public void Init(Class_FeedbackStage _fsX, Class_FeedbackStage _fsZ, int _axisX, int _axisZ,
            VmeControl _vme, int _EH,
            int _scanXStart, int _scanXPitch, int _scanXNum,
            int _scanZStart, int _scanZPitch, int _scanZNum,
            double _dExposTime, double _dWaitTime,
             ScanPara2D.ScanMethod _scanMethod = ScanPara2D.ScanMethod.raster)
        {
            pm = new ScanPara2D();

            pm.fsX = _fsX;
            pm.fsZ = _fsZ;
            pm.axisX = _axisX;
            pm.axisZ = _axisZ;
            pm.vme = _vme;
            pm.EH = _EH;

            pm.XStart = _scanXStart;
            pm.XPitch = _scanXPitch;
            pm.XNum = _scanXNum;

            pm.ZStart = _scanZStart;
            pm.ZPitch = _scanZPitch;
            pm.ZNum = _scanZNum;

            pm.dExposTime = _dExposTime;
            pm.dWaitTime = _dWaitTime;
            pm.scanMethod = _scanMethod;

            countData2D = new CountData2D(pm.XNum, pm.ZNum, pm.dExposTime);

            flagFinish = false;
        }

        public Scan2D(ScanPara2D _pm) { Init(_pm); }

        public void Init(ScanPara2D _pm)
        {
            pm = _pm; 
            
            countData2D = new CountData2D(pm.XNum, pm.ZNum, pm.dExposTime);

            flagFinish = false;
        }

        public void TaskScan(string _saveDir)
        {
            saveDir = _saveDir;
            isScan = true;

            int i = 0;
            int j = 0;
            ij = 0;
            PosX = 0;
            PosZ = 0;


            do
            {
                //ステージ動作
                setScanStatus("ステージ移動");
                pm.fsX.MoveAbs(pm.XStart + i * pm.XPitch);
                pm.fsZ.MoveAbs(pm.ZStart + j * pm.ZPitch);

                setScanStatus("ステージ移動完了後待機");
                Thread.Sleep((int)(1000 * pm.dWaitTime));

                //カウント
                setScanStatus("カウント計測");
                double[] count = pm.vme.CountOnce(pm.EH, pm.dExposTime);

                #region 位置取得
                setScanStatus("ステージ座標取得");
                string[] strPosX = pm.fsX.Position();
                PosX = Convert.ToDouble(strPosX[1]);
                if (strPosX[0].Substring(0, 1) == "C")
                {
                    //CWlimit
                    flagFinish = true;
                }
                else if (strPosX[0].Substring(0, 1) == "W")
                {
                    //CCWlimit
                    flagFinish = true;
                }
                else if (strPosX[0].Substring(0, 1) == "K")
                {
                    //正常
                }

                string[] strPosZ = pm.fsZ.Position();
                PosZ = Convert.ToDouble(strPosZ[1]);
                if (strPosZ[0].Substring(0, 1) == "C")
                {
                    //CWlimit
                    flagFinish = true;
                }
                else if (strPosZ[0].Substring(0, 1) == "W")
                {
                    //CCWlimit
                    flagFinish = true;
                }
                else if (strPosZ[0].Substring(0, 1) == "K")
                {
                    //正常
                }
                #endregion

                //データ格納
                setScanStatus("データ格納");
                countData2D.Add(i, j, PosX, PosZ, count);

                setGraph(this.countData2D);

                //次の座標を決定
                setScanStatus("次のスキャンへ");


                if (pm.scanMethod == ScanPara2D.ScanMethod.raster)
                {
                    if (j % 2 == 0 && i < pm.XNum-1)
                    { i++; }
                    else if (j % 2 == 1 && 0 < i)
                    { i--; }
                    else
                    { j++; }
                }
                else
                {
                    if (i < pm.XNum)
                    { i++; }
                    else
                    {
                        i = 0;
                        j++;
                    }
                }

                ij++;

                //終了判定
                if (ij == pm.XNum * pm.ZNum)
                { flagFinish = true; }


            } while (!flagFinish);

            #region 終了処理
            //Save
            setScanStatus("保存");
            countData2D.Save(saveDir, pm.strChName);
            
            //初期座標に戻る
            setScanStatus("初期座標へ帰還");
            pm.fsX.MoveAbs(pm.XStart);
            pm.fsZ.MoveAbs(pm.ZStart);

            isScan = false;
            #endregion

        }

        public void Abort()
        {
            flagFinish = true;
        }

        #region GraphEvent

        public event ScanDataEventHandler graphSet;
        protected virtual void graphSetEvent(CountData2D _scan2DData)
        {
            if (_scan2DData != null)
                graphSet(_scan2DData);
        }
        internal void setGraph(CountData2D _scan2DData)
        {
            graphSetEvent(_scan2DData);
        }

        #endregion

        #region ScanStatus

        public event ScanStatusEventHandler scanStatusSet;
        protected virtual void scanStatusSetEvent(string _ScanStatus)
        {
            if (_ScanStatus != null)
                scanStatusSet(_ScanStatus);
        }
        internal void setScanStatus(string _ScanStatus)
        {
            string strScan = "スキャン: " + ij.ToString() + "/" + (pm.XNum * pm.ZNum).ToString() + " 回 | ";
            string strPosX = "X座標: " + PosX.ToString() + "[nm] | ";
            string strPosZ = "Z座標: " + PosZ.ToString() + "[nm] | ";

            scanStatusSetEvent(strScan + strPosX + strPosZ + _ScanStatus);
        }

        #endregion

        class ScanDataEventArgs : EventArgs
        {
            public ScanDataEventArgs(CountData2D scan2DData)
            {
                this.scan2DData = scan2DData;
            }
            public CountData2D scan2DData { get; set; }
        }
        public delegate void ScanDataEventHandler(CountData2D scan2DData);


        class ScanStatusEventArgs : EventArgs
        {
            public ScanStatusEventArgs(string ScanStatus)
            {

            }
            public string _ScanStatus { get; set; }
        }
        public delegate void ScanStatusEventHandler(string ScanStatus);
    }

}
