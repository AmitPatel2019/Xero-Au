using CosmicApiModel;
using CosmicCoreApi;
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
    [RoutePrefix("File")]
    public class FileSQLController : ApiController
    {
        CosmicContext _xsContext = CosmicContext.Instance;

        [HttpPost]

        public HttpResponseMessage Save(FileSQLMasters fileSQLMasters)
        {
            try
            {
                var cosDBResponse = _xsContext.SaveFileSQLMasters(fileSQLMasters);
                if (cosDBResponse.StatusCode == DBResponseStatusCode.OK)
                {
                    return Request.CreateResponse<DBResponseStatusCode>(HttpStatusCode.OK, cosDBResponse.Data);
                }
                else
                {
                     return Request.CreateErrorResponse(HttpStatusCode.NotFound,
                             HttpHelper.FormatError("Error while creating Sqlfile", cosDBResponse.Error));

                }

            }
            catch (Exception ex)
            {
                // Log exception code goes here 
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                _xsContext.LogErrorToDB("FileSQL", "Save", ex.Message, ex);

                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                    HttpHelper.FormatError("Error occured while creating Sqlfile", ex.Message));
            }
        }


        [HttpGet]

        public HttpResponseMessage Get(int uid)
        {
            try
            {
                var cosDBResponse = _xsContext.GetFileSQLMastersDetailByID(uid);

                if (cosDBResponse.Data != null)
                {
                    return Request.CreateResponse<FileSQLMasters>(HttpStatusCode.OK, cosDBResponse.Data);
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound,
                          HttpHelper.FormatError("Sqlfile Not Found", cosDBResponse.Error));
                }
            }
            catch (Exception ex)
            {
                // Log exception code goes here
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                _xsContext.LogErrorToDB("FileSQL", "Get", ex.Message, ex);

                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                    HttpHelper.FormatError("Error occured while getting Sqlfile detail", ex.Message));
            }

        }
    }
}
