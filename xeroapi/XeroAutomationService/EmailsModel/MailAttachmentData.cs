using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XeroAutomationService.EmailsModel
{
    public class MailAttachementData
    {
        public String fromEmail { get; set; }

        public byte[] fileData { get; set; }

        public String fileName { get; set; }

        public string messageId { get; set; }

        public string filePath { get; set; }

        public DateTime messageDate { get; set; }

        public int AccountID { get; set; }

        public int docPostAs { get; set; }
    }
}
