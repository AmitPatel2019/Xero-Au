using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmicApiModel
{
    public class QboVendor
    {

        public int ID { get; set; }
        public int AccountID { get; set; }
        public int QboConnectID { get; set; }
        public string QboAccountID { get; set; }
        public string QboVendorID { get; set; }
        public int? AddedBy { get; set; }
        public DateTime? AddedDate { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }

        public string DisplayNameField { get; set; }
        public string CityField { get; set; }
        public string CountrySubDivisionCodeField { get; set; }
        public string PostalCodeField { get; set; }
        public string CountryField { get; set; }

        public string PrimaryPhone { get; set; }
        public string PrimaryEmailAddr { get; set; }
        public string TaxIdentifier { get; set; }




        public bool Select { get; set; }



    }
}
