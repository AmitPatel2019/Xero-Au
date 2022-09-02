using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmicApiModel
{
  public  class XeroChartOfAccount
    {
        public int ID { get; set; }
        public int AccountID { get; set; }
        public int XeroConnectID { get; set; }
        public string XeroAccountID { get; set; }
        public string AccountCode { get; set; }

        public string FullyQualifiedNameField { get; set; }
        public string AccountSubTypeField { get; set; }
    }
}
