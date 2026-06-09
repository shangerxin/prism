using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Text.Json;

using prism.model.Model;
using prism.web.service.Model;
using Microsoft.Practices.EnterpriseLibrary.Common.Utility;
using System.Threading.Tasks;

namespace prism.web.service.Controller
{
    public class TestBuildController : PrismControllerBase<TestBuild>
    {
        public TestBuildController()
        {
        }

        protected override object ToSerizalizable(TestBuild x)
        {
            return new
            {
                x.id,
                x.guid,
                x.testJobId,
                x.buildResultId,
                x.testResultId,
                x.startTime,
                x.endTime
            };
        }

        protected int? getTestJobId(string testJobName)
        {
            return managementDb.TestJobs.Where(p => p.name == testJobName).FirstOrDefault()?.id ?? null;
        }

        // GET api/<controller>
        public string Get()
        {
            using(managementDb)
            {
                var results = managementDb.TestBuilds.ToList().Select(t => this.ToSerizalizable(t));
                return JsonSerializer.Serialize(results);
            }
        }

        // GET api/<controller>/5
        public string Get(int id)
        {
            using (managementDb)
            {
                var testBuild = managementDb.TestBuilds.Where(p => p.id == id).FirstOrDefault();
                if(testBuild == null)
                {
                    return null;
                }
                else
                {
                    var result = ToSerizalizable(testBuild);
                    return JsonSerializer.Serialize(result);
                }
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
        [Route(ServiceHelper.ApiPrefix + "/TestBuild/LastSuccess/{testJobName}")]
        public async Task<HttpResponseMessage> LastSuccess(string testJobName)
        {
            using (managementDb)
            {
                var testJobId = getTestJobId(testJobName);
                if (testJobId == null) { return null; }
                var lastSuccess = (from testBuild in managementDb.TestBuilds
                                   where testJobId == testBuild.testJobId && testBuild.buildResultId == (int)ResultTypes.Pass
                                   orderby testBuild.timestamp descending 
                                   select testBuild).FirstOrDefault();
                return toResponse(Serizalize(lastSuccess));
            }
        }

        [HttpGet]
        [Route(ServiceHelper.ApiPrefix + "/TestBuild/LastSuccess/{testJobName}/{testResultType}")]
        public async Task<HttpResponseMessage> Last(string testJobName, string testResultType)
        {
            using (managementDb)
            {
                var testJobId = getTestJobId(testJobName);
                var resultType = managementDb.ResultTypes.Where(r => r.name == testResultType).FirstOrDefault();
                if (testJobId == null || resultType == null) { return null; }
                var last = (from testBuild in managementDb.TestBuilds
                            where testJobId == testBuild.testJobId && testBuild.testResultId == resultType.id
                            orderby testBuild.timestamp descending
                            select testBuild).FirstOrDefault();
                return toResponse(Serizalize(last));
            }
        }

        [HttpGet]
        [Route(ServiceHelper.ApiPrefix + "/TestBuild/BuildList/{testJobName}/{start}/{end}")]
        public async Task<HttpResponseMessage> BuildList(string testJobName, DateTime start, DateTime end)
        {
            using (managementDb)
            {
                var testJobId = managementDb.TestJobs.Where(p => p.name == testJobName).FirstOrDefault()?.id ?? null;
                if (testJobId == null) { return null; }
                var buildList = (from testBuild in managementDb.TestBuilds
                                 where testBuild.testJobId == testJobId && testBuild.timestamp >= start && testBuild.timestamp <= end
                                 orderby testBuild.timestamp
                                 select testBuild).ToList();
                return toResponse(JsonSerializer.Serialize(buildList.Select(t => ToSerizalizable(t))));
            }
        }

        // POST api/<controller>
        public TestBuild Post([FromBody] string value)
        {
            var testBuild = JsonSerializer.Deserialize<TestBuild>(value);
            using (managementDb)
            {      
                testBuild.timestamp = testBuild.startTime?? DateTime.Now;
                managementDb.TestBuilds.Add(testBuild);
                managementDb.SaveChanges();
            }
            return testBuild;
        }

        // PUT api/<controller>/5
        public bool Put(int id, [FromBody] string value)
        {
            var testBuild = JsonSerializer.Deserialize<TestBuild>(value);
            using (managementDb)
            {
                var existingTestBuild = managementDb.TestBuilds.SingleOrDefault(t => t.guid == testBuild.guid || t.id == id);
                if (existingTestBuild != null)
                {
                    existingTestBuild.testJobId = testBuild.testJobId;
                    existingTestBuild.buildResultId = testBuild.buildResultId;
                    existingTestBuild.testResultId = testBuild.testResultId;
                    existingTestBuild.startTime = testBuild.startTime;
                    existingTestBuild.endTime = testBuild.endTime;
                    managementDb.SaveChanges();
                    return true;
                }
            }
            return false;
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