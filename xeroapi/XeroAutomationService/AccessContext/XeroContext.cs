using CosmicApiModel;
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
using CosmicApiHelper;
using XeroAutomationService.CustomModel;
using XeroAutomationService;
using DevDefined.OAuth.Storage.Basic;
using XeroApi.Model;

namespace XeroAutomationService
{

    public class XeroContext : DbContext
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

        private Logger _log = null;
        private Dictionary<string, SqlParameter> sqlParaKeys = new Dictionary<string, SqlParameter>();

        public XeroContext()
            : base("name=Cosmic_Connection")
        {
            _log = CosmicLogger.SetLog();
        }

        private ErrorLog PrepareErrorLog(string controller, string method, string exceptionMessage, Exception ex)
        {
            ErrorLog errorLog = new ErrorLog();
            errorLog.Controller = controller;
            errorLog.Method = method;



            return errorLog;

        }
        public enum DBResponseStatusCode
        {
            OK = 0,
            FAILED = 1,
            EMAIL_EXISTS = 2,
            EMAIL_NOT_EXISTS = 3
        }
        public void LogErrorToDB(string controller, string method, string exceptionMessage, Exception ex)
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


                Database.ExecuteSqlCommand(sql, sqlParameters);
            }
            catch (Exception Ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
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

        private SqlParameter[] PrepareSql(Dictionary<string, SqlParameter> sqlParas,
                            string procedureName, ref string sql)
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

        internal CosDBResponse<MiscTotal> GetTotalPaidPdfUsed(int AccountID, int PlatformID)
        {
            try
            {
                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                string sql = string.Empty;

                sqlParaKeys.Clear();
                sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = AccountID });
                sqlParaKeys.Add("@PlatformID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = PlatformID });
                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspGetUsedPaidPDF]",
                                         ref sql);

                var data = Database.SqlQuery<MiscTotal>(sql, sqlParameters).FirstOrDefault();
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

        internal CosDBResponse<MiscTotal> GetTotalTrialPdfUsed(int AccountID, int PlatformID)
        {
            try
            {
                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                string sql = string.Empty;

                sqlParaKeys.Clear();
                sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = AccountID });
                sqlParaKeys.Add("@PlatformID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = PlatformID });
                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspGetUsedTrialPDF]",
                                         ref sql);

                var data = Database.SqlQuery<MiscTotal>(sql, sqlParameters).FirstOrDefault();
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

                var data = Database.SqlQuery<AccountSubscribedPlan>(sql, sqlParameters).FirstOrDefault();
                return CosDBResponse<AccountSubscribedPlan>.CreateDBResponse(data, FormatSqlErrorDescParam(sqlParaDesc));

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
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
                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();
                string sql = string.Empty;

                sqlParaKeys.Clear();
                sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = accountID });
                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspGetAccountByAccountID]",
                                         ref sql);

                var data = Database.SqlQuery<MyAccount>(sql, sqlParameters).FirstOrDefault();
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

        internal CosDBResponse<AccountMaster> GetAccountByUserNameAndCode(string userName, string code, int PlatformID)
        {
            try
            {

                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();
                //SqlParameter sqlParaIdentity = null;
                string sql = string.Empty;

                sqlParaKeys.Clear();
                sqlParaKeys.Add("@UserName", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_30, SqlValue = userName });
                sqlParaKeys.Add("@PlatformID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = PlatformID });
                sqlParaKeys.Add("@Code", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_10, SqlValue = code });
                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspValidateActivationCode]",
                                         ref sql);

                var data = Database.SqlQuery<AccountMaster>(sql, sqlParameters).FirstOrDefault();

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
        #endregion
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

                var data = Database.SqlQuery<EzzyAccount>(sql, sqlParameters).FirstOrDefault();
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
        #region QBO
        //




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


                Database.ExecuteSqlCommand(sql, sqlParameters);


                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(GetDBResponseStatusCode(StringHelper.ToString(sqlParaDesc.Value)), StringHelper.ToString(sqlParaDesc.Value));

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


                Database.ExecuteSqlCommand(sql, sqlParameters);


                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(GetDBResponseStatusCode(StringHelper.ToString(sqlParaDesc.Value)), StringHelper.ToString(sqlParaDesc.Value));

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(DBResponseStatusCode.FAILED, ex.Message);

            }
        }


        internal CosDBResponse<ICollection<XeroDocumentLine>> GetXeroDocumentToBill(int AccountID, int XeroConnectID, bool approved)
        {

            try
            {

                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                string sql = string.Empty;

                sqlParaKeys.Clear();

                sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = AccountID });
                sqlParaKeys.Add("@QboConnectID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = XeroConnectID }); //SessionHelper.QboConnectID
                sqlParaKeys.Add("@Approved", new SqlParameter() { SqlDbType = SqlDbType.Bit, Size = SIZE_BIT, SqlValue = approved });

                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspGetXeroDocumentToBill]",
                                         ref sql);

                var data = Database.SqlQuery<XeroDocumentLine>(sql, sqlParameters).ToList();
                var selectedData = data.ToList().Where(a => a.fromEmail == true).ToList();
                return CosDBResponse<ICollection<XeroDocumentLine>>.CreateDBResponse(selectedData, FormatSqlErrorDescParam(sqlParaDesc));
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<ICollection<XeroDocumentLine>>.CreateDBResponse(null, ex.Message);
            }
        }

        //


        internal CosDBResponse<ICollection<XeroDocument>> GetXeroDocumentHistoryByDate(string frDate, string todate, int AccountID, int XeroConnectID)
        {

            try
            {

                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                string sql = string.Empty;

                DateTime fromdate;
                DateTime.TryParse(frDate, out fromdate);

                DateTime toDate;
                DateTime.TryParse(todate, out toDate);

                sqlParaKeys.Clear();

                sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = AccountID });
                sqlParaKeys.Add("@QboConnectID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = XeroConnectID });
                sqlParaKeys.Add("@Fromdate", new SqlParameter() { SqlDbType = SqlDbType.DateTime, Size = 20, SqlValue = fromdate });
                sqlParaKeys.Add("@Todate", new SqlParameter() { SqlDbType = SqlDbType.DateTime, Size = 20, SqlValue = toDate });

                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspGetXeroDocumentHistoryByDate]",
                                         ref sql);

                var data = Database.SqlQuery<XeroDocument>(sql, sqlParameters).ToList();

                return CosDBResponse<ICollection<XeroDocument>>.CreateDBResponse(data, FormatSqlErrorDescParam(sqlParaDesc));
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<ICollection<XeroDocument>>.CreateDBResponse(null, ex.Message);
            }
        }


        internal CosDBResponse<ICollection<XeroDocument>> GetXeroDocumentHistory(int month, int year, int AccountID, int XeroConnectID)
        {

            try
            {

                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                string sql = string.Empty;

                sqlParaKeys.Clear();

                sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = AccountID });
                sqlParaKeys.Add("@QboConnectID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = XeroConnectID });
                sqlParaKeys.Add("@Month", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = month });
                sqlParaKeys.Add("@Year", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = year });

                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspGetXeroDocumentHistory]",
                                         ref sql);

                var data = Database.SqlQuery<XeroDocument>(sql, sqlParameters).ToList();

                return CosDBResponse<ICollection<XeroDocument>>.CreateDBResponse(data, FormatSqlErrorDescParam(sqlParaDesc));
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<ICollection<XeroDocument>>.CreateDBResponse(null, ex.Message);
            }
        }
        internal CosDBResponse<ICollection<XeroTax>> GetXeroTax(int AccountID, int XeroConnectID)
        {

            try
            {

                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                string sql = string.Empty;

                sqlParaKeys.Clear();

                sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = AccountID });
                sqlParaKeys.Add("@QboConnectID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = XeroConnectID });
                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspGetXeroTax]",
                                         ref sql);

                var data = Database.SqlQuery<XeroTax>(sql, sqlParameters).ToList();

                return CosDBResponse<ICollection<XeroTax>>.CreateDBResponse(data, FormatSqlErrorDescParam(sqlParaDesc));
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<ICollection<XeroTax>>.CreateDBResponse(null, ex.Message);
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

                var data = Database.SqlQuery<CheckXeroToken>(sql, sqlParameters).FirstOrDefault();

                return CosDBResponse<CheckXeroToken>.CreateDBResponse(data, FormatSqlErrorDescParam(sqlParaDesc));
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<CheckXeroToken>.CreateDBResponse(null, ex.Message);
            }
        }

        internal CosDBResponse<ICollection<XeroDocument>> GetXeroDocumentToApprove(bool approved, int AccountID, int XeroConnectID)
        {

            try
            {

                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                string sql = string.Empty;

                sqlParaKeys.Clear();

                sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = AccountID });
                sqlParaKeys.Add("@QboConnectID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = XeroConnectID });
                sqlParaKeys.Add("@AspSessionID", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_MAX, SqlValue = string.Empty });
                sqlParaKeys.Add("@Approved", new SqlParameter() { SqlDbType = SqlDbType.Bit, Size = SIZE_BIT, SqlValue = approved });
                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspGetXeroDocumentToApprove]",
                                         ref sql);

                var data = Database.SqlQuery<XeroDocument>(sql, sqlParameters).ToList();

                return CosDBResponse<ICollection<XeroDocument>>.CreateDBResponse(data, FormatSqlErrorDescParam(sqlParaDesc));
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<ICollection<XeroDocument>>.CreateDBResponse(null, ex.Message);
            }
        }


        internal CosDBResponse<ICollection<XeroDocument>> GetXeroDocumentToScan(int AccountID, int XeroConnectID)
        {

            try
            {

                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                string sql = string.Empty;

                sqlParaKeys.Clear();

                sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = AccountID });
                sqlParaKeys.Add("@QboConnectID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = XeroConnectID });

                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspGetXeroDocumentToProcess]",
                                         ref sql);

                var data = Database.SqlQuery<XeroDocument>(sql, sqlParameters).ToList();

                return CosDBResponse<ICollection<XeroDocument>>.CreateDBResponse(data, FormatSqlErrorDescParam(sqlParaDesc));
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<ICollection<XeroDocument>>.CreateDBResponse(null, ex.Message);
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

                var data = Database.SqlQuery<XeroDocument>(sql, sqlParameters).FirstOrDefault();

                return CosDBResponse<XeroDocument>.CreateDBResponse(data, FormatSqlErrorDescParam(sqlParaDesc));
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<XeroDocument>.CreateDBResponse(null, ex.Message);
            }
        }



        internal CosDBResponse<ICollection<CBDocument>> GetXeroDocumentToRead(int AccountID, int XeroConnectID)
        {

            try
            {

                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                string sql = string.Empty;

                sqlParaKeys.Clear();

                sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = AccountID });
                sqlParaKeys.Add("@QboConnectID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = XeroConnectID }); //SessionHelper.QboConnectID
                                                                                                                                               // sqlParaKeys.Add("@AspSessionID", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_MAX, SqlValue = sessionID });
                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspGetXeroDocumentToRead]",
                                         ref sql);

                var data = Database.SqlQuery<CBDocument>(sql, sqlParameters).ToList();

                return CosDBResponse<ICollection<CBDocument>>.CreateDBResponse(data, FormatSqlErrorDescParam(sqlParaDesc));
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<ICollection<CBDocument>>.CreateDBResponse(null, ex.Message);
            }
        }




        internal CosDBResponse<ICollection<XeroDocumentLine>> GetXeroDocumentToAuth(int AccountID, int XeroConnectID)
        {

            try
            {

                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                string sql = string.Empty;

                sqlParaKeys.Clear();

                sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = AccountID });
                sqlParaKeys.Add("@QboConnectID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = XeroConnectID }); //SessionHelper.QboConnectID
                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspGetXeroDocumentToAuth]",
                                         ref sql);

                var data = Database.SqlQuery<XeroDocumentLine>(sql, sqlParameters).ToList();

                return CosDBResponse<ICollection<XeroDocumentLine>>.CreateDBResponse(data, FormatSqlErrorDescParam(sqlParaDesc));
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<ICollection<XeroDocumentLine>>.CreateDBResponse(null, ex.Message);
            }
        }


        internal CosDBResponse<ICollection<XeroDocumentLine>> GetXeroDocumentToBill(bool approved, int AccountID, int XeroConnectID)
        {

            try
            {

                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                string sql = string.Empty;

                sqlParaKeys.Clear();

                sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = AccountID });
                sqlParaKeys.Add("@QboConnectID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = XeroConnectID }); //SessionHelper.QboConnectID
                sqlParaKeys.Add("@Approved", new SqlParameter() { SqlDbType = SqlDbType.Bit, Size = SIZE_BIT, SqlValue = approved });

                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspGetXeroDocumentToBill]",
                                         ref sql);

                var data = Database.SqlQuery<XeroDocumentLine>(sql, sqlParameters).ToList();

                return CosDBResponse<ICollection<XeroDocumentLine>>.CreateDBResponse(data, FormatSqlErrorDescParam(sqlParaDesc));
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<ICollection<XeroDocumentLine>>.CreateDBResponse(null, ex.Message);
            }
        }



        internal CosDBResponse<ICollection<XeroMaster>> GetXeroMasterByAccountID(int AccountID)
        {

            try
            {

                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                string sql = string.Empty;

                sqlParaKeys.Clear();

                sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = AccountID });
                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspGetXeroMasterByAccountID]",
                                         ref sql);

                var data = Database.SqlQuery<XeroMaster>(sql, sqlParameters).ToList();

                return CosDBResponse<ICollection<XeroMaster>>.CreateDBResponse(data, FormatSqlErrorDescParam(sqlParaDesc));
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<ICollection<XeroMaster>>.CreateDBResponse(null, ex.Message);
            }
        }




        internal CosDBResponse<XeroMaster> GetXeroMasterByAccountAndConnectID(int AccountID, int XeroConnectID)
        {
            try
            {

                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                string sql = string.Empty;

                sqlParaKeys.Clear();

                sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = AccountID });
                sqlParaKeys.Add("@XeroConnectID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = XeroConnectID }); //SessionHelper.QboConnectID
                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspGetXeroMasterByAccountAndConnectID]",
                                         ref sql);

                var data = Database.SqlQuery<XeroMaster>(sql, sqlParameters).FirstOrDefault();

                return CosDBResponse<XeroMaster>.CreateDBResponse(data, FormatSqlErrorDescParam(sqlParaDesc));
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<XeroMaster>.CreateDBResponse(null, ex.Message);
            }
        }

        #endregion

        #region ReckonEzzyAccount

        internal CosDBResponse<ReckonEzzyAccount> GetRCScanningCredential(int AccountID, int ReckonFileID)
        {
            try
            {

                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                string sql = string.Empty;

                sqlParaKeys.Clear();

                sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = AccountID });
                sqlParaKeys.Add("@ReckonFileID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = ReckonFileID });
                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspGetRCScanningLogin]", ref sql);

                var data = Database.SqlQuery<ReckonEzzyAccount>(sql, sqlParameters).FirstOrDefault();

                return CosDBResponse<ReckonEzzyAccount>.CreateDBResponse(data, FormatSqlErrorDescParam(sqlParaDesc));

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<ReckonEzzyAccount>.CreateDBResponse(null, ex.Message);
            }
        }
        internal CosDBResponse<XeroEzzyAccount> GetXeroScanningCredential(int AccountID, int XeroConnectID)
        {
            try
            {

                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                string sql = string.Empty;

                sqlParaKeys.Clear();

                sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = AccountID });
               // sqlParaKeys.Add("@QboConnectID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = XeroConnectID }); // SessionHelper.QboConnectID 
                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspGetXeroScanningLogin]", ref sql);

                var data = Database.SqlQuery<XeroEzzyAccount>(sql, sqlParameters).FirstOrDefault();

                return CosDBResponse<XeroEzzyAccount>.CreateDBResponse(data, FormatSqlErrorDescParam(sqlParaDesc));
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<XeroEzzyAccount>.CreateDBResponse(null, ex.Message);
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

                var data = Database.SqlQuery<XeroEzzyAccount>(sql, sqlParameters).FirstOrDefault();

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

        internal CosDBResponse<DBResponseStatusCode> SaveXeroVendor(Contact item, int AccountID, int XeroConnectID)
        {
            try
            {
                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                string sql = string.Empty;

                sqlParaKeys.Clear();

                sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = AccountID });
                sqlParaKeys.Add("@QboConnectID", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_INT, SqlValue = XeroConnectID });
                sqlParaKeys.Add("@QboVendorID", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = 50, SqlValue = Convert.ToString(item.ContactID) });

                sqlParaKeys.Add("@DisplayNameField", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_150, SqlValue = item.Name ?? string.Empty });

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
                    //SqlValue = (item.Phones != null ? (item.ContactNumber ?? string.Empty) : string.Empty)
                });

                sqlParaKeys.Add("@PrimaryEmailAddr", new SqlParameter()
                {
                    SqlDbType = SqlDbType.VarChar,
                    Size = SIZE_VARCHAR_100,
                    SqlValue = (item.EmailAddress != null ? (item.EmailAddress ?? string.Empty) : string.Empty)
                });

                sqlParaKeys.Add("@TaxIdentifier", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_20, SqlValue = item.TaxNumber ?? string.Empty });


                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspSaveXeroVendor]", ref sql);

                Database.ExecuteSqlCommand(sql, sqlParameters);


                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(GetDBResponseStatusCode(StringHelper.ToString(sqlParaDesc.Value)), StringHelper.ToString(sqlParaDesc.Value));

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(DBResponseStatusCode.FAILED, ex.Message);
            }
        }

        #endregion

        #region Reconk Document
        internal CosDBResponse<int> SaveReckonDocument(ReckonDocument reckonDocument, int AccountID, int ReckonFileID)
        {
            try
            {

                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();
                SqlParameter sqlParaIdentity = GetOutputIdentity();
                string sql = string.Empty;

                sqlParaKeys.Clear();
                sqlParaKeys.Add("@ReckonDocumentID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = reckonDocument.ReckonDocumentID });
                sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = AccountID });
                sqlParaKeys.Add("@ReckonFileID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = ReckonFileID });
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


                Database.ExecuteSqlCommand(sql, sqlParameters);

                return CosDBResponse<int>.CreateDBResponse((StringHelper.ToInt(sqlParaIdentity.Value)), StringHelper.ToString(sqlParaDesc.Value));

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

                var data = Database.SqlQuery<StripePayment>(sql, sqlParameters).ToList();
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

                var data = Database.SqlQuery<QboJob>(sql, sqlParameters).ToList();

                return CosDBResponse<ICollection<QboJob>>.CreateDBResponse(data, FormatSqlErrorDescParam(sqlParaDesc));
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<ICollection<QboJob>>.CreateDBResponse(null, ex.Message);
            }
        }
        internal CosDBResponse<ICollection<QboJob>> UpdateQboJob(int jobID)
        {

            try
            {

                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                string sql = string.Empty;

                sqlParaKeys.Clear();

                sqlParaKeys.Add("@QboJobID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = jobID });
                sqlParaKeys.Add("@IsProcessed", new SqlParameter() { SqlDbType = SqlDbType.Bit, Size = SIZE_BIT, SqlValue = true });

                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspUpdateQboJob]",
                                         ref sql);

                var data = Database.SqlQuery<QboJob>(sql, sqlParameters).ToList();

                return CosDBResponse<ICollection<QboJob>>.CreateDBResponse(data, FormatSqlErrorDescParam(sqlParaDesc));
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<ICollection<QboJob>>.CreateDBResponse(null, ex.Message);
            }
        }

        internal CosDBResponse<ICollection<XeroDocumentLine>> GetXeroDocumentLine(int xeroDocumentID, int accountID, int rahID)
        {

            try
            {
                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                string sql = string.Empty;

                sqlParaKeys.Clear();

                sqlParaKeys.Add("@DocumentID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = xeroDocumentID }); //SessionHelper.QboConnectID
                sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = accountID });
                sqlParaKeys.Add("@QboConnectID", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_INT, SqlValue = rahID });

                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspGetXeroDocumentLineByDocumentID]",
                                         ref sql);

                var data = Database.SqlQuery<XeroDocumentLine>(sql, sqlParameters).ToList();

                return CosDBResponse<ICollection<XeroDocumentLine>>.CreateDBResponse(data, FormatSqlErrorDescParam(sqlParaDesc));
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<ICollection<XeroDocumentLine>>.CreateDBResponse(null, ex.Message);
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

                var data = Database.SqlQuery<QboDocument>(sql, sqlParameters).FirstOrDefault();

                return CosDBResponse<QboDocument>.CreateDBResponse(data, FormatSqlErrorDescParam(sqlParaDesc));
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<QboDocument>.CreateDBResponse(null, ex.Message);
            }
        }

        internal CosDBResponse<DBResponseStatusCode> SaveXeroVendorUnKnow(string id, string name, int accountID, int xeroConnectID)
        {
            try
            {
                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                string sql = string.Empty;

                sqlParaKeys.Clear();

                sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = accountID });
                sqlParaKeys.Add("@QboConnectID", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_INT, SqlValue = xeroConnectID });
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

                Database.ExecuteSqlCommand(sql, sqlParameters);


                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(GetDBResponseStatusCode(StringHelper.ToString(sqlParaDesc.Value)), StringHelper.ToString(sqlParaDesc.Value));

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(DBResponseStatusCode.FAILED, ex.Message);
            }
        }

        internal CosDBResponse<DBResponseStatusCode> UpdateIsEzUploadRequired(int LoginID, bool? isEzUploadRequired)
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
                sqlParaKeys.Add("@LoginID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = LoginID });
                sqlParaKeys.Add("@IsEzUploadRequired", new SqlParameter() { SqlDbType = SqlDbType.Bit, Size = SIZE_BIT, SqlValue = isEzUploadRequired ?? false });

                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);


                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspUpdateIsEzUploadRequired]", ref sql);

                Database.ExecuteSqlCommand(sql, sqlParameters);

                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(GetDBResponseStatusCode(StringHelper.ToString(sqlParaDesc.Value)), StringHelper.ToString(sqlParaDesc.Value));

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(0, ex.Message);

            }
        }

        internal CosDBResponse<ICollection<XeroVendor>> GetXeroVendor(int accountID, int xeroConnectID)
        {

            try
            {

                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                string sql = string.Empty;

                sqlParaKeys.Clear();

                sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = accountID });
                sqlParaKeys.Add("@QboConnectID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = xeroConnectID });
                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspGetXeroVendor]",
                                         ref sql);

                var data = Database.SqlQuery<XeroVendor>(sql, sqlParameters).ToList();

                return CosDBResponse<ICollection<XeroVendor>>.CreateDBResponse(data, FormatSqlErrorDescParam(sqlParaDesc));
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<ICollection<XeroVendor>>.CreateDBResponse(null, ex.Message);
            }
        }

        internal CosDBResponse<XeroVendor> GetXeroVendorByVendorID(int accountID, string xeroVendorID)
        {

            try
            {

                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                string sql = string.Empty;

                sqlParaKeys.Clear();

                sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = accountID });
                sqlParaKeys.Add("@XeroVendorID", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_250, SqlValue = xeroVendorID });
                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspGetXeroVendorByVendorID]",
                                         ref sql);

                var data = Database.SqlQuery<XeroVendor>(sql, sqlParameters).FirstOrDefault();

                return CosDBResponse<XeroVendor>.CreateDBResponse(data, FormatSqlErrorDescParam(sqlParaDesc));
            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<XeroVendor>.CreateDBResponse(null, ex.Message);
            }
        }

        internal CosDBResponse<int> InsertXeroDocument(XeroDocument qboDocument)
        {
            try
            {

                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();
                SqlParameter sqlParaIdentity = GetOutputIdentity();
                string sql = string.Empty;

                sqlParaKeys.Clear();
                sqlParaKeys.Add("@DocumentID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = qboDocument.DocumentID });
                sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = qboDocument.AccountID });
                sqlParaKeys.Add("@QboConnectID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = qboDocument.XeroConnectID });  //SessionHelper.QboConnectID
                sqlParaKeys.Add("@QboInvoiceID", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = 50, SqlValue = string.Empty });
                sqlParaKeys.Add("@QboVendorID", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = 50, SqlValue = qboDocument.XeroVendorID ?? string.Empty });
                sqlParaKeys.Add("@QboVendorName", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_MAX, SqlValue = qboDocument.XeroVendorName ?? string.Empty });
                if (qboDocument.ScanInvoiceDate == null)
                {
                    sqlParaKeys.Add("@QboInvoiceDate", new SqlParameter() { SqlDbType = SqlDbType.DateTime, Size = SIZE_DATETIME, SqlValue = DBNull.Value });
                }
                else
                {
                    sqlParaKeys.Add("@QboInvoiceDate", new SqlParameter() { SqlDbType = SqlDbType.DateTime, Size = SIZE_DATETIME, SqlValue = qboDocument.ScanInvoiceDate });
                }
                sqlParaKeys.Add("@ScanInvoiceID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = qboDocument.ScanInvoiceID ?? 0 });
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
                sqlParaKeys.Add("@QboError", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_MAX, SqlValue = qboDocument.XeroError ?? string.Empty });
                sqlParaKeys.Add("@AspSessionID", new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = SIZE_VARCHAR_MAX, SqlValue = qboDocument.AspSessionID ?? string.Empty });
                sqlParaKeys.Add("@Approved", new SqlParameter() { SqlDbType = SqlDbType.Bit, Size = SIZE_BIT, SqlValue = 0 });
                sqlParaKeys.Add("@ErrorCount", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = qboDocument.ErrorCount });
                sqlParaKeys.Add("@fromEmail", new SqlParameter() { SqlDbType = SqlDbType.Bit, Size = SIZE_BIT, SqlValue = 1 });
                sqlParaKeys.Add("@ApproveDocAs", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = qboDocument.ApproveDocAs });

                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);
                sqlParaKeys.Add("@identity", sqlParaIdentity);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspInsertXeroDocument]", ref sql);

                Database.ExecuteSqlCommand(sql, sqlParameters);

                return CosDBResponse<int>.CreateDBResponse((StringHelper.ToInt(sqlParaIdentity.Value)), StringHelper.ToString(sqlParaDesc.Value));

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<int>.CreateDBResponse(0, ex.Message);

            }
        }

        internal CosDBResponse<int> InsertXeroDocumentLine(List<XeroDocumentLine> lstQboDocument, int xeroDocumentID, int AccountID, int XeroConnectID)
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
                    sqlParaKeys.Add("@DocumentID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = xeroDocumentID });
                    sqlParaKeys.Add("@QboAccountID", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = 50, SqlValue = qboDocument.XeroAccountID ?? string.Empty });
                    sqlParaKeys.Add("@QboAccountName", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_150, SqlValue = qboDocument.XeroAccountName ?? string.Empty });
                    sqlParaKeys.Add("@QboClassID", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = 50, SqlValue = qboDocument.XeroClassID ?? string.Empty });
                    sqlParaKeys.Add("@QboClassName", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_150, SqlValue = qboDocument.XeroClassName ?? string.Empty });
                    sqlParaKeys.Add("@QboJobID", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = 50, SqlValue = qboDocument.XeroJobID ?? string.Empty });
                    sqlParaKeys.Add("@QboJobName", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_150, SqlValue = qboDocument.XeroJobName ?? string.Empty });
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

                    SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspInsertXeroDocumentLine]", ref sql);

                    Database.ExecuteSqlCommand(sql, sqlParameters);
                }

                return CosDBResponse<int>.CreateDBResponse((StringHelper.ToInt(sqlParaIdentity.Value)), StringHelper.ToString(sqlParaDesc.Value));

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<int>.CreateDBResponse(0, ex.Message);

            }
        }

        internal CosDBResponse<DBResponseStatusCode> UpdateIsEzUploadRequired(bool? isEzUploadRequired, int LoginID)
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
                sqlParaKeys.Add("@LoginID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = LoginID });
                sqlParaKeys.Add("@IsEzUploadRequired", new SqlParameter() { SqlDbType = SqlDbType.Bit, Size = SIZE_BIT, SqlValue = isEzUploadRequired ?? false });

                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);


                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspUpdateIsEzUploadRequired]", ref sql);

                Database.ExecuteSqlCommand(sql, sqlParameters);

                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(GetDBResponseStatusCode(StringHelper.ToString(sqlParaDesc.Value)), StringHelper.ToString(sqlParaDesc.Value));

            }
            catch (Exception ex)
            {
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(0, ex.Message);

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
                    SqlConnection sqlCon = new SqlConnection(Database.Connection.ConnectionString);
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
                sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = xeroMaster.AccountID });
                sqlParaKeys.Add("@XeroConnectID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = xeroMaster.XeroID });

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

                Database.ExecuteSqlCommand(sql, sqlParameters);

                _log.Info(StringHelper.ToString(sqlParaDesc.Value));

                return CosDBResponse<XeroMaster>.CreateDBResponse(xeroMaster, StringHelper.ToString(sqlParaDesc.Value));

            }
            catch (Exception ex)
            {
                LogErrorToDB("XeroContext", "SaveXeroRefreshedToken",
                    ex.Message + "Access Token:" + (xeroMaster.AccessToken ?? string.Empty) + "Refresh Token:" + (xeroMaster.RefreshToken ?? string.Empty) + " Session:" + (xeroMaster.SessionHandle ?? string.Empty),
                    ex);
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);
                _log.Info("SaveXeroRefreshedToken" + ex.Message);
                return CosDBResponse<XeroMaster>.CreateDBResponse(xeroMaster, ex.Message);
            }
        }

        internal CosDBResponse<DBResponseStatusCode> SaveXeroCompanyInfo(Organisation companyInfo, int AccountID, string XeroRealmId)
        {

            try
            {
                QboMaster qboMaster = new QboMaster();

                SqlParameter sqlParaNum = GetOutputErrNum();
                SqlParameter sqlParaDesc = GetOutputErrDesc();

                string sql = string.Empty;

                sqlParaKeys.Clear();

                sqlParaKeys.Add("@AccountID", new SqlParameter() { SqlDbType = SqlDbType.Int, Size = SIZE_INT, SqlValue = AccountID });
                sqlParaKeys.Add("@RealmId", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_MAX, SqlValue = XeroRealmId });
                sqlParaKeys.Add("@CompanyName", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_MAX, SqlValue = companyInfo.Name ?? string.Empty });
                sqlParaKeys.Add("@LegalName", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_MAX, SqlValue = companyInfo.LegalName ?? string.Empty });
                sqlParaKeys.Add("@Email", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_MAX, SqlValue = "" });
                sqlParaKeys.Add("@PrimaryPhone", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_MAX, SqlValue = "" });
                sqlParaKeys.Add("@WebAddr", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_MAX, SqlValue = "" });
                sqlParaKeys.Add("@Country", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = SIZE_VARCHAR_MAX, SqlValue = companyInfo.CountryCode ?? string.Empty });
                sqlParaKeys.Add("@ShortCode", new SqlParameter() { SqlDbType = SqlDbType.VarChar, Size = 10, SqlValue = companyInfo.ShortCode ?? string.Empty });

                sqlParaKeys.Add("@errNum", sqlParaNum);
                sqlParaKeys.Add("@errDesc", sqlParaDesc);

                SqlParameter[] sqlParameters = PrepareSql(sqlParaKeys, "[uspUpdateXeroCompanyInfo]",
                                      ref sql);

                Database.ExecuteSqlCommand(sql, sqlParameters);

                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(GetDBResponseStatusCode(StringHelper.ToString(sqlParaDesc.Value)), StringHelper.ToString(sqlParaDesc.Value));

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