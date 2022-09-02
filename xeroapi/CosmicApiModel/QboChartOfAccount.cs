using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmicApiModel
{
   public class QboChartOfAccount
    {
        public int ID { get; set; }
        public int AccountID { get; set; }
        public int QboConnectID { get; set; }
        public string QboAccountID { get; set; }

        public string FullyQualifiedNameField { get; set; }
        public string AccountSubTypeField { get; set; }
     
    }
}
