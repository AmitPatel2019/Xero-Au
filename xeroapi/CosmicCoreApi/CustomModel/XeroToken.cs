
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CosmicCoreApi.CustomModel
{
    public class XeroToken
    {
        public string XeroUrl { get; set; }
        public string Code { get; set; }
        public string UserName { get; set; }
        
    }

    public class XeroAuthoriseResponse
    {
        public string access_token { get; set; }
        public string refresh_token { get; set; }
        public int expires_in { get; set; }
        public string token_type { get; set; }
        public bool IsTokenExpired { get; set; }
    }
}