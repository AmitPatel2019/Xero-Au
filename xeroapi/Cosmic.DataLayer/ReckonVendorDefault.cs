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
    
    public partial class ReckonVendorDefault
    {
        public int ID { get; set; }
        public Nullable<int> AccountID { get; set; }
        public Nullable<int> ReckonFileID { get; set; }
        public string VendorListID { get; set; }
        public string DefaultExpenseListID { get; set; }
        public string DefaultExpenseAccount { get; set; }
        public string DefaultItemListID { get; set; }
        public string DefaultItem { get; set; }
        public Nullable<int> AddedBy { get; set; }
        public Nullable<System.DateTime> AddedDate { get; set; }
        public Nullable<int> UpdatedBy { get; set; }
        public Nullable<System.DateTime> UpdatedDate { get; set; }
    }
}
