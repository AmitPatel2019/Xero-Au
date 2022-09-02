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
    // [RoutePrefix("Platform")]
    [CustomAuthorize]
    public class PlatformController : ApiController
    {
        CosmicContext _xsContext = CosmicContext.Instance;

        [HttpPost]
        public HttpResponseMessage Save (PlatformMaster platformMaster)
        {
            try
            {
                var cosDBResponse = _xsContext.SavePlatformMaster(platformMaster);
                if (cosDBResponse.StatusCode == DBResponseStatusCode.OK)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, cosDBResponse);
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, 
                        HttpHelper.FormatError("Error while creating Platform", cosDBResponse.Error));
                }

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                _xsContext.LogErrorToDB("Platform", "Save", ex.Message, ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                                    HttpHelper.FormatError("Error occured while creating Platform", ex.Message));
            }
        }

        [HttpGet]
        public HttpResponseMessage Get(int uid)
        {
            try
            {
                var cosDBResponse = _xsContext.GetPlatformMasterDetailByID(uid);
                if (cosDBResponse.Data !=null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, cosDBResponse);

                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound,
                            HttpHelper.FormatError("Platform Not Found", cosDBResponse.Error));

                }
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                _xsContext.LogErrorToDB("Platform", "Get", ex.Message, ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                       HttpHelper.FormatError("Error occured while getting Platform detail", ex.Message));
            }
        }
    }
}
