using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace XeroAutomationService.AccessContext
{
    public class XeroAuthoriseResponse
    {
        public string access_token { get; set; }
        public string refresh_token { get; set; }
        public int expires_in { get; set; }
        public string token_type { get; set; }
        public bool IsTokenExpired { get; set; }
    }

    public class StripeAuthoriseResponse
    {
        public string access_token { get; set; }
        public string refresh_token { get; set; }
        public string token_type { get; set; }

        public string scope { get; set; }
        public string stripe_publishable_key { get; set; }
        public bool livemode { get; set; }
        public string stripe_user_id { get; set; }
    }

    /// <summary>
    /// Wrapper for abstracting connectivity to web application using Web requests
    /// </summary>
    class XeroOwnerFlowWebClient
    {
        private HttpWebRequest _request;
        public XeroOwnerFlowWebClient(string url, string method = "POST", string accept = "application/json")
        {
            _request = HttpWebRequest.Create(url) as HttpWebRequest;
            _request.Method = method;
            _request.Accept = accept;
        }

        public void AddAuthenticationHeader(string authenticationType, string value)
        {
            _request.Headers.Add(HttpRequestHeader.Authorization, string.Format("{0} {1}", authenticationType, value));
        }

        public TResponseType Send<TRequestType, TResponseType>(TRequestType request, string contentType = "application/json")
        {
            var bodyText = JsonConvert.SerializeObject(request);
            return Send<TResponseType>(bodyText, contentType);
        }

        public TResponseType Send<TResponseType>(string bodyText, string contentType)
        {
            var responseText = Send(bodyText, contentType);
            var responseObject = JsonConvert.DeserializeObject<TResponseType>(responseText);
            return responseObject;
        }

        public string Send(string bodyText, string contentType)
        {
            SetRequestBody(bodyText, contentType);

            //Call the identity server
            var response = _request.GetResponse();

            var responseText = GetResponseText(response);

            return responseText;
        }

        private void SetRequestBody(string bodyText, string contentType)
        {
            var bytesToSend = Encoding.UTF8.GetBytes(bodyText);
            var inputSream = _request.GetRequestStream();
            inputSream.Write(bytesToSend, 0, bytesToSend.Length);
            _request.ContentType = contentType;
            inputSream.Close();
        }

        private static string GetResponseText(WebResponse response)
        {
            var responseStream = response.GetResponseStream();
            if (responseStream == null)
            {
                return String.Empty;
            }

            var streamReader = new StreamReader(responseStream);
            var responseText = streamReader.ReadToEnd();
            return responseText;
        }
    }
}
