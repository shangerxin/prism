using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Text.Json;

using prism.model.Model;

namespace prism.web.service.Controller
{
    public class TestBuildController : PrismControllerBase
    {
        public TestBuildController()
        {
        }

        // GET api/<controller>
        public List<TestBuild> Get()
        {
            using(_managementDb)
            {
                return _managementDb.TestBuilds.ToList();
            }
        }

        // GET api/<controller>/5
        public TestBuild Get(int id)
        {
            using (_managementDb)
            {
                return _managementDb.TestBuilds.Where(p => p.id == id).FirstOrDefault();
            }
        }

        [HttpGet]
        [Route(ServiceHelper.ApiPrefix + "/TestBuild/Id/{guid}")]
        public string Id(string guid)
        {
            using (_managementDb)
            {
                if (Guid.TryParse(guid, out Guid parsedGuid))
                {
                    return _managementDb.TestBuilds.Where(p => p.guid == parsedGuid).FirstOrDefault()?.id.ToString();
                }
                return null;
            }
        }

        // POST api/<controller>
        public void Post([FromBody] string value)
        {
            var testBuild = JsonSerializer.Deserialize<TestBuild>(value);
            using (_managementDb)
            {
                _managementDb.TestBuilds.Add(testBuild);
                _managementDb.SaveChanges();
            }
        }

        // PUT api/<controller>/5
        public void Put(int id, [FromBody] string value)
        {
            var testBuild = JsonSerializer.Deserialize<TestBuild>(value);
            using (_managementDb)
            {
                var existingTestBuild = _managementDb.TestBuilds.SingleOrDefault(t => t.id == id);
                if (existingTestBuild != null)
                {
                    existingTestBuild.testJobId = testBuild.testJobId;
                    existingTestBuild.buildResultId = testBuild.buildResultId;
                    existingTestBuild.testResultId = testBuild.testResultId;
                    existingTestBuild.startTime = testBuild.startTime;
                    existingTestBuild.endTime = testBuild.endTime;
                    _managementDb.SaveChanges();
                }
            }
        }

        // DELETE api/<controller>/5
        public void Delete(int id)
        {
            using (_managementDb)
            {
                var testBuild = _managementDb.TestBuilds.SingleOrDefault(t => t.id == id);
                if (testBuild != null)
                {
                    _managementDb.TestBuilds.Remove(testBuild);
                    _managementDb.SaveChanges();
                }
            }
        }
    }
}