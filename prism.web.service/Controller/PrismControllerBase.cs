using prism.model.Model;
using prism.web.service.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Web;
using System.Web.Http;

namespace prism.web.service.Controller
{
    public abstract class PrismControllerBase: ApiController
    {
        protected TestManagementDBEntities managementDb { 
            get { 
                return new TestManagementDBEntities();
            } 
        }

        protected PrismTestResultsContext resultDb
        {
            get
            {
                return new PrismTestResultsContext();
            }
        }
    }
}