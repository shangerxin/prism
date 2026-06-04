using prism.model.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;

namespace prism.web.service.Controller
{
    public class TestResultController : PrismControllerBase<Object>
    {
        protected JsonNode parseQuery(string query)
        {
            var json = JsonNode.Parse(query);
            var projectName = json["projectName"]?.GetValue<string>();
            var testJobName = json["testJobName"]?.GetValue<string>();
            var buildGuids = json["buildGuids"]?.GetValue<List<string>>();
            var dataInfo = json["dataInfo"]?.GetValue<string>();
            if (projectName == null || testJobName == null || buildGuids == null || dataInfo == null)
            {
                throw new Exception("The fields, project name, test job name, buildGuids, or dataInfo are null.");
            }
            return json;
        }

        protected JsonNode parseResult(string result)
        {
            var json = JsonNode.Parse(result);
            var projectName = json["projectName"]?.GetValue<string>();
            var testJobName = json["testJobName"]?.GetValue<string>();
            var buildGuid = json["buildGuid"]?.GetValue<string>();
            var dataInfo = json["dataInfo"]?.GetValue<string>();
            var isValidData = json["data"] is JsonObject;
            if (projectName == null || testJobName == null || buildGuid == null || dataInfo == null || !isValidData)
            {
                throw new Exception("The fields, project name, test job name, buildGuid, dataInfo or data are null.");
            }
            return json;
        }

        [HttpGet]
        [Route(ServiceHelper.ApiPrefix + "/TestResult/{projectName}/{testJobName}/{buildGuid}/{dataInfo}")]
        public async Task<string> GetResult(string projectName, string testJobName, string buildGuid, string dataInfo)
        {
            using (resultDb)
            {
                var filterJson = JsonSerializer.Serialize(new { projectName, testJobName, buildGuid, dataInfo });
                var result = await resultDb.GetResult(filterJson);

                return result;
            }
        }

        [HttpGet]
        [Route(ServiceHelper.ApiPrefix + "/TestResult/Environment/{projectName}/{testJobName}/{buildGuid}/{dataInfo}")]
        public async Task<string> GetEnvironment(string projectName, string testJobName, string buildGuid, string dataInfo)
        {
            using (resultDb)
            {
                var filterJson = JsonSerializer.Serialize(new { projectName, testJobName, buildGuid, dataInfo });
                var environment = await resultDb.GetEnvironment(filterJson);
                return environment;
            }
        }

        [HttpGet]
        [Route(ServiceHelper.ApiPrefix + "/TestResult/Parameter/{projectName}/{testJobName}/{buildGuid}/{dataInfo}")]
        public async Task<string> GetParameter(string projectName, string testJobName, string buildGuid, string dataInfo)
        {
            using (resultDb)
            {
                var filterJson = JsonSerializer.Serialize(new { projectName, testJobName, buildGuid, dataInfo });
                var parameter = await resultDb.GetParameter(filterJson);
                return parameter;
            }
        }

        [HttpGet]
        [Route(ServiceHelper.ApiPrefix + "/TestResult/Metadata/{projectName}/{testJobName}/{buildGuid}/{dataInfo}")]
        public async Task<string> GetMetadata(string projectName, string testJobName, string buildGuid, string dataInfo)
        {
            using (resultDb)
            {
                var filterJson = JsonSerializer.Serialize(new { projectName, testJobName, buildGuid, dataInfo });
                var metadata = await resultDb.GetMetadata(filterJson);
                return metadata;
            }
        }

        [HttpPost]
        [Route(ServiceHelper.ApiPrefix + "/TestResult/GetResults/")]

        public async Task<string> GetResults([FromBody] string query)
        {
            var json = parseQuery(query);
            var projectName = json["projectName"].GetValue<string>();
            var testJobName = json["testJobName"].GetValue<string>();
            var buildGuids = json["buildGuids"].GetValue<List<string>>();
            var dataInfo = json["dataInfo"].GetValue<string>();
            var queries = buildGuids.Select(x => JsonSerializer.Serialize(new { projectName, testJobName, buildGuid = x, dataInfo})).ToList<string>();
            return await resultDb.GetResults(queries);
        }

        public async Task<bool> Add(string data, Func<string, Task<bool>> AddImp)
        {
            var rawBody = await this.Request.Content.ReadAsStringAsync();
            var json = parseResult(data);
            var projectName = json["projectName"].GetValue<string>();
            var testJobName = json["testJobName"].GetValue<string>();
            using (managementDb)
            {
                var testJob = (from job in managementDb.TestJobs
                               where job.Project != null &&
                                     job.Project.name == projectName &&
                                     job.name == testJobName
                               select job).FirstOrDefault();
                if (testJob == null)
                {
                    var projectId = (from project in managementDb.Projects
                                     where project.name == projectName
                                     select project).FirstOrDefault()?.id ?? null;
                    managementDb.TestJobs.Add(new TestJob
                    {
                        name = testJobName,
                        projectId = projectId
                    });
                    managementDb.SaveChanges();
                }
            }
            return await AddImp(data);
        }

        [HttpPost]
        [Route(ServiceHelper.ApiPrefix + "/TestResult/AddResult/")]
        public async Task<bool> AddTestResult([FromBody] string result)
        {
            return await Add(result, r => resultDb.AddResult(r));

        }

        [HttpPost]
        [Route(ServiceHelper.ApiPrefix + "/TestResult/AddEnvirnoment/")]
        public async Task<bool> AddEnvironment([FromBody] string environment)
        {
            return await Add(environment, e => resultDb.AddEnvironment(e));
        }


        [HttpPost]
        [Route(ServiceHelper.ApiPrefix + "/TestResult/AddParameter/")]

        public async Task<bool> AddParameter([FromBody] string parameter)
        {
            return await Add(parameter, p => resultDb.AddParameter(p));
        }

        [HttpPost]
        [Route(ServiceHelper.ApiPrefix + "/TestResult/AddMetadata/")]
        public async Task<bool> AddMetadata([FromBody] string metadata)
        {
            return await Add(metadata, m => resultDb.AddMetadata(m));
        }


        protected override object ToSerizalizable(object x)
        {
            throw new NotImplementedException();
        }
    }
}
