using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmicApiModel
{
    public class QboJob
    {
        public int ID { get; set; }
        public string DocumentIDs { get; set; }
        public bool IsProcessed { get; set; }
        public int AccountID { get; set; }
    }
}
