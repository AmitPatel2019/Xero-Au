using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmicApiModel
{
    public class PlanMaster
    {
        public int PlanID { get; set; }
        public string PlanName { get; set; }
        public string PlanDescription { get; set; }
        public int? TrialDays { get; set; }
        public int? TrialPdf { get; set; }
        public int? PaidPdf { get; set; }
        public decimal? RatePerYearGST { get; set; }
        public decimal? RatePerYearSubTotal { get; set; }
        public decimal? RatePerYearTotal { get; set; }
        public bool IsPaidPlan { get; set; }

        public string Token { get; set; } //it keeps encrypted info of Account ID and PlanID
        public string StripeToken { get; set; } //Stripe Token
        public string StripeTokenType { get; set; } //Stripe Token Type
        public string StripeEmail { get; set; } //Stripe Token Email

        public bool? IsActive { get; set; } //
        


    }
}
