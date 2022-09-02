using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace CosmicCoreApi.Helper
{
    public static class CSConvert
    {


        public static string ToString(DataRow objRow, string colmnName)
        {
            if (objRow != null && objRow.Table.Columns.Contains(colmnName))
            {
                if (objRow[colmnName] == null)
                    return "";
                else if (objRow[colmnName] == DBNull.Value)
                    return "";
                else if (objRow[colmnName].GetType() == typeof(System.Decimal))
                {
                    decimal decVal = 0;
                    decimal.TryParse(objRow[colmnName].ToString(), out decVal);
                    return decVal.ToString("N2");
                }
                else if (objRow[colmnName].GetType() == typeof(System.DateTime))
                {
                    DateTime dtVal = new DateTime();
                    DateTime.TryParse(objRow[colmnName].ToString(), out dtVal);
                    if (dtVal.Equals(new DateTime()))
                        return "";

                    return dtVal.ToShortDateString();
                }
                else
                    return objRow[colmnName].ToString().Trim();
            }
            else
                return "";
        }

        public static string ToString(object objCol)
        {
            if (objCol == null || objCol == DBNull.Value)
                return "";
            else
                return objCol.ToString().Trim();
        }

        public static bool ToBool(DataRow objRow, string colmnName)
        {
            if (objRow.Table.Columns.Contains(colmnName))
            {
                if (objRow[colmnName] == null)
                    return false;
                else if (objRow[colmnName] == DBNull.Value)
                    return false;
                else
                {
                    bool retVal = false;
                    bool.TryParse(objRow[colmnName].ToString().Trim(), out retVal);
                    return retVal;
                }
            }
            else
                return false;
        }

        public static bool ToBool(string strValue)
        {
            bool boolValue = false;
            bool.TryParse(strValue, out boolValue);
            return boolValue;
        }

        public static bool ToBool(Object objValue)
        {
            if (objValue == null)
                return false;

            bool boolValue = false;
            bool.TryParse(objValue.ToString(), out boolValue);
            return boolValue;
        }

        public static decimal ToDecimal(DataRow objRow, string colmnName)
        {
            Decimal decimalValue = 0;
            Decimal.TryParse(ToString(objRow, colmnName), out decimalValue);
            return decimalValue;
        }


        public static decimal ToDecimal(object value)
        {
            Decimal decimalValue = 0;

            if (value == null) return decimalValue;
            if (value == DBNull.Value) return decimalValue;

            Decimal.TryParse(ToString(value), out decimalValue);
            return decimalValue;
        }


        public static int ToInt(DataRow objRow, string colmnName)
        {
            int intValue = 0;
            int.TryParse(ToString(objRow, colmnName), out intValue);
            return intValue;
        }

        public static int ToInt(string strValue)
        {
            int intValue = 0;
            int.TryParse(strValue, out intValue);
            return intValue;
        }

        public static int ToInt(object objValue)
        {
            if (objValue == null)
                return 0;
            else
            {
                int intValue = 0;
                int.TryParse(objValue.ToString(), out intValue);
                return intValue;
            }
        }

        public static string ToShortDateString(DataRow objRow, string colmnName)
        {
            if (objRow.Table.Columns.Contains(colmnName))
            {
                if (objRow[colmnName] == null)
                    return "";
                else if (objRow[colmnName] == DBNull.Value)
                    return "";
                else
                {
                    DateTime dt;
                    DateTime.TryParse(objRow[colmnName].ToString(), out dt);
                    if (dt.Equals(DateTime.MinValue))
                        return "";
                    else
                        return dt.ToShortDateString();
                }
            }
            else
                return "";
        }

        public static string ToShortDateString(string strValue)
        {
            DateTime dt;
            DateTime.TryParse(strValue, out dt);
            if (dt.Equals(DateTime.MinValue))
                return "";
            else
                return dt.ToShortDateString();
        }

        public static DateTime ToDateTime(DataRow objRow, string colmnName)
        {
            if (objRow.Table.Columns.Contains(colmnName))
            {
                if (objRow[colmnName] == null)
                    return new DateTime();
                else if (objRow[colmnName] == DBNull.Value)
                    return new DateTime();
                else
                {
                    DateTime dt;
                    DateTime.TryParse(objRow[colmnName].ToString(), out dt);
                    if (dt.Equals(DateTime.MinValue))
                        return new DateTime();
                    else
                        return dt;
                }
            }
            else
                return new DateTime();
        }

        public static string ValidateStringLength(string fieldValue, int fieldLimit)
        {
            fieldValue = fieldValue.Replace("''", "'");
            fieldValue = fieldValue.Length > fieldLimit ? fieldValue.Substring(0, fieldLimit) : fieldValue;
            fieldValue = fieldValue.Replace("'", "''");
            return fieldValue;
        }

        public static string CreateFormattedTime()
        {
            string hours;
            string minutes;
            string seconds;
            hours = minutes = seconds = string.Empty;

            hours = DateTime.Now.TimeOfDay.Hours < 10 ? "0" + DateTime.Now.TimeOfDay.Hours.ToString() : DateTime.Now.TimeOfDay.Hours.ToString();
            minutes = DateTime.Now.Minute < 10 ? "0" + DateTime.Now.Minute.ToString() : DateTime.Now.Minute.ToString();
            seconds = DateTime.Now.Second < 10 ? "0" + DateTime.Now.Second.ToString() : DateTime.Now.Second.ToString();
            return String.Format("{0}:{1}:{2}", hours, minutes, seconds);
        }

   

   

        public static DataTable GetDataTable(DataSet ds, int tableNo, ref string error)
        {
            DataTable dt = new DataTable();
            if (ds != null)
            {
                if (ds.Tables.Count > 0)
                {
                    if (!(ds.Tables.Count < (tableNo + 1)))
                    {
                        dt = ds.Tables[tableNo];
                    }
                    else
                    {
                        error = "No Table Found";
                    }
                }
            }

            return dt;
        }





    }
}