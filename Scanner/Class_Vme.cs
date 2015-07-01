#undef DEBUG

using System;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

class VmeControl
{
    public readonly int[] nCh = { 23, 47, 39, 31 };

    TcpClient client;
    NetworkStream stream;

    public bool IsConnect = false;

    //スレッドロック用
    private readonly object syncObj = new object();

    public VmeControl() { }

    public bool Connect()
    {
        try
        {
            string vmeServer = "bl29aws.spring8.or.jp";
            int port = 10101;
            client = new TcpClient(vmeServer, port);
            stream = client.GetStream();
            
            IsConnect = true;
            setLog("Connected");
            return true;
        }
        catch (ArgumentNullException ex)
        {
            setLog("ArgumentNullException: " + ex);
            IsConnect = false;
            return false;
        }
        catch (SocketException ex)
        {
            setLog("SocketException: " + ex);
            IsConnect = false;
            return false;
        }
    }

    #region motor

    /// <summary>
    /// モータの状態を取得
    /// </summary>
    /// <param name="EH">ハッチ番号</param>
    /// <param name="axis">モータ番号</param>
    /// <returns>状態(rotating|inactive|fail), パルス数</returns>
    public string[] GetMotorQuery(int EH, int axis)
    {
        string message = "get/bl_29in_st" + EH.ToString() + "_motor_" + axis.ToString() + "/query";

        //PutMessage(message);

        string resData = GetResponse(message);

        string[] returnCode = new string[2];
        Regex pulseCount = new Regex(@"(\w+)_(-?\d+)pulse");
        Match m = pulseCount.Match(resData);
        if (m.Groups[1].Value == "rotating")
        {
            returnCode[0] = m.Groups[1].Value;
            returnCode[1] = m.Groups[2].Value;
        }
        else if (m.Groups[1].Value == "inactive")
        {
            returnCode[0] = m.Groups[1].Value;
            returnCode[1] = m.Groups[2].Value;
        }
        else
        {
            returnCode[0] = "fail";
            returnCode[1] = "0";
        }

        return returnCode;
    }

    /// <summary>
    /// Motorの動作速度[pulse/sec]を設定
    /// </summary>
    /// <param name="EH">ハッチ番号</param>
    /// <param name="axis">Motor番号</param>
    /// <param name="speed">動作速度[pulse/sec]</param>
    /// <returns></returns>
    public bool PutMotorSpeed(int EH, int axis, int speed)
    {
        string message = "put/bl_29in_st" + EH.ToString() + "_motor_" + axis.ToString()
                            + "_speed/" + speed.ToString() + "pps";

        //PutMessage(message);

        return GetBoolResponse(message);
    }

    /// <summary>
    /// Motorの現在位置のパルス値を、指定したパルス値に設定
    /// </summary>
    /// <param name="EH">ハッチ番号</param>
    /// <param name="axis">Motor番号</param>
    /// <returns></returns>
    public bool PutMotorPreset(int EH, int axis, int pulse)
    {
        string message = "put/bl_29in_st" + EH.ToString() + "_motor_" + axis.ToString() + "_preset/" + pulse.ToString() + "pulse";

        //PutMessage(message);

        return GetBoolResponse(message);
    }

    /// <summary>
    /// MotorのUpperLimitを設定
    /// </summary>
    /// <param name="EH">ハッチ番号</param>
    /// <param name="axis">Motor番号</param>
    /// <param name="limit">リミット値</param>
    /// <returns></returns>
    public bool PutMotorUpperLimit(int EH, int axis, int limit)
    {
        string message = "put/bl_29in_st" + EH.ToString() + "_motor_" + axis.ToString()
        + "_upperlimit/" + limit.ToString() + "pulse";

        //PutMessage(message);

        return GetBoolResponse(message);
    }

    /// <summary>
    /// MotorのLowerLimitを設定
    /// </summary>
    /// <param name="EH">ハッチ番号</param>
    /// <param name="axis">Motor番号</param>
    /// <param name="limit">リミット値</param>
    /// <returns></returns>
    public bool PutMotorLowerLimit(int EH, int axis, int limit)
    {
        string message = "put/bl_29in_st" + EH.ToString() + "_motor_" + axis.ToString()
        + "_lowerlimit/" + limit.ToString() + "pulse";

        //PutMessage(message);

        return GetBoolResponse(message);
    }

    /// <summary>
    /// Motorを絶対パルス指定で動作
    /// </summary>
    /// <param name="EH">ハッチ番号</param>
    /// <param name="axis">Motor番号</param>
    /// <param name="pulse">絶対パルス値</param>
    /// <returns></returns>
    public bool PutMotorPulse(int EH, int axis, int pulse)
    {
        string message = "put/bl_29in_st" + EH.ToString() + "_motor_" + axis.ToString() + "/" + pulse.ToString() + "pulse";

        //PutMessage(message);

        return GetBoolResponse(message);
    }

    /// <summary>
    /// Motorを停止(減速しながら停止)
    /// </summary>
    /// <param name="EH">ハッチ番号</param>
    /// <param name="axis">Motor番号</param>
    /// <returns></returns>
    public bool PutMotorStop(int EH, int axis)
    {
        string message = "put/bl_29in_st" + EH.ToString() + "_motor_" + axis.ToString() + "/stop";

        //PutMessage(message);

        return GetBoolResponse(message);
    }

    /// <summary>
    /// Motorを緊急停止(瞬間的に停止 ※脱調の恐れあり)
    /// </summary>
    /// <param name="EH">ハッチ番号</param>
    /// <param name="axis">Motor番号</param>
    /// <returns></returns>
    public bool PutMotorEmergencyStop(int EH, int axis)
    {
        string message = "get/bl_29in_st" + EH.ToString() + "_motor_" + axis.ToString() + "/emstop";

        //PutMessage(message);

        return GetBoolResponse(message);
    }

    #endregion

    #region Counter


    public double[] CountOnce(int EH, double dExposTime)
    {
        int sleep = 100;

        //カウンターInitialize
        while (true)
        {

            if (PutCounterInitialize(EH))
                break;
            Thread.Sleep(sleep);
        }

        //カウンターClear
        while (true)
        {

            if (PutCounterClear(EH))
                break;
            Thread.Sleep(sleep);
        }


        //カウント計測
        while (true)
        {

            if (PutCounterSec(EH, dExposTime))
                break;
            Thread.Sleep(sleep);
        }

        //計測時間待機
        Thread.Sleep((int)(dExposTime * 1000));

        string[] strCount;
        //カウント取得
        while (true)
        {
            strCount = GetCounterQuery(EH);
            if (strCount[0] == "inactive")
                break;
            Thread.Sleep(sleep);
        }

        double[] dCount = new double[strCount.Length - 1];
        //Count格納
        for (int i = 0; i < strCount.Length - 1; i++)
        {
            dCount[i] = Convert.ToDouble(strCount[i + 1]);
        }

        return dCount;
    }

    /// <summary>
    /// Counterを初期化
    /// </summary>
    /// <param name="EH">ハッチ番号</param>
    /// <returns></returns>
    public bool PutCounterInitialize(int EH)
    {
        string message = "put/bl_29in_st" + EH.ToString() + "_counter_1/init";

        //PutMessage(message);

        return GetBoolResponse(message);
    }

    /// <summary>
    /// Counterのバッファをクリア
    /// </summary>
    /// <param name="EH">ハッチ番号</param>
    /// <returns></returns>
    public bool PutCounterClear(int EH)
    {
        string message = "put/bl_29in_st" + EH.ToString() + "_counter_1/clear";

        //PutMessage(message);

        return GetBoolResponse(message);
    }

    /// <summary>
    /// Coutnerを指定秒[sec]カウントさせる
    /// </summary>
    /// <param name="EH">ハッチ番号</param>
    /// <param name="sec">秒数(最小0.01sec)</param>
    /// <returns></returns>
    public bool PutCounterSec(int EH, double sec)
    {
        string message;
        if (sec < 0.01)
        {
            message = "put/bl_29in_st" + EH.ToString() + "_counter_1/0.01sec";
        }
        else
        {
            message = "put/bl_29in_st" + EH.ToString() + "_counter_1/" + sec.ToString("f1") + "sec";
        }

        //PutMessage(message);

        return GetBoolResponse(message);
    }

    /// <summary>
    /// Coutnerを指定秒[sec]カウントさせる(decimal)
    /// </summary>
    /// <param name="EH">ハッチ番号</param>
    /// <param name="sec">秒数(最小0.01sec)</param>
    /// <returns></returns>
    public bool PutCounterSec(int EH, decimal sec)
    {
        return PutCounterSec(EH, (double)sec);
    }

    /// <summary>
    /// Counterの状態を読み出す
    /// (戻り値：配列の先頭は状態(inactive|counting)、後ろに8個データ)
    /// </summary>
    /// <param name="EH">ハッチ番号</param>
    /// <returns></returns>
    public string[] GetCounterQuery(int EH)
    {
        string message = "get/bl_29in_st" + EH.ToString() + "_counter_1/query";

        //PutMessage(message);

#if !DEBUG
        string responseData = GetResponse(message);

#elif DEBUG
        Random rnd = new Random();
        int ioChamber = 1000 - 10 * rnd.Next(1, 10);
        int APD = 444 - 444 * rnd.Next(10, 100) / 100;
        string responseData =
            "inactive_" + ioChamber.ToString() + "count_" + APD.ToString()
            + "count_44count_44count_44count_44count_44count_44count";
#endif

        string[] query = new string[9];
        Regex pulseCount = new Regex(@"(\w+)_(\d+)count_(\d+)count_(\d+)count_(\d+)count_(\d+)count_(\d+)count_(\d+)count_(\d+)count");

        Match m = pulseCount.Match(responseData);

        for (int i = 0; i < 9; i++)
        {
            query[i] = m.Groups[i + 1].Value;
        }


        return query;
    }

    #endregion

    #region Slit

    /// <summary>
    /// TCスリット状態取得
    /// </summary>
    /// <returns>
    /// 現在回転している軸
    /// ok/upper/lower/right/left
    /// </returns>
    public string GetTCSlitQuery()
    {
        string message = "get/bl_29in_tc1_slit_1/query";

        string responce = GetResponse(message);

        return responce.Split('/')[3];
    }


    public enum aperture { width, height }
    public enum position { horizontal, vertical }

    /// <summary>
    /// TCスリットの開口を取得
    /// </summary>
    /// <param name="_aperture">方向</param>
    /// <returns></returns>
    public string GetTCSlitAperture(aperture _aperture)
    {
        string message = "get/bl_29in_tc1_slit_1_" + _aperture + "/aperture";

        return GetResponse(message).Split('/')[3];
    }

    /// <summary>
    /// TCスリットの位置を取得
    /// </summary>
    /// <param name="_direction">方向</param>
    /// <returns></returns>
    public string GetTCSlitPosition(position _direction)
    {
        string message  = "get/bl_29in_tc1_slit_1_" + _direction + "/position";

        return GetResponse(message).Split('/')[3];
    }

    /// <summary>
    /// TCスリットの開口を設定
    /// </summary>
    /// <param name="_aperture">方向</param>
    /// <param name="size">開口[mm]</param>
    /// <returns></returns>
    public bool PutTcSlitAperture(aperture _aperture, double size)
    {
        string message = "put/bl_29in_tc1_slit_1_" + _aperture + "/" + size.ToString("F2") + "mm";

        //PutMessage(message);

        return GetBoolResponse(message);
    }

    /// <summary>
    /// TCスリットの位置を設定
    /// </summary>
    /// <param name="_direction">方向</param>
    /// <param name="position">位置[mm]</param>
    /// <returns></returns>
    public bool PutTcSlitPosition(position _direction, double position)
    {
        string message = "put/bl_29in_tc1_slit_1_" + _direction + "/" + position.ToString("F2") + "mm";

        //PutMessage(message);

        return GetBoolResponse(message);
    }

    public bool PutTcSlitAperture(ref aperture _aperture, ref double size)
    {
        string message = "put/bl_29in_tc1_slit_1_" + _aperture + "/" + size.ToString("F2") + "mm";

        //PutMessage(message);

        return GetBoolResponse(message);
    }

    #endregion

    #region VME操作汎用ルーチン (内部コール用)

    private void PutMessage(string message)
    {
        try
        {
            byte[] data = Encoding.ASCII.GetBytes(message);
            stream.Write(data, 0, data.Length);
            setLog("Sent: " + message);
        }
        catch
        {
#if DEBUG
            setLog("[DEBUG] Sent: " + message);
#else
            setLog("SendingError: not connecting to VME Server");
#endif
        }
    }

    private string GetResponse()
    {
        string responseData = "";

        try
        {
            byte[] data = new Byte[512];
            int bytes = stream.Read(data, 0, data.Length);
            responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
            setLog("Received: " + responseData);
        }
        catch
        {
#if DEBUG
            setLog("[DEBUG] Recieved: none");
            responseData = "ok";
#else
                setLog("RecievingError: not connecting to VME Server");
#endif
        }
        return responseData;
    }

    private bool GetBoolResponse()
    {
        string responseData = GetResponse();

        bool returnCode;
        if (responseData.Contains("ok")) returnCode = true;
        else if (responseData.Contains("busy")) returnCode = false;
        else if (responseData.Contains("fail")) returnCode = false;
        else
        {
            setLog("Error：Returned unknown code in VME.getBoolResponse()");
            returnCode = false;
        }

        return returnCode;
    }

    private string GetResponse(string message)
    {
        lock(syncObj)
        {
            PutMessage(message);
            return GetResponse();
        }
    }

    private bool GetBoolResponse(string message)
    {
        lock (syncObj)
        {
            PutMessage(message);
            return GetBoolResponse();
        }
    }

    

    #endregion

    #region Logの受け渡し処理

    public event LogEventHandler logSet;
    protected virtual void logSetEvent(string log)
    {
        if (logSet != null)
        {
            logSet(log);
        }
    }

    internal void setLog(string log)
    {
        logSetEvent(log);
    }

    #endregion
}
