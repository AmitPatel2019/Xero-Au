using CosmicApiModel;
using CosmicCoreApi;
using Flexis.Log;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.UI.WebControls;


namespace CosmicCoreApi.Helper
{
    public enum MailTemplate
    {
        AccountRegistration,
        ForgotPassword,
        TrialPDFFinished
    }

    public enum RandomString
    {
        AccountActivationCode,
        UserPassword,
        EzzyUserName
    }

    public class SMTPHelper
    {
        public static SMTPSetting InitCosmicSMPTSetting(string subject, string emailTo, string emailBody)
        {
            SMTPSetting smtpSetting = new SMTPSetting();
            smtpSetting.Server = ConfigurationManager.AppSettings["Cosmic_Email_Server"];
            smtpSetting.UserName = ConfigurationManager.AppSettings["Cosmic_Email_UserID"];
            smtpSetting.Password = ConfigurationManager.AppSettings["Cosmic_Email_Password"];
            smtpSetting.Port = ConfigurationManager.AppSettings["Cosmic_Email_Port"];
            smtpSetting.IsAuth = CSConvert.ToBool(ConfigurationManager.AppSettings["Cosmic_Email_IsAuth"]);
            smtpSetting.SSL = CSConvert.ToBool(ConfigurationManager.AppSettings["Cosmic_Email_IsSSL"]);

            smtpSetting.EmailSender = ConfigurationManager.AppSettings["Cosmic_Email_Sender"];
            smtpSetting.EmailBCCTo = ConfigurationManager.AppSettings["Cosmic_Email_Staff_BCC"];
         //   smtpSetting.EmailCCTo = ConfigurationManager.AppSettings["Cosmic_Email_StaffTeam"];
            smtpSetting.Subject = subject;
            smtpSetting.EmailTo = emailTo;
            smtpSetting.EmailBody = emailBody;
            return smtpSetting;

        }

        public static string ReadText(string filePath)
        {
            string text = string.Empty;
            try
            {

                if (System.IO.File.Exists(filePath))
                {
                    FileInfo file = new FileInfo(filePath);

                    StreamReader fileOpenText = file.OpenText();
                    text = fileOpenText.ReadToEnd();

                    fileOpenText.Close();

                }

                return text;
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }

        public static string GetEmailBody(MailTemplate mailTemplate)
        {
            string emailTemplatePath = string.Empty;

            switch (mailTemplate)
            {
                case MailTemplate.AccountRegistration:
                    emailTemplatePath = System.Web.Hosting.HostingEnvironment.MapPath("~/MailTemplate/AccountRegistration.html");
                    break;
                case MailTemplate.ForgotPassword:
                    emailTemplatePath = System.Web.Hosting.HostingEnvironment.MapPath("~/MailTemplate/ForgotPassword.html");
                    break;
                case MailTemplate.TrialPDFFinished:
                    emailTemplatePath = System.Web.Hosting.HostingEnvironment.MapPath("~/MailTemplate/TrialPdf.html");
                    break;
                default:
                    break;
            }

            if (!string.IsNullOrEmpty(emailTemplatePath))
                return ReadText(emailTemplatePath);

            return string.Empty; ;

            //string mailTemplate = System.Web.Hosting.HostingEnvironment.MapPath("~/MailTemplate/registrationMailTemplate.html");
            //string mailBody = CommonDataAccess.ReadText(mailTemplate);
            //mailBody = mailBody.Replace("##CUSTOMER##", customerName).Replace("##LINK##", link).Replace("##LINK_TEXT##", linkText);

            //return mailBody;
        }

        public static EmailBodyReplacement GetAccountActivationLink(EmailBodyReplacement ebReplacement,string email, string activationCode)
        {
           
            string link = ConfigurationManager.AppSettings["LinkToActivateAccount"];

            ebReplacement.ActivationLinkUrl = "<a href='" + link + "?email=" + HttpUtility.UrlEncode(EncryptionHelper.Encrpt(email)) + "&code=" + HttpUtility.UrlEncode(EncryptionHelper.Encrpt(activationCode)) + "'>Click here to activate your account</a>";
            ebReplacement.ActivationLinkText = link + "?email=" + HttpUtility.UrlEncode(EncryptionHelper.Encrpt(email)) + "&code=" + HttpUtility.UrlEncode(EncryptionHelper.Encrpt(activationCode));

            return ebReplacement;
        }

        public static string FormatEmailBody(MailTemplate mailTemplate, string emailBody, EmailBodyReplacement emailBodyReplace, string userEmail)
        {
            switch (mailTemplate)
            {
                case MailTemplate.AccountRegistration:
                    emailBody = emailBody.Replace("##USER_NAME##", emailBodyReplace.UserName)
                                          .Replace("##EMAIL##", emailBodyReplace.Email)
                                          .Replace("##WebLoginLink##", emailBodyReplace.WebLoginLink)
                                          .Replace("##PRODUCTNAME##", emailBodyReplace.ProductName);

                    break;
                case MailTemplate.ForgotPassword:
                    emailBody = emailBody.Replace("##USER_NAME##", emailBodyReplace.UserName + " (" + userEmail + ")")
                                         .Replace("##CODE##", emailBodyReplace.ActivationCode);

                    break;
                default:
                    break;
            }

            return emailBody;

            //string mailTemplate = System.Web.Hosting.HostingEnvironment.MapPath("~/MailTemplate/registrationMailTemplate.html");
            //string mailBody = CommonDataAccess.ReadText(mailTemplate);
            //mailBody = mailBody.Replace("##CUSTOMER##", customerName).Replace("##LINK##", link).Replace("##LINK_TEXT##", linkText);

            //return mailBody;
        }

        public static CosDBResponse<DBResponseStatusCode> SendEmail(SMTPSetting smtpSetting)
        {
            //now create the HTML version
            MailDefinition message = null;
            MailMessage msgHtml = null;
           
            Logger _log = CosmicLogger.SetLog();
            _log.Info("SendEmail");
            try
            {
                message = new MailDefinition();
                msgHtml = new MailMessage();
                message.From = smtpSetting.UserName;
                
                msgHtml.Body = smtpSetting.EmailBody;
                msgHtml.IsBodyHtml = smtpSetting.EmailBodyHtml;
                msgHtml.From = new MailAddress(smtpSetting.UserName);
                msgHtml.To.Add(new MailAddress(smtpSetting.EmailTo));
                msgHtml.Subject = smtpSetting.Subject;

              

                String emailCCTo = smtpSetting.EmailCCTo;


                if (emailCCTo!=null && emailCCTo.Length > 0)
                {
                    List<string> listCCEmail = emailCCTo.Split(',').ToList();
                    foreach (string cc in listCCEmail)
                    {
                        msgHtml.CC.Add(cc);
                    }
                }

                String emailBCCTo = smtpSetting.EmailBCCTo;
                List<string> listBCCEmail = emailBCCTo.Split(',').ToList();

                if (listBCCEmail?.Count > 0)
                {
                    foreach (string bcc in listBCCEmail)
                    {
                        msgHtml.Bcc.Add(bcc);
                    }
                }

                if (smtpSetting.Attachments != null)
                {
                    if (smtpSetting.Attachments.Count > 0)
                    {
                        foreach (var at in smtpSetting.Attachments)
                        {
                            msgHtml.Attachments.Add(at);
                        }
                    }
                }

                using (SmtpClient smtp = new SmtpClient())
                {
                    smtp.Host = smtpSetting.Server;
                    smtp.Port =  Convert.ToInt32(smtpSetting.Port);

                    if (smtpSetting.IsAuth)
                    {
                        _log.Info("smtpSetting");
                        smtp.Credentials = new System.Net.NetworkCredential(smtpSetting.UserName, smtpSetting.Password);
                    }

                    smtp.EnableSsl = smtpSetting.SSL;
                    _log.Info("msgHtml");
                    smtp.Send(msgHtml);
                }

                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(DBResponseStatusCode.OK, string.Empty);
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                return CosDBResponse<DBResponseStatusCode>.CreateDBResponse(DBResponseStatusCode.FAILED, ex.Message);

            }
            finally
            {
                if (msgHtml != null)
                    msgHtml.Dispose();
            }

        }


    }
}