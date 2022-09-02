
using CosmicApiHelper;
using CosmicApiModel;
using CosmicCoreApi.Controllers;
using Flexis.Log;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using Xero.NetStandard.OAuth2.Api;
using Xero.NetStandard.OAuth2.Client;
using Xero.NetStandard.OAuth2.Config;
using Xero.NetStandard.OAuth2.Models;
using Xero.NetStandard.OAuth2.Token;

namespace CosmicCoreApi.AccessContext
{
    public class XeroTokenHelper
    {
        Logger m_log = CosmicLogger.SetLog();
        private static XeroTokenHelper instance;
        public static XeroTokenHelper Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new XeroTokenHelper();
                }
                return instance;
            }
        }
        private XeroTokenHelper()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
        }

        public async Task<IXeroToken> XeroToken()
        {
            Logger log = CosmicLogger.SetLog();
            try
            {
                log.Info("XeroToken started");
                var account = CosmicContext.Instance.GetXeroMasterByAccountID();
                log.Info("account.Data count:"+ account.Data.Count);
                var acc = account.Data.ElementAt(0);
                XeroOAuth2Token xeroOAuth2Token = new XeroOAuth2Token();
                xeroOAuth2Token.AccessToken = acc.AccessToken;
                xeroOAuth2Token.RefreshToken = acc.RefreshToken;
                xeroOAuth2Token.IdToken = acc.IdentityToken;
                xeroOAuth2Token.ExpiresAtUtc = acc.AccessTokenExpiresIn.GetValueOrDefault();

                xeroOAuth2Token.Tenants = new List<Tenant>()
                {
                    new Tenant()
                    {
                        TenantId = Guid.Parse( acc.RealmId)
                    }
                };
                var currentUtcTimenow = DateTime.UtcNow;
                var tokenExpireTimeinUtc = acc.AccessTokenExpiresIn.Value.ToUniversalTime();
                log.Info("AccessToken will Expirde in:"+ tokenExpireTimeinUtc);
                log.Info("UTC time now(DateTime.UtcNow):" + currentUtcTimenow.ToString());
                //<0 − If date1 is earlier than date2
                // 0 − If date1 is the same as date2
                // > 0 − If date1 is later than date2

                if (tokenExpireTimeinUtc == null || tokenExpireTimeinUtc == DateTime.MinValue || DateTime.Compare(tokenExpireTimeinUtc, currentUtcTimenow) <=0)
                {
                    log.Info("xero AccessTokenExpird");
                    var newToken = await GetXeroClient().RefreshAccessTokenAsync(xeroOAuth2Token);
                    var xm = await ConvertIXeroTokenToXeroMaster(newToken);
                    SaveToken(xm);
                    return newToken;
                }else
                {
                    log.Info("xero AccessToken not Expird");
                }
                return xeroOAuth2Token;
            }
            catch(Exception ex)
            {
                log.Info(ex.Message+ex.StackTrace);
                System.Diagnostics.Debug.WriteLine(ex);
                return null;
            }
          
        }

        public async Task<XeroMaster> ConvertIXeroTokenToXeroMaster(IXeroToken tokenResponse)
        {
            try
            {
                XeroMaster xeroMaster = new XeroMaster();

                if (tokenResponse.Tenants== null || tokenResponse.Tenants.Count ==0)
                {
                    tokenResponse.Tenants = await XeroTokenHelper.Instance.GetXeroTenents(tokenResponse);
                }
                
                xeroMaster.AccessToken = tokenResponse.AccessToken;
                xeroMaster.RefreshToken = tokenResponse.RefreshToken;
                xeroMaster.AccessTokenExpiresIn = tokenResponse.ExpiresAtUtc;
                xeroMaster.RealmId = tokenResponse.Tenants[0].TenantId.ToString();
                xeroMaster.RefreshTokenExpiresIn = DateTime.Now.AddDays(60);
                xeroMaster.IdentityToken = tokenResponse.IdToken;
                return xeroMaster;
            }
            catch(Exception ee)
            {
                m_log.Error(ee.Message+ee.StackTrace);
                return null;
            }
            
        }
        private void SaveToken(XeroMaster xeroMaster)
        {
            try
            {
                var xeroConnectID = 0;
                var cosDBResponse = CosmicContext.Instance.SaveXeroMaster(xeroMaster, ref xeroConnectID);
            }
            catch(Exception ee)
            {
                m_log.Error(ee.Message+ee.StackTrace);
            }

        }

        public AccountingApi AccountingApiRepo()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            var apiAccessor = new AccountingApi();
            return apiAccessor;
        }

        public IApiAccessor GetApiInstance(XeroObject xeroObject)
        {
            IApiAccessor apiAccessor = null;
            switch (xeroObject)
            {
                case XeroObject.Account:
                    apiAccessor = new AccountingApi();
                    break;
                case XeroObject.AssetApi:
                    apiAccessor = new AssetApi();
                    break;
                case XeroObject.BankFeedsApi:
                    apiAccessor = new BankFeedsApi();
                    break;
                case XeroObject.FilesApi:
                    apiAccessor = new FilesApi();
                    break;
                case XeroObject.PayrollUkApi:
                    apiAccessor = new PayrollUkApi();
                    break;
                case XeroObject.ProjectApi:
                    apiAccessor = new ProjectApi();
                    break;
                case XeroObject.AppStoreApi:
                    apiAccessor = new AppStoreApi();
                    break;
            }
            return apiAccessor;
        }
        public async Task<IXeroToken> GetTokenByCode(string code)
        {
            Logger m_log = CosmicLogger.SetLog();
            try
            {
                m_log.Info("GetTokenByCode entry");
                m_log.Info("Auth code >>>>>>>>>>>>>>>> "+ code);
                var client = GetXeroClient();
                //before getting the access token please check that the state matches
                var token = (XeroOAuth2Token)await client.RequestAccessTokenAsync(code);
                m_log.Info("Auth token >>>>>>>>>>>>>>>> " + token?.AccessToken);
                return token;
            }
            catch (Exception ex)
            {
                m_log.Error(ex.Message + ex.StackTrace);
                System.Diagnostics.Debug.WriteLine(ex);
                return null;
            }
        }

        private XeroConfiguration GetXeroConfig()
        {
            try
            {
                string xeroRedirectUrl = ConfigurationManager.AppSettings["xero_redirect_uri"];
                string clientId = ConfigurationManager.AppSettings["XeroclientId"];
                string clientSecret = ConfigurationManager.AppSettings["XeroclientSecret"];

                XeroConfiguration xconfig = new XeroConfiguration();
                xconfig.ClientId = clientId;
                xconfig.ClientSecret = clientSecret;
                xconfig.CallbackUri = new Uri(xeroRedirectUrl); //default for standard webapi template
                xconfig.Scope = ConfigurationManager.AppSettings["xero_scope"];
                
                return xconfig;
            }
            catch(Exception ex)
            {
                m_log.Error(ex.Message+ex.StackTrace);
                return null;
            }
          
        }
        public XeroClient GetXeroClient()
        {
            Logger _log = CosmicLogger.SetLog();
            try
            {
                var config = GetXeroConfig();
                var client = new XeroClient(config);
             //   HttpContext.Current.Response.Redirect(client.BuildLoginUri(), false);
                return client;
            }catch(Exception ex)
            {
                _log.Error(ex.Message+ex.StackTrace);
                return null;
            }
           
        }

        public async Task<List<Tenant>> GetXeroTenents(IXeroToken token, bool includeDemoCompany= false)
        {
            //from here you will need to access your Xero Tenants
            try
            {
                var client = GetXeroClient();
                List<Tenant> tenants = await client.GetConnectionsAsync(token);
                if(!includeDemoCompany && tenants!=null)
                {
                    var ignoreTenents = ConfigurationManager.AppSettings["xero_demo_company_needs_to_remove"];
                    List<string> ignoreTenentList = null;
                    if (!string.IsNullOrEmpty(ignoreTenents))
                    {
                        ignoreTenentList = ignoreTenents.Split(',').ToList();
                    }
                    tenants = tenants.Where(i=> !ignoreTenentList.Contains(i.TenantName)).ToList();
                }
                return tenants;
            }
            catch(Exception ex)
            {
                m_log.Error(ex.Message + ex.StackTrace);
                return null;
            }
           
        }
    }
}