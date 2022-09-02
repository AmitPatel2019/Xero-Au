using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmicApiModel
{
   public class ReckonDesktopMaster
    {
        public int ReckonFileID { get; set; }
        public int AccountID { get; set; }
        public string CompanyName { get; set; }
        public string LegalCompanyName { get; set; }
        public bool? IsSampleCompany { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Addr1 { get; set; }
        public string Addr2 { get; set; }
        public string Addr3 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }

        public string FilePath { get; set; }
        public DateTime? AddedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public int? AdddeBy { get; set; }
        public int? UpdatedBy { get; set; }
        public bool Select { get; set; }
        public bool? IsUsingJobCosting { get; set; }

        public bool? IsAccess { get; set; }
        


    }

    public class ReckonVendDefault
    {
        public bool? Select { get; set; } //used at client side
        public int ID { get; set; }
        public int AccountID { get; set; }
        public int ReckonFileID { get; set; }

        public string VendorListID { get; set; }
        public string VendorName { get; set; }
        

        public string DefaultExpenseListID { get; set; }
        public string DefaultExpenseAccount { get; set; } 

        public string DefaultItemListID { get; set; }
        public string DefaultItem { get; set; }
        public int? AddedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime? AddedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}
