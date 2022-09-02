using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmicApiModel
{
   public class QboMaster
    {
        public int QboID { get; set; }
        public int AccountID { get; set; }
        public string RealmId { get; set; }
        public string CompanyName { get; set; }
        public string LeagalName { get; set; }
        


        public string AccessToken { get; set; }
        public DateTime? AccessTokenExpiresIn { get; set; }

        public string RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiresIn { get; set; }
        public string IdentityToken { get; set; }
        
        public DateTime? AddedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}
