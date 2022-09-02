using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XeroAutomationService.Common
{
    public class Settings
    {
        public static void ReadAllSettings()
        {
            try
            {
                var appSettings = ConfigurationManager.AppSettings;

                if (appSettings.Count == 0)
                {
                    Console.WriteLine("AppSettings is empty.");
                }
                else
                {
                    foreach (var key in appSettings.AllKeys)
                    {
                        Console.WriteLine("Key: {0} Value: {1}", key, appSettings[key]);
                    }
                }
            }
            catch (ConfigurationErrorsException)
            {
                Console.WriteLine("Error reading app settings");
            }
        }

        public static string ReadSetting(string key)
        {
            string result = "";
            try
            {
                var appSettings = ConfigurationManager.AppSettings;
                result = appSettings[key] ?? "Not Found";
                Console.WriteLine(result);
            }
            catch (ConfigurationErrorsException)
            {
                Console.WriteLine("Error reading app settings");
            }
            return result;
        }

        public static void AddUpdateAppSettings(string key, string value)
        {
            try
            {
                var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var settings = configFile.AppSettings.Settings;
                if (settings[key] == null)
                {
                    settings.Add(key, value);
                }
                else
                {
                    settings[key].Value = value;
                }
                configFile.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
            }
            catch (ConfigurationErrorsException)
            {
                Console.WriteLine("Error writing app settings");
            }
        }

        public static EmailSettingModel GetAllEmailSettings()
        {
            var appSettings = ConfigurationManager.AppSettings;
            EmailSettingModel emailSettingModel = new EmailSettingModel
            {
                DisplayName = appSettings["Cosmic_Email_DisplayName"],
                Host = appSettings["Cosmic_Email_Server"],
                Port = Convert.ToInt32(appSettings["Cosmic_Email_Port"]),
                IsAuth = Convert.ToBoolean(appSettings["Cosmic_Email_IsAuth"]),
                IsSsl = Convert.ToBoolean(appSettings["Cosmic_Email_IsSSL"]),
                Password = appSettings["Cosmic_Email_Password"],
                UserName = appSettings["Cosmic_Email_UserID"]
            };

            return emailSettingModel;
        }

        public static List<string> GetAllAdminEmails()
        {
            var appSettings = ConfigurationManager.AppSettings;
            string adminEmail = Convert.ToString(appSettings["AdminEmails"]);



            return adminEmail.Split(',').ToList();
        }

        public static List<string> GetBccEmails()
        {
            var appSettings = ConfigurationManager.AppSettings;
            string adminEmail = Convert.ToString(appSettings["Cosmic_Email_Staff_BCC"]);



            return adminEmail.Split(',').ToList();
        }
    }
}
