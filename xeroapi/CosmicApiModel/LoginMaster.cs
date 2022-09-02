using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmicApiModel
{
  public  class LoginMaster
    {
        public int? LoginID { get; set; }
        public int? AccountID { get; set; }
        public int? ReckonFileID { get; set; }
        public int? PlatformID { get; set; }
        public string UserName { get; set; }
        public string EmailAddress { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
        public bool? Active { get; set; }
        public string Token { get; set; }
        public DateTime? TokenExpiredDate { get; set; }
        public string IPAddress { get; set; }
        public DateTime? LastLogin { get; set; }
        public int? RoleID { get; set; }
        public DateTime? DeActivetedOn { get; set; }
        public int? AddedBy { get; set; }
        public DateTime? AddedOn { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public string ActivationCode { get; set; }
        public int? TokenAvailFor { get; set; }
        public string ExistsPassword { get; set; }
        public string NewPassword { get; set; }
        public string CountryOfOrigin { get; set; }
        public bool? IsAccess { get; set; }
        public bool? IsEzUploadRequired { get; set; }
        






    }
}
