using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace CosmicApiModel
{
   public  class SMTPSetting
    {
        public string EmailSender { get; set; }
        public string Server { get; set; }
        public string Port { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public bool SSL { get; set; }
        public bool IsAuth { get; set; }

        public string EmailTo  { get; set; }
        public string EmailCCTo { get; set; }
        public string EmailBCCTo { get; set; }
        public string Subject { get; set; }
        public string EmailBody { get; set; }
        public bool EmailBodyHtml { get; set; } = true;
        public List<Attachment> Attachments { get; set; }

        public string Cosmic_Email_Staff_BCC { get; set; }

    }

    public class EmailBodyReplacement
    {
        public string UserName { get; set; }
        public string ActivationLinkUrl { get; set; }
        public string ActivationLinkText { get; set; }
        public string ActivationCode { get; set; }
        public string Password { get; set; }
        public string WebLoginLink { get; set; }
        public string Email { get; set; }
        public string ProductName { get; set; }
        public string ErrorMessage { get; set; }
        public string scanSuccessMessage { get; set; }
        public string trialPDFMessage { get; set; }
    }
}
