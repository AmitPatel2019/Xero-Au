using CosmicApiModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using CoreApiModel = CosmicApiModel;

namespace CosmicApiHelper
{
    public class CosCoreApi
    {
        public static string ApiUrl = "";
        public static string AppName = "";
        public static string AppVersion = "";

        //Dictionary<string, string> param
        #region
        //public static async Task<CosDBResponse> GetAccount(Guid uid)
        //{
        //    return await Get<AccountMaster>(Controller.Account, Action.Get,"uid",uid.ToString());
        //}
        #endregion

        #region Get

        //public static async Task<bool> Login(string username, string password)
        //{
        //    return await Get<bool>(Controller.Login, Action.DoLogin, "un", HttpUtility.UrlEncode(username.Trim()), "pwd", HttpUtility.UrlEncode(password.Trim()));
        //}

        //public static async Task<IEnumerable<CoreApiModel.AccountMaster>> GetAccountParts()
        //{
        //    return await Get<IEnumerable<CoreApiModel.AccountMaster>>(Controller.Account, Action.Get);
        //}

        //public static async Task<T> GetData<T>(Guid uid)
        //{
        //    return await Get<T>(Controller.Account, Method.Get, "uid", uid.ToString());
        //}

        //public static async Task<T> GetAllData<T>(Guid uid)
        //{
        //    return await Get<T>(Controller.Account, Method.GetAll, "uid", uid.ToString());
        //}




        //public static async Task<string> SaveCustVend(CoreApiModel.CUSTVEND param)
        //{
        //    var response = await Save<CoreApiModel.CUSTVEND>(Controller.Pentagon, Action.SaveCustVend, param);
        //    if (response.IsSuccessStatusCode)
        //        return await response.Content.ReadAsStringAsync();
        //    return null;
        //}

        //public static async Task SaveCustVendSetup(CoreApiModel.CUSTVENDSETUP param)
        //{
        //    var response = await Save<CoreApiModel.CUSTVENDSETUP>(Controller.Pentagon, Action.SaveCustVendSetup, param);            
        //}

        //public static async Task<string> SaveContact(CoreApiModel.CONTACT param)
        //{
        //    var response = await Save<CoreApiModel.CONTACT>(Controller.Pentagon, Action.SaveContact, param);
        //    if (response.IsSuccessStatusCode)
        //        return await response.Content.ReadAsStringAsync();
        //    return null;
        //}

        //public static async Task<short> SaveShipToAddress(CoreApiModel.SHIPTO param)
        //{
        //    var response = await Save<CoreApiModel.SHIPTO>(Controller.Pentagon, Action.SaveShipToAddress, param);
        //    if (response.IsSuccessStatusCode)
        //        return JsonConvert.DeserializeObject<short>(await response.Content.ReadAsStringAsync());
        //    return 0;
        //}




        //public static async Task<IEnumerable<T>> GetHeaderList<T>(CoreApiModel.XS_HeaderFilter filter, DocCategory docCategory)
        //{
        //    return await Get<IEnumerable<T>>(docCategory.ToString(), Action.GetHeaderList, "filter", JsonConvert.SerializeObject(filter));
        //}

        //public static async Task<T> GetHeader<T>(int docNo, DocCategory docCategory)
        //{
        //    return await Get<T>(docCategory.ToString(), Action.GetHeader, "docNo", docNo.ToString());
        //}

        //public static async Task<byte[]> PrintDocument(int docNo, DocCategory docCategory)
        //{
        //    return await Get<byte[]>(docCategory.ToString(), Action.PrintDocument, "docNo", docNo.ToString());
        //}

        //public static async Task<IEnumerable<T>> GetLines<T>(int docNo, CoreApiModel.LineListType responseType, DocCategory docCategory)
        //{
        //    return await Get<IEnumerable<T>>(docCategory.ToString(), Action.GetLines, "docNo", docNo.ToString(), "responseType", responseType.ToString());
        //}

        //public static async Task<int> SaveHeader<T>(T header, DocCategory docCategory)
        //{
        //    HttpClient http = new HttpClient();
        //    var response = await http.PostAsJsonAsync(string.Format("{0}{1}/{2}?{3}={4}", ApiUrl, docCategory.ToString(), Action.SaveHeader.ToString(), "docLog", JsonConvert.SerializeObject(docLog)), header);
        //    var responseContent = await response.Content.ReadAsStringAsync();
        //    if (response.IsSuccessStatusCode)
        //        return JsonConvert.DeserializeObject<int>(responseContent);
        //    return 0;            
        //}

        //public static async Task<decimal> SaveLine<T>(T line, DocCategory docCategory)
        //{
        //    HttpClient http = new HttpClient();
        //    var response = await http.PostAsJsonAsync(string.Format("{0}{1}/{2}?{3}={4}", ApiUrl, docCategory.ToString(), Action.SaveLine.ToString(), "docLog", JsonConvert.SerializeObject(docLog)), line);
        //    var responseContent = await response.Content.ReadAsStringAsync();
        //    if (response.IsSuccessStatusCode)
        //        return JsonConvert.DeserializeObject<decimal>(responseContent);
        //    return 0;
        //}

        //public static async Task<int> SaveHeaderMsg(CoreApiModel.HEADER_MSG message)
        //{
        //    HttpClient http = new HttpClient();
        //    var response = await http.PostAsJsonAsync(string.Format("{0}{1}/{2}?{3}={4}", ApiUrl, Controller.DocumentHeader.ToString(), Action.SaveHeaderMsg.ToString(), "docLog", JsonConvert.SerializeObject(docLog)), message);
        //    if (response.IsSuccessStatusCode)
        //        return JsonConvert.DeserializeObject<int>(await response.Content.ReadAsStringAsync());
        //    return 0;
        //}

        //public static async Task SaveLineMsg(CoreApiModel.LINE_MSG message)
        //{
        //    HttpClient http = new HttpClient();
        //    await http.PostAsJsonAsync(string.Format("{0}{1}/{2}?{3}={4}", ApiUrl, Controller.DocumentLine.ToString(), Action.SaveLineMsg.ToString(), "docLog", JsonConvert.SerializeObject(docLog)), message);
        //    //var responseContent = response.Content.ReadAsStringAsync().Result;            
        //}

        //public static async Task<int> SaveLineChange<T>(T param, LineChangeDocCategory docCategory)
        //{
        //    HttpClient http = new HttpClient();
        //    var response = await http.PostAsJsonAsync(string.Format("{0}{1}/{2}", ApiUrl, docCategory.ToString(), Action.SaveLineChange.ToString()), param);
        //    var responseContent = await response.Content.ReadAsStringAsync();
        //    if (response.IsSuccessStatusCode)
        //        return JsonConvert.DeserializeObject<int>(responseContent);
        //    return 0;
        //}

        ////public static async Task<int> SalesInvoice_SaveHeaderChange(CoreApiModel.INV_HDR_CHANGES paramHdr)
        ////{
        ////    return await SaveLineChange<CoreApiModel.INV_HDR_CHANGES>(Controller.SalesInvoice, Action.SaveHeaderChange, paramHdr);
        ////}

        //#region Generic Template Methods       

        //public  HttpResponseMessage Get(Controller controller, Action action, string paramName, string paramValue)
        //{
        //    HttpClient _webApiService = new HttpClient();
        //    HttpResponseMessage response =  _webApiService.GetAsync(string.Format("{0}{1}/{2}?{3}={4}", ApiUrl, controller.ToString(), action.ToString(), paramName, paramValue)).Result;
        //  //  var responseContent =  response.Content.ReadAsStringAsync().Result;

        //    return response;
        //}

        private static async Task<T> Get<T>(CosController controller, Action action)
        {
            HttpClient _webApiService = new HttpClient();
            HttpResponseMessage response = await _webApiService.GetAsync(string.Format("{0}{1}/{2}", ApiUrl, controller.ToString(), action.ToString()));
            var responseContent = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
                return JsonConvert.DeserializeObject<T>(responseContent);
            return default(T);
        }

        private static async Task<T> Get<T>(CosController controller, Action action, string paramName, string paramValue)
        {
            HttpClient _webApiService = new HttpClient();
            HttpResponseMessage response = await _webApiService.GetAsync(string.Format("{0}{1}/{2}?{3}={4}", ApiUrl, controller.ToString(), action.ToString(), paramName, paramValue));
            var responseContent = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
                return JsonConvert.DeserializeObject<T>(responseContent);
            return default(T);
        }

        private static async Task<T> Get<T>(string controller, Action action, string paramName, string paramValue)
        {
            HttpClient _webApiService = new HttpClient();
            HttpResponseMessage response = await _webApiService.GetAsync(string.Format("{0}{1}/{2}?{3}={4}", ApiUrl, controller, action.ToString(), paramName, paramValue));
            var responseContent = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
                return JsonConvert.DeserializeObject<T>(responseContent);
            return default(T);
        }

        private static async Task<T> Get<T>(CosController controller, Action action, string param1Name, string param1Value, string param2Name, string param2Value)
        {
            HttpClient _webApiService = new HttpClient();
            HttpResponseMessage response = await _webApiService.GetAsync(string.Format("{0}{1}/{2}?{3}={4}&{5}={6}", ApiUrl, controller.ToString(), action.ToString(), param1Name, param1Value, param2Name, param2Value));
            var responseContent = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
                return JsonConvert.DeserializeObject<T>(responseContent);
            return default(T);
        }

        private static async Task<T> Get<T>(string controller, Action action, string param1Name, string param1Value, string param2Name, string param2Value)
        {
            HttpClient _webApiService = new HttpClient();
            HttpResponseMessage response = await _webApiService.GetAsync(string.Format("{0}{1}/{2}?{3}={4}&{5}={6}", ApiUrl, controller, action.ToString(), param1Name, param1Value, param2Name, param2Value));
            var responseContent = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
                return JsonConvert.DeserializeObject<T>(responseContent);
            return default(T);
        }

        private static async Task<T> Get<T>(CosController controller, Action action, string param1Name, string param1Value, string param2Name, string param2Value, string param3Name, string param3Value)
        {
            HttpClient _webApiService = new HttpClient();
            HttpResponseMessage response = await _webApiService.GetAsync(string.Format("{0}{1}/{2}?{3}={4}&{5}={6}&{7}={8}", ApiUrl, controller.ToString(), action.ToString(), param1Name, param1Value, param2Name, param2Value, param3Name, param3Value));
            var responseContent = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
                return JsonConvert.DeserializeObject<T>(responseContent);
            return default(T);
        }

        private static async Task<HttpResponseMessage> Save<T>(CosController controller, Action action, T param)
        {
            HttpClient http = new HttpClient();
            return await http.PostAsJsonAsync(string.Format("{0}{1}/{2}", ApiUrl, controller.ToString(), action.ToString()), param);
        }



      
        #endregion
    }

}

