using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmicApiModel
{
    public class XeroProduct
    {
        public int ID { get; set; }
        public string VendorID { get; set; }
        public string AccountListID { get; set; }
        public string ProductCode { get; set; }
        public string DisplayNameField { get; set; }
        public string FullyQualifiedNameField { get; set; }
    }
}
