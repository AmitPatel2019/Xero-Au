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
    
    public partial class uspGetLoginMasterByID_Result
    {
        public int LoginID { get; set; }
        public Nullable<int> AccountID { get; set; }
        public Nullable<int> PlatformID { get; set; }
        public string UserName { get; set; }
        public string EmailAddress { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
        public Nullable<bool> Active { get; set; }
        public string Token { get; set; }
        public Nullable<System.DateTime> TokenExpiredDate { get; set; }
        public string IPAddress { get; set; }
        public Nullable<System.DateTime> LastLogin { get; set; }
        public Nullable<int> RoleID { get; set; }
        public Nullable<System.DateTime> DeActivetedOn { get; set; }
        public string ActivationCode { get; set; }
        public Nullable<int> TokenAvailFor { get; set; }
        public Nullable<bool> IsEzUploadRequired { get; set; }
        public Nullable<System.DateTime> EzUploadDone { get; set; }
        public Nullable<int> AddedBy { get; set; }
        public Nullable<System.DateTime> AddedOn { get; set; }
        public Nullable<int> UpdatedBy { get; set; }
        public Nullable<System.DateTime> UpdatedOn { get; set; }
    }
}
