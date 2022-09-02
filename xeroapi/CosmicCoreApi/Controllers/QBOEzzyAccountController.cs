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

    public class QBOEzzyAccountController : ApiController
    {
        CosmicContext _xsContext = CosmicContext.Instance;
        [HttpPost]
        public HttpResponseMessage SaveQBOEzzyAccount(QBOEzzyAccount qBOEzzyAccount)
        {

            try
            {
                var cosDBResponse = _xsContext.SaveQboEzzyAccount(qBOEzzyAccount);

                if (cosDBResponse.StatusCode == DBResponseStatusCode.OK)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, cosDBResponse);
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound,
                              HttpHelper.FormatError("Error while saving qBOEzzyAccount default", cosDBResponse.Error));

                }

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                _xsContext.LogErrorToDB("QBOEzzyAccount", "SaveQBOEzzyAccount", ex.Message, ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                      HttpHelper.FormatError("Error occured while saving qBOEzzyAccount default", ex.Message));
            }

        }
    }
}
