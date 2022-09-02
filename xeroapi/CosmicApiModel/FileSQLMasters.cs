using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmicApiModel
{
   public class FileSQLMasters
    {
        public int SqlID { get; set; }
        public int AccountID { get; set; }
        public string SqlServerName { get; set; }
        public string SqlDataBaseName { get; set; }
        public bool SqlAuthenticationMode { get; set; }
        public string SqlUserName { get; set; }
        public string SqlPassword { get; set; }
        public DateTime? AddedDte { get; set; }
        public DateTime? UpdatedDte { get; set; }

    }
}
