using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace XeroAutomationService.Common
{
    public static class ExceptionHelper
    {

        public static void Setup()
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
        }

        static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            // Log the exception, display it, etc
            Debug.WriteLine(e.Exception.Message);

            var ex = (Exception)e.Exception;
            string errorMsg = $@"Message : {ex.Message} \r\n\r\n Stack Trace :{ ex.StackTrace}";

            EmailSender.SendErrorEmail(Settings.GetAllEmailSettings(), new List<string>() { "harshdesai.088@gmail.com" }, errorMsg);
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            // Log the exception, display it, etc
            Debug.WriteLine((e.ExceptionObject as Exception).Message);

            var ex = (Exception)e.ExceptionObject;
            var errorMsg = "An application error occurred. Please contact the administrator " +
                           "with the following information:\n\n";

            errorMsg += $@"\r\n\r\n Message : {ex.Message} \r\n\r\n Stack Trace :{ ex.StackTrace}";
            EmailSender.SendErrorEmail(Settings.GetAllEmailSettings(), new List<string>() { "harshdesai.088@gmail.com" }, errorMsg);
        }

        public static Exception GetInnerMostException(this Exception exception)
        {
            var innerMostException = exception;
            while (innerMostException.InnerException != null)
            {
                innerMostException = innerMostException.InnerException;
            }

            return innerMostException;
        }
    }
}
