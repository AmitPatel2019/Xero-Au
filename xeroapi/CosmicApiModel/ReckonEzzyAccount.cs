using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmicApiModel
{
  public class ReckonEzzyAccount
    {
        public int? RCEzzyID { get; set; }
        public int? AccountID { get; set; }
        public int? ReckonFileID { get; set; }
        public string EzzyEmailAddress { get; set; }
        
        public string EzzyUserName { get; set; }
        public string EzzyPassword { get; set; }
        public DateTime? AddedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}
