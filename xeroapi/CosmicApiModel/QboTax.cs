using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmicApiModel
{
  public  class QboTax
    {
        public int ID { get; set; }
        public int AccountID { get; set; }
        public int QboConnectID { get; set; }
        public string TaxID { get; set; }
        public string TaxName { get; set; }

    }
}
