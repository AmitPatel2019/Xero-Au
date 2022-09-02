using CosmicApiModel;
using CosmicCoreApi.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;
using XStreamline.Log;

namespace CosmicCoreApi.DBAccess
{
    public class CosDBResponse<T>
    {
        #region Properties

        public CosDBResponse(T value)
        {
            //genericMemberVariable = value;
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

    public enum DBResponseStatusCode
    {
        OK = 0,
        FAILED = 1,
        EMAIL_EXISTS = 2,
        EMAIL_NOT_EXISTS = 3
    }

    public  class CosmicContext : DbContext
    {
        private Logger _log;
        private Dictionary<string, object> sqlParaKeys = new Dictionary<string, object>();

        public CosmicContext()
            : base("name=Cosmic_Connection")
        {

        }


        private ErrorLog PrepareErrorLog(string controller, string method, string exceptionMessage, Exception ex)
        {
            ErrorLog errorLog = new ErrorLog();
            errorLog.Controller = controller;
            errorLog.Method = method;
            errorLog.AccountID = SessionHelper.AccountID;
            errorLog.LoginID = SessionHelper.LoginID;
            errorLog.PlatformID =  SessionHelper.PlatformID;
            errorLog.ReckonFileID =  SessionHelper.ReckonFileID;
            errorLog.QboConnectID = SessionHelper.QboConnectID;


            return errorLog;

        }

        public void LogErrorToDB(string controller, string method, string exceptionMessage, Exception ex)
        {
            try
            {
                var errorLog = PrepareErrorLog(controller, method, exceptionMessage, ex);

                SqlParameter sqlParaNum = null;
                SqlParameter sqlParaDesc = null;
                SqlParameter sqlParaIdentity = null;
                string sql = string.Empty;

                    

                sqlParaKeys.Clear();
                sqlParaKeys.Add("@AccountID", errorLog.AccountID ?? 0);
                sqlParaKeys.Add("@PlatformID", errorLog.PlatformID ?? 0);
                sqlParaKeys.Add("@LoginID", errorLog.LoginID ?? 0);
                sqlParaKeys.Add("@ReckonFileID", errorLog.ReckonFileID ?? 0);
                sqlParaKeys.Add("@QboConnectID", errorLog.QboConnectID ?? 0);
                sqlParaKeys.Add("@Controller", errorLog.Controller ?? string.Empty);
                sqlParaKeys.Add("@Method", errorLog.Method ?? string.Empty);
                sqlParaKeys.Add("@ErrorMessage", errorLog.ErrorMessage ?? string.Empty);
                sqlParaKeys.Add("@ErrorStackTrace", errorLog.ErrorStackTrace ?? string.Empty);
                sqlParaKeys.Add("@ErrorLogDate", errorLog.ErrorLogDate ?? null);
                sqlParaKeys.Add("@errNum", 0);
                sqlParaKeys.Add("@errDesc", string.Empty);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspSaveErrorLog]",
                                         ref sql, ref sqlParaNum, ref sqlParaDesc, ref sqlParaIdentity);


                Database.ExecuteSqlCommand(sql, sqlParameters);
            }
            catch (Exception Ex)
            {
                _log.Error(Ex);

            }
        }



        #region MyRegion

        private DBResponseStatusCode GetDBResponseStatusCode(string error)
        {
            error = Convert.ToString(error);
            return error.Length > 5 ? DBResponseStatusCode.FAILED : DBResponseStatusCode.OK;
        }

        private SqlParameter GetOutputErrNum()
        {
            SqlParameter sqlPara = new SqlParameter();
            sqlPara.Direction = ParameterDirection.Output;
            sqlPara.Value = 0;
            sqlPara.ParameterName = "@errNum";

            return sqlPara;
        }

        private SqlParameter GetOutputErrDesc()
        {
            SqlParameter sqlPara = new SqlParameter();
            sqlPara.Direction = ParameterDirection.Output;
            sqlPara.Value = string.Empty;
            sqlPara.Size = 4000;
            sqlPara.ParameterName = "@errDesc";

            return sqlPara;
        }

        private SqlParameter[] PrepareSql(Dictionary<string, object> sqlParas,
                                string procedureName, ref string sql,
                                ref SqlParameter sqlParaErrNum, ref SqlParameter sqlParaErrDesc, ref SqlParameter sqlParaIdentity)
        {
            //sqlParameters
            int index = 0;
            SqlParameter[] sqlParameters = new SqlParameter[sqlParas.Count];

            try
            {
                foreach (KeyValuePair<string, object> para in sqlParas)
                {
                    SqlParameter sqlParameter = new SqlParameter();

                    if (string.Compare("@errNum", para.Key, true) == 0)
                    {
                        sqlParameter.ParameterName = para.Key;
                        sqlParameter.Value = para.Value;
                        sqlParameter.Direction = ParameterDirection.Output;
                        sqlParameter.Size = 100;

                        sqlParaErrNum = sqlParameter;

                        sql = string.Format("{0} {1} output,", sql, para.Key);

                    }
                    else if (string.Compare("@errDesc", para.Key, true) == 0)
                    {
                        sqlParameter.ParameterName = para.Key;
                        sqlParameter.Value = para.Value;
                        sqlParameter.Direction = ParameterDirection.Output;
                        sqlParameter.Size = 4000;

                        sqlParaErrDesc = sqlParameter;

                        sql = string.Format("{0} {1} output,", sql, para.Key);
                    }
                    else if (string.Compare("@Identity", para.Key, true) == 0)
                    {
                        sqlParameter.ParameterName = para.Key;
                        sqlParameter.Value = para.Value;
                        sqlParameter.Direction = ParameterDirection.Output;
                        sqlParameter.Size = 100;

                        sqlParaIdentity = sqlParameter;

                        sql = string.Format("{0} {1} output,", sql, para.Key);
                    }
                    else
                    {
                        sqlParameter.ParameterName = para.Key;
                        sqlParameter.Value = para.Value;

                        sql = string.Format("{0} {1},", sql, para.Key);
                    }

                    sqlParameters[index] = sqlParameter;

                    index++;
                }

                sql = sql.Remove(sql.LastIndexOf(","), 1);

                sql = string.Format("EXEC {0} {1}", procedureName, sql);

                return sqlParameters;
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                return sqlParameters;
            }
        }

        private string FormatSqlErrorDescParam(SqlParameter sqlPara)
        {
            if (sqlPara == null) return string.Empty;
            return Convert.ToString(sqlPara.Value);
        }

        private int FormatSqlErrorNumParam(SqlParameter sqlPara)
        {
            if (sqlPara == null) return 0;
            return Convert.ToInt32(sqlPara.Value);
        }

        #endregion

        #region SubscriptionMaster
        internal CosDBResponse<AccountSubscribedPlan> GetAccountSubscriptionByAccountID(int accountID)
        {
            try
            {
                SqlParameter sqlParaNum = null;
                SqlParameter sqlParaDesc = null;
                SqlParameter sqlParaIdentity = null;
                string sql = string.Empty;

                sqlParaKeys.Clear();
                sqlParaKeys.Add("@AccountID", accountID);
                sqlParaKeys.Add("@errNum", 0);
                sqlParaKeys.Add("@errDesc", string.Empty);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspGetSubscriptionByAccountID]",
                                         ref sql, ref sqlParaNum, ref sqlParaDesc, ref sqlParaIdentity);

                var data = Database.SqlQuery<AccountSubscribedPlan>(sql, sqlParameters).FirstOrDefault();
                return CosDBResponse<AccountSubscribedPlan>.CreateDBResponse(data, FormatSqlErrorDescParam(sqlParaDesc));

            }
            catch (Exception ex)
            {
                _log.Error(ex);
                return CosDBResponse<AccountSubscribedPlan>.CreateDBResponse(null, ex.Message);
            }
        }
        #endregion

        #region AccountMaster 
        internal CosDBResponse<MyAccount> GetAccountMasterByAccountID(int accountID)
        {
            try
            {
                SqlParameter sqlParaNum = null;
                SqlParameter sqlParaDesc = null;
                SqlParameter sqlParaIdentity = null;
                string sql = string.Empty;

                sqlParaKeys.Clear();
                sqlParaKeys.Add("@AccountID", accountID);
                sqlParaKeys.Add("@errNum", 0);
                sqlParaKeys.Add("@errDesc", string.Empty);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspGetAccountByAccountID]",
                                         ref sql, ref sqlParaNum, ref sqlParaDesc, ref sqlParaIdentity);

                var data = Database.SqlQuery<MyAccount>(sql, sqlParameters).FirstOrDefault();
                return CosDBResponse<MyAccount>.CreateDBResponse(data, FormatSqlErrorDescParam(sqlParaDesc));

            }
            catch (Exception ex)
            {
                _log.Error(ex);
                return CosDBResponse<MyAccount>.CreateDBResponse(null, ex.Message);
            }

        }

        private CosDBResponse<DBResponseStatusCode> SendRegistrationEmail(AccountMaster accountMaster)
        {
            try
            {
                //1 .Generate Random Password and Activation Code for Customer/User
                accountMaster.Password = RandomHelper.GetRandomString(RandomString.UserPassword);
                accountMaster.ActivationCode = RandomHelper.GetRandomString(RandomString.AccountActivationCode);

                //2. Save the Account in database
                var dbResp = SaveAccount(accountMaster);
                if (dbResp == null) return dbResp;
                if (dbResp.StatusCode == DBResponseStatusCode.FAILED) return dbResp;

                //Sending email to customer with random password and activation link
                //3. Get Email Body 
                string emailBody = SMTPHelper.GetEmailBody(MailTemplate.AccountRegistration);

                //4. Prepare Activation link
                EmailBodyReplacement ebReplacement = new EmailBodyReplacement();
                SMTPHelper.GetAccountActivationLink(ebReplacement, accountMaster.Email, accountMaster.ActivationCode);
                ebReplacement.CustomerName = accountMaster.CustomerName;
                ebReplacement.ActivationCode = accountMaster.ActivationCode;

                //5.  format email body
                emailBody = SMTPHelper.FormatEmailBody(MailTemplate.AccountRegistration, emailBody, ebReplacement);

                //Initilize SMTP setting (server, port sender, to address etc)
                var smtpSetting = SMTPHelper.InitCosmicSMPTSetting(
                                        CSConvert.ToString(ConfigurationManager.AppSettings["Cosmic_Email_Subject_Registeration"]),
                                         accountMaster.Email, emailBody);

                return SMTPHelper.SendEmail(smtpSetting);
            }
            catch (Exception ex)
            {
                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(DBResponseStatusCode.FAILED, ex.Message);
            }
        }

        internal CosDBResponse<DBResponseStatusCode> CreateAccount(AccountMaster accountMaster)
        {
            try
            {

                //0 Check if email address is already exist in the system
                var resp = GetLoginMasterByEmail(accountMaster.Email);

                if (resp.Data != null)
                {
                    var loginMaster = resp.Data as LoginMaster;
                    if (loginMaster != null)
                    {
                        if (loginMaster.Active??false)
                        {
                            return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(DBResponseStatusCode.EMAIL_EXISTS, "An account already exists with Email Address you trying to register with");
                        }
                        else
                        {
                            //Send Email With Action Code
                            return SendRegistrationEmail(accountMaster);
                        }
                    }
                }

                return SendRegistrationEmail(accountMaster);
            }
            catch (Exception ex)
            {
                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(DBResponseStatusCode.FAILED, ex.Message);
            }

        }

        internal CosDBResponse<DBResponseStatusCode> SaveAccount(AccountMaster accountMaster)
        {
            try
            {
                SqlParameter sqlParaNum = null;
                SqlParameter sqlParaDesc = null;
                SqlParameter sqlParaIdentity = null;
                string sql = string.Empty;

                sqlParaKeys.Clear();
                sqlParaKeys.Add("@PlatformID", accountMaster.PlatformID);
                sqlParaKeys.Add("@AccountName", accountMaster.AccountName);
                sqlParaKeys.Add("@CustomerName", accountMaster.CustomerName);
                sqlParaKeys.Add("@Phone", accountMaster.Phone ?? string.Empty );
                sqlParaKeys.Add("@Add1", accountMaster.Add1 ?? string.Empty);
                sqlParaKeys.Add("@Add2", accountMaster.Add2 ?? string.Empty );
                sqlParaKeys.Add("@Add3", accountMaster.Add3 ?? string.Empty );
                sqlParaKeys.Add("@City", accountMaster.City ?? string.Empty );
                sqlParaKeys.Add("@State", accountMaster.State ?? string.Empty );
                sqlParaKeys.Add("@Zip", accountMaster.Zip ?? string.Empty );
                sqlParaKeys.Add("@Country", accountMaster.Country ?? string.Empty );
                sqlParaKeys.Add("@Email", accountMaster.Email);
                sqlParaKeys.Add("@Password", accountMaster.Password);
                sqlParaKeys.Add("@ActivationCode", accountMaster.ActivationCode);
                sqlParaKeys.Add("@errNum", 0);
                sqlParaKeys.Add("@errDesc", string.Empty);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspSaveAccount]",
                                         ref sql, ref sqlParaNum, ref sqlParaDesc, ref sqlParaIdentity);

                Database.ExecuteSqlCommand(sql, sqlParameters);

                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(GetDBResponseStatusCode(FormatSqlErrorDescParam(sqlParaDesc)), FormatSqlErrorDescParam(sqlParaDesc));
            }
            catch (Exception ex)
            {
                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(DBResponseStatusCode.FAILED, ex.Message);
            }

        }

        internal CosDBResponse<AccountMaster> GetAccountMasterByEmailAndCode(string emailAddress, string code)
        {
            try
            {
                int errNum = 0;
                SqlParameter[] parameters = {
                    new SqlParameter ("@Email",emailAddress),
                    new SqlParameter("@Code",code),
                    new SqlParameter("@errNum",errNum),
                    new SqlParameter("@errDesc",string.Empty),

                };

                var data = Database.SqlQuery<AccountMaster>("EXEC [uspValidateActivationCode] @Email,@Code,@errNum,@errDesc", parameters).FirstOrDefault();
                return CosDBResponse<AccountMaster>.CreateDBResponse(data, FormatSqlErrorDescParam(parameters[3]));

            }
            catch (Exception ex)
            {
                _log.Error(ex);
                return CosDBResponse<AccountMaster>.CreateDBResponse(null, ex.Message);
            }
        }

        internal CosDBResponse<DBResponseStatusCode> UpdateUserProfile(AccountMaster accountMaster)
        {

            try
            {
                SqlParameter sqlParaNum = null;
                SqlParameter sqlParaDesc = null;
                SqlParameter sqlParaIdentity = null;
                string sql = string.Empty;

                sqlParaKeys.Clear();
                sqlParaKeys.Add("@AccountID", accountMaster.AccountID);
                sqlParaKeys.Add("@CustomerName", accountMaster.CustomerName);
                sqlParaKeys.Add("@Phone", accountMaster.Phone ?? string.Empty );
                sqlParaKeys.Add("@Add1", accountMaster.Add1 ?? string.Empty);
                sqlParaKeys.Add("@Add2", accountMaster.Add2 ?? string.Empty);
                sqlParaKeys.Add("@Add3", accountMaster.Add3 ?? string.Empty);
                sqlParaKeys.Add("@City", accountMaster.City ?? string.Empty);
                sqlParaKeys.Add("@State", accountMaster.State ?? string.Empty);
                sqlParaKeys.Add("@Zip", accountMaster.Zip ?? string.Empty);
                sqlParaKeys.Add("@Country", accountMaster.Country ?? string.Empty);
                sqlParaKeys.Add("@errNum", 0);
                sqlParaKeys.Add("@errDesc", string.Empty);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspUpdateProfile]",
                                         ref sql, ref sqlParaNum, ref sqlParaDesc, ref sqlParaIdentity);

                Database.ExecuteSqlCommand(sql, sqlParameters);

                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(GetDBResponseStatusCode(FormatSqlErrorDescParam(sqlParaDesc)), FormatSqlErrorDescParam(sqlParaDesc));


            }
            catch (Exception ex)
            {
                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(DBResponseStatusCode.FAILED, ex.Message);
            }
        }

        #endregion AccountMaster

        #region LoginMaster

        internal CosDBResponse<DBResponseStatusCode> SaveLoginMaster(LoginMaster loginMaster)
        {

            try
            {
                SqlParameter sqlParaNum = null;
                SqlParameter sqlParaDesc = null;
                SqlParameter sqlParaIdentity = null;
                string sql = string.Empty;

                sqlParaKeys.Clear();

                sqlParaKeys.Add("@", loginMaster.ActivationCode);
                sqlParaKeys.Add("@LoginID", loginMaster.LoginID);
                sqlParaKeys.Add("@errNum", 0);
                sqlParaKeys.Add("@errDesc", string.Empty);
                //uspUpdateLogin

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspUpdateLogin]",
                                        ref sql, ref sqlParaNum, ref sqlParaDesc, ref sqlParaIdentity);

                Database.ExecuteSqlCommand(sql, sqlParameters);

                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(GetDBResponseStatusCode(FormatSqlErrorDescParam(sqlParaDesc)), FormatSqlErrorDescParam(sqlParaDesc));
            }
            catch (Exception ex)
            {
                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(DBResponseStatusCode.FAILED, ex.Message);
            }
        }

        internal string UpdateUserToken(LoginMaster login)
        {
            login.Token = Guid.NewGuid().ToString();

            if ((login.TokenAvailFor ?? 0) > 0)
                login.TokenExpiredDate = DateTime.Now.AddDays((login.TokenAvailFor ?? 0));

            if ((login.TokenAvailFor ?? 0) == -1)
                login.TokenExpiredDate = DateTime.Now.AddMonths(1);

            if ((login.TokenAvailFor ?? 0) <= 0)
                login.TokenExpiredDate = DateTime.Now.AddDays(1);

            //Update Token / TokenExpiredDate in Login Master 
            SqlParameter sqlParaNum = null;
            SqlParameter sqlParaDesc = null;
            SqlParameter sqlParaIdentity = null;
            string sql = string.Empty;

            try
            {
                sqlParaKeys.Clear();
                sqlParaKeys.Add("@LoginID", login.LoginID);
                sqlParaKeys.Add("@Token", login.Token);
                sqlParaKeys.Add("@TokenExpiredDate", login.TokenExpiredDate);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspUpdateToken]",
                                         ref sql, ref sqlParaNum, ref sqlParaDesc, ref sqlParaIdentity);

                Database.ExecuteSqlCommand(sql, sqlParameters);
            }
            catch (Exception ex)
            {
                _log.Error(ex);
            }

            if (GetDBResponseStatusCode(FormatSqlErrorDescParam(sqlParaDesc)) == DBResponseStatusCode.OK)
            {
                return login.Token;
            }


            return "Error while generating user token";

        }

        internal CosDBResponse<LoginMaster> VerifyUserNameAndPassword(LoginMaster loginMaster)
        {
            try
            {
                var resp = GetLoginMasterByEmail(loginMaster.EmailAddress);
                if (resp.Data != null)
                {
                    var login = resp.Data as LoginMaster;
                    if (login.Password == loginMaster.Password)
                    {

                        login.Password = string.Empty;
                        UpdateUserToken(login);


                        return CosDBResponse<LoginMaster>.CreateDBResponse(login, string.Empty);
                    }
                }
                return CosDBResponse<LoginMaster>.CreateDBResponse(null, "Incorrect Credentials");


            }
            catch (Exception ex)
            {
                return CosDBResponse<LoginMaster>.CreateDBResponse(null, ex.Message);
            }
        }

        internal CosDBResponse<DBResponseStatusCode> ProcessForgotPwd(LoginMaster loginMaster)
        {
            try
            {
                var resp = GetLoginMasterByEmail(loginMaster.EmailAddress);
                if (resp.Data != null)
                {
                    var login = resp.Data as LoginMaster;
                    if (login.EmailAddress == loginMaster.EmailAddress)
                    {
                        // Genrate activation code 1
                        loginMaster.ActivationCode = RandomHelper.GetRandomString(RandomString.AccountActivationCode);
                        loginMaster.LoginID = login.LoginID;

                        // Save loginmaster in actvation code  2
                        var dbResponce = SaveLoginMaster(loginMaster);
                        if (dbResponce == null) return dbResponce;
                        if (dbResponce.StatusCode == DBResponseStatusCode.FAILED) return dbResponce;

                        // Send mail processing 

                        string emailBody = SMTPHelper.GetEmailBody(MailTemplate.ForgotPassword);
                        EmailBodyReplacement ebReplacement = new EmailBodyReplacement();
                        SMTPHelper.GetAccountActivationLink(ebReplacement, loginMaster.EmailAddress, loginMaster.ActivationCode);
                        //ebReplacement.CustomerName = accountMaster.CustomerName;
                        ebReplacement.ActivationCode = loginMaster.ActivationCode;

                        //5.  format email body
                        emailBody = SMTPHelper.FormatEmailBody(MailTemplate.ForgotPassword, emailBody, ebReplacement);

                        //Initilize SMTP setting (server, port sender, to address etc)
                        var smtpSetting = SMTPHelper.InitCosmicSMPTSetting(
                                                 ConfigurationManager.AppSettings["Cosmic_Email_Subject_Forgetpassword"],
                                                 loginMaster.EmailAddress, emailBody);

                        return SMTPHelper.SendEmail(smtpSetting);

                    }

                }

                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(DBResponseStatusCode.EMAIL_NOT_EXISTS, "Could not find account by given email address");

            }
            catch (Exception ex)
            {
                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(DBResponseStatusCode.FAILED, ex.Message);
            }
        }

        internal CosDBResponse<LoginMaster> GetLoginMasterByEmail(string emailAddress)
        {
            try
            {
                SqlParameter sqlParaNum = null;
                SqlParameter sqlParaDesc = null;
                SqlParameter sqlParaIdentity = null;
                string sql = string.Empty;

                sqlParaKeys.Clear();
                sqlParaKeys.Add("@Email", emailAddress);
                sqlParaKeys.Add("@errNum", 0);
                sqlParaKeys.Add("@errDesc", string.Empty);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspGetLoginMasterByEmail]",
                                        ref sql, ref sqlParaNum, ref sqlParaDesc, ref sqlParaIdentity);

                //Database.ExecuteSqlCommand(sql, sqlParameters);

                var data = Database.SqlQuery<LoginMaster>(sql, sqlParameters).FirstOrDefault();
                return CosDBResponse<LoginMaster>.CreateDBResponse(data, FormatSqlErrorDescParam(sqlParaDesc));
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                return CosDBResponse<LoginMaster>.CreateDBResponse(null, ex.Message);
            }
        }

        internal CosDBResponse<LoginMaster> GetLoginMasterByToken(string token)
        {
            try
            {
                SqlParameter sqlParaNum = null;
                SqlParameter sqlParaDesc = null;
                SqlParameter sqlParaIdentity = null;
                string sql = string.Empty;

                sqlParaKeys.Clear();
                sqlParaKeys.Add("@Token", token);
                sqlParaKeys.Add("@errNum", 0);
                sqlParaKeys.Add("@errDesc", string.Empty);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspGetLoginMasterByToken]",
                                        ref sql, ref sqlParaNum, ref sqlParaDesc, ref sqlParaIdentity);

               // Database.ExecuteSqlCommand(sql, sqlParameters);

                var data = Database.SqlQuery<LoginMaster>(sql, sqlParameters).FirstOrDefault();
                return CosDBResponse<LoginMaster>.CreateDBResponse(data, FormatSqlErrorDescParam(sqlParaDesc));
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                return CosDBResponse<LoginMaster>.CreateDBResponse(null, ex.Message);
            }
        }

        internal CosDBResponse<DBResponseStatusCode> ActivateAccount(LoginMaster loginMaster)
        {
            try
            {
                SqlParameter sqlParaNum = null;
                SqlParameter sqlParaDesc = null;
                SqlParameter sqlParaIdentity = null;
                string sql = string.Empty;

                sqlParaKeys.Clear();
                sqlParaKeys.Add("@EmailAddress", loginMaster.EmailAddress);
                sqlParaKeys.Add("@Password", loginMaster.Password);
                sqlParaKeys.Add("@errNum", 0);
                sqlParaKeys.Add("@errDesc", string.Empty);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspActivateAccount]",
                                        ref sql, ref sqlParaNum, ref sqlParaDesc, ref sqlParaIdentity);

                Database.ExecuteSqlCommand(sql, sqlParameters);

                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(GetDBResponseStatusCode(FormatSqlErrorDescParam(sqlParaDesc)), FormatSqlErrorDescParam(sqlParaDesc));
            }
            catch (Exception ex)
            {
                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(DBResponseStatusCode.FAILED, ex.Message);
            }

        }

        internal CosDBResponse<DBResponseStatusCode> AddEditUser(LoginMaster loginMaster)
        {
            try
            {
                SqlParameter sqlParaNum = null;
                SqlParameter sqlParaDesc = null;
                SqlParameter sqlParaIdentity = null;
                string sql = string.Empty;

                sqlParaKeys.Clear();
                sqlParaKeys.Add("@AccountID", SessionHelper.AccountID);
                sqlParaKeys.Add("@Name", loginMaster.Name);
                sqlParaKeys.Add("@EmailAddress", loginMaster.EmailAddress);
                sqlParaKeys.Add("@Password", loginMaster.Password);
                sqlParaKeys.Add("@LoginID", loginMaster.LoginID);
                sqlParaKeys.Add("@errNum", 0);
                sqlParaKeys.Add("@errDesc", string.Empty);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspSaveLogin]",
                                        ref sql, ref sqlParaNum, ref sqlParaDesc, ref sqlParaIdentity);

                Database.ExecuteSqlCommand(sql, sqlParameters);

                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(GetDBResponseStatusCode(FormatSqlErrorDescParam(sqlParaDesc)), FormatSqlErrorDescParam(sqlParaDesc));
            }
            catch (Exception ex)
            {
                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(DBResponseStatusCode.FAILED, ex.Message);
            }

        }

        internal CosDBResponse<DBResponseStatusCode> ValidateActivationCode(AccountMaster accountMaster)
        {

            try
            {
                var resp = GetAccountMasterByEmailAndCode(accountMaster.Email, accountMaster.ActivationCode);
                if (resp.Data != null)
                {
                    var loginMaster = resp.Data as AccountMaster;
                    if (loginMaster != null)
                    {
                        return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(DBResponseStatusCode.OK, "Matched emailaddress and activation code");
                    }
                }
                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(DBResponseStatusCode.FAILED, string.Empty);

            }
            catch (Exception ex)
            {
                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(DBResponseStatusCode.FAILED, ex.Message);
            }

        }

        internal CosDBResponse<DBResponseStatusCode> ValidateActivationCodeInloginMaster(LoginMaster loginMaster)
        {

            try
            {
                var resp = GetLoginMasterByEmail(loginMaster.EmailAddress);
                if (resp.Data != null)
                {
                    var login = resp.Data as LoginMaster;
                    if (login.ActivationCode == loginMaster.ActivationCode)
                    {
                        return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(DBResponseStatusCode.OK, "Matched emailaddress and activation code");
                    }
                }
                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(DBResponseStatusCode.FAILED, string.Empty);

            }
            catch (Exception ex)
            {
                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(DBResponseStatusCode.FAILED, ex.Message);
            }

        }

        internal CosDBResponse<DBResponseStatusCode> ChangePassword(LoginMaster loginMaster)
        {
            try
            {

                SqlParameter sqlParaNum = null;
                SqlParameter sqlParaDesc = null;
                SqlParameter sqlParaIdentity = null;
                string sql = string.Empty;

                sqlParaKeys.Clear();
                sqlParaKeys.Add("@Email", loginMaster.EmailAddress);
                sqlParaKeys.Add("@ExistsPassword", loginMaster.ExistsPassword);
                sqlParaKeys.Add("@NewPassword", loginMaster.NewPassword);
                sqlParaKeys.Add("@errNum", 0);
                sqlParaKeys.Add("@errDesc", string.Empty);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspChangePassword]",
                                         ref sql, ref sqlParaNum, ref sqlParaDesc, ref sqlParaIdentity);


                Database.ExecuteSqlCommand(sql, sqlParameters);

                if (CSConvert.ToInt(sqlParaNum.Value) == -1)
                {
                    return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(DBResponseStatusCode.FAILED, "Your existing password is incorrect");
                }
                else
                {
                    return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(GetDBResponseStatusCode(CSConvert.ToString(sqlParaDesc.Value)), CSConvert.ToString(sqlParaDesc.Value));
                }
            }
            catch (Exception ex)
            {
                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(DBResponseStatusCode.FAILED, ex.Message);

            }



        }

        internal CosDBResponse<ICollection<LoginMaster>> GetLoginMasterDetail()
        {
            try
            {
                SqlParameter sqlParaNum = null;
                SqlParameter sqlParaDesc = null;
                SqlParameter sqlParaIdentity = null;
                string sql = string.Empty;

                sqlParaKeys.Clear();
                sqlParaKeys.Add("@errNum", 0);
                sqlParaKeys.Add("@errDesc", string.Empty);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspGetLoginMaster]",
                                        ref sql, ref sqlParaNum, ref sqlParaDesc, ref sqlParaIdentity);

                var data = Database.SqlQuery<LoginMaster>(sql, sqlParameters).ToList();
                return CosDBResponse<ICollection<LoginMaster>>.CreateDBResponse(data, FormatSqlErrorDescParam(sqlParaDesc));
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                return CosDBResponse<ICollection<LoginMaster>>.CreateDBResponse(null, ex.Message);
            }

        }

        #endregion LoginMaster

        #region Ezzy Master

        internal CosDBResponse<EzzyAccount> GetEzzyLogin()
        {
            try
            {
                SqlParameter sqlParaNum = null;
                SqlParameter sqlParaDesc = null;
                SqlParameter sqlParaIdentity = null;
                string sql = string.Empty;

                sqlParaKeys.Clear();
                sqlParaKeys.Add("@errNum", 0);
                sqlParaKeys.Add("@errDesc", string.Empty);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspGetEzzyLogin]",
                                        ref sql, ref sqlParaNum, ref sqlParaDesc, ref sqlParaIdentity);

                //Database.ExecuteSqlCommand(sql, sqlParameters);

                var data = Database.SqlQuery<EzzyAccount>(sql, sqlParameters).FirstOrDefault();
                return CosDBResponse<EzzyAccount>.CreateDBResponse(data, FormatSqlErrorDescParam(sqlParaDesc));
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                return CosDBResponse<EzzyAccount>.CreateDBResponse(null, ex.Message);
            }
        }

        #endregion

        #region PlatformMaster

        internal CosDBResponse<DBResponseStatusCode> SavePlatformMaster(PlatformMaster platformMaster)
        {
            
            try
            {
                SqlParameter sqlParaNum = null;
                SqlParameter sqlParaDesc = null;
                SqlParameter sqlParaIdentity = null;
                string sql = string.Empty;

                sqlParaKeys.Clear();
                sqlParaKeys.Add("@PlatformCode", platformMaster.PlatformCode);
                sqlParaKeys.Add("@PlatformName", platformMaster.PlatformName);
                sqlParaKeys.Add("@Description", platformMaster.Description);
                sqlParaKeys.Add("@Active", platformMaster.Active);
                sqlParaKeys.Add("@errNum", 0);
                sqlParaKeys.Add("@errDesc", string.Empty);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspSavePlatform]",
                                        ref sql, ref sqlParaNum, ref sqlParaDesc, ref sqlParaIdentity);

                Database.ExecuteSqlCommand(sql, sqlParameters);

                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(GetDBResponseStatusCode(FormatSqlErrorDescParam(sqlParaDesc)), FormatSqlErrorDescParam(sqlParaDesc));

            }
            catch (Exception ex)
            {
                _log.Error(ex);
                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(DBResponseStatusCode.FAILED, ex.Message);
            }

        }

        internal CosDBResponse<PlatformMaster> GetPlatformMasterDetailByID(int uid)
        {
            try
            {
                SqlParameter sqlParaNum = null;
                SqlParameter sqlParaDesc = null;
                SqlParameter sqlParaIdentity = null;
                string sql = string.Empty;

                sqlParaKeys.Clear();
                sqlParaKeys.Add("@errNum", 0);

                sqlParaKeys.Add("@errNum", 0);
                sqlParaKeys.Add("@errDesc", string.Empty);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspGetPlatformMasterByID]",
                                        ref sql, ref sqlParaNum, ref sqlParaDesc, ref sqlParaIdentity);

                var data = Database.SqlQuery<PlatformMaster>(sql, sqlParameters).FirstOrDefault();
                return CosDBResponse<PlatformMaster>.CreateDBResponse(data, FormatSqlErrorDescParam(sqlParaDesc));
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                return CosDBResponse<PlatformMaster>.CreateDBResponse(null, ex.Message);
            }

        }

        internal CosDBResponse<ICollection<PlatformMaster>> GetPlatformMasterDetail()
        {
            try
            {
                SqlParameter sqlParaNum = null;
                SqlParameter sqlParaDesc = null;
                SqlParameter sqlParaIdentity = null;
                string sql = string.Empty;

                sqlParaKeys.Clear();
                sqlParaKeys.Add("@errNum", 0);
                sqlParaKeys.Add("@errDesc", string.Empty);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspGetPlatformMaster]",
                                        ref sql, ref sqlParaNum, ref sqlParaDesc, ref sqlParaIdentity);

                var data = Database.SqlQuery<PlatformMaster>(sql, sqlParameters).ToList();
                return CosDBResponse<ICollection<PlatformMaster>>.CreateDBResponse(data, FormatSqlErrorDescParam(sqlParaDesc));
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                return CosDBResponse<ICollection<PlatformMaster>>.CreateDBResponse(null, ex.Message);
            }

        }

        #endregion PlatformMaster

        #region PlanMaster
        internal CosDBResponse<ICollection<PlanMaster>> GetAccountPlan()
        {
            try
            {
                SqlParameter sqlParaNum = null;
                SqlParameter sqlParaDesc = null;
                SqlParameter sqlParaIdentity = null;
                string sql = string.Empty;

                sqlParaKeys.Clear();
                sqlParaKeys.Add("@errNum", 0);
                sqlParaKeys.Add("@errDesc", string.Empty);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspGetPlan]",
                                        ref sql, ref sqlParaNum, ref sqlParaDesc, ref sqlParaIdentity);

                var data = Database.SqlQuery<PlanMaster>(sql, sqlParameters).ToList();
                return CosDBResponse<ICollection<PlanMaster>>.CreateDBResponse(data, FormatSqlErrorDescParam(sqlParaDesc));

            }
            catch (Exception ex)
            {
                _log.Error(ex);
                return CosDBResponse<ICollection<PlanMaster>>.CreateDBResponse(null, ex.Message);
            }
        }
        #endregion PlanMaster

        #region ReconDesktopMaster

        internal CosDBResponse<DBResponseStatusCode> SaveReconDesktopMaster(ReckonDesktopMaster reckonDesktopMaster)
        {

            try
            {

                SqlParameter sqlParaNum = null;
                SqlParameter sqlParaDesc = null;
                SqlParameter sqlParaIdentity = null;
                string sql = string.Empty;

                sqlParaKeys.Clear(); 
                sqlParaKeys.Add("@ReckonFileID", reckonDesktopMaster.ReckonFileID);
                sqlParaKeys.Add("@AccountID", SessionHelper.AccountID);
                sqlParaKeys.Add("@CompanyName", reckonDesktopMaster.CompanyName ?? string.Empty);
                sqlParaKeys.Add("@LegalCompanyName", reckonDesktopMaster.LegalCompanyName ?? string.Empty);
                sqlParaKeys.Add("@IsSampleCompany", (reckonDesktopMaster.IsSampleCompany ?? false));
                sqlParaKeys.Add("@Phone", reckonDesktopMaster.Phone ?? string.Empty);
                sqlParaKeys.Add("@Email", reckonDesktopMaster.Email ?? string.Empty);
                sqlParaKeys.Add("@Addr1", reckonDesktopMaster.Addr1 ?? string.Empty);
                sqlParaKeys.Add("@Addr2", reckonDesktopMaster.Addr2 ?? string.Empty);
                sqlParaKeys.Add("@Addr3", reckonDesktopMaster.City ?? string.Empty);
                sqlParaKeys.Add("@State", reckonDesktopMaster.Addr3 ?? string.Empty);
                sqlParaKeys.Add("@City", reckonDesktopMaster.State ?? string.Empty);
                sqlParaKeys.Add("@PostalCode", reckonDesktopMaster.PostalCode ?? string.Empty);
                sqlParaKeys.Add("@Country", reckonDesktopMaster.Country ?? string.Empty);
                sqlParaKeys.Add("@FilePath", reckonDesktopMaster.FilePath ?? string.Empty);
                sqlParaKeys.Add("@errNum", 0);
                sqlParaKeys.Add("@errDesc", string.Empty);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspSaveReckonDesktop]",
                                         ref sql, ref sqlParaNum, ref sqlParaDesc, ref sqlParaIdentity);


                Database.ExecuteSqlCommand(sql, sqlParameters);

               
               return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(GetDBResponseStatusCode(CSConvert.ToString(sqlParaDesc.Value)), CSConvert.ToString(sqlParaDesc.Value));
               
            }
            catch (Exception ex)
            {
                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(DBResponseStatusCode.FAILED, ex.Message);

            }

           
        }

        internal CosDBResponse<DBResponseStatusCode> SaveReckonVendDefault(ReckonVendDefault reckonVendDefault)
        {            
            try
            {
                SqlParameter sqlParaNum = null;
                SqlParameter sqlParaDesc = null;
                SqlParameter sqlParaIdentity = null;


                string sql = string.Empty;

                sqlParaKeys.Clear();
                sqlParaKeys.Add("@ID", reckonVendDefault.ID);
                sqlParaKeys.Add("@AccountID", SessionHelper.AccountID);
                sqlParaKeys.Add("@ReckonFileID", SessionHelper.ReckonFileID);
                sqlParaKeys.Add("@VendorListID", reckonVendDefault.VendorListID);

                sqlParaKeys.Add("@DefaultExpense", reckonVendDefault.DefaultExpenseAccount ?? string.Empty);
                sqlParaKeys.Add("@DefaultItem", reckonVendDefault.DefaultItem ?? string.Empty);
                sqlParaKeys.Add("@DefaultExpenseListID", reckonVendDefault.DefaultExpenseListID ?? string.Empty);
                sqlParaKeys.Add("@DefaultItemListID", reckonVendDefault.DefaultItemListID ?? string.Empty);

                sqlParaKeys.Add("@AddedBy", SessionHelper.LoginID);
                sqlParaKeys.Add("@AddedDate", DateTime.Now);
                sqlParaKeys.Add("@UpdatedBy", SessionHelper.LoginID);
                sqlParaKeys.Add("@UpdatedDate", DateTime.Now);


                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspSaveReckonVendorDefault]",
                                         ref sql, ref sqlParaNum, ref sqlParaDesc, ref sqlParaIdentity);

                Database.ExecuteSqlCommand(sql, sqlParameters);

                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(GetDBResponseStatusCode(FormatSqlErrorDescParam(sqlParaDesc)), FormatSqlErrorDescParam(sqlParaDesc));
            }
            catch (Exception ex)
            {
                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(DBResponseStatusCode.FAILED, ex.Message);
            }
        }

        internal CosDBResponse<DBResponseStatusCode> SaveAllReckonVendDefault(ICollection<ReckonVendDefault> lstReckonVendDefault)
        {
            //uspSaveReckonVendorDefault

            try
            {
                SqlParameter sqlParaNum = null;
                SqlParameter sqlParaDesc = null;
                SqlParameter sqlParaIdentity = null;

                foreach (var itm in lstReckonVendDefault)
                {
                    string sql = string.Empty;

                    sqlParaKeys.Clear();
                    sqlParaKeys.Add("@ID", itm.ID);
                    sqlParaKeys.Add("@AccountID", SessionHelper.AccountID);
                    sqlParaKeys.Add("@ReckonFileID", SessionHelper.ReckonFileID);
                    sqlParaKeys.Add("@VendorListID", itm.VendorListID);

                    sqlParaKeys.Add("@DefaultExpense", itm.DefaultExpenseAccount ?? string.Empty );
                    sqlParaKeys.Add("@DefaultItem", itm.DefaultItem ?? string.Empty);
                    sqlParaKeys.Add("@DefaultExpenseListID", itm.DefaultExpenseListID ?? string.Empty);
                    sqlParaKeys.Add("@DefaultItemListID", itm.DefaultItemListID ?? string.Empty);

                    sqlParaKeys.Add("@AddedBy", SessionHelper.LoginID);
                    sqlParaKeys.Add("@AddedDate", DateTime.Now);
                    sqlParaKeys.Add("@UpdatedBy", SessionHelper.LoginID);
                    sqlParaKeys.Add("@UpdatedDate", DateTime.Now);


                    SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspSaveReckonVendorDefault]",
                                             ref sql, ref sqlParaNum, ref sqlParaDesc, ref sqlParaIdentity);

                    Database.ExecuteSqlCommand(sql, sqlParameters);
                }
                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(GetDBResponseStatusCode(FormatSqlErrorDescParam(sqlParaDesc)), FormatSqlErrorDescParam(sqlParaDesc));
            }
            catch (Exception ex)
            {
                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(DBResponseStatusCode.FAILED, ex.Message);
            }
        }

        internal CosDBResponse<ICollection<ReckonVendDefault>> GetReckonVendDefault()
        {
            try
            {
                try
                {
                    SqlParameter sqlParaNum = null;
                    SqlParameter sqlParaDesc = null;
                    SqlParameter sqlParaIdentity = null;
                    string sql = string.Empty;

                    sqlParaKeys.Clear();
                    sqlParaKeys.Add("@AccountID", SessionHelper.AccountID);
                    sqlParaKeys.Add("@ReckonFileID", SessionHelper.ReckonFileID);
                    sqlParaKeys.Add("@errNum", 0);
                    sqlParaKeys.Add("@errDesc", string.Empty);

                    SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspGetReckonVendDefault]",
                                             ref sql, ref sqlParaNum, ref sqlParaDesc, ref sqlParaIdentity);

                    var data = Database.SqlQuery<ReckonVendDefault>(sql, sqlParameters).ToList();

                    return CosDBResponse<ICollection<ReckonVendDefault>>.CreateDBResponse(data, FormatSqlErrorDescParam(sqlParaDesc));

                }
                catch (Exception ex)
                {
                    _log.Error(ex);
                    return CosDBResponse<ICollection<ReckonVendDefault>>.CreateDBResponse(null, ex.Message);
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                return CosDBResponse<ICollection<ReckonVendDefault>>.CreateDBResponse(null, ex.Message);
            }

        }

        internal CosDBResponse<ReckonDesktopMaster> GetReconDesktopMasterDetailByID(int uid)
        {
            try
            {

                int errNum = 0;
                SqlParameter[] parameters = {
                    new SqlParameter("@errNum",errNum),
                    new SqlParameter("@errDesc",""),
                    new SqlParameter ("@Uid",uid)
                };

                var data = Database.SqlQuery<ReckonDesktopMaster>("EXEC [uspGetReconDesktopMasterByID] @Uid, @errNum,@errDesc ", parameters).FirstOrDefault();
                return CosDBResponse<ReckonDesktopMaster>.CreateDBResponse(data, FormatSqlErrorDescParam(parameters[1]));
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                return CosDBResponse<ReckonDesktopMaster>.CreateDBResponse(null, ex.Message);
            }

        }

        internal CosDBResponse<ICollection<ReckonDesktopMaster>> GetAllReckonFileByAccountID()
        {
            try
            {
                SqlParameter sqlParaNum = null;
                SqlParameter sqlParaDesc = null;
                SqlParameter sqlParaIdentity = null;
                string sql = string.Empty;

                sqlParaKeys.Clear();
                sqlParaKeys.Add("@AccountID", SessionHelper.AccountID);
                sqlParaKeys.Add("@errNum", 0);
                sqlParaKeys.Add("@errDesc", string.Empty);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspGetReckonFileByAccountID]",
                                        ref sql, ref sqlParaNum, ref sqlParaDesc, ref sqlParaIdentity);

                var data = Database.SqlQuery<ReckonDesktopMaster>(sql, sqlParameters).ToList();
                return CosDBResponse<ICollection<ReckonDesktopMaster>>.CreateDBResponse(data, FormatSqlErrorDescParam(sqlParaDesc));
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                return CosDBResponse<ICollection<ReckonDesktopMaster>>.CreateDBResponse(null, ex.Message);
            }

        }

        #endregion ReconDesktopMaster

        #region FileSQLMasters

        internal CosDBResponse<DBResponseStatusCode> SaveFileSQLMasters(FileSQLMasters fileSQLMasters)
        {
            try
            {
                SqlParameter sqlParaNum = null;
                SqlParameter sqlParaDesc = null;
                SqlParameter sqlParaIdentity = null;
                string sql = string.Empty;

                sqlParaKeys.Clear();
                sqlParaKeys.Add("@AccountID", fileSQLMasters.AccountID);
                sqlParaKeys.Add("@SqlServerName", fileSQLMasters.SqlServerName);
                sqlParaKeys.Add("@SqlDataBaseName", fileSQLMasters.SqlDataBaseName);
                sqlParaKeys.Add("@SqlAuthenticationMode", fileSQLMasters.SqlAuthenticationMode);
                sqlParaKeys.Add("@SqlUserName", fileSQLMasters.SqlUserName);
                sqlParaKeys.Add("@SqlPassword", fileSQLMasters.SqlPassword);
                sqlParaKeys.Add("@AddedDte", fileSQLMasters.AddedDte);
                sqlParaKeys.Add("@UpdatedDte",fileSQLMasters.UpdatedDte);
                sqlParaKeys.Add("@errNum", 0);
                sqlParaKeys.Add("@errDesc", string.Empty);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspSaveFileSql]",
                                         ref sql, ref sqlParaNum, ref sqlParaDesc, ref sqlParaIdentity);

                Database.ExecuteSqlCommand(sql, sqlParameters);

                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(GetDBResponseStatusCode(FormatSqlErrorDescParam(sqlParaDesc)), FormatSqlErrorDescParam(sqlParaDesc));


            }
            catch (Exception ex)
            {
                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(DBResponseStatusCode.FAILED, ex.Message);
            }




        }

        internal ICollection<FileSQLMasters> GetFileSQLMastersDetail()
        {
            try
            {
                int errNum = 0;
                SqlParameter[] parameters = {
                    new SqlParameter("@errNum",errNum),
                    new SqlParameter("@errDesc","")
                };

                ICollection<FileSQLMasters> result = Database.SqlQuery<FileSQLMasters>("EXEC [uspGetFileSQLMasters] @errNum,@errDesc ", parameters).ToList();
                return result;
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                return null;
            }

        }

        internal CosDBResponse<FileSQLMasters> GetFileSQLMastersDetailByID(int uid)
        {
            try
            {
                int errNum = 0;
                SqlParameter[] parameters = {
                    new SqlParameter("@errNum",errNum),
                    new SqlParameter("@errDesc",""),
                    new SqlParameter("@Uid",uid)
                };

                var data = Database.SqlQuery<FileSQLMasters>("EXEC [uspGetAccountByID] @Uid, @errNum,@errDesc ", parameters).FirstOrDefault();
                return CosDBResponse<FileSQLMasters>.CreateDBResponse(data, FormatSqlErrorDescParam(parameters[1]));
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                return CosDBResponse<FileSQLMasters>.CreateDBResponse(null, ex.Message);
            }

        }

        #endregion FileSQLMasters

        #region QBO
        internal CosDBResponse<DBResponseStatusCode> SaveQboMaster(QboMaster qboMaster)
        {

            try
            {

                SqlParameter sqlParaNum = null;
                SqlParameter sqlParaDesc = null;
                SqlParameter sqlParaIdentity = null;
                string sql = string.Empty;

                sqlParaKeys.Clear();
                sqlParaKeys.Add("@AccountID", SessionHelper.AccountID);
                sqlParaKeys.Add("@AccessToken", qboMaster.AccessToken);
                sqlParaKeys.Add("@RealmId", qboMaster.RealmId ?? string.Empty);
                sqlParaKeys.Add("@RefreshToken", qboMaster.RefreshToken);
                
                sqlParaKeys.Add("@errNum", 0);
                sqlParaKeys.Add("@errDesc", string.Empty);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspSaveQBOMaster]",
                                         ref sql, ref sqlParaNum, ref sqlParaDesc, ref sqlParaIdentity);


                Database.ExecuteSqlCommand(sql, sqlParameters);


                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(GetDBResponseStatusCode(CSConvert.ToString(sqlParaDesc.Value)), CSConvert.ToString(sqlParaDesc.Value));

            }
            catch (Exception ex)
            {
                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(DBResponseStatusCode.FAILED, ex.Message);

            }

           
        }
        #endregion


        #region ReckonEzzyAccount
        internal CosDBResponse<DBResponseStatusCode> SaveReckonEzzyAccount(ReckonEzzyAccount reckonEzzyAccount)
        {
            try
            {

                SqlParameter sqlParaNum = null;
                SqlParameter sqlParaDesc = null;
                SqlParameter sqlParaIdentity = null;
                string sql = string.Empty;

                sqlParaKeys.Clear();
                sqlParaKeys.Add("@RCEzzyID", reckonEzzyAccount.RCEzzyID);
                sqlParaKeys.Add("@AccountID", SessionHelper.AccountID);
                sqlParaKeys.Add("@ReckonFileID", reckonEzzyAccount.ReckonFileID ?? 0);
                sqlParaKeys.Add("@EzzyUserName", reckonEzzyAccount.EzzyUserName ?? string.Empty);
                sqlParaKeys.Add("@EzzyPassword", reckonEzzyAccount.EzzyPassword ?? string.Empty);
                sqlParaKeys.Add("@AddedDate", reckonEzzyAccount.AddedDate ?? null);
                sqlParaKeys.Add("@UpdatedDate", reckonEzzyAccount.UpdatedDate ?? null);
                sqlParaKeys.Add("@errNum", 0);
                sqlParaKeys.Add("@errDesc", string.Empty);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspSaveReckonEzzyAccount]",
                                         ref sql, ref sqlParaNum, ref sqlParaDesc, ref sqlParaIdentity);


                Database.ExecuteSqlCommand(sql, sqlParameters);


                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(GetDBResponseStatusCode(CSConvert.ToString(sqlParaDesc.Value)), CSConvert.ToString(sqlParaDesc.Value));

            }
            catch (Exception ex)
            {
                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(DBResponseStatusCode.FAILED, ex.Message);

            }
        }
        #endregion ReckonEzzyAccount

        #region QboEzzyAccount

        internal CosDBResponse<DBResponseStatusCode> SaveQboEzzyAccount(QBOEzzyAccount qBOEzzyAccount)
        {
            try
            {

                SqlParameter sqlParaNum = null;
                SqlParameter sqlParaDesc = null;
                SqlParameter sqlParaIdentity = null;
                string sql = string.Empty;

                sqlParaKeys.Clear();
                sqlParaKeys.Add("@RCEzzyID", qBOEzzyAccount.QBOEzzyID);
                sqlParaKeys.Add("@AccountID", SessionHelper.AccountID);
                sqlParaKeys.Add("@QboConnectID", qBOEzzyAccount.QboConnectID ?? 0);
                sqlParaKeys.Add("@EzzyUserName", qBOEzzyAccount.EzzyUserName ?? string.Empty);
                sqlParaKeys.Add("@EzzyPassword", qBOEzzyAccount.EzzyPassword ?? string.Empty);
                sqlParaKeys.Add("@AddedDate", qBOEzzyAccount.AddedDate ?? null);
                sqlParaKeys.Add("@UpdatedDate", qBOEzzyAccount.UpdatedDate ?? null);
                sqlParaKeys.Add("@errNum", 0);
                sqlParaKeys.Add("@errDesc", string.Empty);

                

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspSaveQBOEzzyAccount]",
                                         ref sql, ref sqlParaNum, ref sqlParaDesc, ref sqlParaIdentity);


                Database.ExecuteSqlCommand(sql, sqlParameters);


                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(GetDBResponseStatusCode(CSConvert.ToString(sqlParaDesc.Value)), CSConvert.ToString(sqlParaDesc.Value));

            }
            catch (Exception ex)
            {
                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(DBResponseStatusCode.FAILED, ex.Message);

            }
        }
        #endregion QboEzzyAccount
        
        #region ErrorLog

       
        #endregion ErrorLog



    }
}