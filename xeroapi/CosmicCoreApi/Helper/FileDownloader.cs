
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CosmicCoreApi.Helper
{
    class FileDownloader
    {
        private readonly string _url;
        private readonly string _fullPathWhereToSave;
        private bool _result = false;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(0);


        StreamReader reader = null;
        static WebClient client = null;


        public static void SaveMemoryStream(MemoryStream ms, string FileName)
        {
            FileStream outStream = File.OpenWrite(FileName);
            ms.WriteTo(outStream);
            outStream.Flush();
            outStream.Close();
        }

        private void Test(string url)
        {
            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
            try
            {

                new System.Net.WebClient().DownloadFile(url, "F:\\testing.PDF");

                //byte[] arrayofbyte = new System.Net.WebClient().DownloadData(url);
                //MemoryStream ms = new MemoryStream(arrayofbyte);

                ////Save the Byte Array as File.

                //File.WriteAllBytes("F:\\test.PDF", arrayofbyte);

                //System.Web.HttpContext.Current.Response.ClearContent();
                //System.Web.HttpContext.Current.Response.ContentEncoding = System.Text.Encoding.UTF8;
                //System.Web.HttpContext.Current.Response.AppendHeader("Content-Length", ms.Length.ToString());
                //System.Web.HttpContext.Current.Response.AddHeader("ContentType", "application/pdf; charset=utf-8");
                //System.Web.HttpContext.Current.Response.AddHeader("Content-Disposition", "attachment; filename=" + DateTime.Now.Ticks.ToString() + ".pdf" + ";");
                //System.Web.HttpContext.Current.Response.BinaryWrite(ms.ToArray());
                //System.Web.HttpContext.Current.Response.Flush();
                //System.Web.HttpContext.Current.Response.Close();
            }
            catch (Exception ex)
            {

            }
        }


        public FileDownloader() { }

        public FileDownloader(string url, string fullPathWhereToSave)
        {
            if (string.IsNullOrEmpty(url)) throw new ArgumentNullException("url");
            if (string.IsNullOrEmpty(fullPathWhereToSave)) throw new ArgumentNullException("fullPathWhereToSave");

            this._url = url;
            this._fullPathWhereToSave = fullPathWhereToSave;

        }

        public bool StartDownload(int timeout)
        {
            try
            {
                System.IO.Directory.CreateDirectory(Path.GetDirectoryName(_fullPathWhereToSave));

                if (File.Exists(_fullPathWhereToSave))
                {
                    File.Delete(_fullPathWhereToSave);
                }
                using (WebClient client = new WebClient())
                {
                    
                    client.Headers.Add("ContentType", "application/pdf; charset=utf-8");
                    //System.Web.HttpContext.Current.Response.AppendHeader("Content-Length", ms.Length.ToString());

                    var ur = new Uri(_url);
                    // client.Credentials = new NetworkCredential("username", "password");
                    client.DownloadProgressChanged += WebClientDownloadProgressChanged;
                    client.DownloadFileCompleted += WebClientDownloadCompleted;
                    Console.WriteLine(@"Downloading file:");
                    client.DownloadFileAsync(ur, _fullPathWhereToSave);
                    _semaphore.Wait(timeout);
                    return _result = true && File.Exists(_fullPathWhereToSave);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Was not able to download file!");
                Console.Write(e);
                return false;
            }
            finally
            {
                //  this._semaphore.Dispose();
            }
        }

        private void WebClientDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            Console.Write("\r     -->    {0}%.", e.ProgressPercentage);
        }

        private void WebClientDownloadCompleted(object sender, AsyncCompletedEventArgs args)
        {
            _result = !args.Cancelled;
            if (!_result)
            {
                Console.Write(args.Error.ToString());
            }
            Console.WriteLine(Environment.NewLine + "Download finished!");

            if (_semaphore != null)
                _semaphore.Release();
        }

        //public static bool DownloadFile(string url, string fullPathWhereToSave, int timeoutInMilliSec)
        //{
        //    return new FileDownloader(url, fullPathWhereToSave).StartDownload(timeoutInMilliSec);
        //}

        #region Stream Download


        public bool StreamDownload(string url, string fullPathWhereToSave)
        {
            try
            {
                Console.WriteLine(fullPathWhereToSave);
                ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;


                if (File.Exists(fullPathWhereToSave))
                {
                    File.Delete(fullPathWhereToSave);
                }

                client = new WebClient();
                client.Headers.Add(HttpRequestHeader.Cookie, "DIBBSDoDWarning=AGREE");

                //client.DownloadProgressChanged += WebClientDownloadProgressChanged;
                //client.DownloadFileCompleted += WebClientDownloadCompleted;

                byte[] myDataBuffer = client.DownloadData((new Uri(url)));

                MemoryStream storeStream = new MemoryStream();

                storeStream.SetLength(myDataBuffer.Length);
                storeStream.Write(myDataBuffer, 0, (int)storeStream.Length);

                storeStream.Flush();

                //TO save into certain file must exist on Local
                SaveMemoryStream(storeStream, fullPathWhereToSave);

                if (File.Exists(fullPathWhereToSave))
                {
                    FileInfo f = new FileInfo(fullPathWhereToSave);
                    if (f.Length > 2)
                    {
                        Console.WriteLine("\r Download Finished !");
                        return true;
                    }
                }

                //The below Getstring method to get data in raw format and manipulate it as per requirement
                //string download = Encoding.ASCII.GetString(myDataBuffer);

                //Console.WriteLine(download);
                //Console.ReadLine();

                return false;
            }
            catch (Exception ex)
            {
                //Logger _log = BaseLog.Instance.GetLogger(null) ;
                //if (_log != null)
                //{
                //    _log.Info("Error while StreamDownload");
                //    _log.Error(ex);
                //}
                return false;
            }
        }


        #endregion

    }
}
