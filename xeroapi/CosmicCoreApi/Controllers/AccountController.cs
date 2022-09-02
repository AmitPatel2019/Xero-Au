using CosmicApiModel;
using CosmicCoreApi;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using CosmicCoreApi.Helper;
using CosminInv.Api.App_Start;
using Flexis.Log;

namespace CosmicCoreApi.Controllers
{
    [CustomAuthorize]
    public class AccountController : ApiController
    {
        #region Properties

        private CosmicContext _xsContext = CosmicContext.Instance;
        private SessionHelper _sessionHelper = new SessionHelper();
        #endregion

        #region Method

        [HttpPost]
        public HttpResponseMessage Save(AccountMaster accountMaster)
        {
         
            try
            {
                Logger _log = CosmicLogger.SetLog();
                _log.Info("AccountController save enterted");

                var cosDBResponse = _xsContext.CreateAccount(accountMaster);

                switch (cosDBResponse.Data)
                {
                    case DBResponseStatusCode.OK:
                        return Request.CreateResponse(HttpStatusCode.OK, cosDBResponse);
                    case DBResponseStatusCode.FAILED:
                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                          HttpHelper.FormatError("Error while creating Account", cosDBResponse.Error));
                    default:
                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                          HttpHelper.FormatError("Error while creating Account", cosDBResponse.Error));
                }

            }
            catch (Exception ex)
            {
                // Log exception code goes here 
                _xsContext.LogErrorToDB("Account", "Save", ex.Message, ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                    HttpHelper.FormatError("Error occured while creating account", ex.Message));
            }
        }

        [HttpGet]
        public HttpResponseMessage Get()
        {
            try
            {
                Logger _log = CosmicLogger.SetLog();
                _log.Info("Account/Get");

                var cosDBResponse = _xsContext.GetAccountMasterByAccountID(_sessionHelper.AccountID);

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
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                // Log exception code goes here  
                _xsContext.LogErrorToDB("Account", "Get", ex.Message, ex);

                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                    HttpHelper.FormatError("Error occured while getting account detail", ex.Message));
            }
        }

        [HttpPost]
        public HttpResponseMessage UpdateUserProfile(AccountMaster accountMaster)
        {
         
            try
            {
                accountMaster.AccountID = _sessionHelper.AccountID;
                var cosDBResponse = _xsContext.UpdateUserProfile(accountMaster);
                if (cosDBResponse.StatusCode == DBResponseStatusCode.OK)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, cosDBResponse);

                }

                return  Request.CreateResponse(HttpStatusCode.OK, cosDBResponse);
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                _xsContext.LogErrorToDB("Account", "UpdateUserProfile", ex.Message, ex);
                // Log exception code goes here 
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                    HttpHelper.FormatError("Error occured while creating account", ex.Message));
            }
        }


        [HttpPost]
        public HttpResponseMessage UpdatePhoneByAccountID(string  phone,string email)
        {

            try
            {
               
                var cosDBResponse = _xsContext.UpdatePhoneByAccountID(phone,email);
                if (cosDBResponse.StatusCode == DBResponseStatusCode.OK)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, cosDBResponse);

                }

                return Request.CreateResponse(HttpStatusCode.OK, cosDBResponse);
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                _xsContext.LogErrorToDB("Account", "UpdatePhoneByAccountID", ex.Message, ex);
                // Log exception code goes here 
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                    HttpHelper.FormatError("Error occured while creating account", ex.Message));
            }
        }

        [HttpPost]
        public HttpResponseMessage AddEditUser(LoginMaster loginMaster)
        {

            try
            {
                

                loginMaster.AccountID = _sessionHelper.AccountID;

                var userNameChkResp = _xsContext.GetLoginByUserName(loginMaster.UserName);
                if (userNameChkResp != null)
                {
                    if (userNameChkResp.Data != null)
                    {
                        if (string.Compare(userNameChkResp.Data.UserName.Trim(), loginMaster.UserName.Trim(), true) == 0)
                        {
                            userNameChkResp.StatusCode = DBResponseStatusCode.FAILED;
                            userNameChkResp.Message = "User Name is already occupied by any other user";

                            return Request.CreateResponse(HttpStatusCode.NotFound, string.Empty);
                        }
                    }
                }

                var cosDBResponse = _xsContext.AddEditUser(loginMaster);
                if (cosDBResponse.StatusCode == DBResponseStatusCode.OK)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, cosDBResponse);

                }

                return Request.CreateResponse(HttpStatusCode.OK, cosDBResponse);
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                // Log exception code goes here 
                _xsContext.LogErrorToDB("Account", "AddEditUser", ex.Message, ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                    HttpHelper.FormatError("Error occured while creating account", ex.Message));
            }
        }

        #endregion
        [HttpPost]
        public HttpResponseMessage UpdateXeroMaster(XeroMaster xeromaster)
        {
            Logger _log = CosmicLogger.SetLog();
            _log.Info("Account/UpdateClassJob");
            try
            {
                var cosDBResponse = _xsContext.UpdateXeroMaster(xeromaster);

                switch (cosDBResponse.Data)
                {
                    case DBResponseStatusCode.OK:
                        return Request.CreateResponse(HttpStatusCode.OK, cosDBResponse);
                    case DBResponseStatusCode.FAILED:
                        return Request.CreateErrorResponse(HttpStatusCode.NotFound,
                          HttpHelper.FormatError("Error while updateing Account", cosDBResponse.Error));
                    default:
                        return Request.CreateErrorResponse(HttpStatusCode.NotFound,
                          HttpHelper.FormatError("Error while updateing Account", cosDBResponse.Error));
                }

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                    HttpHelper.FormatError("Error occured while updateing account", ex.Message));
            }
        }

    }
}
