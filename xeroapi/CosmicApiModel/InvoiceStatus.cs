using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmicApiModel
{
    public class InvoiceStatus
    {
        public string InvoiceID { get; set; }

        //1 means true
        public decimal proccessStatus { get; set; }
        public string accountID { get; set; }
    }
}
