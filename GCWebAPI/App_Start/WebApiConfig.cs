﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace GCWebAPI
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "action",
                routeTemplate: "api/{controller}/{action}/{server}/{point}",
                defaults: new { server = RouteParameter.Optional, point = RouteParameter.Optional }
            );

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            

            //Allows response in Json format 
            GlobalConfiguration.Configuration.Formatters.JsonFormatter.MediaTypeMappings.Add(new System.Net.Http.Formatting.RequestHeaderMapping("Accept",
                                                                                                                                                  "text/html",
                                                                                                                                                  StringComparison.InvariantCultureIgnoreCase,
                                                                                                                                                  true,
                                                                                                                                                  "application/json"));
        }
    }
}
