//#define DEBUG

using System;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Windows.Threading;

class Class_FeedbackStage
{
    #region 宣言
    string stageName;

    SerialPort serialPort;

    char[] separator = { ',' };

    const int sleep = 100;

    public bool IsOpen { get { return serialPort.IsOpen; } }

    private readonly object syncObj = new object();

    /// <summary>
    /// speed nm/sec
    /// </summary>
    private const int speed = 1000000;

    #region status

    string[] stat;
    string[] Stat
    {
        get { return stat; }
        set
        {
            stat = value;
            Pos1 = value[0];
            Pos2 = value[1];
            Error = value[2];
            Axis1 = value[3];
            Axis2 = value[4];
            Ready = value[6];
        }
    }

    public string Pos1 { get; set; }
    public string Pos2 { get; set; }
    string error;
    public string Error
    {
        get { return error; }
        set
        {
            error = value + ": ";
            switch (value)
            {
                case "K":
                    error += "正常";
                    break;
                case "1":
                    error += "コマンドエラー";
                    break;
                case "2":
                    error += "スケールエラー";
                    break;
                case "3":
                    error += "リミット停止";
                    break;
                case "4":
                    error += "オーバースピードエラー";
                    break;
                case "5":
                    error += "オーバーフローエラー";
                    break;
                case "6":
                    error += "エマージェンシーエラー";
                    break;
                case "7":
                    error += "MN102エラー";
                    break;
                case "8":
                    error += "リミットエラー";
                    break;
                case "9":
                    error += "システムエラー";
                    break;
            }
        }
    }
    string axis1;
    public string Axis1
    {
        get { return axis1; }
        set
        {
            axis1 = value + ": ";
            switch (value)
            {
                case "K":
                    axis1 += "正常停止状態";
                    break;
                case "M":
                    axis1 += "動作中";
                    break;
                case "C":
                    axis1 += "CWリミット停止状態";
                    break;
                case "W":
                    axis1 += "CCWリミット停止状態";
                    break;
            }
        }
    }
    string axis2;
    public string Axis2
    {
        get { return axis2; }
        set
        {
            axis2 = value + ": ";
            switch (value)
            {
                case "K":
                    axis2 += "正常停止状態";
                    break;
                case "M":
                    axis2 += "正常動作状態";
                    break;
                case "C":
                    axis2 += "CWリミット停止状態";
                    break;
                case "W":
                    axis2 += "CCWリミット停止状態";
                    break;
                case "D":
                    axis2 += "AXIS Select1";
                    break;

            }
        }
    }
    string ready;
    public string Ready
    {
        get { return ready; }
        set
        {
            ready = value + ": ";
            switch (value)
            {
                case "B":
                    ready += "ビジー状態 位置決め未完了状態";
                    break;
                case "R":
                    ready += "レディ状態 位置決め完了状態";
                    break;
            }
        }
    }
    #endregion

    #endregion

    ~Class_FeedbackStage()
    {
        serialPort.Close();
        serialPort = null;
    }

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="_sleep">モニタのスリープ時間</param>
    public Class_FeedbackStage(string _stageName="")
    {
        stageName = _stageName + ": ";

        serialPort = new SerialPort();
        serialPort.DataReceived += new SerialDataReceivedEventHandler(serialPort_DataReceived);

        
        
    }

    //データ受信
    void serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        //受信データを読み取り
        string rData = serialPort.ReadLine();
        string[] rDataArray = rData.Split(separator);

        if (rDataArray.Length == 7)
        {
            //Q:の返値の時
            Stat = (string[])rDataArray.Clone();
            setStat();
        }
        logSet(rData);
    }

    #region openclose

    public bool Open(string _portName)
    {
        try
        {
            serialPort.PortName = _portName;
            serialPort.BaudRate = 9600;
            serialPort.DataBits = 8;
            serialPort.Parity = Parity.None;
            serialPort.StopBits = StopBits.One;

            serialPort.NewLine = "\r\n";
            serialPort.Open();

            logSet("Connect " + serialPort.PortName);

            Send("D:1F" + speed.ToString());
            Send("D:2F" + speed.ToString());

            return true;
        }
        catch (Exception ex)
        {
            System.Windows.Forms.MessageBox.Show(ex.Message);
            return false;
        }
    }

    public bool Close()
    {
        try
        {
            serialPort.Close();
            serialPort.Dispose();

            logSet(serialPort.PortName + " Disconnect");
            return true;
        }
        catch (Exception ex)
        {
            setLog(ex.Message);
            return false;
        }
    }

    #endregion

    #region コマンド送受信

    /// <summary>
    /// 送信
    /// </summary>
    /// <param name="command"></param>
    public void Send(string command)
    {
        try
        {
            lock (syncObj)
            {
                serialPort.WriteLine(command);
            }
            setLog("Sent: " + command);
        }
        catch
        {
            setLog("SendingError: not connecting to feedback stage");
        }
    }
    
    /// <summary>
    /// 全ステータスの要求 Q:
    /// </summary>
    /// <returns>1軸座標,2軸座標,エラー状態の表示,1軸状態表示,2軸状態表示,システム予約,ステージ位置決め状態</returns>
    public void Status()
    {
        string Command = "Q:";
        Send(Command);
    }

    /// <summary>
    /// ポジション
    /// </summary>
    /// <param name="axis"></param>
    /// <returns>{状態,Position}</returns>
    public string[] Position(int axis = 1)
    {
        Monitor(axis);

        //status==M以外になったら，そのときの状態とポジションを返す
        string[] str = axis == 1 ? new string[] { Axis1, Pos1 } : new string[] { Axis2, Pos2 };
        return str;
    }

    public void Monitor(int axis = 1)
    {
        //status=Mのとき，モニタし続ける
        while (true)
        {
            Status();
            System.Threading.Thread.Sleep(sleep);
            if (Stat[2 + axis] == "K")
                break;
        }
    }

    /// <summary>
    /// Absolute Drive
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="axis"></param>
    /// <returns>{position,status(K,C,W)}</returns>
    public string[] MoveAbs(int pos, int axis = 1)
    {
        string sign = pos < 0 ? "-" : "+";
        //A:1+P0000000
        string command = "A:" + axis.ToString() + sign + "P" + Math.Abs(pos).ToString();
        Send(command);

        command = "G";
        Send(command);

        return Position(axis);
    }

    /// <summary>
    /// Increse Drive
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="axis"></param>
    /// <returns>{position,status(K,C,W)}</returns>
    public string[] MoveInc(int pos, int axis = 1)
    {
        string sign = pos < 0 ? "-" : "+";
        //M:1+P0000000
        string command = "M:" + axis.ToString() + sign + "P" + Math.Abs(pos).ToString();
        Send(command);

        command = "G";
        Send(command);

        return Position(axis);
    }

    /// <summary>
    /// 停止
    /// </summary>
    /// <param name="axis">軸</param>
    public void Stop(int axis = 1)
    {
        string command = "L:" + axis.ToString();
        Send(command);
    }

    /// <summary>
    /// スピード設定
    /// </summary>
    /// <param name="speed">速度</param>
    /// <param name="axis">軸</param>
    public void Speed(int speed,int axis=1)
    {
        string command = "D:" + axis.ToString() + "F" + speed.ToString();
        Send(command);
    }

    #endregion

    #region 受け渡し処理

    #region log
    public event LogEventHandler logSet;
    protected virtual void logSetEvent(string log)
    {
        if (logSet != null)
        {
            logSet(log);
        }
    }

    internal void setLog(string _log)
    {
        string log = stageName + _log;
        logSetEvent(log);
    }
    #endregion

    #region Status
    public event StatusEventHandler statSet;
    protected virtual void statSetEvent(string Pos1, string Pos2,string Error,string Axis1,string Axis2,string  Ready)
    {
        if (statSet != null)
            statSet(Pos1, Pos2, Error, Axis1, Axis2, Ready);
    }
    internal void setStat()
    {
        statSetEvent(Pos1, Pos2, Error, Axis1, Axis2, Ready);
    }
    #endregion

    #endregion

}


#region Event

class StatusEventArgs : EventArgs
{
    public StatusEventArgs(string Pos1, string Pos,string Error,string Axis1,string Axis2,string  Ready)
    {
        this.Pos1 = Pos1;
        this.Pos2 = Pos2;
        this.Error = Error;
        this.Axis1 = Axis1;
        this.Axis2 = Axis2;
        this.Ready = Ready;
    }

    public string Pos1 { get; set; }
    public string Pos2 { get; set; }
    public string Error { get; set; }
    public string Axis1 { get; set; }
    public string Axis2 { get; set; }
    public string Ready { get; set; }
}
public delegate void StatusEventHandler(string Pos1, string Pos2, string Error, string Axis1, string Axis2, string Ready);


#endregion



