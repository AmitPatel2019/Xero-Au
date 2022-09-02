using Common;
using CosmicApiHelper;
using CosmicApiModel;
using Flexis.Log;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using XeroAutomationService.Common;

namespace XeroAutomationService
{
    public partial class XeroService : ServiceBase
    {
        private System.Timers.Timer _scheduler;
        private bool IsElevated => new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
        Logger Log;
        public XeroService()
        {
            InitializeComponent();
            string serverActualPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments) + "\\Log";
            if (!Directory.Exists(serverActualPath))
            {
                Directory.CreateDirectory(serverActualPath);
            }
            Logger _log = BaseLog.Instance.GetLogger(null);
            serverActualPath = serverActualPath + "\\CosmicBillsXero\\cosmicBillsXeroErrorLog.txt";
            if (!File.Exists(serverActualPath))
            {
                File.Create(serverActualPath);
            }
            BaseLog.Instance.SetLogFile(serverActualPath);
            CosmicLogger.LogFilePath = serverActualPath;
            Log = CosmicLogger.SetLog();

            string path = StringHelper.GetCommonDirectoryPath();

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            var uidfilepath = $"{path}\\{Settings.ReadSetting("EmailIdFileName")}.txt";
            if (!File.Exists(uidfilepath))
            {
                File.Create(uidfilepath);
            }

            var uidpath = $"{path}\\{Settings.ReadSetting("UserIdFileName")}.txt";
            if (!File.Exists(uidpath))
            {
                File.Create(uidpath);
            }
        }
        private void EnsureElevation()
        {
            if (IsElevated)
                return;

            Log.Info("App requires elevation.  Closing...");
            Thread.Sleep(3000);
            OnStop();
            Environment.Exit(-1);
        }
        protected override void OnStart(string[] args)
        {
            try
            {
                Log.Info($"Service started.");
                var version = Assembly.GetExecutingAssembly().GetName().Version;
                Log.Info($"Version: {version}");
                ScheduleService();
                Scheduler_Elapsed(null, null); //todo pass in real values
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                Log.Error($"Error on Start ");
            }
        }

        protected override void OnStop()
        {
            Log.Info($"Service stopped.");
            this._scheduler?.Dispose();
        }

        private void ScheduleService()
        {
            var intervalInMilliseconds = GetIntervalInMilliseconds();
            _scheduler = new System.Timers.Timer(intervalInMilliseconds);
            _scheduler.Elapsed += new ElapsedEventHandler(Scheduler_Elapsed);
            _scheduler.Start();
        }

        private static int GetIntervalInMilliseconds()
        {
            var intervalMinutes = Convert.ToInt32(ConfigurationManager.AppSettings["IntervalInMinutes"]);
            var intervalInSeconds = intervalMinutes * 60;
            var intervalInMilliseconds = intervalInSeconds * 1000;
            return intervalInMilliseconds;
        }

        private void Scheduler_Elapsed(object sender, ElapsedEventArgs e)
        {
            Log.Info($"Service Run.");
            _scheduler.Stop();
            try
            {
                ScanDocumentSync();
                EmailSync();
                ReceiptSync();
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                Log.Error($"Simple Service Error on: " + ex.Message);
            }
            _scheduler.Start();
        }

        private async void ReceiptSync()
        {
            //EzzyScans ezzyScans = new EzzyScans();
            //await ezzyScans.SyncCreditCardEzzyScanning();
            //await ezzyScans.SendCreditCardToRecon();
        }
        private async void ScanDocumentSync()
        {
            EzzyScans ezzyScans = new EzzyScans(); ;
            await ezzyScans.scanUploadedDocumentfromWeb();
        }
        private async void EmailSync()
        {
            Logger _log = CosmicLogger.SetLog();
            _log.Info("Email Sync Started..");
            AutomationEmailModel emailModel = new AutomationEmailModel
            {

                Host = Settings.ReadSetting("ReceiverEmailHost"),
                Port = Convert.ToInt32(Settings.ReadSetting("ReceiverEmailPort")),
                userEmail = Settings.ReadSetting("ReceiverEmailAddress"),
                userPassword = Settings.ReadSetting("ReceiverEmailPassword"),
            };
            EmailScans emailScans = new EmailScans(emailModel);

            EzzyScans ezzyScans = new EzzyScans();
            List<XeroDocument> qboDocuments = await emailScans.ReadEmail();

            _log.Info("Total Documents=" + qboDocuments.Count());
            if (qboDocuments?.Count > 0)
            {
                await ezzyScans.SyncXeroDocumentEzzyScanning(qboDocuments);
            }
            await ezzyScans.SendDocumentToRecon();
        }
    }
}
