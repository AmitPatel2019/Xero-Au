using XeroAutomationService.EzzyService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace XeroAutomationService.CustomModel
{
    public class CBQboDocument
    {

        public string ABNnumber { get; set; }
        public decimal? chargeTotal { get; set; }
        public decimal? discountTotal { get; set; }
        public string docType { get; set; }
        public decimal? invoiceTotal { get; set; }
        public decimal? gstTotal { get; set; }
        public DateTime? invoiceDate = null;

        public DateTime? paymentDate = null;
        public string purchaseOrder = null;
        public string RefNumber = null;
        public decimal? subTotal { get; set; }
        public string supplierName = null;
        public string tag = null;




        public string DocumentName { get; set; }
        public string ScanPdfPath { get; set; }
        public string VendorName { get; set; }
        public int? QboVendorID { get; set; }

        public string AccountName { get; set; }
        public int AccountID { get; set; }

        public string InvoiceNumber { get; set; }
        public string InvoiceType { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public DateTime? UploadedDate { get; set; }
        public decimal? InvoiceTotal { get; set; }
        public decimal? TaxTotal { get; set; }
        


        public int QbDocumentID { get; set; }
        public int ScanInvoiceID { get; set; }
        public int QBOInvoiceID { get; set; }

        


    }


}