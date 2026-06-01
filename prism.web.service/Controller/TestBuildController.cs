using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Text.Json;

using prism.model.Model;
using prism.web.service.Model;

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
            using(managementDb)
            {
                return managementDb.TestBuilds.ToList();
            }
        }

        // GET api/<controller>/5
        public TestBuild Get(int id)
        {
            using (managementDb)
            {
                return managementDb.TestBuilds.Where(p => p.id == id).FirstOrDefault();
            }
        }

        [HttpGet]
        [Route(ServiceHelper.ApiPrefix + "/TestBuild/Id/{guid}")]
        public string Id(string guid)
        {
            using (managementDb)
            {
                if (Guid.TryParse(guid, out Guid parsedGuid))
                {
                    return managementDb.TestBuilds.Where(p => p.guid == parsedGuid).FirstOrDefault()?.id.ToString();
                }
                return null;
            }
        }

        [HttpGet]
        [Route(ServiceHelper.ApiPrefix + "/TestBuild/GUID/{id}")]
        public string GUID(int id)
        {
            using (managementDb)
            {
                return managementDb.TestBuilds.Where(p => p.id == id).FirstOrDefault()?.guid.ToString();
            }
        }


        [HttpGet]
        public TestBuild LastSuccess(string testJobName)
        {
            using (managementDb)
            {
                var testJobId = managementDb.TestJobs.Where(p => p.name == testJobName).FirstOrDefault()?.id ?? null;
                if (testJobId == null) { return null; }
                var lastSuccess = (from testBuild in managementDb.TestBuilds
                                   where testJobId == testBuild.testJobId && testBuild.buildResultId == (int)ResultTypes.Pass
                                   orderby testBuild.timestamp descending 
                                   select testBuild).FirstOrDefault();
                return lastSuccess;
            }
        }

        [HttpGet]
        public List<TestBuild> BuildList(string testJobName, DateTime start, DateTime end)
        {
            using (managementDb)
            {
                var testJobId = managementDb.TestJobs.Where(p => p.name == testJobName).FirstOrDefault()?.id ?? null;
                if (testJobId == null) { return null; }
                var buildList = (from testBuild in managementDb.TestBuilds
                                 where testBuild.testJobId == testJobId && testBuild.timestamp >= start && testBuild.timestamp <= end
                                 orderby testBuild.timestamp
                                 select testBuild).ToList();
                return buildList;
            }
        }

        // POST api/<controller>
        public void Post([FromBody] string value)
        {
            var testBuild = JsonSerializer.Deserialize<TestBuild>(value);
            using (managementDb)
            {
                managementDb.TestBuilds.Add(testBuild);
                managementDb.SaveChanges();
            }
        }

        // PUT api/<controller>/5
        public void Put(int id, [FromBody] string value)
        {
            var testBuild = JsonSerializer.Deserialize<TestBuild>(value);
            using (managementDb)
            {
                var existingTestBuild = managementDb.TestBuilds.SingleOrDefault(t => t.id == id);
                if (existingTestBuild != null)
                {
                    existingTestBuild.testJobId = testBuild.testJobId;
                    existingTestBuild.buildResultId = testBuild.buildResultId;
                    existingTestBuild.testResultId = testBuild.testResultId;
                    existingTestBuild.startTime = testBuild.startTime;
                    existingTestBuild.endTime = testBuild.endTime;
                    managementDb.SaveChanges();
                }
            }
        }

        // DELETE api/<controller>/5
        public void Delete(int id)
        {
            using (managementDb)
            {
                var testBuild = managementDb.TestBuilds.SingleOrDefault(t => t.id == id);
                if (testBuild != null)
                {
                    managementDb.TestBuilds.Remove(testBuild);
                    managementDb.SaveChanges();
                }
            }
        }
    }
}