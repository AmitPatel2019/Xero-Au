using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmicApiModel
{
  public  class XeroMaster
    {
        public int XeroID { get; set; }
        public int AccountID { get; set; }

        /// <summary>
        /// tenent id stored in realm field
        /// </summary>
        public string RealmId { get; set; }
        public string CompanyName { get; set; }
        public string LeagalName { get; set; }

        public string OAuthToken { get; set; } = "0";
        public string OAuthTokenSec { get; set; } = "0";
        public string AccessToken { get; set; }

        /// <summary>
        /// accesstoke default stored as UTC
        /// </summary>
        public DateTime? AccessTokenExpiresIn { get; set; }

        public string RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiresIn { get; set; }
        public string IdentityToken { get; set; }

        public DateTime? AddedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }

        public string ShortCode { get; set; }
        public bool? IsAuthrorize { get; set; }

        public string SessionHandle { get; set; } = "";
        public bool AsyncBackEndScanning { get; set; }

        public bool DirectPostfromEmail { get; set; }
        public string XeroDocPostAs { get; set; }
    }
    public class QBResponse
    {
        public string RequestId { get; set; }

        public string Data { get; set; }

        public bool? Success { get; set; }

        public bool? RetryLater { get; set; }

        public string Message { get; set; }

        public string InvoiceNo { get; set; }

        public DateTime uploadedDate { get; set; }
        public string InvoiceSupplier { get; set; }

    }
}
