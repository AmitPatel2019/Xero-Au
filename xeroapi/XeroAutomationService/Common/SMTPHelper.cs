using CosmicApiHelper;
using CosmicApiHelper.Enums;
using CosmicApiModel;
using Flexis.Log;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.UI.WebControls;

namespace XeroAutomationService.Common
{
    public class SMTPHelper
    {
        public static SMTPSetting InitCosmicSMPTSetting(string subject, string emailTo, string emailBody)
        {
            SMTPSetting smtpSetting = new SMTPSetting();
            smtpSetting.Server = ConfigurationManager.AppSettings["Cosmic_Email_Server"];
            smtpSetting.UserName = ConfigurationManager.AppSettings["Cosmic_Email_UserID"];
            smtpSetting.Password = ConfigurationManager.AppSettings["Cosmic_Email_Password"];
            smtpSetting.Port = ConfigurationManager.AppSettings["Cosmic_Email_Port"];
            smtpSetting.IsAuth = StringHelper.ToBool(ConfigurationManager.AppSettings["Cosmic_Email_IsAuth"]);
            smtpSetting.SSL = StringHelper.ToBool(ConfigurationManager.AppSettings["Cosmic_Email_IsSSL"]);
            smtpSetting.EmailSender = ConfigurationManager.AppSettings["Cosmic_Email_Sender"];
            smtpSetting.Cosmic_Email_Staff_BCC = ConfigurationManager.AppSettings["Cosmic_Email_Staff_BCC"];
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
                if (File.Exists(filePath))
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
                Console.Write(ex.Message);
                return string.Empty;
            }
        }

        public static string GetEmailBody(MailTemplate mailTemplate)
        {
            Logger _log = CosmicLogger.SetLog();
            string workingDirectory = AppDomain.CurrentDomain.BaseDirectory;
            // or: Directory.GetCurrentDirectory() gives the same result
            _log.Info("Workign Directory :" + workingDirectory);

            // This will get the current PROJECT bin directory (ie ../bin/)
            string currentDirectoty = Directory.GetParent(workingDirectory).Parent.FullName;
            string emailTemplatePath = string.Empty;

            _log.Info("Directory :" + currentDirectoty);
            switch (mailTemplate)
            {
                case MailTemplate.ReconError:
                    emailTemplatePath = System.Web.Hosting.HostingEnvironment.MapPath("~/MailTemplate/ReconError.html");
                    break;
                case MailTemplate.QboDocumentScanSuccess:
                    emailTemplatePath = System.Web.Hosting.HostingEnvironment.MapPath( "~/MailTemplate/ScanSuccess.html");
                    break;
                case MailTemplate.QboDocPostSuccess:
                    emailTemplatePath = System.Web.Hosting.HostingEnvironment.MapPath( "~/MailTemplate/PostSuccess.html");
                    break;
                case MailTemplate.QboDocumentScanFailed:
                    emailTemplatePath = System.Web.Hosting.HostingEnvironment.MapPath( "~/MailTemplate/ScanError.html");
                    break;
                case MailTemplate.TrialPDFFinished:
                    emailTemplatePath = System.Web.Hosting.HostingEnvironment.MapPath("~/MailTemplate/TrialPDF.html");
                    break;
                default:
                    break;
            }

            if (!string.IsNullOrEmpty(emailTemplatePath))
                return ReadText(emailTemplatePath);

            return string.Empty; ;
        }

        public static EmailBodyReplacement GetAccountActivationLink(EmailBodyReplacement ebReplacement, string email, string activationCode)
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
                case MailTemplate.ReconError:
                    emailBody = emailBody.Replace("##USER_NAME##", emailBodyReplace.UserName)
                                         .Replace("##WebLoginLink##", emailBodyReplace.WebLoginLink)
                                         .Replace("##ErrorMessage##", emailBodyReplace.ErrorMessage);
                    break;
                case MailTemplate.QboDocumentScanSuccess:
                    emailBody = emailBody.Replace("##USER_NAME##", emailBodyReplace.UserName)
                                         .Replace("##WebLoginLink##", emailBodyReplace.WebLoginLink)
                                         .Replace("##SuccessMessage##", emailBodyReplace.scanSuccessMessage);
                    break;
                case MailTemplate.QboDocumentScanFailed:
                    emailBody = emailBody.Replace("##USER_NAME##", emailBodyReplace.UserName)
                                         .Replace("##ErrorMessage##", emailBodyReplace.scanSuccessMessage);
                    break;
                case MailTemplate.QboDocPostSuccess:
                    emailBody = emailBody.Replace("##USER_NAME##", emailBodyReplace.UserName)
                                         .Replace("##POSTSUCCESS##", emailBodyReplace.scanSuccessMessage);
                    break;
                case MailTemplate.TrialPDFFinished:
                    emailBody = emailBody.Replace("##USER_NAME##", emailBodyReplace.UserName)
                                         .Replace("##TRIALPDFMESSAGE##", emailBodyReplace.scanSuccessMessage);
                    break;
                default:
                    break;
            }

            return emailBody;
        }

        public static CosDBResponse<DBResponseStatusCode> SendEmail(SMTPSetting smtpSetting)
        {
            //now create the HTML version
            MailDefinition message = null;
            MailMessage msgHtml = null;

            Logger _log = CosmicLogger.SetLog();

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

                if (!string.IsNullOrEmpty(smtpSetting.EmailCCTo))
                    msgHtml.CC.Add(smtpSetting.EmailCCTo);

                if (!string.IsNullOrEmpty(smtpSetting.EmailBCCTo))
                    msgHtml.Bcc.Add(smtpSetting.EmailBCCTo);

                if (!string.IsNullOrEmpty(smtpSetting.Cosmic_Email_Staff_BCC))
                    msgHtml.Bcc.Add(smtpSetting.Cosmic_Email_Staff_BCC);

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
                    smtp.Port = Convert.ToInt32(smtpSetting.Port);

                    if (smtpSetting.IsAuth)
                    {
                        smtp.Credentials = new System.Net.NetworkCredential(smtpSetting.UserName, smtpSetting.Password);
                    }

                    smtp.EnableSsl = smtpSetting.SSL;

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