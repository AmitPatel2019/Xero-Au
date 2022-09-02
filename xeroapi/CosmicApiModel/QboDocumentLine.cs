using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmicApiModel
{
    public class QboDocumentLine
    {
        public int DocumentLineID { get; set; }
        public int DocumentID { get; set; }
        public string QboAccountID { get; set; }
        public string QboAccountName { get; set; }

        public string QboClassID { get; set; }
        public string QboClassName { get; set; }
        public string QboJobID { get; set; }
        public string QboJobName { get; set; }

        public string ScanDescription { get; set; }
        public string ScanArticle_Code { get; set; }
        public string ScanGL_Code { get; set; }
        public decimal? ScanGST { get; set; }
        public string ScanTax_Code { get; set; }
        public string ScanTracking { get; set; }
        public string ScanUnit_Measure { get; set; }
        public decimal? ScanUnit_Price { get; set; }
        public decimal? Scan_Quantity { get; set; }
        public decimal? Scan_Total { get; set; }

        //Additional Fields Used in for creating Bill
        public bool? SelectToBill { get; set; }
        public string QboVendorName { get; set; }
        public string QboVendorID { get; set; }
        public string ScanABNNumber { get; set; }
        public decimal? ScanInvoiceTotal { get; set; }
        public decimal? ScanTaxTotal { get; set; }
        public string ScanFile_Name { get; set; }
        public decimal? ScanSubTotal { get; set; }
        public string ScanRefNumber { get; set; }
        public string ScanDocClassification { get; set; }
        public string ScanDocType { get; set; }

        public string ScanBlob_Url { get; set; }

        public bool fromEmail { get; set; }


    }
}
