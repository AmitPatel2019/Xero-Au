using CosmicApiModel;
using CosmicCoreApi;
using CosmicCoreApi.Helper;
using CosminInv.Api.App_Start;
using Flexis.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace CosmicCoreApi.Controllers
{

    [CustomAuthorize]
    public class ReckonDesktopController : ApiController
    {
        CosmicContext _xsContext = CosmicContext.Instance;
        private SessionHelper _sessionHelper = new SessionHelper();

        [HttpPost]
        public HttpResponseMessage Save(ReckonDesktopMaster reckonDesktopMaster)
        {

            try
            {
                int reckonFileID = 0;
                var cosDBResponse = _xsContext.SaveReconDesktopMaster(reckonDesktopMaster, ref reckonFileID);
                if (cosDBResponse.StatusCode == DBResponseStatusCode.OK)
                {
                    if (reckonDesktopMaster.ReckonFileID <= 0)
                    {
                        //Register user in Ezzy Account
                        EzzyContext ezzyContext = new EzzyContext();
                       // ezzyContext.RegisterUserForReckon(reckonFileID);
                    }
                    
                    return Request.CreateResponse(HttpStatusCode.OK, cosDBResponse);
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound,
                              HttpHelper.FormatError("Error while creating ReckonDesktop", cosDBResponse.Error));

                }

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                _xsContext.LogErrorToDB("ReckonDesktop", "Save", ex.Message, ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                      HttpHelper.FormatError("Error occured while creating ReckonDesktop", ex.Message));
            }

        }

        [HttpPost]
        public HttpResponseMessage SaveAllVendDefault(ICollection<ReckonVendDefault> lstReckonVendorDefault)
        {

            try
            {
                var cosDBResponse = _xsContext.SaveAllReckonVendDefault(lstReckonVendorDefault);
                if (cosDBResponse.StatusCode == DBResponseStatusCode.OK)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, cosDBResponse);
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound,
                              HttpHelper.FormatError("Error while saving vendor default", cosDBResponse.Error));

                }

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                _xsContext.LogErrorToDB("ReckonDesktop", "SaveAllVendDefault", ex.Message, ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                      HttpHelper.FormatError("Error occured while saving vendor default", ex.Message));
            }

        }

        [HttpPost]
        public HttpResponseMessage SaveVendDefault(ReckonVendDefault reckonVendorDefault)
        {

            try
            {
                var cosDBResponse = _xsContext.SaveReckonVendDefault(reckonVendorDefault);

                if (cosDBResponse.StatusCode == DBResponseStatusCode.OK)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, cosDBResponse);
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound,
                              HttpHelper.FormatError("Error while saving vendor default", cosDBResponse.Error));

                }

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                _xsContext.LogErrorToDB("ReckonDesktop", "SaveVendDefault", ex.Message, ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                      HttpHelper.FormatError("Error occured while saving vendor default", ex.Message));
            }

        }

        [HttpGet]
        public HttpResponseMessage GetVendorDefault()
        {
            try
            {
                
                if (_sessionHelper.AccountID == 0)
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                      HttpHelper.FormatError("Account ID Not Found", "Account ID Not Found"));

                var cosDBResponse = _xsContext.GetReckonVendDefault();
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
                _xsContext.LogErrorToDB("ReckonDesktop", "GetVendorDefault", ex.Message, ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                          HttpHelper.FormatError("Error occured while getting ReckonDesktop detail", ex.Message));

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

                var cosDBResponse = _xsContext.GetAllReckonFileByAccountID();
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
                _xsContext.LogErrorToDB("ReckonDesktop", "GetAll", ex.Message, ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                          HttpHelper.FormatError("Error occured while getting ReckonDesktop detail", ex.Message));

            }

        }


        [HttpGet]
        public HttpResponseMessage GetReckonDocument(int month, int year)
        {
            try
            {
                
                if (_sessionHelper.AccountID == 0)
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                      HttpHelper.FormatError("Account ID Not Found", "Account ID Not Found"));

                var cosDBResponse = _xsContext.GetReckonDocument(month, year);
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
                _xsContext.LogErrorToDB("ReckonDesktop", "GetReckonDocument", ex.Message, ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                          HttpHelper.FormatError("Error occured while getting ReckonDesktop detail", ex.Message));

            }

        }

       [HttpGet]
        public HttpResponseMessage GetReckonFileUserAccess(int loginID)
        {
            try
            {
                var cosDBResponse = _xsContext.GetReckonFileWithUserAccess(loginID);
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
                _xsContext.LogErrorToDB("ReckonDesktop", "GetReckonFileUserAccess", ex.Message, ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                          HttpHelper.FormatError("Error occured while getting ReckonDesktop detail", ex.Message));

            }

        }

        [HttpPost]
        public HttpResponseMessage UpdateReckonFileUserAccess(LoginMaster loginMaster)
        {

            try
            {
                var cosDBResponse = _xsContext.UpdateReckonFileUserAccess(loginMaster);
                if (cosDBResponse.StatusCode == DBResponseStatusCode.OK)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, cosDBResponse);
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound,
                              HttpHelper.FormatError("Error while creating ReckonDesktop", cosDBResponse.Error));

                }

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                _xsContext.LogErrorToDB("ReckonDesktop", "UpdateReckonFileUserAccess", ex.Message, ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                      HttpHelper.FormatError("Error occured while creating ReckonDesktop", ex.Message));
            }

        }


    }
}
