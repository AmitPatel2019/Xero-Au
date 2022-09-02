using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosmic.DataLayer.Logic
{
    public class DataManager_Users
    {

        public static AccountMaster GetAccount(int AccountID)
        {
            using (var cosmic = new XS_XEROEntities())
            {
                var result = cosmic.AccountMasters.FirstOrDefault(a => a.AccountID == AccountID);

                if (result == null)
                {
                    return null;
                }

                return result;
            }
        }
        public static XeroMaster GetXeroSettings(int AccountID)
        {
            using (var cosmic = new XS_XEROEntities())
            {
                var result = cosmic.XeroMasters.FirstOrDefault(a => a.AccountID == AccountID);

                if (result == null)
                {
                    return null;
                }

                return result;
            }
        }

        public static LoginMaster GetLogin(int AccountID)
        {
            using (var cosmic = new XS_XEROEntities())
            {
                var result = cosmic.LoginMasters.FirstOrDefault(a => a.AccountID == AccountID);

                if (result == null)
                {
                    return null;
                }

                return result;
            }
        }

        public static XeroEzzyAccount GetEzzyAccount(int AccountID, int xeroId)
        {
            using (var cosmic = new XS_XEROEntities())
            {
                var result = cosmic.XeroEzzyAccounts.FirstOrDefault(a => a.AccountID == AccountID && a.XeroConnectID == xeroId);

                if (result == null)
                {
                    return null;
                }

                return result;
            }
        }

        public static List<LoginMaster> GetLoginAccountID(string emailid)
        {
            List<LoginMaster> accountids = new List<LoginMaster>();
            using (var cosmic = new XS_XEROEntities())
            {
                accountids = cosmic.LoginMasters.Where(a => a.EmailAddress == emailid).ToList();
                return accountids;
            }
        }

        public static LoginMaster GetLoginAccountIDfromUsername(string username)
        {
            using (var cosmic = new XS_XEROEntities())
            {
                var result = cosmic.LoginMasters.FirstOrDefault(a => a.UserName == username);


                if (result == null)
                {
                    return null;
                }

                return result;
            }
        }


    }
}
