using CosmicApiModel;
using CosmicCoreApi;
using CosmicCoreApi.AccessContext;
using CosmicCoreApi.CustomModel;
using CosmicCoreApi.EzzyService;
using CosmicCoreApi.Helper;
using CosminInv.Api.App_Start;
using Flexis.Log;
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
using System.Web.Http.Cors;

namespace CosmicCoreApi.Controllers
{

    [CustomAuthorize]
    public class ScanController : ApiController
    {
        private CosmicContext _csContext =  CosmicContext.Instance;
        private EzzyContext _ezContext = new EzzyContext();
        QboContext _qboContext = new QboContext();
        XeroContext _xeroContext = new XeroContext();
        SessionHelper _sessionHelper = new SessionHelper();

        [HttpGet]
        public HttpResponseMessage GetRCScanningAccount()
        {
            try
            {
                var cosDBResponse = _csContext.GetRCScanningCredential();

                if (cosDBResponse.Data != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, cosDBResponse);
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound,
                          HttpHelper.FormatError("Scannig credentials not found", cosDBResponse.Error));
                }
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                // Log exception code goes here 
                _csContext.LogErrorToDB("Scan", "GetRCScanningAccount", ex.Message, ex);

                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                    HttpHelper.FormatError("Error occured while getting scanning account detail", ex.Message));
            }

        }



        private static bool RemoteCertificateValidate(

        object sender, System.Security.Cryptography.X509Certificates.X509Certificate cert,

         System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors error)
        {

            // trust any certificate!!!

            System.Console.WriteLine("Warning, trust any certificate");

            return true;

        }

        private async Task<CBDocument> ScanDocument()
        {
            CBDocument cbDocument = new CBDocument();
            try
            {
                //ServicePointManager.ServerCertificateValidationCallback += RemoteCertificateValidate;

                if (!Request.Content.IsMimeMultipartContent())
                    throw new Exception();

                var provider = new MultipartMemoryStreamProvider();
                await Request.Content.ReadAsMultipartAsync(provider);

                var file = provider.Contents.First();
                var filename = file.Headers.ContentDisposition.FileName.Trim('\"');
                var buffer = await file.ReadAsByteArrayAsync();
                var stream = new MemoryStream((buffer));

                //cbDocument = await _ezContext.ScanDocument(buffer, filename);

                cbDocument = await _ezContext.UploadDocument(buffer, filename);
                return cbDocument;

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                _csContext.LogErrorToDB("Scan", "ScanDocument", ex.Message, ex);

                cbDocument.Error = ex.Message;
                return cbDocument;
            }

        }



        public HttpResponseMessage UpdateRCDocument(CBDocument cbDocument)
        {
            try
            {


                ReckonDocument rcDocument = new ReckonDocument();
                rcDocument.AccountID = _sessionHelper.AccountID;
                rcDocument.TxnID = cbDocument.CB_TxnID;
                rcDocument.ReckonError = cbDocument.Error;
                rcDocument.ReckonFileID = _sessionHelper.ReckonFileID;
                rcDocument.ReckonDocumentID = cbDocument.ReckonDocumentID;
                rcDocument.VendorName = cbDocument.CB_VendorListID;

                var cosDBResponse = _csContext.SaveReckonDocument(rcDocument);

                if (cosDBResponse.Data > 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, cosDBResponse);
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound,
                          HttpHelper.FormatError("Scannig credentials not found", cosDBResponse.Error));
                }
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                // Log exception code goes here 
                _csContext.LogErrorToDB("Scan", "UpdateRCDocument", ex.Message, ex);

                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                    HttpHelper.FormatError("Error occured while getting scanning account detail", ex.Message));
            }

        }
        [HttpGet]
        public HttpResponseMessage InsertQboJob(string documentIDs)
        {
            try
            {
                    var resp = _csContext.InsertQboJob(documentIDs);
                    return Request.CreateResponse(HttpStatusCode.OK, resp);

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                _csContext.LogErrorToDB("Scan", "InsertQboJob", ex.Message, ex);

                return Request.CreateErrorResponse(HttpStatusCode.NotFound,
                HttpHelper.FormatError("Insert Qbo Job Error", ex.Message));
            }
            finally
            {

            }
        }
        [HttpPost]
        public async Task<HttpResponseMessage> ScanRCDocument()
        {
            try
            {


                CBDocument cbDocument = await ScanDocument();
                if (cbDocument != null)
                {
                    if (cbDocument.InvoiceID > 0)
                    {
                        ReckonDocument rcDocument = new ReckonDocument();
                        rcDocument.ScanInvoiceID = cbDocument.InvoiceID;
                        rcDocument.InvoiceTotal = cbDocument.EzzyInvoiceBlocksSS.invoiceForm.invoiceTotal;
                        rcDocument.TaxTotal = cbDocument.EzzyInvoiceBlocksSS.invoiceForm.gstTotal;
                        rcDocument.InvoiceDate = cbDocument.EzzyInvoiceBlocksSS.invoiceForm.invoiceDate;
                        rcDocument.VendorName = cbDocument.EzzyInvoiceBlocksSS.invoiceForm.supplierName;
                        rcDocument.InvoiceNumber = cbDocument.EzzyInvoiceBlocksSS.invoiceForm.invoiceNumber;
                        rcDocument.ScanPdfPath = cbDocument.CB_FilePath;


                        if (cbDocument.EzzyDocumentClassificationSS != null)
                        {
                            rcDocument.InvoiceType = cbDocument.EzzyDocumentClassificationSS.doc_subtype.ToString();
                        }
                        else
                        {
                            rcDocument.InvoiceType = "";
                        }

                        var dbRes = _csContext.SaveReckonDocument(rcDocument);

                        cbDocument.ReckonDocumentID = CSConvert.ToInt(dbRes.Data);

                        var data = CosDBResponse<CBDocument>.CreateDBResponse(cbDocument, string.Empty);

                        return Request.CreateResponse(HttpStatusCode.OK, data);
                    }
                }

                var errData = CosDBResponse<CBDocument>.CreateDBResponse(cbDocument, string.Empty);

                return Request.CreateErrorResponse(HttpStatusCode.NotFound,
                         HttpHelper.FormatError("Scannig credentials not found", "could not scan your data"));

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                _csContext.LogErrorToDB("Scan", "ScanRCDocument", ex.Message, ex);

                return Request.CreateErrorResponse(HttpStatusCode.NotFound,
                       HttpHelper.FormatError("Scannig credentials not found", ex.Message));

            }
            finally
            {

            }

        }

        [HttpPost]
        public async Task<HttpResponseMessage> UploadDocument()
        {
            try
            {
                CBDocument cbDocument = await ScanDocument();
                if (cbDocument != null)
                {
                    if (cbDocument.InvoiceID > 0)
                    {
                        ReckonDocument rcDocument = new ReckonDocument();
                        rcDocument.ScanInvoiceID = cbDocument.InvoiceID;
                        rcDocument.ScanPdfPath = cbDocument.CB_FilePath;
                        rcDocument.VendorName = cbDocument.CB_VendorListID;
                        rcDocument.InvoiceType = "";


                        var dbRes = _csContext.SaveReckonDocument(rcDocument);

                        cbDocument.ReckonDocumentID = CSConvert.ToInt(dbRes.Data);

                        var data = CosDBResponse<CBDocument>.CreateDBResponse(cbDocument, string.Empty);

                        return Request.CreateResponse(HttpStatusCode.OK, data);
                    }
                }

                var errData = CosDBResponse<CBDocument>.CreateDBResponse(cbDocument, string.Empty);

                return Request.CreateErrorResponse(HttpStatusCode.NotFound,
                         HttpHelper.FormatError("Error while Uploading Document", "could not scan your data"));

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                _csContext.LogErrorToDB("Scan", "UploadDocument", ex.Message, ex);

                return Request.CreateErrorResponse(HttpStatusCode.NotFound,
                       HttpHelper.FormatError("Error while Uploading Document", ex.Message));

            }
            finally
            {

            }

        }

        [HttpPost]
        public async Task<HttpResponseMessage> GetInvoiceDetail(CBDocument cbDocument)
        {
            try
            {
                cbDocument = await _ezContext.GetInvoiceDetail(cbDocument);

                if (cbDocument != null)
                {
                    if (cbDocument.InvoiceID > 0)
                    {
                        ReckonDocument rcDocument = new ReckonDocument();
                        rcDocument.ScanInvoiceID = cbDocument.InvoiceID;
                        rcDocument.InvoiceTotal = cbDocument.EzzyInvoiceBlocksSS.invoiceForm.invoiceTotal;
                        rcDocument.TaxTotal = cbDocument.EzzyInvoiceBlocksSS.invoiceForm.gstTotal;
                        rcDocument.InvoiceDate = cbDocument.EzzyInvoiceBlocksSS.invoiceForm.invoiceDate;
                        rcDocument.VendorName = cbDocument.EzzyInvoiceBlocksSS.invoiceForm.supplierName;
                        rcDocument.InvoiceNumber = cbDocument.EzzyInvoiceBlocksSS.invoiceForm.invoiceNumber;
                        rcDocument.ScanPdfPath = cbDocument.CB_FilePath;
                        rcDocument.ReckonDocumentID = cbDocument.ReckonDocumentID;


                        if (cbDocument.EzzyDocumentClassificationSS != null)
                        {
                            rcDocument.InvoiceType = cbDocument.EzzyDocumentClassificationSS.doc_subtype.ToString();
                        }
                        else
                        {
                            rcDocument.InvoiceType = "";
                        }

                        var dbRes = _csContext.SaveReckonDocument(rcDocument);

                        cbDocument.ReckonDocumentID = CSConvert.ToInt(dbRes.Data);

                        var data = CosDBResponse<CBDocument>.CreateDBResponse(cbDocument, string.Empty);

                        return Request.CreateResponse(HttpStatusCode.OK, data);
                    }
                }

                var errData = CosDBResponse<CBDocument>.CreateDBResponse(cbDocument, string.Empty);

                return Request.CreateErrorResponse(HttpStatusCode.NotFound,
                         HttpHelper.FormatError("Scannig credentials not found", "could not scan your data"));

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                _csContext.LogErrorToDB("Scan", "GetInvoiceDetail", ex.Message, ex);

                return Request.CreateErrorResponse(HttpStatusCode.NotFound,
                       HttpHelper.FormatError("Scannig credentials not found", ex.Message));

            }
            finally
            {

            }

        }

        //Test phase
        [HttpPost]
        public HttpResponseMessage ScanRCDocument1()
        {
            try
            {
                // CBDocument cbDocument = await ScanDocument();

                CBDocument cbDocument = new CBDocument();
                if (cbDocument != null)
                {
                    //if (cbDocument.InvoiceID > 0)
                    {
                        Task.Delay(10000);

                        cbDocument.InvoiceID = 100;
                        cbDocument.ReckonDocumentID = 100;
                        cbDocument.CB_VendorInvoiceNo = "100";
                        cbDocument.ScanServiceStatus = "OK";

                        ReckonDocument rcDocument = new ReckonDocument();
                        rcDocument.ScanInvoiceID = cbDocument.InvoiceID;
                        rcDocument.InvoiceTotal = 100;
                        rcDocument.TaxTotal = 10;
                        rcDocument.InvoiceDate = DateTime.Now;
                        rcDocument.VendorName = "Test";

                        var dbRes = _csContext.SaveReckonDocument(rcDocument);

                        cbDocument.ReckonDocumentID = CSConvert.ToInt(dbRes.Data);

                        var data = CosDBResponse<CBDocument>.CreateDBResponse(cbDocument, string.Empty);

                        return Request.CreateResponse(HttpStatusCode.OK, data);
                    }
                }

                var errData = CosDBResponse<CBDocument>.CreateDBResponse(cbDocument, string.Empty);

                return Request.CreateErrorResponse(HttpStatusCode.NotFound,
                         HttpHelper.FormatError("Scannig credentials not found", "could not scan your data"));

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                _csContext.LogErrorToDB("Scan", "ScanRCDocument1", ex.Message, ex);

                return Request.CreateErrorResponse(HttpStatusCode.NotFound,
                       HttpHelper.FormatError("Scannig credentials not found", ex.Message));

            }
            finally
            {

            }

        }


        #region QBO Scan Document

        [HttpPost]
        public async Task<HttpResponseMessage> UploadDocumentQbo(string sessionID)
        {

            try
            {
                List<QboDocument> lstQboDocument = new List<QboDocument>();
                if (!Request.Content.IsMimeMultipartContent())
                    throw new Exception();

                var provider = new MultipartMemoryStreamProvider();
                await Request.Content.ReadAsMultipartAsync(provider);

                //Commented for testing purpose -without sending out to ezzybill
                //foreach (var content in provider.Contents)
                //{
                //    var filename = content.Headers.ContentDisposition.FileName.Trim('\"');
                //    var buffer = await content.ReadAsByteArrayAsync();

                //    System.Threading.Thread.Sleep(2000);

                //    var qboDocument = new QboDocument();
                //    qboDocument.ScanInvoiceID = 100;
                //    qboDocument.ClientFileID = content.Headers.ContentDisposition.Name;
                //    qboDocument.ClientFileID = qboDocument.ClientFileID.Replace("\"", string.Empty);
                //    lstQboDocument.Add(qboDocument);

                //}
                //


                foreach (var content in provider.Contents)
                {
                    var filename = content.Headers.ContentDisposition.FileName.Trim('\"');
                    var buffer = await content.ReadAsByteArrayAsync();
                    var qboDocument = _ezContext.UploadQboDocument(buffer, filename);
                    if (qboDocument != null)
                    {
                        
                        qboDocument.ClientFileID = content.Headers.ContentDisposition.Name;
                        qboDocument.ClientFileID = qboDocument.ClientFileID.Replace("\"", string.Empty);

                        qboDocument.ScanFile_Name = filename;

                        var dbRes = _csContext.InsertQboDocument(qboDocument);
                        if (dbRes != null)
                        {
                            if (dbRes.Data > 0)
                            {
                                qboDocument.DocumentID = dbRes.Data;
                                lstQboDocument.Add(qboDocument);
                            }
                        }
                    }
                }

                var data = CosDBResponse<List<QboDocument>>.CreateDBResponse(lstQboDocument, string.Empty);

                return Request.CreateResponse(HttpStatusCode.OK, data);


                //return Request.CreateResponse(HttpStatusCode.OK, lstQboDocument);
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                _csContext.LogErrorToDB("Scan", "UploadDocumentQbo", ex.Message, ex);

                return Request.CreateErrorResponse(HttpStatusCode.NotFound,
                         HttpHelper.FormatError("Error while Uploading Document", ex.Message));
            }

        }

        [HttpGet]
        public HttpResponseMessage GetDocumentToBill()
        {
            try
            {

                var respLine = _csContext.GetQboDocumentToBill(true);

                return Request.CreateResponse(HttpStatusCode.OK, respLine);
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                _csContext.LogErrorToDB("Scan", "GetDocumentToBill", ex.Message, ex);

                return Request.CreateErrorResponse(HttpStatusCode.NotFound,
                HttpHelper.FormatError("Scannig credentials not found", ex.Message));
            }
            finally
            {

            }
        }


        [HttpGet]
        public HttpResponseMessage GetDocumentQboToRead(string sessionID)
        {
            try
            {
                var cosDBResponse = _csContext.GetQboDocumentToRead(sessionID);

                if (cosDBResponse.Data != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, cosDBResponse);
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound,
                          HttpHelper.FormatError("Error while getting Qbo Document to read", cosDBResponse.Error));
                }
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                _csContext.LogErrorToDB("Scan", "GetDocumentQboToRead", ex.Message, ex);

                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                    HttpHelper.FormatError("Error occured while getting Qbo Document to read", ex.Message));
            }
        }

        public async Task<HttpResponseMessage> GetQbDocumentFile(int qboDocumentID)
        {
            if (qboDocumentID <= 0)
                return Request.CreateErrorResponse(HttpStatusCode.NotFound,
                         HttpHelper.FormatError("Error while getting Qbo Document ", "No record found"));

            var resp = _csContext.GetQboDocument(qboDocumentID);
            if (resp.StatusCode == DBResponseStatusCode.FAILED)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound,
                         HttpHelper.FormatError("Error while getting Qbo Document ", "No record found"));
            }

            if (string.IsNullOrEmpty(resp.Data.ScanBlob_Url))
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound,
                         HttpHelper.FormatError("Error while getting Qbo Document ", "No record found"));
            }


            string path = System.Web.HttpContext.Current.Server.MapPath("~/TempPdfDownload/temp.pdf");
            new FileDownloader().StreamDownload(resp.Data.ScanBlob_Url, path);
            //return Request.CreateResponse(HttpStatusCode.OK, "http://localhost:61457/TempPdfDownload/temp.pdf");



            var dataBytes = File.ReadAllBytes(path);
            //adding bytes to memory stream   
            var dataStream = new MemoryStream(dataBytes);

            HttpResponseMessage httpResponseMessage = Request.CreateResponse(HttpStatusCode.OK);
            httpResponseMessage.Content = new StreamContent(dataStream);
            httpResponseMessage.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment");
            httpResponseMessage.Content.Headers.ContentDisposition.FileName = "temp.pdf";
            httpResponseMessage.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

            return httpResponseMessage;
        }


        [HttpPost]
        public async Task<HttpResponseMessage> ScanQboDocument(QboDocument doc)
        {
            try
            {
                var cbDocument = await _ezContext.GetScanDetailQbo(doc);

                if (cbDocument != null)
                {
                    var validToken = await GetQBOToken();
                    if (validToken)
                    {
                        string vendID = _qboContext.CreateVendInQbo(cbDocument);
                    }

                    var respScanHdr = _csContext.InsertQboDocument(cbDocument);
                    if (respScanHdr.StatusCode == DBResponseStatusCode.OK)
                    {
                        _csContext.InsertQboDocumentLine(cbDocument.DocumentLine, cbDocument.DocumentID);
                    }
                }


                var data = CosDBResponse<QboDocument>.CreateDBResponse(cbDocument, string.Empty);
                return Request.CreateResponse(HttpStatusCode.OK, data);

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                _csContext.LogErrorToDB("Scan", "GetDocumentToApprove", ex.Message, ex);

                return Request.CreateErrorResponse(HttpStatusCode.NotFound,
                HttpHelper.FormatError("Scannig credentials not found", ex.Message));
            }
            finally
            {

            }
        }


        [HttpGet]
        public HttpResponseMessage GetDocumentToScan()
        {
            try
            {

                var respLine = _csContext.GetQboDocumentToScan();

                return Request.CreateResponse(HttpStatusCode.OK, respLine);
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                _csContext.LogErrorToDB("Scan", "GetDocumentToBill", ex.Message, ex);

                return Request.CreateErrorResponse(HttpStatusCode.NotFound,
                HttpHelper.FormatError("Scannig credentials not found", ex.Message));
            }
            finally
            {

            }
        }

        [HttpGet]
        public async Task<HttpResponseMessage> GetDocumentToApprove()
        {
            try
            {
                var resp = _csContext.GetQboDocumentToApprove(false);
                if (resp.Data == null)
                {

                    // return Request.CreateResponse(HttpStatusCode.OK, null);
                }


                foreach (var doc in resp.Data)
                {

                    if (!string.IsNullOrEmpty(doc.ScanBlob_Url)) continue;

                    if (doc.ScanInvoiceID == 0) continue;

                    var cbDocument = await _ezContext.GetScanDetailQbo(doc);

                    if (cbDocument != null)
                    {
                        var respScanHdr = _csContext.InsertQboDocument(cbDocument);
                        if (respScanHdr.StatusCode == DBResponseStatusCode.OK)
                        {
                            _csContext.InsertQboDocumentLine(cbDocument.DocumentLine, cbDocument.DocumentID);
                        }
                    }
                }

             
                var respLine = _csContext.GetQboDocumentToBill(false);

                return Request.CreateResponse(HttpStatusCode.OK, respLine);

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                _csContext.LogErrorToDB("Scan", "GetDocumentToApprove", ex.Message, ex);

                return Request.CreateErrorResponse(HttpStatusCode.NotFound,
                HttpHelper.FormatError("Scannig credentials not found", ex.Message));
            }
            finally
            {

            }
        }

        private async Task<bool> GetQBOToken()
        {
            SessionHelper sessionHelper = new SessionHelper();

            if (CSConvert.ToString(HttpContext.Current.Items["PlatformID"]) == "2") //Qbo Connetion
            {
                var resp =  CosmicContext.Instance.GetQboMasterByAccountAndConnectID();
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
                            if (DateTime.Now > Convert.ToDateTime(resp.Data.AccessTokenExpiresIn).AddMinutes(-5))
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



        #endregion

        #region Xero Scan Document

        [HttpPost]
        public async Task<HttpResponseMessage> UploadDocumentXero(string sessionID)
        {

            try
            {
                List<XeroDocument> lstXeroDocument = new List<XeroDocument>();
                if (!Request.Content.IsMimeMultipartContent())
                    throw new Exception();

                var provider = new MultipartMemoryStreamProvider();
                await Request.Content.ReadAsMultipartAsync(provider);

                foreach (var content in provider.Contents)
                {
                   
                    var buffer = await content.ReadAsByteArrayAsync();
                    var filename = content.Headers.ContentDisposition.FileName.Trim('\"');
                    var qboDocument = _ezContext.UploadXeroDocument(buffer, filename);
                    if (qboDocument != null)
                    {

                        qboDocument.ClientFileID = content.Headers.ContentDisposition.Name;
                        qboDocument.ClientFileID = qboDocument.ClientFileID.Replace("\"", string.Empty);

                        qboDocument.ScanFile_Name = filename;

                        var dbRes = _csContext.InsertXeroDocument(qboDocument);
                        if (dbRes != null)
                        {
                            if (dbRes.Data > 0)
                            {
                                qboDocument.DocumentID = dbRes.Data;
                                lstXeroDocument.Add(qboDocument);
                            }
                        }
                    }
                }

                var data = CosDBResponse<List<XeroDocument>>.CreateDBResponse(lstXeroDocument, string.Empty);

                return Request.CreateResponse(HttpStatusCode.OK, data);

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                _csContext.LogErrorToDB("Scan", "UploadDocumentXero", ex.Message, ex);

                return Request.CreateErrorResponse(HttpStatusCode.NotFound,
                         HttpHelper.FormatError("Error while Uploading Document", ex.Message));
            }

        }

        [HttpGet]
        [AllowAnonymous]
        public async void SaveInvoice(string token, string parentid, string docid, string state)
        {
            Logger log = CosmicLogger.SetLog();
            try
            {
                log.Info("SaveInvoice entry");
                if(string.IsNullOrEmpty(token))
                {
                    log.Info("SaveInvoice token is null");
                    return;
                }
                 _csContext.SaveXeroInvoiceProccessStatus(docid,1);
            }
            catch (Exception ex)
            {

                log.Error(ex);
                _csContext.LogErrorToDB("Scan", "SaveInvoice ", ex.Message, ex);

            }

        }

        [HttpGet]
        public HttpResponseMessage GetXeroDocumentToBill()
        {
            try
            {

                var respLine = _csContext.GetXeroDocumentToBill(true);

                return Request.CreateResponse(HttpStatusCode.OK, respLine);
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                _csContext.LogErrorToDB("Scan", "GetXeroDocumentToBill", ex.Message, ex);

                return Request.CreateErrorResponse(HttpStatusCode.NotFound,
                HttpHelper.FormatError("Scannig credentials not found", ex.Message));
            }
            finally
            {

            }
        }

        [HttpGet]
        public HttpResponseMessage GetXeroDocumentToAuth()
        {
            try
            {

                var respLine = _csContext.GetXeroDocumentToAuth();

                return Request.CreateResponse(HttpStatusCode.OK, respLine);
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                _csContext.LogErrorToDB("Scan", "GetXeroDocumentToAuth", ex.Message, ex);

                return Request.CreateErrorResponse(HttpStatusCode.NotFound,
                HttpHelper.FormatError("Scannig credentials not found", ex.Message));
            }
            finally
            {

            }
        }

        [HttpGet]
        public async Task<HttpResponseMessage> GetUpdatedDocument(int documentID)
        {
            try
            {
               // GetToken();
                var resp = _csContext.GetUpdatedQboDocument(documentID);


                return Request.CreateResponse(HttpStatusCode.OK, resp);

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                _csContext.LogErrorToDB("Scan", "GetDocumentToApprove", ex.Message, ex);

                return Request.CreateErrorResponse(HttpStatusCode.NotFound,
                HttpHelper.FormatError("Scannig credentials not found", ex.Message));
            }
            finally
            {

            }
        }

        [HttpGet]
        public HttpResponseMessage GetDocumentXeroToRead(string sessionID)
        {
            try
            {
                var cosDBResponse = _csContext.GetXeroDocumentToRead(sessionID);

                if (cosDBResponse.Data != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, cosDBResponse);
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound,
                          HttpHelper.FormatError("Error while getting Qbo Document to read", cosDBResponse.Error));
                }
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                _csContext.LogErrorToDB("Scan", "GetDocumentQboToRead", ex.Message, ex);

                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                    HttpHelper.FormatError("Error occured while getting Qbo Document to read", ex.Message));
            }
        }

        public async Task<HttpResponseMessage> GetXeroDocumentFile(int xeroDocumentID)
        {
            if (xeroDocumentID <= 0)
                return Request.CreateErrorResponse(HttpStatusCode.NotFound,
                         HttpHelper.FormatError("Error while getting Xero Document ", "No record found"));

            var resp = _csContext.GetXeroDocument(xeroDocumentID);
            if (resp.StatusCode == DBResponseStatusCode.FAILED)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound,
                         HttpHelper.FormatError("Error while getting XeroXero Document ", "No record found"));
            }

            if (string.IsNullOrEmpty(resp.Data.ScanBlob_Url))
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound,
                         HttpHelper.FormatError("Error while getting Xero Document ", "No record found"));
            }

            string path = System.Web.HttpContext.Current.Server.MapPath("~/TempPdfDownload/temp.pdf");
            new FileDownloader().StreamDownload(resp.Data.ScanBlob_Url, path);
            //return Request.CreateResponse(HttpStatusCode.OK, "http://localhost:61457/TempPdfDownload/temp.pdf");

            var dataBytes = File.ReadAllBytes(path);
            //adding bytes to memory stream   
            var dataStream = new MemoryStream(dataBytes);

            HttpResponseMessage httpResponseMessage = Request.CreateResponse(HttpStatusCode.OK);
            httpResponseMessage.Content = new StreamContent(dataStream);
            httpResponseMessage.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment");
            httpResponseMessage.Content.Headers.ContentDisposition.FileName = "temp.pdf";
            httpResponseMessage.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

            return httpResponseMessage;
        }

        //JAN22
        //private async Task<bool> GetXeroToken()
        //{
        //    SessionHelper sessionHelper = new SessionHelper();

        //    if (CSConvert.ToString(HttpContext.Current.Items["PlatformID"]) == "3") //Qbo Connetion
        //    {
        //        var cosDBResponse = _csContext.GetCheckXeroToken(_sessionHelper.AccountID, _sessionHelper.XeroConnectID);
        //        var checkXeroToken = cosDBResponse.Data;
        //        if (checkXeroToken != null)
        //        {
        //            var resp = CosmicContext.Instance.GetXeroMasterByAccountAndConnectID();
        //            if (resp.StatusCode == DBResponseStatusCode.OK)
        //            {
        //                if (resp.Data != null)
        //                {
        //                    //HttpContext.Current.Items["QboRealmId"] = resp.Data.RealmId;
        //                    HttpContext.Current.Items["XeroToken"] = resp.Data.AccessToken;
        //                    HttpContext.Current.Items["XeroToken_Sec"] = resp.Data.RefreshToken;

        //                    HttpContext.Current.Items["XeroOAuth_Token"] = resp.Data.OAuthToken;
        //                    HttpContext.Current.Items["XeroOAuth_Token_Sec"] = resp.Data.OAuthTokenSec;

        //                    HttpContext.Current.Items["XeroExpiresIn"] = resp.Data.AccessTokenExpiresIn;
        //                    HttpContext.Current.Items["XeroRefreshTokenExpiresIn"] = resp.Data.RefreshTokenExpiresIn;

        //                    sessionHelper.XeroToken = resp.Data.AccessToken;
        //                    _sessionHelper.XeroToken_Sec = resp.Data.RefreshToken;
        //                    _sessionHelper.XeroOAuth_Token = resp.Data.OAuthToken;
        //                    _sessionHelper.XeroOAuth_Token_Sec = resp.Data.OAuthTokenSec;
        //                    _sessionHelper.SessionHandle = resp.Data.SessionHandle;
        //                    if (checkXeroToken.XeroTokenMinute > 0)
        //                    {
        //                        return true;
        //                    }
        //                    if (checkXeroToken.XeroTokenMinute <= 0)
        //                    {

        //                        AccessToken accessToken = _xeroContext.ReNewToken();
        //                        if (accessToken != null)
        //                        {
        //                            if (string.IsNullOrEmpty(accessToken.Token))
        //                            {
        //                                return false;
        //                            }
        //                            XeroMaster _xeroMaster = new XeroMaster();
        //                            _xeroMaster.OAuthToken = resp.Data.OAuthToken;
        //                            _xeroMaster.OAuthTokenSec = resp.Data.OAuthTokenSec;
        //                            _xeroMaster.AccessToken = accessToken.Token;
        //                            _xeroMaster.RefreshToken = accessToken.TokenSecret;
        //                            _xeroMaster.AccessTokenExpiresIn = Convert.ToDateTime(accessToken.ExpiryDateUtc);
        //                            _xeroMaster.RefreshTokenExpiresIn = Convert.ToDateTime(accessToken.CreatedDateUtc);
        //                            _xeroMaster.XeroID = _sessionHelper.XeroConnectID;
        //                            _xeroMaster.SessionHandle = accessToken.SessionHandle;

        //                            _sessionHelper.XeroToken = accessToken.Token;
        //                            _sessionHelper.XeroToken_Sec = accessToken.TokenSecret;
        //                            _sessionHelper.XeroOAuth_Token = Convert.ToString(HttpContext.Current.Items["XeroToken"]);
        //                            _sessionHelper.XeroOAuth_Token_Sec = Convert.ToString(HttpContext.Current.Items["XeroToken_Sec"]);
        //                            _sessionHelper.SessionHandle = accessToken.SessionHandle;

        //                            Repository repository = _xeroContext.ServiceReq();
        //                            var company = repository.Organisation;
        //                            if (company != null)
        //                            {
        //                                if (company.APIKey == null)
        //                                {
        //                                    _xeroMaster.RealmId = "11111";
        //                                }
        //                                else
        //                                {
        //                                    _xeroMaster.RealmId = company.APIKey;
        //                                }
        //                            }

        //                            Logger _log = CosmicLogger.SetLog();
        //                            _log.Info("XERO/SaveToken");

        //                            int xeroConnectID = 0;
        //                            var respMsater = _csContext.SaveXeroMaster(_xeroMaster, ref xeroConnectID);


        //                            if (respMsater.StatusCode == DBResponseStatusCode.OK)
        //                            {
        //                                respMsater.Data.CompanyName = "Company";
        //                                respMsater.Data.LeagalName = "Company";

        //                                _log.Info("XERO/SaveToken == OK");

        //                                HttpContext.Current.Items["XeroToken"] = respMsater.Data.AccessToken;
        //                                HttpContext.Current.Items["XeroToken_Sec"] = respMsater.Data.RefreshToken;

        //                                HttpContext.Current.Items["XeroOAuth_Token"] = respMsater.Data.OAuthToken;
        //                                HttpContext.Current.Items["XeroOAuth_Token_Sec"] = respMsater.Data.OAuthTokenSec;

        //                                HttpContext.Current.Items["XeroExpiresIn"] = respMsater.Data.AccessTokenExpiresIn;
        //                                HttpContext.Current.Items["XeroRefreshTokenExpiresIn"] = respMsater.Data.RefreshTokenExpiresIn;

        //                                _sessionHelper.XeroToken = respMsater.Data.AccessToken;
        //                                _sessionHelper.XeroToken_Sec = respMsater.Data.RefreshToken;
        //                                _sessionHelper.XeroOAuth_Token = respMsater.Data.OAuthToken;
        //                                _sessionHelper.XeroOAuth_Token_Sec = respMsater.Data.OAuthTokenSec;
        //                                _sessionHelper.XeroConnectID = xeroConnectID;
        //                                _sessionHelper.XeroRealmId = _xeroMaster.RealmId;
        //                                _sessionHelper.SessionHandle = _xeroMaster.SessionHandle;
        //                                var companyInfo = _xeroContext.GetCompany();
        //                                if (companyInfo != null)
        //                                {
        //                                    var resp1 = _csContext.SaveXeroCompanyInfo(companyInfo);
        //                                }
        //                                return true;

        //                            }

        //                        }
        //                    }
        //                }
        //            }

        //        }
        //    }
        //    return false;
        //}

        

        [HttpPost]
        public async Task<HttpResponseMessage> ScanXeroDocument(XeroDocument doc)
        {
            try
            {
                var cbDocument = await _ezContext.GetScanDetailXero(doc);

                if (cbDocument != null)
                {
                    var validToken = await _xeroContext.IsValidToken();
                    if (validToken)
                    {
                        string vendID = await _xeroContext.CreateVendInXero(cbDocument);

                        var respScanHdr = _csContext.InsertXeroDocument(cbDocument);
                        if (respScanHdr.StatusCode == DBResponseStatusCode.OK)
                        {
                            _csContext.InsertXeroDocumentLine(cbDocument.DocumentLine, cbDocument.DocumentID);
                        }
                    }
                }
                var data = CosDBResponse<XeroDocument>.CreateDBResponse(cbDocument, string.Empty);
                return Request.CreateResponse(HttpStatusCode.OK, data);
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                _csContext.LogErrorToDB("Scan", "GetDocumentToApprove", ex.Message, ex);

                return Request.CreateErrorResponse(HttpStatusCode.NotFound,
                HttpHelper.FormatError("Scannig credentials not found", ex.Message));
            }
        }


        [HttpGet]
        [CustomAuthorize]
        public HttpResponseMessage GetXeroDocumentToScan()
        {
            try
            {

                var respLine = _csContext.GetXeroDocumentToScan();

                return Request.CreateResponse(HttpStatusCode.OK, respLine);
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                _csContext.LogErrorToDB("Scan", "GetXeroDocumentToScan", ex.Message, ex);

                return Request.CreateErrorResponse(HttpStatusCode.NotFound,
                HttpHelper.FormatError("Scannig credentials not found", ex.Message));
            }
            finally
            {

            }
        }

        [HttpGet]
      //  [CustomAuthorize]
        public async Task<HttpResponseMessage> GetXeroDocumentToApprove()
        {
            try
            {
                var resp = _csContext.GetXeroDocumentToApprove(false);
                if (resp.Data == null)
                {

                    // return Request.CreateResponse(HttpStatusCode.OK, null);
                }

                var qboJobs = _csContext.GetQboJobToProcess();
                List<string> qboDocuments = new List<string>();

                foreach (QboJob qboJob in qboJobs.Data)
                {
                    List<string> qbotempDocuments = qboJob.DocumentIDs.ToString().Split(',').ToList();
                    qboDocuments.AddRange(qbotempDocuments);
                }
                foreach (var doc in resp?.Data)
                {

                    if (!string.IsNullOrEmpty(doc.ScanBlob_Url)) continue;

                    if (doc.ScanInvoiceID == 0) continue;
                    if (!qboDocuments.Contains(doc.DocumentID.ToString()))
                    {
                        var cbDocument = await _ezContext.GetScanDetailXero(doc);

                        if (cbDocument != null)
                        {
                            var respScanHdr = _csContext.InsertXeroDocument(cbDocument);
                            if (respScanHdr.StatusCode == DBResponseStatusCode.OK)
                            {
                                _csContext.InsertXeroDocumentLine(cbDocument.DocumentLine, cbDocument.DocumentID);
                            }
                        }
                    }
                }


                var respLine = _csContext.GetXeroDocumentToBill(false);
                return Request.CreateResponse(HttpStatusCode.OK, respLine);

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                _csContext.LogErrorToDB("Scan", "GetDocumentToApprove", ex.Message, ex);

                return Request.CreateErrorResponse(HttpStatusCode.NotFound,
                HttpHelper.FormatError("Scannig credentials not found", ex.Message));
            }
            finally
            {

            }
        }


        #endregion
    }
}

