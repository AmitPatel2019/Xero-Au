using CosmicApiHelper;
using CosmicApiModel;
using XeroAutomationService;
using XeroAutomationService.EzzyModel;
using XeroAutomationService.EzzyService;
using Flexis.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace XeroAutomationService.Common
{
    public class EzzyServiceNew
    {
        string _userName;
        string _password;
        Logger log;
        public EzzyServiceNew(string userName, string password)
        {
            _userName = userName;
            _password = password;
            log = CosmicLogger.SetLog();
            //log = CosmicLogger.SetLog();
        }

        internal async Task<ScanInvoiceData> GetInvoiceScanData(XeroDocument cbDocument)
        {

            ScanInvoiceData scanInvoiceData = new ScanInvoiceData();

            log = CosmicLogger.SetLog();
            try
            {
             
                EzzyServiceClient _ezzyClient = new EzzyServiceClient();
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                int rc = _ezzyClient.Login(_userName, _password);  // rc==1 indicates success
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
                Thread.Sleep(3000);
                log.Info("counter:" + counter);
                string addresss = String.Empty;
                string job = string.Empty;
                var ezzyInvoiceBlocksSS = await _ezzyClient.getInvoiceHeaderBlocksAsync((cbDocument.ScanInvoiceID ?? 0));
                log.Info("InvoiceBlock:" + ezzyInvoiceBlocksSS);
                scanInvoiceData.invoiceBlocksSS = ezzyInvoiceBlocksSS;
                scanInvoiceData.formData = _ezzyClient.getFormData((cbDocument.ScanInvoiceID ?? 0));
                log.Info("formData:" + scanInvoiceData.formData);

                scanInvoiceData.invoiceDetailsSS = await _ezzyClient.getInvoiceDetailsAsync((cbDocument.ScanInvoiceID ?? 0));
                log.Info("invoiceDetailsSS:" + scanInvoiceData.invoiceDetailsSS);
                scanInvoiceData.documentClassificationSS = await _ezzyClient.getDocumentClassificationAsync((cbDocument.ScanInvoiceID ?? 0));
                log.Info("scanInvoiceData.documentClassificationSS:" + scanInvoiceData.documentClassificationSS);
                scanInvoiceData.formDataSS2 = await _ezzyClient.getFormDataAsync((cbDocument.ScanInvoiceID ?? 0));
                log.Info("formDatass2:" + scanInvoiceData.formDataSS2);

                //now less get the data



            }
            catch (Exception ex)
            {
                cbDocument.XeroError = ex.Message;
                log.Error(ex);
            }

            return scanInvoiceData;
        }

        //public async Task<ScanInvoiceData> GetInvoiceScanData(QboDocument qboDocument)
        //{
        //    ScanInvoiceData scanInvoiceData = new ScanInvoiceData();
        //    EzzyServiceClient _ezzyClient = new EzzyServiceClient();
        //    ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
        //    int rc = _ezzyClient.Login(_userName, _password);  // rc==1 indicates success
        //    int counter = 0;
        //    while (true)
        //    {
        //        WorkflowStatusSS ws = _ezzyClient.workflowStatus((qboDocument.ScanInvoiceID ?? 0));
        //        if (ws.complete)
        //        {
        //            if (ws.children.Count() > 0)
        //            {
        //                //document was autoseperated. iterate to get all child docs.
        //                int id = ws.children[0];
        //                InvoiceDetailsSS dd2 = await _ezzyClient.getInvoiceDetailsAsync(id);
        //                if (dd2 != null)
        //                {
        //                    if (dd2.invForm != null)
        //                    {
        //                        string s = dd2.invForm.file_name; //the filename will be <parentdocid>.pdf //  cbDocument.CB_FilePath = dd2.invForm.blob_url;
        //                    }
        //                }
        //            }
        //            break; //workflow complete
        //        }

        //        Console.WriteLine("status=" + ws.state);
        //        Thread.Sleep(1000);
        //        counter++;

        //        scanInvoiceData.ScanServiceStatus = (ws.service_status.status).ToString();
        //        scanInvoiceData.ScanServiceMessage = (ws.service_status.message).ToString();
        //    }

        //    Thread.Sleep(3000);

        //    string addresss = String.Empty;
        //    string job = string.Empty;
        //    var ezzyInvoiceBlocksSS = await _ezzyClient.getInvoiceHeaderBlocksAsync((qboDocument.ScanInvoiceID ?? 0));
        //    scanInvoiceData.invoiceBlocksSS = ezzyInvoiceBlocksSS;
        //    scanInvoiceData.formData = _ezzyClient.getFormData((qboDocument.ScanInvoiceID ?? 0));
        //    scanInvoiceData.invoiceDetailsSS = await _ezzyClient.getInvoiceDetailsAsync((qboDocument.ScanInvoiceID ?? 0));
        //    scanInvoiceData.documentClassificationSS = await _ezzyClient.getDocumentClassificationAsync((qboDocument.ScanInvoiceID ?? 0));
        //    scanInvoiceData.formDataSS2 = await _ezzyClient.getFormDataAsync((qboDocument.ScanInvoiceID ?? 0));

        //    return scanInvoiceData;
        //}

        public async Task<ScanInvoiceData> GetInvoiceScanDataAsync(int scanInvoiceID)
        {
            ScanInvoiceData scanInvoiceData = new ScanInvoiceData();
            EzzyServiceClient _ezzyClient = new EzzyServiceClient();
            try
            {
#if DEBUG
                Utils.SetCertificatePolicy();
#endif
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                int rc = _ezzyClient.Login(_userName, _password);  // rc==1 indicates success
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                while (true)
                {
                    WorkflowStatusSS ws = _ezzyClient.workflowStatus(scanInvoiceID);
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
                                    string s = dd2.invForm.file_name; //the filename will be <parentdocid>.pdf //  cbDocument.CB_FilePath = dd2.invForm.blob_url;
                                }
                            }
                        }
                        break;
                    }

                    Console.WriteLine("status=" + ws.state);
                    Thread.Sleep(1000);

                    scanInvoiceData.ScanServiceStatus = (ws.service_status.status).ToString();
                    scanInvoiceData.ScanServiceMessage = (ws.service_status.message).ToString();
                }

                Thread.Sleep(3000);

                scanInvoiceData.invoiceBlocksSS = await _ezzyClient.getInvoiceHeaderBlocksAsync(scanInvoiceID);
                scanInvoiceData.formData = await _ezzyClient.getFormDataAsync(scanInvoiceID);
                scanInvoiceData.invoiceDetailsSS = await _ezzyClient.getInvoiceDetailsAsync(scanInvoiceID);
                scanInvoiceData.documentClassificationSS = await _ezzyClient.getDocumentClassificationAsync(scanInvoiceID);
                scanInvoiceData.formDataSS2 = await _ezzyClient.getFormDataAsync(scanInvoiceID);
            }
            catch (Exception ex)
            {
                log.Error(ex);
                log.Error("Error in EzzyService GetInvoiceScanData");
            }
            return scanInvoiceData;
        }

        internal XeroDocument UploadXeroDocument(Byte[] fileBytes, string filename)
        {
            XeroDocument qboDocument = new XeroDocument();

            try
            {
                EzzyServiceClient _ezzyClient = new EzzyServiceClient();
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

                int rc = _ezzyClient.Login(_userName, _password);  // rc==1 indicates success


                PictureFileSS pf = new PictureFileSS() { PictureName = filename, PictureStream = fileBytes };
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                uploadStatusSS us = _ezzyClient.uploadInvoiceImage(pf, documenttypes.Invoice, "cosmicbills");

                qboDocument.ScanInvoiceID = us.invoice_id;
                qboDocument.ScanServiceMessage = us.service_status.message;
                qboDocument.ScanServiceStatus = StringHelper.ToString(us.service_status.status);

            }
            catch (Exception ex)
            {
                qboDocument.XeroError = ex.Message;
            }

            return qboDocument;
        }

       
    }
}
