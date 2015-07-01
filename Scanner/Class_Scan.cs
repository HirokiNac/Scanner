using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;

namespace Scanner
{
    public class ScanData
    {
        public double bg { get; set; }
        public int iScanCount { get; private set; }
        public double dExposTime { get; private set; }

        /// <summary>Position [nm]</summary>
        public List<double> Position { get; private set; }

        /// <summary>Inカウント CountIn [count]</summary>
        public List<double> CountIn { get; private set; }
        /// <summary>Outカウント CountOut [count]</summary>
        public List<double> CountOut { get; private set; }

        /// <summary>単位時間Inカウント CountIn/ExposureTime [count]</summary>
        public List<double> CountInNT { get; private set; }
        /// <summary>単位時間Outカウント CountOut/ExposureTime [count]</summary>
        public List<double> CountOutNT { get; private set; }

        /// <summary>規格化カウント OutNT/InNT</summary>
        public List<double> CountNT { get; private set; }

        /// <summary>規格化微分値</summary>
        public List<double> CountND { get; private set; }

        public List<double> CountND2 { get; private set; }

        #region keisan
        /// <summary>
        /// CountNTピークのX座標
        /// </summary>
        public double PeakX_CountNT { get { return PeakPosition(Position.ToArray(), CountNT.ToArray()); } }

        /// <summary>
        /// CountNTの外FWHM
        /// </summary>
        public Tuple<double, double> FWHM_NT { get { return FWHM(Position, CountNT,bg); } }

        /// <summary>
        /// CountNDピークのX座標
        /// </summary>
        public double PeakX_CountND { get { return PeakPosition(Position.ToArray(), CountND.ToArray()); } }

        /// <summary>
        /// 微分値の内FWHM,外FWHM
        /// </summary>
        public Tuple<double, double> FWHM_ND { get { return FWHM(Position, CountND); } }

        /// <summary>
        /// 微分値２の内FWHM,外FWHM
        /// </summary>
        public Tuple<double, double> FWHM_ND2 { get { return FWHM(Position, CountND2); } }

        public Tuple<double, double> FWHM_NDmin { get { return FWHM_minus(Position, CountND); } }

        public Tuple<double, double> FWHM_ND2min { get { return FWHM_minus(Position, CountND2); } }


        #endregion

        public ScanData(double _dExposTime)
        {
            Initialize(_dExposTime);
        }

        public void Initialize(double _dExposTime)
        {
            iScanCount = 0;
            dExposTime = _dExposTime;
            Position = new List<double>();
            CountIn = new List<double>();
            CountOut = new List<double>();

            CountInNT = new List<double>();
            CountOutNT = new List<double>();

            CountND = new List<double>();
            CountNT = new List<double>();
            CountND2 = new List<double>();
        }

        /// <summary>
        /// データの追加
        /// </summary>
        /// <param name="_iScanCount"></param>
        /// <param name="_Position"></param>
        /// <param name="_CountIn"></param>
        /// <param name="_CountOut"></param>
        /// <param name="pitch"></param>
        public void Add(int _iScanCount, double _Position, double _CountIn, double _CountOut, int pitch)
        {
            if (iScanCount == _iScanCount)
            {
                iScanCount++;
            }
            else if (iScanCount > _iScanCount)
            {
                Position.RemoveRange(_iScanCount, iScanCount - _iScanCount + 1);

                CountIn.RemoveRange(_iScanCount, iScanCount - _iScanCount + 1);
                CountOut.RemoveRange(_iScanCount, iScanCount - _iScanCount + 1);

                CountInNT.RemoveRange(_iScanCount, iScanCount - _iScanCount + 1);
                CountOutNT.RemoveRange(_iScanCount, iScanCount - _iScanCount + 1);

                CountNT.RemoveRange(_iScanCount, iScanCount - _iScanCount + 1);
                CountND.RemoveRange(_iScanCount, iScanCount - _iScanCount + 1);
                CountND2.RemoveRange(_iScanCount, iScanCount - _iScanCount + 1);
            }
            else if (iScanCount < _iScanCount)
            {
                for (int i = iScanCount; i < _iScanCount; i++)
                {
                    Position.Add(_Position - pitch * (_iScanCount - iScanCount + i));

                    CountIn.Add(_CountIn);
                    CountOut.Add(_CountOut);

                    CountInNT.Add(_CountIn / dExposTime);
                    CountOutNT.Add(_CountOut / dExposTime);

                    CountNT.Add(CountOutNT[i] / CountInNT[i]);
                    CountND.Add(i == 0 ? 0.0 : -(CountNT[i - 1] - CountNT[i]) / (Position[i - 1] - Position[i]));
                    CountND2.Add(i < 2 ? 0.0 : -(CountNT[i - 2] - CountNT[i]) / (Position[i - 2] - Position[i]));

                }
            }

            Position.Add(_Position);

            CountIn.Add(_CountIn);
            CountOut.Add(_CountOut);

            CountInNT.Add(_CountIn / dExposTime);
            CountOutNT.Add(_CountOut / dExposTime);

            CountNT.Add(CountOutNT[_iScanCount] / CountInNT[_iScanCount]);
            CountND.Add(_iScanCount == 0 ? 0.0 : -(CountNT[_iScanCount - 1] - CountNT[_iScanCount]) / (Position[_iScanCount - 1] - Position[_iScanCount]));
            CountND2.Add(_iScanCount < 2 ? 0.0 : -(CountNT[_iScanCount - 2] - CountNT[_iScanCount]) / (Position[_iScanCount - 2] - Position[_iScanCount]));

        }

        public void SaveData(string filePath, string strComment = "")
        {
            StringBuilder sbData = new StringBuilder();
            sbData.AppendLine("pztPos\tCountA\tCountA(単位時間当たり)\tCountB\tCountB(単位時間当たり)\tCountA(規格化値：/CountB)\tCountdA(微分値)\t" + strComment);
            for (int i = 0; i < iScanCount; i++)
            {
                sbData.Append(Position[i].ToString()).Append("\t");
                sbData.Append(CountIn[i].ToString()).Append("\t");
                sbData.Append(CountInNT[i].ToString()).Append("\t");
                sbData.Append(CountOut[i].ToString()).Append("\t");
                sbData.Append(CountOutNT[i].ToString()).Append("\t");
                sbData.Append(CountNT[i].ToString()).Append("\t");
                sbData.Append(CountND[i].ToString()).Append("\r\n");
            }
            if (!Directory.Exists(Path.GetDirectoryName(filePath)))
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            File.AppendAllText(filePath, sbData.ToString(), System.Text.Encoding.Unicode);
        }

        /// <summary>
        /// FWHM計算
        /// </summary>
        /// <param name="Position"></param>
        /// <param name="Count"></param>
        /// <returns>内FWHM,外FWHM</returns>
        static Tuple<double,double>FWHM(double[] Position,double[] Count,double background=0.0)
        {
            if (Position.Length > 2)
            {
                double max = 0.0;
                int iMax = 0;
                for (int i = 0; i < Position.Length; i++)
                {
                    Count[i] -= background;
                    if (Count[i] > max)
                    {
                        max = Count[i];
                        iMax = i;
                    }
                }

                #region FWHM1
                //外側からピークトップにかけて
                //カウント/2以上になる境目を探す
                double dblePos1 = 0.0;
                double dblePos2 = 0.0;
                for (int i = 1; i <= iMax; i++)
                {
                    if (max / 2.0 < Count[i])
                    {
                        dblePos1 = (Position[i] - Position[i - 1]) / (Count[i] - Count[i - 1]) * (max / 2.0 - Count[i]) + Position[i];
                        break;
                    }
                }
                for (int i = Position.Length - 2; i >= iMax; i--)
                {
                    if (max / 2.0 < Count[i])
                    {
                        dblePos2 = (Position[i] - Position[i + 1]) / (Count[i] - Count[i + 1]) * (max / 2.0 - Count[i]) + Position[i];
                        break;
                    }
                }
                double FWHM1 = Math.Abs(dblePos1 - dblePos2);
                #endregion

                #region FWHM2
                //ピークトップから外側にかけて
                //カウント/2以下になる境目を探す
                dblePos1 = 0.0;
                dblePos2 = 0.0;
                for (int i = iMax - 1; i >= 0; i--)
                {
                    if (max / 2.0 > Count[i])
                    {
                        dblePos1 = (Position[i] - Position[i + 1]) / (Count[i] - Count[i + 1]) * (max / 2.0 - Count[i]) + Position[i];
                        break;
                    } 
                }
                for (int i = iMax + 1; i <= Position.Length - 1; i++)
                {
                    if (max / 2.0 > Count[i])
                    {
                        dblePos2 = (Position[i] - Position[i - 1]) / (Count[i] - Count[i - 1]) * (max / 2.0 - Count[i]) + Position[i];
                        break;
                    }
                }
                double FWHM2 = Math.Abs(dblePos1 - dblePos2);
                #endregion

                return Tuple.Create<double, double>(FWHM1, FWHM2);
            }
            else
                return Tuple.Create<double, double>(0.0, 0.0);

        }

        static Tuple<double,double>FWHM(List<double> Position,List<double> Count,double background=0.0)
        {
            return FWHM(Position.ToArray(), Count.ToArray(), background);
        }

        static Tuple<double,double>FWHM_minus(List<double> Position,List<double> Count,double background=0.0)
        {
            double[] _count = new double[Count.Count];
            for (int i = 0; i < Count.Count; i++) _count[i] = -Count[i];

            return FWHM(Position.ToArray(), _count, background);
        }

        static double PeakPosition(double[] Position, double[] Count)
        {
            double xMax = 0.0;
            double yMax = 0.0;
            for (int i = 0; i < Position.Length; i++)
            {
                if (yMax < Count[i])
                {
                    yMax = Count[i];
                    xMax = Position[i];
                }
            }
            return xMax;
        }

        static double PeakCount(double[] Position, double[] Count)
        {
            double xMax = 0.0;
            double yMax = 0.0;
            for (int i = 0; i < Position.Length; i++)
            {
                if (yMax < Count[i])
                {
                    yMax = Count[i];
                    xMax = Position[i];
                }
            }
            return yMax;
        }

        /// <summary>
        /// PeakのX,Yを探す
        /// </summary>
        /// <param name="Position"></param>
        /// <param name="Count"></param>
        /// <returns>Position,Count</returns>
        public static Tuple<double, double> PeakPosCount(double[] Position, double[] Count)
        {
            double xMax = 0.0;
            double yMax = 0.0;
            for (int i = 0; i < Position.Length; i++)
            {
                if (yMax < Count[i])
                {
                    yMax = Count[i];
                    xMax = Position[i];
                }
            }
            return Tuple.Create(xMax, yMax);

        }
    }

    class Scan
    {

        #region Scan変数

        public bool IsScan;

        /// <summary>
        /// feedbackstage class
        /// </summary>
        Class_FeedbackStage fs;
        /// <summary>
        /// feedbackstage axis
        /// </summary>
        int Axis { get; set; }
        /// <summary>
        /// vme
        /// </summary>
        VmeControl vme;
        /// <summary>
        /// vme eh
        /// </summary>
        int EH { get; set; }
        /// <summary>
        /// vme counter ch in
        /// </summary>
        int ChIn { get; set; }
        /// <summary>
        /// vme counter ch out
        /// </summary>
        int ChOut { get; set; }

        /// <summary>
        /// スキャン開始位置[nm]
        /// </summary>
        int iScanStart { get; set; }
        /// <summary>
        /// スキャンピッチ[nm]
        /// </summary>
        int iScanPitch { get; set; }
        /// <summary>
        /// スキャン回数[回]
        /// </summary>
        public int iScanMax { get; private set; }
        /// <summary>
        /// カウンター露光時間[sec]
        /// </summary>
        double dExposTime { get; set; }
        /// <summary>
        /// ステージ移動後待機時間[sec]
        /// </summary>
        double dWaitTime { get; set; }
        /// <summary>
        /// 一点あたりの計測回数
        /// </summary>
        int iNoPt { get; set; }

        int iScanPlus { get; set; }
        int iPosition { get; set; }

        bool flagFinish = false;

        public enum ScanField { bright, dark }
        public ScanField scanField;

        const int sleep = 100;
        /// <summary>
        /// VMEcounter
        /// </summary>
        string[] strCount;
        /// <summary>
        /// scandata
        /// </summary>
        public ScanData scanData;
        /// <summary>
        /// scan何回目
        /// </summary>
        int iScanCount;

        int iScanPos;
        /// <summary>
        /// 保存先.csv
        /// </summary>
        public string filePath;
        /// <summary>
        /// コメント
        /// </summary>
        public string strComment;
        /// <summary>
        /// peak minus
        /// </summary>
        public bool minusPeak { get; set; }

        #endregion

        public Scan() { }

        public Scan(Class_FeedbackStage _fs, int _Axis, VmeControl _vme,
                    int _EH, int _ChIn, int _ChOut,
                    int _iScanStart, int _iScanPitch, int _iScanMax, int _iNoPt,
                    double _dExposTime, double _dWaitTime,
                    string _filePath, ScanField _field, string _strComment = "")
        {
            Initialize(_fs, _Axis, _vme, _EH, _ChIn, _ChOut,
                _iScanStart, _iScanPitch, _iScanMax, _iNoPt,
                _dExposTime, _dWaitTime, _filePath, _field, _strComment);
        }

        public void Initialize(Class_FeedbackStage _fs, int _Axis, VmeControl _vme,
                    int _EH, int _ChIn, int _ChOut,
                    int _iScanStart, int _iScanPitch, int _iScanMax,int _iNoPt,
                    double _dExposTime, double _dWaitTime,
                    string _filePath, ScanField _scanField, string _strComment = "")
        {
            fs = _fs;
            Axis = _Axis;
            vme = _vme;
            EH = _EH;
            ChIn = _ChIn;
            ChOut = _ChOut;

            iScanStart = _iScanStart;
            iScanPitch = _iScanPitch;
            iScanMax = _iScanMax;
            iNoPt = _iNoPt;

            dExposTime = _dExposTime;
            dWaitTime = _dWaitTime;

            filePath = _filePath;

            scanField = _scanField;

            strComment = _strComment;


            scanData = new ScanData(dExposTime);

            flagFinish = false;
        }

        public void TaskScan()
        {
            IsScan = true;

            iScanPos = 0;
            iScanCount = 0;
            iScanPlus = 1;

            //**繰り返し
            while (true)
            {

                setScanStatus("ステージ移動");
                //ステージ移動
                fs.MoveAbs(iScanStart + iScanPos * iScanPitch, Axis);

                setScanStatus("ステージ移動完了後待機");
                //設定時間待機
                Thread.Sleep((int)(dWaitTime * 1000));


                for (int i = 0; i < iNoPt; i++)
                {
                    #region カウンター初期化・消去
                    setScanStatus("カウンター初期化");
                    //カウンターInitialize
                    while (true)
                    {
                        if (this.vme.PutCounterInitialize(EH))
                            break;
                        Thread.Sleep(sleep);
                    }

                    setScanStatus("カウンター消去");
                    //カウンターClear
                    while (true)
                    {
                        if (this.vme.PutCounterClear(EH))
                            break;
                        Thread.Sleep(sleep);
                    }
                    #endregion

                    #region ステージ位置取得
                    //このときの座標をiPositionに格納
                    setScanStatus("ステージ座標取得");
                    string[] strPos = fs.Position(Axis);
                    iPosition = Convert.ToInt32(strPos[1]);
                    if (strPos[0].Substring(0, 1) == "C")
                    {
                        //CWlimit
                        flagFinish = true;
                    }
                    else if (strPos[0].Substring(0, 1) == "W")
                    {
                        //CCWlimit
                        flagFinish = true;
                    }
                    else if (strPos[0].Substring(0, 1) == "K")
                    {
                        //正常
                    }
                    #endregion

                    #region カウント

                    //カウント計測
                    setScanStatus("カウント計測");
                    while (true)
                    {
                        if (this.vme.PutCounterSec(EH, dExposTime))
                            break;
                        Thread.Sleep(sleep);
                    }

                    //計測時間待機（計測時間の1.1倍）
                    Thread.Sleep((int)(dExposTime * 1100));

                    //カウント取得
                    setScanStatus("カウント取得");
                    while (true)
                    {
                        strCount = this.vme.GetCounterQuery(EH);
                        if (strCount[0] == "inactive")
                            break;
                        Thread.Sleep(sleep);
                    }
                    #endregion

                    //データ格納
                    setScanStatus("データ格納");
                    scanData.Add(iScanCount, (double)iPosition, Convert.ToInt32(strCount[ChIn + 1]), Convert.ToInt32(strCount[ChOut + 1]), iScanPitch);
                    //グラフ用データ送信
                    setGraph(this.scanData);

                    iScanCount++;
                }
                #region 次へor終了処理
                //次へ
                setScanStatus("次のスキャンへ");
                //iScanPlusは早送りor巻戻し指示を反映しているので足す
                iScanPos += iScanPlus;
                iScanCount += (iScanPlus-1) * iNoPt;
                //iScanPlusを元に戻す
                iScanPlus = 1;
                //iScanCountの正常判別
                if (iScanPos > iScanMax || flagFinish)
                {
                    //iScanMaxを超えたときor終了指示がある場合 終了処理
                    break;
                }
                else if (iScanPos < 0)
                {
                    //巻き戻しでiScanCountが0以下の時，0に戻す
                    iScanPos = 0;
                    iScanCount = 0;
                }
                #endregion
            }


            #region 終了処理
            setScanStatus("終了処理");
            if (scanField == ScanField.bright)
            {
                var FWHM = minusPeak ? scanData.FWHM_NDmin : scanData.FWHM_ND;
                filePath += "(" + FWHM.Item1.ToString("F0") + "_" + FWHM.Item2.ToString("F0") + ").csv";
            }
            else
            {
                var FWHM = scanData.FWHM_NT;
                filePath += "(" + FWHM.Item1.ToString("F0") + "_" + FWHM.Item2.ToString("F0") + ").csv";

            }
            scanData.SaveData(filePath, strComment);

            IsScan = false;
            #endregion
            
            //初期座標に戻る
            setScanStatus("初期座標へ帰還");
            //ステージ移動
            fs.MoveAbs(iScanStart, Axis);
}

        public void Finish()
        {
            flagFinish = true;
        }

        #region GraphEvent

        public event ScanDataEventHandler graphSet;
        protected virtual void graphSetEvent(ScanData _scanData)
        {
            if (_scanData != null)
                graphSet(_scanData);
        }
        internal void setGraph(ScanData _scanData)
        {
            graphSetEvent(_scanData);
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
            scanStatusSetEvent(_ScanStatus);
        }

        #endregion
    }

    class ScanDataEventArgs : EventArgs
    {
        public ScanDataEventArgs(ScanData scanData)
        {
            this.scanData = scanData;
        }
        public ScanData scanData { get; set; }
    }
    public delegate void ScanDataEventHandler(ScanData scanData);


    class ScanStatusEventArgs : EventArgs
    {
        public ScanStatusEventArgs(string ScanStatus)
        {

        }
        public string _ScanStatus { get; set; }
    }
    public delegate void ScanStatusEventHandler(string ScanStatus);
}

