using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web.Http;

namespace prism.web.service
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            var apiVersion = ServiceHelper.GetApiVersion();
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();
            config.Formatters.Add(new BsonMediaTypeFormatter());
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: $"api/v{apiVersion}/{{controller}}/{{id}}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
