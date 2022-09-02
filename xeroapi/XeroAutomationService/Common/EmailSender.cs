//using PreMailer.Net;
using CosmicApiModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Mail;
using XeroAutomationService.Common;

namespace XeroAutomationService.Common
{
    public class EmailSender
    {
        public static void SendErrorEmail(EmailSettingModel emailSettingModel, List<string> emilTO, string errorMsg)
        {
            var mail = new MailMessage();
            var smtpServer = new SmtpClient(emailSettingModel.Host);
            mail.From = new MailAddress(emailSettingModel.UserName, emailSettingModel.DisplayName);
            List<string> listAdminEmail = Settings.GetAllAdminEmails();

            if (listAdminEmail?.Count > 0)
            {
                foreach (string to in listAdminEmail)
                {
                    mail.CC.Add(to);
                }
            }
            foreach (string to in emilTO)
            {
                mail.To.Add(to);
            }

            mail.Subject = $"Scan Service"; // at {ip}";
            //mail.Body = PreMailer.Net.PreMailer.MoveCssInline(errorMsg).Html;
            mail.IsBodyHtml = true;
            smtpServer.Port = emailSettingModel.Port;
            smtpServer.Credentials = new System.Net.NetworkCredential(emailSettingModel.UserName, emailSettingModel.Password);
            smtpServer.EnableSsl = true;

            //mail.CC.Clear();
            //mail.To.Clear();
            //mail.To.Add("dhrutis2000@gmail.com");

            smtpServer.Send(mail);

        }
        public static void SendSuccessEmail(EmailSettingModel emailSettingModel, List<string> emailTO, string subject, string bodyText, string userName)
        {
            var mail = new MailMessage();
            var smtpServer = new SmtpClient(emailSettingModel.Host);
            mail.From = new MailAddress(emailSettingModel.UserName, emailSettingModel.DisplayName);

            List<string> listAdminEmail = Settings.GetAllAdminEmails();

            if (listAdminEmail?.Count > 0)
            {
                foreach (string to in listAdminEmail)
                {
                    mail.CC.Add(to);
                }
            }

            foreach (string to in emailTO)
            {
                mail.To.Add(to);
            }
            EmailBodyReplacement emailBodyReplacement = new EmailBodyReplacement();
            emailBodyReplacement.UserName = userName;
            emailBodyReplacement.scanSuccessMessage = bodyText;
            emailBodyReplacement.WebLoginLink = Settings.ReadSetting("BASE_CLIENT_URL");

            mail.Subject = subject; // at {ip}";
            mail.Body = SMTPHelper.FormatEmailBody(CosmicApiHelper.Enums.MailTemplate.QboDocumentScanSuccess, SMTPHelper.GetEmailBody(CosmicApiHelper.Enums.MailTemplate.QboDocumentScanSuccess), emailBodyReplacement, null);

            mail.IsBodyHtml = true;
            smtpServer.Port = emailSettingModel.Port;
            smtpServer.Credentials = new System.Net.NetworkCredential(emailSettingModel.UserName, emailSettingModel.Password);
            smtpServer.EnableSsl = true;

            //mail.CC.Clear();
            //mail.To.Clear();
            //mail.To.Add("dhrutis2000@gmail.com");

            smtpServer.Send(mail);

        }

        public static void SendTrialPDFFinishedEmail(EmailSettingModel emailSettingModel, List<string> emailTO, string subject, string bodyText, string userName)
        {
            var mail = new MailMessage();
            var smtpServer = new SmtpClient(emailSettingModel.Host);
            mail.From = new MailAddress(emailSettingModel.UserName, emailSettingModel.DisplayName);

            List<string> listAdminEmail = Settings.GetAllAdminEmails();

            if (listAdminEmail?.Count > 0)
            {
                foreach (string to in listAdminEmail)
                {
                    mail.CC.Add(to);
                }
            }

            foreach (string to in emailTO)
            {
                mail.To.Add(to);
            }
            EmailBodyReplacement emailBodyReplacement = new EmailBodyReplacement();
            emailBodyReplacement.UserName = userName;
            emailBodyReplacement.scanSuccessMessage = bodyText;
            emailBodyReplacement.WebLoginLink = Settings.ReadSetting("BASE_CLIENT_URL");

            mail.Subject = subject; // at {ip}";
            mail.Body = SMTPHelper.FormatEmailBody(CosmicApiHelper.Enums.MailTemplate.TrialPDFFinished, SMTPHelper.GetEmailBody(CosmicApiHelper.Enums.MailTemplate.TrialPDFFinished), emailBodyReplacement, null);

            mail.IsBodyHtml = true;
            smtpServer.Port = emailSettingModel.Port;
            smtpServer.Credentials = new System.Net.NetworkCredential(emailSettingModel.UserName, emailSettingModel.Password);
            smtpServer.EnableSsl = true;

            //mail.CC.Clear();
            //mail.To.Clear();
            //mail.To.Add("dhrutis2000@gmail.com");

            smtpServer.Send(mail);

        }

        public static void SendPostSuccessEmail(EmailSettingModel emailSettingModel, List<string> emailTO, string subject, string bodyText, string userName)
        {
            var mail = new MailMessage();
            var smtpServer = new SmtpClient(emailSettingModel.Host);
            mail.From = new MailAddress(emailSettingModel.UserName, emailSettingModel.DisplayName);


            //cc
            List<string> listCCEmail = Settings.GetAllAdminEmails();
            if (listCCEmail?.Count > 0)
            {
                foreach (string to in listCCEmail)
                {
                    //mail.CC.Add(to);
                }
            }

            //bccGetBccEmails
           var listBccEmail = Settings.GetBccEmails();
            if (listBccEmail?.Count > 0)
            {
                foreach (string to in listBccEmail)
                {
                    mail.Bcc.Add(to);
                }
            }

            foreach (string to in emailTO)
            {
                mail.To.Add(to);
            }
            EmailBodyReplacement emailBodyReplacement = new EmailBodyReplacement();
            emailBodyReplacement.UserName = userName;
            emailBodyReplacement.scanSuccessMessage = bodyText;
            emailBodyReplacement.WebLoginLink = Settings.ReadSetting("BASE_CLIENT_URL").ToString();

            mail.Subject = subject; // at {ip}";
            mail.Body = SMTPHelper.FormatEmailBody(CosmicApiHelper.Enums.MailTemplate.QboDocPostSuccess, SMTPHelper.GetEmailBody(CosmicApiHelper.Enums.MailTemplate.QboDocPostSuccess), emailBodyReplacement, null);

            mail.IsBodyHtml = true;
            smtpServer.Port = emailSettingModel.Port;
            smtpServer.Credentials = new System.Net.NetworkCredential(emailSettingModel.UserName, emailSettingModel.Password);
            smtpServer.EnableSsl = true;

            //mail.CC.Clear();
            //mail.To.Clear();
            //mail.To.Add("dhrutis2000@gmail.com");

            smtpServer.Send(mail);

        }


        public static void SendErrorEmail(EmailSettingModel emailSettingModel, List<string> emailTO, string subject, string bodyText, string userName)
        {
            var mail = new MailMessage();
            var smtpServer = new SmtpClient(emailSettingModel.Host);
            mail.From = new MailAddress(emailSettingModel.UserName, emailSettingModel.DisplayName);
            List<string> listAdminEmail = Settings.GetAllAdminEmails();

            if (listAdminEmail?.Count > 0)
            {
                foreach (string to in listAdminEmail)
                {
                    mail.CC.Add(to);
                }
            }
            foreach (string to in emailTO)
            {
                mail.To.Add(to);
            }
            EmailBodyReplacement emailBodyReplacement = new EmailBodyReplacement();
            emailBodyReplacement.UserName = userName;
            emailBodyReplacement.ErrorMessage = bodyText;
            emailBodyReplacement.WebLoginLink = Settings.ReadSetting("BASE_CLIENT_URL").ToString();

            mail.Subject = subject; // at {ip}";
            mail.Body = SMTPHelper.FormatEmailBody(CosmicApiHelper.Enums.MailTemplate.QboDocumentScanFailed, SMTPHelper.GetEmailBody(CosmicApiHelper.Enums.MailTemplate.QboDocumentScanFailed), emailBodyReplacement, null);

            mail.IsBodyHtml = true;
            smtpServer.Port = emailSettingModel.Port;
            smtpServer.Credentials = new System.Net.NetworkCredential(emailSettingModel.UserName, emailSettingModel.Password);
            smtpServer.EnableSsl = true;
            //mail.CC.Clear();
            //mail.To.Clear();
            //mail.To.Add("dhrutis2000@gmail.com");
            smtpServer.Send(mail);

        }

        //public static void SendApprovedEmail(EmailSettingModel emailSettingModel,string subject, string emailTO, string bodyMessage)
        //{
        //    var mail = new MailMessage();
        //    var smtpServer = new SmtpClient(emailSettingModel.Host);
        //    mail.From = new MailAddress(emailSettingModel.UserName, emailSettingModel.DisplayName);

        //   mail.To.Add(emailTO);


        //    mail.Subject = Settings.ReadSetting("Cosmic_Email_Subject_DocumentScanned");
        //    mail.Body = bodyMessage;
        //    mail.IsBodyHtml = false;
        //    smtpServer.Port = emailSettingModel.Port;
        //    smtpServer.Credentials = new System.Net.NetworkCredential(emailSettingModel.UserName, emailSettingModel.Password);
        //    smtpServer.EnableSsl = true;

        //    smtpServer.Send(mail);

        //}
    }
}
