using CosmicCoreApi.Helper;
using Flexis.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;



namespace CosmicCoreApi
{
    public static class WebApiConfig
    {
        
        public static void Register(HttpConfiguration config)
        {

            var cors = new System.Web.Http.Cors.EnableCorsAttribute("*", "*", "*");
            config.EnableCors(cors);


           

            // Web API configuration and services
            // Configure Web API to use only bearer token authentication.

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Formatters.JsonFormatter.SupportedMediaTypes.Add(new System.Net.Http.Headers.MediaTypeHeaderValue("text/html"));

            config.Routes.MapHttpRoute(
               name: "ControllerAndAction",
               routeTemplate: "api/{controller}/{action}/{id}",
               defaults: new { id = RouteParameter.Optional });

            string serverActualPath = System.Web.Hosting.HostingEnvironment.MapPath("~/Log");
            Logger _log = BaseLog.Instance.GetLogger(null);
            serverActualPath = serverActualPath + "\\cosmicBillsErrorLog.txt";
            BaseLog.Instance.SetLogFile(serverActualPath);

            CosmicLogger.LogFilePath = serverActualPath;


        }
    }
}
