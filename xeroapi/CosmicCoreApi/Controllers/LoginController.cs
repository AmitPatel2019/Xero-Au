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
using System.Threading.Tasks;
using System.Web.Http;

namespace CosmicCoreApi.DoLogin
{
    [CustomAuthorize]
    public class LoginController : ApiController
    {
        #region Properties

        CosmicContext _xsContext = CosmicContext.Instance;

        #endregion

        #region Method

        [HttpPost]
        public HttpResponseMessage Save(LoginMaster loginMaster)
        {
            try
            {
                var cosDBResponse = _xsContext.SaveLoginMaster(loginMaster);
                if (cosDBResponse.StatusCode == DBResponseStatusCode.OK)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, cosDBResponse);

                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound,
                           HttpHelper.FormatError("Error while creating login", cosDBResponse.Error));
                }

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                _xsContext.LogErrorToDB("Login", "Save", ex.Message, ex);

                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                    HttpHelper.FormatError("Error occured while creating login", ex.Message));
            }

        }

        [HttpPost]
        public async Task<HttpResponseMessage> DoLogin(LoginMaster loginMaster)
        {
            Logger log = CosmicLogger.SetLog();
            try
            {
                log.Info("DoLogin Enterted");
                var cosDBResponse = _xsContext.VerifyUserNameAndPassword(loginMaster);
                LoginMaster loginMast = cosDBResponse.Data;
                if (cosDBResponse == null)
                {
                    log.Info("DoLogin cosDBResponse is null");
                    return Request.CreateResponse(HttpStatusCode.NotFound,
      HttpHelper.FormatError("Error while login", cosDBResponse.Error));
                }


                if (cosDBResponse.Data == null)

                {
                    log.Info("DoLogin cosDBResponse.Data is null");
                    return Request.CreateResponse(HttpStatusCode.NotFound,
                        HttpHelper.FormatError("Incorrect Credentials Found", cosDBResponse.Error));
                }


                SessionHelper sessionHelper = new SessionHelper();
                     
                sessionHelper.AccountID = loginMast.AccountID ??0;
                var loginResponse = Request.CreateResponse(HttpStatusCode.OK, cosDBResponse);
                log.Info("DoLogin loginResponse:" + await loginResponse.Content.ReadAsStringAsync());
                return loginResponse;

            }
            catch (Exception ex)
            {

                log.Error(ex);
                _xsContext.LogErrorToDB("Login", "DoLogin", ex.Message, ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                          HttpHelper.FormatError("Error occured while getting Login detail", ex.Message));

            }


        }

        [HttpPost]
        public HttpResponseMessage ProcessForgotPass(LoginMaster loginMaster)
        {
            try
            {
                var cosDBResponse = _xsContext.ProcessForgotPwd(loginMaster);

                if (cosDBResponse.StatusCode == DBResponseStatusCode.OK)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, cosDBResponse);

                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound,
                           HttpHelper.FormatError("Error while creating login", cosDBResponse.Error));
                }
               

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                _xsContext.LogErrorToDB("Login", "ProcessForgotPass", ex.Message, ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                          HttpHelper.FormatError("Error occured while getting Login detail", ex.Message));

            }


        }

        [HttpPost]
        public HttpResponseMessage ActivateAccount(LoginMaster loginMaster)
        {
            try
            {
                var cosDBResponse = _xsContext.ActivateAccount(loginMaster);
                if (cosDBResponse.StatusCode == DBResponseStatusCode.OK)
                {


                    return Request.CreateResponse(HttpStatusCode.OK, cosDBResponse);

                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound,
                           HttpHelper.FormatError("Error while creating login", cosDBResponse.Error));
                }

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                _xsContext.LogErrorToDB("Login", "ActivateAccount", ex.Message, ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                    HttpHelper.FormatError("Error occured while creating login", ex.Message));
            }

        }

        [HttpPost]
        public HttpResponseMessage ValidateActivationCode(AccountMaster accountMaster)
        {

            try
            {
                var cosDBResponse = _xsContext.ValidateActivationCode(accountMaster);

                switch (cosDBResponse.Data)
                {
                    case DBResponseStatusCode.OK:
                        return Request.CreateResponse(HttpStatusCode.OK, cosDBResponse);
                    case DBResponseStatusCode.FAILED:
                        return Request.CreateErrorResponse(HttpStatusCode.NotFound,
                          HttpHelper.FormatError("Error while matching Account", cosDBResponse.Error));

                    default:
                        return Request.CreateErrorResponse(HttpStatusCode.NotFound,
                          HttpHelper.FormatError("Error while matching Account", cosDBResponse.Error));
                }

            }
            catch (Exception ex)
            {
                // Log exception code goes here
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                _xsContext.LogErrorToDB("Login", "ValidateActivationCode", ex.Message, ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                    HttpHelper.FormatError("Error occured while matching account", ex.Message));
            }
        }

        [HttpPost]
        public HttpResponseMessage ValidateActivationCodeInLoginMaster(LoginMaster loginMaster)
        {

            try
            {
                var cosDBResponse = _xsContext.ValidateActivationCodeInloginMaster(loginMaster);

                switch (cosDBResponse.Data)
                {
                    case DBResponseStatusCode.OK:
                        return Request.CreateResponse(HttpStatusCode.OK, cosDBResponse);
                    case DBResponseStatusCode.FAILED:
                        return Request.CreateErrorResponse(HttpStatusCode.NotFound,
                          HttpHelper.FormatError("Error while matching Account", cosDBResponse.Error));

                    default:
                        return Request.CreateErrorResponse(HttpStatusCode.NotFound,
                          HttpHelper.FormatError("Error while matching Account", cosDBResponse.Error));
                }

            }
            catch (Exception ex)
            {
                // Log exception code goes here
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                _xsContext.LogErrorToDB("Login", "ValidateActivationCodeInLoginMaster", ex.Message, ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                    HttpHelper.FormatError("Error occured while matching account", ex.Message));
            }
        }

        [HttpPost]
        public HttpResponseMessage ChangePassword(LoginMaster loginMaster)
        {

            try
            {
                var cosDBResponse = _xsContext.ChangePassword(loginMaster);

                switch (cosDBResponse.Data)
                {
                    case DBResponseStatusCode.OK:
                        return Request.CreateResponse(HttpStatusCode.OK, cosDBResponse);
                    case DBResponseStatusCode.FAILED:
                        return Request.CreateErrorResponse(HttpStatusCode.NotFound,
                          HttpHelper.FormatError("Error while changing password", cosDBResponse.Error));
                    default:
                        return Request.CreateErrorResponse(HttpStatusCode.NotFound,
                          HttpHelper.FormatError("Error while changing password", cosDBResponse.Error));
                }

            }
            catch (Exception ex)
            {
                // Log exception code goes here
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                _xsContext.LogErrorToDB("Login", "ChangePassword", ex.Message, ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                    HttpHelper.FormatError("Error occured while password changeing", ex.Message));
            }
        }

        [HttpGet]
        public HttpResponseMessage GetAll()
        {
            try
            {
                var cosDBResponse = _xsContext.GetLoginMasterDetail();

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
                _xsContext.LogErrorToDB("Login", "GetAll", ex.Message, ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                    HttpHelper.FormatError("Error occured while getting account detail", ex.Message));
            }

        }

        [HttpGet]
        public HttpResponseMessage GetSubUser()
        {
            try
            {
                var cosDBResponse = _xsContext.GetSubUser();

                if (cosDBResponse.Data != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, cosDBResponse);
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound,
                          HttpHelper.FormatError("SubUsers Not Found", cosDBResponse.Error));
                }
            }
            catch (Exception ex)
            {
                // Log exception code goes here
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                _xsContext.LogErrorToDB("Login", "GetSubUser", ex.Message, ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                    HttpHelper.FormatError("Error occured while getting account detail", ex.Message));
            }

        }

        [HttpGet]
        public HttpResponseMessage IsVendDownloadRequired()
        {
            try
            {
                var cosDBResponse = _xsContext.IsEzUploadRequired();
                return Request.CreateResponse(HttpStatusCode.OK, cosDBResponse);
            }
            catch (Exception ex)
            {
                // Log exception code goes here
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                _xsContext.LogErrorToDB("Login", "IsVendDownloadRequired", ex.Message, ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                    HttpHelper.FormatError("Error occured while getting flag wether to download suppliers", ex.Message));
            }

        }

        #endregion


    }
}
