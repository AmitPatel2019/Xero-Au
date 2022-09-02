using CosmicApiHelper.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flexis.Log;

namespace CosmicApiHelper
{
    public class CosDBResponse<T>
    {
        #region Properties

        public CosDBResponse(T value)
        {
        }
        public T Data { get; set; }
        public string Error { get; set; }
        public DBResponseStatusCode StatusCode { get; set; }
        public string Message { get; set; }
        public static CosDBResponse<T> CreateDBResponse(T data, string error)
        {
            var xsDBResponse = new CosDBResponse<T>(data);

            try
            {
                error = !string.IsNullOrEmpty(error) ? Convert.ToString(error) : string.Empty;

                xsDBResponse.StatusCode = error.Length > 5 ? DBResponseStatusCode.FAILED : DBResponseStatusCode.OK;
                xsDBResponse.Message = error.Length > 5 ? "Error while processing your request" : "Your request is processed sucessfully";
                xsDBResponse.Data = data;
                xsDBResponse.Error = error;


                return xsDBResponse;
            }
            catch (Exception ex)
            {
                Logger _log = BaseLog.Instance.GetLogger(null);
                if (_log != null)
                {
                    _log.Error(ex);
                }
            }

            return xsDBResponse;
        }

        #endregion
    }
}
