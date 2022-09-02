using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmicApiModel
{
   public class ReckonVendorDefault
    {
        public int ID { get; set; }
        public int AccountID { get; set; }
        public int ReckonFileID { get; set; }
        public string VendorListID { get; set; }
        public string DefaultExpenseAccount { get; set; }
        public string DefaultItem { get; set; }
        public int? AddedBy { get; set; }
        public DateTime? AddedDate { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
       
    }
}
