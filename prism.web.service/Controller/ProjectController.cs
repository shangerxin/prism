using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using prism.model.Model;

namespace prism.web.service.Controller
{
    public class ProjectController : PrismControllerBase
    {
        public ProjectController()
        {
        }

        // GET: api/Project/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/Project
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/Project/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Project/5
        public void Delete(int id)
        {
        }
    }
}
