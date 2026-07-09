
using prism.infra.Enum;
using prism.infra.Extension;
using prism.infra.WebAPI;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace prism.web.service.Controller
{
    public class BackupController : PrismControllerBase<Object>
    {
        protected string _backupRoot = ((PrismWebAPIConfigSection)ConfigurationManager.GetSection("PrismWebAPIConfig")).BackupRoot.Path;
        protected string _sqlServerDB = ((PrismWebAPIConfigSection)ConfigurationManager.GetSection("PrismWebAPIConfig")).BackupRoot.SqlServerDB;
        protected string _mongoDB = ((PrismWebAPIConfigSection)ConfigurationManager.GetSection("PrismWebAPIConfig")).BackupRoot.MongoDB;

        [HttpGet]
        [Route(ServiceHelper.ApiPrefix + "/Backup/Items")]
        public IEnumerable<string> Items()
        {
            return Enum.GetNames(typeof(BackupItemTypes));
        }

        [HttpPost]
        [Route(ServiceHelper.ApiPrefix + "/Backup/Database/{item}")]
        public void Database(string item)
        {
            var backupItemType = item.ToEnum<BackupItemTypes>();
            
        }

        protected override object ToSerizalizable(object x)
        {
            throw new NotImplementedException();
        }
    }
}