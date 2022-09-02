using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Net.Http;
using CosmicApiModel;
using CosmicCoreApi.Helper;
using Flexis.Log;
using CosmicCoreApi;
using System.Threading.Tasks;

namespace CosminInv.Api.App_Start
{
    public class CustomAuthorizeAttribute : AuthorizeAttribute
    {
        private const string BasicAuthResponseHeader = "WWW-Authenticate";
        private const string BasicAuthResponseHeaderValue = "Basic";

        public override void OnAuthorization(HttpActionContext actionContext)
        {
            Logger _log = CosmicLogger.SetLog();

            try
            {
            
                string cosmicBillUserToken = HttpRequestMessageExtensions.GetHeader(actionContext.Request, "CosmicBill-UserToken");
                string cosmicBillPlatform = HttpRequestMessageExtensions.GetHeader(actionContext.Request, "CosmicBill-PlatformID");
                string cosmicBillReckonFileID = HttpRequestMessageExtensions.GetHeader(actionContext.Request, "CosmicBill-ReckonFileID");
                string cosmicBillQboConnectID = HttpRequestMessageExtensions.GetHeader(actionContext.Request, "CosmicBill-QboConnectID");
                string cosmicBillXeroConnectID = HttpRequestMessageExtensions.GetHeader(actionContext.Request, "CosmicBill-XeroConnectID");
                _log.Info("cosmicBillUserToken:" + cosmicBillUserToken);
                _log.Info("cosmicBillPlatform:" + cosmicBillPlatform);
                _log.Info("cosmicBillReckonFileID:" + cosmicBillReckonFileID);
                _log.Info("cosmicBillQboConnectID:" + cosmicBillQboConnectID);


                // Extract Platform info
                if (!string.IsNullOrEmpty(cosmicBillPlatform))
                {
                    HttpContext.Current.Items["PlatformID"] = cosmicBillPlatform;
                }

                // Extract Platform info
                if (!string.IsNullOrEmpty(cosmicBillReckonFileID))
                {
                    HttpContext.Current.Items["ReckonFileID"] = CSConvert.ToInt(cosmicBillReckonFileID);
                }

                if (!string.IsNullOrEmpty(cosmicBillQboConnectID))
                {
                    HttpContext.Current.Items["QboConnectID"] = CSConvert.ToInt(cosmicBillQboConnectID);
                }

                if (!string.IsNullOrEmpty(cosmicBillXeroConnectID))
                {
                    HttpContext.Current.Items["XeroConnectID"] = CSConvert.ToInt(cosmicBillXeroConnectID);
                }



                //Extarct User Token Info
                if (!string.IsNullOrEmpty(cosmicBillUserToken))
                {
                    _log.Info("string.IsNullOrEmpty(cosmicBillUserToken)");

                    var result =  DecodeCosmicBillUserToken(cosmicBillUserToken);

                    if (!result)
                    {
                        _log.Info("custom authorization is failed");

                        actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized, "custom authorization is failed");
                        actionContext.Response.Headers.Add(BasicAuthResponseHeader, BasicAuthResponseHeaderValue);
                    }
                }
            }
            catch (Exception ex)
            {
                actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.InternalServerError);
                actionContext.Response.Headers.Add(BasicAuthResponseHeader, BasicAuthResponseHeaderValue);

                _log.Info("Error on Custom-Authorize");
                _log.Error(ex);
                return;
            }
        }

        public bool DecodeCosmicBillUserToken(string userToken)
        {
            
            Logger _log = CosmicLogger.SetLog();
            _log.Info("DecodeCosmicBillUserToken");
            try
            {

                var loginMaster = CosmicContext.Instance.GetLoginMasterByToken(userToken);
                if (loginMaster == null)
                {
                    _log.Info("GetLoginMasterByToken has failed");
                    return false;
                }

                var mylogin = loginMaster.Data;

                _log.Info("loginMaster.LoginID" + mylogin.LoginID);
                _log.Info("loginMaster.AccountID" + mylogin.AccountID);
                _log.Info("loginMaster.CountryOfOrigin" + mylogin.CountryOfOrigin);

                HttpContext.Current.Items["LoginID"] = mylogin.LoginID;
                HttpContext.Current.Items["AccountID"] = mylogin.AccountID;
                HttpContext.Current.Items["CountryOfOrigin"] = mylogin.CountryOfOrigin;


                return true;
            }
            catch (Exception ex)
            {
                _log.Error(ex);

                return false;
            }

        }

    }
}

