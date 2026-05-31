using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace prism.web.service
{
    public static class ServiceHelper
    {
        private static string _apiVersion;
        public static string GetApiVersion()
        {
            if (_apiVersion == null)
            {
                _apiVersion = System.Configuration.ConfigurationManager.AppSettings["prismapi:Version"];
                if (ApiPrefix != $"api/v{_apiVersion}")
                {
                    throw new InvalidOperationException("API version mismatch in the code vs web configuration file.");
                }
            }
            return _apiVersion;
        }

        public const string ApiPrefix = "api/v1";
    }
}