using CosmicCoreApi.EzzyService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CosmicCoreApi.CustomModel
{

    public class CBDocument
    {
        public bool? SelectToBill { get; set; }
        public int Index { get; set; }
        public string Title { get; set; }
        public string FilePath { get; set; }
        public int Status { get; set; } ///=> 1 Inprogress => 2 Completed
        public int InvoiceID { get; set; }
        public InvoiceBlocksSS EzzyInvoiceBlocksSS { get; set; }
        public DocumentClassificationSS EzzyDocumentClassificationSS { get; set; }


        public List<CBDocumentLine> CBLine { get; set; }

        public string ScanServiceStatus { get; set; }
        public string ScanServiceStatusMessage { get; set; }

        public bool? CreateBillStatus { get; set; }

        public string CB_VendorListID { get; set; }
        public string CB_VendorInvoiceNo { get; set; }
        public string CB_TxnID { get; set; }
        public string CB_FilePath { get; set; }
        public string CB_FileName { get; set; }
        public documentsubtypes CB_DocSubType { get; set; }
        public string CB_DocType { get; set; }
        public string CB_JobName { get; set; }
        public string CB_PurchaseOrderNo { get; set; }
        public bool CB_JobRequiredToBill { get; set; } = false;

        public int ReckonDocumentID { get; set; }
        public int QboDocumentID { get; set; }
        public int XeroDocumentID { get; set; }
        public int? ScanInvoiceID { get; set; }
        public string InvoiceNumber { get; set; }
        
        public string Error { get; set; }


        //public string CB_VednorOK { get; set; }
        //public string CB_JobOK { get; set; }
        //public string CB_ExpAccountOK { get; set; }
    }
}