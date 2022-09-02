using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmicApiModel
{
  public class QBOEzzyAccount
    {
        public int? QBOEzzyID { get; set; }
        public int? AccountID { get; set; }
        public int? QboConnectID { get; set; }
        public string EzzyUserName { get; set; }
        public string EzzyPassword { get; set; }
        public string EzzyEmailAddress { get; set; }
        public DateTime? AddedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }

    }
}
