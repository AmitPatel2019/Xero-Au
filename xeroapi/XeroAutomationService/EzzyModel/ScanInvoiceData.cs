using XeroAutomationService.EzzyService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XeroAutomationService.EzzyModel
{
    public class ScanInvoiceData
    {
        public InvoiceBlocksSS invoiceBlocksSS { get; set; } = new InvoiceBlocksSS();
        public FormDataSS formData { get; set; } = new FormDataSS();
        public FormDataSS formDataSS2 { get; set; } = new FormDataSS();

        public InvoiceDetailsSS invoiceDetailsSS { get; set; } = new InvoiceDetailsSS();

        public DocumentClassificationSS documentClassificationSS { get; set; } = new DocumentClassificationSS();

        public string ScanServiceStatus { get; set; }

        public string ScanServiceMessage { get; set; }


    }
}
