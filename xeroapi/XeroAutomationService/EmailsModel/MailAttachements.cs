using System;
using System.Collections.Generic;
using System.Data.Entity.SqlServer;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CosmicApiHelper;
using Flexis.Log;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MimeKit;
using XeroAutomationService.Common;
using Cosmic.DataLayer.Logic;
using Cosmic.DataLayer;

namespace XeroAutomationService.EmailsModel
{
    public class MailAttachments
    {
        private XeroContext  _csContext = new XeroContext();
        private String _userName;
        private String _password;
        private string _host;
        private int _port;
        Logger _log;
        EmailSettingModel emailSettingModel = null;

        public MailAttachments(string host, int port, string username, String password)
        {
            _log = CosmicLogger.SetLog();
            _userName = username;
            _password = password;
            _host = host;
            _port = port;
            emailSettingModel = Settings.GetAllEmailSettings();
        }


        public List<MailAttachementData> GetInboxEmails()
        {
            _log.Info("-------GetInboxEmails-------");
            List<MailAttachementData> listMailAttachementDatas = new List<MailAttachementData>();
            try
            {

                string filePath = $"{StringHelper.GetCommonDirectoryPath()}\\{Settings.ReadSetting("EmailIdFileName")}.txt";
                List<string> alreadyExist = new List<string>();
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite))
                {
                    using (StreamReader sr = new StreamReader(fs))
                    {
                        string all = sr.ReadToEnd();
                        alreadyExist = all.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    }
                }

                string uidfilePath = $"{StringHelper.GetCommonDirectoryPath()}\\{Settings.ReadSetting("UserIdFileName")}.txt";
                List<string> uidAlreadyExist = new List<string>();
                using (FileStream fs = new FileStream(uidfilePath, FileMode.Open, FileAccess.ReadWrite))
                {
                    using (StreamReader sr = new StreamReader(fs))
                    {
                        string all = sr.ReadToEnd();
                        uidAlreadyExist = all.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    }
                }

                //List<string> alreadyExist = File.ReadAllLines(filePath).ToList();

                _log.Info(_userName+"/"+_password+"  Connecting...");

                using (var client = new ImapClient())
                {
                    client.Connect(_host, _port, true);
                    client.Authenticate(_userName, _password);
                    client.Inbox.Open(FolderAccess.ReadOnly);

                    if (client.IsConnected)
                    {
                        List<UniqueId> uids = client.Inbox.Search(SearchQuery.GMailRawSearch("Category:Primary"))?.Where(a => !alreadyExist.Contains(a.Id.ToString())).ToList();
                        //List<UniqueId> newIds = uids.Where(a => !alreadyExist.Contains(a.Id.ToString())).ToList();
                        int i = 1;
                        int cnt = 0;
                        
                        for (int j = uids.Count - 1; j >= 0; j--)
                        {
                            
                            var message = client.Inbox.GetMessage(uids.ElementAt(j));

                            List<LoginMaster> accountids = new List<LoginMaster>();
                            List<string> mailSubject=null;
                            if (message.Subject?.Length>0)
                                mailSubject = message.Subject.ToString().Split(':').ToList();


                            LoginMaster login = DataManager_Users.GetLoginAccountIDfromUsername(mailSubject?.ElementAt(0));
                            if (login != null)
                                accountids.Add(login);

                            if (accountids?.Count == 0)
                                accountids = DataManager_Users.GetLoginAccountID(message.From.Mailboxes.ElementAt(0).Address.ToString());

                            if (accountids?.Count > 0)
                            {

                                for (int k = 0; k < accountids.Count; k++)
                                {
                                    _log.Info("Checking remaining pdf for Account==>" + ((LoginMaster)accountids.ElementAt(k)).AccountID.Value);
                                    try
                                    {
                                        var accountSubscribedPlan = _csContext.GetAccountSubscriptionByAccountID(((LoginMaster)accountids.ElementAt(k)).AccountID.Value).Data;
                                        var remainingPDF = 0;
                                        if (accountSubscribedPlan != null)
                                        {
                                            if (accountSubscribedPlan.IsPaidPlan.HasValue && accountSubscribedPlan.IsPaidPlan == true)
                                            {
                                                var totalPDF = accountSubscribedPlan.TotalAllocatePDF.Value;
                                                var totalUsedPDF = _csContext.GetTotalPaidPdfUsed(((LoginMaster)accountids.ElementAt(k)).AccountID.Value, 2).Data?.TotalPaidUsed.Value;
                                                _log.Info($"Paid Plan:TotalPDF={totalPDF} UsedPDF={totalUsedPDF}");

                                                if (!totalUsedPDF.HasValue)
                                                    totalUsedPDF = 0;
                                                remainingPDF = (int)(totalPDF - totalUsedPDF);
                                            }
                                            else
                                            {
                                                var totalPDF = accountSubscribedPlan.TrialPdf.Value;
                                                var totalUsedPDF = _csContext.GetTotalTrialPdfUsed(((LoginMaster)accountids.ElementAt(k)).AccountID.Value, 2).Data?.TotalTrialUsed.Value;
                                                _log.Info($"Trial Plan:TotalPDF={totalPDF} UsedPDF={totalUsedPDF}");

                                                if (!totalUsedPDF.HasValue)
                                                    totalUsedPDF = 0;
                                                remainingPDF = (int)(totalPDF - totalUsedPDF);
                                            }


                                            _log.Info($"RemainingPDF={remainingPDF}");
                                            if ((remainingPDF >= 0 && remainingPDF <= 20) || remainingPDF < message.Attachments.Count())
                                            {
                                                if (!uidAlreadyExist.Contains(((LoginMaster)accountids.ElementAt(k)).AccountID.ToString()))
                                                {
                                                    using (StreamWriter sw = File.AppendText(uidfilePath))
                                                    {
                                                        sw.WriteLine(((LoginMaster)accountids.ElementAt(k)).AccountID.ToString());
                                                    }
                                                    string msg = "";
                                                    if (remainingPDF == 0)
                                                    {
                                                        msg = "<br/>You have exceed your trial pdf limits... Please upgrade your plan.. <br/> <br/><br/>Thank you ";
                                                        //EmailSender.SendTrialPDFFinishedEmail(emailSettingModel, new List<string> { message.From.Mailboxes.ElementAt(0).Address.ToString() }, "Upgrade Your Trial PDF Plan ", msg, ((LoginMaster)accountids.ElementAt(k)).UserName);
                                                    }
                                                    else if (remainingPDF <= 20 && remainingPDF > 0)
                                                    {
                                                        msg = $"You have only {remainingPDF} trial pdf remaining ... So Please upgrade your plan..";
                                                        //EmailSender.SendTrialPDFFinishedEmail(emailSettingModel, new List<string> { message.From.Mailboxes.ElementAt(0).Address.ToString() }, "Upgrade Your Trial PDF Plan ", msg, ((LoginMaster)accountids.ElementAt(k)).UserName);
                                                    }

                                                }

                                            }
                                            if (remainingPDF > message.Attachments.Count())
                                            {
                                                try
                                                {
                                                    foreach (MimeEntity attachment in message.Attachments)
                                                    {
                                                        _log.Info($"Getting Mail Attachement for {uids[j].Id.ToString()}");

                                                        MailAttachementData mailAttachementData = new MailAttachementData();

                                                        var fileName = attachment.ContentDisposition?.FileName ?? attachment.ContentType.Name;

                                                        string path = Directory.GetCurrentDirectory() + "\\Attachments\\";

                                                        //if (!Directory.Exists(path))
                                                        //    Directory.CreateDirectory(path);

                                                        String attachementFilePath = path + fileName;

                                                        Byte[] bytes;

                                                        using (var memory = new MemoryStream())
                                                        {
                                                            if (attachment is MimePart)
                                                                ((MimePart)attachment).Content.DecodeTo(memory);
                                                            else
                                                                ((MessagePart)attachment).Message.WriteTo(memory);

                                                            bytes = memory.ToArray();
                                                            // UploadMailedDocumentQbo(bytes, fileName);
                                                        }

                                                        mailAttachementData.fileName = fileName;
                                                        mailAttachementData.filePath = attachementFilePath;
                                                        mailAttachementData.fromEmail = message.From.Mailboxes.ElementAt(0).Address.ToString();
                                                        mailAttachementData.AccountID = (int)((LoginMaster)accountids.ElementAt(k)).AccountID;
                                                        mailAttachementData.messageId = uids[j].Id.ToString();
                                                        mailAttachementData.messageDate = message.Date.LocalDateTime;
                                                        mailAttachementData.fileData = bytes;
                                                        if (mailSubject.Count() > 1 && !String.IsNullOrEmpty(mailSubject.ElementAt(1)))
                                                            mailAttachementData.docPostAs = getApprovalValue(mailSubject.ElementAt(1).ToUpper().ToString());
                                                        listMailAttachementDatas.Add(mailAttachementData);
                                                    }
                                                }
                                                catch (Exception ex)
                                                {
                                                    _log = CosmicLogger.SetLog();
                                                    _log.Error(ex);
                                                    _csContext.LogErrorToDB("Error in getting attachement", "Error in getting attachement", ex.Message, ex);

                                                }
                                            }
                                        }

                                    }
                                    catch (Exception ex)
                                    {
                                        _log = CosmicLogger.SetLog();
                                        _log.Error(ex);
                                        _csContext.LogErrorToDB("pdf checking error", "PDF check error", ex.Message, ex);
                                    }
                                }
                            }




                        }
                    }
                    client.Disconnect(true);

                }

            }
            catch (Exception ex)
            {
                _log = CosmicLogger.SetLog();
                _log.Error(ex);
                _csContext.LogErrorToDB("Read Email", "Read Email", ex.Message, ex);
            }
            return listMailAttachementDatas;
        }
        private int getApprovalValue(string status)
        {
            int result = 0;
            switch (status)
            {
                case "DR":
                    result = 1;
                    break;

                case "AP":
                    result = 2;
                    break;

                case "WAP":
                    result = 3;
                    break;

                default:
                    break;
            }
            return result;
        }
    }
}
