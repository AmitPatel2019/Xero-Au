using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace CosmicCoreApi.Helper
{
    public class HttpHelper
    {
        public static HttpError FormatError(string msgForUser, string actualError)
        {
            // Log exception code goes here  
            HttpError httpError = new HttpError();
            httpError.Add("MsgForUser", msgForUser);
            httpError.Add("Error", actualError);

            return httpError;
        }
    }
}