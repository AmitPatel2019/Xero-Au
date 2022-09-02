using Cosmic.DataLayer.Logic;
using CosmicApiHelper;
using CosmicApiHelper.Enums;
using CosmicApiModel;
using XeroAutomationService.EzzyModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using XeroAutomationService.Common;
using Flexis.Log;
using XeroAutomationService.EmailsModel;

namespace XeroAutomationService.Common
{
    public class EmailScans
    {
        private XeroContext _csContext = new XeroContext();
        private AutomationEmailModel emailModel;
        Logger _log;

        public EmailScans(AutomationEmailModel emailModel)
        {
            _log = CosmicLogger.SetLog();
            this.emailModel = emailModel;
        }

        public async Task<List<XeroDocument>> ReadEmail()
        {
            _log.Info("Email Scan ==> Read Email");
            List<XeroDocument> result = new List<XeroDocument>();
            try
            {
                List<MailAttachementData> listMailAttachementDatas = new List<MailAttachementData>();
                MailAttachments mailAttachments = new MailAttachments(emailModel.Host, emailModel.Port, emailModel.userEmail, emailModel.userPassword);
                listMailAttachementDatas = mailAttachments.GetInboxEmails();
                string filePath = $"{StringHelper.GetCommonDirectoryPath()}\\{Settings.ReadSetting("EmailIdFileName")}.txt";
                _log.Info(listMailAttachementDatas.Count() + "");

                for (int i = 0; i < listMailAttachementDatas.Count(); i++)
                {
                    try
                    {
                        int AccountID = ((MailAttachementData)listMailAttachementDatas.ElementAt(i)).AccountID;
                        var XeroMaster = DataManager_Users.GetXeroSettings(((MailAttachementData)listMailAttachementDatas.ElementAt(i)).AccountID);
                        if (XeroMaster != null)
                        {
                            int QboConnectID = (int)XeroMaster.XeroID;

                            var xeroEzzyAccount = DataManager_Users.GetEzzyAccount(AccountID, QboConnectID);

                            _log.Info($"AccountId : {AccountID} ====> XEROID ====> {QboConnectID}");

                            if (xeroEzzyAccount != null)
                            {

                                EzzyServiceNew ezzyService = new EzzyServiceNew(xeroEzzyAccount.EzzyUserName, xeroEzzyAccount.EzzyPassword);

                                XeroDocument qboDocument = ezzyService.UploadXeroDocument(((MailAttachementData)listMailAttachementDatas.ElementAt(i)).fileData, ((MailAttachementData)listMailAttachementDatas.ElementAt(i)).fileName);

                                if (qboDocument != null)
                                {

                                    qboDocument.ClientFileID = "1";
                                    qboDocument.AccountID = AccountID;
                                    qboDocument.XeroConnectID = QboConnectID;
                                    qboDocument.Approved = false;
                                    qboDocument.ScanFile_Name = ((MailAttachementData)listMailAttachementDatas.ElementAt(i)).fileName;
                                    qboDocument.ApproveDocAs = ((MailAttachementData)listMailAttachementDatas.ElementAt(i)).docPostAs;

                                    var respInsertDocument = _csContext.InsertXeroDocument(qboDocument);
                                    if (respInsertDocument.StatusCode == DBResponseStatusCode.OK)
                                    {
                                        _log.Info("Document inserted successfully...");

                                        qboDocument.DocumentID = respInsertDocument.Data;
                                        result.Add(qboDocument);
                                        List<string> alreadyExist = new List<string>();
                                        using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite))
                                        {
                                            using (StreamReader sr = new StreamReader(fs))
                                            {
                                                string all = sr.ReadToEnd();
                                                alreadyExist = all.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                                            }
                                        }
                                        using (StreamWriter sw = File.AppendText(filePath))
                                        {
                                            if (!alreadyExist.Contains(((MailAttachementData)listMailAttachementDatas.ElementAt(i)).messageId))
                                                sw.WriteLine(((MailAttachementData)listMailAttachementDatas.ElementAt(i)).messageId);
                                        }

                                    }
                                    else
                                        _log.Info("Failed to insert Document ...");


                                }
                            }
                            else
                            {
                                _log.Info($"Xero Settings not found for AccountID ==> {AccountID}");
                            }
                        }
                        else
                            _log.Info("Xero Settings not found...");
                    }catch(Exception ex)
                    {
                        _log = CosmicLogger.SetLog();
                        _log.Error(ex);
                        _csContext.LogErrorToDB("EmailScans", "Xero Attachement", ex.Message, ex);
                    }
                }
            }
            catch (Exception ex)
            {
                _log = CosmicLogger.SetLog();
                _log.Error(ex);
                _csContext.LogErrorToDB("Scan", "ScanEmail", ex.Message, ex);

            }
            return result;

        }

    }
}
