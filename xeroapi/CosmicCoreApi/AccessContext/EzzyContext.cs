using CosmicApiModel;
using CosmicCoreApi.CustomModel;
using CosmicCoreApi.EzzyService;
using CosmicCoreApi.Helper;
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

namespace CosmicCoreApi
{
    public class EzzyContext
    {
        EzzyServiceClient _ezzyClient = new EzzyServiceClient();
        CosmicContext _cosmicContext = CosmicContext.Instance;

        private SessionHelper _sessionHelper = new SessionHelper();
        Logger _log = null;

        private static bool RemoteCertificateValidate(

         object sender, System.Security.Cryptography.X509Certificates.X509Certificate cert,

          System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors error)
        {
            // trust any certificate!!!

            System.Console.WriteLine("Warning, trust any certificate");

            return true;
        }

        private userlocale GetUserlocale()
        {
            switch (_sessionHelper.CountryOfOrigin)
            {
                case "AU":
                    return userlocale.en_AU;

                case "NZ":
                    return userlocale.en_NZ;

                case "US":
                    return userlocale.en_US;

                case "UK":
                    return userlocale.en_GB;

                default:
                    return userlocale.en_AU;
            }
        }

        //internal bool RegisterUserForReckon(int reckonFileID)
        //{
        //    try
        //    {

        //        bool isLiveMode = CSConvert.ToBool(ConfigurationManager.AppSettings["Is_Live_Mode"]);

        //        ServicePointManager.ServerCertificateValidationCallback += RemoteCertificateValidate;

        //        System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;


        //        string cosmicEzzyAccount = CSConvert.ToString(ConfigurationManager.AppSettings["Cosmic_Ezzy_AccountName"]);
        //        var cosDBResponse = _cosmicContext.GetAccountMasterByAccountID(_sessionHelper.AccountID);
        //        if (cosDBResponse.Data != null)
        //        {
        //            cosmicEzzyAccount = CSConvert.ToString(cosDBResponse.Data.UserName);
        //        }
        //        string cosmicEzzyPass = CSConvert.ToString(ConfigurationManager.AppSettings["Cosmic_Ezzy_Password"]);

        //        var ezzyNextID = _cosmicContext.GetMaxEzzyReckonAccountID();
        //        string userPrefix = CSConvert.ToString(ConfigurationManager.AppSettings["User_Prefix_Reckon"]);

        //        string userName = string.Format("{0}_{1}_{2}", cosmicEzzyAccount, (isLiveMode ? userPrefix : "Test"), ezzyNextID);
        //        string emailAddress = string.Format("{0}_{1}_{2}@{3}.com", cosmicEzzyAccount, (isLiveMode ? userPrefix : "Test"), ezzyNextID, cosmicEzzyAccount);
        //        string password = RandomHelper.GetRandomString(RandomString.UserPassword);
        //        string cosmicEzzyAccountname = cosmicEzzyAccount;


        //        if (isLiveMode)
        //        {
        //            int rc = _ezzyClient.Login(cosmicEzzyAccount, cosmicEzzyPass);  // rc==1 indicates success

        //            // _ezzyClient.inviteUser(cosmicEzzyAccountname);
        //            Servicestatus serviceStatus = _ezzyClient.registerNewUser2(exporttarget.CUSTOM_WORKFLOW,
        //                                             GetUserlocale(),
        //                                              userName, cosmicEzzyAccountname, emailAddress, password);

        //            if (serviceStatus.status == servicestatus1.OK)
        //            {
        //                ReckonEzzyAccount reckonEzzyAccount = new ReckonEzzyAccount();
        //                reckonEzzyAccount.RCEzzyID = 0;
        //                reckonEzzyAccount.EzzyUserName = userName;
        //                reckonEzzyAccount.EzzyPassword = password;
        //                reckonEzzyAccount.EzzyEmailAddress = emailAddress;
        //                reckonEzzyAccount.ReckonFileID = reckonFileID;


        //                _cosmicContext.SaveReckonEzzyAccount(reckonEzzyAccount);

        //                return true;
        //            }
        //        }
        //        else
        //        {
        //            ReckonEzzyAccount reckonEzzyAccount = new ReckonEzzyAccount();
        //            reckonEzzyAccount.RCEzzyID = 0;
        //            reckonEzzyAccount.EzzyUserName = "Cosmicbills_1";
        //            reckonEzzyAccount.EzzyPassword = "nDTxDXBO";
        //            reckonEzzyAccount.EzzyEmailAddress = "Cosmicbills_1@Cosmicbills.com"; ;
        //            reckonEzzyAccount.ReckonFileID = reckonFileID;


        //            _cosmicContext.SaveReckonEzzyAccount(reckonEzzyAccount);

        //            return true;
        //        }


        //        // serviceStatus.message
        //        return false;
        //    }
        //    catch (Exception ex)
        //    {
        //        _log.Error(ex);
        //        return false;
        //    }
        //}

        //internal bool RegisterUserForQbo(int qboConnectID)
        //{
        //    try
        //    {
        //        bool isLiveMode = CSConvert.ToBool(ConfigurationManager.AppSettings["Is_Live_Mode"]);

        //        ServicePointManager.ServerCertificateValidationCallback += RemoteCertificateValidate;

        //        System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;


        //        string cosmicEzzyAccount = CSConvert.ToString(ConfigurationManager.AppSettings["Cosmic_Ezzy_AccountName"]);
        //        var cosDBResponse = _cosmicContext.GetAccountMasterByAccountID(_sessionHelper.AccountID);
        //        if (cosDBResponse.Data != null)
        //        {
        //            cosmicEzzyAccount = CSConvert.ToString(cosDBResponse.Data.UserName);
        //        }
        //        string cosmicEzzyPass = CSConvert.ToString(ConfigurationManager.AppSettings["Cosmic_Ezzy_Password"]);

        //        var ezzyNextID = _cosmicContext.GetMaxEzzyQboAccountID();
        //        string userLivePrefix = CSConvert.ToString(ConfigurationManager.AppSettings["User_Prefix_Qbo"]);
        //        string userTestPrefix = CSConvert.ToString(ConfigurationManager.AppSettings["User_Prefix_Qbo_Test"]);

        //        string userName = string.Format("{0}_{1}_{2}", cosmicEzzyAccount, (isLiveMode ? userLivePrefix : userTestPrefix), ezzyNextID);
        //        string emailAddress = string.Format("{0}_{1}_{2}@{3}.com", cosmicEzzyAccount, (isLiveMode ? userLivePrefix : userTestPrefix), ezzyNextID, cosmicEzzyAccount);
        //        string password = RandomHelper.GetRandomString(RandomString.UserPassword);
        //        string cosmicEzzyAccountname = cosmicEzzyAccount;


        //        if (isLiveMode)
        //        {
        //            int rc = _ezzyClient.Login(cosmicEzzyAccount, cosmicEzzyPass);  // rc==1 indicates success

        //            // _ezzyClient.inviteUser(cosmicEzzyAccountname);
        //            Servicestatus serviceStatus = _ezzyClient.registerNewUser2(exporttarget.CUSTOM_WORKFLOW,
        //                                             GetUserlocale(),
        //                                              userName, cosmicEzzyAccountname, emailAddress, password);

        //            if (serviceStatus.status == servicestatus1.OK)
        //            {
        //                QBOEzzyAccount qboEzzyAccount = new QBOEzzyAccount();
        //                qboEzzyAccount.QBOEzzyID = 0;
        //                qboEzzyAccount.EzzyUserName = userName;
        //                qboEzzyAccount.EzzyPassword = password;
        //                qboEzzyAccount.EzzyEmailAddress = emailAddress;
        //                qboEzzyAccount.QboConnectID = qboConnectID;


        //                _cosmicContext.SaveQboEzzyAccount(qboEzzyAccount);

        //                return true;
        //            }
        //        }
        //        else
        //        {
        //            QBOEzzyAccount qboEzzyAccount = new QBOEzzyAccount();
        //            qboEzzyAccount.QBOEzzyID = 0;
        //            qboEzzyAccount.EzzyUserName = "Cosmicbills_1";
        //            qboEzzyAccount.EzzyPassword = "nDTxDXBO";
        //            qboEzzyAccount.EzzyEmailAddress = "Cosmicbills_1@Cosmicbills.com"; ;
        //            qboEzzyAccount.QboConnectID = qboConnectID;


        //            _cosmicContext.SaveQboEzzyAccount(qboEzzyAccount);

        //            return true;
        //        }


        //        // serviceStatus.message
        //        return false;
        //    }
        //    catch (Exception ex)
        //    {
        //        _log.Error(ex);
        //        return false;
        //    }
        //}

        internal bool RegisterUserForXero(int xeroConnectID)
        {
            try
            {
                bool isLiveMode = CSConvert.ToBool(ConfigurationManager.AppSettings["Xero_Ezzy_Live"]);
                string webhookUrl = CSConvert.ToString(ConfigurationManager.AppSettings["Ezzybills_Webhook_Prod"]);
#if DEBUG
                webhookUrl = CSConvert.ToString(ConfigurationManager.AppSettings["Ezzybills_Webhook_Dev"]);
#endif
                ServicePointManager.ServerCertificateValidationCallback += RemoteCertificateValidate;

                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                
                string cosmicEzzyAccount = CSConvert.ToString(ConfigurationManager.AppSettings["Cosmic_Ezzy_AccountName"]);
                var cosDBResponse = _cosmicContext.GetAccountMasterByAccountID(_sessionHelper.AccountID);
                string customerUserName = string.Empty;
                if (cosDBResponse.Data != null)
                {
                    customerUserName = CSConvert.ToString(cosDBResponse.Data.UserName);
                }

                string cosmicEzzyPass = CSConvert.ToString(ConfigurationManager.AppSettings["Cosmic_Ezzy_Password"]);

                // var ezzyNextID = _cosmicContext.GetMaxEzzyXeroAccountID();
                var ezzyNextID = DateTime.Now.Year + "" + DateTime.Now.Month + "" + DateTime.Now.Day + "" + DateTime.Now.Hour + "" + DateTime.Now.Minute + "" + DateTime.Now.Second;
                string userLivePrefix = CSConvert.ToString(ConfigurationManager.AppSettings["User_Prefix_Xero"]);
                string userTestPrefix = CSConvert.ToString(ConfigurationManager.AppSettings["User_Prefix_Xero_Test"]);

                //string userName = string.Format("{0}_{1}_{2}", customerUserName, (isLiveMode ? userLivePrefix : userTestPrefix), ezzyNextID);
                //string emailAddress = string.Format("{0}_{1}_{2}@{3}.com", customerUserName, (isLiveMode ? userLivePrefix : userTestPrefix), ezzyNextID, cosmicEzzyAccount);
                string password = RandomHelper.GetRandomString(RandomString.UserPassword);
                //string userName = string.Format("{0}_{1}", customerUserName, (isLiveMode ? userLivePrefix : userTestPrefix));
                //string userName = string.Format("{0}_{1}_{2}", customerUserName, (isLiveMode ? userLivePrefix : userTestPrefix), ezzyNextID);
                //string emailAddress = string.Format("{0}_{1}_{2}@{3}.com", customerUserName, (isLiveMode ? userLivePrefix : userTestPrefix), ezzyNextID, cosmicEzzyAccount);
                string userName = string.Format("{0}_{1}", customerUserName, (isLiveMode ? userLivePrefix : userTestPrefix));
                string emailAddress = string.Format("{0}_{1}@{2}.com", customerUserName, (isLiveMode ? userLivePrefix : userTestPrefix), cosmicEzzyAccount);

                if (isLiveMode)
                {
                    int rc = _ezzyClient.Login(cosmicEzzyAccount, cosmicEzzyPass);  // rc==1 indicates success

                    Servicestatus serviceStatus = _ezzyClient.registerNewUser2(exporttarget.CUSTOM_WORKFLOW,
                                                    GetUserlocale(),
                                                      userName, cosmicEzzyAccount, emailAddress, password);
                    RegisterWebhook(_ezzyClient, webhookUrl, userName, password);
                    if (serviceStatus.status == servicestatus1.OK)
                    {
                        XeroEzzyAccount qboEzzyAccount = new XeroEzzyAccount();
                        qboEzzyAccount.XeroEzzyID = 0;
                        qboEzzyAccount.EzzyUserName = userName;
                        qboEzzyAccount.EzzyPassword = password;
                        qboEzzyAccount.EzzyEmailAddress = emailAddress;
                        qboEzzyAccount.XeroConnectID = xeroConnectID;
                        _cosmicContext.SaveXeroEzzyAccount(qboEzzyAccount);

                        return true;
                    }
                }
                else
                {
                    XeroEzzyAccount qboEzzyAccount = new XeroEzzyAccount();
                    qboEzzyAccount.XeroEzzyID = 0;
                    qboEzzyAccount.EzzyUserName = "Cosmicbills_1";
                    qboEzzyAccount.EzzyPassword = "nDTxDXBO";
                    qboEzzyAccount.EzzyEmailAddress = "Cosmicbills_1@Cosmicbills.com"; 
                    qboEzzyAccount.XeroConnectID = xeroConnectID;


                    _cosmicContext.SaveXeroEzzyAccount(qboEzzyAccount);

                    return true;
                }


                // serviceStatus.message
                return false;
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                return false;
            }
        }

        private void RegisterWebhook(EzzyServiceClient ezzyServiceClient, string webhookUrl, string userName, string password)
        {
            _ezzyClient.Login(userName, password);
            var f = _ezzyClient.mySettingsForm();
            f.settings.webHookURL = webhookUrl;
            _ezzyClient.saveMySettingsForm(f.settings);
        }

        internal async Task<CBDocument> ScanDocument(Byte[] fileBytes, string filename)
        {
            _log = CosmicLogger.SetLog();
            CBDocument cbDocument = new CBDocument();

            try
            {
                var scanningCred = _cosmicContext.GetRCScanningCredential();
                if (scanningCred == null)
                {
                    if (scanningCred.Data == null)
                    {
                        cbDocument.Error = "Failed to get credentials to scan document";
                    }
                }

                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

                int rc = _ezzyClient.Login(scanningCred.Data.EzzyUserName, scanningCred.Data.EzzyPassword);  // rc==1 indicates success

                PictureFileSS pf = new PictureFileSS() { PictureName = filename, PictureStream = fileBytes };

                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

                uploadStatusSS us = await _ezzyClient.uploadInvoiceImageAsync(pf, documenttypes.Invoice, "cosmicbills");

                //Do indenpendent work

                //Debug.Assert(us.service_status.status == servicestatus1.OK);
                //Debug.Assert(us.invoice_id > 0); // you can also get back the unique id for doc
                //ezzyClient.saveMySettingsForm(new SettingsForm() {  })

                cbDocument.InvoiceID = us.invoice_id;

                cbDocument.ScanServiceStatusMessage = us.service_status.message;
                cbDocument.ScanServiceStatus = CSConvert.ToString(us.service_status.status);

                if (cbDocument.InvoiceID == 0)
                {
                    return null;
                }

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


                _log.Error(ex);

            }
            //get a list of my 10 most recent invoices
            // InvoiceQueueSS qd = ezzyClient.getMyInvoiceQueue(0, 10, "Ordered");


            //now lets delete the invocie
            //int[] del = { invoice_id };
            //Servicestatus ss = ezzyClient.moveToRecycleBin(string.Empty, del);

            //get a list of deleted invoices
            // DeletedDocumentQueueSS dd = ezzyClient.getDeletedDocuments();
            return cbDocument;
        }

        internal async Task<CBDocument> UploadDocument(Byte[] fileBytes, string filename)
        {
            _log = CosmicLogger.SetLog();
            CBDocument cbDocument = new CBDocument();

            try
            {
                var scanningCred = _cosmicContext.GetRCScanningCredential();
                if (scanningCred == null)
                {
                    if (scanningCred.Data == null)
                    {
                        cbDocument.Error = "Failed to get credentials to scan document";
                    }
                }

                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

                int rc = _ezzyClient.Login(scanningCred.Data.EzzyUserName, scanningCred.Data.EzzyPassword);  // rc==1 indicates success


                PictureFileSS pf = new PictureFileSS() { PictureName = filename, PictureStream = fileBytes };
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                uploadStatusSS us = await _ezzyClient.uploadInvoiceImageAsync(pf, documenttypes.Invoice, "cosmicbills");

                cbDocument.InvoiceID = us.invoice_id;
                cbDocument.ScanServiceStatusMessage = us.service_status.message;
                cbDocument.ScanServiceStatus = CSConvert.ToString(us.service_status.status);
            }
            catch (Exception ex)
            {
                cbDocument.Error = ex.Message;
                _log.Error(ex);
            }

            return cbDocument;
        }

        internal async Task<CBDocument> GetInvoiceDetail(CBDocument cbDocument)
        {
            _log = CosmicLogger.SetLog();
            try
            {

                var scanningCred = _cosmicContext.GetRCScanningCredential();
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

        internal async Task<CBDocument> GetInvoiceDetailQbo(CBDocument cbDocument)
        {


            _log = CosmicLogger.SetLog();
            try
            {

                var scanningCred = _cosmicContext.GetQBOScanningCredential();
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

                cbDocument.EzzyInvoiceBlocksSS = await _ezzyClient.getInvoiceHeaderBlocksAsync((cbDocument.ScanInvoiceID ?? 0));
                cbDocument.ScanServiceStatus = cbDocument.EzzyInvoiceBlocksSS != null ? cbDocument.EzzyInvoiceBlocksSS.service_status.status.ToString() : string.Empty;
                if (cbDocument.EzzyInvoiceBlocksSS != null)
                {
                    if (cbDocument.EzzyInvoiceBlocksSS.invoiceForm != null)
                    {
                        cbDocument.CB_VendorListID = cbDocument.EzzyInvoiceBlocksSS.invoiceForm.supplierName;
                    }
                }

                InvoiceDetailsSS invDetails = await _ezzyClient.getInvoiceDetailsAsync((cbDocument.ScanInvoiceID ?? 0));
                if (invDetails != null)
                {
                    if (invDetails.invForm != null)
                    {
                        cbDocument.CB_FilePath = invDetails.invForm.blob_url;
                        cbDocument.CB_FileName = invDetails.invForm.file_name;
                    }
                }

                DocumentClassificationSS docClassification = await _ezzyClient.getDocumentClassificationAsync((cbDocument.ScanInvoiceID ?? 0));
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

    
        internal QboDocument UploadQboDocument(Byte[] fileBytes, string filename)
        {
            _log = CosmicLogger.SetLog();
            QboDocument qboDocument = new QboDocument();

            try
            {
                var scanningCred = _cosmicContext.GetQBOScanningCredential();
                if (scanningCred == null)
                {
                    if (scanningCred.Data == null)
                    {
                        qboDocument.QBOError = "Failed to get credentials to scan document";
                    }
                }

                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

                int rc = _ezzyClient.Login(scanningCred.Data.EzzyUserName, scanningCred.Data.EzzyPassword);  // rc==1 indicates success


                PictureFileSS pf = new PictureFileSS() { PictureName = filename, PictureStream = fileBytes };
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                uploadStatusSS us = _ezzyClient.uploadInvoiceImage(pf, documenttypes.Invoice, "cosmicbills");

                qboDocument.ScanInvoiceID = us.invoice_id;
                qboDocument.ScanServiceMessage = us.service_status.message;
                qboDocument.ScanServiceStatus = CSConvert.ToString(us.service_status.status);

            }
            catch (Exception ex)
            {
                qboDocument.QBOError = ex.Message;
                _log.Error(ex);
            }

            return qboDocument;
        }

        internal XeroDocument UploadXeroDocument(Byte[] fileBytes, string filename)
        {
            _log = CosmicLogger.SetLog();
            XeroDocument qboDocument = new XeroDocument();

            try
            {
                var scanningCred = _cosmicContext.GetXeroScanningCredential();
                if (scanningCred == null)
                {
                    if (scanningCred.Data == null)
                    {
                        qboDocument.XeroError = "Failed to get credentials to scan document";
                    }
                }
                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

                int rc = _ezzyClient.Login(scanningCred.Data.EzzyUserName, scanningCred.Data.EzzyPassword);  // rc==1 indicates success


                PictureFileSS pf = new PictureFileSS() { PictureName = filename, PictureStream = fileBytes };
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                uploadStatusSS us = _ezzyClient.uploadInvoiceImage(pf, documenttypes.Invoice, "cosmicbills");

                qboDocument.ScanInvoiceID = us.invoice_id;
                qboDocument.ScanServiceMessage = us.service_status.message;
                qboDocument.ScanServiceStatus = CSConvert.ToString(us.service_status.status);

            }
            catch (Exception ex)
            {
                qboDocument.XeroError = ex.Message;
                _log.Error(ex);
            }

            return qboDocument;
        }

        //SEP29
        internal async Task<QboDocument> GetScanDetailQbo(QboDocument cbDocument)
        {


            _log = CosmicLogger.SetLog();
            try
            {

                var scanningCred = _cosmicContext.GetQBOScanningCredential();
                if (scanningCred == null)
                {
                    if (scanningCred.Data == null)
                    {
                        cbDocument.QBOError = "Failed to get credentials to scan document";
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

                    cbDocument.ScanServiceStatus = CSConvert.ToString(ws.service_status.status);
                    cbDocument.ScanServiceMessage = CSConvert.ToString(ws.service_status.message);
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

                    //Lines

                    List<QboDocumentLine> lstQboDocumentLine = new List<QboDocumentLine>();

                    foreach (var line in ezzyInvoiceBlocksSS.table)
                    {
                        QboDocumentLine docLine = new QboDocumentLine();
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
                    cbDocument.ScanDocType = CSConvert.ToString(docClassification.doc_subtype);
                    cbDocument.ScanDocClassification = docClassification.doc_classification;
                }
            }
            catch (Exception ex)
            {
                cbDocument.QBOError = ex.Message;
                _log.Error(ex);
            }

            return cbDocument;
        }

        internal async Task<XeroDocument> GetScanDetailXero(XeroDocument cbDocument)
        {


            _log = CosmicLogger.SetLog();
            try
            {

                var scanningCred = _cosmicContext.GetXeroScanningCredential();
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
                    var invoiceStatus = _cosmicContext.GetXeroInvoiceProccessStatus(cbDocument.ScanInvoiceID==null?"0": cbDocument.ScanInvoiceID.Value.ToString());
                    if(invoiceStatus!=null && invoiceStatus.proccessStatus == 1)
                    {
                        break;
                    }
                    Thread.Sleep(2000);
                    counter++;

                    if (counter > 100) break;

                    cbDocument.ScanServiceStatus = CSConvert.ToString("Unable to proccess invoice");
                    cbDocument.ScanServiceMessage = CSConvert.ToString("time out"); 
                    /*
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

                    if (counter > 100)
                    {
                        break;
                    }
                      cbDocument.ScanServiceStatus = CSConvert.ToString(ws.service_status.status);
                      cbDocument.ScanServiceMessage = CSConvert.ToString(ws.service_status.message); */


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
                    if(string.IsNullOrEmpty(cbDocument.ScanVendorName))
                    {

                    }
                    cbDocument.ScanDocumentTotal = ezzyInvoiceBlocksSS.invoiceForm.discountTotal;
                    cbDocument.ScanChargeTotal = ezzyInvoiceBlocksSS.invoiceForm.chargeTotal;
                    cbDocument.ScanABNNumber = ezzyInvoiceBlocksSS.invoiceForm.ABNnumber;
                    cbDocument.ScanInvoiceDate = ezzyInvoiceBlocksSS.invoiceForm.invoiceDate;
                    //Lines

                    List<XeroDocumentLine> lstQboDocumentLine = new List<XeroDocumentLine>();
                    decimal? gstTotal = 0;
                    List<InvoiceRow> invRow = new List<InvoiceRow>();
                    foreach (var line in ezzyInvoiceBlocksSS.table)
                    {
                        if(line.gst<=0 && line.total <= 0 && string.IsNullOrEmpty(line.tax_code))
                        {
                         //   continue;
                        }
                        invRow.Add(line);
                        XeroDocumentLine docLine = new XeroDocumentLine();
                        docLine.Scan_Quantity = line.quantity;
                        docLine.ScanUnit_Measure = line.unit_measure;
                        docLine.ScanUnit_Price = line.unit_price;
                        docLine.ScanTracking = line.tracking;
                        docLine.ScanTax_Code = line.tax_code;
                        docLine.ScanGST = (line.gst);
                        docLine.ScanGL_Code = line.gl_code;
                        docLine.ScanDescription = line.description;
                        docLine.ScanArticle_Code = line.article_code;
                        docLine.Scan_Total =line.total;
                        docLine.Scan_Total = MathHelper.TruncateDecimal(docLine.Scan_Total);

                        lstQboDocumentLine.Add(docLine);
                        gstTotal += docLine.ScanGST;
                    }
                    ezzyInvoiceBlocksSS.table = invRow.ToArray();
                    cbDocument.ScanTaxTotal = MathHelper.TruncateDecimal(gstTotal.Value);
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
                    cbDocument.ScanDocType = CSConvert.ToString(docClassification.doc_subtype);
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
       

        internal void UploadQboVendorToEz(List<QboVendor> lstQboVendor)
        {
            List<ContactDetails> lstContactDetail = new List<ContactDetails>();

            try
            {

                if (lstQboVendor != null)
                {
                    foreach (var item in lstQboVendor)
                    {
                        ContactDetails contact = new ContactDetails();
                        contact.Name = item.DisplayNameField;
                        contact.Id = item.QboVendorID;
                        contact.taxNumber = item.TaxIdentifier;
                        contact.email = item.PrimaryEmailAddr;
                        //contact.contact = item. CSConvert.ToString(row, "Contact");
                        //contact.fax = CSConvert.ToString(row, "Fax");
                        contact.phone = item.PrimaryPhone;

                        contact.city = item.CityField;
                        contact.postcode = item.PostalCodeField;
                        contact.country = item.CountryField;
                        contact.region = item.CountrySubDivisionCodeField;
                        //contact.address = item.A CSConvert.ToString(drAdd[0], "Addr1");
                        lstContactDetail.Add(contact);
                    }
                }

                if (lstContactDetail.Count > 0)
                {
                    var scanningCred = _cosmicContext.GetQBOScanningCredential();
                    if (scanningCred == null)
                    {
                        if (scanningCred.Data == null)
                        {
                            Logger log = CosmicLogger.SetLog();
                            log.Info("Failed to get credentials to upload vendors : AccountID: " + _sessionHelper.AccountID);
                        }
                    }

                    ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

                    int rc = _ezzyClient.Login(scanningCred.Data.EzzyUserName, scanningCred.Data.EzzyPassword);  // rc==1 indicates success

                    Servicestatus ss3 = _ezzyClient.addContacts(lstContactDetail.ToArray(), exporttarget.CUSTOM_WORKFLOW);
                    if (ss3.status != servicestatus1.OK)
                    {
                        Logger log = CosmicLogger.SetLog();
                        log.Info("Failed to upload venords  : AccountID: " + _sessionHelper.AccountID + " Message :" + ss3.message);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
            }
        }

        internal void UploadXeroVendorToEz(List<XeroVendor> lstQboVendor)
        {
            List<ContactDetails> lstContactDetail = new List<ContactDetails>();

            try
            {

                if (lstQboVendor != null)
                {
                    foreach (var item in lstQboVendor)
                    {
                        ContactDetails contact = new ContactDetails();
                        contact.Name = item.DisplayNameField;
                        contact.Id = item.XeroVendorID;
                        contact.taxNumber = item.TaxIdentifier;
                        contact.email = item.PrimaryEmailAddr;
                        //contact.contact = item. CSConvert.ToString(row, "Contact");
                        //contact.fax = CSConvert.ToString(row, "Fax");
                        contact.phone = item.PrimaryPhone;

                        contact.city = item.CityField;
                        contact.postcode = item.PostalCodeField;
                        contact.country = item.CountryField;
                        contact.region = item.CountrySubDivisionCodeField;
                        //contact.address = item.A CSConvert.ToString(drAdd[0], "Addr1");
                        lstContactDetail.Add(contact);
                    }
                }

                if (lstContactDetail.Count > 0)
                {
                    var scanningCred = _cosmicContext.GetXeroScanningCredential();
                    if (scanningCred == null)
                    {
                        if (scanningCred.Data == null)
                        {
                            Logger log = CosmicLogger.SetLog();
                            log.Info("Failed to get credentials to upload vendors : AccountID: " + _sessionHelper.AccountID);
                        }
                    }

                    ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

                    int rc = _ezzyClient.Login(scanningCred.Data.EzzyUserName, scanningCred.Data.EzzyPassword);  // rc==1 indicates success

                    Servicestatus ss3 = _ezzyClient.addContacts(lstContactDetail.ToArray(), exporttarget.CUSTOM_WORKFLOW);
                    if (ss3.status != servicestatus1.OK)
                    {
                        Logger log = CosmicLogger.SetLog();
                        log.Info("Failed to upload venords  : AccountID: " + _sessionHelper.AccountID + " Message :" + ss3.message);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
            }
        }
    }
}