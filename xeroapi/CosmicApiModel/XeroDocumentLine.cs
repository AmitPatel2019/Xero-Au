using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmicApiModel
{
  public  class XeroDocumentLine
    {
        public int DocumentLineID { get; set; }
        public int DocumentID { get; set; }
        public string XeroAccountID { get; set; }
        public string XeroAccountName { get; set; }

        public string XeroClassID { get; set; }
        public string XeroClassName { get; set; }
        public string XeroJobID { get; set; }
        public string XeroJobName { get; set; }

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
        public DateTime? ScanInvoiceDate { get; set; }
        //Additional Fields Used in for creating Bill
        public bool? SelectToBill { get; set; }
        public string XeroVendorName { get; set; }
        public string XeroVendorID { get; set; }
        public string ScanABNNumber { get; set; }
        public decimal? ScanInvoiceTotal { get; set; }
        public decimal? ScanTaxTotal { get; set; }
        public string ScanFile_Name { get; set; }
        public decimal? ScanSubTotal { get; set; }
        public string ScanRefNumber { get; set; }
        public string ScanDocClassification { get; set; }
        public string ScanDocType { get; set; }

        public string ScanBlob_Url { get; set; }
        public string AccountCode { get; set; }
        public int? Status { get; set; }
        public string XeroInvoiceID { get; set; }
        public string BillStatus { get; set; }

        public int ApproveDocAs { get; set; }

        public bool fromEmail { get; set; }

        public int Duplicate { get; set; }
    }
}
