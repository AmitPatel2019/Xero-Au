using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmicApiModel
{
  public  class ReckonDocument
    {
        public int ReckonDocumentID { get; set; }
        public int AccountID { get; set; }
        public int ReckonFileID { get; set; }
        public int ScanInvoiceID { get; set; } //Ezzy InvoiceID
        public string ScanPdfPath { get; set; } //Ezzy Document Path
        public string TxnID { get; set; } //Reckon Txn ID of the Bill
        public string InvoiceType { get; set; } //Ezzy Document Type
        
        public string InvoiceNumber { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public string VendorName { get; set; }
        public decimal? InvoiceTotal { get; set; }
        public decimal? TaxTotal { get; set; }
        public DateTime? UploadedDate { get; set; }
        public string ReckonError { get; set; }
    }
}
