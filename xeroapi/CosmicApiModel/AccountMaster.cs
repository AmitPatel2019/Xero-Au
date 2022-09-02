using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmicApiModel
{
    public class AccountMaster
    {
        public int AccountID { get; set; }
        public int PlatformID { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string CountryOfOrigin { get; set; }
        public string Phone { get; set; }
        
        public string Add1 { get; set; }
        public string Add2 { get; set; }
        public string Add3 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string Country { get; set; }
        public bool Active { get; set; }
        public string Password { get; set; }
        public string ActivationCode { get; set; }
        public DateTime? AddedDate { get; set; }
    }
}
