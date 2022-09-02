using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmicApiModel
{
   public  class ErrorLog
    {
        public int? ErrorLogID { get; set; }
        public int? AccountID { get; set; }
        public int? PlatformID { get; set; }
        public int? LoginID { get; set; }
        public int? ReckonFileID { get; set; }
        public int? QboConnectID { get; set; }
        public string Controller { get; set; }
        public string Method { get; set; }
        public string ErrorMessage { get; set; }
        public string ErrorStackTrace { get; set; }
        public DateTime? ErrorLogDate { get; set; } = DateTime.Now;


    }
}
