using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmicApiModel
{
   public class AccountSubscribedPlan
    {
        public int PlanID { get; set; }
        public string PlanName { get; set; }
        public string PlanDescription { get; set; }
        public int? TrialDays { get; set; }
        public int? TrialPdf { get; set; }
        public decimal? RatePerInvoiced { get; set; }
        public decimal? RatePerMonth { get; set; }
        public decimal? RatePerYear { get; set; }
        public int SubscriptionID { get; set; }
        public int StartYear { get; set; }
        public int StartMonth { get; set; }
        public int StartDay { get; set; }
        public DateTime? StartDateTime { get; set; }
        public bool? IsPaidPlan { get; set; }

        public int? TotalAllocatePDF { get; set; }
        public DateTime? ExpiringOn { get; set; }



    }
}
