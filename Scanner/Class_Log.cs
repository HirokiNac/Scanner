using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class LogEventArgs : EventArgs
{
    public LogEventArgs(string Log)
    {
        this.Log = Log;
    }

    private string log;

    public string Log
    {
        get { return log; }
        set { log = value; }
    }
}
public delegate void LogEventHandler(string log);
