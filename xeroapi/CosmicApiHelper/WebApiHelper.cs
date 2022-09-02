using Flexis.Log;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;


namespace CosmicApiHelper
{
    public class WebApiHelper
    {
        public string ApiBaseUrl { get; set; } // = ConfigurationManager.AppSettings["CosmicApiAddress"];
        // protected WebApiHelper apiHelper;
        private Logger _log;

        public WebApiHelper()
        {
            _log = BaseLog.Instance.GetLogger(null);
            //apiHelper = new WebApiHelper();
            if (string.IsNullOrEmpty(ApiBaseUrl))
            {
                ApiBaseUrl = ConfigurationManager.AppSettings["CosmicApiAddress"];
            }
        }

        public DataSet Get(string api)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(ApiBaseUrl);
            // Add an Accept header for XML format.
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
            //client.DefaultRequestHeaders.Add("ccode", Program.CCode);
            //client.DefaultRequestHeaders.Add("Authorization", "Bearer " + Program.ApiKey);

            HttpResponseMessage response = client.GetAsync(api).Result;

            DataSet ds = null;
            if (response.IsSuccessStatusCode)
            {
                var jsonResult = response.Content.ReadAsStringAsync().Result;
                if (!string.IsNullOrWhiteSpace(jsonResult))
                {
                    ds = ConvertXMLToDataSet(jsonResult);
                }
            }

            return ds;
        }


        //public static async Task<bool> Login(string username, string password)
        //{
        //    return await Get<bool>(Controller.Pentagon, Action.Login, "un", HttpUtility.UrlEncode(username.Trim()), "pwd", HttpUtility.UrlEncode(password.Trim()));
        //}

        public string Get(string api, ref string error)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(ApiBaseUrl);
            // Add an Accept header for XML format.
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            //client.DefaultRequestHeaders.Add("ccode", Program.CCode);
            //client.DefaultRequestHeaders.Add("Authorization", "Bearer " + Program.ApiKey);

            HttpResponseMessage response = client.GetAsync(api).Result;

            if (response.IsSuccessStatusCode)
            {
                var jsonResult = response.Content.ReadAsStringAsync().Result;
                return jsonResult;
            }
            else
            {
                error = response.ReasonPhrase;
                _log.Info(error);
                return "";
            }
        }


        //public bool Post(string api, Object data, ref string error, ref string resultSet)
        //{

        //    bool result = false;
        //    try
        //    {

        //        HttpClient client = new HttpClient();
        //        client.BaseAddress = new Uri(baseUrl);
        //        // Add an Accept header for XML format.
        //        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        //        //client.DefaultRequestHeaders.Add("ccode", Program.CCode);
        //        client.DefaultRequestHeaders.Add("Authorization", "Bearer " + Program.AccessToken);
        //        client.DefaultRequestHeaders.Add("X-Auth-Token", Program.UserToken);
        //        client.DefaultRequestHeaders.Add("X-FileID-Token", Program.FileID);
                

        //        HttpResponseMessage response = client.PostAsJsonAsync(api, data).Result;

        //        if (response.StatusCode == System.Net.HttpStatusCode.OK)
        //        {
        //            var jsonResult = response.Content.ReadAsStringAsync().Result;
        //            resultSet = jsonResult;
        //            result = true;
        //        }
        //        else
        //        {
        //            error = response.ReasonPhrase;
        //            result = false;
        //        }

        //        return result;

        //    }
        //    catch (Exception ex)
        //    {
        //        _log = Program.Get_Log();
        //        _log.Error(ex);
        //    }

        //    return result;
        //}



        public string PostWithParam(string api, Object data, ref string error, ref string statusCode)
        {
            HttpClient client = new HttpClient();
            //client.BaseAddress = new Uri(baseUrl);
            // Add an Accept header for XML format.
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            //client.DefaultRequestHeaders.Add("ccode", Program.CCode);
            //client.DefaultRequestHeaders.Add("Authorization", "Bearer " + Program.ApiKey);

            HttpResponseMessage response = client.PostAsJsonAsync(api, data).Result;

            if (response.IsSuccessStatusCode)
            {
                var jsonResult = response.Content.ReadAsStringAsync().Result;
                statusCode = response.ReasonPhrase;
                return jsonResult;
            }
            else
            {
                statusCode = response.ReasonPhrase;
                error = response.Content.ReadAsStringAsync().Result;
                return "";
            }
        }




        public string Put(string api)
        {
            return "";
        }

        public string Delete(string api)
        {
            return "";
        }


        private DataSet ConvertXMLToDataSet(string xmlData)
        {
            StringReader stream = null;
            XmlTextReader reader = null;
            try
            {
                DataSet xmlDS = new DataSet();
                stream = new StringReader(xmlData);
                reader = new XmlTextReader(stream);
                xmlDS.ReadXml(reader);
                return xmlDS;
            }
            catch
            {
                return null;
            }
            finally
            {
                if (reader != null) reader.Close();
            }
        }

        public string GetApiUrl(Dictionary<string, string> apiParameter, string URL)
        {
            string param = "";
            foreach (KeyValuePair<string, string> item in apiParameter)
            {
                param = param + (item.Key + "=" + item.Value + "&");
            }

            if (!string.IsNullOrEmpty(param))
            {
                param = param.Remove(param.LastIndexOf("&"));
                URL = URL + "?" + param;
            }
            return URL;
        }

        enum Controller
        {
            Pentagon

        }

        enum Action
        {
            Login
        }




    }
}
