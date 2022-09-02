using CosmicApiModel;
using CosmicCoreApi;
using CosmicCoreApi.Helper;
using CosminInv.Api.App_Start;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace CosmicCoreApi.Controllers
{
    [CustomAuthorize]
    public class EzzyController : ApiController
    {
        private CosmicContext _xsContext = new CosmicContext();

        [HttpGet]
        public HttpResponseMessage GetScanningAccount()
        {
            try
            {
                var cosDBResponse = _xsContext.GetEzzyLogin();

                if (cosDBResponse.Data != null)
                {
                    return Request.CreateResponse<EzzyAccount>(HttpStatusCode.OK, cosDBResponse.Data);
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound,
                          HttpHelper.FormatError("Scannig credentials not found", cosDBResponse.Error));
                }
            }
            catch (Exception ex)
            {
                // Log exception code goes here 
                _xsContext.LogErrorToDB("Ezzy", "GetScanningAccount", ex.Message, ex);

                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                    HttpHelper.FormatError("Error occured while getting scanning account detail", ex.Message));
            }

        }
    }
}
