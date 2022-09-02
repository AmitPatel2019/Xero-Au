using CosmicApiModel;
using CosmicCoreApi;
using CosmicCoreApi.Helper;
using CosminInv.Api.App_Start;
using Flexis.Log;
using Intuit.Ipp.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace CosmicCoreApi.Controllers
{
    [CustomAuthorize]
    public class QboController : ApiController
    {
        CosmicContext _csContext = CosmicContext.Instance;
        QboContext _qboContext = new QboContext();
        SessionHelper _sessionHelper = new SessionHelper();

        [HttpPost]
        public HttpResponseMessage SaveToken(QboMaster qboMaster)
        {
            Logger _log = CosmicLogger.SetLog();
            _log.Info("QBO/SaveToken");

            try
            {
                int qboConnectID = 0;
                var cosDBResponse = _csContext.SaveQboMaster(qboMaster, ref qboConnectID);

                if (cosDBResponse.StatusCode == DBResponseStatusCode.OK)
                {
                    _log.Info("QBO/SaveToken == OK");

                    _sessionHelper.QboAccessToken = qboMaster.AccessToken;
                    _sessionHelper.QboRealmId = qboMaster.RealmId;

                    _log.Info("QBO/SaveToken AccessToken == "+ qboMaster.AccessToken);
                    _log.Info("QBO/SaveToken RealmId == " + qboMaster.RealmId);

                    _log.Info("Sleeping");
                    System.Threading.Thread.Sleep(5000);
                    _log.Info("Awaken");

                    if(qboConnectID > 0)
                    {
                        _log.Info("QBO/SaveToken == RegisterUserForQbo: qboConnectID" + qboConnectID);
                       // var resp2 = new EzzyContext().RegisterUserForQbo(qboConnectID);

                        var companyInfo = _qboContext.ExecuteQueryScalar<CompanyInfo>("SELECT * FROM CompanyInfo");

                        if (companyInfo != null)
                        {

                            _log.Info("QBO/SaveToken == Company Info Got");

                            var resp = _csContext.SaveQboCompanyInfo(qboMaster.RealmId, companyInfo);
                            if (resp.StatusCode == DBResponseStatusCode.OK)
                            {
                                cosDBResponse.Data.CompanyName = companyInfo.CompanyName;
                                cosDBResponse.Data.LeagalName = companyInfo.CompanyFileName;
                            }
                        }

                        cosDBResponse.Data.CompanyName = companyInfo.CompanyName;
                        cosDBResponse.Data.LeagalName = companyInfo.CompanyFileName;
                    }

                    return Request.CreateResponse(HttpStatusCode.OK, cosDBResponse);
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound,HttpHelper.FormatError("Error while saving Qbo Token", cosDBResponse.Error));
                }
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                _csContext.LogErrorToDB("Qbo", "SaveToken", ex.Message, ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                      HttpHelper.FormatError("Error occured while saving Qbo Token", ex.Message));
            }
        }

        [HttpGet]
        public async Task<HttpResponseMessage> GetTax(bool isRefresh)
        {

            try
            {
                //if (!isRefresh)
                //{
                //    var cosResp = _csContext.GetQboVendor();

                //    //Check to see if Vendors needs to upload in Ezzy
                //    if (cosResp.Data != null)
                //    {
                //        if (cosResp.Data.Count > 0)
                //        {
                //            new EzzyContext().UploadQboVendorToEz(cosResp.Data as List<QboVendor>);
                //            _csContext.UpdateIsEzUploadRequired(false);

                //            return Request.CreateResponse(HttpStatusCode.OK, cosResp);
                //        }
                //    }
                //}



                bool issucess = false;
                int noofAttempt = 0;
                List<TaxCode> lstQboTax = null;
                while (!issucess)
                {
                    Logger log = CosmicLogger.SetLog();
                    log.Info("QBO/GetTax");

                    try
                    {
                        var validToken = await GetQBOToken();
                        if (!validToken)
                        {
                            return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                              HttpHelper.FormatError("Unable to get QBO Access Token", "Failed to get QBO Access Token"));
                        }

                        lstQboTax = _qboContext.ExecuteQueryList<TaxCode>("SELECT * FROM TaxCode MaxResults 1000");
                        if (lstQboTax != null)
                        {
                            if (lstQboTax.Count > 0)
                            {
                                _csContext.SaveQboTax(lstQboTax);
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

                var cosDBResponse = _csContext.GetQboVendor();

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

                _csContext.LogErrorToDB("Qbo", "GetAllVendor", ex.Message, ex);
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
                    var cosResp = _csContext.GetQboVendor();

                    //Check to see if Vendors needs to upload in Ezzy
                    if (cosResp.Data != null)
                    {
                        if (cosResp.Data.Count > 0)
                        {
                            new EzzyContext().UploadQboVendorToEz(cosResp.Data as List<QboVendor>);
                            _csContext.UpdateIsEzUploadRequired(false);

                            return Request.CreateResponse(HttpStatusCode.OK, cosResp);
                        }
                    }
                }



                bool issucess = false;
                int noofAttempt = 0;
                List<Vendor> lstQboVendor = null;
                while (!issucess)
                {
                    Logger log = CosmicLogger.SetLog();
                    log.Info("QBO/GetAllVendor");

                    try
                    {
                        var validToken = await GetQBOToken();
                        if (!validToken)
                        {
                            return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                              HttpHelper.FormatError("Unable to get QBO Access Token", "Failed to get QBO Access Token"));
                        }

                        lstQboVendor = _qboContext.ExecuteQueryList<Vendor>("SELECT * FROM Vendor MaxResults 1000");
                        if (lstQboVendor != null)
                        {
                            if (lstQboVendor.Count > 0)
                            {
                                _csContext.SaveAllQboVendor(lstQboVendor);
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

                var cosDBResponse = _csContext.GetQboVendor();

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

                _csContext.LogErrorToDB("Qbo", "GetAllVendor", ex.Message, ex);
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
                    var cosResp = _csContext.GetQboChartOfAccount();

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
                        var validToken = await GetQBOToken();
                        if (!validToken)
                        {
                            return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                              HttpHelper.FormatError("Unable to get QBO Access Token", "Failed to get QBO Access Token"));
                        }

                        lstQboAccount = _qboContext.ExecuteQueryList<Account>("SELECT * FROM Account MaxResults 1000");
                        if (lstQboAccount != null)
                        {
                            if (lstQboAccount.Count > 0)
                            {
                                _csContext.SaveAllQboChartOfAccount(lstQboAccount);
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

                var cosDBResponse = _csContext.GetQboChartOfAccount();
                return Request.CreateResponse(HttpStatusCode.OK, cosDBResponse);
            }
            catch (Exception ex)
            {
                Logger _log = CosmicLogger.SetLog();
                _log.Error(ex);

                _csContext.LogErrorToDB("Qbo", "GetAllAccount", ex.Message, ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                      HttpHelper.FormatError("Error occured while getting vendors", ex.Message));
            }

        }

        [HttpGet]
        public HttpResponseMessage GetByAccountID()
        {
            try
            {
                var cosDBResponse = _csContext.GetQboMasterByAccountID();

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


            // Check whether the POST operation is MultiPart?
            //if (!Request.Content.IsMimeMultipartContent())
            //{
            //    throw new HttpResponseException(HttpStatusCode.Accepted);
            //}

            // Prepare CustomMultipartFormDataStreamProvider in which our multipart form
            // data will be loaded.
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

        #region Method - QBO Access Token

        private async Task<bool> GetQBOToken()
        {
            SessionHelper sessionHelper = new SessionHelper();

            if (CSConvert.ToString(HttpContext.Current.Items["PlatformID"]) == "2") //Qbo Connetion
            {
                var resp = CosmicContext.Instance.GetQboMasterByAccountAndConnectID();
                if (resp.StatusCode == DBResponseStatusCode.OK)
                {
                    if (resp.Data != null)
                    {
                        HttpContext.Current.Items["QboRealmId"] = resp.Data.RealmId;
                        HttpContext.Current.Items["QboAccessToken"] = resp.Data.AccessToken;
                        HttpContext.Current.Items["QboRefreshToken"] = resp.Data.RefreshToken;

                        HttpContext.Current.Items["QboAccessTokenExpiresIn"] = resp.Data.AccessTokenExpiresIn;
                        HttpContext.Current.Items["QboRefreshTokenExpiresIn"] = resp.Data.RefreshTokenExpiresIn;

                        if (resp.Data.AccessTokenExpiresIn != null)
                        {
                            if (DateTime.Now > Convert.ToDateTime(resp.Data.AccessTokenExpiresIn).AddMinutes(-2))
                            {
                                //Token Expired needs to get token
                                var tokenResp = await QboContext.oauthClient.RefreshTokenAsync(resp.Data.RefreshToken);
                                if (tokenResp != null)
                                {
                                    HttpContext.Current.Items["QboAccessToken"] = tokenResp.AccessToken;
                                    HttpContext.Current.Items["QboRefreshToken"] = tokenResp.RefreshToken;

                                    QboMaster qboMaster = new QboMaster();
                                    qboMaster.RealmId = resp.Data.RealmId;
                                    qboMaster.AccountID = sessionHelper.AccountID;
                                    qboMaster.AccessTokenExpiresIn = DateTime.Now.AddSeconds(tokenResp.AccessTokenExpiresIn);
                                    qboMaster.RefreshTokenExpiresIn = DateTime.Now.AddSeconds(tokenResp.RefreshTokenExpiresIn);
                                    qboMaster.AccessToken = tokenResp.AccessToken;
                                    qboMaster.RefreshToken = tokenResp.RefreshToken;

                                    int qbConnectID = 0;
                                    CosmicContext.Instance.SaveQboMaster(qboMaster, ref qbConnectID);

                                    return true;
                                }

                            }
                        }

                        return true;
                    }
                }
            }
            return false;
        }


        
        [HttpPost]
        public HttpResponseMessage UpdateQboDocument(QboDocumentLine qboDocumentLine)
        {

            try
            {
                 _csContext.UpdateQboDocumentHdr(qboDocumentLine);
                var cosDBResponse = _csContext.UpdateQboDocumentLine(qboDocumentLine);

                if (!string.IsNullOrEmpty(qboDocumentLine.QboVendorID) && !string.IsNullOrEmpty(qboDocumentLine.QboAccountID))
                {
                    try
                    {
                        List<QboVendor> lstQboVendor = new List<QboVendor>();
                        QboVendor qboVendor = new QboVendor();
                        qboVendor.QboVendorID = qboDocumentLine.QboVendorID;
                        qboVendor.QboAccountID = qboDocumentLine.QboAccountID;
                        lstQboVendor.Add(qboVendor);
                        _csContext.SaveAllQboVendAcctDefault(lstQboVendor);
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
                _csContext.LogErrorToDB("Qbo", "UpdateQboDocument", ex.Message, ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                      HttpHelper.FormatError("Error occured while Approving Document", ex.Message));
            }

        }



        [HttpPost]
        public HttpResponseMessage ApproveQboDocument(QboDocumentLine qboDocumentLine)
        {

            try
            {
                var cosDBResponse  = _csContext.ApproveQboDocument(qboDocumentLine);
               
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
                _csContext.LogErrorToDB("Qbo", "ApproveQboDocument", ex.Message, ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                      HttpHelper.FormatError("Error occured while Approving Document", ex.Message));
            }

        }

        //[HttpGet]
        //public HttpResponseMessage GetAllDbQboVendor()
        //{
        //    try
        //    {
        //        var cosDBResponse = _csContext.GetQboVendor();

        //        if (cosDBResponse.StatusCode == DBResponseStatusCode.OK)
        //        {
        //            return Request.CreateResponse(HttpStatusCode.OK, cosDBResponse);
        //        }
        //        else
        //        {
        //            return Request.CreateErrorResponse(HttpStatusCode.NotFound,
        //                      HttpHelper.FormatError("Error while fetching default accounts", cosDBResponse.Error));
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger log = CosmicLogger.SetLog();
        //        log.Error(ex);
        //        _csContext.LogErrorToDB("Qbo", "GetVendAcctDefault", ex.Message, ex);
        //        return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
        //              HttpHelper.FormatError("Error occured while fetching default accounts", ex.Message));
        //    }

        //}

        [HttpPost]
        public HttpResponseMessage SaveAllVendAcctDefault(ICollection<QboVendor> lstQboVendAcctDefault)
        {
            try
            {
                var cosDBResponse = _csContext.SaveAllQboVendAcctDefault(lstQboVendAcctDefault);

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
        public HttpResponseMessage SaveVendAcctDefault(QboVendor qboVendAcctDefault)
        {
            try
            {
                var cosDBResponse = _csContext.SaveQboVendAcctDefault(qboVendAcctDefault);

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

        [HttpPost]
        public async Task<HttpResponseMessage> CreateBill(List<QboDocumentLine> lstQboDocumentLine)
        {
            try
            {
                QboTax qboTax = null;
                var qboTaxs = _csContext.GetQboTax();
                if (qboTaxs != null)
                {
                    if (qboTaxs.Data != null)
                    {
                        qboTax = qboTaxs.Data.FirstOrDefault(ff => ff.TaxName == "GST on purchases");
                    }

                    if(qboTax == null)
                    {
                        qboTax = qboTaxs.Data.FirstOrDefault(ff => ff.TaxName == "GST");
                    }
                }

                if (qboTax == null) {
                    qboTax = new QboTax() { TaxName = "GST on purchases", TaxID = "36" };
                }

                var validToken = await GetQBOToken();
                if (!validToken)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                      HttpHelper.FormatError("Unable to get QBO Access Token", "Failed to get QBO Access Token"));
                }

                string error = string.Empty;

                var bills = _qboContext.AddBill(lstQboDocumentLine, qboTax, ref error);

                if (string.IsNullOrEmpty(error))
                {
                    var data = CosDBResponse<ICollection<Bill>>.CreateDBResponse(bills, string.Empty);

                    return Request.CreateResponse(HttpStatusCode.OK, data);
                }

                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                   HttpHelper.FormatError("Error occured while creating bill", error));
                }
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                _csContext.LogErrorToDB("Qbo", "Createbill", ex.Message, ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                      HttpHelper.FormatError("Error occured while creating bill", ex.Message));
            }

        }

        [HttpGet]
        public HttpResponseMessage GetQboHistoryDocument()
        {
            try
            {
                var cosDBResponse = _csContext.GetQboDocumentHistory(10, 2018);

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
        public HttpResponseMessage DeleteQboDocument(QboDocument qboDocument)
        {
            try
            {
                var cosDBResponse = _csContext.DeleteQboDocument(qboDocument);

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
                _csContext.LogErrorToDB("Qbo", "DeleteQboDocument", ex.Message, ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                      HttpHelper.FormatError("Error occured while deleting document", ex.Message));
            }
        }

        [HttpPost]
        public HttpResponseMessage SaveQboDocumentEditChanges(List<QboDocumentLine> lstQboDocument)
        {
            try
            {
                var cosDBResponse = _csContext.SaveQboDocumentEditChanges(lstQboDocument);

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
                _csContext.LogErrorToDB("Qbo", "DeleteQboDocument", ex.Message, ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                      HttpHelper.FormatError("Error occured while deleting document", ex.Message));
            }
        }
        
        #endregion


    }
}
