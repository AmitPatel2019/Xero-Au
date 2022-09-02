using CosmicApiModel;

using CosmicCoreApi.Helper;
using Flexis.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace CosmicCoreApi.Controllers
{
    public class AdminController : ApiController
    {
        private CosmicContext _csContext = CosmicContext.Instance;

        #region ManageAccount

        [HttpGet]
        public HttpResponseMessage GetAllPlan()
        {
            try
            {
                var cosDBResponse = _csContext.GetAccountPlan();
                if (cosDBResponse.Data != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, cosDBResponse);

                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                        HttpHelper.FormatError("Admin Not Found", cosDBResponse.Error));
                }

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                _csContext.LogErrorToDB("Admin", "GetAllPlan", ex.Message, ex);

                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                          HttpHelper.FormatError("Error occured while getting ReckonDesktop detail", ex.Message));

            }

        }

        [HttpGet]
        public HttpResponseMessage GetAllPlatformMaster()
        {
            try
            {
                var cosDBResponse = _csContext.GetPlatformMasterDetail();
                if (cosDBResponse.Data != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, cosDBResponse);

                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                        HttpHelper.FormatError("Admin Not Found", cosDBResponse.Error));
                }

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                _csContext.LogErrorToDB("Admin", "GetAllPlatformMaster", ex.Message, ex);

                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                          HttpHelper.FormatError("Error occured while getting PlatformMaster detail", ex.Message));

            }

        }

        [HttpGet]
        public HttpResponseMessage GetAccountMasterByPlatformAndPlan(int platform, int plan)
        {
            try
            {
                var cosDBResponse = _csContext.GetAccountMasterByPlatformAndPlan(platform, plan);
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
                _csContext.LogErrorToDB("Admin", "GetAccountMasterByPlatformAndPlan", ex.Message, ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                          HttpHelper.FormatError("Error occured while getting ReckonDesktop detail", ex.Message));

            }

        }

        #endregion ManageAccount

        #region Login

        [HttpGet]
        public HttpResponseMessage GetLogin(int accountID)
        {
            try
            {
                var cosDBResponse = _csContext.GetLoginMaster(accountID);
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
                _csContext.LogErrorToDB("Admin", "GetLogin", ex.Message, ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                          HttpHelper.FormatError("Error occured while getting ReckonDesktop detail", ex.Message));

            }

        }

        #endregion Login

        #region ReckonFiles

        [HttpGet]
        public HttpResponseMessage GetReckonFiles(int accountID, int platformID)
        {
            try
            {
                var cosDBResponse = _csContext.GetReckonFiles(accountID, platformID);
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
                _csContext.LogErrorToDB("Admin", "GetReckonFiles", ex.Message, ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                          HttpHelper.FormatError("Error occured while getting ReckonDesktop detail", ex.Message));

            }

        }

        #endregion ReckonFiles

        #region ReckonDocument

        [HttpGet]
        public HttpResponseMessage GetAccountMaster()
        {
            try
            {
                var cosDBResponse = _csContext.GetAccountMaster();
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
                _csContext.LogErrorToDB("Admin", "GetAccountMaster", ex.Message, ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                          HttpHelper.FormatError("Error occured while getting ReckonDesktop detail", ex.Message));

            }

        }

        [HttpGet]
        public HttpResponseMessage GetReckonDocument(int accountID, int reckonFileID, int month, int year)
        {
            try
            {

                var cosDBResponse = _csContext.GetReckonDocumentByAccoundId(accountID, reckonFileID, month, year);
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
                _csContext.LogErrorToDB("Admin", "GetReckonDocument", ex.Message, ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                          HttpHelper.FormatError("Error occured while getting ReckonDesktop detail", ex.Message));

            }

        }

        [HttpGet]
        public HttpResponseMessage GetReckonFilesByAccount(int accountID)
        {
            try
            {
                var cosDBResponse = _csContext.GetReckonFilesByAccountId(accountID);
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
                _csContext.LogErrorToDB("Admin", "GetReckonFilesByAccount", ex.Message, ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                          HttpHelper.FormatError("Error occured while getting ReckonDesktop detail", ex.Message));

            }

        }

        #endregion ReckonDocument


        #region StripPayment

        [HttpGet]
        public HttpResponseMessage GetStripePaymentByAccountID(int accountID, int planID)
        {
            try
            {
                var cosDBResponse = _csContext.GetStripePaymentByAccountID(accountID, planID);
                if (cosDBResponse.Data != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, cosDBResponse);

                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                        HttpHelper.FormatError("Admin Not Found", cosDBResponse.Error));
                }

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                _csContext.LogErrorToDB("Admin", "GetStripePaymentByAccountID", ex.Message, ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                          HttpHelper.FormatError("Error occured while getting Admin detail", ex.Message));

            }

        }

        [HttpGet]
        public HttpResponseMessage GetStripePayment(int month, int year)
        {
            try
            {
                var cosDBResponse = _csContext.GetStripePayment(month, year);
                if (cosDBResponse.Data != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, cosDBResponse);

                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                        HttpHelper.FormatError("Admin Not Found", cosDBResponse.Error));
                }

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                _csContext.LogErrorToDB("Admin", "GetStripePayment", ex.Message, ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                          HttpHelper.FormatError("Error occured while getting Admin detail", ex.Message));

            }

        }


        #endregion StripPayment

        #region Plan
        [HttpPost]
        public HttpResponseMessage Save(PlanMaster planMaster)
        {
            try
            {
                var cosDBResponse = _csContext.SavePlan(planMaster);
                if (cosDBResponse.StatusCode == DBResponseStatusCode.OK)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, cosDBResponse);

                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound,
                           HttpHelper.FormatError("Error while creating plan", cosDBResponse.Error));
                }

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                _csContext.LogErrorToDB("Admin", "Save", ex.Message, ex);

                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                    HttpHelper.FormatError("Error occured while creating plan", ex.Message));
            }

        }

        #endregion Plan

    }
}
