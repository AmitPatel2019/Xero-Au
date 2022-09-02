using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XeroAutomationService.Common
{
    public class AutomationEmailModel
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string userEmail { get; set; }
        public string userPassword { get; set; }
    }
}
