using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosmic.DataLayer.Logic
{
    public static class DataManager_Documents
    {
        public static List<XeroDocument> GetAllApprovedDocument(bool approved)
        {
            using (var cosmic = new XS_XEROEntities())
            {
                var result = cosmic.XeroDocuments.Where(a => a.Approved == approved && string.IsNullOrEmpty(a.XeroInvoiceID)/* && a.AccountID==1142*/);
                if (result == null)
                {
                    return null;
                }

                return result.ToList();
            }
        }

        public static XeroDocument GetXeroDocument(int documentID)
        {
            using (var cosmic = new XS_XEROEntities())
            {
                var result = cosmic.XeroDocuments.Where(a => a.DocumentID == documentID).FirstOrDefault();
                if (result == null)
                {
                    return null;
                }

                return result;
            }
        }
        public static XeroDocument GetSameInvoiceQBODocument(int docID, string scanRefNumber,int AccountID)
        {
            using (var cosmic = new XS_XEROEntities())
            {
                var result = cosmic.XeroDocuments.FirstOrDefault(a => a.ScanRefNumber == scanRefNumber && a.DocumentID != docID && !String.IsNullOrEmpty(a.XeroInvoiceID) && a.AccountID == AccountID);
                if (result == null)
                {
                    return null;
                }

                return result;
            }
        }
    }
}
