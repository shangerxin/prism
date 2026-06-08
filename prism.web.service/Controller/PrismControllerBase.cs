using prism.model.Model;
using prism.web.service.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text.Json;
using System.Web;
using System.Web.Http;

namespace prism.web.service.Controller
{
    public abstract class PrismControllerBase<T>: ApiController
    {
        protected TestManagementDBEntities _managementDB;
        protected TestManagementDBEntities managementDb { 
            get { 
                if(_managementDB == null)
                {
                    _managementDB = new TestManagementDBEntities();
                }
                return _managementDB;
            } 
        }

        protected PrismTestResultsContext resultDb
        {
            get
            {
                return new PrismTestResultsContext();
            }
        }

        protected abstract Object ToSerizalizable(T x);

        protected string Serizalize(T x)
        {
            if(x == null) return null;
            return JsonSerializer.Serialize(ToSerizalizable(x));
        }
}
}