using prism.model.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace prism.web.service.Controller
{
    public abstract class PrismControllerBase: ApiController
    {
        protected TestManagementDBEntities _db;
        public PrismControllerBase()
        {
            _db = new TestManagementDBEntities();
        }
    }
}