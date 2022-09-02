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
    public class ReckonEzzyAccountController : ApiController
    {
        CosmicContext _xsContext = CosmicContext.Instance;
        [HttpPost]
        public HttpResponseMessage SaveReckonEzzy(ReckonEzzyAccount reckonEzzyAccount)
        {

            try
            {
                var cosDBResponse = _xsContext.SaveReckonEzzyAccount(reckonEzzyAccount);

                if (cosDBResponse.StatusCode == DBResponseStatusCode.OK)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, cosDBResponse);
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound,
                              HttpHelper.FormatError("Error while saving reckonEzzyAccount default", cosDBResponse.Error));

                }

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                _xsContext.LogErrorToDB("ReckonEzzy", "SaveReckonEzzy", ex.Message, ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                      HttpHelper.FormatError("Error occured while saving reckonEzzyAccount default", ex.Message));
            }

        }
    }
}
