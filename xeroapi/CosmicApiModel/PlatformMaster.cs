using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmicApiModel
{
  public  class PlatformMaster
    {
        public int PlatformID { get; set; }
        public string PlatformCode { get; set; }
        public string PlatformName { get; set; }
        public string Description { get; set; }
        public bool Active { get; set; }

       
    }
}
