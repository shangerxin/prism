using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using prism.model.Model;

namespace prism.web.service.Controller
{
    public class TestResultController : PrismControllerBase
    {
        public TestResultController()
        {
        }

        // GET api/<controller>
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<controller>/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<controller>
        public void Post([FromBody] string value)
        {
            //create
        }

        // PUT api/<controller>/5
        public void Put(int id, [FromBody] string value)
        {
            //replace
        }

        // DELETE api/<controller>/5
        public void Delete(int id)
        {
        }
    }
}