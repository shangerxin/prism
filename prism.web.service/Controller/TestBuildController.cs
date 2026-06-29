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
using MongoDB.Bson;

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
                guid = x.guid.ToString(),
                x.testJobId,
                x.buildResultId,
                x.testResultId,
                x.startTime,
                x.endTime
            };
        }

        protected int? getTestJobId(string testJobName)
        {
            return ManagementDb.TestJobs.Where(p => p.name == testJobName).FirstOrDefault()?.id ?? null;
        }

        // GET api/<controller>
        public async Task<HttpResponseMessage> Get()
        {
            using (ManagementDb)
            {
                var results = ManagementDb.TestBuilds.Select(t => this.ToSerizalizable(t));
                return toResponse(JsonSerializer.Serialize(results));
            }
        }

        // GET api/<controller>/5
        public async Task<HttpResponseMessage> Get(int id)
        {
            using (ManagementDb)
            {
                var testBuild = ManagementDb.TestBuilds.Where(p => p.id == id).FirstOrDefault();
                if (testBuild == null)
                {
                    return ResponseNotFound;
                }
                else
                {
                    var result = ToSerizalizable(testBuild).ToJson();
                    return toResponse(result);
                }
            }
        }

        [HttpGet]
        [Route(ServiceHelper.ApiPrefix + "/TestBuild/Id/{guid}")]
        public async Task<HttpResponseMessage> Id(string guid)
        {
            using (ManagementDb)
            {
                if (Guid.TryParse(guid, out Guid parsedGuid))
                {
                    return toResponse(ManagementDb.TestBuilds.Where(p => p.guid == parsedGuid).FirstOrDefault()?.id.ToString());
                }
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }
        }

        [HttpGet]
        [Route(ServiceHelper.ApiPrefix + "/TestBuild/GUID/{id}")]
        public async Task<HttpResponseMessage> GUID(int id)
        {
            using (ManagementDb)
            {
                var testBuild = ManagementDb.TestBuilds.Where(p => p.id == id).FirstOrDefault();
                if (testBuild == null)
                {
                    return ResponseNotFound;
                }
                else
                {
                    return toResponse(testBuild.guid.ToString());
                }
            }
        }


        [HttpGet]
        [Route(ServiceHelper.ApiPrefix + "/TestBuild/LastSuccess/{testJobName}")]
        public async Task<HttpResponseMessage> LastSuccess(string testJobName)
        {
            using (ManagementDb)
            {
                var testJobId = getTestJobId(testJobName);
                if (testJobId == null) { return null; }
                var lastSuccess = (from testBuild in ManagementDb.TestBuilds
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
            using (ManagementDb)
            {
                var testJobId = getTestJobId(testJobName);
                var resultType = ManagementDb.ResultTypes.Where(r => r.name == testResultType).FirstOrDefault();
                if (testJobId == null || resultType == null) { return null; }
                var last = (from testBuild in ManagementDb.TestBuilds
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
            using (ManagementDb)
            {
                var testJobId = ManagementDb.TestJobs.Where(p => p.name == testJobName).FirstOrDefault()?.id ?? null;
                if (testJobId == null) { return null; }
                var buildList = (from testBuild in ManagementDb.TestBuilds
                                 where testBuild.testJobId == testJobId && testBuild.timestamp >= start && testBuild.timestamp <= end
                                 orderby testBuild.timestamp
                                 select testBuild).ToList();
                return toResponse(JsonSerializer.Serialize(buildList.Select(t => ToSerizalizable(t))));
            }
        }

        // POST api/<controller>
        public async Task<HttpResponseMessage> Post([FromBody] string value)
        {
            var testBuild = JsonSerializer.Deserialize<TestBuild>(value);
            using (ManagementDb)
            {
                testBuild.timestamp = testBuild.startTime ?? DateTime.Now;
                ManagementDb.TestBuilds.Add(testBuild);
                ManagementDb.SaveChanges();
            }
            return toResponse(Serizalize(testBuild));
        }

        // PUT api/<controller>/5
        public async Task<HttpResponseMessage> Put(int id, [FromBody] string value)
        {
            var testBuild = JsonSerializer.Deserialize<TestBuild>(value);
            using (ManagementDb)
            {
                var existingTestBuild = ManagementDb.TestBuilds.SingleOrDefault(t => t.guid == testBuild.guid || t.id == id);
                if (existingTestBuild != null)
                {
                    existingTestBuild.testJobId = testBuild.testJobId;
                    existingTestBuild.buildResultId = testBuild.buildResultId;
                    existingTestBuild.testResultId = testBuild.testResultId;
                    existingTestBuild.startTime = testBuild.startTime;
                    existingTestBuild.endTime = testBuild.endTime;
                    ManagementDb.SaveChanges();
                    return ResponseOK;
                }
            }
            return ResponseNotFound;
        }

        // DELETE api/<controller>/5
        public async Task<HttpResponseMessage> Delete(int id)
        {
            using (ManagementDb)
            {
                var testBuild = ManagementDb.TestBuilds.SingleOrDefault(t => t.id == id);
                if (testBuild != null)
                {
                    ManagementDb.TestBuilds.Remove(testBuild);
                    ManagementDb.SaveChanges();
                    return ResponseOK;
                }
            }
            return ResponseNotFound;
        }
    }
}