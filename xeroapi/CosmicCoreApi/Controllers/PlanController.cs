using CosmicApiModel;
using CosmicCoreApi;
using CosmicCoreApi.Helper;
using CosminInv.Api.App_Start;
using Flexis.Log;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace CosmicCoreApi.Controllers
{
    [CustomAuthorize]
    public class PlanController : ApiController
    {
        private CosmicContext _xsContext = CosmicContext.Instance;
        SessionHelper _sessionHelper = new SessionHelper();
        


        [HttpGet]
        public HttpResponseMessage GetAccountSubscribedPlan()
        {
            try
            {
                

                if (_sessionHelper.AccountID == 0)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound,
                         HttpHelper.FormatError("Error while retriving Subscribed Plan", "Could not resolved Account ID"));
                }

                var cosDBResponse = _xsContext.GetAccountSubscriptionByAccountID(_sessionHelper.AccountID);

                if (cosDBResponse.Data != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, cosDBResponse);
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound,
                          HttpHelper.FormatError("Account Not Found", cosDBResponse.Error));
                }
            }
            catch (Exception ex)
            {
                // Log exception code goes here 
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                _xsContext.LogErrorToDB("Plan", "GetAccountSubscribedPlan", ex.Message, ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                    HttpHelper.FormatError("Error occured while getting account detail", ex.Message));
            }

        }

        [HttpGet]
        public HttpResponseMessage GetAll()
        {
            try
            {
                

                if (_sessionHelper.AccountID == 0)
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                      HttpHelper.FormatError("Account ID Not Found", "Account ID Not Found"));

                var cosDBResponse = _xsContext.GetAccountPlan();
                if (cosDBResponse.Data != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, cosDBResponse);

                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                        HttpHelper.FormatError("ReckonDesktop Not Found", cosDBResponse.Error));
                }

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                _xsContext.LogErrorToDB("Plan", "GetAll", ex.Message, ex);

                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                          HttpHelper.FormatError("Error occured while getting ReckonDesktop detail", ex.Message));

            }

        }

        [HttpGet]
        public HttpResponseMessage GetTotalTrialPdfUsed()
        {
            try
            {

                

                if (_sessionHelper.AccountID == 0)
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                      HttpHelper.FormatError("Account ID Not Found", "Account ID Not Found"));

                var cosDBResponse = _xsContext.GetTotalTrialPdfUsed();
               
                    return Request.CreateResponse(HttpStatusCode.OK, cosDBResponse);

               

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                _xsContext.LogErrorToDB("Plan", "GetTotalTrialPdfUsed", ex.Message, ex);

                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                          HttpHelper.FormatError("Error occured while getting usage of trial pdf", ex.Message));

            }

        }

        [HttpGet]
        public HttpResponseMessage GetTotalPaidPdfUsed()
        {
            try
            {
                

                if (_sessionHelper.AccountID == 0)
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                      HttpHelper.FormatError("Account ID Not Found", "Account ID Not Found"));

                var cosDBResponse = _xsContext.GetTotalPaidPdfUsed();
                if (cosDBResponse.Data != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, cosDBResponse);

                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                        HttpHelper.FormatError("Trial pdf usage Not Found", cosDBResponse.Error));
                }

            }
            catch (Exception ex)
            {
                _xsContext.LogErrorToDB("Plan", "GetTotalPaidPdfUsed", ex.Message, ex);

                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                          HttpHelper.FormatError("Error occured while getting usage of trial pdf", ex.Message));

            }

        }

        [HttpGet]
        public HttpResponseMessage Get(string token)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                {
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                       HttpHelper.FormatError("Bad Request Found", "No Token found the request"));
                }

                string decrypted = EncryptionHelper.Decrypt(token);

                if (string.IsNullOrEmpty(decrypted))
                {
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                      HttpHelper.FormatError("Bad Request Found", "No Token found the request"));
                }

                string[] spltQueryString = decrypted.Split(new char[] { '|' });

                

                _sessionHelper.AccountID = CSConvert.ToInt(spltQueryString[0]);
                int planID = CSConvert.ToInt(spltQueryString[1]);


                if (_sessionHelper.AccountID == 0)
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                      HttpHelper.FormatError("Account ID Not Found", "Account ID Not Found"));

                var cosDBResponse = _xsContext.GetPlan(planID);
                if (cosDBResponse.Data != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, cosDBResponse);
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                        HttpHelper.FormatError("ReckonDesktop Not Found", cosDBResponse.Error));
                }

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                _xsContext.LogErrorToDB("Plan", "GetAll", ex.Message, ex);

                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                          HttpHelper.FormatError("Error occured while getting ReckonDesktop detail", ex.Message));

            }

        }


        [HttpGet]
        public HttpResponseMessage GetForXero(string plan)
        {
            try
            {
                if (string.IsNullOrEmpty(plan))
                {
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                       HttpHelper.FormatError("Bad Request Found", "No Token found the request"));
                }

                var xsResp = _xsContext.GetAccountSubscriptionByAccountID(CSConvert.ToInt(plan));
                if (xsResp != null)
                {                    

                    if (_sessionHelper.AccountID == 0)
                        return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                          HttpHelper.FormatError("Account ID Not Found", "Account ID Not Found"));

                    var cosDBResponse = _xsContext.GetPlan(CSConvert.ToInt(plan));
                    if (cosDBResponse.Data != null)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, cosDBResponse);
                    }
                    else
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                            HttpHelper.FormatError("Xero Not Found", cosDBResponse.Error));
                    }
                }

                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                           HttpHelper.FormatError("Xero Not Found", "Error while getting plan"));
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                _xsContext.LogErrorToDB("Plan", "GetAll", ex.Message, ex);

                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                          HttpHelper.FormatError("Error occured while getting ReckonDesktop detail", ex.Message));

            }

        }
        [HttpGet]
        public HttpResponseMessage SendRemainingTrialPdfMail(int RemainingTrialPDF)
        {
            try
            {
                if (_sessionHelper.AccountID == 0)
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                      HttpHelper.FormatError("Account ID Not Found", "Account ID Not Found"));

                string uidfilepath = System.Web.Hosting.HostingEnvironment.MapPath("~/Data");

                uidfilepath = uidfilepath + "\\UserId.txt";

                List<string> uidList = new List<string>();
                using (FileStream fs = new FileStream(uidfilepath, FileMode.Open, FileAccess.ReadWrite))
                {
                    using (StreamReader sr = new StreamReader(fs))
                    {
                        string all = sr.ReadToEnd();
                        uidList = all.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    }
                }
                if (!uidList.Contains(_sessionHelper.AccountID.ToString()))
                {
                    using (StreamWriter sw = File.AppendText(uidfilepath))
                    {
                        sw.WriteLine(_sessionHelper.AccountID.ToString());
                    }
                    var cosDBResponse = _xsContext.SendTrialPdfMailNotification(RemainingTrialPDF);
                    if (cosDBResponse?.Data != null)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, cosDBResponse);

                    }
                    else
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                            HttpHelper.FormatError("Error in sending mail for trial pdf usage", cosDBResponse.Error));
                    }
                }

                return Request.CreateErrorResponse(HttpStatusCode.OK,
                          HttpHelper.FormatError("Trial pdf Mail already sent", "Mail Sent"));
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                _xsContext.LogErrorToDB("Plan", "SendRemainingTrialPdfMail", ex.Message, ex);

                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                          HttpHelper.FormatError("Error occured while sending mail of trial pdf", ex.Message));

            }

        }
    }
}
