//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Cosmic.DataLayer
{
    using System;
    using System.Collections.Generic;
    
    public partial class SubscriptionMaster
    {
        public int SubscriptionID { get; set; }
        public Nullable<int> AccountID { get; set; }
        public Nullable<int> PlanID { get; set; }
        public Nullable<int> StartYear { get; set; }
        public Nullable<int> StartMonth { get; set; }
        public Nullable<int> StartDay { get; set; }
        public Nullable<System.DateTime> StartDateTime { get; set; }
        public Nullable<int> TrialDays { get; set; }
        public Nullable<int> TrialPdf { get; set; }
        public Nullable<int> PaidPdf { get; set; }
    }
}