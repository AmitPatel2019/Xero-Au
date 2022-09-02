using Cosmic.DataLayer.Logic;
using CosmicApiModel;
using CosmicCoreApi.Helper;
using Flexis.Log;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Web;
using Xero.NetStandard.OAuth2.Api;
using Xero.NetStandard.OAuth2.Client;
using Xero.NetStandard.OAuth2.Config;
using Xero.NetStandard.OAuth2.Model.Accounting;
using Xero.NetStandard.OAuth2.Model.Identity;
using Xero.NetStandard.OAuth2.Token;
using XeroAutomationService.Common;
using static Xero.NetStandard.OAuth2.Model.Accounting.Account;

namespace CosmicCoreApi.AccessContext
{
    public class XeroContext
    {
        CosmicContext _csContext =  CosmicContext.Instance;

        private static bool RemoteCertificateValidate(

         object sender, System.Security.Cryptography.X509Certificates.X509Certificate cert,

          System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors error)
        {
            // trust any certificate!!!

            System.Console.WriteLine("Warning, trust any certificate");

            return true;
        }

        string certificateFilePath = HttpContext.Current.Server.MapPath("~") + "\\Certtificate\\public_privatekey.pfx";

       


        public async Task<List<TaxRate>> GetTaxCode()
        {
            try
            {
                var xeroToken = await XeroTokenHelper.Instance.XeroToken();
                var tax = await XeroTokenHelper.Instance.AccountingApiRepo().GetTaxRatesAsync(xeroToken.AccessToken, xeroToken.Tenants[0].TenantId.ToString());
                return tax._TaxRates;
            }
            catch (Exception ex)
            {
                _csContext.LogErrorToDB("XeroContext", "GetTaxCode", ex.Message, ex);
                Logger log = CosmicLogger.SetLog();
                if (log != null)
                {
                    log.Error(ex);
                }
                return null;
            }
        }

        public async Task<List<Account>> GetAccount()
        {
            try
            {
                var xeroToken = await XeroTokenHelper.Instance.XeroToken();
                var account = await XeroTokenHelper.Instance.AccountingApiRepo().GetAccountsAsync(xeroToken.AccessToken, xeroToken.Tenants[0].TenantId.ToString());
               // var account = repository.Accounts.Where(xx => xx.Type == "EXPENSE" || xx.Type == "DIRECTCOSTS").ToList();
                return account._Accounts;
            }
            catch (Exception ex)
            {
                _csContext.LogErrorToDB("XeroContext", "GetAccount", ex.Message, ex);
                Logger log = CosmicLogger.SetLog();
                if (log != null)
                {
                    log.Error(ex);
                }
                return null;
            }
        }


        public async Task<List<Contact>> GetSupplier()
        {
            try
            {
                var xeroToken = await XeroTokenHelper.Instance.XeroToken();
                var contacts = await XeroTokenHelper.Instance.AccountingApiRepo().GetContactsAsync(xeroToken.AccessToken, xeroToken.Tenants[0].TenantId.ToString());

                return contacts._Contacts;
            }
            catch (Exception ex)
            {
                _csContext.LogErrorToDB("Xero", "GetSupplier", ex.Message, ex);
                Logger log = CosmicLogger.SetLog();
                if (log != null)
                {
                    log.Error(ex);
                }
                return null;
            }
        }


        public async Task<Contact> GetVendor(string vendorName)
        {
            Logger log = CosmicLogger.SetLog();
            try
            {
                var xeroToken = await XeroTokenHelper.Instance.XeroToken();
                var _vend = await XeroTokenHelper.Instance.AccountingApiRepo().GetContactsAsync(xeroToken.AccessToken, xeroToken.Tenants[0].TenantId.ToString());
                var contact = _vend._Contacts.FirstOrDefault(xx => xx.Name == vendorName);
          
                return contact;
            }
            catch (Exception ex)
            {
                log.Info(ex.Message+ex.StackTrace);
                System.Diagnostics.Debug.WriteLine(ex);
            }


            return null;
        }


        public async Task<Organisation> GetCompany(XeroMaster xeroMaster= null)
        {
            try
            {
                var xeroToken = await XeroTokenHelper.Instance.XeroToken();
                var org = await XeroTokenHelper.Instance.AccountingApiRepo().GetOrganisationsAsync(xeroMaster==null? xeroToken.AccessToken: xeroMaster.AccessToken, xeroMaster==null? xeroToken.Tenants[0].TenantId.ToString(): xeroMaster.RealmId);//tenents id stored in realmin
                var company = org._Organisations;
                return company[0];
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



        public async Task<List<XeroPostResponse>> AddBill(List<XeroDocumentLine> lstDocumentLine, XeroTax qboTax, List<XeroPostResponse> _lstResponse)
        {
            List<XeroPostResponse> lstResponse = _lstResponse;
            Logger log = CosmicLogger.SetLog();
            CosmicContext _csContext =  CosmicContext.Instance;
            log.Info("AddBill entered");
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

                    if (string.Compare(CSConvert.ToString(hdr.ScanDocType).Trim().ToUpper(), "CREDITNOTE", true) == 0)
                    {
                        log.Info("CreateVendorCredit about to enter");
                        lstResponse = await CreateVendorCredit(docId, hdr, lines, qboTax);
                    }
                    else
                    {
                        log.Info("CreateBill about to enter");
                        var invoiceRs = await CreateBill(docId, hdr, lines, qboTax);
                        if (invoiceRs != null && invoiceRs.Count > 0)
                        {
                            lstResponse.Add(invoiceRs[0]);
                        }
                    }
                }
                var accId = GetXeroToken().AccountID;
                var loginMaster = DataManager_Users.GetLogin(accId);
                if (loginMaster?.EmailAddress != null)
                {
                    EmailSender.SendPostSuccessEmail(Settings.GetAllEmailSettings(), new List<string> { loginMaster.EmailAddress }, "Xero - Document Scanned Successfully", HtmlUtility.getInvoiceTable(lstResponse), loginMaster.UserName);

                }


            }
            catch (Exception ex)
            {
                log.Error(ex.StackTrace);
                _csContext.LogErrorToDB("Xero", "Createbill", ex.Message, ex);
                return lstResponse;
            }

            return lstResponse;
        }

        private CreditNote.StatusEnum BillStatus(string status)
        {
            CreditNote.StatusEnum result = CreditNote.StatusEnum.DRAFT;
            switch (status)
            {
                case "DR":
                    result = CreditNote.StatusEnum.DRAFT;
                    break;

                case "AP":
                    result = CreditNote.StatusEnum.SUBMITTED;
                    break;

                case "WAP":
                    result = CreditNote.StatusEnum.AUTHORISED;
                    break;

                default:
                    break;
            }
            return result;
        }

        private Invoice.StatusEnum InvBillStatus(string status)
        {
            Invoice.StatusEnum result = Invoice.StatusEnum.DRAFT;
            switch (status)
            {
                case "DR":
                    result = Invoice.StatusEnum.DRAFT;
                    break;

                case "AP":
                    result = Invoice.StatusEnum.SUBMITTED;
                    break;

                case "WAP":
                    result = Invoice.StatusEnum.AUTHORISED;
                    break;

                default:
                    break;
            }
            return result;
        }

        private int BillStatusInt(string status)
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

        private async Task<List<XeroPostResponse>> CreateVendorCredit(int docId, XeroDocumentLine hdr, List<XeroDocumentLine> lines, XeroTax qboTax)
        {
            List<XeroPostResponse> lstResponse = new List<XeroPostResponse>();
            Logger log = CosmicLogger.SetLog();
            log.Info("CreateVendorCredit entered");
            string docClassification = string.Empty;
            string invNumber = string.Empty;
            try
            {

                CreditNote inv = new CreditNote();

                inv.Status = BillStatus(hdr.BillStatus);
                inv.Contact = new Contact();
                inv.Contact.Name = hdr.XeroVendorName;

                inv.Contact.IsSupplier = true;
                inv.Contact.IsSupplier = false;
                inv.CurrencyCode = inv.Contact.DefaultCurrency;

                inv.Date = hdr.ScanInvoiceDate != null ? Convert.ToDateTime(hdr.ScanInvoiceDate) : DateTime.Now;
                inv.DueDate = hdr.ScanInvoiceDate != null ? Convert.ToDateTime(hdr.ScanInvoiceDate).AddDays(30) : DateTime.Now.AddDays(30);
                inv.LineItems = new List<Xero.NetStandard.OAuth2.Model.Accounting.LineItem>();
                //double invoiceTotal = 0;

                //foreach (var ln in lines)
                //{
                //    docClassification = ln.ScanDocClassification;
                //    invNumber = ln.ScanRefNumber;

                //    LineItem qboln = new LineItem();

                //    qboln.Description = ln.ScanDescription != null ? ln.ScanDescription.Trim() : "";
                //    qboln.Quantity = ln.Scan_Quantity;
                //    qboln.UnitAmount = ln.ScanUnit_Price ?? 0;
                //    qboln.AccountCode = ln.XeroAccountID;
                //    //qboln.TaxType = qboTax.TaxID;
                //    qboln.TaxAmount = ln.ScanGST ?? 0;
                //    qboln.AccountCode = ln.AccountCode;
                //    inv.LineItems.Add(qboln);
                //    invoiceTotal += ((double)(qboln.UnitAmount * qboln.Quantity) + (double)qboln.TaxAmount);

                //}
                //inv.Type = "ACCPAYCREDIT";

                //Repository repository = ServiceReq();
                //var items = repository.Create<CreditNote>(inv);

                //if (items != null)
                //{
                //    decimal qboTotalAmount = Convert.ToDecimal(items.Total) - Convert.ToDecimal(invoiceTotal);
                //    if (qboTotalAmount > 0)
                //    {
                //        //List<LineItem> LineItems = new List<LineItem>();
                //        //foreach (var item in xeroInvoice.LineItems)
                //        //{
                //        //    LineItems.Add(item);
                //        //}

                //        LineItem qboln = new LineItem();
                //        qboln.LineAmount = -qboTotalAmount;
                //        qboln.AccountCode = lines[0].AccountCode;
                //        qboln.TaxType = qboTax.TaxID;
                //        qboln.TaxAmount = 0;
                //        inv.LineItems.Add(qboln);

                //        //xeroInvoice.LineItems = new LineItems();
                //        //foreach (var item in LineItems)
                //        //{
                //        //    xeroInvoice.LineItems.Add(qboln);
                //        //}

                //        repository = ServiceReq();
                //        items = repository.Create<CreditNote>(inv);
                //    }

                //    lstResponse.Add(SaveResp(invNumber, hdr.XeroVendorName, "", "Success", false));
                //    _csContext.UpdateXeroBillID(docId, Convert.ToString(items.CreditNoteID), string.Empty, BillStatusInt(hdr.BillStatus));
                //    UploadAttachment("VendorCredit", hdr.ScanBlob_Url, items.CreditNoteID, hdr.ScanFile_Name);
                //}


                double invoiceTotal = 0;
                foreach (var ln in lines)
                {

                    invNumber = ln.ScanRefNumber;

                    LineItem qboln = new LineItem();

                    qboln.Description = ln.ScanDescription != null ? ln.ScanDescription.Trim() : "";
                    qboln.Quantity = ln.Scan_Quantity;

                    qboln.UnitAmount = ln.ScanUnit_Price ?? 0;
                    qboln.AccountCode = ln.AccountCode;

                    qboln.TaxType = qboTax.TaxID;
                    qboln.TaxAmount = ln.ScanGST ?? 0;

                    inv.LineItems.Add(qboln);
                    invoiceTotal += ((double)(qboln.UnitAmount * qboln.Quantity) + (double)qboln.TaxAmount);
                }
                inv.Type = CreditNote.TypeEnum.ACCPAYCREDIT;
                decimal qboTotalAmount = Convert.ToDecimal(invoiceTotal) - Convert.ToDecimal(hdr.ScanInvoiceTotal) - Convert.ToDecimal(hdr.ScanTaxTotal);

                if (qboTotalAmount > 0)
                {

                    LineItem qboln = new LineItem();
                    qboln.LineAmount = -qboTotalAmount;
                    qboln.AccountCode = lines[0].AccountCode;
                    qboln.TaxType = qboTax.TaxID;
                    qboln.TaxAmount = 0;
                    inv.LineItems.Add(qboln);

                }
                CreditNotes creditNotes = new CreditNotes();
                creditNotes._CreditNotes = new List<CreditNote>() { inv };
                var xeroToken = await XeroTokenHelper.Instance.XeroToken();
                log.Info("creating CreditNotes");
                var res =  await XeroTokenHelper.Instance.AccountingApiRepo().CreateCreditNotesAsync(xeroToken.AccessToken, xeroToken.Tenants[0].TenantId.ToString(), creditNotes);
                

                if (res != null)
                {
                    lstResponse.Add(SaveResp(invNumber, hdr.XeroVendorName, "", "Success", false));
                    _csContext.UpdateXeroBillID(docId, Convert.ToString(res._CreditNotes[0].CreditNoteID), string.Empty, BillStatusInt(hdr.BillStatus));
                    UploadAttachment("VendorCredit", hdr.ScanBlob_Url, res._CreditNotes[0].CreditNoteID.GetValueOrDefault(), hdr.ScanFile_Name);

                }

                //if (items != null)
                //{
                //    lstResponse.Add(SaveResp(invNumber, hdr.XeroVendorName, "", "Success", false));
                //    _csContext.UpdateXeroBillID(docId, Convert.ToString(items.CreditNoteID), string.Empty, BillStatusInt(hdr.BillStatus));
                //    UploadAttachment("VendorCredit", hdr.ScanBlob_Url, items.CreditNoteID, hdr.ScanFile_Name);
                //}
                return lstResponse;
            }

            catch (Exception ex)
            {
                log.Error(ex.StackTrace);
                if (string.Compare(ex.Message, "The access token has not been authorized, or has been revoked by the user", true) == 0)
                {
                    lstResponse.Add(SaveResp(invNumber, hdr.XeroVendorName, ex.Message, "Failed", true));
                }
                else
                {
                    lstResponse.Add(SaveResp(invNumber, hdr.XeroVendorName, ex.Message, "Failed", false));
                }
                _csContext.LogErrorToDB("Xero", "Createbill", ex.Message, ex);
            }
            return null;
        }

        private LineAmountTypes SetupTaxCode(string docClassification, decimal totalTax)
        {

            docClassification = CSConvert.ToString(docClassification).Trim();
            bool export_header_only = false;
            switch (docClassification)
            {
                case "UNIT_PRICE_INCLUSIVE_OF_TAX_AND_CHARGES":
                case "GST_IN_TABLE_CHARGE_INCLUDES_GST":
                case "GST_IN_TABLE":
                case "NO_TABLE":
                    return LineAmountTypes.Inclusive;

                case "HEADER_TABLE_NO_TABLE_VALIDATION":
                case "HEADER_TABLE_TOTAL_ONLY_VALIDATION":

                    if (export_header_only || totalTax > 0)
                    {
                        return LineAmountTypes.Inclusive;
                    }
                    else
                    {
                        return LineAmountTypes.Exclusive;
                    }


                case "GST_NOT_IN_TABLE":
                case "CHARGE_INCLUDES_TAX":
                default:

                    if (export_header_only)
                    {
                        //if header only then using the totals, which include tax.
                        return LineAmountTypes.Inclusive;
                    }
                    else
                    {
                        //this means the line item totals need to get adjusted, with the tax added.
                        return LineAmountTypes.Exclusive;
                    }

            }
        }

        private async Task<List<XeroPostResponse>> CreateBill(int docId, XeroDocumentLine hdr, List<XeroDocumentLine> lines, XeroTax qboTax)
        {
            Logger log = CosmicLogger.SetLog();
            string docClassification = string.Empty;
            string invNumber = string.Empty;
            List<XeroPostResponse> lstResponse = new List<XeroPostResponse>();
            try
            {
                log.Info("invoice build started");
                Invoice inv = new Invoice();
                inv.Type = Xero.NetStandard.OAuth2.Model.Accounting.Invoice.TypeEnum.ACCPAY;
                inv.Status = InvBillStatus(hdr.BillStatus);
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
                log.Info("set tax code");
                  inv.LineAmountTypes = SetupTaxCode(docClassification, hdr.ScanTaxTotal ?? 0);// GlobalTaxCalculationEnum.TaxExcluded;
                //inv.LineAmountTypes = LineAmountTypes.Exclusive;
                inv.LineItems = new List<LineItem>();
                // bool isTaxExclusive = false;
                double invoiceTotal = 0;
                foreach (var ln in lines)
                {

                    invNumber = ln.ScanRefNumber;

                    LineItem qboln = new LineItem();

                    qboln.Description = ln.ScanDescription != null ? ln.ScanDescription.Trim() : "";
                    qboln.Quantity = ln.Scan_Quantity;

                    qboln.UnitAmount = ln.ScanUnit_Price ?? 0;
                    qboln.AccountCode = ln.AccountCode;

                    qboln.TaxType = qboTax.TaxID;
                    qboln.TaxAmount = ln.ScanGST ?? 0;
                    qboln.LineAmount = MathHelper.TruncateDecimal(ln.Scan_Total);
                    var manualTotalCalculation = MathHelper.TruncateDecimal(ln.ScanUnit_Price.Value * ln.Scan_Quantity.Value);
                    

                    qboln = AdjustLineItem(ln, qboln, manualTotalCalculation);

                    inv.LineItems.Add(qboln);
                    if(inv.LineAmountTypes == LineAmountTypes.Inclusive)
                    {
                        invoiceTotal += (double)((double)(qboln.UnitAmount * qboln.Quantity));
                    }
                    else
                    {
                        invoiceTotal += (double)((double)(qboln.UnitAmount * qboln.Quantity) + (double)qboln.TaxAmount);
                    }

                }
                invoiceTotal = (double)MathHelper.TruncateDecimal((decimal?)invoiceTotal);
                inv.InvoiceNumber = invNumber;
                decimal qboTotalAmount = Convert.ToDecimal(invoiceTotal) - Convert.ToDecimal(hdr.ScanInvoiceTotal) - Convert.ToDecimal(hdr.ScanTaxTotal);

                if (qboTotalAmount > 0)
                {

                    LineItem qboln = new LineItem();
                    qboln.LineAmount = -qboTotalAmount;
                    qboln.AccountCode = lines[0].AccountCode;
                    qboln.TaxType = qboTax.TaxID;
                    qboln.TaxAmount = 0;
                    inv.LineItems.Add(qboln);

                }
                Invoices invoices = new Invoices();

                invoices._Invoices = new List<Invoice>() { inv};
                log.Info("XeroToken started");
                var xeroToken = await XeroTokenHelper.Instance.XeroToken();
                log.Info("Xero invoicing started");
                List<LineItem> lineItemsForUpdate = new List<LineItem>(inv.LineItems);
                var res = await XeroTokenHelper.Instance.AccountingApiRepo().CreateInvoicesAsync(xeroToken.AccessToken, xeroToken.Tenants[0].TenantId.ToString(), invoices, unitdp:4);
                log.Info("Xero invoicing completed");
                
                if (res != null)
                {
                    if (res._Invoices != null && res._Invoices.Count>0)
                    {
                        var insertedInvoiceTotal = res._Invoices[0].SubTotal + res._Invoices[0].TotalTax;
                        if (insertedInvoiceTotal != null && !res._Invoices[0].HasErrors.Value )
                        {
                            var roundval = MathHelper.TruncateDecimal(hdr.ScanInvoiceTotal);
                            if (insertedInvoiceTotal != roundval)
                            {
                                var diffAmount = Convert.ToDecimal(invoiceTotal) - insertedInvoiceTotal;
                                LineItem qboln = new LineItem();
                                qboln.Description = "Rounding Adjustment";
                                qboln.LineAmount = diffAmount;
                                qboln.AccountCode = lines[0].AccountCode;
                                qboln.TaxType = qboTax.TaxID;
                                qboln.Quantity = 1;
                                inv.LineItems.Add(qboln);
                                var updateInv = new Invoice();
                                updateInv.LineItems = lineItemsForUpdate;
                                updateInv.LineItems.Add( qboln );
                                updateInv.InvoiceID = res._Invoices[0].InvoiceID;
                                var updateInvs = new Invoices();
                                updateInvs._Invoices = new List<Invoice>(){ updateInv };
                                res = await XeroTokenHelper.Instance.AccountingApiRepo().UpdateInvoiceAsync(xeroToken.AccessToken, xeroToken.Tenants[0].TenantId.ToString(), res._Invoices[0].InvoiceID.Value, updateInvs,unitdp:4);

                            }
                        }

                        
                    }
                    log.Info("Xero invoice number:"+res.ToJson());
                    lstResponse.Add(SaveResp(invNumber, hdr.XeroVendorName, "", "Success", false));
                    _csContext.UpdateXeroBillID(docId, Convert.ToString(res._Invoices[0].InvoiceID), string.Empty, BillStatusInt(hdr.BillStatus));
                    UploadAttachment("Bill", hdr.ScanBlob_Url, res._Invoices[0].InvoiceID.GetValueOrDefault(), hdr.ScanFile_Name);
                    var attach = await CreateInvoiceAttachment(xeroToken, hdr.ScanBlob_Url, hdr.ScanFile_Name, res._Invoices[0].InvoiceID.GetValueOrDefault());
                }
              

            }

            catch (Exception ex)
            {
                log.Error(ex.Message+ex.StackTrace);
                if (string.Compare(ex.Message, "The access token has not been authorized, or has been revoked by the user", true) == 0)
                {
                    lstResponse.Add(SaveResp(invNumber, hdr.XeroVendorName, ex.Message, "Failed", true));
                }
                else
                {
                    lstResponse.Add(SaveResp(invNumber, hdr.XeroVendorName, ex.Message, "Failed", false));
                }
                _csContext.LogErrorToDB("Xero", "Createbill", ex.Message, ex);
            }

            return lstResponse;
        }

        private LineItem AdjustLineItem(XeroDocumentLine xeroDocumentLine, LineItem lineItem, decimal manualTotal)
        {
            
            if (xeroDocumentLine.Scan_Total > manualTotal)
            {
                var lineGst = xeroDocumentLine.ScanGST;
                decimal eachQntyGst = 0 ;
                if (lineGst!=null && xeroDocumentLine.Scan_Quantity!=null)
                {
                    eachQntyGst = lineGst.Value / xeroDocumentLine.Scan_Quantity.Value;
                }
                lineItem.UnitAmount = (xeroDocumentLine.ScanUnit_Price + eachQntyGst);

                var customTotal = MathHelper.TruncateDecimal( lineItem.UnitAmount * lineItem.Quantity);
                if(customTotal!= xeroDocumentLine.Scan_Total)
                {
                    lineItem.UnitAmount = MathHelper.TruncateDecimal(xeroDocumentLine.ScanUnit_Price + eachQntyGst);
                }
            }


            return lineItem;
        }

        private async Task<Attachments> CreateInvoiceAttachment(IXeroToken xeroToken, string fileurl, string fileName, Guid? invoiceID)
        {
            var body = new WebClient().DownloadData(fileurl);
         //   byte[] body = System.IO.File.ReadAllBytes(fileName);

            try
            {
                var result = await XeroTokenHelper.Instance.AccountingApiRepo().CreateInvoiceAttachmentByFileNameAsync(xeroToken.AccessToken, xeroToken.Tenants[0].TenantId.ToString(),  invoiceID.GetValueOrDefault(),fileName, body, true);
                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception when calling apiInstance.CreateInvoiceAttachmentByFileName: " + e.Message);
                return null;
            }
        }
        private XeroPostResponse SaveResp(string invNumber, string supplier, string resp, string status, bool isReAuthorized)
        {
            XeroPostResponse xeroPostResponse = new XeroPostResponse();
            xeroPostResponse.InvoiceNo = invNumber;
            xeroPostResponse.Supplier = supplier;
            xeroPostResponse.ErrorMessage = resp;
            xeroPostResponse.ReponseFromXero = status;
            xeroPostResponse.IsAuthrorize = isReAuthorized;
            xeroPostResponse.uploadedDate = DateTime.Now;
            return xeroPostResponse;
        }


        private void UploadAttachment(string documentType, string documentPath, Guid invoiceID, string scanFileName)
        {
            /*
            try
            {
                string path = System.Web.HttpContext.Current.Server.MapPath("~/TempPdfUpload/");

                //string folder = string.Format("{0}{1}", path, _sessionHelper.LoginID);
                //if (Directory.Exists(folder))
                //{
                //    Directory.Delete(string.Format("{0}", folder),true);
                //}

                //new FileDownloader().StreamDownload(documentPath, path);////download kar dega

                DateTime dt = DateTime.Now; // Or whatever
                string dateString = dt.ToString("yyyyMMddHHmmss");

                if (!Directory.Exists(string.Format("{0}{1}", path, _sessionHelper.LoginID)))
                {
                    Directory.CreateDirectory(string.Format("{0}{1}", path, _sessionHelper.LoginID));
                }

                if (!Directory.Exists(string.Format("{0}{1}//{2}", path, _sessionHelper.LoginID, dateString)))
                {
                    Directory.CreateDirectory(string.Format("{0}{1}//{2}", path, _sessionHelper.LoginID, dateString));
                }

                string filePath = string.Format("{0}{1}//{2}//{3}", path, _sessionHelper.LoginID, dateString, scanFileName);
                new FileDownloader().StreamDownload(documentPath, filePath);
                //var dataBytes = File.ReadAllBytes(path);
                ////adding bytes to memory stream   
                //var dataStream = new MemoryStream(dataBytes);

                //FileStream fs = new FileStream(path, FileMode.Open);
                //int fsLength = unchecked((int)fs.Length);

                Attachment attachment = new Attachment();
                Repository repository = ServiceReq();

                if (documentType == "Bill")
                {
                    var anyPurchasesInvoice = repository.Invoices.FirstOrDefault(it => it.Type == "ACCPAY" && it.InvoiceID == invoiceID);

                    var newAttachment = repository.Attachments.Create(anyPurchasesInvoice, new FileInfo(filePath));
                }
                //else
                //{
                //    var notes = repository.CreditNotes.FirstOrDefault(it => it.Type == "ACCPAYCREDIT" && it.CreditNoteID == invoiceID);
                //    AttachmentRepository attachmentRepository = new AttachmentRepository;
                //    var newAttachment = repository.Attachments.Create(notes, new FileInfo(filePath));
                //}
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                CosmicContext _csContext = CosmicContext.Instance;
                _csContext.LogErrorToDB("XeroUpload", "UploadAttachment", ex.Message, ex);
            } */
        }

        public async Task<string> GetValidXeroVendorName()
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
                    Contact qboVend = await GetVendor(nameToValidate);
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
                CosmicContext _csContext =  CosmicContext.Instance;
                _csContext.LogErrorToDB("XeroConext", "GetValidXeroVendorName", ex.Message, ex);
            }

            return nameToValidate;
        }

        public async Task<string> CreateVendInXero(XeroDocument qboDocument)
        {
            var log = CosmicLogger.SetLog();
            try
            {


                log.Info(qboDocument.ScanVendorName);
                string ezVendorName = CSConvert.ToString(qboDocument.ScanVendorName).Trim();
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

                            log.Info("Step 3 due -");
                            return string.Empty;
                        }
                    }

                    // Check if vendor name exist in QBO then gets id as well
                    Contact qboVend = await GetVendor(ezVendorName);
                    if (qboVend != null)
                    {
                        log.Info("Step 4 - GetVendor not null ");

                        qboDocument.XeroVendorID = Convert.ToString(qboVend.ContactID);
                        qboDocument.XeroVendorName = qboVend.Name;

                        log.Info("Step 4 - retruning Contact ID  " + Convert.ToString(qboVend.ContactID));
                        return Convert.ToString(qboVend.ContactID);
                    }
                }

                CosmicContext _csContext = CosmicContext.Instance;
                string ezVendorNameToCreate = string.Empty;

                //In case of empty treat it as Unknown Supplier
                if (string.IsNullOrEmpty(ezVendorName) || string.IsNullOrWhiteSpace(ezVendorName))
                {
                    log.Info("Step 5 - Checking unknown supplier");
                    Contact xeroVend = await GetVendor("Unknown_Supplier");
                    if (xeroVend != null)
                    {
                        log.Info("Step 6 - Checking unknown supplier not null");

                        qboDocument.XeroVendorID = Convert.ToString(xeroVend.ContactID);
                        qboDocument.XeroVendorName = xeroVend.Name;

                        var lst = _csContext.GetXeroVendor();
                        var trackItem = lst.Data.FirstOrDefault(xx => xx.XeroVendorID == qboDocument.XeroVendorID);
                        if (trackItem == null)
                        {
                            log.Info("Step 7 - trackItem ==null Saving SaveXeroVendorUnKnow");
                            _csContext.SaveXeroVendorUnKnow(qboDocument.XeroVendorID, qboDocument.XeroVendorName);
                        }
                        else
                        {
                            log.Info("Step 8 - trackItem != null Saving SaveXeroVendorUnKnow");
                            _csContext.SaveXeroVendorUnKnow(trackItem.XeroVendorID, trackItem.DisplayNameField);
                        }
                        return Convert.ToString(xeroVend.ContactID);
                    }
                }

                if (string.IsNullOrEmpty(ezVendorName) || string.IsNullOrWhiteSpace(ezVendorName))
                {
                    log.Info("Step 9 - GetValidXeroVendorName");
                    ezVendorNameToCreate = await GetValidXeroVendorName();
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

                Contacts contacts = new Contacts();
                contacts._Contacts = new List<Contact>() { vend };
                var xeroToken = await XeroTokenHelper.Instance.XeroToken();
                var res = await XeroTokenHelper.Instance.AccountingApiRepo().CreateContactsAsync(xeroToken.AccessToken,xeroToken.Tenants[0].TenantId.ToString(), contacts);
                
                if (res != null)
                {
                    log.Info("Step 10 - repository");
                    qboDocument.XeroVendorID = Convert.ToString(res._Contacts[0].ContactID);
                    qboDocument.XeroVendorName = res._Contacts[0].Name;

                    _csContext.SaveXeroVendor(res._Contacts[0]);
                    _csContext.UpdateIsEzUploadRequired(true);

                    return Convert.ToString(res._Contacts[0].ContactID);
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                log.Info("Step 11" + ex.Message);
                CosmicContext _csContext =  CosmicContext.Instance;
                _csContext.LogErrorToDB("Xero", "CreateVendInXero", ex.Message, ex);

                return string.Empty;
            }

        }
        public async Task<bool> IsValidToken()
		{
            await Task.Delay(10);//remove after testing
            var xeroAccount = CosmicContext.Instance.GetXeroMasterByAccountID();
            return xeroAccount?.Data?.ElementAt(0)!=null;
        }
        public XeroMaster GetXeroToken()
        {
            var xeroAccount = CosmicContext.Instance.GetXeroMasterByAccountID();
            return xeroAccount?.Data?.ElementAt(0);
        }
    }
}