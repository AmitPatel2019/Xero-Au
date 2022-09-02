using XeroAutomationService.EzzyService;
using Flexis.Log;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using XeroAutomationService.CustomModel;
using CosmicApiHelper;
using CosmicApiModel;
using XeroAutomationService;

namespace XeroAutomationService
{
    public class EzzyContext
    {
        EzzyServiceClient _ezzyClient = new EzzyServiceClient();
        XeroContext _xcContext = new XeroContext();

        Logger _log = null;

        private static bool RemoteCertificateValidate(

         object sender, System.Security.Cryptography.X509Certificates.X509Certificate cert,

          System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors error)
        {
            // trust any certificate!!!

            System.Console.WriteLine("Warning, trust any certificate");

            return true;
        }

       

        internal async Task<CBDocument> GetInvoiceDetail(CBDocument cbDocument,int AccountID,int ReckonFileID)
        {
            _log = CosmicLogger.SetLog();
            try
            {

                var scanningCred = _xcContext.GetRCScanningCredential(AccountID, ReckonFileID);
                if (scanningCred == null)
                {
                    if (scanningCred.Data == null)
                    {
                        cbDocument.Error = "Failed to get credentials to scan document";
                    }
                }

                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

                int rc = _ezzyClient.Login(scanningCred.Data.EzzyUserName, scanningCred.Data.EzzyPassword);  // rc==1 indicates success


                while (true)
                {

                    WorkflowStatusSS ws = _ezzyClient.workflowStatus(cbDocument.InvoiceID);

                    if (ws.complete)
                    {

                        if (ws.children.Count() > 0)
                        {
                            //document was autoseperated. iterate to get all child docs.
                            int id = ws.children[0];
                            InvoiceDetailsSS dd2 = await _ezzyClient.getInvoiceDetailsAsync(id);
                            if (dd2 != null)
                            {
                                if (dd2.invForm != null)
                                {
                                    string s = dd2.invForm.file_name; //the filename will be <parentdocid>.pdf
                                    cbDocument.CB_FilePath = dd2.invForm.blob_url;
                                }
                            }
                            //dd2.invForm.
                        }
                        break; //workflow complete
                    }
                    Console.WriteLine("status=" + ws.state);
                    Thread.Sleep(1000);
                }

                //now less get the data

                cbDocument.EzzyInvoiceBlocksSS = await _ezzyClient.getInvoiceHeaderBlocksAsync(cbDocument.InvoiceID);
                cbDocument.ScanServiceStatus = cbDocument.EzzyInvoiceBlocksSS != null ? cbDocument.EzzyInvoiceBlocksSS.service_status.status.ToString() : string.Empty;
                if (cbDocument.EzzyInvoiceBlocksSS != null)
                {
                    if (cbDocument.EzzyInvoiceBlocksSS.invoiceForm != null)
                    {
                        cbDocument.CB_VendorListID = cbDocument.EzzyInvoiceBlocksSS.invoiceForm.supplierName;
                    }
                }

                InvoiceDetailsSS invDetails = await _ezzyClient.getInvoiceDetailsAsync(cbDocument.InvoiceID);
                if (invDetails != null)
                {
                    if (invDetails.invForm != null)
                    {
                        cbDocument.CB_FilePath = invDetails.invForm.blob_url;
                        cbDocument.CB_FileName = invDetails.invForm.file_name;
                    }
                }

                DocumentClassificationSS docClassification = await _ezzyClient.getDocumentClassificationAsync(cbDocument.InvoiceID);
                if (docClassification != null)
                {
                    cbDocument.EzzyDocumentClassificationSS = docClassification;
                    cbDocument.CB_DocSubType = docClassification.doc_subtype;
                }
            }
            catch (Exception ex)
            {
                cbDocument.Error = ex.Message;
                _log.Error(ex);
            }

            return cbDocument;
        }

        internal async Task<XeroDocument> GetScanDetailXero(XeroDocument cbDocument)
        {


            _log = CosmicLogger.SetLog();
            try
            {

                var scanningCred = _xcContext.GetXeroScanningCredential(cbDocument.AccountID,cbDocument.XeroConnectID);
                if (scanningCred == null)
                {
                    if (scanningCred.Data == null)
                    {
                        cbDocument.XeroError = "Failed to get credentials to scan document";
                    }
                }

                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

                int rc = _ezzyClient.Login(scanningCred.Data.EzzyUserName, scanningCred.Data.EzzyPassword);  // rc==1 indicates success

                int counter = 0;

                while (true)
                {

                    WorkflowStatusSS ws = _ezzyClient.workflowStatus((cbDocument.ScanInvoiceID ?? 0));

                    if (ws.complete)
                    {

                        if (ws.children.Count() > 0)
                        {
                            //document was autoseperated. iterate to get all child docs.
                            int id = ws.children[0];
                            InvoiceDetailsSS dd2 = await _ezzyClient.getInvoiceDetailsAsync(id);
                            if (dd2 != null)
                            {
                                if (dd2.invForm != null)
                                {
                                    string s = dd2.invForm.file_name; //the filename will be <parentdocid>.pdf
                                                                      //  cbDocument.CB_FilePath = dd2.invForm.blob_url;
                                }
                            }
                            //dd2.invForm.
                        }
                        break; //workflow complete
                    }

                    Console.WriteLine("status=" + ws.state);
                    Thread.Sleep(1000);
                    counter++;

                    if (counter > 100) break;

                    cbDocument.ScanServiceStatus = StringHelper.ToString(ws.service_status.status);
                    cbDocument.ScanServiceMessage = StringHelper.ToString(ws.service_status.message);
                }

                //now less get the data


                var ezzyInvoiceBlocksSS = await _ezzyClient.getInvoiceHeaderBlocksAsync((cbDocument.ScanInvoiceID ?? 0));
                if (ezzyInvoiceBlocksSS != null)
                {


                    cbDocument.ScanInvoiceTotal = ezzyInvoiceBlocksSS.invoiceForm.invoiceTotal;
                    cbDocument.ScanPurchaseOrder = ezzyInvoiceBlocksSS.invoiceForm.purchaseOrder;
                    cbDocument.ScanRefNumber = ezzyInvoiceBlocksSS.invoiceForm.invoiceNumber;
                    cbDocument.ScanSubTotal = ezzyInvoiceBlocksSS.invoiceForm.subTotal;
                    cbDocument.ScanTag = ezzyInvoiceBlocksSS.invoiceForm.tag;
                    cbDocument.ScanTaxTotal = ezzyInvoiceBlocksSS.invoiceForm.gstTotal;
                    cbDocument.ScanVendorName = ezzyInvoiceBlocksSS.invoiceForm.supplierName;
                    cbDocument.ScanDocumentTotal = ezzyInvoiceBlocksSS.invoiceForm.discountTotal;
                    cbDocument.ScanChargeTotal = ezzyInvoiceBlocksSS.invoiceForm.chargeTotal;
                    cbDocument.ScanABNNumber = ezzyInvoiceBlocksSS.invoiceForm.ABNnumber;
                    cbDocument.ScanInvoiceDate = ezzyInvoiceBlocksSS.invoiceForm.invoiceDate;
                    //Lines

                    List<XeroDocumentLine> lstQboDocumentLine = new List<XeroDocumentLine>();

                    foreach (var line in ezzyInvoiceBlocksSS.table)
                    {
                        XeroDocumentLine docLine = new XeroDocumentLine();
                        docLine.Scan_Quantity = line.quantity;
                        docLine.ScanUnit_Measure = line.unit_measure;
                        docLine.ScanUnit_Price = line.unit_price;
                        docLine.ScanTracking = line.tracking;
                        docLine.ScanTax_Code = line.tax_code;
                        docLine.ScanGST = line.gst;
                        docLine.ScanGL_Code = line.gl_code;
                        docLine.ScanDescription = line.description;
                        docLine.ScanArticle_Code = line.article_code;
                        docLine.Scan_Total = line.total;

                        lstQboDocumentLine.Add(docLine);
                    }

                    cbDocument.DocumentLine = lstQboDocumentLine;

                }


                InvoiceDetailsSS invDetails = await _ezzyClient.getInvoiceDetailsAsync((cbDocument.ScanInvoiceID ?? 0));
                if (invDetails != null)
                {
                    if (invDetails.invForm != null)
                    {
                        cbDocument.ScanBlob_Url = invDetails.invForm.blob_url;


                    }
                }

                DocumentClassificationSS docClassification = await _ezzyClient.getDocumentClassificationAsync((cbDocument.ScanInvoiceID ?? 0));
                if (docClassification != null)
                {
                    cbDocument.ScanDocType = StringHelper.ToString(docClassification.doc_subtype);
                    cbDocument.ScanDocClassification = docClassification.doc_classification;
                }
            }
            catch (Exception ex)
            {
                cbDocument.XeroError = ex.Message;
                _log.Error(ex);
            }

            return cbDocument;
        }
        //SEP29
     
    }
}