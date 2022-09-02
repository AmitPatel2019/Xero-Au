using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmicApiHelper
{
    public class StringHelper
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

        public static string GetCommonDirectoryPath()
        {
            return $"{ Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments) }\\CosmicBills";
        }

    }
}
