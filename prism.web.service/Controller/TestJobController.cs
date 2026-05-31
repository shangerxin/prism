using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace prism.web.service.Controller
{
    public class TestJobController : PrismControllerBase
    {
        // GET: api/TestJob
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/TestJob/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/TestJob
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/TestJob/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/TestJob/5
        public void Delete(int id)
        {
        }
    }
}
