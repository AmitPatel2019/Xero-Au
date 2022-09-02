using Flexis.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmicApiHelper
{
    public static class CosmicLogger
    {
        public static string LogFilePath { get; set; }

        public static Logger SetLog()
        {
            // Get a logger instance 
            Logger _log = BaseLog.Instance.GetLogger(null);
            BaseLog.Instance.SetLogFile(LogFilePath);

            return _log;
        }
    }
}
