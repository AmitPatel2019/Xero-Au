using Cosmic.DataLayer.Logic;
using CosmicApiModel;
using CosmicCoreApi.AccessContext;
using CosmicCoreApi.CustomModel;
using CosmicCoreApi.Helper;
using CosminInv.Api.App_Start;
using Flexis.Log;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Xero.NetStandard.OAuth2.Model.Accounting;

namespace CosmicCoreApi.Controllers
{
    [CustomAuthorize]
    public class XeroController : ApiController
    {
        CosmicContext _csContext =  CosmicContext.Instance;
        XeroContext _xeroContext = new XeroContext();
        SessionHelper _sessionHelper = new SessionHelper();
        List<XeroPostResponse> lstResponse = new List<XeroPostResponse>();


        [HttpPost]
        public async Task<HttpResponseMessage> SaveToken(XeroMaster xeroMaster, string username=null)
        {
            Logger _log = CosmicLogger.SetLog();
            _log.Info("XERO/SaveToken");

            int xeroConnectID = 0;
            if (username != null)
            {
                var loginMaster = CosmicContext.Instance.GetLoginMasterByUserName(username);
                var mylogin = loginMaster.Data;
                HttpContext.Current.Items["AccountID"] = mylogin.AccountID;
            }
            var cosDBResponse = _csContext.SaveXeroMaster(xeroMaster, ref xeroConnectID);

            try
            {

                if (cosDBResponse.StatusCode == DBResponseStatusCode.OK)
                {
                    cosDBResponse.Data.CompanyName = "Company";
                    cosDBResponse.Data.LeagalName = "Company";

                    _log.Info("XERO/SaveToken == OK");
                    


                    _sessionHelper.XeroToken = xeroMaster.AccessToken;
                    _sessionHelper.XeroToken_Sec = xeroMaster.RefreshToken;
                    _sessionHelper.XeroOAuth_Token = xeroMaster.OAuthToken;
                    _sessionHelper.XeroOAuth_Token_Sec = xeroMaster.OAuthTokenSec;
                    _sessionHelper.XeroConnectID = xeroConnectID;
                    _sessionHelper.XeroRealmId = xeroMaster.RealmId;
                    _sessionHelper.SessionHandle = xeroMaster.SessionHandle;
                   // _sessionHelper.AccountID = xeroMaster.AccountID;
                    _log.Info("XERO/SaveToken AccessToken == " + xeroMaster.AccessToken);


                    _log.Info("Sleeping");
                    System.Threading.Thread.Sleep(5000);
                    _log.Info("Awaken");

                    if (xeroConnectID > 0)
                    {

                        _log.Info("XERO/SaveToken == RegisterUserForXERO: xeroConnectID" + xeroConnectID);
                        var resp2 = new EzzyContext().RegisterUserForXero(xeroConnectID);

                        var companyInfo = await _xeroContext.GetCompany(xeroMaster);

                        if (companyInfo != null)
                        {

                            _log.Info("XERO/SaveToken == Company Info Got");

                            var resp = _csContext.SaveXeroCompanyInfo(companyInfo);
                            if (resp.StatusCode == DBResponseStatusCode.OK)
                            {
                                cosDBResponse.Data.CompanyName = companyInfo.Name;
                                cosDBResponse.Data.LeagalName = companyInfo.LegalName;
                            }
                        }

                        cosDBResponse.Data.CompanyName = companyInfo.Name;
                        cosDBResponse.Data.LeagalName = companyInfo.LegalName;
                    }

                    return Request.CreateResponse(HttpStatusCode.OK, cosDBResponse);
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, HttpHelper.FormatError("Error while saving Qbo Token", cosDBResponse.Error));
                }
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                _csContext.LogErrorToDB("XERO", "SaveToken", ex.Message, ex);
                return Request.CreateResponse(HttpStatusCode.OK, cosDBResponse);
                //return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                //      HttpHelper.FormatError("Error occured while saving Xero Token", ex.Message));
            }
        }

        [HttpGet]
        public HttpResponseMessage CheckXeroToken(int XeroID)
        {
            try
            {
                var cosDBResponse = _csContext.GetCheckXeroToken(_sessionHelper.AccountID, XeroID);

                if (cosDBResponse.StatusCode == DBResponseStatusCode.OK)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, cosDBResponse);
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound,
                              HttpHelper.FormatError("Error while fetching Xero Token", cosDBResponse.Error));

                }

            }
            catch (Exception ex)
            {
                Logger _log = CosmicLogger.SetLog();
                _log.Error(ex);

                _csContext.LogErrorToDB("Xero", "CheckXeroToken", ex.Message, ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                      HttpHelper.FormatError("Error occured while fetching Xero Document History", ex.Message));
            }

        }

        [HttpGet]
        public async Task<HttpResponseMessage> GetTax(bool isRefresh)
        {

            try
            {

                bool issucess = false;
                int noofAttempt = 0;
                List<TaxRate> lstQboTax = null;
                while (!issucess)
                {
                    Logger log = CosmicLogger.SetLog();
                    log.Info("Xero/GetTax");

                    try
                    {
                        var validToken = await _xeroContext.IsValidToken();
                        if (!validToken)
                        {
                            return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                              HttpHelper.FormatError("Unable to get Xero Access Token", "XERO_TOKEN_EXPIRED"));
                        }

                        lstQboTax = await _xeroContext.GetTaxCode();
                        if (lstQboTax != null)
                        {
                            if (lstQboTax.Count > 0)
                            {
                                _csContext.SaveXeroTax(lstQboTax);
                                //save all vendor to database
                                issucess = true;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Error(ex);
                    }
                    finally
                    {
                        noofAttempt++;
                    }

                    if (noofAttempt > 4) break;
                }

                var cosDBResponse = _csContext.GetXeroVendor();

                //Check to see if Vendors needs to upload in Ezzy
                if (cosDBResponse.Data != null)
                {
                    new EzzyContext().UploadQboVendorToEz(cosDBResponse.Data as List<QboVendor>);
                    _csContext.UpdateIsEzUploadRequired(false);
                }

                return Request.CreateResponse(HttpStatusCode.OK, cosDBResponse);
            }
            catch (Exception ex)
            {
                Logger _log = CosmicLogger.SetLog();
                _log.Error(ex);

                _csContext.LogErrorToDB("Xero", "GetAllVendor", ex.Message, ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                      HttpHelper.FormatError("Error occured while saving vendor default", ex.Message));
            }

        }



        [HttpGet]
        public async Task<HttpResponseMessage> GetAllVendor(bool isRefresh)
        {

            try
            {
                if (!isRefresh)
                {

                    var cosResp = _csContext.GetXeroVendor();

                    //Check to see if Vendors needs to upload in Ezzy
                    if (cosResp.Data != null)
                    {
                        if (cosResp.Data.Count > 0)
                        {
                            new EzzyContext().UploadXeroVendorToEz(cosResp.Data as List<XeroVendor>);
                            _csContext.UpdateIsEzUploadRequired(false);

                            return Request.CreateResponse(HttpStatusCode.OK, cosResp);
                        }
                    }
                }



                bool issucess = false;
                int noofAttempt = 0;
                List<Contact> lstQboVendor = null;
                while (!issucess)
                {
                    Logger log = CosmicLogger.SetLog();
                    log.Info("Xero/GetAllVendor");

                    try
                    {
                        

                        lstQboVendor = await _xeroContext.GetSupplier();
                        if (lstQboVendor != null)
                        {
                            if (lstQboVendor.Count > 0)
                            {
                                _csContext.SaveAllXeroVendor(lstQboVendor);
                                //save all vendor to database
                                issucess = true;
                            }
                        }

                        var lstQboTax = await _xeroContext.GetTaxCode();
                        if (lstQboTax != null)
                        {
                            if (lstQboTax.Count > 0)
                            {
                                _csContext.SaveXeroTax(lstQboTax);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Error(ex);
                    }
                    finally
                    {
                        noofAttempt++;
                    }

                    if (noofAttempt > 4) break;
                }

                var cosDBResponse = _csContext.GetXeroVendor();

                //Check to see if Vendors needs to upload in Ezzy
                if (cosDBResponse.Data != null)
                {
                    new EzzyContext().UploadXeroVendorToEz(cosDBResponse.Data as List<XeroVendor>);
                    _csContext.UpdateIsEzUploadRequired(false);
                }

                return Request.CreateResponse(HttpStatusCode.OK, cosDBResponse);
            }
            catch (Exception ex)
            {
                Logger _log = CosmicLogger.SetLog();
                _log.Error(ex);

                _csContext.LogErrorToDB("Xero", "GetAllVendor", ex.Message, ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                      HttpHelper.FormatError("Error occured while saving vendor default", ex.Message));
            }

        }

        [HttpGet]
        public async Task<HttpResponseMessage> GetAllAccount(bool isRefresh)
        {

            try
            {
                if (!isRefresh)
                {
                    var cosResp = _csContext.GetXeroChartOfAccount();

                    //Check to see if Vendors needs to upload in Ezzy
                    if (cosResp.Data != null)
                    {
                        if (cosResp.Data.Count > 0)
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, cosResp);
                        }
                    }
                }



                bool issucess = false;
                int noofAttempt = 0;
                List<Account> lstQboAccount = null;
                while (!issucess)
                {
                    Logger log = CosmicLogger.SetLog();
                    try
                    {
                        var validToken = await _xeroContext.IsValidToken();
                        if (!validToken)
                        {
                            return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                              HttpHelper.FormatError("Unable to get Xero Access Token", "XERO_TOKEN_EXPIRED"));
                        }

                        lstQboAccount = await _xeroContext.GetAccount();
                        if (lstQboAccount != null)
                        {
                            if (lstQboAccount.Count > 0)
                            {
                                _csContext.SaveAllXeroChartOfAccount(lstQboAccount);
                                issucess = true;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Error(ex);
                    }
                    finally
                    {
                        noofAttempt++;
                    }

                    if (noofAttempt > 4) break;
                }

                var cosDBResponse = _csContext.GetXeroChartOfAccount();
                return Request.CreateResponse(HttpStatusCode.OK, cosDBResponse);
            }
            catch (Exception ex)
            {
                Logger _log = CosmicLogger.SetLog();
                _log.Error(ex);

                _csContext.LogErrorToDB("Xero", "GetAllAccount", ex.Message, ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                      HttpHelper.FormatError("Error occured while getting vendors", ex.Message));
            }

        }

        [HttpGet]
        public HttpResponseMessage GetShortCode()
        {
            try
            {
                var cosDBResponse = _csContext.GetXeroMasterByAccountAndConnectID();
                XeroMaster lst = cosDBResponse.Data as XeroMaster;
                if (cosDBResponse.StatusCode == DBResponseStatusCode.OK)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, cosDBResponse);
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound,
                              HttpHelper.FormatError("Error while fetching xero Companies", cosDBResponse.Error));
                }
            }
            catch (Exception ex)
            {
                Logger _log = CosmicLogger.SetLog();
                _log.Error(ex);

                _csContext.LogErrorToDB("Qbo", "GetByAccountID", ex.Message, ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                      HttpHelper.FormatError("Error occured while fetching Qbo companies", ex.Message));
            }

        }

        [HttpGet]
        public HttpResponseMessage GetXeroByte()
        {
            try
            {
                var cosDBResponse = _csContext.GetXeroByte();
                if (cosDBResponse.StatusCode == DBResponseStatusCode.OK)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, cosDBResponse);
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound,
                              HttpHelper.FormatError("Error while fetching xero Companies", cosDBResponse.Error));
                }
            }
            catch (Exception ex)
            {
                Logger _log = CosmicLogger.SetLog();
                _log.Error(ex);

                _csContext.LogErrorToDB("Qbo", "GetByAccountID", ex.Message, ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                      HttpHelper.FormatError("Error occured while fetching Qbo companies", ex.Message));
            }

        }
        [CustomAuthorize]
        [HttpGet]
        public HttpResponseMessage GetByAccountID()
        {
            try
            {
                Logger _log = CosmicLogger.SetLog();
                _log.Info("GetByAccountID entered");
                var cosDBResponse = _csContext.GetXeroMasterByAccountID();

                if (cosDBResponse.StatusCode == DBResponseStatusCode.OK)
                {
                    _log.Info("GetByAccountID StatusCode Ok");
                    return Request.CreateResponse(HttpStatusCode.OK, cosDBResponse);

                }
                else
                {
                    _log.Error("GetByAccountID StatusCode "+ cosDBResponse.StatusCode);
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound,
                              HttpHelper.FormatError("Error while fetching xero Companies", cosDBResponse.Error));

                }

            }
            catch (Exception ex)
            {
                Logger _log = CosmicLogger.SetLog();
                _log.Error(ex);

                _csContext.LogErrorToDB("Qbo", "GetByAccountID", ex.Message, ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                      HttpHelper.FormatError("Error occured while fetching Qbo companies", ex.Message));
            }

        }

        [HttpPost]
        public async Task<HttpResponseMessage> PostLogo()
        {
            Logger _log = CosmicLogger.SetLog();
            _log.Info("upload logo called");

            string fileSaveLocation = HttpContext.Current.Server.MapPath("~/CompanyLogo");

            if (!Directory.Exists(fileSaveLocation))
            {
                Directory.CreateDirectory(fileSaveLocation);
            }

            List<string> files = new List<string>();

            try
            {
                // Read all contents of multipart message into CustomMultipartFormDataStreamProvider.


                string fileSaveLocation2 = HttpContext.Current.Server.MapPath("~/CompanyLogo/");
                if (!Directory.Exists(fileSaveLocation2))
                {
                    Directory.CreateDirectory(fileSaveLocation2);
                }

                CustomMultipartFormDataStreamProvider provider = new CustomMultipartFormDataStreamProvider(fileSaveLocation);
                await Request.Content.ReadAsMultipartAsync(provider);

                foreach (MultipartFileData file in provider.FileData)
                {

                    File.ReadAllBytes(file.LocalFileName);


                    files.Add(Path.GetFileName(file.LocalFileName));

                }

                //Update logo name to database
                // Send OK Response along with saved file names to the client.
                return Request.CreateResponse(HttpStatusCode.OK, files);
            }
            catch (System.Exception ex)
            {

                _log.Error(ex);
                _csContext.LogErrorToDB("Qbo", "PostLogo", ex.Message, ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        public class CustomMultipartFormDataStreamProvider : MultipartFormDataStreamProvider
        {
            public CustomMultipartFormDataStreamProvider(string path) : base(path)
            {
            }

            public override string GetLocalFileName(HttpContentHeaders headers)
            {
                return headers.ContentDisposition.FileName.Replace("\"", string.Empty);
            }
        }

        [HttpPost]
        public HttpResponseMessage UpdateXeroDocument(XeroDocumentLine XeroDocumentLine)
        {

            try
            {
                _csContext.UpdateXeroDocumentHdr(XeroDocumentLine);
                //string error = "";
                //var account = _xeroContext.GetAccountByName(XeroDocumentLine.XeroAccountID, ref error);
                //if (account != null)
                //{
                //    XeroDocumentLine.AccountCode = account.Code;
                //}
                var cosDBResponse = _csContext.UpdateXeroDocumentLine(XeroDocumentLine);

                if (!string.IsNullOrEmpty(XeroDocumentLine.XeroVendorID) && !string.IsNullOrEmpty(XeroDocumentLine.XeroAccountID))
                {
                    try
                    {
                        List<XeroVendor> lstQboVendor = new List<XeroVendor>();
                        XeroVendor qboVendor = new XeroVendor();
                        qboVendor.XeroVendorID = XeroDocumentLine.XeroVendorID;
                        qboVendor.XeroAccountID = XeroDocumentLine.XeroAccountID;
                        lstQboVendor.Add(qboVendor);
                        _csContext.SaveAllXeroVendAcctDefault(lstQboVendor);
                    }
                    catch (Exception ex)
                    {
                        Logger log = CosmicLogger.SetLog();
                        log.Error(ex);
                    }
                }

                if (cosDBResponse.StatusCode == DBResponseStatusCode.OK)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, cosDBResponse);
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound,
                              HttpHelper.FormatError("Error while approving document", cosDBResponse.Error));
                }

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                _csContext.LogErrorToDB("QbXeroo", "UpdateXeroDocument", ex.Message, ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                      HttpHelper.FormatError("Error occured while Approving Document", ex.Message));
            }

        }

        [HttpPost]
        public HttpResponseMessage UpdateReAuthrorizeByAccountID(bool isReaouth)
        {
            try
            {
                var cosDBResponse = _csContext.UpdateReAuthrorizeByAccountID(isReaouth);
                if (cosDBResponse.StatusCode == DBResponseStatusCode.OK)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, cosDBResponse);
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound,
                              HttpHelper.FormatError("Error while approving document", cosDBResponse.Error));
                }

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                _csContext.LogErrorToDB("Xero", "UpdateReAuthrorizeByAccountID", ex.Message, ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                      HttpHelper.FormatError("Error occured while Is Auth", ex.Message));
            }

        }


        [HttpPost]
        public HttpResponseMessage ApproveXeroDocument(XeroDocumentLine XeroDocumentLine)
        {

            try
            {
                if (XeroDocumentLine.SelectToBill == true)
                {
                    var result = DataManager_Documents.GetSameInvoiceQBODocument(XeroDocumentLine.DocumentID, XeroDocumentLine.ScanRefNumber, _sessionHelper.AccountID);
                    if (result == null)
                    {
                        var cosDBResponse = _csContext.ApproveXeroDocument(XeroDocumentLine);

                        if (cosDBResponse.StatusCode == DBResponseStatusCode.OK)
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, cosDBResponse);
                        }
                        else
                        {
                            return Request.CreateErrorResponse(HttpStatusCode.NotFound,
                                      HttpHelper.FormatError("Error while approving document", cosDBResponse.Error));
                        }
                    }
                    else
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.Conflict,
                                      HttpHelper.FormatError("Error while approving document", "Same InvoiceNumber Exist"));

                    }
                }
                else
                {
                    var cosDBResponse = _csContext.ApproveXeroDocument(XeroDocumentLine);

                    if (cosDBResponse.StatusCode == DBResponseStatusCode.OK)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, cosDBResponse);
                    }
                    else
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.NotFound,
                                  HttpHelper.FormatError("Error while approving document", cosDBResponse.Error));
                    }
                }


            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                _csContext.LogErrorToDB("Xero", "ApproveXeroDocument", ex.Message, ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                      HttpHelper.FormatError("Error occured while Approving Document", ex.Message));
            }

        }

        [HttpPost]
        public HttpResponseMessage SaveAllVendAcctDefault(ICollection<XeroVendor> lstQboVendAcctDefault)
        {
            try
            {
                var cosDBResponse = _csContext.SaveAllXeroVendAcctDefault(lstQboVendAcctDefault);

                if (cosDBResponse.StatusCode == DBResponseStatusCode.OK)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, cosDBResponse);
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound,
                              HttpHelper.FormatError("Error while fetching default accounts", cosDBResponse.Error));

                }

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                _csContext.LogErrorToDB("Qbo", "SaveAllVendAcctDefault", ex.Message, ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                      HttpHelper.FormatError("Error occured while fetching default accounts", ex.Message));
            }

        }

        [HttpPost]
        public HttpResponseMessage SaveVendAcctDefault(XeroVendor qboVendAcctDefault)
        {
            try
            {
                var cosDBResponse = _csContext.SaveXeroVendAcctDefault(qboVendAcctDefault);

                if (cosDBResponse.StatusCode == DBResponseStatusCode.OK)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, cosDBResponse);
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound,
                              HttpHelper.FormatError("Error while fetching default accounts", cosDBResponse.Error));

                }

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                _csContext.LogErrorToDB("Qbo", "SaveVendAcctDefault", ex.Message, ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                      HttpHelper.FormatError("Error occured while fetching default accounts", ex.Message));
            }

        }

        [HttpGet]
        public async Task<HttpResponseMessage> GetXeroProduct(string vendorID)
        {
            try
            {
                var validToken = await _xeroContext.IsValidToken();
                var cosDBResponse = _csContext.GetXeroProduct(vendorID);

                if (cosDBResponse.StatusCode == DBResponseStatusCode.OK)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, cosDBResponse);
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound,
                              HttpHelper.FormatError("Error while fetching Xero Product", cosDBResponse.Error));

                }

            }
            catch (Exception ex)
            {
                Logger _log = CosmicLogger.SetLog();
                _log.Error(ex);

                _csContext.LogErrorToDB("Xero", "GetXeroProduct", ex.Message, ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                      HttpHelper.FormatError("Error occured while fetching Xero companies", ex.Message));
            }

        }

        [HttpPost]
        public async Task<HttpResponseMessage> SaveXeroProduct(XeroProduct xeroProduct)
        {
            try
            {
                var validToken = await _xeroContext.IsValidToken();

                var cosDBResponse = _csContext.SaveXeroProduct(xeroProduct);

                if (cosDBResponse.StatusCode == DBResponseStatusCode.OK)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, cosDBResponse);
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound,
                              HttpHelper.FormatError("Error while fetching default product", cosDBResponse.Error));

                }

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                _csContext.LogErrorToDB("Xero", "SaveXeroProduct", ex.Message, ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                      HttpHelper.FormatError("Error occured while fetching default product", ex.Message));
            }

        }

        [HttpPost]
        public async Task<HttpResponseMessage> DeleteXeroProduct(SalesItem salesItem)
        {
            try
            {
                var validToken = await _xeroContext.IsValidToken();
                var cosDBResponse = _csContext.DeleteXeroProduct(salesItem);

                if (cosDBResponse.StatusCode == DBResponseStatusCode.OK)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, cosDBResponse);
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound,
                              HttpHelper.FormatError("Error while deleting document", cosDBResponse.Error));

                }

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                _csContext.LogErrorToDB("SalesItem", "DeleteSalesItem", ex.Message, ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                      HttpHelper.FormatError("Error occured while deleting document", ex.Message));
            }
        }



        private XeroTax GetXeroTax()
        {
            XeroTax qboTax = null;
            var qboTaxs = _csContext.GetXeroTax();

            switch (_sessionHelper.CountryOfOrigin)
            {
                case "AU":

                    if (qboTaxs != null)
                    {
                        if (qboTaxs.Data != null)
                        {
                            qboTax = qboTaxs.Data.FirstOrDefault(ff => CSConvert.ToString(ff.TaxName).Trim().ToUpper() == "GST ON EXPENSES");
                        }

                        if (qboTax == null)
                        {
                            qboTax = qboTaxs.Data.FirstOrDefault(ff => CSConvert.ToString(ff.TaxID).Trim().ToUpper() == "INPUT");
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
                            qboTax = qboTaxs.Data.FirstOrDefault(ff => CSConvert.ToString(ff.TaxName).Trim().ToUpper() == "GST ON EXPENSES");
                        }

                        if (qboTax == null)
                        {
                            qboTax = qboTaxs.Data.FirstOrDefault(ff => CSConvert.ToString(ff.TaxID).Trim().ToUpper() == "INPUT");
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
                            qboTax = qboTaxs.Data.FirstOrDefault(ff => CSConvert.ToString(ff.TaxName).Trim().ToUpper() == "17.5% (VAT ON EXPENSES)");
                        }

                        if (qboTax == null)
                        {
                            qboTax = qboTaxs.Data.FirstOrDefault(ff => CSConvert.ToString(ff.TaxID).Trim().ToUpper() == "INPUT");
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

        [HttpPost]
        public async Task<HttpResponseMessage> DeleteMultipleXeroDocument(ICollection<XeroDocumentLine> lstQboDocument)
        {
            try
            {
                var validToken = await _xeroContext.IsValidToken();
                if (!validToken)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                      HttpHelper.FormatError("Unable to get Xero Access Token", "XERO_TOKEN_EXPIRED"));
                }
                else
                {
                    var cosDBResponse = _csContext.DeleteMultipleXeroDocument(lstQboDocument);

                    if (cosDBResponse.StatusCode == DBResponseStatusCode.OK)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, cosDBResponse);
                    }
                    else
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.NotFound,
                                  HttpHelper.FormatError("Error while deleting document", cosDBResponse.Error));

                    }
                }

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                _csContext.LogErrorToDB("Qbo", "DeleteQboDocument", ex.Message, ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                      HttpHelper.FormatError("Error occured while deleting document", ex.Message));
            }
        }
        [HttpPost]
        [CustomAuthorize]
        public async Task<HttpResponseMessage> CreateBill(List<XeroDocumentLine> lstQboDocumentLine)
        {
            Logger log = CosmicLogger.SetLog();
            try
            {
                log.Info("CreateBill entered");
                lstResponse = new List<XeroPostResponse>();
                var validToken = await _xeroContext.IsValidToken();
                if (!validToken)
                {
                    log.Info("Unable to get Xero Access Token");
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                      HttpHelper.FormatError("Unable to get Xero Access Token", "XERO_TOKEN_EXPIRED"));
                }

                string error = string.Empty;

                var bills = await _xeroContext.AddBill(lstQboDocumentLine, GetXeroTax(),  lstResponse);

                if (string.IsNullOrEmpty(error))
                {
                    var data = CosDBResponse<ICollection<XeroPostResponse>>.CreateDBResponse(bills, string.Empty);

                    return Request.CreateResponse(HttpStatusCode.OK, data);
                }

                else
                {
                    log.Error("Error occured while creating bill");
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                   HttpHelper.FormatError("Error occured while creating bill", error));
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
                _csContext.LogErrorToDB("Xero", "Createbill", ex.Message, ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                      HttpHelper.FormatError("Error occured while creating bill", ex.Message));
            }

        }

        [HttpGet]
        public HttpResponseMessage GetXeroDocumentHistoryByDate(string fromdate, string todate)
        {
            try
            {

                var cosDBResponse = _csContext.GetXeroDocumentHistoryByDate(fromdate, todate);

                if (cosDBResponse.StatusCode == DBResponseStatusCode.OK)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, cosDBResponse);
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound,
                              HttpHelper.FormatError("Error while fetching Xero Companies", cosDBResponse.Error));

                }

            }
            catch (Exception ex)
            {
                Logger _log = CosmicLogger.SetLog();
                _log.Error(ex);

                _csContext.LogErrorToDB("Xero", "GetXeroDocumentHistoryByDate", ex.Message, ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                      HttpHelper.FormatError("Error occured while fetching Xero Document History", ex.Message));
            }

        }
        [HttpGet]
        public HttpResponseMessage GetXeroHistoryDocument()
        {
            try
            {
                var cosDBResponse = _csContext.GetXeroDocumentHistory(12, 2018);

                if (cosDBResponse.StatusCode == DBResponseStatusCode.OK)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, cosDBResponse);
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound,
                              HttpHelper.FormatError("Error while fetching Qbo Companies", cosDBResponse.Error));

                }

            }
            catch (Exception ex)
            {
                Logger _log = CosmicLogger.SetLog();
                _log.Error(ex);

                _csContext.LogErrorToDB("Qbo", "GetQboHistoryDocument", ex.Message, ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                      HttpHelper.FormatError("Error occured while fetching Qbo Document History", ex.Message));
            }

        }


        [HttpPost]
        public HttpResponseMessage DeleteXeroDocument(XeroDocument qboDocument)
        {
            try
            {
                var cosDBResponse = _csContext.DeleteXeroDocument(qboDocument);

                if (cosDBResponse.StatusCode == DBResponseStatusCode.OK)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, cosDBResponse);
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound,
                              HttpHelper.FormatError("Error while deleting document", cosDBResponse.Error));

                }

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                _csContext.LogErrorToDB("Xero", "DeleteXeroDocument", ex.Message, ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                      HttpHelper.FormatError("Error occured while deleting document", ex.Message));
            }
        }

        [HttpPost]
        public HttpResponseMessage EditScanTotal(XeroDocument qboDocument)
        {

            try
            {
                var cosDBResponse = _csContext.UpdateScanTotal(qboDocument);


                if (cosDBResponse.StatusCode == DBResponseStatusCode.OK)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, cosDBResponse);
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound,
                              HttpHelper.FormatError("Error while approving document", cosDBResponse.Error));
                }

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                _csContext.LogErrorToDB("Xero", "UpdateXeroDocument", ex.Message, ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                      HttpHelper.FormatError("Error occured while Approving Document", ex.Message));
            }

        }
        [HttpPost]
        public HttpResponseMessage DeleteXeroDocumentLine(XeroDocumentLine qboDocument)
        {
            try
            {
                var cosDBResponse = _csContext.DeleteXeroDocumentLine(qboDocument);

                if (cosDBResponse.StatusCode == DBResponseStatusCode.OK)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, cosDBResponse);
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound,
                              HttpHelper.FormatError("Error while deleting document", cosDBResponse.Error));

                }

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                _csContext.LogErrorToDB("Xero", "DeleteXeroDocument", ex.Message, ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                      HttpHelper.FormatError("Error occured while deleting document", ex.Message));
            }
        }

        [HttpPost]
        public HttpResponseMessage SaveXeroDocumentEditChanges(List<XeroDocumentLine> lstQboDocument)
        {
            try
            {
                var cosDBResponse = _csContext.SaveXeroDocumentEditChanges(lstQboDocument);

                if (cosDBResponse.StatusCode == DBResponseStatusCode.OK)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, cosDBResponse);
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                              HttpHelper.FormatError("Error while deleting document", cosDBResponse.Error));

                }

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                _csContext.LogErrorToDB("Xero", "DeleteXeroDocument", ex.Message, ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                      HttpHelper.FormatError("Error occured while deleting document", ex.Message));
            }
        }
        [HttpPost]
      //  [CustomAuthorize]
        public async Task<HttpResponseMessage> GetXeroAccessTokenByCode(XeroToken xeroToken)
        {
            try
            {
                Logger log = CosmicLogger.SetLog();
                log.Info("GetXeroAccessTokenByCode entered");
                if (xeroToken == null || string.IsNullOrEmpty(xeroToken.Code))
                {
                    log.Info("GetXeroAccessTokenByCode code is empty");
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                      HttpHelper.FormatError("unsupported Auth code", "Code error"));
                }
                var tokenResponse = await XeroTokenHelper.Instance.GetTokenByCode(xeroToken.Code);
                if(tokenResponse!=null)
                {
                    log.Info("GetXeroAccessTokenByCode tokenResponse:"+ tokenResponse.AccessToken +"{}"+ tokenResponse.ExpiresAtUtc);
                }
                else
                {
                    log.Error("GetXeroAccessTokenByCode tokenResponse null");
                }
                var xeroMaster = await XeroTokenHelper.Instance.ConvertIXeroTokenToXeroMaster(tokenResponse);
                await SaveToken(xeroMaster, xeroToken.UserName);
                return Request.CreateResponse(HttpStatusCode.OK, xeroMaster);
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                _csContext.LogErrorToDB("Xero", "DeleteXeroDocument", ex.Message, ex);
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                     HttpHelper.FormatError("error occured", ex.Message));
            }
        }

    }


}
