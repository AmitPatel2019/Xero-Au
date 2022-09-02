using Cosmic.DataLayer.Logic;
using CosmicApiHelper;
using CosmicApiHelper.Enums;
using CosmicApiModel;
using XeroAutomationService;
using XeroAutomationService.Common;
using XeroAutomationService.EmailsModel;
using XeroAutomationService.EzzyModel;
using Flexis.Log;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using XeroAutomationService.AccessContext;
using MailKit;
using System.Web;
using System.Security.Cryptography.X509Certificates;
using DevDefined.OAuth.Storage.Basic;
using XeroApi.Model;
using XeroApi;

namespace Common
{
    public class EzzyScans
    {
        private XeroContext _csContext = new XeroContext();
        private EzzyContext _ezContext = new EzzyContext();
        Flexis.Log.Logger _log;
        List<string> adminEmails = new List<string>();
        private int errorCount = 1;
        EmailSettingModel emailSettingModel = null;
        public EzzyScans()
        {
            _log = CosmicLogger.SetLog();
            adminEmails = Settings.GetAllAdminEmails();
            emailSettingModel = Settings.GetAllEmailSettings();
            errorCount = Int32.Parse(Settings.ReadSetting("ErrorCount"));

        }
        private static bool RemoteCertificateValidate(

                object sender, System.Security.Cryptography.X509Certificates.X509Certificate cert,

                 System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors error)
        {
            // trust any certificate!!!

            System.Console.WriteLine("Warning, trust any certificate");

            return true;
        }
        string domainPath = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.FullName;
        string certificateFilePath = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.FullName + "\\Certtificate\\public_privatekey.pfx";

        public async Task scanUploadedDocumentfromWeb()
        {
            _log.Info($"Start Scan Document ==> ScanUploadedDocumentfromWeb {DateTime.Now}");

            try
            {
                var qboJobs = _csContext.GetQboJobToProcess();

                //List<QboDocument> listQboDocuments = new List<QboDocument>();
                if (qboJobs != null && qboJobs.Data != null && qboJobs.Data.Count > 0)
                {
                    foreach (QboJob qboJob in qboJobs.Data)
                    {
                        List<string> qboDocuments = qboJob.DocumentIDs.ToString().Split(',').ToList();
                        List<QBResponse> listScanResponse = new List<QBResponse>();
                        List<XeroPostResponse> listPostResponse = new List<XeroPostResponse>();
                        var loginMaster = DataManager_Users.GetLogin(qboJob.AccountID);
                        var xeroMasterData = _csContext.GetXeroMasterByAccountID(qboJob.AccountID);
                        XeroMaster xeroMaster = xeroMasterData.Data.FirstOrDefault();
                        List<XeroDocumentLine> listXeroDocumentLines = new List<XeroDocumentLine>();
                        foreach (string documentID in qboDocuments)
                        {
                            var xeroDocument = _csContext.GetXeroDocument(StringHelper.ToInt(documentID));
                            if (xeroDocument.Data != null)
                            {
                                if (String.IsNullOrEmpty(xeroDocument.Data.XeroInvoiceID))
                                {
                                    var scanResponse = await ScanXeroDocument(xeroDocument.Data);

                                    //var loginMaster = DataManager_Users.GetLogin(qboDocument.Data.AccountID);
                                    if (scanResponse != null)
                                    {
                                        if (scanResponse.Success == true)
                                        {

                                            _log.Info("Scanning successfully done....==> Web PDFs & DocumentID : " + documentID);
                                            if (String.IsNullOrEmpty(xeroDocument.Data.XeroInvoiceID))
                                            {
                                                if (xeroMaster.DirectPostfromEmail == true)
                                                {

                                                    var xeroDocumentLine = _csContext.GetXeroDocumentLine(StringHelper.ToInt(documentID), xeroDocument.Data.AccountID, xeroDocument.Data.XeroConnectID);
                                                    if (xeroDocumentLine?.Data?.Count == 0)
                                                    {

                                                        _log.Info($"Not found Any document for Update! for User {xeroDocument.Data.AccountID}  {DateTime.Now} ");
                                                        //return;
                                                    }
                                                    else
                                                    {
                                                        if (String.IsNullOrEmpty(xeroDocument.Data.XeroVendorID))
                                                        {
                                                            _log.Info($"can't post document because Supplier Name Not Exist {xeroDocument.Data.DocumentID}  {xeroDocument.Data.ScanRefNumber}");
                                                            continue;
                                                        }
                                                        else
                                                        {
                                                            var emptyitemlist = xeroDocumentLine.Data.FirstOrDefault(ff => string.IsNullOrEmpty(ff.XeroAccountID) == true);
                                                            if (emptyitemlist != null)
                                                            {
                                                                Logger log = CosmicLogger.SetLog();
                                                                log.Error($"can't post document because XeroAccountID Not Exist {xeroDocument.Data.DocumentID}");
                                                                _csContext.LogErrorToDB("rah", "create invoice", $"can't post document because AccountID Not Exist {xeroDocument.Data.DocumentID} ", null);

                                                                continue;
                                                            }

                                                        }
                                                        xeroDocument.Data.Approved = true;

                                                        var res = _csContext.InsertXeroDocument(xeroDocument.Data);

                                                        listXeroDocumentLines = listXeroDocumentLines.Concat(xeroDocumentLine.Data).ToList();

                                                    }

                                                }


                                                else
                                                {
                                                    listScanResponse.Add(scanResponse);

                                                }
                                            }
                                            else
                                                _log.Info("Document already Posted....==> Web PDFs & DocumentID : " + documentID);


                                        }
                                        else
                                        {
                                            listScanResponse.Add(scanResponse);

                                            _log.Info("Error in scanning document ==> Web PDF " + scanResponse.Message);
                                        }
                                    }
                                    else
                                    {
                                        _log.Info("Error found in Document with DOcumentID ==> " + xeroDocument.Data.DocumentID);
                                    }
                                }
                            }

                            else
                            {
                                _log.Info($"Not Found Document with ID==> {documentID}");
                            }
                        }
                        if (listXeroDocumentLines.Count > 0)
                        {

                            listXeroDocumentLines.ToList().ForEach(u => u.SelectToBill = true);
                            if (xeroMaster.XeroDocPostAs?.Length > 0)
                                listXeroDocumentLines.ToList().ForEach(u => u.ApproveDocAs = getApprovalValue(xeroMaster.XeroDocPostAs));

                            if (GetXeroToken(xeroMasterData.Data.FirstOrDefault()))
                            {
                                List<XeroPostResponse> lstResponse = new List<XeroPostResponse>();
                                string error = string.Empty;
                                Cosmic.DataLayer.XeroMaster xeroMasterDataLayer = DataManager_Users.GetXeroSettings(Convert.ToInt32(xeroMaster.AccountID));

                                var bills = AddBill(listXeroDocumentLines, GetXeroTax(xeroMaster.AccountID, xeroMaster.XeroID), ref error, ref lstResponse, xeroMasterDataLayer);


                                if (lstResponse?.Count > 0)
                                {
                                    loginMaster = DataManager_Users.GetLogin(xeroMaster.AccountID);
                                    if (loginMaster?.EmailAddress != null)
                                    {
                                        EmailSender.SendPostSuccessEmail(Settings.GetAllEmailSettings(), new List<string> { loginMaster.EmailAddress }, "Document Posted Successfully", HtmlUtility.getInvoiceTable(lstResponse), loginMaster.UserName);

                                        _log.Info("Successfully sent mail..");
                                    }
                                    else
                                        _log.Info("Email Address not found for user with AccountID==" + loginMaster.AccountID);
                                }
                            }
                            else
                            {
                                _log.Info($"Get xerotoken returned false for Account ID ==>{ xeroMaster.AccountID}");

                            }

                        }
                        var resp = _csContext.UpdateQboJob(qboJob.ID);
                        if (loginMaster != null)
                        {
                            if (listScanResponse?.Count > 0)
                            {
                                if (loginMaster?.EmailAddress != null)
                                {
                                    EmailSender.SendPostSuccessEmail(Settings.GetAllEmailSettings(), new List<string> { loginMaster.EmailAddress }, "Document Scanned Successfully", HtmlUtility.getScanInvoiceTable(listScanResponse), loginMaster.UserName);

                                    _log.Info("Successfully sent mail..");
                                }
                                else
                                    _log.Info("Email Address not found for user with AccountID==" + loginMaster.AccountID);
                            }

                        }

                    }

                }
                else
                {
                    _log.Info("Not found Any document to Process==>scanuploadeddocumentfromweb");
                }
            }
            catch (Exception e)
            {
                _log.Info("scanUploadedDocumentfromWeb => Error in Document Proccessing ==>" + e.Message.ToString());
            }

        }
        private int getApprovalValue(string status)
        {
            int result = 0;
            switch (status)
            {
                case "DR":
                    result = 1;
                    break;

                case "AP":
                    result = 2;
                    break;

                case "WAP":
                    result = 3;
                    break;

                default:
                    break;
            }
            return result;
        }

        public async Task SyncXeroDocumentEzzyScanning(List<XeroDocument> listXeroDocument)
        {
            _log.Info("XeroDocument Scanning Started....");
            foreach (XeroDocument xeroDocument in listXeroDocument)
            {
                var scanResponse = await ScanXeroDocument(xeroDocument);
                var xeroMasterData = _csContext.GetXeroMasterByAccountID(xeroDocument.AccountID);

                XeroMaster xeroMaster = xeroMasterData.Data.FirstOrDefault();

                if (xeroMaster.DirectPostfromEmail == true)
                {
                    var lines = _csContext.GetXeroDocumentLine(xeroDocument.DocumentID, xeroMaster.AccountID, xeroMaster.XeroID);

                    if (String.IsNullOrEmpty(xeroDocument.ScanVendorName))
                    {
                        _log.Info($"can't post document because Supplier Name Not Exist {xeroDocument.DocumentID}  {xeroDocument.ScanRefNumber}");
                        continue;
                    }
                    else
                    {
                        var emptyitemlist = lines.Data.FirstOrDefault(ff => string.IsNullOrEmpty(ff.XeroAccountID) == true);
                        if (emptyitemlist != null)
                        {
                            Logger log = CosmicLogger.SetLog();
                            log.Error($"can't post document because XeroAccountID Not Exist {xeroDocument.DocumentID}");
                            _csContext.LogErrorToDB("rah", "create invoice", $"can't post document because AccountID Not Exist {xeroDocument.DocumentID} ", null);

                            continue;
                        }

                    }

                    xeroDocument.Approved = true;

                    lines.Data.ToList().ForEach(u => u.SelectToBill = true);

                    var res = _csContext.InsertXeroDocument(xeroDocument);
                    List<XeroPostResponse> postResponse = new List<XeroPostResponse>();

                    var sameInvoiceData = DataManager_Documents.GetSameInvoiceQBODocument(xeroDocument.DocumentID, xeroDocument.ScanRefNumber, xeroDocument.AccountID);
                    if (sameInvoiceData == null)
                    {
                        List<XeroPostResponse> lstResponse = new List<XeroPostResponse>();

                        var qboDocumentRes = _csContext.GetXeroDocument(xeroDocument.DocumentID).Data;
                        if (qboDocumentRes.ErrorCount < errorCount)
                        {
                            Cosmic.DataLayer.XeroMaster xeroMasterDataLayer = DataManager_Users.GetXeroSettings(Convert.ToInt32(xeroDocument.AccountID));
                            XeroPostResponse xeroResponse;
                            if (string.Compare(StringHelper.ToString(xeroDocument.ScanDocType).Trim().ToUpper(), "CREDITNOTE", true) == 0)
                            {
                                xeroResponse = CreateVendorCredit(xeroDocument.DocumentID, lines.Data?.ElementAt(0), lines.Data.ToList(), GetXeroTax(xeroMaster.AccountID, xeroMaster.XeroID), ref lstResponse, xeroMasterDataLayer);
                            }
                            else
                            {
                                xeroResponse = CreateBill(xeroDocument.DocumentID, lines.Data?.ElementAt(0), lines.Data.ToList(), GetXeroTax(xeroMaster.AccountID, xeroMaster.XeroID), ref lstResponse, xeroMasterDataLayer);
                            }

                            if (xeroResponse.ReponseFromXero.ToLower().Equals("failed"))
                            {

                                xeroDocument.ErrorCount++;
                                _csContext.InsertXeroDocument(xeroDocument);
                                _log.Info($"Post document failed for Invoice No: {xeroResponse.InvoiceNo}");

                                postResponse.Add(xeroResponse);
                            }
                            else
                            {
                                _log.Info($"Successfully posted document for Invoice No: {xeroResponse.InvoiceNo}");
                                postResponse.Add(xeroResponse);
                            }
                            if (postResponse?.Count > 0)
                            {
                                var loginMasterData = DataManager_Users.GetLogin(xeroDocument.AccountID);
                                if (loginMasterData?.EmailAddress != null)
                                {
                                    EmailSender.SendPostSuccessEmail(Settings.GetAllEmailSettings(), new List<string> { loginMasterData.EmailAddress }, "Document Posted Successfully", HtmlUtility.getInvoiceTable(postResponse), loginMasterData.UserName);

                                    _log.Info("Successfully sent mail..");
                                }
                                else
                                    _log.Info("Email Address not found for user with AccountID==" + loginMasterData.AccountID);
                            }
                        }

                        else
                            _log.Info($"Can't again post to account because limit exceeds from its higher value ===> Doc ID {xeroDocument.DocumentID}");

                    }
                    else
                    {
                        xeroDocument.Approved = false;
                        _csContext.InsertXeroDocument(xeroDocument);
                        XeroPostResponse xeroResponse = SaveResp(xeroDocument.ScanRefNumber, xeroDocument.XeroVendorName, "Same Invoice ID Exist..", "Failed", false);
                        postResponse.Add(xeroResponse);
                        if (postResponse?.Count > 0)
                        {
                            var loginMasterData = DataManager_Users.GetLogin(xeroDocument.AccountID);
                            if (loginMasterData?.EmailAddress != null)
                            {
                                EmailSender.SendPostSuccessEmail(Settings.GetAllEmailSettings(), new List<string> { loginMasterData.EmailAddress }, "Document Post Error", HtmlUtility.getInvoiceTable(postResponse), loginMasterData.UserName);

                                _log.Info("Successfully sent mail..");
                            }
                            else
                                _log.Info("Email Address not found for user with AccountID==" + loginMasterData.AccountID);
                        }   
                        _log.Info($"Same Invoice Number Exist {xeroDocument.ScanRefNumber}..... Post document failed for Document ID: {xeroDocument.DocumentID} ====> User Account : {xeroMasterData.Data.FirstOrDefault().AccountID}");
                        continue;
                    }

                }
                else
                {
                    var loginMasterData = DataManager_Users.GetLogin(xeroDocument.AccountID);
                    if (loginMasterData != null)
                    {
                        string senderEmailAddress = loginMasterData.EmailAddress;

                        if (scanResponse.Success == true)
                        {
                            EmailSender.SendSuccessEmail(emailSettingModel, new List<string> { senderEmailAddress }, "Document scanned successfully", "<br/>We have scanned your PDFs please login to cosmicbills and Approve at process 3.<br/> <br/> Select and Tick ALL or select PDFs needed to be sent to Accounting System and just Log out <br/> <br/><br/>Thank you ", loginMasterData.UserName);
                            _log.Info("Scanning successfully done & email sent to user....");

                        }
                        else
                        {
                            EmailSender.SendErrorEmail(emailSettingModel, new List<string> { senderEmailAddress }, "Document scan Error", "Your Document with Document ID : " + xeroDocument.DocumentID + " has an error.<br/><b>Error:</b> " + scanResponse.Message, loginMasterData.UserName);
                            _log.Info("Error in scanning document " + scanResponse.Message);
                        }
                    }
                    else
                        _log.Info("Login ID Not found for user ==> " + loginMasterData.AccountID);
                }
            }
        }
        private XeroTax GetXeroTax(int AccountID, int XeroConnectID)
        {
            XeroTax qboTax = null;
            var qboTaxs = _csContext.GetXeroTax(AccountID, XeroConnectID);
            var loginMaster = DataManager_Users.GetAccount(AccountID);
            if (loginMaster != null)
            {

                switch (loginMaster.CountryOfOrigin)
                {
                    case "AU":

                        if (qboTaxs != null)
                        {
                            if (qboTaxs.Data != null)
                            {
                                qboTax = qboTaxs.Data.FirstOrDefault(ff => StringHelper.ToString(ff.TaxName).Trim().ToUpper() == "GST ON EXPENSES");
                            }

                            if (qboTax == null)
                            {
                                qboTax = qboTaxs.Data.FirstOrDefault(ff => StringHelper.ToString(ff.TaxID).Trim().ToUpper() == "INPUT");
                            }
                        }

                        if (qboTax == null)
                        {
                            qboTax = new XeroTax() { TaxName = "GST on Expenses", TaxID = "INPUT" };
                        }

                        return qboTax;

                    case "NZ":
                        if (qboTaxs != null)
                        {
                            if (qboTaxs.Data != null)
                            {
                                qboTax = qboTaxs.Data.FirstOrDefault(ff => StringHelper.ToString(ff.TaxName).Trim().ToUpper() == "GST ON EXPENSES");
                            }

                            if (qboTax == null)
                            {
                                qboTax = qboTaxs.Data.FirstOrDefault(ff => StringHelper.ToString(ff.TaxID).Trim().ToUpper() == "INPUT");
                            }
                        }

                        if (qboTax == null)
                        {
                            qboTax = new XeroTax() { TaxName = "GST on Expenses", TaxID = "INPUT" };
                        }

                        return qboTax;


                    case "UK":
                        if (qboTaxs != null)
                        {
                            if (qboTaxs.Data != null)
                            {
                                qboTax = qboTaxs.Data.FirstOrDefault(ff => StringHelper.ToString(ff.TaxName).Trim().ToUpper() == "17.5% (VAT ON EXPENSES)");
                            }

                            if (qboTax == null)
                            {
                                qboTax = qboTaxs.Data.FirstOrDefault(ff => StringHelper.ToString(ff.TaxID).Trim().ToUpper() == "INPUT");
                            }
                        }

                        if (qboTax == null)
                        {
                            qboTax = new XeroTax() { TaxName = "17.5% (VAT on Expenses)", TaxID = "INPUT" };
                        }

                        return qboTax;

                    default:
                        return qboTax;
                }
            }
            return null;

        }



        public async Task SendDocumentToRecon()
        {

            _log.Info($"Start Sync Document {DateTime.Now}");
            var getAllApproveDocument = DataManager_Documents.GetAllApprovedDocument(true)
                               .GroupBy(p => new { p.AccountID, p.XeroConnectID }).ToList();


            foreach (var t in getAllApproveDocument)
            {
                var xeroDocuments = _csContext.GetXeroDocumentToBill(t.Key.AccountID.Value, t.Key.XeroConnectID.Value, true);
                Cosmic.DataLayer.XeroMaster xeroMaster = DataManager_Users.GetXeroSettings(Convert.ToInt32(t.Key.AccountID.Value));
                if (xeroDocuments.Data == null || xeroDocuments?.Data?.Count == 0)
                {
                    _log.Info($"Not found Any document for Update! {DateTime.Now}");
                }
                else
                {

                    //xeroDocuments.Data.ToList().ForEach(u => u.BillStatus = "WAP");
                    xeroDocuments.Data.ToList().ForEach(u => u.SelectToBill = true);
                    var apiXero = _csContext.GetXeroMasterByAccountAndConnectID(t.Key.AccountID.Value, t.Key.XeroConnectID.Value);

                    if (GetXeroToken(apiXero.Data))
                    {
                        List<XeroPostResponse> lstResponse = new List<XeroPostResponse>();
                        string error = string.Empty;

                        var bills = AddBill(xeroDocuments.Data.ToList(), GetXeroTax(xeroMaster.AccountID.Value, xeroMaster.XeroID), ref error, ref lstResponse, xeroMaster);


                        if (lstResponse?.Count > 0)
                        {
                            var loginMaster = DataManager_Users.GetLogin(xeroMaster.AccountID.Value);
                            if (loginMaster?.EmailAddress != null)
                            {
                                EmailSender.SendPostSuccessEmail(Settings.GetAllEmailSettings(), new List<string> { loginMaster.EmailAddress }, "Document Posted Successfully", HtmlUtility.getInvoiceTable(lstResponse), loginMaster.UserName);

                                _log.Info("Successfully sent mail..");
                            }
                            else
                                _log.Info("Email Address not found for user with AccountID==" + loginMaster.AccountID);
                        }
                    }
                    else
                    {
                        _log.Info($"Get xerotoken returned false for Account ID ==>{ xeroMaster.AccountID}");

                    }
                }
            }

        }
        private string BillStatus(int status)
        {
            string result = "";
            switch (status)
            {
                case 1:
                    result = "DRAFT";
                    break;

                case 2:
                    result = "SUBMITTED";
                    break;

                case 3:
                    result = "AUTHORISED";
                    break;

                default:
                    break;
            }
            return result;
        }

        private XeroPostResponse CreateVendorCredit(int docId, XeroDocumentLine hdr, List<XeroDocumentLine> lines, XeroTax qboTax, ref List<XeroPostResponse> lstResponse, Cosmic.DataLayer.XeroMaster xeroMaster)
        {
            _log.Info($"In Create Vendor Credit Account ID ==>{ xeroMaster.AccountID}");

            string docClassification = string.Empty;
            string invNumber = string.Empty;
            XeroPostResponse xeroPostResponse = new XeroPostResponse();
            try
            {

                CreditNote inv = new CreditNote();

                inv.Status = BillStatus(hdr.ApproveDocAs);
                inv.Contact = new Contact();
                inv.Contact.Name = hdr.XeroVendorName;

                inv.Contact.IsSupplier = true;
                inv.Contact.IsSupplier = false;
                inv.CurrencyCode = inv.Contact.DefaultCurrency;

                inv.Date = hdr.ScanInvoiceDate != null ? Convert.ToDateTime(hdr.ScanInvoiceDate) : DateTime.Now;
                inv.DueDate = hdr.ScanInvoiceDate != null ? Convert.ToDateTime(hdr.ScanInvoiceDate).AddDays(30) : DateTime.Now.AddDays(30);
                inv.LineItems = new LineItems();
                var apiDataXeroMaster = _csContext.GetXeroMasterByAccountAndConnectID(xeroMaster.AccountID.Value, xeroMaster.XeroID);
                if (apiDataXeroMaster != null)
                {
                    foreach (var ln in lines)
                    {
                        docClassification = ln.ScanDocClassification;
                        invNumber = ln.ScanRefNumber;

                        LineItem qboln = new LineItem();

                        qboln.Description = ln.ScanDescription != null ? ln.ScanDescription.Trim() : "";
                        qboln.Quantity = ln.Scan_Quantity;
                        qboln.UnitAmount = ln.ScanUnit_Price ?? 0;
                        qboln.AccountCode = ln.XeroAccountID;
                        //qboln.TaxType = qboTax.TaxID;
                        qboln.TaxAmount = ln.ScanGST ?? 0;
                        qboln.AccountCode = ln.AccountCode;
                        inv.LineItems.Add(qboln);
                    }
                    inv.Type = "ACCPAYCREDIT";

                    Repository repository = ServiceReq(apiDataXeroMaster.Data);
                    var items = repository.Create<CreditNote>(inv);
                    if (items != null)
                    {
                        xeroPostResponse = SaveResp(invNumber, hdr.XeroVendorName, "", "Success", false);
                        _csContext.UpdateXeroBillID(docId, Convert.ToString(items.CreditNoteID), string.Empty, hdr.ApproveDocAs);
                        //UploadAttachment("VendorCredit", hdr.ScanBlob_Url, items.CreditNoteID, hdr.ScanFile_Name);
                    }
                    else
                    {
                        _log.Info($"In Createvendorcredit Account ID ==>{ xeroMaster.AccountID}  got null item");

                    }
                }
            }

            catch (Exception ex)
            {
                if (string.Compare(ex.Message, "The access token has not been authorized, or has been revoked by the user", true) == 0)
                {
                    xeroPostResponse = SaveResp(invNumber, hdr.XeroVendorName, ex.Message, "Failed", true);
                    // lstResponse.Add();
                }
                else
                {
                    xeroPostResponse = SaveResp(invNumber, hdr.XeroVendorName, ex.Message, "Failed", false);
                    //lstResponse.Add();
                }
                _csContext.LogErrorToDB("Xero", "Createbill", ex.Message, ex);
                _log.Info($"In Create Bill Account ID ==>{ xeroMaster.AccountID}  {ex.Message.ToString()}");


            }
            return xeroPostResponse;
        }

        //private int BillStatusInt(string status)
        //{
        //    int result = 0;
        //    switch (status)
        //    {
        //        case "DR":
        //            result = 1;
        //            break;

        //        case "AP":
        //            result = 2;
        //            break;

        //        case "WAP":
        //            result = 3;
        //            break;

        //        default:
        //            break;
        //    }
        //    return result;
        //}

        private XeroPostResponse SaveResp(string invNumber, string supplier, string resp, string status, bool isReAuthorized)
        {
            XeroPostResponse xeroPostResponse = new XeroPostResponse();
            xeroPostResponse.InvoiceNo = invNumber;
            xeroPostResponse.Supplier = supplier;
            xeroPostResponse.ErrorMessage = resp;
            xeroPostResponse.ReponseFromXero = status;
            xeroPostResponse.IsAuthrorize = isReAuthorized;
            return xeroPostResponse;
        }

        public List<Invoice> AddBill(List<XeroDocumentLine> lstDocumentLine, XeroTax qboTax, ref string error, ref List<XeroPostResponse> lstResponse, Cosmic.DataLayer.XeroMaster xeroMaster)
        {
            _log.Info($"In Add Bill Account ID ==>{ xeroMaster.AccountID}");
            try
            {
                var disntHdr = lstDocumentLine.Where(ff => ff.SelectToBill == true && string.IsNullOrEmpty(ff.XeroVendorID) == false).Select(ff => ff.DocumentID).Distinct();
                if (disntHdr == null) return null;

                foreach (var docId in disntHdr)
                {
                    var hdr = lstDocumentLine.FirstOrDefault(ff => ff.DocumentID == docId && string.IsNullOrEmpty(ff.XeroVendorID) == false);
                    var lines = lstDocumentLine.FindAll(ff => ff.DocumentID == docId);

                    if (lines == null) continue;
                    if (lines.Count == 0) continue;
                    XeroPostResponse xeroResponse;
                    var sameInvoiceData = DataManager_Documents.GetSameInvoiceQBODocument(docId, hdr.ScanRefNumber, (int)xeroMaster.AccountID);
                    if (sameInvoiceData == null)
                    {
                        var xeroDocument = _csContext.GetXeroDocument(docId).Data;
                        if (xeroDocument.ErrorCount < errorCount)
                        {
                            if (string.Compare(StringHelper.ToString(hdr.ScanDocType).Trim().ToUpper(), "CREDITNOTE", true) == 0)
                            {
                                xeroResponse = CreateVendorCredit(docId, hdr, lines, qboTax, ref lstResponse, xeroMaster);
                            }
                            else
                            {
                                xeroResponse = CreateBill(docId, hdr, lines, qboTax, ref lstResponse, xeroMaster);
                            }
                            if (xeroResponse.ReponseFromXero.ToLower().Equals("failed"))
                            {
                                xeroDocument = _csContext.GetXeroDocument(docId).Data;
                                if (xeroDocument.ErrorCount < errorCount)
                                {
                                    xeroDocument.ErrorCount++;
                                    _csContext.InsertXeroDocument(xeroDocument);
                                    _log.Info($"Post document failed for Invoice No: {xeroResponse.InvoiceNo}");

                                    lstResponse.Add(xeroResponse);
                                }
                                else
                                    _log.Info("Can't again post to account because limit exceeds from its higher value");


                            }
                            else
                            {
                                _log.Info($"Successfully posted document for Invoice No: {xeroResponse.InvoiceNo}");
                                lstResponse.Add(xeroResponse);
                            }
                        }
                        else
                            _log.Info("Can't again post to account because limit exceeds from its higher value");
                    }
                    else
                    {
                        var xeroDocument = _csContext.GetXeroDocument(docId).Data;
                        xeroDocument.Approved = false;
                        _csContext.InsertXeroDocument(xeroDocument);
                        xeroResponse = SaveResp(hdr.ScanRefNumber, hdr.XeroVendorName, "Same Invoice ID Exist..", "Failed", false);
                        lstResponse.Add(xeroResponse);
                        _log.Info($"Same Invoice Number Exist {hdr.ScanRefNumber}..... Post document failed for Document ID: {docId} ====> User Account : {xeroMaster.AccountID}");

                    }
                }
                return null;

            }
            catch (Exception ex)
            {
                _log.Info($"In Add Bill Account ID ==>{ xeroMaster.AccountID}   {ex.Message.ToString()}");

                _csContext.LogErrorToDB("Xero", "Createbill", ex.Message, ex);
                error = ex.Message;
                return null;
            }

        }

        private XeroPostResponse CreateBill(int docId, XeroDocumentLine hdr, List<XeroDocumentLine> lines, XeroTax qboTax, ref List<XeroPostResponse> lstResponse, Cosmic.DataLayer.XeroMaster xeroMaster)
        {
            _log.Info($"In create Bill Account ID ==>{ xeroMaster.AccountID}");

            string docClassification = string.Empty;
            XeroPostResponse xeroPostResponse = new XeroPostResponse();
            string invNumber = string.Empty;
            try
            {

                Invoice inv = new Invoice();
                inv.Type = "ACCPAY";
                inv.Status = BillStatus(hdr.ApproveDocAs);
                inv.Contact = new Contact();
                inv.Contact.Name = hdr.XeroVendorName;
                inv.Contact.IsSupplier = true;
                inv.CurrencyCode = inv.Contact.DefaultCurrency;

                inv.Date = hdr.ScanInvoiceDate != null ? Convert.ToDateTime(hdr.ScanInvoiceDate) : DateTime.Now;

                if (hdr.ScanInvoiceDate != null)
                {
                    DateTime reference = Convert.ToDateTime(hdr.ScanInvoiceDate);
                    DateTime firstDayThisMonth = new DateTime(reference.Year, reference.Month, 1);
                    DateTime firstDayPlusTwoMonths = firstDayThisMonth.AddMonths(2);
                    DateTime lastDayNextMonth = firstDayPlusTwoMonths.AddDays(-1);
                    DateTime endOfLastDayNextMonth = firstDayPlusTwoMonths.AddTicks(-1);

                    inv.DueDate = endOfLastDayNextMonth;
                }
                else
                {
                    DateTime reference = DateTime.Now;
                    DateTime firstDayThisMonth = new DateTime(reference.Year, reference.Month, 1);
                    DateTime firstDayPlusTwoMonths = firstDayThisMonth.AddMonths(2);
                    DateTime lastDayNextMonth = firstDayPlusTwoMonths.AddDays(-1);
                    DateTime endOfLastDayNextMonth = firstDayPlusTwoMonths.AddTicks(-1);

                    inv.DueDate = endOfLastDayNextMonth;
                }

                //NP
                docClassification = hdr.ScanDocClassification;
                inv.LineAmountTypes = SetupTaxCode(docClassification, hdr.ScanTaxTotal ?? 0);// GlobalTaxCalculationEnum.TaxExcluded;

                inv.LineItems = new LineItems();
                // bool isTaxExclusive = false;
                foreach (var ln in lines)
                {

                    invNumber = ln.ScanRefNumber;

                    LineItem qboln = new LineItem();

                    qboln.Description = ln.ScanDescription != null ? ln.ScanDescription.Trim() : "";
                    qboln.Quantity = ln.Scan_Quantity;

                    //NP
                    //if (inv.LineAmountTypes == LineAmountType.Inclusive)
                    //    qboln.LineAmount = ((ln.Scan_Total ?? 0) - (ln.ScanGST ?? 0));
                    //else
                    //    qboln.LineAmount = (ln.Scan_Total ?? 0);

                    qboln.UnitAmount = ln.Scan_Total ?? 0;
                    qboln.AccountCode = ln.AccountCode;

                    qboln.TaxType = qboTax.TaxID;
                    qboln.TaxAmount = ln.ScanGST ?? 0;

                    inv.LineItems.Add(qboln);
                }
                var apiDataXeroMaster = _csContext.GetXeroMasterByAccountAndConnectID(xeroMaster.AccountID.Value, xeroMaster.XeroID);

                inv.InvoiceNumber = invNumber;
                Repository repository = ServiceReq(apiDataXeroMaster.Data);
                var xeroInvoice = repository.Create<Invoice>(inv);

                if (xeroInvoice != null)
                {
                    decimal qboTotalAmount = Convert.ToDecimal(xeroInvoice.Total) - Convert.ToDecimal(hdr.ScanInvoiceTotal);
                    if (qboTotalAmount > 0)
                    {

                        LineItem qboln = new LineItem();
                        qboln.LineAmount = -qboTotalAmount;
                        qboln.AccountCode = lines[0].AccountCode;
                        qboln.TaxType = qboTax.TaxID;
                        qboln.TaxAmount = 0;
                        inv.LineItems.Add(qboln);

                        repository = ServiceReq(apiDataXeroMaster.Data);
                        xeroInvoice = repository.Create<Invoice>(inv);
                    }
                    xeroPostResponse = SaveResp(invNumber, hdr.XeroVendorName, "", "Success", false);
                    _csContext.UpdateXeroBillID(docId, Convert.ToString(xeroInvoice.InvoiceID), string.Empty, hdr.ApproveDocAs);
                    // UploadAttachment("Bill", hdr.ScanBlob_Url, xeroInvoice.InvoiceID, hdr.ScanFile_Name);
                }


            }

            catch (Exception ex)
            {
                if (string.Compare(ex.Message, "The access token has not been authorized, or has been revoked by the user", true) == 0)
                {
                    xeroPostResponse = SaveResp(invNumber, hdr.XeroVendorName, ex.Message, "Failed", true);
                }
                else
                {
                    xeroPostResponse = SaveResp(invNumber, hdr.XeroVendorName, ex.Message, "Failed", false);
                }
                _csContext.LogErrorToDB("Xero", "Createbill", ex.Message, ex);

                _log.Info($"In Create Bill Account ID ==>{ xeroMaster.AccountID}  {ex.Message.ToString()}");

            }
            return xeroPostResponse;
        }
        private LineAmountType SetupTaxCode(string docClassification, decimal totalTax)
        {

            docClassification = StringHelper.ToString(docClassification).Trim();
            bool export_header_only = false;
            switch (docClassification)
            {
                case "UNIT_PRICE_INCLUSIVE_OF_TAX_AND_CHARGES":
                case "GST_IN_TABLE_CHARGE_INCLUDES_GST":
                case "GST_IN_TABLE":
                case "NO_TABLE":
                    return LineAmountType.Inclusive;

                case "HEADER_TABLE_NO_TABLE_VALIDATION":
                case "HEADER_TABLE_TOTAL_ONLY_VALIDATION":

                    if (export_header_only || totalTax > 0)
                    {
                        return LineAmountType.Inclusive;
                    }
                    else
                    {
                        return LineAmountType.Exclusive;
                    }


                case "GST_NOT_IN_TABLE":
                case "CHARGE_INCLUDES_TAX":
                default:

                    if (export_header_only)
                    {
                        //if header only then using the totals, which include tax.
                        return LineAmountType.Inclusive;
                    }
                    else
                    {
                        //this means the line item totals need to get adjusted, with the tax added.
                        return LineAmountType.Exclusive;
                    }

            }
        }

        public async Task<QBResponse> ScanXeroDocument(XeroDocument xeroDocument)
        {
            _log.Info("EzzyScans ==> ScanXeroDocument start");
            try
            {
                var xeroMaster = DataManager_Users.GetXeroSettings(xeroDocument.AccountID);
                if (xeroMaster != null)
                {
                    int QboConnectID = (int)xeroMaster.XeroID;

                    var qboEzzyAccount = DataManager_Users.GetEzzyAccount(xeroDocument.AccountID, QboConnectID);

                    EzzyServiceNew ezzyService = new EzzyServiceNew(qboEzzyAccount.EzzyUserName, qboEzzyAccount.EzzyPassword);

                    Task<ScanInvoiceData> task = Task.Run<ScanInvoiceData>(async () => await ezzyService.GetInvoiceScanData(xeroDocument));
                    ScanInvoiceData s = task.Result;
                    if (s != null)
                    {
                        if (s.invoiceBlocksSS != null)
                        {


                            xeroDocument.ScanInvoiceTotal = s.invoiceBlocksSS.invoiceForm.invoiceTotal;
                            xeroDocument.ScanPurchaseOrder = s.invoiceBlocksSS.invoiceForm.purchaseOrder;
                            xeroDocument.ScanRefNumber = s.invoiceBlocksSS.invoiceForm.invoiceNumber;
                            xeroDocument.ScanSubTotal = s.invoiceBlocksSS.invoiceForm.subTotal;
                            xeroDocument.ScanTag = s.invoiceBlocksSS.invoiceForm.tag;
                            xeroDocument.ScanTaxTotal = s.invoiceBlocksSS.invoiceForm.gstTotal;
                            xeroDocument.ScanVendorName = s.invoiceBlocksSS.invoiceForm.supplierName;
                            xeroDocument.ScanDocumentTotal = s.invoiceBlocksSS.invoiceForm.discountTotal;
                            xeroDocument.ScanChargeTotal = s.invoiceBlocksSS.invoiceForm.chargeTotal;
                            xeroDocument.ScanABNNumber = s.invoiceBlocksSS.invoiceForm.ABNnumber;
                            xeroDocument.ScanInvoiceDate = s.invoiceBlocksSS.invoiceForm.invoiceDate;
                            //Lines



                        }
                        if (xeroDocument.ScanVendorName != null)
                        {
                            string ven = CreateVendInXero(xeroDocument);

                        }


                        if (s.invoiceDetailsSS != null)
                        {
                            if (s.invoiceDetailsSS.invForm != null)
                            {
                                xeroDocument.ScanBlob_Url = s.invoiceDetailsSS.invForm.blob_url;


                            }
                        }

                        if (s.documentClassificationSS != null)
                        {
                            xeroDocument.ScanDocType = StringHelper.ToString(s.documentClassificationSS.doc_subtype);
                            xeroDocument.ScanDocClassification = s.documentClassificationSS.doc_classification;
                        }



                        var respInsertDocument = _csContext.InsertXeroDocument(xeroDocument);

                        if (xeroDocument.DocumentID > 0)
                        {
                            if (s.invoiceBlocksSS.table.Length > 0)
                            {
                                List<XeroDocumentLine> lstQboDocumentLine = new List<XeroDocumentLine>();

                                foreach (var line in s.invoiceBlocksSS.table)
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

                                xeroDocument.DocumentLine = lstQboDocumentLine;

                                _csContext.InsertXeroDocumentLine(xeroDocument.DocumentLine, xeroDocument.DocumentID, xeroDocument.AccountID, xeroDocument.XeroConnectID);

                            }
                        }

                        //if (respInsertDocument.StatusCode == DBResponseStatusCode.OK && Convert.ToInt32(respInsertDocument.Data) > 0)
                        //{
                        //    qboDocument.DocumentID = respInsertDocument.Data;
                        //}

                        //int DocumentID = Int32.Parse(_csContext.InsertQboDocument(qboDocument).ToString());

                    }
                }
                string vendor = xeroDocument.XeroVendorName;
                if (String.IsNullOrEmpty(vendor))
                    vendor = "Unknown Vendor";

                _log.Info($"In Scan document Account ID ==>{ xeroDocument.AccountID} success");
                return GetQBResponse(xeroDocument.ScanRefNumber, vendor, "Scan Success", true, xeroDocument.UploadedDate);


            }
            catch (Exception ex)
            {
                _log = CosmicLogger.SetLog();
                _log.Error(ex);
                _log.Info($"In Scan document Account ID ==>{ xeroDocument.AccountID}");

                _csContext.LogErrorToDB("Scan", "ScanQboDocument", ex.Message, ex);
                return GetQBResponse(xeroDocument.ScanRefNumber, xeroDocument.ScanVendorName, ex.Message, false, xeroDocument.UploadedDate);


            }

        }

        public string CreateVendInXero(XeroDocument qboDocument)
        {
            XeroMaster xeroMaster = _csContext.GetXeroMasterByAccountID(qboDocument.AccountID).Data.FirstOrDefault();

            var log = CosmicLogger.SetLog();
            try
            {


                log.Info(qboDocument.ScanVendorName);
                string ezVendorName = StringHelper.ToString(qboDocument.ScanVendorName).Trim();
                string error = string.Empty;

                if (!string.IsNullOrEmpty(ezVendorName) && !string.IsNullOrWhiteSpace(ezVendorName))
                {
                    log.Info("Step 2" + ezVendorName);

                    string[] splitEZVendorName = ezVendorName.Split(new char[] { '-' });
                    if (splitEZVendorName != null)
                    {
                        if (splitEZVendorName.Length >= 3)
                        {
                            qboDocument.XeroVendorID = ezVendorName;
                            if (!String.IsNullOrEmpty(ezVendorName))
                            {
                                String vendorName = _csContext.GetXeroVendorByVendorID(xeroMaster.AccountID, ezVendorName).Data?.DisplayNameField;
                                if (!String.IsNullOrEmpty(vendorName))
                                    qboDocument.XeroVendorName = vendorName;
                            }

                            log.Info("Step 3 due -");
                            return string.Empty;
                        }
                    }
                    //Check if vendor name acatully contains id of supplier
                    //int scanVendorID = 0;

                    //if (int.TryParse(ezVendorName, out scanVendorID))
                    //{
                    //    qboDocument.XeroVendorID = scanVendorID.ToString();
                    //    return string.Empty;
                    //}

                    // Check if vendor name exist in QBO then gets id as well
                    Contact qboVend = GetVendor(ezVendorName, ref error, xeroMaster);
                    if (qboVend != null)
                    {
                        log.Info("Step 4 - GetVendor not null ");

                        qboDocument.XeroVendorID = Convert.ToString(qboVend.ContactID);
                        qboDocument.XeroVendorName = qboVend.Name;

                        log.Info("Step 4 - retruning Contact ID  " + Convert.ToString(qboVend.ContactID));
                        return Convert.ToString(qboVend.ContactID);
                    }
                }

                string ezVendorNameToCreate = string.Empty;

                //In case of empty treat it as Unknown Supplier
                if (string.IsNullOrEmpty(ezVendorName) || string.IsNullOrWhiteSpace(ezVendorName))
                {
                    log.Info("Step 5 - Checking unknown supplier");
                    Contact xeroVend = GetVendor("Unknown_Supplier", ref error, xeroMaster);
                    if (xeroVend != null)
                    {
                        log.Info("Step 6 - Checking unknown supplier not null");

                        qboDocument.XeroVendorID = Convert.ToString(xeroVend.ContactID);
                        qboDocument.XeroVendorName = xeroVend.Name;

                        var lst = _csContext.GetXeroVendor(qboDocument.AccountID, qboDocument.XeroConnectID);
                        var trackItem = lst.Data.FirstOrDefault(xx => xx.XeroVendorID == qboDocument.XeroVendorID);
                        if (trackItem == null)
                        {
                            log.Info("Step 7 - trackItem ==null Saving SaveXeroVendorUnKnow");
                            _csContext.SaveXeroVendorUnKnow(qboDocument.XeroVendorID, qboDocument.XeroVendorName, qboDocument.AccountID, qboDocument.XeroConnectID);
                        }
                        else
                        {
                            log.Info("Step 8 - trackItem != null Saving SaveXeroVendorUnKnow");
                            _csContext.SaveXeroVendorUnKnow(trackItem.XeroVendorID, trackItem.DisplayNameField, qboDocument.AccountID, qboDocument.XeroConnectID);
                        }
                        return Convert.ToString(xeroVend.ContactID);
                    }
                }

                if (string.IsNullOrEmpty(ezVendorName) || string.IsNullOrWhiteSpace(ezVendorName))
                {
                    log.Info("Step 9 - GetValidXeroVendorName");
                    ezVendorNameToCreate = GetValidXeroVendorName(xeroMaster);
                }
                else
                {
                    ezVendorNameToCreate = ezVendorName;
                }




                //Create Vendor in QBO
                Contact vend = new Contact();
                vend.Name = ezVendorNameToCreate.Replace(":", string.Empty).Trim();
                vend.FirstName = ezVendorNameToCreate.Replace(":", string.Empty).Trim();
                vend.IsSupplier = true;
                Repository repository = ServiceReq(xeroMaster);
                var vendResp = repository.Create<Contact>(vend);

                if (vendResp != null)
                {
                    log.Info("Step 10 - repository");
                    qboDocument.XeroVendorID = Convert.ToString(vendResp.ContactID);
                    qboDocument.XeroVendorName = vendResp.Name;

                    _csContext.SaveXeroVendor(vendResp, qboDocument.AccountID, qboDocument.XeroConnectID);

                    return Convert.ToString(vendResp.ContactID);
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                log.Info("Step 11" + ex.Message);
                _csContext.LogErrorToDB("Xero", "CreateVendInXero", ex.Message, ex);

                return string.Empty;
            }

        }

        public string GetValidXeroVendorName(XeroMaster xeroMaster)
        {

            bool isValid = false;
            string error = string.Empty;
            string nameToValidate = string.Empty;
            int index = 0;
            try
            {
                while (!isValid)
                {
                    nameToValidate = (index == 0 ? "Unknown_Supplier" : "Unknown_Supplier_" + index);
                    Contact qboVend = GetVendor(nameToValidate, ref error, xeroMaster);
                    if (qboVend == null) // Its Ok to create 
                    {
                        isValid = true;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                _csContext.LogErrorToDB("XeroConext", "GetValidXeroVendorName", ex.Message, ex);
            }

            return nameToValidate;
        }


        public Contact GetVendor(string vendorName, ref string error, XeroMaster xeroMaster)
        {
            bool isSuccessfullAttempt = false;
            int noofAttempt = 0;
            Contact vend = new Contact();
            while (!isSuccessfullAttempt)
            {
                try
                {
                    Repository repository = ServiceReq(xeroMaster);
                    vend = repository.Contacts.FirstOrDefault(xx => xx.Name == vendorName);
                    isSuccessfullAttempt = true;
                }
                catch (Exception ex)
                {
                    error = ex.Message;
                }

                if (noofAttempt > 4) break;
            }

            return vend;
        }

        private bool SaveXeroRefreshedToken(AccessToken xeroAccessToken, XeroMaster xeroMaster)
        {
            try
            {
                XeroMaster _xeroMaster = new XeroMaster();
                _xeroMaster.XeroID = xeroMaster.XeroID;
                _xeroMaster.RealmId = xeroMaster.RealmId;
                _xeroMaster.AccessToken = xeroAccessToken.Token;
                _xeroMaster.RefreshToken = xeroAccessToken.TokenSecret;
                _xeroMaster.SessionHandle = xeroAccessToken.SessionHandle;
                _xeroMaster.OAuthToken = xeroMaster.OAuthToken;
                _xeroMaster.OAuthTokenSec = xeroMaster.OAuthTokenSec;
                _xeroMaster.AccessTokenExpiresIn = Convert.ToDateTime(xeroAccessToken.ExpiryDateUtc);
                _xeroMaster.RefreshTokenExpiresIn = Convert.ToDateTime(xeroAccessToken.SessionExpiryDateUtc);
                _xeroMaster.AccountID = xeroMaster.AccountID;

                xeroMaster.SessionHandle = xeroAccessToken.SessionHandle;

                var respMsater = _csContext.SaveXeroRefreshedToken(_xeroMaster);
                if (respMsater.StatusCode == DBResponseStatusCode.OK)
                {
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _csContext.LogErrorToDB("XeroContext", "SaveXeroRefreshedToken", ex.Message, ex);
                Logger log = CosmicLogger.SetLog();
                if (log != null)
                {
                    log.Error(ex);
                }
                return false;
            }
        }
        private string GetAPIUrl()
        {
            return string.Format("{0}/{1}?subscription-key={2}", ConfigurationManager.AppSettings["RAH_Product"], "V2", ConfigurationManager.AppSettings["RAH_SubscriptionKey"]);
        }


        private QBResponse GetQBResponse(string invoiceNo, string invoiceSupplier, string msg, bool status, DateTime uploadedDate)
        {

            QBResponse qbResponse = new QBResponse();
            qbResponse.InvoiceNo = invoiceNo;
            qbResponse.InvoiceSupplier = invoiceSupplier;
            qbResponse.Success = status;
            qbResponse.Message = msg;
            qbResponse.uploadedDate = uploadedDate;
            return qbResponse;

        }

        private XeroPostResponse SavePostDocResp(string invNumber, string supplier, string resp, string status)
        {
            XeroPostResponse xeroPostResponse = new XeroPostResponse();
            xeroPostResponse.InvoiceNo = invNumber;
            xeroPostResponse.Supplier = supplier;
            xeroPostResponse.ErrorMessage = resp;
            xeroPostResponse.ReponseFromXero = status;
            return xeroPostResponse;
        }


        public bool GetXeroToken(XeroMaster xeroMaster)
        {
            try
            {


                Logger log = CosmicLogger.SetLog();

                if (!IsDBTokenValid(xeroMaster.AccountID, xeroMaster.XeroID))
                {

                    var refreshTokenResp = RefreshXeroAccessToken(xeroMaster.AccountID, xeroMaster.XeroID);
                    if (refreshTokenResp != null)
                    {
                        if (string.IsNullOrEmpty(refreshTokenResp.Token))
                        {
                            log.Info("accountid:" + xeroMaster.AccountID + " xero-token:" + xeroMaster.RefreshToken);
                            return false;
                        }

                        if (SaveXeroRefreshedToken(refreshTokenResp, xeroMaster))
                        {
                            if (string.IsNullOrEmpty(xeroMaster.CompanyName))
                            {
                                var companyInfo = GetCompany(xeroMaster);
                                if (companyInfo != null)
                                {
                                    log.Info("XERO/SaveToken == Company Info Got");
                                    var respDB = _csContext.SaveXeroCompanyInfo(companyInfo, xeroMaster.AccountID, xeroMaster.RealmId);
                                }
                            }

                            return true;
                        }

                    }

                }
                else
                {
                    if (string.IsNullOrEmpty(xeroMaster.CompanyName))
                    {
                        var companyInfo = GetCompany(xeroMaster);
                        if (companyInfo != null)
                        {
                            log.Info("XERO/SaveToken == Company Info Got");
                            var respDB = _csContext.SaveXeroCompanyInfo(companyInfo, xeroMaster.AccountID, xeroMaster.RealmId);
                        }
                    }
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _csContext.LogErrorToDB("XeroContext", "GetXeroToken", ex.Message, ex);
                Logger log = CosmicLogger.SetLog();
                if (log != null)
                {
                    log.Error(ex);
                }

                return false;
            }
        }
        public Repository ServiceReq(XeroMaster xeroMaster)
        {
            _log.Info($"In Service Req Account ID ==>{ xeroMaster.AccountID}");

            try
            {
                var consumerSession = GetXeroApiPartnerSession(xeroMaster);
                //_log.Info("Access Token="+consumerSession.AccessToken.ToString());
                return new Repository(consumerSession);
            }
            catch (Exception ex)
            {
                _csContext.LogErrorToDB("XeroContext", "ServiceReq", ex.Message, ex);
                Logger log = CosmicLogger.SetLog();
                if (log != null)
                {
                    log.Error(ex);
                }
                return null;
            }
        }
        public Organisation GetCompany(XeroMaster xeroMaster)
        {
            try
            {
                Repository repository = ServiceReq(xeroMaster);
                //_log.Info(repository.)
                var company = repository.Organisation;
                return company;
            }
            catch (Exception ex)
            {
                _csContext.LogErrorToDB("Xero", "GetCompany", ex.Message, ex);
                Logger log = CosmicLogger.SetLog();
                if (log != null)
                {
                    log.Error(ex);
                }
                return null;
            }
        }

        private bool IsValidXML(string xmlStr)
        {
            try
            {
                if (!string.IsNullOrEmpty(xmlStr))
                {
                    System.Xml.XmlDocument xmlDoc = new System.Xml.XmlDocument();
                    xmlDoc.LoadXml(xmlStr);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (System.Xml.XmlException)
            {
                return false;
            }
        }

        private XeroPostResponse SaveResp(string invNumber, string supplier, string resp, string status)
        {
            XeroPostResponse xeroPostResponse = new XeroPostResponse();
            xeroPostResponse.InvoiceNo = invNumber;
            xeroPostResponse.Supplier = supplier;
            xeroPostResponse.ErrorMessage = resp;
            xeroPostResponse.ReponseFromXero = status;
            return xeroPostResponse;
        }

        private bool checkedTokenExpired(DateTime? expireIN)
        {
            if (!expireIN.HasValue)
            {
                return true;
            }

            if (expireIN > DateTime.Now)
            {
                return true;
            }
            return false;
        }

        public XeroApi.OAuth.XeroApiPartnerSession GetXeroApiPartnerSession(XeroMaster xeroMaster)
        {
            _log.Info($"In Apipartnersession Account ID ==>{ xeroMaster.AccountID}");

            try
            {
                ServicePointManager.ServerCertificateValidationCallback += RemoteCertificateValidate;

                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;

                _log.Info(certificateFilePath + "");
                X509Certificate2 x509Certificate = new X509Certificate2(certificateFilePath,
                                                ConfigurationManager.AppSettings["CtrFilePassword"],
                                                X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.MachineKeySet);

                RequestToken reqToken = new RequestToken();
                reqToken.Token = xeroMaster.OAuthToken;
                reqToken.TokenSecret = xeroMaster.OAuthTokenSec;
                reqToken.ConsumerKey = ConfigurationManager.AppSettings["XeroclientId"];

                AccessToken accessToken = new AccessToken();
                accessToken.Token = xeroMaster.AccessToken;
                accessToken.TokenSecret = xeroMaster.RefreshToken;
                accessToken.SessionHandle = xeroMaster.SessionHandle;


                InMemoryTokenRepository repo = new InMemoryTokenRepository();
                repo.SaveRequestToken(reqToken);
                repo.SaveAccessToken(accessToken);
                return new XeroApi.OAuth.XeroApiPartnerSession(ConfigurationManager.AppSettings["UserAgent"],
                                                                ConfigurationManager.AppSettings["XeroclientId"],
                                                                x509Certificate, x509Certificate, repo);
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                if (log != null)
                {
                    log.Error(ex);
                }

                return null;
            }
        }

        public AccessToken RefreshXeroAccessToken(int accountID, int xeroID)
        {

            var xeroSetting = _csContext.GetXeroMasterByAccountAndConnectID(accountID, xeroID);
            if (xeroSetting.Data != null)
            {
                try
                {
                    ServicePointManager.ServerCertificateValidationCallback += RemoteCertificateValidate;

                    System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                    //ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;


                    var consumerSession = GetXeroApiPartnerSession(xeroSetting.Data);
                    if (consumerSession == null) return null;

                    AccessToken accessToken2 = consumerSession.RenewAccessToken();
                    _log.Info("TOken:" + accessToken2.Token.ToString());
                    _log.Info("Token Secret:" + accessToken2.TokenSecret.ToString());
                    return accessToken2;
                }

                catch (Exception ex)
                {
                    _csContext.LogErrorToDB("XeroContext", "RefreshXeroAccessToken", ex.Message, ex);
                    Logger log = CosmicLogger.SetLog();
                    if (log != null)
                    {
                        log.Error(ex);
                    }
                    return null;
                }
            }
            return null;

        }

        public bool IsDBTokenValid(int AccountID, int XeroConnectID)
        {
            try
            {
                var dbXeroToken = _csContext.GetCheckXeroToken(AccountID, XeroConnectID);
                if (dbXeroToken != null)
                {
                    if (dbXeroToken.Data != null)
                    {
                        return (dbXeroToken.Data.XeroTokenMinute ?? 0) > 1;
                    }
                }
            }
            catch (Exception ex)
            {
                _csContext.LogErrorToDB("XeroContext", "IsDBTokenValid", ex.Message, ex);
                Logger log = CosmicLogger.SetLog();
                if (log != null)
                {
                    log.Error(ex);
                }

                return false;
            }

            return false;
        }




    }
}
