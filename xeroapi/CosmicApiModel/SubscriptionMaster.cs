using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmicApiModel
{
   public class SubscriptionMaster
    {
        public int SubscriptionID { get; set; }
        public int AccountID { get; set; }
        public int PlanID { get; set; }
        public int StartYear { get; set; }
        public int StartMonth { get; set; }
        public int StartDay { get; set; }
        public DateTime? StartDateTime { get; set; }

    }
}


