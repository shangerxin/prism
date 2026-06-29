using prism.model.Model;
using prism.web.service.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Web;
using System.Web.Http;

namespace prism.web.service.Controller
{
    public abstract class PrismControllerBase<T>: ApiController
    {   
        protected HttpResponseMessage ResponseNotFound { get; set; } = new HttpResponseMessage(HttpStatusCode.NotFound);
        protected HttpResponseMessage ResponseOK { get; set; } = new HttpResponseMessage(HttpStatusCode.OK);
        protected TestManagementDBEntities _managementDB;
        protected TestManagementDBEntities ManagementDb { 
            get { 
                if(_managementDB == null)
                {
                    _managementDB = new TestManagementDBEntities();
                }
                return _managementDB;
            } 
        }

        protected PrismTestResultsContext ResultDb
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

        protected HttpResponseMessage toResponse(string content, HttpStatusCode code = HttpStatusCode.OK, string metadataType="application/json")
        {
            if(string.IsNullOrEmpty(content)) 
            {
                return ResponseNotFound;
            }

            var response = new HttpResponseMessage(code);
            response.Content = new StringContent(content, Encoding.UTF8, metadataType);
            return response;
        }
    }
}