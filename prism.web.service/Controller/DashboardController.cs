using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace prism.web.service.Controller
{
    public class DashboardController : PrismControllerBase
    {
        // GET: api/v{apiVersion}/Dashboard
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/v{apiVersion}/Dashboard/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/v{apiVersion}/Dashboard
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/v{apiVersion}/Dashboard/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/v{apiVersion}/Dashboard/5
        public void Delete(int id)
        {
        }
    }
}
