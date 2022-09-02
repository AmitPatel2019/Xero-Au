using Flexis.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CosmicCoreApi.Helper
{
    public class SessionHelper
    {
        //public int LoginID { get; set; }
        //public int AccountID { get; set; }
        //public int ReckonFileID { get; set; }
        //public int PlatformID { get; set; }
        //public int QboConnectID { get; set; }
        //public string QboAccessToken { get; set; }
        //public string QboRefreshToken { get; set; }
        //public DateTime? QboAccessTokenExpiresIn { get; set; }
        //public DateTime? QboRefreshTokenExpiresIn { get; set; }
        //public string QboRealmId { get; set; }
        //public string CountryOfOrigin { get; set; }

        // private int _platformID = 0;
        public int PlatformID
        {

            get
            {
                try
                {
                    return CSConvert.ToInt(HttpContext.Current.Items["PlatformID"]);
                }
                catch (Exception ex)
                {
                    Logger _log = CosmicLogger.SetLog();
                    _log.Error(ex);
                    return 0;
                }
            }

            set { HttpContext.Current.Items["PlatformID"] = value; }
        }

        //private int _loginID = 0;
        public int LoginID
        {
            get {

                try
                {
                    return CSConvert.ToInt(HttpContext.Current.Items["LoginID"]);
                }
                catch (Exception ex)
                {
                    Logger _log = CosmicLogger.SetLog();
                    _log.Error(ex);
                    return 0;
                }

            }

            set { HttpContext.Current.Items["LoginID"] = value; }
        }

        //private int _accountID = 0;
        public int AccountID
        {
            get {

                try
                {
                    if(HttpContext.Current == null || !HttpContext.Current.Items.Contains("AccountID"))
                    {
                        return 0;
                    }
                    return CSConvert.ToInt(HttpContext.Current.Items["AccountID"]);
                }
                catch (Exception ex)
                {
                    Logger _log = CosmicLogger.SetLog();
                    _log.Error(ex);
                    return 0;
                }

            }

            set { HttpContext.Current.Items["AccountID"] = value; }
        }

        //private int _reckonFileID = 0;
        public int ReckonFileID
        {
            get
            {
                try
                {
                    return CSConvert.ToInt(HttpContext.Current.Items["ReckonFileID"]);
                }
                catch (Exception ex)
                {
                    Logger _log = CosmicLogger.SetLog();
                    _log.Error(ex);
                    return 0;
                }
            }

            set { HttpContext.Current.Items["ReckonFileID"] = value; }
        }

        public int QboConnectID
        {
            get
            {
                try
                {
                    return CSConvert.ToInt(HttpContext.Current.Items["QboConnectID"]);
                }
                catch (Exception ex)
                {
                    Logger _log = CosmicLogger.SetLog();
                    _log.Error(ex);
                    return 0;
                }
            }

            set { HttpContext.Current.Items["QboConnectID"] = value; }
        }

        //private string _qboAccessToken = string.Empty;
        public string QboAccessToken
        {
            get {

                try
                {
                    return CSConvert.ToString(HttpContext.Current.Items["QboAccessToken"]);
                }
                catch (Exception ex)
                {
                    Logger _log = CosmicLogger.SetLog();
                    _log.Error(ex);
                    return string.Empty;
                }
            }

            set { HttpContext.Current.Items["QboAccessToken"] = value; }
        }

        //private string _qboRealmId = string.Empty;
        public string QboRealmId
        {
            get
            {

                try
                {
                    return CSConvert.ToString(HttpContext.Current.Items["QboRealmId"]);
                }
                catch (Exception ex)
                {
                    Logger _log = CosmicLogger.SetLog();
                    _log.Error(ex);
                    return string.Empty;
                }
            }

            set { HttpContext.Current.Items["QboRealmId"] = value; }
        }

        //private DateTime? _qboAccessTokenExpiresIn = null;
        public DateTime? QboAccessTokenExpiresIn
        {
            get
            {
                try
                {
                    if (HttpContext.Current.Items["QboAccessTokenExpiresIn"] != null)
                        return Convert.ToDateTime(HttpContext.Current.Items["QboAccessTokenExpiresIn"]);

                    return null;
                }
                catch (Exception ex)
                {
                    Logger _log = CosmicLogger.SetLog();
                    _log.Error(ex);
                    return null;
                }
            }

            set { HttpContext.Current.Items["QboAccessTokenExpiresIn"] = value; }
        }

        // private DateTime? _qboRefreshTokenExpiresIn = null;
        public DateTime? QboRefreshTokenExpiresIn
        {
            get
            {
                try
                {
                    if (HttpContext.Current.Items["QboRefreshTokenExpiresIn"] != null)
                        return Convert.ToDateTime(HttpContext.Current.Items["QboRefreshTokenExpiresIn"]);

                    return null;
                }
                catch (Exception ex)
                {
                    Logger _log = CosmicLogger.SetLog();
                    _log.Error(ex);
                    return null;
                }

            }

            set { HttpContext.Current.Items["QboRefreshTokenExpiresIn"] = value; }
        }

        //private string _qboRefreshToken = null;
        public string QboRefreshToken
        {
            get
            {
                try
                {
                    return CSConvert.ToString(HttpContext.Current.Items["QboRefreshToken"]);
                }
                catch (Exception ex)
                {
                    Logger _log = CosmicLogger.SetLog();
                    _log.Error(ex);
                    return string.Empty;
                }
            }

            set { HttpContext.Current.Items["QboRefreshToken"] = value; }
        }

        //private string _countryOfOrigin = null;
        public string CountryOfOrigin
        {
            get
            {
                try
                {
                    return CSConvert.ToString(HttpContext.Current.Items["CountryOfOrigin"]);
                }
                catch (Exception ex)
                {
                    Logger _log = CosmicLogger.SetLog();
                    _log.Error(ex);
                    return string.Empty;
                }
            }

            set { HttpContext.Current.Items["CountryOfOrigin"] = value; }
        }


        //private string XeroToken = string.Empty;
        public string XeroToken
        {
            get
            {

                try
                {
                    return CSConvert.ToString(HttpContext.Current.Items["XeroToken"]);
                }
                catch (Exception ex)
                {
                    Logger _log = CosmicLogger.SetLog();
                    _log.Error(ex);
                    return string.Empty;
                }
            }

            set { HttpContext.Current.Items["XeroToken"] = value; }
        }

        //private string XeroToken_Sec = string.Empty;
        public string XeroToken_Sec
        {
            get
            {

                try
                {
                    return CSConvert.ToString(HttpContext.Current.Items["XeroToken_Sec"]);
                }
                catch (Exception ex)
                {
                    Logger _log = CosmicLogger.SetLog();
                    _log.Error(ex);
                    return string.Empty;
                }
            }

            set { HttpContext.Current.Items["XeroToken_Sec"] = value; }
        }


        //private DateTime? _qboAccessTokenExpiresIn = null;
        public DateTime? XeroRefreshTokenExpiresIn
        {
            get
            {
                try
                {
                    if (HttpContext.Current.Items["XeroRefreshTokenExpiresIn"] != null)
                        return Convert.ToDateTime(HttpContext.Current.Items["XeroRefreshTokenExpiresIn"]);

                    return null;
                }
                catch (Exception ex)
                {
                    Logger _log = CosmicLogger.SetLog();
                    _log.Error(ex);
                    return null;
                }
            }

            set { HttpContext.Current.Items["QboAccessTokenExpiresIn"] = value; }
        }

        // private DateTime? _qboRefreshTokenExpiresIn = null;
        public DateTime? XeroExpiresIn
        {
            get
            {
                try
                {
                    if (HttpContext.Current.Items["XeroExpiresIn"] != null)
                        return Convert.ToDateTime(HttpContext.Current.Items["XeroExpiresIn"]);

                    return null;
                }
                catch (Exception ex)
                {
                    Logger _log = CosmicLogger.SetLog();
                    _log.Error(ex);
                    return null;
                }

            }

            set { HttpContext.Current.Items["QboRefreshTokenExpiresIn"] = value; }
        }

        //private string XeroOAuth_Token = null;
        public string XeroOAuth_Token_Sec
        {
            get
            {
                try
                {
                    return CSConvert.ToString(HttpContext.Current.Items["XeroOAuth_Token_Sec"]);
                }
                catch (Exception ex)
                {
                    Logger _log = CosmicLogger.SetLog();
                    _log.Error(ex);
                    return string.Empty;
                }
            }

            set { HttpContext.Current.Items["XeroOAuth_Token_Sec"] = value; }
        }

        //private string XeroOAuth_Token = null;
        public string XeroOAuth_Token
        {
            get
            {
                try
                {
                    return CSConvert.ToString(HttpContext.Current.Items["XeroOAuth_Token"]);
                }
                catch (Exception ex)
                {
                    Logger _log = CosmicLogger.SetLog();
                    _log.Error(ex);
                    return string.Empty;
                }
            }

            set { HttpContext.Current.Items["XeroOAuth_Token"] = value; }
        }

        public int XeroConnectID
        {
            get
            {
                try
                {
                    return CSConvert.ToInt(HttpContext.Current.Items["XeroConnectID"]);
                }
                catch (Exception ex)
                {
                    Logger _log = CosmicLogger.SetLog();
                    _log.Error(ex);
                    return 0;
                }
            }

            set { HttpContext.Current.Items["XeroConnectID"] = value; }
        }
        /*
        public IOAuthSession consumerSession
        {
            get
            {
                try
                {
                    return HttpContext.Current.Items["IOAuthSession"] as IOAuthSession;
                }
                catch (Exception ex)
                {
                    Logger _log = CosmicLogger.SetLog();
                    _log.Error(ex);
                    return null;
                }
            }

            set { HttpContext.Current.Items["IOAuthSession"] = value; }
        }

       */
        public string XeroRealmId
        {
            get
            {

                try
                {
                     if (HttpContext.Current == null || !HttpContext.Current.Items.Contains("XeroRealmId"))
                    {
                        return string.Empty;
                    }
                    return CSConvert.ToString(HttpContext.Current.Items["XeroRealmId"]);
                }
                catch (Exception ex)
                {
                    Logger _log = CosmicLogger.SetLog();
                    _log.Error(ex);
                    return string.Empty;
                }
            }

            set { HttpContext.Current.Items["XeroRealmId"] = value; }
        }

        public string SessionHandle
        {
            get
            {

                try
                {
                    return CSConvert.ToString(HttpContext.Current.Items["SessionHandle"]);
                }
                catch (Exception ex)
                {
                    Logger _log = CosmicLogger.SetLog();
                    _log.Error(ex);
                    return string.Empty;
                }
            }

            set { HttpContext.Current.Items["SessionHandle"] = value; }
        }
    }
}