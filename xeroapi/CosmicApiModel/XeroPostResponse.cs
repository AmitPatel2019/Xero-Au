using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmicApiModel
{
   public class XeroPostResponse
    {
        public string InvoiceNo { get; set; }
        public string Supplier { get; set; }
        public string ReponseFromXero { get; set; }
        public string ErrorMessage { get; set; }
        public bool? IsAuthrorize { get; set; }

        public DateTime uploadedDate { get; set; } = DateTime.Now;


    }
}
