using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CosmicCoreApi.CustomModel
{
    

    public class CBDocumentLine
    {
        public string EB_ArticleCode { get; set; }
        public string EB_Description { get; set; }
        public string EB_GLCode { get; set; }
        public decimal EB_GST { get; set; }
        public decimal EB_Quantity { get; set; }
        public string EB_Taxcode { get; set; }
        public decimal EB_Total { get; set; }
        public string EB_Tracking { get; set; }
        public string EB_UM { get; set; }
        public decimal EB_UnitPrice { get; set; }

        public string CB_ExpenseAccountListID { get; set; }
        public string CB_JobListID { get; set; }
        public string CB_Taxcode { get; set; }

    }
}
