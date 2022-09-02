using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XeroAutomationService.Common
{
    public class EmailSettingModel
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string UserName { get; set; }
        public string DisplayName { get; set; }
        public string Password { get; set; }
        public bool IsSsl { get; set; }
        public bool IsAuth { get; set; }
    }
}
