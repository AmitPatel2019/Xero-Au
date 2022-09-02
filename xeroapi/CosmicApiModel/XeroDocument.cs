using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmicApiModel
{
   public class XeroDocument
    {
        public int DocumentID { get; set; }
        public int AccountID { get; set; }
        public int XeroConnectID { get; set; }
        public string XeroInvoiceID { get; set; }
        public string XeroVendorID { get; set; }
        public string XeroVendorName { get; set; }
        public DateTime? XeroInvoiceDate { get; set; }
        public int? ScanInvoiceID { get; set; }
        public string ScanFile_Name { get; set; }
        public string ScanBlob_Url { get; set; }
        
        public string ScanABNNumber { get; set; }
        public string ScanRefNumber { get; set; }
        public string ScanDocType { get; set; }
        public decimal? ScanSubTotal { get; set; }
        public decimal? ScanChargeTotal { get; set; }
        public decimal? ScanDocumentTotal { get; set; }
        public string ScanTag { get; set; }
        public DateTime? ScanInvoiceDate { get; set; }
        public string ScanPurchaseOrder { get; set; }
        public string ScanVendorName { get; set; }
        public decimal? ScanInvoiceTotal { get; set; }
        public decimal? ScanTaxTotal { get; set; }
        public string ScanServiceStatus { get; set; }
        public string ScanServiceMessage { get; set; }
        public DateTime UploadedDate { get; set; }
        public string XeroError { get; set; }
        public string AspSessionID { get; set; }
        public bool? Approved { get; set; }
        public string ScanDocClassification { get; set; }
        public int? Status { get; set; }
        public List<XeroDocumentLine> DocumentLine { get; set; }
        public string ClientFileID { get; set; } //The Html File ID

        public bool? Deleted { get; set; }

        public int ErrorCount { get; set; }

        public int ApproveDocAs { get; set; }

    }
}
