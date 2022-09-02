using CosmicApiModel;
using CosmicCoreApi.Helper;
using Flexis.Log;
using Intuit.Ipp.Core;
using Intuit.Ipp.Data;
using Intuit.Ipp.DataService;
using Intuit.Ipp.OAuth2PlatformClient;
using Intuit.Ipp.QueryFilter;
using Intuit.Ipp.Security;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;

namespace CosmicCoreApi
{
    public class QboContext
    {
        public class QueryServiceObject<T>
        {
            public QueryServiceObject(T value)
            {
                //genericMemberVariable = value;
            }
            public T Data { get; set; }
        }

        SessionHelper _sessionHelper = new SessionHelper();

        public static OAuth2Client oauthClient = new OAuth2Client(ConfigurationManager.AppSettings["clientId"],
                                                              ConfigurationManager.AppSettings["clientSecret"],
                                                              ConfigurationManager.AppSettings["redirectURI"],
                                                              ConfigurationManager.AppSettings["appEnvironment"]);


        public ServiceContext InitQboContext()
        {


            OAuth2RequestValidator oauthValidator = new OAuth2RequestValidator(_sessionHelper.QboAccessToken);
            ServiceContext serviceContext = new ServiceContext(_sessionHelper.QboRealmId, IntuitServicesType.QBO, oauthValidator);
            serviceContext.IppConfiguration.BaseUrl.Qbo = ConfigurationManager.AppSettings["IppConfiguration.BaseUrl.Qbo"];
            serviceContext.IppConfiguration.MinorVersion.Qbo = ConfigurationManager.AppSettings["IppConfiguration.MinorVersion.Qbo"];

            return serviceContext;
        }

        public DataService InitQboDataService()
        {
            var serviceContext = InitQboContext();
            return new DataService(serviceContext);
        }

        public QueryService<T> InitQboQueryService<T>()
        {
            var serviceContext = InitQboContext();
            QueryService<T> qService = new QueryService<T>(serviceContext);

            return qService;
        }

        public T ExecuteQueryScalar<T>(string query)
        {
            var qService = InitQboQueryService<T>();
            return qService.ExecuteIdsQuery(query).FirstOrDefault();
        }

        public List<T> ExecuteQueryList<T>(string query)
        {
            try
            {
                var qService = InitQboQueryService<T>();
                var cust = qService.ExecuteIdsQuery(query).ToList();
                return cust;
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return null;
            }
        }

        public Bill AddBill(Bill bill)
        {
            var dService = InitQboDataService();
            return dService.Add(bill);

        }

        //public void Add()
        //{
        //    //--------------------------------BILL Start --------------------------------
        //    Bill bill = new Bill();
        //    //bill.SalesTermRef = new ReferenceType();
        //    //bill.SalesTermRef.name = "Net 10";
        //    // bill.Id = "30";
        //    bill.DueDate = DateTime.Now;
        //    bill.Balance = 0;
        //    bill.TxnDate = DateTime.Now;
        //    bill.CurrencyRef = new ReferenceType();
        //    bill.CurrencyRef.name = "United States Dollar";
        //    bill.CurrencyRef.Value = "USD";
        //    bill.PrivateNote = "Opening Balance";

        //    //List<LinkedTxn> linkedList = new List<LinkedTxn>();
        //    //LinkedTxn linkedobj = new LinkedTxn();
        //    //linkedobj.TxnId = "22";
        //    //linkedobj.TxnType = "BillPaymentCheck";
        //    //linkedList.Add(linkedobj);
        //    //bill.LinkedTxn = linkedList.ToArray();

        //    List<Line> lineList = new List<Line>();
        //    Line line = new Line();
        //    //line.Id = "1";
        //    line.LineNum = "555";
        //    line.Description = "Description";
        //    line.Amount = new decimal(105.20);
        //    line.AmountSpecified = true;
        //    line.DetailType = LineDetailTypeEnum.AccountBasedExpenseLineDetail;
        //    line.DetailTypeSpecified = true;
        //    AccountBasedExpenseLineDetail accountBasedExpenseLineDetail = new AccountBasedExpenseLineDetail();
        //    //accountBasedExpenseLineDetail.CustomerRef = new ReferenceType();
        //    //accountBasedExpenseLineDetail.CustomerRef.name = "Travis Waldron";
        //    //accountBasedExpenseLineDetail.CustomerRef.Value = "26";
        //    accountBasedExpenseLineDetail.AccountRef = new ReferenceType();
        //    accountBasedExpenseLineDetail.AccountRef.name = "Advertising";// "Miscellaneous";
        //    accountBasedExpenseLineDetail.AccountRef.Value = "7";
        //    accountBasedExpenseLineDetail.BillableStatus = BillableStatusEnum.NotBillable;
        //    accountBasedExpenseLineDetail.BillableStatusSpecified = true;

        //    //accountBasedExpenseLineDetail.TaxCodeRef = new ReferenceType();
        //    //accountBasedExpenseLineDetail.TaxCodeRef.Value = "NON";

        //    //accountBasedExpenseLineDetail.TaxAmount = new decimal(1.20);
        //    //accountBasedExpenseLineDetail.TaxAmountSpecified = true;

        //    //accountBasedExpenseLineDetail.TaxInclusiveAmt = new decimal(20.30);
        //    //accountBasedExpenseLineDetail.TaxInclusiveAmtSpecified = true;

        //    //accountBasedExpenseLineDetail.ExpenseDetailLineDetailEx = new IntuitAnyType();
        //    //accountBasedExpenseLineDetail.ExpenseDetailLineDetailEx.

        //    line.AnyIntuitObject = accountBasedExpenseLineDetail;

        //    lineList.Add(line);
        //    bill.Line = lineList.ToArray();

        //    bill.VendorRef = new ReferenceType();
        //    bill.VendorRef.name = "Vendor No 1";
        //    bill.VendorRef.Value = "61";

        //    //bill.APAccountRef = new ReferenceType();
        //    //bill.APAccountRef.name = "Accounts Payable (A/P)";
        //    //bill.APAccountRef.Value = "33";
        //    bill.TotalAmt = new decimal(105.20);
        //    DataService servicebill = new DataService(serviceContext);
        //    var billqu = servicebill.Add(bill);
        //    //--------------------------------BILL END --------------------------------
        //}


        private GlobalTaxCalculationEnum SetupTaxCode(string docClassification)
        {
            switch (docClassification)
            {

                case "UNIT_PRICE_INCLUSIVE_OF_TAX_AND_CHARGES":
                case "GST_IN_TABLE_CHARGE_INCLUDES_GST":
                case "GST_IN_TABLE":
                case "NO_TABLE":
                    return GlobalTaxCalculationEnum.TaxInclusive;


                case "HEADER_TABLE_NO_TABLE_VALIDATION":
                case "HEADER_TABLE_TOTAL_ONLY_VALIDATION":
                    return GlobalTaxCalculationEnum.TaxExcluded;
                //if (qboDocumentLine.to export_header_only || totalTax > 0)
                //{
                //    invoice.GlobalTaxCalculation = GlobalTaxCalculationEnum.TaxInclusive;
                //}
                //else
                //{
                //    invoice.GlobalTaxCalculation = GlobalTaxCalculationEnum.TaxExcluded;
                //}

                case "GST_NOT_IN_TABLE":
                case "CHARGE_INCLUDES_TAX":
                    return GlobalTaxCalculationEnum.TaxExcluded;
                //default:
                //    {
                //        if (export_header_only)
                //        {
                //            //if header only then using the totals, which include tax.
                //            invoice.GlobalTaxCalculation = GlobalTaxCalculationEnum.TaxInclusive;
                //        }
                //        else
                //        {
                //            //this means the line item totals need to get adjusted, with the tax added.
                //            invoice.GlobalTaxCalculation = GlobalTaxCalculationEnum.TaxExcluded;
                //        }
                //        break;
                //    }

                default:
                    return GlobalTaxCalculationEnum.TaxExcluded;
            }
        }

        public List<Bill> AddBill(List<QboDocumentLine> lstDocumentLine, QboTax qboTax, ref string error)
        {
            CosmicContext _csContext = CosmicContext.Instance;
          
            try
            {
                var disntHdr = lstDocumentLine.Where(ff => ff.SelectToBill == true && string.IsNullOrEmpty(ff.QboVendorID) == false).Select(ff => ff.DocumentID).ToList();
                if (disntHdr == null) return null;

                foreach (var docId in disntHdr)
                {
                    var hdr = lstDocumentLine.FirstOrDefault(ff => ff.DocumentID == docId && string.IsNullOrEmpty(ff.QboVendorID) == false);
                    var lines = lstDocumentLine.FindAll(ff => ff.DocumentID == docId);

                    if (lines == null) continue;
                    if (lines.Count == 0) continue;

                    if (string.Compare(hdr.ScanDocType, "CreditNote", true) == 0)
                    {
                        CreateVendorCredit(docId, hdr, lines, qboTax);

                    }
                    else
                    {
                        CreateBill(docId, hdr, lines, qboTax);
                    }
                }

            }
            catch (Exception ex)
            {
                _csContext.LogErrorToDB("Qbo", "Createbill", ex.Message, ex);
                error = ex.Message;
                return null;
            }

            return null;
        }


        private void CreateBill(int docId, QboDocumentLine hdr, List<QboDocumentLine> lines, QboTax qboTax)
        {
            string docClassification = string.Empty;
            string invNumber = string.Empty;
            CosmicContext _csContext = CosmicContext.Instance;

            try
            {
                Bill bill = new Bill();
                bill.TxnDate = DateTime.Now;
                bill.DueDate = DateTime.Now;


                bill.VendorRef = new ReferenceType()
                {
                    name = hdr.QboVendorName,
                    Value = CSConvert.ToString(hdr.QboVendorID)
                };

                List<Line> qboLines = new List<Line>();

                foreach (var ln in lines)
                {
                    docClassification = ln.ScanDocClassification;
                    invNumber = ln.ScanRefNumber;

                    Line qboln = new Line();

                    qboln.Description = ln.ScanDescription != null ? ln.ScanDescription.Trim() : "";

                    qboln.Amount = ln.Scan_Total ?? 0;
                    qboln.AmountSpecified = true;
                    qboln.DetailType = LineDetailTypeEnum.AccountBasedExpenseLineDetail;
                    qboln.DetailTypeSpecified = true;
                    AccountBasedExpenseLineDetail accountBasedExpenseLineDetail = new AccountBasedExpenseLineDetail();

                    accountBasedExpenseLineDetail.AccountRef = new ReferenceType();
                    accountBasedExpenseLineDetail.AccountRef.name = ln.QboAccountName;
                    accountBasedExpenseLineDetail.AccountRef.Value = ln.QboAccountID ?? string.Empty;

                    accountBasedExpenseLineDetail.BillableStatus = BillableStatusEnum.NotBillable;
                    accountBasedExpenseLineDetail.BillableStatusSpecified = true;

                    accountBasedExpenseLineDetail.TaxCodeRef = new ReferenceType();
                    accountBasedExpenseLineDetail.TaxCodeRef.name = qboTax.TaxName; // "GST on purchases";
                    accountBasedExpenseLineDetail.TaxCodeRef.Value = qboTax.TaxID; // "36";
                    accountBasedExpenseLineDetail.TaxAmount = ln.ScanGST ?? 0;
                    accountBasedExpenseLineDetail.TaxAmountSpecified = true;

                    qboln.AnyIntuitObject = accountBasedExpenseLineDetail;
                    qboLines.Add(qboln);
                }

                bill.GlobalTaxCalculation = SetupTaxCode(docClassification);// GlobalTaxCalculationEnum.TaxExcluded;
                bill.GlobalTaxCalculationSpecified = true;


                bill.Line = qboLines.ToArray();
                bill.DocNumber = invNumber;

                var dService = InitQboDataService();
                var billQuery = dService.Add(bill);

                if (billQuery != null)
                {
                    _csContext.UpdateQboBillID(docId, billQuery.Id, string.Empty);

                    UploadAttachment("Bill", hdr.ScanBlob_Url, billQuery.Id);
                }
            }
            catch (Exception ex)
            {
                _csContext.LogErrorToDB("Qbo", "Createbill", ex.Message, ex);
            }
        }

        private void CreateVendorCredit(int docId, QboDocumentLine hdr, List<QboDocumentLine> lines, QboTax qboTax)
        {
            string docClassification = string.Empty;
            string invNumber = string.Empty;
            CosmicContext _csContext = CosmicContext.Instance;

            try
            {
                VendorCredit bill = new VendorCredit();
                bill.TxnDate = DateTime.Now;
              
                bill.VendorRef = new ReferenceType()
                {
                    name = hdr.QboVendorName,
                    Value = CSConvert.ToString(hdr.QboVendorID)
                };

                List<Line> qboLines = new List<Line>();

                foreach (var ln in lines)
                {
                    docClassification = ln.ScanDocClassification;
                    invNumber = ln.ScanRefNumber;

                    Line qboln = new Line();

                    qboln.Description = ln.ScanDescription != null ? ln.ScanDescription.Trim() : "";

                    qboln.Amount = ln.Scan_Total ?? 0;
                    qboln.AmountSpecified = true;
                    qboln.DetailType = LineDetailTypeEnum.AccountBasedExpenseLineDetail;
                    qboln.DetailTypeSpecified = true;
                    AccountBasedExpenseLineDetail accountBasedExpenseLineDetail = new AccountBasedExpenseLineDetail();

                    accountBasedExpenseLineDetail.AccountRef = new ReferenceType();
                    accountBasedExpenseLineDetail.AccountRef.name = ln.QboAccountName;
                    accountBasedExpenseLineDetail.AccountRef.Value = ln.QboAccountID ?? string.Empty;

                    accountBasedExpenseLineDetail.BillableStatus = BillableStatusEnum.NotBillable;
                    accountBasedExpenseLineDetail.BillableStatusSpecified = true;

                    accountBasedExpenseLineDetail.TaxCodeRef = new ReferenceType();
                    accountBasedExpenseLineDetail.TaxCodeRef.name = qboTax.TaxName; // "GST on purchases";
                    accountBasedExpenseLineDetail.TaxCodeRef.Value = qboTax.TaxID; // "36";
                    accountBasedExpenseLineDetail.TaxAmount = ln.ScanGST ?? 0;
                    accountBasedExpenseLineDetail.TaxAmountSpecified = true;

                    qboln.AnyIntuitObject = accountBasedExpenseLineDetail;
                    qboLines.Add(qboln);
                }

                bill.GlobalTaxCalculation = SetupTaxCode(docClassification);// GlobalTaxCalculationEnum.TaxExcluded;
                bill.GlobalTaxCalculationSpecified = true;


                bill.Line = qboLines.ToArray();
                bill.DocNumber = invNumber;

                var dService = InitQboDataService();
                var billQuery = dService.Add(bill);

                if (billQuery != null)
                {

                    _csContext.UpdateQboBillID(docId, billQuery.Id, string.Empty);
                    UploadAttachment("VendorCredit", hdr.ScanBlob_Url, billQuery.Id);
                }
            }
            catch (Exception ex)
            {
                _csContext.LogErrorToDB("Qbo", "Createbill", ex.Message, ex);
            }
        }

        private void UploadAttachment(string documentType, string documentPath, string id)
        {

            try
            {
                string path = System.Web.HttpContext.Current.Server.MapPath("~/TempPdfDownload/temp.pdf");
                new FileDownloader().StreamDownload(documentPath, path);
                var dataBytes = File.ReadAllBytes(path);
                //adding bytes to memory stream   
                var dataStream = new MemoryStream(dataBytes);

                Attachable attachment = new Attachable();
                attachment.FileName = "taxinvoice.pdf";

                attachment.ContentType = "image/pdf";

                List<AttachableRef> lstAttachRef = new List<AttachableRef>();
                AttachableRef attachRef = new AttachableRef();
                attachRef.EntityRef = new ReferenceType();
                attachRef.EntityRef.Value = id;
                attachRef.EntityRef.type = documentType;

                lstAttachRef.Add(attachRef);

                attachment.AttachableRef = lstAttachRef.ToArray();

                var dService = InitQboDataService();
                var billQuery = dService.Upload(attachment, dataStream);
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                CosmicContext _csContext = CosmicContext.Instance;
                _csContext.LogErrorToDB("QboConext", "UploadAttachment", ex.Message, ex);
            }
        }


        public Vendor GetVendor(string vendorName, ref string error)
        {
            bool isSuccessfullAttempt = false;
            int noofAttempt = 0;
            Vendor vend = null;
            while (!isSuccessfullAttempt)
            {
                try
                {
                    vend = ExecuteQueryScalar<Vendor>("SELECT * FROM Vendor Where  DisplayName = '" + vendorName + "'");
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

        public string GetValidQboVendorName()
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
                    Vendor qboVend = GetVendor(nameToValidate, ref error);
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
                CosmicContext _csContext = CosmicContext.Instance;
                _csContext.LogErrorToDB("QboConext", "GetValidQboVendorName", ex.Message, ex);
            }

            return nameToValidate;
        }

        public string CreateVendInQbo(QboDocument qboDocument)
        {
            try
            {
                string ezVendorName = CSConvert.ToString(qboDocument.ScanVendorName).Trim();
                string error = string.Empty;

                if (!string.IsNullOrEmpty(ezVendorName) && !string.IsNullOrWhiteSpace(ezVendorName))
                {

                    //Check if vendor name acatully contains id of supplier
                    int scanVendorID = 0;

                    if (int.TryParse(ezVendorName, out scanVendorID))
                    {
                        qboDocument.QboVendorID = scanVendorID.ToString();
                        return string.Empty;
                    }
                    
                    // Check if vendor name exist in QBO then gets id as well
                    Vendor qboVend = GetVendor(ezVendorName, ref error);
                    if (qboVend != null)
                    {
                        qboDocument.QboVendorID = qboVend.Id;
                        qboDocument.QboVendorName = qboVend.DisplayName;
                        return qboVend.Id;
                    }
                }


                string ezVendorNameToCreate = string.Empty;

                //In case of empty treat it as Unknown Supplier
                if (string.IsNullOrEmpty(ezVendorName) || string.IsNullOrWhiteSpace(ezVendorName))
                {
                    Vendor qboVend = GetVendor("Unknown_Supplier", ref error);
                    if (qboVend != null)
                    {
                        qboDocument.QboVendorID = qboVend.Id;
                        qboDocument.QboVendorName = qboVend.DisplayName;
                        return qboVend.Id;
                    }
                }

                if (string.IsNullOrEmpty(ezVendorName) || string.IsNullOrWhiteSpace(ezVendorName))
                {
                    ezVendorNameToCreate = GetValidQboVendorName();
                }
                else
                {
                    ezVendorNameToCreate = ezVendorName;
                }


                CosmicContext _csContext = CosmicContext.Instance;

                //Create Vendor in QBO
                Vendor vend = new Vendor();
                vend.DisplayName = ezVendorNameToCreate.Replace(":", string.Empty).Trim();
                vend.CompanyName = ezVendorNameToCreate.Replace(":", string.Empty).Trim();

                var dService = InitQboDataService();
                var vendResp = dService.Add(vend);
                if (vendResp != null)
                {
                    qboDocument.QboVendorID = vendResp.Id;
                    qboDocument.QboVendorName = vendResp.DisplayName;

                    _csContext.SaveQboVendor(vendResp);
                    _csContext.UpdateIsEzUploadRequired(true);

                    return vendResp.Id;
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                CosmicContext _csContext = CosmicContext.Instance;
                _csContext.LogErrorToDB("Qbo", "CreateVendInQbo", ex.Message, ex);

                return string.Empty;
            }

        }

    }
}