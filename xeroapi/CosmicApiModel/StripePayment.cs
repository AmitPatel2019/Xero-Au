using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmicApiModel
{
   public class StripePayment
    {
        public int StripePaymentID { get; set; }
        public int AccountID { get; set; }
        public int PlanID { get; set; }
        public string StripeID { get; set; }
        public string StripeBalanceTxnID { get; set; }
        public bool? StripeIsPaid { get; set; }
        public string StripeStatus { get; set; }
        public DateTime? StripeDateTime { get; set; }
        public decimal? Amount { get; set; }

        public string PlanName { get; set; }  //Extra Field
    }
}
