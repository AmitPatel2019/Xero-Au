using CosmicApiModel;
using CosmicCoreApi.AccessContext;
using CosmicCoreApi.CustomModel;
using CosmicCoreApi.Helper;
using Flexis.Log;
using Intuit.Ipp.Data;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Xero.NetStandard.OAuth2.Model.Accounting;

namespace CosmicCoreApi
{
    public class CosDBResponse<T>
    {
        SessionHelper _sessionHelper = new SessionHelper();

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

    public class CosmicContext : CosmicDbBase
    {

        const int SIZE_INT = 20;
        const int SIZE_BIT = 1;
        const int SIZE_DECIMAL = 20;
        const int SIZE_VARCHAR_10 = 10;
        const int SIZE_VARCHAR_20 = 20;
        const int SIZE_VARCHAR_30 = 30;
        const int SIZE_VARCHAR_40 = 40;
        const int SIZE_VARCHAR_50 = 50;
        const int SIZE_VARCHAR_80 = 80;
        const int SIZE_VARCHAR_100 = 100;
        const int SIZE_VARCHAR_150 = 150;
        const int SIZE_VARCHAR_200 = 200;
        const int SIZE_VARCHAR_250 = 250;
        const int SIZE_VARCHAR_MAX = 4000;
        const int SIZE_DATETIME = 23;

        private Logger _log = CosmicLogger.SetLog();
        Dictionary<string, SqlParameter> sqlParaKeys = new Dictionary<string, SqlParameter>();
        SessionHelper _sessionHelper = new SessionHelper();

        private static CosmicContext instance;
        public static CosmicContext Instance
		{
			get
			{
                if(instance==null)
				{
                    instance = new CosmicContext();
                }
                return instance;
            }
		}

        private CosmicContext()
        {

        }

        private ErrorLog PrepareErrorLog(string controller, string method, string exceptionMessage, Exception ex)
        {
            ErrorLog errorLog = new ErrorLog();
            errorLog.Controller = controller;
            errorLog.Method = method;
            errorLog.AccountID = _sessionHelper.AccountID;
            errorLog.LoginID = _sessionHelper.LoginID;
            errorLog.PlatformID = _sessionHelper.PlatformID;
            errorLog.ReckonFileID = _sessionHelper.ReckonFileID;
            errorLog.QboConnectID = _sessionHelper.QboConnectID;


            return errorLog;

        }

        object objLogErrorToDB = new object();
        public void LogErrorToDB(string controller, string method, string exceptionMessage, Exception ex)
        {
            lock(objLogErrorToDB)
            {
                try
                {
                    var errorLog = PrepareErrorLog(controller, method, exceptionMessage, ex);

                    SqlParameter sqlParaNum = GetOutputErrNum();
                    SqlParameter sqlParaDesc = GetOutputErrDesc();

                    string sql = string.Empty;

                    sqlParaKeys.Clear();
                    sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = errorLog.AccountID ?? 0 });
                    sqlParaKeys.Add("@PlatformID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = errorLog.PlatformID ?? 0 });
                    sqlParaKeys.Add("@LoginID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = errorLog.LoginID ?? 0 });
                    sqlParaKeys.Add("@ReckonFileID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = errorLog.ReckonFileID ?? 0 });
                    sqlParaKeys.Add("@QboConnectID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = errorLog.QboConnectID ?? 0 });
                    sqlParaKeys.Add("@Controller", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_50, SqlValue = errorLog.Controller ?? string.Empty });
                    sqlParaKeys.Add("@Method", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_50, SqlValue = errorLog.Method ?? string.Empty });
                    sqlParaKeys.Add("@ErrorMessage", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_MAX, SqlValue = errorLog.ErrorMessage ?? string.Empty });
                    sqlParaKeys.Add("@ErrorStackTrace", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_MAX, SqlValue = errorLog.ErrorStackTrace ?? string.Empty });
                    sqlParaKeys.Add("@ErrorLogDate", new SqlParameter() { SqlDbType = SqlDbType.DateTime, Size = SIZE_DATETIME, SqlValue = errorLog.ErrorLogDate ?? null });
                    sqlParaKeys.Add("@errNum", sqlParaNum);
                    sqlParaKeys.Add("@errDesc", sqlParaDesc);

                    SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspSaveErrorLog]",
                                             ref sql);


                     ExecuteSqlCommand(sql, sqlParameters);
                }
                catch (Exception Ex)
                {
                    Logger log = CosmicLogger.SetLog();
                    log.Error(ex);
                    _log.Error(Ex);

                }
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

        private SqlParameter GetOutputIdentity()
        {
            SqlParameter sqlPara = new SqlParameter();
            sqlPara.Direction = ParameterDirection.Output;
            sqlPara.Value = 0;
            sqlPara.ParameterName = "@identity";

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
        object obj1 = new object();
        private SqlParameter[] PrepareSql(Dictionary<string, SqlParameter> sqlParas,
                            string procedureName, ref string sql)
        {
            lock(obj1)
            {
                //sqlParameters
                int index = 0;
                SqlParameter[] sqlParameters = new SqlParameter[sqlParas.Count];

                try
                {
                    foreach (KeyValuePair<string, SqlParameter> para in sqlParas)
                    {
                        SqlParameter sqlParameter = para.Value;

                        if (string.Compare("@errNum", para.Key, true) == 0)
                        {
                            sqlParameter.ParameterName = para.Key;
                            sqlParameter.Direction = ParameterDirection.Output;
                            sqlParameter.Size = 100;

                            sql = string.Format("{0} {1} output,", sql, para.Key);

                        }
                        else if (string.Compare("@errDesc", para.Key, true) == 0)
                        {
                            sqlParameter.ParameterName = para.Key;
                            sqlParameter.Direction = ParameterDirection.Output;
                            sqlParameter.Size = 4000;

                            sql = string.Format("{0} {1} output,", sql, para.Key);
                        }
                        else if (string.Compare("@Identity", para.Key, true) == 0)
                        {
                            sqlParameter.ParameterName = para.Key;
                            sqlParameter.Direction = ParameterDirection.Output;
                            sqlParameter.Size = 100;

                            sql = string.Format("{0} {1} output,", sql, para.Key);
                        }
                        else
                        {
                            sqlParameter.ParameterName = para.Key;

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
                    Logger log = CosmicLogger.SetLog();
                    log.Error(ex);
                    _log.Error(ex);
                    return sqlParameters;
                }
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

        internal CosDBResponse<MiscTotal> GetTotalPaidPdfUsed()
        {
            try
            {
                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                string sql = string.Empty;

                sqlParaKeys.Clear();
                sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.AccountID });
                sqlParaKeys.Add("@PlatformID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.PlatformID });
                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspGetUsedPaidPDF]",
                                         ref sql);

                var data =  SqlQuery<MiscTotal>(sql, sqlParameters).FirstOrDefault();
                return CosDBResponse<MiscTotal>.CreateDBResponse(data, FormatSqlErrorDescParam(sqlParaDesc));

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                _log.Error(ex);
                return CosDBResponse<MiscTotal>.CreateDBResponse(null, ex.Message);
            }
        }

        internal CosDBResponse<MiscTotal> GetTotalTrialPdfUsed()
        {
            try
            {
                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                string sql = string.Empty;

                sqlParaKeys.Clear();
                sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.AccountID });
                sqlParaKeys.Add("@PlatformID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.PlatformID });
                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspGetUsedTrialPDF]",
                                         ref sql);

                var data =  SqlQuery<MiscTotal>(sql, sqlParameters).FirstOrDefault();
                return CosDBResponse<MiscTotal>.CreateDBResponse(data, FormatSqlErrorDescParam(sqlParaDesc));

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                _log.Error(ex);
                return CosDBResponse<MiscTotal>.CreateDBResponse(null, ex.Message);
            }
        }

        internal CosDBResponse<ICollection<ReckonDocument>> GetReckonDocument(int month, int year)
        {
            try
            {
                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();
                //SqlParameter sqlParaIdentity = null;
                string sql = string.Empty;

                sqlParaKeys.Clear();
                sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.AccountID });
                sqlParaKeys.Add("@ReckonFileID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.ReckonFileID });
                sqlParaKeys.Add("@Month", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.AccountID });
                sqlParaKeys.Add("@Year", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.ReckonFileID });
                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspGetReckonDocument]",
                                        ref sql);

                var data =  SqlQuery<ReckonDocument>(sql, sqlParameters).ToList();
                return CosDBResponse<ICollection<ReckonDocument>>.CreateDBResponse(data, FormatSqlErrorDescParam(sqlParaDesc));
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                _log.Error(ex);
                return CosDBResponse<ICollection<ReckonDocument>>.CreateDBResponse(null, ex.Message);
            }

        }

        internal CosDBResponse<AccountSubscribedPlan> GetAccountSubscriptionByAccountID(int accountID)
        {
            try
            {
                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();
                string sql = string.Empty;

                sqlParaKeys.Clear();
                sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = accountID });
                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspGetSubscriptionByAccountID]",
                                         ref sql);

                var data =  SqlQuery<AccountSubscribedPlan>(sql, sqlParameters).FirstOrDefault();
                return CosDBResponse<AccountSubscribedPlan>.CreateDBResponse(data, FormatSqlErrorDescParam(sqlParaDesc));

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log?.Error(ex);
                _log?.Error(ex);
                return CosDBResponse<AccountSubscribedPlan>.CreateDBResponse(null, ex.Message);
            }
        }
        #endregion

        #region AccountMaster 
        internal CosDBResponse<MyAccount> GetAccountMasterByAccountID(int accountID)
        {
            try
            {
                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();
                string sql = string.Empty;

                sqlParaKeys.Clear();
                sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = accountID });
                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspGetAccountByAccountID]",
                                         ref sql);

                var data =  SqlQuery<MyAccount>(sql, sqlParameters).FirstOrDefault();
                return CosDBResponse<MyAccount>.CreateDBResponse(data, FormatSqlErrorDescParam(sqlParaDesc));

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                _log.Error(ex);
                return CosDBResponse<MyAccount>.CreateDBResponse(null, ex.Message);
            }

        }
        internal CosDBResponse<DBResponseStatusCode> DeleteMultipleXeroDocument(ICollection<XeroDocumentLine> listQboDocument)
        {

            try
            {
                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();
                foreach (var qboDocument in listQboDocument)
                {
                    try
                    {

                        sqlParaNum = GetOutputErrNum();
                        sqlParaDesc = GetOutputErrDesc();

                        string sql = string.Empty;

                        sqlParaKeys.Clear();

                        sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.AccountID });
                        sqlParaKeys.Add("@QboConnectID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.XeroConnectID });
                        sqlParaKeys.Add("@DocumentID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = qboDocument.DocumentID });
                        sqlParaKeys.Add("@Deleted", new SqlParameter() { SqlDbType = SqlDbType.Bit, Size = SIZE_BIT, SqlValue = true });


                        sqlParaKeys.Add("@errNum", sqlParaNum);
                        sqlParaKeys.Add("@errDesc", sqlParaDesc);

                        SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspDeleteXeroDocument]",
                                              ref sql);

                         ExecuteSqlCommand(sql, sqlParameters);
                    }
                    catch (Exception ex)
                    {
                        Logger log = CosmicLogger.SetLog();
                        log.Error(ex);
                        return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(DBResponseStatusCode.FAILED, ex.Message);
                    }
                }


                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(GetDBResponseStatusCode(CSConvert.ToString(sqlParaDesc.Value)), CSConvert.ToString(sqlParaDesc.Value));

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(DBResponseStatusCode.FAILED, ex.Message);
            }
        }

        private CosDBResponse<DBResponseStatusCode> SendRegistrationEmail(AccountMaster accountMaster)
        {
            Logger log = CosmicLogger.SetLog();
            try
            {
                log.Info("accountMaster");
              
                //1 .Generate Random Password and Activation Code for Customer/User
                // accountMaster.Password = RandomHelper.GetRandomString(RandomString.UserPassword);
                accountMaster.ActivationCode = "";// RandomHelper.GetRandomString(RandomString.AccountActivationCode);

                //2. Save the Account in database
                var dbResp = SaveAccount(accountMaster);

                if (dbResp == null) return dbResp;
                if (dbResp.StatusCode == DBResponseStatusCode.FAILED) return dbResp;

                //Sending email to customer with random password and activation link
                //3. Get Email Body 
                string emailBody = SMTPHelper.GetEmailBody(MailTemplate.AccountRegistration);

                //4. Prepare Activation link
                EmailBodyReplacement ebReplacement = new EmailBodyReplacement();
                //SMTPHelper.GetAccountActivationLink(ebReplacement, accountMaster.Email, accountMaster.ActivationCode);
                ebReplacement.UserName = accountMaster.UserName;
                ebReplacement.WebLoginLink = CSConvert.ToString(ConfigurationManager.AppSettings["BASE_CLIENT_URL"]);
                ebReplacement.Email = accountMaster.Email;
                ebReplacement.ProductName = CSConvert.ToString(ConfigurationManager.AppSettings["PRODUCT_NAME"]);
                log.Info("ebReplacement.Password");
                log.Info(ebReplacement.Password);
                //5.  format email body
                emailBody = SMTPHelper.FormatEmailBody(MailTemplate.AccountRegistration, emailBody, ebReplacement, accountMaster.Email);
                log.Info("emailBody");
                log.Info(emailBody);
                //Initilize SMTP setting (server, port sender, to address etc)
                var smtpSetting = SMTPHelper.InitCosmicSMPTSetting(
                                         CSConvert.ToString(ConfigurationManager.AppSettings["Cosmic_Email_Subject_Registeration"]),
                                         accountMaster.Email, emailBody);

                return SMTPHelper.SendEmail(smtpSetting);
              //  return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(DBResponseStatusCode.OK, string.Empty);
            }
            catch (Exception ex)
            {
                
                log.Error(ex);
                log.Info("emailBody"+ex.Message);
                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(DBResponseStatusCode.FAILED, ex.Message);
            }
        }

        internal CosDBResponse<DBResponseStatusCode> CreateAccount(AccountMaster accountMaster)
        {
            try
            {
               // accountMaster.Password = EncryptionHelper.Encrpt(accountMaster.Password);
                //0 Check if email address is already exist in the system
                var resp = GetLoginMasterByUserName(accountMaster.UserName);

                if (resp.Data != null)
                {
                    var loginMaster = resp.Data as LoginMaster;
                    if (loginMaster != null)
                    {
                        if ((loginMaster.Active ?? false))
                        {
                            return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(DBResponseStatusCode.EMAIL_EXISTS, "An account already exists with the User Name you trying to register with");
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
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(DBResponseStatusCode.FAILED, ex.Message);
            }

        }

        internal CosDBResponse<DBResponseStatusCode> SaveAccount(AccountMaster accountMaster)
        {
            try
            {
                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();
                SqlParameter sqlParaIdentity = GetOutputIdentity();
                // SqlParameter sqlParaIdentity = null;
                string sql = string.Empty;

                sqlParaKeys.Clear();
                sqlParaKeys.Add("@PlatformID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.PlatformID });
                sqlParaKeys.Add("@UserName", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_30, SqlValue = accountMaster.UserName });
                sqlParaKeys.Add("@CountryOfOrigin", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = 3, SqlValue = accountMaster.CountryOfOrigin ?? string.Empty});
                sqlParaKeys.Add("@Phone", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_20, SqlValue = accountMaster.Phone ?? string.Empty });
                sqlParaKeys.Add("@Add1", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_100, SqlValue = accountMaster.Add1 ?? string.Empty });
                sqlParaKeys.Add("@Add2", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_100, SqlValue = accountMaster.Add2 ?? string.Empty });
                sqlParaKeys.Add("@Add3", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_50, SqlValue = accountMaster.Add3 ?? string.Empty });
                sqlParaKeys.Add("@City", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_30, SqlValue = accountMaster.City ?? string.Empty });
                sqlParaKeys.Add("@State", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_30, SqlValue = accountMaster.State ?? string.Empty });
                sqlParaKeys.Add("@Zip", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_10, SqlValue = accountMaster.Zip ?? string.Empty });
                sqlParaKeys.Add("@Country", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_50, SqlValue = accountMaster.Country ?? string.Empty });
                sqlParaKeys.Add("@Email", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_50, SqlValue = accountMaster.Email });
                sqlParaKeys.Add("@Password", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_MAX, SqlValue = accountMaster.Password });
                sqlParaKeys.Add("@ActivationCode", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_10, SqlValue = "" });
                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspSaveAccount]",
                                         ref sql);

                 ExecuteSqlCommand(sql, sqlParameters);

                //var xeroConnectID = 0;
                //var dbResp = SaveXeroMaster(xeroMaster, ref xeroConnectID);

                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(GetDBResponseStatusCode(FormatSqlErrorDescParam(sqlParaDesc)), FormatSqlErrorDescParam(sqlParaDesc));
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(DBResponseStatusCode.FAILED, ex.Message);
            }

        }

        internal CosDBResponse<AccountMaster> GetAccountByUserNameAndCode(string userName, string code)
        {
            try
            {

                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();
                //SqlParameter sqlParaIdentity = null;
                string sql = string.Empty;

                sqlParaKeys.Clear();
                sqlParaKeys.Add("@UserName", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_30, SqlValue = userName });
                sqlParaKeys.Add("@PlatformID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.PlatformID });
                sqlParaKeys.Add("@Code", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_10, SqlValue = code });
                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspValidateActivationCode]",
                                         ref sql);

                var data =  SqlQuery<AccountMaster>(sql, sqlParameters).FirstOrDefault();

                return CosDBResponse<AccountMaster>.CreateDBResponse(data, FormatSqlErrorDescParam(sqlParaDesc));



            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                _log.Error(ex);
                return CosDBResponse<AccountMaster>.CreateDBResponse(null, ex.Message);
            }
        }

        internal CosDBResponse<DBResponseStatusCode> UpdatePhoneByAccountID(string phone,string email)
        {

            try
            {
                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();
                string sql = string.Empty;

                sqlParaKeys.Clear();
                sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.AccountID});
                sqlParaKeys.Add("@Phone", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_20, SqlValue = phone ?? string.Empty });
                sqlParaKeys.Add("@Email", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_150, SqlValue = email ?? string.Empty });
                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);


                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspUpdatePhoneByAccountID]",
                                         ref sql);

                 ExecuteSqlCommand(sql, sqlParameters);

                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(GetDBResponseStatusCode(FormatSqlErrorDescParam(sqlParaDesc)), FormatSqlErrorDescParam(sqlParaDesc));


            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(DBResponseStatusCode.FAILED, ex.Message);
            }
        }

        internal CosDBResponse<DBResponseStatusCode> UpdateUserProfile(AccountMaster accountMaster)
        {

            try
            {
                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();
                //SqlParameter sqlParaIdentity = null;
                string sql = string.Empty;

                sqlParaKeys.Clear();
                sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = accountMaster.AccountID });
                sqlParaKeys.Add("@CustomerName", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_30, SqlValue = accountMaster.UserName });
                sqlParaKeys.Add("@Phone", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_20, SqlValue = accountMaster.Phone ?? string.Empty });
                sqlParaKeys.Add("@Add1", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_100, SqlValue = accountMaster.Add1 ?? string.Empty });
                sqlParaKeys.Add("@Add2", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_100, SqlValue = accountMaster.Add2 ?? string.Empty });
                sqlParaKeys.Add("@Add3", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_50, SqlValue = accountMaster.Add3 ?? string.Empty });
                sqlParaKeys.Add("@City", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_30, SqlValue = accountMaster.City ?? string.Empty });
                sqlParaKeys.Add("@State", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_30, SqlValue = accountMaster.State ?? string.Empty });
                sqlParaKeys.Add("@Zip", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_10, SqlValue = accountMaster.Zip ?? string.Empty });
                sqlParaKeys.Add("@Country", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_50, SqlValue = accountMaster.Country ?? string.Empty });
                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);


                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspUpdateProfile]",
                                         ref sql);

                 ExecuteSqlCommand(sql, sqlParameters);

                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(GetDBResponseStatusCode(FormatSqlErrorDescParam(sqlParaDesc)), FormatSqlErrorDescParam(sqlParaDesc));


            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(DBResponseStatusCode.FAILED, ex.Message);
            }
        }

        #endregion AccountMaster

        #region LoginMaster

        internal CosDBResponse<DBResponseStatusCode> SaveLoginMaster(LoginMaster loginMaster)
        {

            try
            {
                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                string sql = string.Empty;

                sqlParaKeys.Clear();

                sqlParaKeys.Add("@ActivationCode", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_10, SqlValue = loginMaster.ActivationCode });
                sqlParaKeys.Add("@LoginID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = loginMaster.LoginID });
                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);


                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspUpdateLogin]",
                                        ref sql);

                 ExecuteSqlCommand(sql, sqlParameters);


                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(GetDBResponseStatusCode(FormatSqlErrorDescParam(sqlParaDesc)), FormatSqlErrorDescParam(sqlParaDesc));
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(DBResponseStatusCode.FAILED, ex.Message);
            }
        }
        object obj = new object();
        internal string UpdateUserToken(LoginMaster login)
        {
            lock(obj)
            {
                login.Token = Guid.NewGuid().ToString();

                if ((login.TokenAvailFor ?? 0) > 0)
                    login.TokenExpiredDate = DateTime.Now.AddDays((login.TokenAvailFor ?? 0));

                if ((login.TokenAvailFor ?? 0) == -1)
                    login.TokenExpiredDate = DateTime.Now.AddMonths(1);

                if ((login.TokenAvailFor ?? 0) <= 0)
                    login.TokenExpiredDate = DateTime.Now.AddDays(1);

                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                string sql = string.Empty;

                try
                {
                    //sqlParaKeys.Clear();
                    var sqlParaKeys = new Dictionary<string, SqlParameter>();

                    sqlParaKeys.Add("@LoginID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = login.LoginID });
                    sqlParaKeys.Add("@Token", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = 500, SqlValue = login.Token });
                    sqlParaKeys.Add("@TokenExpiredDate", new SqlParameter() { SqlDbType = SqlDbType.DateTime, Size = SIZE_DATETIME, SqlValue = login.TokenExpiredDate });

                    SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspUpdateToken]",
                                             ref sql);

                     ExecuteSqlCommand(sql, sqlParameters);

                    #region XeroMaster Insert

                    SqlParameter sqlParaIdentity = GetOutputIdentity();

                    var resp = GetLoginMasterByUserName(login.UserName);

                    var sqlParaKeysXero = new Dictionary<string, SqlParameter>();
                    string sqlXero = string.Empty;

                    if (resp.Data != null)
                    {
                        sqlParaKeysXero.Clear();
                        sqlParaKeysXero.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = login.AccountID });
                        sqlParaKeysXero.Add("@OAuthToken", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_MAX, SqlValue = login.Token ?? string.Empty });
                        sqlParaKeysXero.Add("@OAuthTokenSec", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_MAX, SqlValue = login.TokenExpiredDate });
                        sqlParaKeysXero.Add("@IdentityToken", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_MAX, SqlValue = login.Token ?? string.Empty });
                        sqlParaKeysXero.Add("@AccessToken", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_MAX, SqlValue = login.Token ?? string.Empty });
                        sqlParaKeysXero.Add("@AccessTokenExpiresIn", new SqlParameter() { SqlDbType = SqlDbType.Date, Size = SIZE_VARCHAR_100, SqlValue = login.TokenExpiredDate });
                        sqlParaKeysXero.Add("@RealmId", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_MAX, SqlValue = Guid.NewGuid().ToString() ?? string.Empty });
                        sqlParaKeysXero.Add("@RefreshToken", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_MAX, SqlValue = login.Token ?? string.Empty });
                        sqlParaKeysXero.Add("@RefreshTokenExpiresIn", new SqlParameter() { SqlDbType = SqlDbType.DateTime, Size = SIZE_VARCHAR_MAX, SqlValue = login.TokenExpiredDate });
                        sqlParaKeysXero.Add("@SessionHandle", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_MAX, SqlValue = string.Empty });

                        sqlParaKeysXero.Add("@errNum", sqlParaNum);
                        sqlParaKeysXero.Add("@errDesc", sqlParaDesc);
                        sqlParaKeysXero.Add("@identity", sqlParaIdentity);

                        SqlParameter[] sqlParameters1 = PrepareSql(sqlParaKeysXero, "[uspSaveXeroMaster]", ref sqlXero);

                        ExecuteSqlCommand(sqlXero, sqlParameters1);
                    }

                    #endregion XeroMaster Insert
                }
                catch (Exception ex)
                {
                    Logger log = CosmicLogger.SetLog();
                    log?.Error(ex);
                    _log?.Error(ex);
                }

                if (GetDBResponseStatusCode(FormatSqlErrorDescParam(sqlParaDesc)) == DBResponseStatusCode.OK)
                {
                    return login.Token;
                }


                return "Error while generating user token";
            }
           

        }

        internal CosDBResponse<LoginMaster> VerifyUserNameAndPassword(LoginMaster loginMaster)
        {
            try
            {
                //loginMaster.Password =EncryptionHelper.Encrpt(loginMaster.Password);
                var resp = GetLoginByUserName(loginMaster.UserName);
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
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<LoginMaster>.CreateDBResponse(null, ex.Message);
            }
        }

        internal CosDBResponse<LoginMaster> ProcessForgotPwd(LoginMaster loginMaster)
        {
            try
            {
                var resp = GetLoginByUserName(loginMaster.UserName);
                if (resp.Data != null)
                {
                    var login = resp.Data as LoginMaster;
                    if (login.UserName == loginMaster.UserName)
                    {
                        // Genrate activation code 1
                        loginMaster.ActivationCode = RandomHelper.GetRandomString(RandomString.AccountActivationCode);
                        loginMaster.LoginID = login.LoginID;
                        loginMaster.UserName = login.UserName;
                       
                        // Save loginmaster in actvation code  2
                        //var dbResponce = SaveLoginMaster(loginMaster);
                        //if (dbResponce != null) return dbResponce;
                        if (resp.StatusCode == DBResponseStatusCode.OK) return resp;

                      

                        //string emailBody = SMTPHelper.GetEmailBody(MailTemplate.ForgotPassword);
                        //EmailBodyReplacement ebReplacement = new EmailBodyReplacement();


                        //SMTPHelper.GetAccountActivationLink(ebReplacement, loginMaster.EmailAddress, loginMaster.ActivationCode);

                        //ebReplacement.ActivationCode = loginMaster.ActivationCode;
                        //ebReplacement.UserName = login.UserName;

                        ////5.  format email body
                        //emailBody = SMTPHelper.FormatEmailBody(MailTemplate.ForgotPassword, emailBody, ebReplacement, loginMaster.EmailAddress);

                        ////Initilize SMTP setting (server, port sender, to address etc)
                        //var smtpSetting = SMTPHelper.InitCosmicSMPTSetting(
                        //                         ConfigurationManager.AppSettings["Cosmic_Email_Subject_Forgetpassword"],
                        //                         loginMaster.EmailAddress, emailBody);

                        //return SMTPHelper.SendEmail(smtpSetting);

                    }

                }
                return CosDBResponse<LoginMaster>.CreateDBResponse(null, "Could not find account by given email address");


            }
            catch (Exception ex)
            {
                Logger _log = CosmicLogger.SetLog();
                _log.Error(ex);
               return CosDBResponse<LoginMaster>.CreateDBResponse(null, ex.Message);
            }
        }

        internal CosDBResponse<LoginMaster> GetLoginMasterByUserName(string userName)
        {
            try
            {
                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                string sql = string.Empty;

                sqlParaKeys.Clear();
                sqlParaKeys.Add("@UserName", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_30, SqlValue = userName });
                sqlParaKeys.Add("@PlatformID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.PlatformID });
                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspGetLoginMasterByUserName]",
                                        ref sql);


                var data =  SqlQuery<LoginMaster>(sql, sqlParameters).FirstOrDefault();
                return CosDBResponse<LoginMaster>.CreateDBResponse(data, FormatSqlErrorDescParam(sqlParaDesc));
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<LoginMaster>.CreateDBResponse(null, ex.Message);
            }
        }

        internal CosDBResponse<LoginMaster> GetLoginByUserName(string userName)
        {
            try
            {
                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                string sql = string.Empty;

                sqlParaKeys.Clear();
                sqlParaKeys.Add("@UserName", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_100, SqlValue = userName });
                sqlParaKeys.Add("@PlatformID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.PlatformID});
                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);


                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspGetLoginByUserName]",
                                        ref sql);


                var data =  SqlQuery<LoginMaster>(sql, sqlParameters).FirstOrDefault();
                return CosDBResponse<LoginMaster>.CreateDBResponse(data, FormatSqlErrorDescParam(sqlParaDesc));
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<LoginMaster>.CreateDBResponse(null, ex.Message);
            }
        }
        object _obj = new object();
        internal CosDBResponse<LoginMaster> GetLoginMasterByToken(string token)
        {
            lock(_obj)
            {
                try
                {
                    SqlParameter sqlParaNum = GetOutputErrNum();
                    SqlParameter sqlParaDesc = GetOutputErrDesc();
                    //SqlParameter sqlParaIdentity = null;
                    string sql = string.Empty;
                    Dictionary<string, SqlParameter> m_sqlParaKeys = new Dictionary<string, SqlParameter>();
                    m_sqlParaKeys.Add("@Token", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = 500, SqlValue = token });
                    m_sqlParaKeys.Add("@errNum", sqlParaNum);
                    m_sqlParaKeys.Add("@errDesc", sqlParaDesc);


                    SqlParameter[] sqlParameters = PrepareSql(m_sqlParaKeys, "[uspGetLoginMasterByToken]",
                                            ref sql);

                    //  ExecuteSqlCommand(sql, sqlParameters);

                    var data =  SqlQuery<LoginMaster>(sql, sqlParameters).FirstOrDefault();
                    return CosDBResponse<LoginMaster>.CreateDBResponse(data, FormatSqlErrorDescParam(sqlParaDesc));
                }
                catch (Exception ex)
                {
                    Logger log = CosmicLogger.SetLog();
                    log.Error(ex);
                    return CosDBResponse<LoginMaster>.CreateDBResponse(null, ex.Message);
                }
            }
           
        }

        internal CosDBResponse<DBResponseStatusCode> ActivateAccount(LoginMaster loginMaster)
        {
            try
            {
                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                string sql = string.Empty;

                sqlParaKeys.Clear();
                sqlParaKeys.Add("@UserName", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_30, SqlValue = loginMaster.UserName });
                sqlParaKeys.Add("@Password", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_MAX, SqlValue = loginMaster.Password });
                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspActivateAccount]",
                                        ref sql);

                 ExecuteSqlCommand(sql, sqlParameters);

                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(GetDBResponseStatusCode(FormatSqlErrorDescParam(sqlParaDesc)), FormatSqlErrorDescParam(sqlParaDesc));
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(DBResponseStatusCode.FAILED, ex.Message);
            }

        }

        internal CosDBResponse<DBResponseStatusCode> AddEditUser(LoginMaster loginMaster)
        {
            try
            {
                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                string sql = string.Empty;

                sqlParaKeys.Clear();
                sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.AccountID });
                sqlParaKeys.Add("@PlatformID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.PlatformID });
                sqlParaKeys.Add("@UserName", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_30, SqlValue = loginMaster.UserName });
                sqlParaKeys.Add("@EmailAddress", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_50, SqlValue = loginMaster.EmailAddress });
                sqlParaKeys.Add("@Password", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_MAX, SqlValue = loginMaster.Password });
                sqlParaKeys.Add("@LoginID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = loginMaster.LoginID });
                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspSaveLogin]",
                                        ref sql);

                 ExecuteSqlCommand(sql, sqlParameters);

                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(GetDBResponseStatusCode(FormatSqlErrorDescParam(sqlParaDesc)), FormatSqlErrorDescParam(sqlParaDesc));
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(DBResponseStatusCode.FAILED, ex.Message);
            }

        }

        internal CosDBResponse<DBResponseStatusCode> ValidateActivationCode(AccountMaster accountMaster)
        {

            try
            {
                var resp = GetAccountByUserNameAndCode(accountMaster.UserName, accountMaster.ActivationCode);
                if (resp.Data != null)
                {
                    var loginMaster = resp.Data as AccountMaster;
                    if (loginMaster != null)
                    {
                        return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(DBResponseStatusCode.OK, "");
                    }
                }
                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(DBResponseStatusCode.FAILED, string.Empty);

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(DBResponseStatusCode.FAILED, ex.Message);
            }

        }

        internal CosDBResponse<DBResponseStatusCode> ValidateActivationCodeInloginMaster(LoginMaster loginMaster)
        {

            try
            {
                var resp = GetLoginByUserName(loginMaster.UserName);
                if (resp.Data != null)
                {
                    var login = resp.Data as LoginMaster;
                    if (login.ActivationCode == loginMaster.ActivationCode)
                    {
                        return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(DBResponseStatusCode.OK, "");
                    }
                }
                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(DBResponseStatusCode.FAILED, string.Empty);

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(DBResponseStatusCode.FAILED, ex.Message);
            }

        }

        internal CosDBResponse<DBResponseStatusCode> ChangePassword(LoginMaster loginMaster)
        {
            try
            {

                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                string sql = string.Empty;

                sqlParaKeys.Clear();
                sqlParaKeys.Add("@UserName", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_30, SqlValue = loginMaster.UserName });
                sqlParaKeys.Add("@ExistsPassword", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_MAX, SqlValue = loginMaster.ExistsPassword });
                sqlParaKeys.Add("@NewPassword", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_MAX, SqlValue = loginMaster.NewPassword });
                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspChangePassword]",
                                         ref sql);


                 ExecuteSqlCommand(sql, sqlParameters);

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
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(DBResponseStatusCode.FAILED, ex.Message);

            }
        }


        internal CosDBResponse<ICollection<LoginMaster>> GetSubUser()
        {
            try
            {
                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                string sql = string.Empty;

                sqlParaKeys.Clear();
                sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.AccountID });
                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);
                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspGetSubUser]",
                                        ref sql);

                var data =  SqlQuery<LoginMaster>(sql, sqlParameters).ToList();
                return CosDBResponse<ICollection<LoginMaster>>.CreateDBResponse(data, FormatSqlErrorDescParam(sqlParaDesc));
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<ICollection<LoginMaster>>.CreateDBResponse(null, ex.Message);
            }

        }

        internal CosDBResponse<ICollection<LoginMaster>> GetLoginMasterDetail()
        {
            try
            {
                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                string sql = string.Empty;

                sqlParaKeys.Clear();
                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);
                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspGetLoginMaster]",
                                        ref sql);

                var data =  SqlQuery<LoginMaster>(sql, sqlParameters).ToList();
                return CosDBResponse<ICollection<LoginMaster>>.CreateDBResponse(data, FormatSqlErrorDescParam(sqlParaDesc));
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<ICollection<LoginMaster>>.CreateDBResponse(null, ex.Message);
            }

        }

        #endregion LoginMaster

        #region Ezzy Master

        internal CosDBResponse<EzzyAccount> GetEzzyLogin()
        {
            try
            {
                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                string sql = string.Empty;

                sqlParaKeys.Clear();
                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);
                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspGetEzzyLogin]",
                                        ref sql);

                var data =  SqlQuery<EzzyAccount>(sql, sqlParameters).FirstOrDefault();
                return CosDBResponse<EzzyAccount>.CreateDBResponse(data, FormatSqlErrorDescParam(sqlParaDesc));
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<EzzyAccount>.CreateDBResponse(null, ex.Message);
            }
        }

        #endregion

        #region PlatformMaster

        internal CosDBResponse<DBResponseStatusCode> SavePlatformMaster(PlatformMaster platformMaster)
        {

            try
            {
                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                string sql = string.Empty;

                sqlParaKeys.Clear();
                sqlParaKeys.Add("@PlatformCode", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = 5, SqlValue = platformMaster.PlatformCode });
                sqlParaKeys.Add("@PlatformName", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_250, SqlValue = platformMaster.PlatformName });
                sqlParaKeys.Add("@Description", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_MAX, SqlValue = platformMaster.Description });
                sqlParaKeys.Add("@Active", new SqlParameter() { SqlDbType = SqlDbType.Bit, Size = SIZE_BIT, SqlValue = platformMaster.Active });
                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);
                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspSavePlatform]",
                                        ref sql);

                 ExecuteSqlCommand(sql, sqlParameters);

                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(GetDBResponseStatusCode(FormatSqlErrorDescParam(sqlParaDesc)), FormatSqlErrorDescParam(sqlParaDesc));

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(DBResponseStatusCode.FAILED, ex.Message);
            }

        }

        internal CosDBResponse<PlatformMaster> GetPlatformMasterDetailByID(int uid)
        {
            try
            {
                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                string sql = string.Empty;

                sqlParaKeys.Clear();


                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspGetPlatformMasterByID]",
                                        ref sql);

                var data =  SqlQuery<PlatformMaster>(sql, sqlParameters).FirstOrDefault();
                return CosDBResponse<PlatformMaster>.CreateDBResponse(data, FormatSqlErrorDescParam(sqlParaDesc));
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<PlatformMaster>.CreateDBResponse(null, ex.Message);
            }

        }

        internal CosDBResponse<ICollection<PlatformMaster>> GetPlatformMasterDetail()
        {
            try
            {
                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                string sql = string.Empty;

                sqlParaKeys.Clear();
                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspGetPlatformMaster]",
                                        ref sql);

                var data =  SqlQuery<PlatformMaster>(sql, sqlParameters).ToList();
                return CosDBResponse<ICollection<PlatformMaster>>.CreateDBResponse(data, FormatSqlErrorDescParam(sqlParaDesc));
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<ICollection<PlatformMaster>>.CreateDBResponse(null, ex.Message);
            }

        }

        #endregion PlatformMaster

        #region PlanMaster

        internal CosDBResponse<DBResponseStatusCode> UpdgradePlan(int planID)
        {
            try
            {
                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();
                string sql = string.Empty;

                sqlParaKeys.Clear();

                sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.AccountID });
                sqlParaKeys.Add("@PlanID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = planID });
                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspUpgradePlan]",
                                         ref sql);

                 ExecuteSqlCommand(sql, sqlParameters);

                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(GetDBResponseStatusCode(CSConvert.ToString(sqlParaDesc.Value)), CSConvert.ToString(sqlParaDesc.Value));

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(DBResponseStatusCode.FAILED, ex.Message);
            }
        }

        internal CosDBResponse<StripePayment> SavePayment(StripePayment stripePayment)
        {
            try
            {

                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();
                SqlParameter sqlParaIdentity = GetOutputIdentity();
                string sql = string.Empty;

                sqlParaKeys.Clear();
                sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = stripePayment.AccountID });
                sqlParaKeys.Add("@PlanID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = stripePayment.PlanID });
                sqlParaKeys.Add("@Amount", new SqlParameter() { SqlDbType = SqlDbType.Decimal, Size = SIZE_DECIMAL, SqlValue = stripePayment.Amount });
                sqlParaKeys.Add("@StripeBalanceTxnID", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = 500, SqlValue = stripePayment.StripeBalanceTxnID });
                sqlParaKeys.Add("@StripeID", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = 500, SqlValue = stripePayment.StripeID });
                sqlParaKeys.Add("@StripeIsPaid", new SqlParameter() { SqlDbType = SqlDbType.Bit, Size = SIZE_BIT, SqlValue = stripePayment.StripeIsPaid });
                sqlParaKeys.Add("@StripeStatus", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_30, SqlValue = stripePayment.StripeStatus });
                sqlParaKeys.Add("@StripeDateTime", new SqlParameter() { SqlDbType = SqlDbType.DateTime, Size = SIZE_DATETIME, SqlValue = stripePayment.StripeDateTime });
                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);
                sqlParaKeys.Add("@identity", sqlParaIdentity);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspSaveStripePayment]",
                                         ref sql);

                 ExecuteSqlCommand(sql, sqlParameters);

                return CosDBResponse<StripePayment>.CreateDBResponse(stripePayment, CSConvert.ToString(sqlParaDesc.Value));

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<StripePayment>.CreateDBResponse(stripePayment, ex.Message);

            }
        }

        internal CosDBResponse<ICollection<PlanMaster>> GetAccountPlan()
        {
            try
            {
                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                string sql = string.Empty;

                sqlParaKeys.Clear();
                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspGetPlan]",
                                        ref sql);

                var data =  SqlQuery<PlanMaster>(sql, sqlParameters).ToList();
                return CosDBResponse<ICollection<PlanMaster>>.CreateDBResponse(data, FormatSqlErrorDescParam(sqlParaDesc));

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<ICollection<PlanMaster>>.CreateDBResponse(null, ex.Message);
            }
        }

        internal CosDBResponse<PlanMaster> GetPlan(int planID)
        {
            try
            {
                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                string sql = string.Empty;

                sqlParaKeys.Clear();
                sqlParaKeys.Add("@PlanID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = planID });
                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspGetPlanByID]",
                                        ref sql);

                var data =  SqlQuery<PlanMaster>(sql, sqlParameters).FirstOrDefault();
                return CosDBResponse<PlanMaster>.CreateDBResponse(data, FormatSqlErrorDescParam(sqlParaDesc));

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<PlanMaster>.CreateDBResponse(null, ex.Message);
            }
        }

        #endregion PlanMaster

        #region ReconDesktopMaster

        internal CosDBResponse<DBResponseStatusCode> SaveReconDesktopMaster(ReckonDesktopMaster reckonDesktopMaster, ref int reckonFileID)
        {

            try
            {

                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();
                SqlParameter sqlParaIdentity = GetOutputIdentity();
                string sql = string.Empty;

                sqlParaKeys.Clear();
                sqlParaKeys.Add("@ReckonFileID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = reckonDesktopMaster.ReckonFileID });
                sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.AccountID });
                sqlParaKeys.Add("@CompanyName", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_100, SqlValue = reckonDesktopMaster.CompanyName ?? string.Empty });
                sqlParaKeys.Add("@LegalCompanyName", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_100, SqlValue = reckonDesktopMaster.LegalCompanyName ?? string.Empty });
                sqlParaKeys.Add("@IsSampleCompany", new SqlParameter() { SqlDbType = SqlDbType.Bit, Size = SIZE_BIT, SqlValue = (reckonDesktopMaster.IsSampleCompany ?? false) });
                sqlParaKeys.Add("@Phone", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_20, SqlValue = reckonDesktopMaster.Phone ?? string.Empty });
                sqlParaKeys.Add("@Email", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_100, SqlValue = reckonDesktopMaster.Email ?? string.Empty });
                sqlParaKeys.Add("@Addr1", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_100, SqlValue = reckonDesktopMaster.Addr1 ?? string.Empty });
                sqlParaKeys.Add("@Addr2", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_100, SqlValue = reckonDesktopMaster.Addr2 ?? string.Empty });
                sqlParaKeys.Add("@Addr3", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_100, SqlValue = reckonDesktopMaster.Addr3 ?? string.Empty });
                sqlParaKeys.Add("@State", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_30, SqlValue = reckonDesktopMaster.State ?? string.Empty });
                sqlParaKeys.Add("@City", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_50, SqlValue = reckonDesktopMaster.City ?? string.Empty });
                sqlParaKeys.Add("@PostalCode", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_20, SqlValue = reckonDesktopMaster.PostalCode ?? string.Empty });
                sqlParaKeys.Add("@Country", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_50, SqlValue = reckonDesktopMaster.Country ?? string.Empty });
                sqlParaKeys.Add("@FilePath", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = 500, SqlValue = reckonDesktopMaster.FilePath ?? string.Empty });
                sqlParaKeys.Add("@IsUsingJobCosting", new SqlParameter() { SqlDbType = SqlDbType.Bit, Size = SIZE_BIT, SqlValue = reckonDesktopMaster.IsUsingJobCosting ?? false });
                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);
                sqlParaKeys.Add("@identity", sqlParaIdentity);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspSaveReckonDesktop]",
                                         ref sql);



                 ExecuteSqlCommand(sql, sqlParameters);

                reckonFileID = CSConvert.ToInt(sqlParaIdentity.Value);


                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(GetDBResponseStatusCode(CSConvert.ToString(sqlParaDesc.Value)), CSConvert.ToString(sqlParaDesc.Value));

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(DBResponseStatusCode.FAILED, ex.Message);

            }


        }

        internal CosDBResponse<DBResponseStatusCode> SaveReckonVendDefault(ReckonVendDefault reckonVendDefault)
        {
            try
            {
                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                string sql = string.Empty;

                sqlParaKeys.Clear();
                sqlParaKeys.Add("@ID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = reckonVendDefault.ID });
                sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.AccountID });
                sqlParaKeys.Add("@ReckonFileID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.ReckonFileID });
                sqlParaKeys.Add("@VendorListID", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_50, SqlValue = reckonVendDefault.VendorListID });

                sqlParaKeys.Add("@DefaultExpense", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_250, SqlValue = reckonVendDefault.DefaultExpenseAccount ?? string.Empty });
                sqlParaKeys.Add("@DefaultItem", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_250, SqlValue = reckonVendDefault.DefaultItem ?? string.Empty });
                sqlParaKeys.Add("@DefaultExpenseListID", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_50, SqlValue = reckonVendDefault.DefaultExpenseListID ?? string.Empty });
                sqlParaKeys.Add("@DefaultItemListID", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_50, SqlValue = reckonVendDefault.DefaultItemListID ?? string.Empty });

                sqlParaKeys.Add("@AddedBy", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.LoginID });
                sqlParaKeys.Add("@AddedDate", new SqlParameter() { SqlDbType = SqlDbType.DateTime, Size = SIZE_DATETIME, SqlValue = DateTime.Now });
                sqlParaKeys.Add("@UpdatedBy", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.LoginID });
                sqlParaKeys.Add("@UpdatedDate", new SqlParameter() { SqlDbType = SqlDbType.DateTime, Size = SIZE_DATETIME, SqlValue = DateTime.Now });


                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspSaveReckonVendorDefault]",
                                         ref sql);

                 ExecuteSqlCommand(sql, sqlParameters);

                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(GetDBResponseStatusCode(FormatSqlErrorDescParam(sqlParaDesc)), FormatSqlErrorDescParam(sqlParaDesc));
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(DBResponseStatusCode.FAILED, ex.Message);
            }
        }

        internal CosDBResponse<DBResponseStatusCode> SaveAllReckonVendDefault(ICollection<ReckonVendDefault> lstReckonVendDefault)
        {
            try
            {
                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                foreach (var itm in lstReckonVendDefault)
                {
                    string sql = string.Empty;

                    sqlParaKeys.Clear();
                    sqlParaKeys.Add("@ID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = itm.ID });
                    sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.AccountID });
                    sqlParaKeys.Add("@ReckonFileID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.ReckonFileID });
                    sqlParaKeys.Add("@VendorListID", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_50, SqlValue = itm.VendorListID });

                    sqlParaKeys.Add("@DefaultExpense", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_250, SqlValue = itm.DefaultExpenseAccount ?? string.Empty });
                    sqlParaKeys.Add("@DefaultItem", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_250, SqlValue = itm.DefaultItem ?? string.Empty });
                    sqlParaKeys.Add("@DefaultExpenseListID", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_50, SqlValue = itm.DefaultExpenseListID ?? string.Empty });
                    sqlParaKeys.Add("@DefaultItemListID", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_50, SqlValue = itm.DefaultItemListID ?? string.Empty });

                    sqlParaKeys.Add("@AddedBy", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.LoginID });
                    sqlParaKeys.Add("@AddedDate", new SqlParameter() { SqlDbType = SqlDbType.DateTime, Size = SIZE_DATETIME, SqlValue = DateTime.Now });
                    sqlParaKeys.Add("@UpdatedBy", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.LoginID });
                    sqlParaKeys.Add("@UpdatedDate", new SqlParameter() { SqlDbType = SqlDbType.DateTime, Size = SIZE_DATETIME, SqlValue = DateTime.Now });

                    SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspSaveReckonVendorDefault]",
                                             ref sql);

                     ExecuteSqlCommand(sql, sqlParameters);
                }
                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(GetDBResponseStatusCode(FormatSqlErrorDescParam(sqlParaDesc)), FormatSqlErrorDescParam(sqlParaDesc));
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(DBResponseStatusCode.FAILED, ex.Message);
            }
        }

        internal CosDBResponse<ICollection<ReckonVendDefault>> GetReckonVendDefault()
        {
            try
            {
                try
                {
                    SqlParameter sqlParaNum = GetOutputErrNum();
                    SqlParameter sqlParaDesc = GetOutputErrDesc();

                    string sql = string.Empty;

                    sqlParaKeys.Clear();
                    sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.AccountID });
                    sqlParaKeys.Add("@ReckonFileID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.ReckonFileID });
                    sqlParaKeys.Add("@errNum", sqlParaNum);
                    sqlParaKeys.Add("@errDesc", sqlParaDesc);

                    SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspGetReckonVendDefault]",
                                             ref sql);

                    var data =  SqlQuery<ReckonVendDefault>(sql, sqlParameters).ToList();

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
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
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

                var data =  SqlQuery<ReckonDesktopMaster>("EXEC [uspGetReconDesktopMasterByID] @Uid, @errNum,@errDesc ", parameters).FirstOrDefault();
                return CosDBResponse<ReckonDesktopMaster>.CreateDBResponse(data, FormatSqlErrorDescParam(parameters[1]));
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<ReckonDesktopMaster>.CreateDBResponse(null, ex.Message);
            }

        }

        internal CosDBResponse<ICollection<ReckonDesktopMaster>> GetAllReckonFileByAccountID()
        {
            try
            {
                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                string sql = string.Empty;

                sqlParaKeys.Clear();
                sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.AccountID });
                sqlParaKeys.Add("@LoginID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.LoginID });

                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspGetReckonFileByAccountID]",
                                        ref sql);

                var data =  SqlQuery<ReckonDesktopMaster>(sql, sqlParameters).ToList();
                return CosDBResponse<ICollection<ReckonDesktopMaster>>.CreateDBResponse(data, FormatSqlErrorDescParam(sqlParaDesc));
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<ICollection<ReckonDesktopMaster>>.CreateDBResponse(null, ex.Message);
            }

        }

        internal CosDBResponse<ICollection<ReckonDesktopMaster>> GetReckonFileWithUserAccess(int loginID)
        {
            try
            {
                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                string sql = string.Empty;

                sqlParaKeys.Clear();
                sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.AccountID });
                sqlParaKeys.Add("@LoginID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = loginID });
                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspAdminpGetReckonMasterByAccountID]",
                                        ref sql);

                var data =  SqlQuery<ReckonDesktopMaster>(sql, sqlParameters).ToList();
                return CosDBResponse<ICollection<ReckonDesktopMaster>>.CreateDBResponse(data, FormatSqlErrorDescParam(sqlParaDesc));
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<ICollection<ReckonDesktopMaster>>.CreateDBResponse(null, ex.Message);
            }

        }

        internal CosDBResponse<DBResponseStatusCode> UpdateReckonFileUserAccess(LoginMaster loginMaster)
        {

            try
            {

                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                string sql = string.Empty;

                sqlParaKeys.Clear();
                sqlParaKeys.Add("@UserID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = loginMaster.LoginID });
                sqlParaKeys.Add("@ReckonFileID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = loginMaster.ReckonFileID });
                sqlParaKeys.Add("@IsAccess", new SqlParameter() { SqlDbType = SqlDbType.Bit, Size = SIZE_BIT, SqlValue = (loginMaster.IsAccess ?? false) });
                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspUpdateReckonFileUserAccess]",
                                         ref sql);


                 ExecuteSqlCommand(sql, sqlParameters);


                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(GetDBResponseStatusCode(CSConvert.ToString(sqlParaDesc.Value)), CSConvert.ToString(sqlParaDesc.Value));

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(DBResponseStatusCode.FAILED, ex.Message);

            }


        }


        #endregion ReconDesktopMaster

        #region FileSQLMasters

        internal CosDBResponse<DBResponseStatusCode> SaveFileSQLMasters(FileSQLMasters fileSQLMasters)
        {
            try
            {
                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                string sql = string.Empty;

                sqlParaKeys.Clear();
                sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = fileSQLMasters.AccountID });
                sqlParaKeys.Add("@SqlServerName", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_MAX, SqlValue = fileSQLMasters.SqlServerName });
                sqlParaKeys.Add("@SqlDataBaseName", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_MAX, SqlValue = fileSQLMasters.SqlDataBaseName });
                sqlParaKeys.Add("@SqlAuthenticationMode", new SqlParameter() { SqlDbType = SqlDbType.Bit, Size = SIZE_BIT, SqlValue = fileSQLMasters.SqlAuthenticationMode });
                sqlParaKeys.Add("@SqlUserName", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_MAX, SqlValue = fileSQLMasters.SqlUserName });
                sqlParaKeys.Add("@SqlPassword", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_MAX, SqlValue = fileSQLMasters.SqlPassword });
                sqlParaKeys.Add("@AddedDte", new SqlParameter() { SqlDbType = SqlDbType.DateTime, Size = SIZE_DATETIME, SqlValue = fileSQLMasters.AddedDte });
                sqlParaKeys.Add("@UpdatedDte", new SqlParameter() { SqlDbType = SqlDbType.DateTime, Size = SIZE_DATETIME, SqlValue = fileSQLMasters.UpdatedDte });
                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);


                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspSaveFileSql]",
                                         ref sql);

                 ExecuteSqlCommand(sql, sqlParameters);

                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(GetDBResponseStatusCode(FormatSqlErrorDescParam(sqlParaDesc)), FormatSqlErrorDescParam(sqlParaDesc));


            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
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

                ICollection<FileSQLMasters> result =  SqlQuery<FileSQLMasters>("EXEC [uspGetFileSQLMasters] @errNum,@errDesc ", parameters).ToList();
                return result;
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
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

                var data =  SqlQuery<FileSQLMasters>("EXEC [uspGetAccountByID] @Uid, @errNum,@errDesc ", parameters).FirstOrDefault();
                return CosDBResponse<FileSQLMasters>.CreateDBResponse(data, FormatSqlErrorDescParam(parameters[1]));
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<FileSQLMasters>.CreateDBResponse(null, ex.Message);
            }

        }

        #endregion FileSQLMasters

        #region QBO
        //
        internal CosDBResponse<ICollection<XeroDocumentLine>> GetUpdatedQboDocument(int DocumentID)
        {

            try
            {

                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                string sql = string.Empty;

                sqlParaKeys.Clear();

                sqlParaKeys.Add("@DocumentID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = DocumentID });

                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspGetUpdatedXeroDocument]",
                                         ref sql);

                var data =  SqlQuery<XeroDocumentLine>(sql, sqlParameters).ToList();

                return CosDBResponse<ICollection<XeroDocumentLine>>.CreateDBResponse(data, FormatSqlErrorDescParam(sqlParaDesc));
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<ICollection<XeroDocumentLine>>.CreateDBResponse(null, ex.Message);
            }
        }
        internal CosDBResponse<DBResponseStatusCode> ApproveQboDocument(QboDocumentLine qboDocumentLine)
        {

            try
            {

                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                string sql = string.Empty;

                sqlParaKeys.Clear();
                sqlParaKeys.Add("@DocumentID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = qboDocumentLine.DocumentID });
                sqlParaKeys.Add("@Approve", new SqlParameter() { SqlDbType = SqlDbType.Bit, Size = SIZE_BIT, SqlValue = qboDocumentLine.SelectToBill ?? false });
                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspApproveQboDocument]",
                                         ref sql);


                 ExecuteSqlCommand(sql, sqlParameters);


                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(GetDBResponseStatusCode(CSConvert.ToString(sqlParaDesc.Value)), CSConvert.ToString(sqlParaDesc.Value));

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(DBResponseStatusCode.FAILED, ex.Message);

            }
        }
        object _obj1 = new object();
        internal CosDBResponse<DBResponseStatusCode> ApproveXeroDocument(XeroDocumentLine qboDocumentLine)
        {
            lock(_obj1)
            {
                try
                {

                    SqlParameter sqlParaNum = GetOutputErrNum();
                    SqlParameter sqlParaDesc = GetOutputErrDesc();

                    string sql = string.Empty;

                    sqlParaKeys.Clear();
                    sqlParaKeys.Add("@DocumentID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = qboDocumentLine.DocumentID });
                    sqlParaKeys.Add("@Approve", new SqlParameter() { SqlDbType = SqlDbType.Bit, Size = SIZE_BIT, SqlValue = qboDocumentLine.SelectToBill ?? false });
                    sqlParaKeys.Add("@ApproveDocAs", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = qboDocumentLine.ApproveDocAs });
                    sqlParaKeys.Add("@errNum", sqlParaNum);
                    sqlParaKeys.Add("@errDesc", sqlParaDesc);

                    SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspApproveXeroDocument]",
                                             ref sql);


                     ExecuteSqlCommand(sql, sqlParameters);


                    return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(GetDBResponseStatusCode(CSConvert.ToString(sqlParaDesc.Value)), CSConvert.ToString(sqlParaDesc.Value));

                }
                catch (Exception ex)
                {
                    Logger log = CosmicLogger.SetLog();
                    log.Error(ex);
                    return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(DBResponseStatusCode.FAILED, ex.Message);

                }
            }
            
        }

        internal CosDBResponse<DBResponseStatusCode> UpdateQboDocumentHdr(QboDocumentLine qboDocument)
        {

            try
            {

                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                string sql = string.Empty;

                sqlParaKeys.Clear();
                sqlParaKeys.Add("@DocumentID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = qboDocument.DocumentID });
                sqlParaKeys.Add("@QboVendorID", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_20, SqlValue = qboDocument.QboVendorID });
                sqlParaKeys.Add("@QboVendorName", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_250, SqlValue = qboDocument.QboVendorName ?? string.Empty });

                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspUpdateQboDocument]",
                                         ref sql);


                 ExecuteSqlCommand(sql, sqlParameters);


                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(GetDBResponseStatusCode(CSConvert.ToString(sqlParaDesc.Value)), CSConvert.ToString(sqlParaDesc.Value));

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(DBResponseStatusCode.FAILED, ex.Message);

            }
        }

        internal CosDBResponse<DBResponseStatusCode> UpdateXeroDocumentHdr(XeroDocumentLine xeroDocument)
        {

            try
            {

                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                string sql = string.Empty;

                sqlParaKeys.Clear();
                sqlParaKeys.Add("@DocumentID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = xeroDocument.DocumentID });
                sqlParaKeys.Add("@QboVendorID", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = 50, SqlValue = xeroDocument.XeroVendorID });
                sqlParaKeys.Add("@QboVendorName", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_250, SqlValue = xeroDocument.XeroVendorName ?? string.Empty });

                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspUpdateXeroDocument]",
                                         ref sql);


                 ExecuteSqlCommand(sql, sqlParameters);


                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(GetDBResponseStatusCode(CSConvert.ToString(sqlParaDesc.Value)), CSConvert.ToString(sqlParaDesc.Value));

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(DBResponseStatusCode.FAILED, ex.Message);

            }
        }

        internal CosDBResponse<DBResponseStatusCode> UpdateQboBillID(int qboDocumentID, string qboInvoiceID, string error)
        {

            try
            {

                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                string sql = string.Empty;

                sqlParaKeys.Clear();
                sqlParaKeys.Add("@DocumentID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = qboDocumentID });
                sqlParaKeys.Add("@QboInvoiceID", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_20, SqlValue = qboInvoiceID });
                sqlParaKeys.Add("@Error", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_MAX, SqlValue = error });

                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspUpdateQboBillID]",
                                         ref sql);


                 ExecuteSqlCommand(sql, sqlParameters);


                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(GetDBResponseStatusCode(CSConvert.ToString(sqlParaDesc.Value)), CSConvert.ToString(sqlParaDesc.Value));

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(DBResponseStatusCode.FAILED, ex.Message);

            }
        }

        internal CosDBResponse<DBResponseStatusCode> UpdateXeroBillID(int qboDocumentID, string qboInvoiceID, string error, int status)
        {

            try
            {

                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                string sql = string.Empty;

                sqlParaKeys.Clear();
                sqlParaKeys.Add("@DocumentID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = qboDocumentID });
                sqlParaKeys.Add("@QboInvoiceID", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = 50, SqlValue = qboInvoiceID });
                sqlParaKeys.Add("@Status", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = 50, SqlValue = status });
                sqlParaKeys.Add("@Error", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_MAX, SqlValue = error });

                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspUpdateXeroBillID]",
                                         ref sql);


                 ExecuteSqlCommand(sql, sqlParameters);


                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(GetDBResponseStatusCode(CSConvert.ToString(sqlParaDesc.Value)), CSConvert.ToString(sqlParaDesc.Value));

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(DBResponseStatusCode.FAILED, ex.Message);

            }
        }

        internal CosDBResponse<DBResponseStatusCode> UpdateQboDocumentLine(QboDocumentLine line)
        {

            try
            {

                SqlParameter sqlParaNum = null;
                SqlParameter sqlParaDesc = null;

                sqlParaNum = GetOutputErrNum();
                sqlParaDesc = GetOutputErrDesc();

                string sql = string.Empty;

                sqlParaKeys.Clear();
                sqlParaKeys.Add("@DocumentID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = line.DocumentID });
                sqlParaKeys.Add("@DocumentLineID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = line.DocumentLineID });
                sqlParaKeys.Add("@QboAccountID", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_20, SqlValue = line.QboAccountID ?? string.Empty });
                sqlParaKeys.Add("@QboAccountName", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_250, SqlValue = line.QboAccountName ?? string.Empty });
                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspUpdateQboDocumentLine]",
                                         ref sql);

                 ExecuteSqlCommand(sql, sqlParameters);


                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(GetDBResponseStatusCode(CSConvert.ToString(sqlParaDesc.Value)), CSConvert.ToString(sqlParaDesc.Value));

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(DBResponseStatusCode.FAILED, ex.Message);

            }
        }

        internal CosDBResponse<DBResponseStatusCode> UpdateXeroDocumentLine(XeroDocumentLine line)
        {

            try
            {

                SqlParameter sqlParaNum = null;
                SqlParameter sqlParaDesc = null;

                sqlParaNum = GetOutputErrNum();
                sqlParaDesc = GetOutputErrDesc();

                string sql = string.Empty;

                sqlParaKeys.Clear();
                sqlParaKeys.Add("@DocumentID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = line.DocumentID });
                sqlParaKeys.Add("@DocumentLineID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = line.DocumentLineID });
                sqlParaKeys.Add("@QboAccountID", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = 50, SqlValue = line.XeroAccountID ?? string.Empty });
                sqlParaKeys.Add("@QboAccountName", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_250, SqlValue = line.XeroAccountName ?? string.Empty });

                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspUpdateXeroDocumentLine]",
                                         ref sql);

                 ExecuteSqlCommand(sql, sqlParameters);


                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(GetDBResponseStatusCode(CSConvert.ToString(sqlParaDesc.Value)), CSConvert.ToString(sqlParaDesc.Value));

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(DBResponseStatusCode.FAILED, ex.Message);

            }
        }

        //
        internal CosDBResponse<ICollection<QboDocument>> GetQboDocumentHistory(int month, int year)
        {

            try
            {

                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                string sql = string.Empty;

                sqlParaKeys.Clear();

                sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.AccountID });
                sqlParaKeys.Add("@QboConnectID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.QboConnectID });
                sqlParaKeys.Add("@Month", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = month });
                sqlParaKeys.Add("@Year", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = year });

                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspGetQboDocumentHistory]",
                                         ref sql);

                var data =  SqlQuery<QboDocument>(sql, sqlParameters).ToList();

                return CosDBResponse<ICollection<QboDocument>>.CreateDBResponse(data, FormatSqlErrorDescParam(sqlParaDesc));
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<ICollection<QboDocument>>.CreateDBResponse(null, ex.Message);
            }
        }



        internal CosDBResponse<ICollection<XeroDocument>> GetXeroDocumentHistoryByDate(string frDate,string todate)
        {

            try
            {

                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                string sql = string.Empty;
                
                DateTime fromdate ;
                DateTime.TryParse(frDate, out fromdate);

               DateTime toDate;
                DateTime.TryParse(todate, out toDate);

                    sqlParaKeys.Clear();

                sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.AccountID });
                sqlParaKeys.Add("@QboConnectID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.XeroConnectID });
                sqlParaKeys.Add("@Fromdate", new SqlParameter() { SqlDbType = SqlDbType.DateTime, Size = 20, SqlValue = fromdate });
                sqlParaKeys.Add("@Todate", new SqlParameter() { SqlDbType = SqlDbType.DateTime, Size = 20, SqlValue =toDate });

                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspGetXeroDocumentHistoryByDate]",
                                         ref sql);

                var data =  SqlQuery<XeroDocument>(sql, sqlParameters).ToList();

                return CosDBResponse<ICollection<XeroDocument>>.CreateDBResponse(data, FormatSqlErrorDescParam(sqlParaDesc));
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<ICollection<XeroDocument>>.CreateDBResponse(null, ex.Message);
            }
        }


        internal CosDBResponse<ICollection<XeroDocument>> GetXeroDocumentHistory(int month, int year)
        {

            try
            {

                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                string sql = string.Empty;

                sqlParaKeys.Clear();

                sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.AccountID });
                sqlParaKeys.Add("@QboConnectID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.XeroConnectID });
                sqlParaKeys.Add("@Month", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = month });
                sqlParaKeys.Add("@Year", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = year });

                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspGetXeroDocumentHistory]",
                                         ref sql);

                var data =  SqlQuery<XeroDocument>(sql, sqlParameters).ToList();

                return CosDBResponse<ICollection<XeroDocument>>.CreateDBResponse(data, FormatSqlErrorDescParam(sqlParaDesc));
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<ICollection<XeroDocument>>.CreateDBResponse(null, ex.Message);
            }
        }

        internal CosDBResponse<CheckXeroToken> GetCheckXeroToken(int accountID, int xeroID)
        {

            try
            {

                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                string sql = string.Empty;

                sqlParaKeys.Clear();

                sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = accountID });
                sqlParaKeys.Add("@XeroID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = xeroID });
                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspCheckXeroToken]",
                                         ref sql);

                var data =  SqlQuery<CheckXeroToken>(sql, sqlParameters).FirstOrDefault();

                return CosDBResponse<CheckXeroToken>.CreateDBResponse(data, FormatSqlErrorDescParam(sqlParaDesc));
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<CheckXeroToken>.CreateDBResponse(null, ex.Message);
            }
        }

        internal CosDBResponse<ICollection<QboDocument>> GetQboDocumentToApprove(bool approved)
        {

            try
            {

                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                string sql = string.Empty;

                sqlParaKeys.Clear();

                sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.AccountID });
                sqlParaKeys.Add("@QboConnectID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.QboConnectID });
                sqlParaKeys.Add("@AspSessionID", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_MAX, SqlValue = string.Empty });
                sqlParaKeys.Add("@Approved", new SqlParameter() { SqlDbType = SqlDbType.Bit, Size = SIZE_BIT, SqlValue = approved });
                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspGetQboDocumentToApprove]",
                                         ref sql);

                var data =  SqlQuery<QboDocument>(sql, sqlParameters).ToList();

                return CosDBResponse<ICollection<QboDocument>>.CreateDBResponse(data, FormatSqlErrorDescParam(sqlParaDesc));
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<ICollection<QboDocument>>.CreateDBResponse(null, ex.Message);
            }
        }

        internal CosDBResponse<ICollection<XeroDocument>> GetXeroDocumentToApprove(bool approved)
        {

            try
            {

                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                string sql = string.Empty;

                sqlParaKeys.Clear();

                sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.AccountID });
                sqlParaKeys.Add("@QboConnectID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.XeroConnectID });
                sqlParaKeys.Add("@AspSessionID", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_MAX, SqlValue = string.Empty });
                sqlParaKeys.Add("@Approved", new SqlParameter() { SqlDbType = SqlDbType.Bit, Size = SIZE_BIT, SqlValue = approved });
                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspGetXeroDocumentToApprove]",
                                         ref sql);

                var data =  SqlQuery<XeroDocument>(sql, sqlParameters).ToList();

                return CosDBResponse<ICollection<XeroDocument>>.CreateDBResponse(data, FormatSqlErrorDescParam(sqlParaDesc));
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<ICollection<XeroDocument>>.CreateDBResponse(null, ex.Message);
            }
        }

        internal CosDBResponse<ICollection<QboDocument>> GetQboDocumentToScan()
        {

            try
            {

                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                string sql = string.Empty;

                sqlParaKeys.Clear();

                sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.AccountID });
                sqlParaKeys.Add("@QboConnectID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.QboConnectID });

                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspGetQboDocumentToProcess]",
                                         ref sql);

                var data =  SqlQuery<QboDocument>(sql, sqlParameters).ToList();

                return CosDBResponse<ICollection<QboDocument>>.CreateDBResponse(data, FormatSqlErrorDescParam(sqlParaDesc));
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<ICollection<QboDocument>>.CreateDBResponse(null, ex.Message);
            }
        }

        internal CosDBResponse<ICollection<XeroDocument>> GetXeroDocumentToScan()
        {

            try
            {

                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                string sql = string.Empty;

                sqlParaKeys.Clear();

                sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.AccountID });
                sqlParaKeys.Add("@QboConnectID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.XeroConnectID });

                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspGetXeroDocumentToProcess]",
                                         ref sql);

                var data =  SqlQuery<XeroDocument>(sql, sqlParameters).ToList();

                var qboJobs = GetQboJobToProcess();
               
                if (qboJobs?.Data?.Count() > 0)
                {
                    List<string> qboDocuments = new List<string>();
                    List<XeroDocument> qboUpdatedDocuments = new List<XeroDocument>();
                    foreach (QboJob qboJob in qboJobs.Data)
                    {
                        List<string> qbotempDocuments = qboJob.DocumentIDs.ToString().Split(',').ToList();
                        qboDocuments.AddRange(qbotempDocuments);
                    }
                    foreach (XeroDocument doc in data)
                    {
                        if (!qboDocuments.Contains(doc.DocumentID.ToString()))
                        {
                            qboUpdatedDocuments.Add(doc);
                        }
                    }
                    return CosDBResponse<ICollection<XeroDocument>>.CreateDBResponse(qboUpdatedDocuments, FormatSqlErrorDescParam(sqlParaDesc));

                }
                else
                     return CosDBResponse<ICollection<XeroDocument>>.CreateDBResponse(data, FormatSqlErrorDescParam(sqlParaDesc));
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<ICollection<XeroDocument>>.CreateDBResponse(null, ex.Message);
            }
        }
        internal CosDBResponse<ICollection<QboJob>> GetQboJobToProcess()
        {

            try
            {

                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                string sql = string.Empty;

                sqlParaKeys.Clear();


                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspGetQboJob]",
                                         ref sql);

                var data =  SqlQuery<QboJob>(sql, sqlParameters).ToList();

                return CosDBResponse<ICollection<QboJob>>.CreateDBResponse(data, FormatSqlErrorDescParam(sqlParaDesc));
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<ICollection<QboJob>>.CreateDBResponse(null, ex.Message);
            }
        }
        internal CosDBResponse<QboDocument> GetQboDocument(int qboDocumentID)
        {

            try
            {
                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                string sql = string.Empty;

                sqlParaKeys.Clear();

                sqlParaKeys.Add("@qboDocumentID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = qboDocumentID }); //SessionHelper.QboConnectID

                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspGetQboDocumentById]",
                                         ref sql);

                var data =  SqlQuery<QboDocument>(sql, sqlParameters).FirstOrDefault();

                return CosDBResponse<QboDocument>.CreateDBResponse(data, FormatSqlErrorDescParam(sqlParaDesc));
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<QboDocument>.CreateDBResponse(null, ex.Message);
            }
        }

        internal CosDBResponse<XeroDocument> GetXeroDocument(int qboDocumentID)
        {

            try
            {
                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                string sql = string.Empty;

                sqlParaKeys.Clear();

                sqlParaKeys.Add("@qboDocumentID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = qboDocumentID }); //SessionHelper.QboConnectID

                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspGetXeroDocumentById]",
                                         ref sql);

                var data =  SqlQuery<XeroDocument>(sql, sqlParameters).FirstOrDefault();

                return CosDBResponse<XeroDocument>.CreateDBResponse(data, FormatSqlErrorDescParam(sqlParaDesc));
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<XeroDocument>.CreateDBResponse(null, ex.Message);
            }
        }

        internal CosDBResponse<ICollection<CBDocument>> GetQboDocumentToRead(string sessionID)
        {

            try
            {

                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                string sql = string.Empty;

                sqlParaKeys.Clear();

                sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.AccountID });
                sqlParaKeys.Add("@QboConnectID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.QboConnectID }); //SessionHelper.QboConnectID
                sqlParaKeys.Add("@AspSessionID", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_MAX, SqlValue = sessionID });
                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspGetQboDocumentToRead]",
                                         ref sql);

                var data =  SqlQuery<CBDocument>(sql, sqlParameters).ToList();

                return CosDBResponse<ICollection<CBDocument>>.CreateDBResponse(data, FormatSqlErrorDescParam(sqlParaDesc));
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<ICollection<CBDocument>>.CreateDBResponse(null, ex.Message);
            }
        }


        internal CosDBResponse<ICollection<CBDocument>> GetXeroDocumentToRead(string sessionID)
        {

            try
            {

                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                string sql = string.Empty;

                sqlParaKeys.Clear();

                sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.AccountID });
                sqlParaKeys.Add("@QboConnectID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.XeroConnectID }); //SessionHelper.QboConnectID
                sqlParaKeys.Add("@AspSessionID", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_MAX, SqlValue = sessionID });
                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspGetXeroDocumentToRead]",
                                         ref sql);

                var data =  SqlQuery<CBDocument>(sql, sqlParameters).ToList();

                return CosDBResponse<ICollection<CBDocument>>.CreateDBResponse(data, FormatSqlErrorDescParam(sqlParaDesc));
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<ICollection<CBDocument>>.CreateDBResponse(null, ex.Message);
            }
        }



        internal CosDBResponse<ICollection<QboDocumentLine>> GetQboDocumentLineToApprove(bool approved)
        {

            try
            {

                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                string sql = string.Empty;

                sqlParaKeys.Clear();

                sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.AccountID });
                sqlParaKeys.Add("@QboConnectID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.QboConnectID }); //SessionHelper.QboConnectID
                sqlParaKeys.Add("@Approved", new SqlParameter() { SqlDbType = SqlDbType.Bit, Size = SIZE_BIT, SqlValue = approved });

                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspGetQboDocumentLineToApprove]",
                                         ref sql);

                var data =  SqlQuery<QboDocumentLine>(sql, sqlParameters).ToList();

                return CosDBResponse<ICollection<QboDocumentLine>>.CreateDBResponse(data, FormatSqlErrorDescParam(sqlParaDesc));
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<ICollection<QboDocumentLine>>.CreateDBResponse(null, ex.Message);
            }
        }


        internal CosDBResponse<ICollection<QboDocumentLine>> GetQboDocumentToBill(bool approved)
        {

            try
            {

                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                string sql = string.Empty;

                sqlParaKeys.Clear();

                sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.AccountID });
                sqlParaKeys.Add("@QboConnectID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.QboConnectID }); //SessionHelper.QboConnectID
                sqlParaKeys.Add("@Approved", new SqlParameter() { SqlDbType = SqlDbType.Bit, Size = SIZE_BIT, SqlValue = approved });

                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspGetQboDocumentToBill]",
                                         ref sql);

                var data =  SqlQuery<QboDocumentLine>(sql, sqlParameters).ToList();
                if (approved == true)
                    data = data.ToList().Where(a => a.fromEmail == false).ToList();
                return CosDBResponse<ICollection<QboDocumentLine>>.CreateDBResponse(data, FormatSqlErrorDescParam(sqlParaDesc));
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<ICollection<QboDocumentLine>>.CreateDBResponse(null, ex.Message);
            }
        }
        internal CosDBResponse<ICollection<XeroDocumentLine>> GetXeroDocumentToAuth()
        {

            try
            {

                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                string sql = string.Empty;

                sqlParaKeys.Clear();

                sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.AccountID });
                sqlParaKeys.Add("@QboConnectID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.XeroConnectID }); //SessionHelper.QboConnectID
                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspGetXeroDocumentToAuth]",
                                         ref sql);

                var data = SqlQuery<XeroDocumentLine>(sql, sqlParameters).ToList();

                return CosDBResponse<ICollection<XeroDocumentLine>>.CreateDBResponse(data, FormatSqlErrorDescParam(sqlParaDesc));
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<ICollection<XeroDocumentLine>>.CreateDBResponse(null, ex.Message);
            }
        }

        internal CosDBResponse<DBResponseStatusCode> UpdateXeroMaster(XeroMaster xeroMaster)
        {

            try
            {
                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();
                string sql = string.Empty;

                sqlParaKeys.Clear();
                sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = xeroMaster.AccountID });
                sqlParaKeys.Add("@AsyncBackEndScanning", new SqlParameter() { SqlDbType = SqlDbType.Bit, Size = 1, SqlValue = xeroMaster.AsyncBackEndScanning });
                sqlParaKeys.Add("@DirectPostfromEmail", new SqlParameter() { SqlDbType = SqlDbType.Bit, Size = 1, SqlValue = xeroMaster.DirectPostfromEmail });
                sqlParaKeys.Add("@XeroDocPostAs", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_MAX, SqlValue = xeroMaster.XeroDocPostAs ?? string.Empty });

                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspUpdateXeroMaster]", ref sql);
                 ExecuteSqlCommand(sql, sqlParameters);

                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(GetDBResponseStatusCode(FormatSqlErrorDescParam(sqlParaDesc)), FormatSqlErrorDescParam(sqlParaDesc));

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(DBResponseStatusCode.FAILED, ex.Message);
            }
        }

        object objGetXeroDocumentToBill = new object();
        internal CosDBResponse<ICollection<XeroDocumentLine>> GetXeroDocumentToBill(bool approved)
        {
            lock(objGetXeroDocumentToBill)
            {
                try
                {

                    SqlParameter sqlParaNum = GetOutputErrNum();
                    SqlParameter sqlParaDesc = GetOutputErrDesc();

                    string sql = string.Empty;

                    sqlParaKeys.Clear();

                    sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.AccountID });
                    sqlParaKeys.Add("@QboConnectID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.XeroConnectID }); //SessionHelper.QboConnectID
                    sqlParaKeys.Add("@Approved", new SqlParameter() { SqlDbType = SqlDbType.Bit, Size = SIZE_BIT, SqlValue = approved });

                    sqlParaKeys.Add("@errNum", sqlParaNum);
                    sqlParaKeys.Add("@errDesc", sqlParaDesc);

                    SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspGetXeroDocumentToBill]",
                                             ref sql);

                    var data =  SqlQuery<XeroDocumentLine>(sql, sqlParameters).ToList();
                    if (approved == true)
                        data = data.ToList().Where(a => a.fromEmail == false).ToList();
                    return CosDBResponse<ICollection<XeroDocumentLine>>.CreateDBResponse(data, FormatSqlErrorDescParam(sqlParaDesc));
                }
                catch (Exception ex)
                {
                    Logger log = CosmicLogger.SetLog();
                    log.Error(ex);
                    return CosDBResponse<ICollection<XeroDocumentLine>>.CreateDBResponse(null, ex.Message);
                }

            }
        }


        internal CosDBResponse<QboMaster> SaveQboMaster(QboMaster qboMaster, ref int qboConnectID)
        {
            Logger _log = CosmicLogger.SetLog();
            try
            {

                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();
                SqlParameter sqlParaIdentity = GetOutputIdentity();
                string sql = string.Empty;

                sqlParaKeys.Clear();
                sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.AccountID });
                sqlParaKeys.Add("@AccessToken", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_MAX, SqlValue = qboMaster.AccessToken ?? string.Empty });
                sqlParaKeys.Add("@AccessTokenExpiresIn", new SqlParameter() { SqlDbType = SqlDbType.DateTime, Size = SIZE_VARCHAR_MAX, SqlValue = qboMaster.AccessTokenExpiresIn });

                sqlParaKeys.Add("@RealmId", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_MAX, SqlValue = qboMaster.RealmId ?? string.Empty });
                sqlParaKeys.Add("@RefreshToken", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_MAX, SqlValue = qboMaster.RefreshToken ?? string.Empty });
                sqlParaKeys.Add("@RefreshTokenExpiresIn", new SqlParameter() { SqlDbType = SqlDbType.DateTime, Size = SIZE_VARCHAR_MAX, SqlValue = qboMaster.RefreshTokenExpiresIn });

                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);
                sqlParaKeys.Add("@identity", sqlParaIdentity);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspSaveQBOMaster]", ref sql);


                 ExecuteSqlCommand(sql, sqlParameters);

                if (sqlParaIdentity != null)
                {
                    qboConnectID = CSConvert.ToInt(sqlParaIdentity.Value);
                }

                qboMaster.QboID = qboConnectID;

                _log.Info(CSConvert.ToString(sqlParaDesc.Value));

                return CosDBResponse<QboMaster>.CreateDBResponse(qboMaster, CSConvert.ToString(sqlParaDesc.Value));

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                _log.Info("SaveQboMaster" + ex.Message);
                return CosDBResponse<QboMaster>.CreateDBResponse(qboMaster, ex.Message);
            }
        }

        internal CosDBResponse<ICollection<QboMaster>> GetQboMasterByAccountID()
        {

            try
            {

                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                string sql = string.Empty;

                sqlParaKeys.Clear();

                sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.AccountID });
                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspGetQBOMasterByAccountID]",
                                         ref sql);

                var data =  SqlQuery<QboMaster>(sql, sqlParameters).ToList();

                return CosDBResponse<ICollection<QboMaster>>.CreateDBResponse(data, FormatSqlErrorDescParam(sqlParaDesc));
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<ICollection<QboMaster>>.CreateDBResponse(null, ex.Message);
            }
        }

        object obj2 = new object();
        internal CosDBResponse<ICollection<XeroMaster>> GetXeroMasterByAccountID()
        {
            lock(obj2)
            {
                Logger log = CosmicLogger.SetLog();
                try
                {
                    log.Info("GetXeroMasterByAccountID started");
                    SqlParameter sqlParaNum = GetOutputErrNum();
                    SqlParameter sqlParaDesc = GetOutputErrDesc();

                    string sql = string.Empty;

                    sqlParaKeys.Clear();
                    log.Info("GetXeroMasterByAccountID clear");
                    sqlParaKeys["@AccountID"] = new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.AccountID };
                    sqlParaKeys["@errNum"] = sqlParaNum;
                    sqlParaKeys["@errDesc"] = sqlParaDesc;
                    log.Info("GetXeroMasterByAccountID before execute");
                    SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspGetXeroMasterByAccountID]",
                                             ref sql);
                    log.Info("GetXeroMasterByAccountID after execute");
                    var data =  SqlQuery<XeroMaster>(sql, sqlParameters).ToList();
                    log.Info("GetXeroMasterByAccountID data" + data.Count);
                    return CosDBResponse<ICollection<XeroMaster>>.CreateDBResponse(data, FormatSqlErrorDescParam(sqlParaDesc));
                }
                catch (Exception ex)
                {
                    log.Error(ex);
                    return CosDBResponse<ICollection<XeroMaster>>.CreateDBResponse(null, ex.Message);
                }
            }
        }

      

        internal CosDBResponse<DBResponseStatusCode> UpdateReAuthrorizeByAccountID(bool IsAuthrorize)
        {

            try
            {

                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                string sql = string.Empty;

                sqlParaKeys.Clear();

                sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.AccountID });
                sqlParaKeys.Add("@IsAuthrorize", new SqlParameter() { SqlDbType = SqlDbType.Bit, Size = 1, SqlValue = IsAuthrorize });
                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspUpdateReAuthrorizeByAccountID]",
                                         ref sql);

                var data =  SqlQuery<DBResponseStatusCode>(sql, sqlParameters).ToList();

                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(GetDBResponseStatusCode(CSConvert.ToString(sqlParaDesc.Value)), CSConvert.ToString(sqlParaDesc.Value));
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(DBResponseStatusCode.FAILED, ex.Message);
            }
        }

        internal CosDBResponse<QboMaster> GetQboMasterByAccountAndConnectID()
        {
            try
            {

                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                string sql = string.Empty;

                sqlParaKeys.Clear();

                sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.AccountID });
                sqlParaKeys.Add("@QboConnectID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.QboConnectID }); //SessionHelper.QboConnectID
                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspGetQBOMasterByAccountAndConnectID]",
                                         ref sql);

                var data =  SqlQuery<QboMaster>(sql, sqlParameters).FirstOrDefault();

                return CosDBResponse<QboMaster>.CreateDBResponse(data, FormatSqlErrorDescParam(sqlParaDesc));
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<QboMaster>.CreateDBResponse(null, ex.Message);
            }
        }

        internal CosDBResponse<XeroMaster> GetXeroMasterByAccountAndConnectID()
        {
            try
            {

                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                string sql = string.Empty;

                sqlParaKeys.Clear();

                sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.AccountID });
                sqlParaKeys.Add("@XeroConnectID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.XeroConnectID }); //SessionHelper.QboConnectID
                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspGetXeroMasterByAccountAndConnectID]",
                                         ref sql);

                var data =  SqlQuery<XeroMaster>(sql, sqlParameters).FirstOrDefault();

                return CosDBResponse<XeroMaster>.CreateDBResponse(data, FormatSqlErrorDescParam(sqlParaDesc));
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<XeroMaster>.CreateDBResponse(null, ex.Message);
            }
        }


        internal CosDBResponse<DBResponseStatusCode> SaveQboCompanyInfo(string realmID, CompanyInfo companyInfo)
        {

            try
            {
                QboMaster qboMaster = new QboMaster();

                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                string sql = string.Empty;

                sqlParaKeys.Clear();

                sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.AccountID });
                sqlParaKeys.Add("@RealmId", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_MAX, SqlValue = realmID });
                sqlParaKeys.Add("@CompanyName", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_MAX, SqlValue = companyInfo.CompanyName ?? string.Empty });
                sqlParaKeys.Add("@LegalName", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_MAX, SqlValue = companyInfo.LegalName ?? string.Empty });
                sqlParaKeys.Add("@Email", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_MAX, SqlValue = CSConvert.ToString(companyInfo.Email.Address) });
                sqlParaKeys.Add("@PrimaryPhone", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_MAX, SqlValue = CSConvert.ToString(companyInfo.PrimaryPhone.FreeFormNumber) });
                sqlParaKeys.Add("@WebAddr", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_MAX, SqlValue = CSConvert.ToString(companyInfo.WebAddr.URI) });
                sqlParaKeys.Add("@Country", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_MAX, SqlValue = companyInfo.Country ?? string.Empty });

                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspUpdateQBOCompanyInfo]",
                                      ref sql);

                 ExecuteSqlCommand(sql, sqlParameters);

                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(GetDBResponseStatusCode(CSConvert.ToString(sqlParaDesc.Value)), CSConvert.ToString(sqlParaDesc.Value));

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(DBResponseStatusCode.FAILED, ex.Message);
            }
        }

        internal CosDBResponse<DBResponseStatusCode> SaveXeroCompanyInfo(Organisation companyInfo)
        {

            try
            {
                QboMaster qboMaster = new QboMaster();

                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                string sql = string.Empty;

                sqlParaKeys.Clear();

                sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.AccountID });
                sqlParaKeys.Add("@RealmId", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_MAX, SqlValue = _sessionHelper.XeroRealmId });
                sqlParaKeys.Add("@CompanyName", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_MAX, SqlValue = companyInfo.Name ?? string.Empty });
                sqlParaKeys.Add("@LegalName", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_MAX, SqlValue = companyInfo.LegalName ?? string.Empty });
                sqlParaKeys.Add("@Email", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_MAX, SqlValue = "" });
                sqlParaKeys.Add("@PrimaryPhone", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_MAX, SqlValue = "" });
                sqlParaKeys.Add("@WebAddr", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_MAX, SqlValue = "" });
                sqlParaKeys.Add("@Country", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_MAX, SqlValue = companyInfo.CountryCode.ToString() ?? string.Empty });
                sqlParaKeys.Add("@ShortCode", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = 10, SqlValue = companyInfo.ShortCode ?? string.Empty });
                
                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspUpdateXeroCompanyInfo]",
                                      ref sql);

                 ExecuteSqlCommand(sql, sqlParameters);

                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(GetDBResponseStatusCode(CSConvert.ToString(sqlParaDesc.Value)), CSConvert.ToString(sqlParaDesc.Value));

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(DBResponseStatusCode.FAILED, ex.Message);
            }
        }

        //

        internal CosDBResponse<ICollection<QboChartOfAccount>> GetQboChartOfAccount()
        {

            try
            {

                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                string sql = string.Empty;

                sqlParaKeys.Clear();

                sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.AccountID });
                sqlParaKeys.Add("@QboConnectID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.QboConnectID });
                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspGetQboChartOfAccount]",
                                         ref sql);

                var data =  SqlQuery<QboChartOfAccount>(sql, sqlParameters).ToList();

                return CosDBResponse<ICollection<QboChartOfAccount>>.CreateDBResponse(data, FormatSqlErrorDescParam(sqlParaDesc));
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<ICollection<QboChartOfAccount>>.CreateDBResponse(null, ex.Message);
            }
        }

        internal CosDBResponse<ICollection<XeroChartOfAccount>> GetXeroChartOfAccount()
        {

            try
            {

                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                string sql = string.Empty;

                sqlParaKeys.Clear();

                sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.AccountID });
                sqlParaKeys.Add("@QboConnectID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.XeroConnectID });
                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspGetXeroChartOfAccount]",
                                         ref sql);

                var data =  SqlQuery<XeroChartOfAccount>(sql, sqlParameters).ToList();

                return CosDBResponse<ICollection<XeroChartOfAccount>>.CreateDBResponse(data, FormatSqlErrorDescParam(sqlParaDesc));
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<ICollection<XeroChartOfAccount>>.CreateDBResponse(null, ex.Message);
            }
        }


        internal CosDBResponse<ICollection<QboVendor>> GetQboVendor()
        {

            try
            {

                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                string sql = string.Empty;

                sqlParaKeys.Clear();

                sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.AccountID });
                sqlParaKeys.Add("@QboConnectID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.QboConnectID });
                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspGetQboVendor]",
                                         ref sql);

                var data =  SqlQuery<QboVendor>(sql, sqlParameters).ToList();

                return CosDBResponse<ICollection<QboVendor>>.CreateDBResponse(data, FormatSqlErrorDescParam(sqlParaDesc));
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<ICollection<QboVendor>>.CreateDBResponse(null, ex.Message);
            }
        }


        internal CosDBResponse<ICollection<XeroVendor>> GetXeroVendor()
        {

            try
            {

                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                string sql = string.Empty;

                sqlParaKeys.Clear();

                sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.AccountID });
                sqlParaKeys.Add("@QboConnectID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.XeroConnectID });
                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspGetXeroVendor]",
                                         ref sql);

                var data =  SqlQuery<XeroVendor>(sql, sqlParameters).ToList();

                return CosDBResponse<ICollection<XeroVendor>>.CreateDBResponse(data, FormatSqlErrorDescParam(sqlParaDesc));
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<ICollection<XeroVendor>>.CreateDBResponse(null, ex.Message);
            }
        }

        //
        internal CosDBResponse<DBResponseStatusCode> SaveAllQboChartOfAccount(ICollection<Intuit.Ipp.Data.Account> lstQboAccount)
        {

            try
            {
                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                foreach (var item in lstQboAccount)
                {
                    string sql = string.Empty;

                    sqlParaKeys.Clear();

                    sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.AccountID });
                    sqlParaKeys.Add("@QboConnectID", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_INT, SqlValue = _sessionHelper.QboConnectID });
                    sqlParaKeys.Add("@QboAccountID", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_20, SqlValue = item.Id });

                    sqlParaKeys.Add("@FullyQualifiedNameField", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_100, SqlValue = item.FullyQualifiedName ?? string.Empty });
                    sqlParaKeys.Add("@AccountSubTypeField", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_50, SqlValue = item.AccountSubType ?? string.Empty });

                    sqlParaKeys.Add("@errNum", sqlParaNum);
                    sqlParaKeys.Add("@errDesc", sqlParaDesc);

                    SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspSaveQboChartOfAccount]",
                                          ref sql);

                     ExecuteSqlCommand(sql, sqlParameters);
                }

                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(GetDBResponseStatusCode(CSConvert.ToString(sqlParaDesc.Value)), CSConvert.ToString(sqlParaDesc.Value));

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(DBResponseStatusCode.FAILED, ex.Message);
            }
        }

        internal CosDBResponse<DBResponseStatusCode> SaveAllXeroChartOfAccount(List<Xero.NetStandard.OAuth2.Model.Accounting.Account> lstQboAccount)
        {

            try
            {
                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                foreach (var item in lstQboAccount)
                {
                    string sql = string.Empty;

                    sqlParaKeys.Clear();

                    sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.AccountID });
                    sqlParaKeys.Add("@QboConnectID", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_INT, SqlValue = _sessionHelper.XeroConnectID });
                    sqlParaKeys.Add("@QboAccountID", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = 50, SqlValue = Convert.ToString(item.AccountID) });

                    sqlParaKeys.Add("@FullyQualifiedNameField", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_100, SqlValue = item.Name ?? string.Empty });
                    sqlParaKeys.Add("@AccountSubTypeField", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_50, SqlValue = item.TaxType ?? string.Empty });
                    sqlParaKeys.Add("@AccountCode", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = 5, SqlValue = item.Code ?? string.Empty });

                    sqlParaKeys.Add("@errNum", sqlParaNum);
                    sqlParaKeys.Add("@errDesc", sqlParaDesc);

                    SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspSaveXeroChartOfAccount]",
                                          ref sql);

                     ExecuteSqlCommand(sql, sqlParameters);
                }

                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(GetDBResponseStatusCode(CSConvert.ToString(sqlParaDesc.Value)), CSConvert.ToString(sqlParaDesc.Value));

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(DBResponseStatusCode.FAILED, ex.Message);
            }
        }


        internal CosDBResponse<ICollection<QboTax>> GetQboTax()
        {

            try
            {

                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                string sql = string.Empty;

                sqlParaKeys.Clear();

                sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.AccountID });
                sqlParaKeys.Add("@QboConnectID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.QboConnectID });
                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspGetQboTax]",
                                         ref sql);

                var data =  SqlQuery<QboTax>(sql, sqlParameters).ToList();

                return CosDBResponse<ICollection<QboTax>>.CreateDBResponse(data, FormatSqlErrorDescParam(sqlParaDesc));
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<ICollection<QboTax>>.CreateDBResponse(null, ex.Message);
            }
        }


        internal CosDBResponse<ICollection<XeroTax>> GetXeroTax()
        {

            try
            {

                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                string sql = string.Empty;

                sqlParaKeys.Clear();

                sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.AccountID });
                sqlParaKeys.Add("@QboConnectID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.XeroConnectID });
                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspGetXeroTax]",
                                         ref sql);

                var data =  SqlQuery<XeroTax>(sql, sqlParameters).ToList();

                return CosDBResponse<ICollection<XeroTax>>.CreateDBResponse(data, FormatSqlErrorDescParam(sqlParaDesc));
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<ICollection<XeroTax>>.CreateDBResponse(null, ex.Message);
            }
        }


        internal CosDBResponse<DBResponseStatusCode> SaveQboTax(TaxCode item)
        {
            try
            {
                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                string sql = string.Empty;

                sqlParaKeys.Clear();

                sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.AccountID });
                sqlParaKeys.Add("@QboConnectID", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_INT, SqlValue = _sessionHelper.QboConnectID });
                sqlParaKeys.Add("@TaxID", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_20, SqlValue = item.Id });
                sqlParaKeys.Add("@TaxName", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_50, SqlValue = item.Name });

                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspSaveQboTax]", ref sql);

                 ExecuteSqlCommand(sql, sqlParameters);


                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(GetDBResponseStatusCode(CSConvert.ToString(sqlParaDesc.Value)), CSConvert.ToString(sqlParaDesc.Value));

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(DBResponseStatusCode.FAILED, ex.Message);
            }
        }

        internal CosDBResponse<DBResponseStatusCode> SaveXeroTax(List<Xero.NetStandard.OAuth2.Model.Accounting.TaxRate> lstxeroTax)
        {

            try
            {
                foreach (var item in lstxeroTax)
                {
                    SaveXeroTax(item);
                }

                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(GetDBResponseStatusCode(string.Empty), CSConvert.ToString(string.Empty));

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(DBResponseStatusCode.FAILED, ex.Message);
            }
        }

        internal CosDBResponse<DBResponseStatusCode> SaveXeroTax(Xero.NetStandard.OAuth2.Model.Accounting.TaxRate item)
        {
            try
            {
                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                string sql = string.Empty;

                sqlParaKeys.Clear();

                sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.AccountID });
                sqlParaKeys.Add("@QboConnectID", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_INT, SqlValue = _sessionHelper.XeroConnectID });
                sqlParaKeys.Add("@TaxID", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = 50, SqlValue = item.TaxType });
                sqlParaKeys.Add("@TaxName", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_50, SqlValue = item.Name });

                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspSaveXeroTax]", ref sql);

                 ExecuteSqlCommand(sql, sqlParameters);


                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(GetDBResponseStatusCode(CSConvert.ToString(sqlParaDesc.Value)), CSConvert.ToString(sqlParaDesc.Value));

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(DBResponseStatusCode.FAILED, ex.Message);
            }
        }

        internal CosDBResponse<DBResponseStatusCode> SaveQboVendorUnknow(string id,string name)
        {
            try
            {
                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                string sql = string.Empty;

                sqlParaKeys.Clear();

                sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.AccountID });
                sqlParaKeys.Add("@QboConnectID", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_INT, SqlValue = _sessionHelper.QboConnectID });
                sqlParaKeys.Add("@QboVendorID", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_20, SqlValue =id });

                sqlParaKeys.Add("@DisplayNameField", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_150, SqlValue = name ?? string.Empty });

               
                    sqlParaKeys.Add("@CityField", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_150, SqlValue = string.Empty });
                    sqlParaKeys.Add("@CountrySubDivisionCodeField", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_150, SqlValue = string.Empty });
                    sqlParaKeys.Add("@PostalCodeField", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_150, SqlValue = string.Empty });
                    sqlParaKeys.Add("@CountryField", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_150, SqlValue = string.Empty });
                


                sqlParaKeys.Add("@PrimaryPhone", new SqlParameter()
                {
                    SqlDbType = SqlDbType.VarChar,
                    Size = SIZE_VARCHAR_20,
                    SqlValue =string.Empty
                });

                sqlParaKeys.Add("@PrimaryEmailAddr", new SqlParameter()
                {
                    SqlDbType = SqlDbType.VarChar,
                    Size = SIZE_VARCHAR_100,
                    SqlValue =  string.Empty
                });

                sqlParaKeys.Add("@TaxIdentifier", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_20, SqlValue =  string.Empty });


                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspSaveQboVendor]", ref sql);

                 ExecuteSqlCommand(sql, sqlParameters);


                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(GetDBResponseStatusCode(CSConvert.ToString(sqlParaDesc.Value)), CSConvert.ToString(sqlParaDesc.Value));

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(DBResponseStatusCode.FAILED, ex.Message);
            }
        }

        internal CosDBResponse<DBResponseStatusCode> SaveQboVendor(Vendor item)
        {
            try
            {
                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                string sql = string.Empty;

                sqlParaKeys.Clear();

                sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.AccountID });
                sqlParaKeys.Add("@QboConnectID", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_INT, SqlValue = _sessionHelper.QboConnectID });
                sqlParaKeys.Add("@QboVendorID", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_20, SqlValue = item.Id });

                sqlParaKeys.Add("@DisplayNameField", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_150, SqlValue = item.DisplayName ?? string.Empty });

                if (item.BillAddr != null)
                {
                    sqlParaKeys.Add("@CityField", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_150, SqlValue = item.BillAddr.City ?? string.Empty });
                    sqlParaKeys.Add("@CountrySubDivisionCodeField", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_150, SqlValue = item.BillAddr.CountrySubDivisionCode ?? string.Empty });
                    sqlParaKeys.Add("@PostalCodeField", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_150, SqlValue = item.BillAddr.PostalCode ?? string.Empty });
                    sqlParaKeys.Add("@CountryField", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_150, SqlValue = item.BillAddr.Country ?? string.Empty });
                }
                else
                {
                    sqlParaKeys.Add("@CityField", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_150, SqlValue = string.Empty });
                    sqlParaKeys.Add("@CountrySubDivisionCodeField", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_150, SqlValue = string.Empty });
                    sqlParaKeys.Add("@PostalCodeField", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_150, SqlValue = string.Empty });
                    sqlParaKeys.Add("@CountryField", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_150, SqlValue = string.Empty });
                }


                sqlParaKeys.Add("@PrimaryPhone", new SqlParameter()
                {
                    SqlDbType = SqlDbType.VarChar,
                    Size = SIZE_VARCHAR_20,
                    SqlValue = (item.PrimaryPhone != null ? (item.PrimaryPhone.FreeFormNumber ?? string.Empty) : string.Empty)
                });

                sqlParaKeys.Add("@PrimaryEmailAddr", new SqlParameter()
                {
                    SqlDbType = SqlDbType.VarChar,
                    Size = SIZE_VARCHAR_100,
                    SqlValue = (item.PrimaryEmailAddr != null ? (item.PrimaryEmailAddr.Address ?? string.Empty) : string.Empty)
                });

                sqlParaKeys.Add("@TaxIdentifier", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_20, SqlValue = item.TaxIdentifier ?? string.Empty });


                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspSaveQboVendor]", ref sql);

                 ExecuteSqlCommand(sql, sqlParameters);


                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(GetDBResponseStatusCode(CSConvert.ToString(sqlParaDesc.Value)), CSConvert.ToString(sqlParaDesc.Value));

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(DBResponseStatusCode.FAILED, ex.Message);
            }
        }
        internal CosDBResponse<DBResponseStatusCode> SaveXeroVendorUnKnow(string id ,string name)
        {
            try
            {
                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                string sql = string.Empty;

                sqlParaKeys.Clear();

                sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.AccountID });
                sqlParaKeys.Add("@QboConnectID", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_INT, SqlValue = _sessionHelper.XeroConnectID });
                sqlParaKeys.Add("@QboVendorID", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = 50, SqlValue = id });

                sqlParaKeys.Add("@DisplayNameField", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_150, SqlValue = name ?? string.Empty });

                //if (item.BillAddr != null)
                //{
                //    sqlParaKeys.Add("@CityField", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_150, SqlValue = item.BillAddr.City ?? string.Empty });
                //    sqlParaKeys.Add("@CountrySubDivisionCodeField", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_150, SqlValue = item.BillAddr.CountrySubDivisionCode ?? string.Empty });
                //    sqlParaKeys.Add("@PostalCodeField", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_150, SqlValue = item.BillAddr.PostalCode ?? string.Empty });
                //    sqlParaKeys.Add("@CountryField", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_150, SqlValue = item.BillAddr.Country ?? string.Empty });
                //}
                //else
                {
                    sqlParaKeys.Add("@CityField", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_150, SqlValue = string.Empty });
                    sqlParaKeys.Add("@CountrySubDivisionCodeField", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_150, SqlValue = string.Empty });
                    sqlParaKeys.Add("@PostalCodeField", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_150, SqlValue = string.Empty });
                    sqlParaKeys.Add("@CountryField", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_150, SqlValue = string.Empty });
                }


                sqlParaKeys.Add("@PrimaryPhone", new SqlParameter()
                {
                    SqlDbType = SqlDbType.VarChar,
                    Size = SIZE_VARCHAR_20,
                    SqlValue = string.Empty
                });

                sqlParaKeys.Add("@PrimaryEmailAddr", new SqlParameter()
                {
                    SqlDbType = SqlDbType.VarChar,
                    Size = SIZE_VARCHAR_100,
                    SqlValue = string.Empty
                });

                sqlParaKeys.Add("@TaxIdentifier", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_20, SqlValue = string.Empty });


                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspSaveXeroVendor]", ref sql);

                 ExecuteSqlCommand(sql, sqlParameters);


                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(GetDBResponseStatusCode(CSConvert.ToString(sqlParaDesc.Value)), CSConvert.ToString(sqlParaDesc.Value));

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(DBResponseStatusCode.FAILED, ex.Message);
            }
        }

        object obj3 = new object();
        internal CosDBResponse<DBResponseStatusCode> SaveXeroVendor(Contact item)
        {
            lock(obj3)
            {
                try
                {
                    
                    SqlParameter sqlParaNum = GetOutputErrNum();
                    SqlParameter sqlParaDesc = GetOutputErrDesc();

                    string sql = string.Empty;
                    Dictionary<string, SqlParameter> _sqlParaKeys = new Dictionary<string, SqlParameter>();
                    //_sqlParaKeys.Clear();

                    _sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.AccountID });
                    _sqlParaKeys.Add("@QboConnectID", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_INT, SqlValue = _sessionHelper.XeroConnectID });
                    _sqlParaKeys.Add("@QboVendorID", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = 50, SqlValue = Convert.ToString(item.ContactID) });

                    _sqlParaKeys.Add("@DisplayNameField", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_150, SqlValue = item.Name ?? string.Empty });

                    {
                        _sqlParaKeys.Add("@CityField", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_150, SqlValue = string.Empty });
                        _sqlParaKeys.Add("@CountrySubDivisionCodeField", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_150, SqlValue = string.Empty });
                        _sqlParaKeys.Add("@PostalCodeField", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_150, SqlValue = string.Empty });
                        _sqlParaKeys.Add("@CountryField", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_150, SqlValue = string.Empty });
                    }


                    _sqlParaKeys.Add("@PrimaryPhone", new SqlParameter()
                    {
                        SqlDbType = SqlDbType.VarChar,
                        Size = SIZE_VARCHAR_20,
                        SqlValue = (item.Phones != null ? (item.ContactNumber ?? string.Empty) : string.Empty)
                    });

                    _sqlParaKeys.Add("@PrimaryEmailAddr", new SqlParameter()
                    {
                        SqlDbType = SqlDbType.VarChar,
                        Size = SIZE_VARCHAR_100,
                        SqlValue = (item.EmailAddress != null ? (item.EmailAddress ?? string.Empty) : string.Empty)
                    });

                    _sqlParaKeys.Add("@TaxIdentifier", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_20, SqlValue = item.TaxNumber ?? string.Empty });


                    _sqlParaKeys.Add("@errNum", sqlParaNum);
                    _sqlParaKeys.Add("@errDesc", sqlParaDesc);

                    SqlParameter[] sqlParameters = PrepareSql(_sqlParaKeys, "[uspSaveXeroVendor]", ref sql);

                     ExecuteSqlCommand(sql, sqlParameters);

                    return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(GetDBResponseStatusCode(CSConvert.ToString(sqlParaDesc.Value)), CSConvert.ToString(sqlParaDesc.Value));

                }
                catch (Exception ex)
                {
                    Logger log = CosmicLogger.SetLog();
                    log.Error(ex);
                    return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(DBResponseStatusCode.FAILED, ex.Message);
                }
            }
           
        }

        internal CosDBResponse<DBResponseStatusCode> SaveQboTax(ICollection<TaxCode> lstQboTax)
        {

            try
            {
                foreach (var item in lstQboTax)
                {
                    SaveQboTax(item);
                }

                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(GetDBResponseStatusCode(string.Empty), CSConvert.ToString(string.Empty));

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(DBResponseStatusCode.FAILED, ex.Message);
            }
        }

        internal CosDBResponse<DBResponseStatusCode> SaveAllQboVendor(ICollection<Vendor> lstQboVendor)
        {

            try
            {
                foreach (var item in lstQboVendor)
                {
                    SaveQboVendor(item);
                }

                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(GetDBResponseStatusCode(string.Empty), CSConvert.ToString(string.Empty));

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(DBResponseStatusCode.FAILED, ex.Message);
            }
        }

        internal CosDBResponse<DBResponseStatusCode> SaveAllXeroVendor(List<Contact> lstQboVendor)
        {

            try
            {
                foreach (var item in lstQboVendor)
                {
                    SaveXeroVendor(item);
                }

                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(GetDBResponseStatusCode(string.Empty), CSConvert.ToString(string.Empty));

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(DBResponseStatusCode.FAILED, ex.Message);
            }
        }

        internal CosDBResponse<DBResponseStatusCode> SaveAllQboVendAcctDefault(ICollection<QboVendor> lstQboVendAcctDefault)
        {

            try
            {
                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                foreach (var item in lstQboVendAcctDefault)
                {
                    string sql = string.Empty;

                    sqlParaKeys.Clear();

                    sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.AccountID });
                    sqlParaKeys.Add("@QboConnectID", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_INT, SqlValue = _sessionHelper.QboConnectID });
                    sqlParaKeys.Add("@QboVendorID", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_20, SqlValue = item.QboVendorID ?? string.Empty });
                    sqlParaKeys.Add("@QboAccountID", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_20, SqlValue = item.QboAccountID ?? string.Empty });


                    sqlParaKeys.Add("@errNum", sqlParaNum);
                    sqlParaKeys.Add("@errDesc", sqlParaDesc);

                    SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspUpdateQboVendAcct]",
                                          ref sql);

                     ExecuteSqlCommand(sql, sqlParameters);
                }

                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(GetDBResponseStatusCode(CSConvert.ToString(sqlParaDesc.Value)), CSConvert.ToString(sqlParaDesc.Value));

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(DBResponseStatusCode.FAILED, ex.Message);
            }
        }



        internal CosDBResponse<DBResponseStatusCode> SaveAllXeroVendAcctDefault(ICollection<XeroVendor> lstQboVendAcctDefault)
        {

            try
            {
                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                foreach (var item in lstQboVendAcctDefault)
                {
                    string sql = string.Empty;

                    sqlParaKeys.Clear();

                    sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.AccountID });
                    sqlParaKeys.Add("@QboConnectID", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_INT, SqlValue = _sessionHelper.XeroConnectID });
                    sqlParaKeys.Add("@QboVendorID", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = 50, SqlValue = item.XeroVendorID ?? string.Empty });
                    sqlParaKeys.Add("@QboAccountID", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = 50, SqlValue = item.XeroAccountID ?? string.Empty });


                    sqlParaKeys.Add("@errNum", sqlParaNum);
                    sqlParaKeys.Add("@errDesc", sqlParaDesc);

                    SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspUpdateXeroVendAcct]",
                                          ref sql);

                     ExecuteSqlCommand(sql, sqlParameters);
                }

                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(GetDBResponseStatusCode(CSConvert.ToString(sqlParaDesc.Value)), CSConvert.ToString(sqlParaDesc.Value));

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(DBResponseStatusCode.FAILED, ex.Message);
            }
        }

        internal CosDBResponse<DBResponseStatusCode> SaveQboVendAcctDefault(QboVendor qboVendAcctDefault)
        {

            try
            {
                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();


                string sql = string.Empty;

                sqlParaKeys.Clear();

                sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.AccountID });
                sqlParaKeys.Add("@QboConnectID", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_INT, SqlValue = _sessionHelper.QboConnectID });
                sqlParaKeys.Add("@QboVendorID", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_20, SqlValue = qboVendAcctDefault.QboVendorID ?? string.Empty });
                sqlParaKeys.Add("@QboAccountID", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_20, SqlValue = qboVendAcctDefault.QboAccountID ?? string.Empty });


                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspUpdateQboVendAcct]",
                                      ref sql);

                 ExecuteSqlCommand(sql, sqlParameters);


                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(GetDBResponseStatusCode(CSConvert.ToString(sqlParaDesc.Value)), CSConvert.ToString(sqlParaDesc.Value));

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(DBResponseStatusCode.FAILED, ex.Message);
            }
        }


        internal CosDBResponse<DBResponseStatusCode> SaveXeroVendAcctDefault(XeroVendor qboVendAcctDefault)
        {

            try
            {
                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();


                string sql = string.Empty;

                sqlParaKeys.Clear();

                sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.AccountID });
                sqlParaKeys.Add("@QboConnectID", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_INT, SqlValue = _sessionHelper.XeroConnectID });
                sqlParaKeys.Add("@QboVendorID", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_100, SqlValue = qboVendAcctDefault.XeroVendorID ?? string.Empty });
                sqlParaKeys.Add("@QboAccountID", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_100, SqlValue = qboVendAcctDefault.XeroAccountID ?? string.Empty });

                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspUpdateXeroVendAcct]",
                                      ref sql);

                 ExecuteSqlCommand(sql, sqlParameters);


                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(GetDBResponseStatusCode(CSConvert.ToString(sqlParaDesc.Value)), CSConvert.ToString(sqlParaDesc.Value));

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(DBResponseStatusCode.FAILED, ex.Message);
            }
        }


        internal CosDBResponse<DBResponseStatusCode> DeleteQboDocument(QboDocument qboDocument)
        {

            try
            {
                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();


                string sql = string.Empty;

                sqlParaKeys.Clear();

                sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.AccountID });
                sqlParaKeys.Add("@QboConnectID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.QboConnectID });
                sqlParaKeys.Add("@DocumentID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = qboDocument.DocumentID });
                sqlParaKeys.Add("@Deleted", new SqlParameter() { SqlDbType = SqlDbType.Bit, Size = SIZE_BIT, SqlValue = qboDocument.Deleted ?? false });


                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspDeleteQboDocument]",
                                      ref sql);

                 ExecuteSqlCommand(sql, sqlParameters);


                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(GetDBResponseStatusCode(CSConvert.ToString(sqlParaDesc.Value)), CSConvert.ToString(sqlParaDesc.Value));

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(DBResponseStatusCode.FAILED, ex.Message);
            }
        }
        internal CosDBResponse<ICollection<XeroDocument>> UpdateScanTotal(XeroDocument qboDocument)
        {

            try
            {

                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                string sql = string.Empty;

                sqlParaKeys.Clear();

                sqlParaKeys.Add("@DocumentID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = qboDocument.DocumentID });
                sqlParaKeys.Add("@ScanInvoiceTotal", new SqlParameter() { SqlDbType = SqlDbType.Decimal, Size = SIZE_DECIMAL, SqlValue = qboDocument.ScanInvoiceTotal ?? 0 });

                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspUpdateScanTotal]",
                                         ref sql);

                var data =  SqlQuery<XeroDocument>(sql, sqlParameters).ToList();

                return CosDBResponse<ICollection<XeroDocument>>.CreateDBResponse(data, FormatSqlErrorDescParam(sqlParaDesc));
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<ICollection<XeroDocument>>.CreateDBResponse(null, ex.Message);
            }
        }

        internal CosDBResponse<DBResponseStatusCode> DeleteXeroDocument(XeroDocument qboDocument)
        {

            try
            {
                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();


                string sql = string.Empty;

                sqlParaKeys.Clear();

                sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.AccountID });
                sqlParaKeys.Add("@QboConnectID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.XeroConnectID });
                sqlParaKeys.Add("@DocumentID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = qboDocument.DocumentID });
                sqlParaKeys.Add("@Deleted", new SqlParameter() { SqlDbType = SqlDbType.Bit, Size = SIZE_BIT, SqlValue = qboDocument.Deleted ?? false });


                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspDeleteXeroDocument]",
                                      ref sql);

                 ExecuteSqlCommand(sql, sqlParameters);


                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(GetDBResponseStatusCode(CSConvert.ToString(sqlParaDesc.Value)), CSConvert.ToString(sqlParaDesc.Value));

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(DBResponseStatusCode.FAILED, ex.Message);
            }
        }

        internal CosDBResponse<DBResponseStatusCode> DeleteXeroDocumentLine(XeroDocumentLine qboDocument)
        {

            try
            {
                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();


                string sql = string.Empty;

                sqlParaKeys.Clear();

                sqlParaKeys.Add("@DocumentLineID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = qboDocument.DocumentLineID });


                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspDeleteXeroDocumentLine]",
                                      ref sql);

                 ExecuteSqlCommand(sql, sqlParameters);


                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(GetDBResponseStatusCode(CSConvert.ToString(sqlParaDesc.Value)), CSConvert.ToString(sqlParaDesc.Value));

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(DBResponseStatusCode.FAILED, ex.Message);
            }
        }

        //

        //internal CosDBResponse<int> SaveQboDocument(QboDocument qboDocument)
        //{
        //    try
        //    {

        //        SqlParameter sqlParaNum = GetOutputErrNum();
        //        SqlParameter sqlParaDesc = GetOutputErrDesc();
        //        SqlParameter sqlParaIdentity = GetOutputIdentity();
        //        string sql = string.Empty;

        //        sqlParaKeys.Clear();
        //        sqlParaKeys.Add("@QboDocumentID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = qboDocument.QboDocumentID });
        //        sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.AccountID });
        //        sqlParaKeys.Add("@QboConnectID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.QboConnectID });  //SessionHelper.QboConnectID
        //        sqlParaKeys.Add("@ScanInvoiceID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = qboDocument.ScanInvoiceID });
        //        sqlParaKeys.Add("@QboInvoiceID", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_50, SqlValue = qboDocument.QboInvoiceID ?? string.Empty });
        //        sqlParaKeys.Add("@ScanPdfPath", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_MAX, SqlValue = qboDocument.ScanPdfPath ?? string.Empty });
        //        sqlParaKeys.Add("@DocumentName", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_MAX, SqlValue = qboDocument.DocumentName ?? string.Empty });

        //        sqlParaKeys.Add("@InvoiceNumber", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_50, SqlValue = qboDocument.InvoiceNumber ?? string.Empty });
        //        sqlParaKeys.Add("@InvoiceType", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_20, SqlValue = qboDocument.InvoiceType ?? string.Empty });

        //        if (qboDocument.InvoiceDate == null)
        //            sqlParaKeys.Add("@InvoiceDate", new SqlParameter() { SqlDbType = SqlDbType.DateTime, Size = SIZE_DATETIME, SqlValue = DBNull.Value });
        //        else
        //            sqlParaKeys.Add("@InvoiceDate", new SqlParameter() { SqlDbType = SqlDbType.DateTime, Size = SIZE_DATETIME, SqlValue = qboDocument.InvoiceDate });

        //        sqlParaKeys.Add("@VendorName", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_150, SqlValue = qboDocument.VendorName ?? string.Empty });
        //        sqlParaKeys.Add("@InvoiceTotal", new SqlParameter() { SqlDbType = SqlDbType.Decimal, Size = SIZE_DECIMAL, SqlValue = qboDocument.InvoiceTotal ?? 0 });
        //        sqlParaKeys.Add("@TaxTotal", new SqlParameter() { SqlDbType = SqlDbType.Decimal, Size = SIZE_DECIMAL, SqlValue = qboDocument.TaxTotal ?? 0 });

        //        sqlParaKeys.Add("@QboError", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_MAX, SqlValue = qboDocument.QboError ?? string.Empty });
        //        sqlParaKeys.Add("@AspSessionID", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_MAX, SqlValue = qboDocument.AspSessionID ?? string.Empty });

        //        sqlParaKeys.Add("@errNum", sqlParaNum);
        //        sqlParaKeys.Add("@errDesc",sqlParaDesc);
        //        sqlParaKeys.Add("@identity", sqlParaIdentity);

        //        SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspSaveQboDocument]",
        //                                 ref sql);


        //         ExecuteSqlCommand(sql, sqlParameters);

        //        return CosDBResponse<int>.CreateDBResponse((CSConvert.ToInt(sqlParaIdentity.Value)), CSConvert.ToString(sqlParaDesc.Value));

        //    }
        //    catch (Exception ex)
        //    {
        //        return CosDBResponse<int>.CreateDBResponse(0, ex.Message);

        //    }
        //}

        #endregion

        #region ReckonEzzyAccount

        internal CosDBResponse<ReckonEzzyAccount> GetRCScanningCredential()
        {
            try
            {

                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                string sql = string.Empty;

                sqlParaKeys.Clear();

                sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.AccountID });
                sqlParaKeys.Add("@ReckonFileID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.ReckonFileID });
                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspGetRCScanningLogin]", ref sql);

                var data =  SqlQuery<ReckonEzzyAccount>(sql, sqlParameters).FirstOrDefault();

                return CosDBResponse<ReckonEzzyAccount>.CreateDBResponse(data, FormatSqlErrorDescParam(sqlParaDesc));

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<ReckonEzzyAccount>.CreateDBResponse(null, ex.Message);
            }
        }

        internal CosDBResponse<QBOEzzyAccount> GetQBOScanningCredential()
        {
            try
            {

                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                string sql = string.Empty;

                sqlParaKeys.Clear();

                sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.AccountID });
                sqlParaKeys.Add("@QboConnectID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.QboConnectID }); // SessionHelper.QboConnectID 
                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspGetQBOScanningLogin]", ref sql);

                var data =  SqlQuery<QBOEzzyAccount>(sql, sqlParameters).FirstOrDefault();

                return CosDBResponse<QBOEzzyAccount>.CreateDBResponse(data, FormatSqlErrorDescParam(sqlParaDesc));
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<QBOEzzyAccount>.CreateDBResponse(null, ex.Message);
            }
        }


        object objGetXeroScanningCredential = new object();
        internal CosDBResponse<XeroEzzyAccount> GetXeroScanningCredential()
        {
            lock(objGetXeroScanningCredential)
            {
                try
                {

                    SqlParameter sqlParaNum = GetOutputErrNum();
                    SqlParameter sqlParaDesc = GetOutputErrDesc();

                    string sql = string.Empty;

                    Dictionary<string, SqlParameter> s_sqlParaKeys = new Dictionary<string, SqlParameter>();

                    s_sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.AccountID });
                    //  sqlParaKeys.Add("@QboConnectID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.XeroConnectID }); // SessionHelper.QboConnectID 
                    s_sqlParaKeys.Add("@errNum", sqlParaNum);
                    s_sqlParaKeys.Add("@errDesc", sqlParaDesc);

                    SqlParameter[] sqlParameters = PrepareSql(s_sqlParaKeys, "[uspGetXeroScanningLogin]", ref sql);

                    var data =  SqlQuery<XeroEzzyAccount>(sql, sqlParameters).FirstOrDefault();

                    return CosDBResponse<XeroEzzyAccount>.CreateDBResponse(data, FormatSqlErrorDescParam(sqlParaDesc));
                }
                catch (Exception ex)
                {
                    Logger log = CosmicLogger.SetLog();
                    log.Error(ex);
                    return CosDBResponse<XeroEzzyAccount>.CreateDBResponse(null, ex.Message);
                }
            }
         
        }

        internal int GetMaxEzzyReckonAccountID()
        {
            try
            {

                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                string sql = string.Empty;

                sqlParaKeys.Clear();

                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspGetMaxEzzyAccountID]", ref sql);

                var data =  SqlQuery<ReckonEzzyAccount>(sql, sqlParameters).FirstOrDefault();

                if (data != null)
                {
                    return data.RCEzzyID ?? 0;
                }

                return -1;
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return -1;
            }
        }

        internal int GetMaxEzzyQboAccountID()
        {
            try
            {

                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                string sql = string.Empty;

                sqlParaKeys.Clear();

                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspGetMaxEzzyQboAccountID]", ref sql);

                var data =  SqlQuery<QBOEzzyAccount>(sql, sqlParameters).FirstOrDefault();

                if (data != null)
                {
                    return data.QBOEzzyID ?? 0;
                }

                return -1;
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return -1;
            }
        }

        internal int GetMaxEzzyXeroAccountID()
        {
            try
            {

                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                string sql = string.Empty;

                sqlParaKeys.Clear();

                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspGetMaxEzzyXeroAccountID]", ref sql);

                var data =  SqlQuery<XeroEzzyAccount>(sql, sqlParameters).FirstOrDefault();

                if (data != null)
                {
                    return data.XeroEzzyID ?? 0;
                }

                return -1;
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return -1;
            }
        }

        internal CosDBResponse<DBResponseStatusCode> SaveReckonEzzyAccount(ReckonEzzyAccount reckonEzzyAccount)
        {
            try
            {

                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                string sql = string.Empty;

                sqlParaKeys.Clear();
                sqlParaKeys.Add("@RCEzzyID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = reckonEzzyAccount.RCEzzyID });
                sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.AccountID });
                sqlParaKeys.Add("@ReckonFileID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = reckonEzzyAccount.ReckonFileID ?? 0 });
                sqlParaKeys.Add("@EzzyEmailAddress", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_50, SqlValue = reckonEzzyAccount.EzzyEmailAddress ?? string.Empty });
                sqlParaKeys.Add("@EzzyUserName", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_20, SqlValue = reckonEzzyAccount.EzzyUserName ?? string.Empty });
                sqlParaKeys.Add("@EzzyPassword", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_30, SqlValue = reckonEzzyAccount.EzzyPassword ?? string.Empty });
                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspSaveReckonEzzyAccount]", ref sql);

                 ExecuteSqlCommand(sql, sqlParameters);

                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(GetDBResponseStatusCode(CSConvert.ToString(sqlParaDesc.Value)), CSConvert.ToString(sqlParaDesc.Value));

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(DBResponseStatusCode.FAILED, ex.Message);

            }
        }
        #endregion ReckonEzzyAccount

        #region QboEzzyAccount

        internal CosDBResponse<DBResponseStatusCode> SaveQboEzzyAccount(QBOEzzyAccount qBOEzzyAccount)
        {
            try
            {

                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                string sql = string.Empty;

                sqlParaKeys.Clear();
                sqlParaKeys.Add("@QboEzzyID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = qBOEzzyAccount.QBOEzzyID });
                sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.AccountID });
                sqlParaKeys.Add("@QboConnectID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = qBOEzzyAccount.QboConnectID ?? 0 });
                sqlParaKeys.Add("@EzzyUserName", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_50, SqlValue = qBOEzzyAccount.EzzyUserName ?? string.Empty });
                sqlParaKeys.Add("@EzzyPassword", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_30, SqlValue = qBOEzzyAccount.EzzyPassword ?? string.Empty });
                sqlParaKeys.Add("@EzzyEmailAddress", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_100, SqlValue = qBOEzzyAccount.EzzyEmailAddress ?? string.Empty });
                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspSaveQBOEzzyAccount]",
                                         ref sql);

                 ExecuteSqlCommand(sql, sqlParameters);

                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(GetDBResponseStatusCode(CSConvert.ToString(sqlParaDesc.Value)), CSConvert.ToString(sqlParaDesc.Value));

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(DBResponseStatusCode.FAILED, ex.Message);

            }
        }
        #endregion QboEzzyAccount

        #region XeroEzzyAccount

        internal CosDBResponse<DBResponseStatusCode> SaveXeroEzzyAccount(XeroEzzyAccount xeroEzzyAccount)
        {
            try
            {

                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                string sql = string.Empty;

                sqlParaKeys.Clear();
                sqlParaKeys.Add("@XeroEzzyID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = xeroEzzyAccount.XeroEzzyID });
                sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.AccountID });
                sqlParaKeys.Add("@XeroConnectID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = xeroEzzyAccount.XeroConnectID ?? 0 });
                sqlParaKeys.Add("@EzzyUserName", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_50, SqlValue = xeroEzzyAccount.EzzyUserName ?? string.Empty });
                sqlParaKeys.Add("@EzzyPassword", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_30, SqlValue = xeroEzzyAccount.EzzyPassword ?? string.Empty });
                sqlParaKeys.Add("@EzzyEmailAddress", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_100, SqlValue = xeroEzzyAccount.EzzyEmailAddress ?? string.Empty });
                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspSaveXeroEzzyAccount]",
                                         ref sql);

                 ExecuteSqlCommand(sql, sqlParameters);

                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(GetDBResponseStatusCode(CSConvert.ToString(sqlParaDesc.Value)), CSConvert.ToString(sqlParaDesc.Value));

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(DBResponseStatusCode.FAILED, ex.Message);

            }
        }

        #endregion XeroEzzyAccount

        #region Reconk Document
        internal CosDBResponse<int> SaveReckonDocument(ReckonDocument reckonDocument)
        {
            try
            {

                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();
                SqlParameter sqlParaIdentity = GetOutputIdentity();
                string sql = string.Empty;

                sqlParaKeys.Clear();
                sqlParaKeys.Add("@ReckonDocumentID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = reckonDocument.ReckonDocumentID });
                sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.AccountID });
                sqlParaKeys.Add("@ReckonFileID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.ReckonFileID });
                sqlParaKeys.Add("@ScanInvoiceID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = reckonDocument.ScanInvoiceID });
                sqlParaKeys.Add("@TxnID", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_50, SqlValue = reckonDocument.TxnID ?? string.Empty });
                sqlParaKeys.Add("@ScanPdfPath", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_MAX, SqlValue = reckonDocument.ScanPdfPath ?? string.Empty });
                sqlParaKeys.Add("@InvoiceNumber", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_50, SqlValue = reckonDocument.InvoiceNumber ?? string.Empty });
                sqlParaKeys.Add("@InvoiceType", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_20, SqlValue = reckonDocument.InvoiceType ?? string.Empty });

                if (reckonDocument.InvoiceDate == null)
                    sqlParaKeys.Add("@InvoiceDate", new SqlParameter() { SqlDbType = SqlDbType.DateTime, Size = SIZE_DATETIME, SqlValue = DBNull.Value });
                else
                    sqlParaKeys.Add("@InvoiceDate", new SqlParameter() { SqlDbType = SqlDbType.DateTime, Size = SIZE_DATETIME, SqlValue = reckonDocument.InvoiceDate });

                sqlParaKeys.Add("@VendorName", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_150, SqlValue = reckonDocument.VendorName ?? string.Empty });
                sqlParaKeys.Add("@InvoiceTotal", new SqlParameter() { SqlDbType = SqlDbType.Decimal, Size = SIZE_DECIMAL, SqlValue = reckonDocument.InvoiceTotal ?? 0 });
                sqlParaKeys.Add("@TaxTotal", new SqlParameter() { SqlDbType = SqlDbType.Decimal, Size = SIZE_DECIMAL, SqlValue = reckonDocument.TaxTotal ?? 0 });
                sqlParaKeys.Add("@UploadedDate", new SqlParameter() { SqlDbType = SqlDbType.DateTime, Size = SIZE_DATETIME, SqlValue = reckonDocument.UploadedDate ?? DateTime.Now });

                sqlParaKeys.Add("@ReckonError", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_MAX, SqlValue = reckonDocument.ReckonError ?? string.Empty });

                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);
                sqlParaKeys.Add("@Identity", sqlParaIdentity);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspSaveReckonDocument]",
                                         ref sql);


                 ExecuteSqlCommand(sql, sqlParameters);

                return CosDBResponse<int>.CreateDBResponse((CSConvert.ToInt(sqlParaIdentity.Value)), CSConvert.ToString(sqlParaDesc.Value));

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<int>.CreateDBResponse(0, ex.Message);
            }
        }

        #endregion

        #region Stripe 
        internal CosDBResponse<ICollection<StripePayment>> GetPayment(int accountID)
        {
            try
            {
                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();
                string sql = string.Empty;

                sqlParaKeys.Clear();
                sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = accountID });
                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);
                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspGetPaymentByAccountID]",
                                         ref sql);

                var data =  SqlQuery<StripePayment>(sql, sqlParameters).ToList();
                return CosDBResponse<ICollection<StripePayment>>.CreateDBResponse(data, FormatSqlErrorDescParam(sqlParaDesc));

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<ICollection<StripePayment>>.CreateDBResponse(null, ex.Message);
            }

        }
        #endregion

        #region ManageAccount
        internal CosDBResponse<ICollection<AccountMaster>> GetAccountMasterByPlatformAndPlan(int platform, int plan)
        {
            try
            {
                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                string sql = string.Empty;

                sqlParaKeys.Clear();
                sqlParaKeys.Add("@Platform", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = platform });
                sqlParaKeys.Add("@Plan", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = plan });
                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspAdminpGetAccountMasterByPlatformAndPlan]", ref sql);

                var data =  SqlQuery<AccountMaster>(sql, sqlParameters).ToList();
                return CosDBResponse<ICollection<AccountMaster>>.CreateDBResponse(data, FormatSqlErrorDescParam(sqlParaDesc));
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<ICollection<AccountMaster>>.CreateDBResponse(null, ex.Message);
            }

        }

        internal CosDBResponse<ICollection<LoginMaster>> GetLoginMaster(int accountID)
        {
            try
            {
                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                string sql = string.Empty;

                sqlParaKeys.Clear();
                sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = accountID });
                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspAdminpGetLoginMasterByAccountID]",
                                        ref sql);

                var data =  SqlQuery<LoginMaster>(sql, sqlParameters).ToList();
                return CosDBResponse<ICollection<LoginMaster>>.CreateDBResponse(data, FormatSqlErrorDescParam(sqlParaDesc));
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<ICollection<LoginMaster>>.CreateDBResponse(null, ex.Message);
            }

        }

        internal CosDBResponse<ICollection<ReckonDesktopMaster>> GetReckonFiles(int accountID, int plarformID)
        {
            try
            {
                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                string sql = string.Empty;

                sqlParaKeys.Clear();
                sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = accountID });
                sqlParaKeys.Add("@PlatformID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = plarformID });
                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);


                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspAdminpGetReckonMasterByAccountIDAndPlatform]", ref sql);

                var data =  SqlQuery<ReckonDesktopMaster>(sql, sqlParameters).ToList();
                return CosDBResponse<ICollection<ReckonDesktopMaster>>.CreateDBResponse(data, FormatSqlErrorDescParam(sqlParaDesc));
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<ICollection<ReckonDesktopMaster>>.CreateDBResponse(null, ex.Message);
            }
        }

        internal CosDBResponse<ICollection<AccountMaster>> GetAccountMaster()
        {
            try
            {
                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                string sql = string.Empty;

                sqlParaKeys.Clear();
                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);
                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspAdminpGetAccountMaster]", ref sql);

                var data =  SqlQuery<AccountMaster>(sql, sqlParameters).ToList();
                return CosDBResponse<ICollection<AccountMaster>>.CreateDBResponse(data, FormatSqlErrorDescParam(sqlParaDesc));
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<ICollection<AccountMaster>>.CreateDBResponse(null, ex.Message);
            }

        }

        internal CosDBResponse<ICollection<ReckonDesktopMaster>> GetReckonFilesByAccountId(int accountID)
        {
            try
            {
                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                string sql = string.Empty;

                sqlParaKeys.Clear();
                sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = accountID });
                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspAdminpGetReckonMasterByAccountID]", ref sql);

                var data =  SqlQuery<ReckonDesktopMaster>(sql, sqlParameters).ToList();
                return CosDBResponse<ICollection<ReckonDesktopMaster>>.CreateDBResponse(data, FormatSqlErrorDescParam(sqlParaDesc));
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<ICollection<ReckonDesktopMaster>>.CreateDBResponse(null, ex.Message);
            }
        }

        internal CosDBResponse<ICollection<ReckonDocument>> GetReckonDocumentByAccoundId(int accountID, int reckonfileID, int month, int year)
        {
            try
            {
                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                string sql = string.Empty;

                sqlParaKeys.Clear();
                sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = accountID });
                sqlParaKeys.Add("@ReckonFileID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = reckonfileID });
                sqlParaKeys.Add("@month", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = month });
                sqlParaKeys.Add("@Year", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = year });
                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspAdminGetReckonDocument]", ref sql);

                var data =  SqlQuery<ReckonDocument>(sql, sqlParameters).ToList();
                return CosDBResponse<ICollection<ReckonDocument>>.CreateDBResponse(data, FormatSqlErrorDescParam(sqlParaDesc));
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<ICollection<ReckonDocument>>.CreateDBResponse(null, ex.Message);
            }
        }

        internal CosDBResponse<ICollection<StripePayment>> GetStripePaymentByAccountID(int accountID, int planID)
        {
            try
            {
                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                string sql = string.Empty;

                sqlParaKeys.Clear();
                sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = accountID });
                sqlParaKeys.Add("@PlanID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = planID });
                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspAdminpGetStripePaymentByAccountID]", ref sql);

                var data =  SqlQuery<StripePayment>(sql, sqlParameters).ToList();
                return CosDBResponse<ICollection<StripePayment>>.CreateDBResponse(data, FormatSqlErrorDescParam(sqlParaDesc));
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<ICollection<StripePayment>>.CreateDBResponse(null, ex.Message);
            }

        }

        internal CosDBResponse<ICollection<StripePayment>> GetStripePayment(int month, int year)
        {
            try
            {
                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                string sql = string.Empty;

                sqlParaKeys.Clear();
                sqlParaKeys.Add("@month", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = month });
                sqlParaKeys.Add("@Year", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = year });
                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspAdminGetStripePayment]", ref sql);

                var data =  SqlQuery<StripePayment>(sql, sqlParameters).ToList();
                return CosDBResponse<ICollection<StripePayment>>.CreateDBResponse(data, FormatSqlErrorDescParam(sqlParaDesc));
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<ICollection<StripePayment>>.CreateDBResponse(null, ex.Message);
            }
        }

        internal CosDBResponse<DBResponseStatusCode> SavePlan(PlanMaster planMaster)
        {

            try
            {
                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                string sql = string.Empty;

                sqlParaKeys.Clear();

                sqlParaKeys.Add("@PlanID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = planMaster.PlanID });
                sqlParaKeys.Add("@PlanName", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_50, SqlValue = planMaster.PlanName });
                sqlParaKeys.Add("@PlanDescription", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_MAX, SqlValue = planMaster.PlanDescription });
                sqlParaKeys.Add("@TrialDays", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = planMaster.TrialDays });
                sqlParaKeys.Add("@TrialPdf", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = planMaster.TrialPdf });
                sqlParaKeys.Add("@PaidPdf", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = planMaster.PaidPdf });
                sqlParaKeys.Add("@RatePerYearGST", new SqlParameter() { SqlDbType = SqlDbType.Decimal, Size = SIZE_DECIMAL, SqlValue = planMaster.RatePerYearGST });
                sqlParaKeys.Add("@RatePerYearSubTotal", new SqlParameter() { SqlDbType = SqlDbType.Decimal, Size = SIZE_DECIMAL, SqlValue = planMaster.RatePerYearSubTotal });
                sqlParaKeys.Add("@RatePerYearTotal", new SqlParameter() { SqlDbType = SqlDbType.Decimal, Size = SIZE_DECIMAL, SqlValue = planMaster.RatePerYearTotal });
                sqlParaKeys.Add("@IsPaidPlan", new SqlParameter() { SqlDbType = SqlDbType.Bit, Size = SIZE_BIT, SqlValue = planMaster.IsPaidPlan });
                sqlParaKeys.Add("@IsActive", new SqlParameter() { SqlDbType = SqlDbType.Bit, Size = SIZE_BIT, SqlValue = planMaster.IsActive });
                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspAdminSavePlan]", ref sql);

                 ExecuteSqlCommand(sql, sqlParameters);

                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(GetDBResponseStatusCode(FormatSqlErrorDescParam(sqlParaDesc)), FormatSqlErrorDescParam(sqlParaDesc));
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(DBResponseStatusCode.FAILED, ex.Message);
            }
        }

        #endregion ManageAccount


        internal CosDBResponse<ICollection<QboJob>> InsertQboJob(string DocumentIDs)
        {

            try
            {

                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                string sql = string.Empty;

                sqlParaKeys.Clear();

                sqlParaKeys.Add("@DocumentIDs", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_MAX, SqlValue = DocumentIDs });
                sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.AccountID });

                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspInsertQboJob]",
                                         ref sql);

                var data =  SqlQuery<QboJob>(sql, sqlParameters).ToList();

                return CosDBResponse<ICollection<QboJob>>.CreateDBResponse(data, FormatSqlErrorDescParam(sqlParaDesc));
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<ICollection<QboJob>>.CreateDBResponse(null, ex.Message);
            }
        }

        internal CosDBResponse<int> InsertQboDocument(QboDocument qboDocument)
        {
            try
            {

                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();
                SqlParameter sqlParaIdentity = GetOutputIdentity();
                string sql = string.Empty;

                sqlParaKeys.Clear();
                sqlParaKeys.Add("@DocumentID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = qboDocument.DocumentID });
                sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_20, SqlValue = _sessionHelper.AccountID });
                sqlParaKeys.Add("@QboConnectID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.QboConnectID });  //SessionHelper.QboConnectID
                sqlParaKeys.Add("@QboInvoiceID", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_20, SqlValue = string.Empty });
                sqlParaKeys.Add("@QboVendorID", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_20, SqlValue = qboDocument.QboVendorID ?? string.Empty });
                sqlParaKeys.Add("@QboVendorName", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_MAX, SqlValue = qboDocument.QboVendorName ?? string.Empty });
                sqlParaKeys.Add("@QboInvoiceDate", new SqlParameter() { SqlDbType = SqlDbType.DateTime, Size = SIZE_DATETIME, SqlValue = DBNull.Value });

                sqlParaKeys.Add("@ScanInvoiceID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = qboDocument.ScanInvoiceID });
                sqlParaKeys.Add("@ScanFile_Name", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_150, SqlValue = qboDocument.ScanFile_Name ?? string.Empty });



                sqlParaKeys.Add("@ScanBlob_Url", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_MAX, SqlValue = qboDocument.ScanBlob_Url ?? string.Empty });
                sqlParaKeys.Add("@ScanABNNumber", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_30, SqlValue = qboDocument.ScanABNNumber ?? string.Empty });
                sqlParaKeys.Add("@ScanRefNumber", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_30, SqlValue = qboDocument.ScanRefNumber ?? string.Empty });

                sqlParaKeys.Add("@ScanDocType", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_30, SqlValue = qboDocument.ScanDocType ?? string.Empty });
                sqlParaKeys.Add("@ScanSubTotal", new SqlParameter() { SqlDbType = SqlDbType.Decimal, Size = SIZE_DECIMAL, SqlValue = qboDocument.ScanSubTotal ?? 0 });
                sqlParaKeys.Add("@ScanChargeTotal", new SqlParameter() { SqlDbType = SqlDbType.Decimal, Size = SIZE_DECIMAL, SqlValue = qboDocument.ScanChargeTotal ?? 0 });
                sqlParaKeys.Add("@ScanDocumentTotal", new SqlParameter() { SqlDbType = SqlDbType.Decimal, Size = SIZE_DECIMAL, SqlValue = qboDocument.ScanDocumentTotal ?? 0 });
                sqlParaKeys.Add("@ScanTag", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_30, SqlValue = qboDocument.ScanTag ?? string.Empty });

                if (qboDocument.ScanInvoiceDate == null)
                    sqlParaKeys.Add("@ScanInvoiceDate", new SqlParameter() { SqlDbType = SqlDbType.DateTime, Size = SIZE_DATETIME, SqlValue = DBNull.Value });
                else
                    sqlParaKeys.Add("@ScanInvoiceDate", new SqlParameter() { SqlDbType = SqlDbType.DateTime, Size = SIZE_DATETIME, SqlValue = qboDocument.ScanInvoiceDate });

                sqlParaKeys.Add("@ScanPurchaseOrder", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_30, SqlValue = qboDocument.ScanPurchaseOrder ?? string.Empty });
                sqlParaKeys.Add("@ScanVendorName", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_150, SqlValue = qboDocument.ScanVendorName ?? string.Empty });
                sqlParaKeys.Add("@ScanInvoiceTotal", new SqlParameter() { SqlDbType = SqlDbType.Decimal, Size = SIZE_DECIMAL, SqlValue = qboDocument.ScanInvoiceTotal ?? 0 });
                sqlParaKeys.Add("@ScanTaxTotal", new SqlParameter() { SqlDbType = SqlDbType.Decimal, Size = SIZE_DECIMAL, SqlValue = qboDocument.ScanTaxTotal ?? 0 });
                sqlParaKeys.Add("@ScanDocClassification", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_50, SqlValue = qboDocument.ScanDocClassification ?? string.Empty });



                sqlParaKeys.Add("@ScanServiceStatus", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_MAX, SqlValue = qboDocument.ScanServiceStatus ?? string.Empty });
                sqlParaKeys.Add("@ScanServiceMessage", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_MAX, SqlValue = qboDocument.ScanServiceMessage ?? string.Empty });

                sqlParaKeys.Add("@QboError", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_MAX, SqlValue = qboDocument.QBOError ?? string.Empty });
                sqlParaKeys.Add("@AspSessionID", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_MAX, SqlValue = qboDocument.AspSessionID ?? string.Empty });
                sqlParaKeys.Add("@Approved", new SqlParameter() { SqlDbType = SqlDbType.Bit, Size = SIZE_BIT, SqlValue = 0 });

                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);
                sqlParaKeys.Add("@identity", sqlParaIdentity);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspInsertQboDocument]", ref sql);

                 ExecuteSqlCommand(sql, sqlParameters);

                return CosDBResponse<int>.CreateDBResponse((CSConvert.ToInt(sqlParaIdentity.Value)), CSConvert.ToString(sqlParaDesc.Value));

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<int>.CreateDBResponse(0, ex.Message);

            }
        }

        internal CosDBResponse<int> InsertQboDocumentLine(List<QboDocumentLine> lstQboDocument, int qboDocumentID)
        {
            try
            {
                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();
                SqlParameter sqlParaIdentity = GetOutputIdentity();

                foreach (var qboDocument in lstQboDocument)
                {
                    sqlParaNum = GetOutputErrNum();
                    sqlParaDesc = GetOutputErrDesc();
                    sqlParaIdentity = GetOutputIdentity();
                    string sql = string.Empty;

                    sqlParaKeys.Clear();
                    sqlParaKeys.Add("@DocumentID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = qboDocumentID });
                    sqlParaKeys.Add("@QboAccountID", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_20, SqlValue = qboDocument.QboAccountID ?? string.Empty });
                    sqlParaKeys.Add("@QboAccountName", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_150, SqlValue = qboDocument.QboAccountName ?? string.Empty });
                    sqlParaKeys.Add("@QboClassID", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_20, SqlValue = qboDocument.QboClassID ?? string.Empty });
                    sqlParaKeys.Add("@QboClassName", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_150, SqlValue = qboDocument.QboClassName ?? string.Empty });
                    sqlParaKeys.Add("@QboJobID", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_20, SqlValue = qboDocument.QboJobID ?? string.Empty });
                    sqlParaKeys.Add("@QboJobName", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_150, SqlValue = qboDocument.QboJobName ?? string.Empty });
                    sqlParaKeys.Add("@ScanDescription", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_MAX, SqlValue = qboDocument.ScanDescription ?? string.Empty });
                    sqlParaKeys.Add("@ScanArticle_Code", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_MAX, SqlValue = qboDocument.ScanArticle_Code ?? string.Empty });
                    sqlParaKeys.Add("@ScanGL_Code", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_INT, SqlValue = qboDocument.ScanGL_Code ?? string.Empty });
                    sqlParaKeys.Add("@ScanGST", new SqlParameter() { SqlDbType = SqlDbType.Decimal, Size = SIZE_DECIMAL, SqlValue = qboDocument.ScanGST ?? 0 });
                    sqlParaKeys.Add("@ScanTax_Code", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_50, SqlValue = qboDocument.ScanTax_Code ?? string.Empty });
                    sqlParaKeys.Add("@ScanTracking", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_50, SqlValue = qboDocument.ScanTracking ?? string.Empty });
                    sqlParaKeys.Add("@ScanUnit_Measure", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_50, SqlValue = qboDocument.ScanUnit_Measure ?? string.Empty });
                    sqlParaKeys.Add("@ScanUnit_Price", new SqlParameter() { SqlDbType = SqlDbType.Decimal, Size = SIZE_DECIMAL, SqlValue = qboDocument.ScanUnit_Price ?? 0 });

                    sqlParaKeys.Add("@Scan_Quantity", new SqlParameter() { SqlDbType = SqlDbType.Decimal, Size = SIZE_DECIMAL, SqlValue = qboDocument.Scan_Quantity ?? 1 });
                    sqlParaKeys.Add("@Scan_Total", new SqlParameter() { SqlDbType = SqlDbType.Decimal, Size = SIZE_DECIMAL, SqlValue = qboDocument.Scan_Total ?? 0 });


                    sqlParaKeys.Add("@errNum", sqlParaNum);
                    sqlParaKeys.Add("@errDesc", sqlParaDesc);
                    sqlParaKeys.Add("@identity", sqlParaIdentity);

                    SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspInsertQboDocumentLine]", ref sql);

                     ExecuteSqlCommand(sql, sqlParameters);
                }

                return CosDBResponse<int>.CreateDBResponse((CSConvert.ToInt(sqlParaIdentity.Value)), CSConvert.ToString(sqlParaDesc.Value));

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<int>.CreateDBResponse(0, ex.Message);

            }
        }

        object objSaveXeroInvoiceProccessStatus = new object();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="invoiceId"></param>
        /// <param name="status">1 means proccess completed</param>
        internal void SaveXeroInvoiceProccessStatus(string invoiceId, int status)
        {
            lock (objSaveXeroInvoiceProccessStatus)
            {
                try
                {
                    var sql = "insert into dbo.XeroInvoiceProccessStatus(InvoiceID,proccessStatus) values({0},{1})";
                    var res = ExecuteSqlCommand(sql, invoiceId, status);

                }
                catch (Exception ex)
                {
                    Logger log = CosmicLogger.SetLog();
                    log.Error(ex);

                }
            }
        }

        object objGetXeroInvoiceProccessStatus = new object();
        internal InvoiceStatus GetXeroInvoiceProccessStatus(string invoiceId)
        {
            lock (objGetXeroInvoiceProccessStatus)
            {
                try
                {
                    var sql = "select * from dbo.XeroInvoiceProccessStatus where InvoiceID ={0}";
                    var res = SqlQuery<InvoiceStatus>(sql, invoiceId);
                    return res.FirstOrDefault();
                }
                catch (Exception ex)
                {
                    Logger log = CosmicLogger.SetLog();
                    log.Error(ex);
                    return null;
                }
            }
        }

        object objInsertXeroDocument = new object();
        internal CosDBResponse<int> InsertXeroDocument(XeroDocument qboDocument)
        {
            lock(objInsertXeroDocument)
            {
              //  var transaction =  BeginTransaction();
                try
                {

                    SqlParameter sqlParaNum = GetOutputErrNum();
                    SqlParameter sqlParaDesc = GetOutputErrDesc();
                    SqlParameter sqlParaIdentity = GetOutputIdentity();
                    string sql = string.Empty;

                    Dictionary<string, SqlParameter> m_sqlParaKeys = new Dictionary<string, SqlParameter>();
                    m_sqlParaKeys.Add("@DocumentID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = qboDocument.DocumentID });
                    m_sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_20, SqlValue = _sessionHelper.AccountID });
                    m_sqlParaKeys.Add("@QboConnectID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.XeroConnectID });  //SessionHelper.QboConnectID
                    m_sqlParaKeys.Add("@QboInvoiceID", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = 50, SqlValue = string.Empty });
                    m_sqlParaKeys.Add("@QboVendorID", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = 50, SqlValue = qboDocument.XeroVendorID ?? string.Empty });
                    m_sqlParaKeys.Add("@QboVendorName", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_MAX, SqlValue = qboDocument.XeroVendorName ?? string.Empty });
                    if (qboDocument.ScanInvoiceDate == null)
                    {
                        m_sqlParaKeys.Add("@QboInvoiceDate", new SqlParameter() { SqlDbType = SqlDbType.DateTime, Size = SIZE_DATETIME, SqlValue = DBNull.Value });
                    }
                    else
                    {
                        m_sqlParaKeys.Add("@QboInvoiceDate", new SqlParameter() { SqlDbType = SqlDbType.DateTime, Size = SIZE_DATETIME, SqlValue = qboDocument.ScanInvoiceDate });
                    }


                    m_sqlParaKeys.Add("@ScanInvoiceID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = qboDocument.ScanInvoiceID });
                    m_sqlParaKeys.Add("@ScanFile_Name", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_150, SqlValue = qboDocument.ScanFile_Name ?? string.Empty });



                    m_sqlParaKeys.Add("@ScanBlob_Url", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_MAX, SqlValue = qboDocument.ScanBlob_Url ?? string.Empty });
                    m_sqlParaKeys.Add("@ScanABNNumber", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_30, SqlValue = qboDocument.ScanABNNumber ?? string.Empty });
                    m_sqlParaKeys.Add("@ScanRefNumber", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_30, SqlValue = qboDocument.ScanRefNumber ?? string.Empty });

                    m_sqlParaKeys.Add("@ScanDocType", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_30, SqlValue = qboDocument.ScanDocType ?? string.Empty });
                    m_sqlParaKeys.Add("@ScanSubTotal", new SqlParameter() { SqlDbType = SqlDbType.Decimal, Size = SIZE_DECIMAL, SqlValue = qboDocument.ScanSubTotal ?? 0 });
                    m_sqlParaKeys.Add("@ScanChargeTotal", new SqlParameter() { SqlDbType = SqlDbType.Decimal, Size = SIZE_DECIMAL, SqlValue = qboDocument.ScanChargeTotal ?? 0 });
                    m_sqlParaKeys.Add("@ScanDocumentTotal", new SqlParameter() { SqlDbType = SqlDbType.Decimal, Size = SIZE_DECIMAL, SqlValue = qboDocument.ScanDocumentTotal ?? 0 });
                    m_sqlParaKeys.Add("@ScanTag", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_30, SqlValue = qboDocument.ScanTag ?? string.Empty });

                    if (qboDocument.ScanInvoiceDate == null)
                        m_sqlParaKeys.Add("@ScanInvoiceDate", new SqlParameter() { SqlDbType = SqlDbType.DateTime, Size = SIZE_DATETIME, SqlValue = DBNull.Value });
                    else
                        m_sqlParaKeys.Add("@ScanInvoiceDate", new SqlParameter() { SqlDbType = SqlDbType.DateTime, Size = SIZE_DATETIME, SqlValue = qboDocument.ScanInvoiceDate });

                    m_sqlParaKeys.Add("@ScanPurchaseOrder", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_30, SqlValue = qboDocument.ScanPurchaseOrder ?? string.Empty });
                    m_sqlParaKeys.Add("@ScanVendorName", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_150, SqlValue = qboDocument.ScanVendorName ?? string.Empty });
                    m_sqlParaKeys.Add("@ScanInvoiceTotal", new SqlParameter() { SqlDbType = SqlDbType.Decimal, Size = SIZE_DECIMAL, SqlValue = qboDocument.ScanInvoiceTotal ?? 0 });
                    m_sqlParaKeys.Add("@ScanTaxTotal", new SqlParameter() { SqlDbType = SqlDbType.Decimal, Size = SIZE_DECIMAL, SqlValue = qboDocument.ScanTaxTotal ?? 0 });
                    m_sqlParaKeys.Add("@ScanDocClassification", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_50, SqlValue = qboDocument.ScanDocClassification ?? string.Empty });



                    m_sqlParaKeys.Add("@ScanServiceStatus", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_MAX, SqlValue = qboDocument.ScanServiceStatus ?? string.Empty });
                    m_sqlParaKeys.Add("@ScanServiceMessage", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_MAX, SqlValue = qboDocument.ScanServiceMessage ?? string.Empty });

                    m_sqlParaKeys.Add("@QboError", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_MAX, SqlValue = qboDocument.XeroError ?? string.Empty });
                    m_sqlParaKeys.Add("@AspSessionID", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_MAX, SqlValue = qboDocument.AspSessionID ?? string.Empty });
                    m_sqlParaKeys.Add("@Approved", new SqlParameter() { SqlDbType = SqlDbType.Bit, Size = SIZE_BIT, SqlValue = 0 });
                    m_sqlParaKeys.Add("@ErrorCount", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = qboDocument.ErrorCount });
                    m_sqlParaKeys.Add("@fromEmail", new SqlParameter() { SqlDbType = SqlDbType.Bit, Size = SIZE_BIT, SqlValue = 0 });
                    m_sqlParaKeys.Add("@ApproveDocAs", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = qboDocument.ApproveDocAs });

                    m_sqlParaKeys.Add("@errNum", sqlParaNum);
                    m_sqlParaKeys.Add("@errDesc", sqlParaDesc);
                    m_sqlParaKeys.Add("@identity", sqlParaIdentity);

                     SqlParameter[] sqlParameters = PrepareSql(m_sqlParaKeys, "[uspInsertXeroDocument]", ref sql);

                     ExecuteSqlCommand(sql, sqlParameters);
                  //  transaction.Commit();
                    return CosDBResponse<int>.CreateDBResponse((CSConvert.ToInt(sqlParaIdentity.Value)), CSConvert.ToString(sqlParaDesc.Value));

                }
                catch (Exception ex)
                {
                    Logger log = CosmicLogger.SetLog();
                    log.Error(ex);
                    return CosDBResponse<int>.CreateDBResponse(0, ex.Message);

                }
            }
            
        }


        internal CosDBResponse<DBResponseStatusCode> SaveXeroProduct(XeroProduct xeroProduct)
        {

            try
            {
                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();


                string sql = string.Empty;

                sqlParaKeys.Clear();
                sqlParaKeys.Add("@ID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = xeroProduct.ID });
                sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.AccountID });
                sqlParaKeys.Add("@XeroID", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_INT, SqlValue = _sessionHelper.XeroConnectID });
                sqlParaKeys.Add("@VendorID", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = 50, SqlValue = xeroProduct.VendorID ?? string.Empty });
                sqlParaKeys.Add("@AccountListID", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = 50, SqlValue = xeroProduct.AccountListID ?? string.Empty });
                sqlParaKeys.Add("@ProductCode", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = 50, SqlValue = xeroProduct.ProductCode ?? string.Empty });

                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspSaveXeroProduct]",
                                      ref sql);

                 ExecuteSqlCommand(sql, sqlParameters);


                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(GetDBResponseStatusCode(CSConvert.ToString(sqlParaDesc.Value)), CSConvert.ToString(sqlParaDesc.Value));

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(DBResponseStatusCode.FAILED, ex.Message);
            }
        }

      
        internal CosDBResponse<DBResponseStatusCode> DeleteXeroProduct(SalesItem salesItem)
        {

            try
            {
                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();


                string sql = string.Empty;

                sqlParaKeys.Clear();

                sqlParaKeys.Add("@ID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = salesItem.ID });
                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspDeleteXeroProduct]",
                                      ref sql);

                 ExecuteSqlCommand(sql, sqlParameters);


                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(GetDBResponseStatusCode(CSConvert.ToString(sqlParaDesc.Value)), CSConvert.ToString(sqlParaDesc.Value));

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(DBResponseStatusCode.FAILED, ex.Message);
            }
        }
       

        internal CosDBResponse<ICollection<XeroProduct>> GetXeroProduct(string vendorID)
        {

            try
            {

                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                string sql = string.Empty;

                sqlParaKeys.Clear();

                sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.AccountID });
                sqlParaKeys.Add("@XeroID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.XeroConnectID });
                sqlParaKeys.Add("@VendorID", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_150, SqlValue = vendorID });
                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspGetXeroProduct]",
                                         ref sql);

                var data =  SqlQuery<XeroProduct>(sql, sqlParameters).ToList();

                return CosDBResponse<ICollection<XeroProduct>>.CreateDBResponse(data, FormatSqlErrorDescParam(sqlParaDesc));
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<ICollection<XeroProduct>>.CreateDBResponse(null, ex.Message);
            }
        }

        object objInsertXeroDocumentLine = new object();
        internal CosDBResponse<int> InsertXeroDocumentLine(List<XeroDocumentLine> lstQboDocument, int xeroDocumentID)
        {
            lock(objInsertXeroDocumentLine)
            {
                try
                {
                    SqlParameter sqlParaNum = GetOutputErrNum();
                    SqlParameter sqlParaDesc = GetOutputErrDesc();
                    SqlParameter sqlParaIdentity = GetOutputIdentity();

                    foreach (var qboDocument in lstQboDocument)
                    {
                        sqlParaNum = GetOutputErrNum();
                        sqlParaDesc = GetOutputErrDesc();
                        sqlParaIdentity = GetOutputIdentity();
                        string sql = string.Empty;

                        Dictionary<string, SqlParameter> m_sqlParaKeys = new Dictionary<string, SqlParameter>();
                        m_sqlParaKeys.Add("@DocumentID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = xeroDocumentID });
                        m_sqlParaKeys.Add("@QboAccountID", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = 50, SqlValue = qboDocument.XeroAccountID ?? string.Empty });
                        m_sqlParaKeys.Add("@QboAccountName", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_150, SqlValue = qboDocument.XeroAccountName ?? string.Empty });
                        m_sqlParaKeys.Add("@QboClassID", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = 50, SqlValue = qboDocument.XeroClassID ?? string.Empty });
                        m_sqlParaKeys.Add("@QboClassName", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_150, SqlValue = qboDocument.XeroClassName ?? string.Empty });
                        m_sqlParaKeys.Add("@QboJobID", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = 50, SqlValue = qboDocument.XeroJobID ?? string.Empty });
                        m_sqlParaKeys.Add("@QboJobName", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_150, SqlValue = qboDocument.XeroJobName ?? string.Empty });
                        m_sqlParaKeys.Add("@ScanDescription", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_MAX, SqlValue = qboDocument.ScanDescription ?? string.Empty });
                        m_sqlParaKeys.Add("@ScanArticle_Code", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_MAX, SqlValue = qboDocument.ScanArticle_Code ?? string.Empty });
                        m_sqlParaKeys.Add("@ScanGL_Code", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_INT, SqlValue = qboDocument.ScanGL_Code ?? string.Empty });
                        m_sqlParaKeys.Add("@ScanGST", new SqlParameter() { SqlDbType = SqlDbType.Decimal, Size = SIZE_DECIMAL, SqlValue = qboDocument.ScanGST ?? 0 });
                        m_sqlParaKeys.Add("@ScanTax_Code", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_50, SqlValue = qboDocument.ScanTax_Code ?? string.Empty });
                        m_sqlParaKeys.Add("@ScanTracking", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_50, SqlValue = qboDocument.ScanTracking ?? string.Empty });
                        m_sqlParaKeys.Add("@ScanUnit_Measure", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_50, SqlValue = qboDocument.ScanUnit_Measure ?? string.Empty });
                        m_sqlParaKeys.Add("@ScanUnit_Price", new SqlParameter() { SqlDbType = SqlDbType.Decimal, Size = SIZE_DECIMAL, SqlValue = qboDocument.ScanUnit_Price ?? 0 });

                        m_sqlParaKeys.Add("@Scan_Quantity", new SqlParameter() { SqlDbType = SqlDbType.Decimal, Size = SIZE_DECIMAL, SqlValue = qboDocument.Scan_Quantity ?? 1 });
                        m_sqlParaKeys.Add("@Scan_Total", new SqlParameter() { SqlDbType = SqlDbType.Decimal, Size = SIZE_DECIMAL, SqlValue = qboDocument.Scan_Total ?? 0 });



                        m_sqlParaKeys.Add("@errNum", sqlParaNum);
                        m_sqlParaKeys.Add("@errDesc", sqlParaDesc);
                        m_sqlParaKeys.Add("@identity", sqlParaIdentity);

                        SqlParameter[] sqlParameters = PrepareSql(m_sqlParaKeys, "[uspInsertXeroDocumentLine]", ref sql);

                         ExecuteSqlCommand(sql, sqlParameters);
                    }

                    return CosDBResponse<int>.CreateDBResponse((CSConvert.ToInt(sqlParaIdentity.Value)), CSConvert.ToString(sqlParaDesc.Value));

                }
                catch (Exception ex)
                {
                    Logger log = CosmicLogger.SetLog();
                    log.Error(ex);
                    return CosDBResponse<int>.CreateDBResponse(0, ex.Message);

                }
            }
           
        }
        object objUpdateIsEzUploadRequired = new object();
        internal CosDBResponse<DBResponseStatusCode> UpdateIsEzUploadRequired(bool? isEzUploadRequired)
        {
            lock(objUpdateIsEzUploadRequired)
            {
                try
                {
                    SqlParameter sqlParaNum = null;
                    SqlParameter sqlParaDesc = null;
                    SqlParameter sqlParaIdentity = null;


                    sqlParaNum = GetOutputErrNum();
                    sqlParaDesc = GetOutputErrDesc();
                    sqlParaIdentity = GetOutputIdentity();
                    string sql = string.Empty;

                    sqlParaKeys.Clear();
                    sqlParaKeys.Add("@LoginID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.LoginID });
                    sqlParaKeys.Add("@IsEzUploadRequired", new SqlParameter() { SqlDbType = SqlDbType.Bit, Size = SIZE_BIT, SqlValue = isEzUploadRequired ?? false });

                    sqlParaKeys.Add("@errNum", sqlParaNum);
                    sqlParaKeys.Add("@errDesc", sqlParaDesc);


                    SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspUpdateIsEzUploadRequired]", ref sql);

                     ExecuteSqlCommand(sql, sqlParameters);

                    return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(GetDBResponseStatusCode(CSConvert.ToString(sqlParaDesc.Value)), CSConvert.ToString(sqlParaDesc.Value));

                }
                catch (Exception ex)
                {
                    Logger log = CosmicLogger.SetLog();
                    log.Error(ex);
                    return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(0, ex.Message);

                }
            }
            
        }

        internal CosDBResponse<bool> IsEzUploadRequired()
        {
            try
            {
                SqlParameter sqlParaNum = null;
                SqlParameter sqlParaDesc = null;
                SqlParameter sqlParaIdentity = null;

                sqlParaNum = GetOutputErrNum();
                sqlParaDesc = GetOutputErrDesc();
                sqlParaIdentity = GetOutputIdentity();
                string sql = string.Empty;

                sqlParaKeys.Clear();
                sqlParaKeys.Add("@LoginID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.LoginID });

                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspIsEzUploadRequired]", ref sql);

                var data =  SqlQuery<LoginMaster>(sql, sqlParameters).FirstOrDefault();

                return CosDBResponse<bool>.CreateDBResponse(data.IsEzUploadRequired ?? false, CSConvert.ToString(sqlParaDesc.Value));

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<bool>.CreateDBResponse(false, ex.Message);

            }
        }

        internal CosDBResponse<int> SaveQboDocumentEditChanges(List<QboDocumentLine> lstQboDocument)
        {
            try
            {
                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                int documentID = 0;

                foreach (var qboDocument in lstQboDocument)
                {
                    documentID = qboDocument.DocumentID;

                    sqlParaNum = GetOutputErrNum();
                    sqlParaDesc = GetOutputErrDesc();

                    string sql = string.Empty;

                    sqlParaKeys.Clear();
                    sqlParaKeys.Add("@DocumentID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = qboDocument.DocumentID });
                    sqlParaKeys.Add("@DocumentLineID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = qboDocument.DocumentLineID });

                    sqlParaKeys.Add("@ScanDescription", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_MAX, SqlValue = qboDocument.ScanDescription ?? string.Empty });
                    sqlParaKeys.Add("@ScanGST", new SqlParameter() { SqlDbType = SqlDbType.Decimal, Size = SIZE_DECIMAL, SqlValue = qboDocument.ScanGST ?? 0 });
                    sqlParaKeys.Add("@ScanUnit_Price", new SqlParameter() { SqlDbType = SqlDbType.Decimal, Size = SIZE_DECIMAL, SqlValue = qboDocument.ScanUnit_Price ?? 0 });
                    sqlParaKeys.Add("@Scan_Quantity", new SqlParameter() { SqlDbType = SqlDbType.Decimal, Size = SIZE_DECIMAL, SqlValue = qboDocument.Scan_Quantity ?? 1 });
                    sqlParaKeys.Add("@Scan_Total", new SqlParameter() { SqlDbType = SqlDbType.Decimal, Size = SIZE_DECIMAL, SqlValue = qboDocument.Scan_Total ?? 0 });
                    sqlParaKeys.Add("@ScanInvoiceTotal", new SqlParameter() { SqlDbType = SqlDbType.Decimal, Size = SIZE_DECIMAL, SqlValue = qboDocument.ScanInvoiceTotal ?? 0 });


                    sqlParaKeys.Add("@errNum", sqlParaNum);
                    sqlParaKeys.Add("@errDesc", sqlParaDesc);

                    SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspEditQboDocumentLine]", ref sql);

                     ExecuteSqlCommand(sql, sqlParameters);
                }

                return CosDBResponse<int>.CreateDBResponse(documentID, CSConvert.ToString(sqlParaDesc.Value));

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<int>.CreateDBResponse(0, ex.Message);

            }
        }

        internal CosDBResponse<int> SaveXeroDocumentEditChanges(List<XeroDocumentLine> lstQboDocument)
        {
            try
            {
                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                int documentID = 0;

                foreach (var qboDocument in lstQboDocument)
                {
                    documentID = qboDocument.DocumentID;

                    sqlParaNum = GetOutputErrNum();
                    sqlParaDesc = GetOutputErrDesc();

                    string sql = string.Empty;

                    sqlParaKeys.Clear();
                    sqlParaKeys.Add("@DocumentID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = qboDocument.DocumentID });
                    sqlParaKeys.Add("@DocumentLineID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = qboDocument.DocumentLineID });

                    sqlParaKeys.Add("@ScanDescription", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_MAX, SqlValue = qboDocument.ScanDescription ?? string.Empty });
                    sqlParaKeys.Add("@ScanGST", new SqlParameter() { SqlDbType = SqlDbType.Decimal, Size = SIZE_DECIMAL, SqlValue = qboDocument.ScanGST ?? 0 });
                    sqlParaKeys.Add("@ScanUnit_Price", new SqlParameter() { SqlDbType = SqlDbType.Decimal, Size = SIZE_DECIMAL, SqlValue = qboDocument.ScanUnit_Price ?? 0 });
                    sqlParaKeys.Add("@Scan_Quantity", new SqlParameter() { SqlDbType = SqlDbType.Decimal, Size = SIZE_DECIMAL, SqlValue = qboDocument.Scan_Quantity ?? 1 });
                    sqlParaKeys.Add("@Scan_Total", new SqlParameter() { SqlDbType = SqlDbType.Decimal, Size = SIZE_DECIMAL, SqlValue = qboDocument.Scan_Total ?? 0 });
                    sqlParaKeys.Add("@ScanInvoiceTotal", new SqlParameter() { SqlDbType = SqlDbType.Decimal, Size = SIZE_DECIMAL, SqlValue = qboDocument.ScanInvoiceTotal ?? 0 });
                    sqlParaKeys.Add("@ScanPurchaseOrder", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_30, SqlValue = qboDocument.ScanRefNumber });
                    sqlParaKeys.Add("@QboVendorID", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = 50, SqlValue = qboDocument.XeroVendorID ?? string.Empty });
                    sqlParaKeys.Add("@QboVendorName", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_MAX, SqlValue = qboDocument.XeroVendorName ?? string.Empty });
                    sqlParaKeys.Add("@QboAccountID", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = 50, SqlValue = qboDocument.XeroAccountID ?? string.Empty });
                    sqlParaKeys.Add("@QboAccountName", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_150, SqlValue = qboDocument.XeroAccountName ?? string.Empty });
                    sqlParaKeys.Add("@ScanABNNumber", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_30, SqlValue = qboDocument.ScanABNNumber ?? string.Empty });
                    sqlParaKeys.Add("@ScanInvoiceDate", new SqlParameter() { SqlDbType = SqlDbType.DateTime, Size = SIZE_DATETIME, SqlValue = qboDocument.ScanInvoiceDate });


                    sqlParaKeys.Add("@errNum", sqlParaNum);
                    sqlParaKeys.Add("@errDesc", sqlParaDesc);

                    SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspEditXeroDocumentLine]", ref sql);

                     ExecuteSqlCommand(sql, sqlParameters);
                }

                return CosDBResponse<int>.CreateDBResponse(documentID, CSConvert.ToString(sqlParaDesc.Value));

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<int>.CreateDBResponse(0, ex.Message);

            }
        }

        internal CosDBResponse<XeroMaster> SaveXeroMaster(XeroMaster xeroMaster, ref int qboConnectID)
        {
            Logger _log = CosmicLogger.SetLog();
            try
            {

                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();
                SqlParameter sqlParaIdentity = GetOutputIdentity();
                string sql = string.Empty;

                sqlParaKeys.Clear();//_sessionHelper.AccountID
                sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.AccountID });
                sqlParaKeys.Add("@OAuthToken", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_MAX, SqlValue = xeroMaster.OAuthToken ?? string.Empty });
                sqlParaKeys.Add("@OAuthTokenSec", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_MAX, SqlValue = xeroMaster.OAuthTokenSec });
                sqlParaKeys.Add("@IdentityToken", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_MAX, SqlValue = xeroMaster.IdentityToken });

                sqlParaKeys.Add("@AccessToken", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_MAX, SqlValue = xeroMaster.AccessToken ?? string.Empty });
                sqlParaKeys.Add("@AccessTokenExpiresIn", new SqlParameter() { SqlDbType = SqlDbType.DateTime, Size = SIZE_VARCHAR_MAX, SqlValue = xeroMaster.AccessTokenExpiresIn });

                sqlParaKeys.Add("@RealmId", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_MAX, SqlValue = xeroMaster.RealmId ?? string.Empty });
                sqlParaKeys.Add("@RefreshToken", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_MAX, SqlValue = xeroMaster.RefreshToken ?? string.Empty });
                sqlParaKeys.Add("@RefreshTokenExpiresIn", new SqlParameter() { SqlDbType = SqlDbType.DateTime, Size = SIZE_VARCHAR_MAX, SqlValue = xeroMaster.RefreshTokenExpiresIn });
                sqlParaKeys.Add("@SessionHandle", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_MAX, SqlValue = xeroMaster.SessionHandle });
                
                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);
                sqlParaKeys.Add("@identity", sqlParaIdentity);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspSaveXeroMaster]", ref sql);


                 ExecuteSqlCommand(sql, sqlParameters);

                if (sqlParaIdentity != null)
                {
                    qboConnectID = CSConvert.ToInt(sqlParaIdentity.Value);
                }

                xeroMaster.XeroID = qboConnectID;

                _log.Info(CSConvert.ToString(sqlParaDesc.Value));

                return CosDBResponse<XeroMaster>.CreateDBResponse(xeroMaster, CSConvert.ToString(sqlParaDesc.Value));

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                _log.Info("uspSaveXeroMaster" + ex.Message);
                return CosDBResponse<XeroMaster>.CreateDBResponse(xeroMaster, ex.Message);
            }
        }

        internal CosDBResponse<XeroMaster> SaveXeroRefreshedToken(XeroMaster xeroMaster)
        {
            Logger _log = CosmicLogger.SetLog();
            try
            {

                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();
                SqlParameter sqlParaIdentity = GetOutputIdentity();
                string sql = string.Empty;

                sqlParaKeys.Clear();//_sessionHelper.AccountID
                sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.AccountID });
                sqlParaKeys.Add("@XeroConnectID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = _sessionHelper.XeroConnectID });

                sqlParaKeys.Add("@OAuthToken", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_MAX, SqlValue = xeroMaster.OAuthToken ?? string.Empty });
                sqlParaKeys.Add("@OAuthTokenSec", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_MAX, SqlValue = xeroMaster.OAuthTokenSec ?? string.Empty });

                sqlParaKeys.Add("@AccessToken", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_MAX, SqlValue = xeroMaster.AccessToken ?? string.Empty });
                sqlParaKeys.Add("@AccessTokenExpiresIn", new SqlParameter() { SqlDbType = SqlDbType.DateTime, Size = SIZE_VARCHAR_MAX, SqlValue = xeroMaster.AccessTokenExpiresIn });
                
                sqlParaKeys.Add("@RefreshToken", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_MAX, SqlValue = xeroMaster.RefreshToken ?? string.Empty });
                sqlParaKeys.Add("@RefreshTokenExpiresIn", new SqlParameter() { SqlDbType = SqlDbType.DateTime, Size = SIZE_VARCHAR_MAX, SqlValue = xeroMaster.RefreshTokenExpiresIn });
                sqlParaKeys.Add("@SessionHandle", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_MAX, SqlValue = xeroMaster.SessionHandle });

                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);
               

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspSaveRefreshedXeroToken]", ref sql);
                
                 ExecuteSqlCommand(sql, sqlParameters);
                
                _log.Info(CSConvert.ToString(sqlParaDesc.Value));

                return CosDBResponse<XeroMaster>.CreateDBResponse(xeroMaster, CSConvert.ToString(sqlParaDesc.Value));

            }
            catch (Exception ex)
            {
                LogErrorToDB("XeroContext", "SaveXeroRefreshedToken", 
                    ex.Message + "Access Token:"+ (xeroMaster.AccessToken ?? string.Empty) + "Refresh Token:"+ (xeroMaster.RefreshToken ?? string.Empty) + " Session:"+ (xeroMaster.SessionHandle ?? string.Empty),
                    ex);
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                _log.Info("SaveXeroRefreshedToken" + ex.Message);
                return CosDBResponse<XeroMaster>.CreateDBResponse(xeroMaster, ex.Message);
            }
        }

        public CosDBResponse<Byte[]> GetXeroByte()
        {
            Logger _log = CosmicLogger.SetLog();
            try
            {

                DataTable dt = new DataTable();

                try
                {
                    SqlConnection sqlCon = new SqlConnection(base.ConnectionString);
                    SqlCommand sqlCmd = new SqlCommand("uspGetByte", sqlCon);
                    sqlCmd.CommandTimeout = 120;
                    sqlCmd.CommandType = CommandType.StoredProcedure;
                    SqlDataAdapter sqlAdapter = new SqlDataAdapter(sqlCmd);
                    sqlAdapter.Fill(dt);
                }
                catch (Exception ex)
                {
                    _log.Error(ex);
                }

                Byte[] bytes = (Byte[])dt.Rows[0]["Data"];

                return CosDBResponse<Byte[]>.CreateDBResponse(bytes, "");

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                _log.Info("uspGetByte" + ex.Message);
                return CosDBResponse<Byte[]>.CreateDBResponse(null, ex.Message);
            }
        }


        internal CosDBResponse<DBResponseStatusCode> SendTrialPdfMailNotification(int remainingTrialPdf)
        {
            try
            {
                string emailBody = SMTPHelper.GetEmailBody(MailTemplate.TrialPDFFinished);

                var cosDBResponse = GetAccountMasterByAccountID(_sessionHelper.AccountID);
                MyAccount accountMaster = cosDBResponse.Data;
                if (accountMaster != null)
                {
                    EmailBodyReplacement ebReplacement = new EmailBodyReplacement();
                    ebReplacement.UserName = accountMaster.UserName;
                    ebReplacement.Email = accountMaster.Email;
                    if (remainingTrialPdf <= 0)
                    {
                        ebReplacement.trialPDFMessage = "<br/>You have exceed your trial pdf limits... Please upgrade your plan.. <br/> <br/><br/>Thank you ";
                    }
                    else if (remainingTrialPdf <= 20 && remainingTrialPdf > 0)
                    {
                        ebReplacement.trialPDFMessage = $"You have only {remainingTrialPdf} trial pdf remaining ... So Please upgrade your plan..";
                    }
                    emailBody = SMTPHelper.FormatEmailBody(MailTemplate.TrialPDFFinished, emailBody, ebReplacement, accountMaster.Email);

                    //Initilize SMTP setting (server, port sender, to address etc)
                    var smtpSetting = SMTPHelper.InitCosmicSMPTSetting(
                                            "Upgrade Your Plan",
                                             accountMaster.Email, emailBody);
                    return SMTPHelper.SendEmail(smtpSetting);

                }


                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(DBResponseStatusCode.OK, string.Empty);
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(DBResponseStatusCode.FAILED, ex.Message);
            }
        }

    }
}