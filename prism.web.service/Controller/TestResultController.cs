using MongoDB.Bson;
using prism.model.Model;
using prism.web.service.Model;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
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
            var data = json["data"];
            var isValidData = data is JsonObject || data is JsonValue || data is JsonArray;
            if (projectName == null || testJobName == null || buildGuid == null || dataInfo == null || !isValidData)
            {
                throw new Exception("The fields, project name, test job name, buildGuid, dataInfo or data are null.");
            }
            return json;
        }

        [HttpGet]
        [Route(ServiceHelper.ApiPrefix + "/TestResult/{projectName}/{testJobName}/{buildGuid}/{dataInfo}")]
        public async Task<HttpResponseMessage> GetResult(string projectName, string testJobName, string buildGuid, string dataInfo)
        {
            using (ResultDb)
            {
                var filterJson = JsonSerializer.Serialize(new { projectName, testJobName, buildGuid, dataInfo });
                var result = await ResultDb.GetResult(filterJson);

                return toResponse(result);
            }
        }

        [HttpGet]
        [Route(ServiceHelper.ApiPrefix + "/TestResult/Environment/{projectName}/{testJobName}/{buildGuid}/{dataInfo}")]
        public async Task<HttpResponseMessage> GetEnvironment(string projectName, string testJobName, string buildGuid, string dataInfo)
        {
            using (ResultDb)
            {
                var filterJson = JsonSerializer.Serialize(new { projectName, testJobName, buildGuid, dataInfo });
                var environment = await ResultDb.GetEnvironment(filterJson);
                return toResponse(environment);
            }
        }

        public async Task<List<BsonDocument>> GetEnvironments(string projectName, string testJobName, List<string> buildGuids, string dataInfo)
        {
            using (ResultDb)
            {
                var filterJson = buildGuids.Select(buildGuid => JsonSerializer.Serialize(new { projectName, testJobName, buildGuid, dataInfo })).ToList();
                var environments = await ResultDb.GetBsonEnvironments(filterJson);
                return environments;
            }
        }

        [HttpGet]
        [Route(ServiceHelper.ApiPrefix + "/TestResult/Parameter/{projectName}/{testJobName}/{buildGuid}/{dataInfo}")]
        public async Task<HttpResponseMessage> GetParameter(string projectName, string testJobName, string buildGuid, string dataInfo)
        {
            using (ResultDb)
            {
                var filterJson = JsonSerializer.Serialize(new { projectName, testJobName, buildGuid, dataInfo });
                var parameter = await ResultDb.GetParameter(filterJson);
                return toResponse(parameter);
            }
        }

        public async Task<List<BsonDocument>> GetParameters(string projectName, string testJobName, List<string> buildGuids, string dataInfo)
        {
            using (ResultDb)
            {
                var filterJson = buildGuids.Select(buildGuid => JsonSerializer.Serialize(new { projectName, testJobName, buildGuid, dataInfo })).ToList();
                var parameters = await ResultDb.GetBsonParameters(filterJson);
                return parameters;
            }
        }


        [HttpGet]
        [Route(ServiceHelper.ApiPrefix + "/TestResult/Metadata/{projectName}/{testJobName}/{buildGuid}/{dataInfo}")]
        public async Task<HttpResponseMessage> GetMetadata(string projectName, string testJobName, string buildGuid, string dataInfo)
        {
            using (ResultDb)
            {
                var filterJson = JsonSerializer.Serialize(new { projectName, testJobName, buildGuid, dataInfo });
                var metadata = await ResultDb.GetBsonMetadata(new List<string> { filterJson });
                return toResponse(metadata.ToJson());
            }
        }

        public async Task<List<BsonDocument>> GetMetadata(string projectName, string testJobName, List<string> buildGuids, string dataInfo)
        {
            using (ResultDb)
            {
                var filterJson = buildGuids.Select(buildGuid => JsonSerializer.Serialize(new { projectName, testJobName, buildGuid, dataInfo })).ToList();
                var metadata = await ResultDb.GetBsonMetadata(filterJson);
                return metadata;
            }
        }

        [HttpPost]
        [Route(ServiceHelper.ApiPrefix + "/TestResult/GetResults/")]

        public async Task<HttpResponseMessage> GetResults([FromBody] string query)
        {
            var json = parseQuery(query);
            var projectName = json["projectName"].GetValue<string>();
            var testJobName = json["testJobName"].GetValue<string>();
            var buildGuids = json["buildGuids"].GetValue<List<string>>();
            var dataInfo = json["dataInfo"].GetValue<string>();
            var queries = buildGuids.Select(x => JsonSerializer.Serialize(new { projectName, testJobName, buildGuid = x, dataInfo})).ToList<string>();
            var results = await ResultDb.GetBsonResults(queries);
            return toResponse(results.ToJson());
        }

        public async Task<List<BsonDocument>> GetResults(string projectName, string testJobName, List<string> buildGuids, string dataInfo)
        {
            var queries = buildGuids.Where(g => !string.IsNullOrEmpty(g)).Select(g => JsonSerializer.Serialize(new { projectName, testJobName, buildGuid = g, dataInfo })).ToList<string>();
            if(queries.Count == 0)
            {
                return new List<BsonDocument>();
            }
        
            var results = await ResultDb.GetBsonResults(queries);
            return results;
        }


        public async Task<List<BsonDocument>> GetResults(string projectName, string testJobName, string dataInfo, string dataColumnName, string dataColumnValue)
        {
            if(string.IsNullOrEmpty(dataColumnName) || string.IsNullOrEmpty(dataColumnValue))
            {
                return new List<BsonDocument>();
            }
            var query = JsonSerializer.Serialize(new { projectName, testJobName, dataColumnName, dataColumnValue, dataInfo });
            var results = await ResultDb.GetBsonResults(new List<string> { query });
            return results;
        }


        public async Task<bool> Add(string data, Func<string, Task<bool>> AddImp)
        {
            var rawBody = await this.Request.Content.ReadAsStringAsync();
            var json = parseResult(data);
            var projectName = json["projectName"].GetValue<string>();
            var testJobName = json["testJobName"].GetValue<string>();
            var buildGuid = json["buildGuid"].GetValue<string>();
            Guid.TryParse(buildGuid, out Guid parsedGuid);
            var buildResult = json["buildResult"]?.GetValue<string>();
            var testResult = json["testResult"]?.GetValue<string>();
            Enum.TryParse<ResultTypes>(buildResult, out ResultTypes buildResultType);
            Enum.TryParse<ResultTypes>(testResult, out ResultTypes testResultType);
            var startTime = json["start"]?.GetValue<DateTime>();
            var endTime = json["end"]?.GetValue<DateTime>();
            var timeoutHours = json["end"]?.GetValue<int>()?? 0;
            using (ManagementDb)
            {
                var testJob = (from job in ManagementDb.TestJobs
                               where job.Project != null &&
                                     job.Project.name.Trim() == projectName &&
                                     job.name == testJobName
                               select job).FirstOrDefault();
                if (testJob == null)
                {
                    var projectId = (from project in ManagementDb.Projects
                                     where project.name.Trim() == projectName
                                     select project).FirstOrDefault()?.id ?? null;
                    testJob = new TestJob
                    {
                        name = testJobName,
                        projectId = projectId
                    };
                    ManagementDb.TestJobs.Add(testJob);
                    ManagementDb.SaveChanges();
                }
                var testBuild = (from build in ManagementDb.TestBuilds
                                 where build.testJobId == testJob.id &&
                                       build.guid == parsedGuid
                                 select build).FirstOrDefault();
                if (testBuild == null)
                {
                    testBuild = new TestBuild
                    {
                        testJobId = testJob.id,
                        timestamp = startTime ?? DateTime.Now,
                        buildResultId = (int)buildResultType,
                        testResultId = (int)testResultType,
                        guid = parsedGuid,
                        startTime = startTime,
                        endTime = endTime,
                        timeoutHours = timeoutHours

                    };
                    ManagementDb.TestBuilds.Add(testBuild);
                    ManagementDb.SaveChanges();
                }
            }
            return await AddImp(data);
        }

        [HttpPost]
        [Route(ServiceHelper.ApiPrefix + "/TestResult/AddResult/")]
        public async Task<bool> AddTestResult([FromBody] string result)
        {
            return await Add(result, r => ResultDb.AddResult(r));

        }

        [HttpPost]
        [Route(ServiceHelper.ApiPrefix + "/TestResult/AddEnvirnoment/")]
        public async Task<bool> AddEnvironment([FromBody] string environment)
        {
            return await Add(environment, e => ResultDb.AddEnvironment(e));
        }


        [HttpPost]
        [Route(ServiceHelper.ApiPrefix + "/TestResult/AddParameter/")]

        public async Task<bool> AddParameter([FromBody] string parameter)
        {
            return await Add(parameter, p => ResultDb.AddParameter(p));
        }

        [HttpPost]
        [Route(ServiceHelper.ApiPrefix + "/TestResult/AddMetadata/")]
        public async Task<bool> AddMetadata([FromBody] string metadata)
        {
            return await Add(metadata, m => ResultDb.AddMetadata(m));
        }

        [HttpPost]
        [Route(ServiceHelper.ApiPrefix + "/TestResult/CompareResults/")]
        public async Task<HttpResponseMessage> CompareResults([FromBody] string results)
        {
            var json = JsonNode.Parse(results);
            var current = json["currentResult"].GetValue<string>();
            var compare = json["compareResult"].GetValue<string>();
            var reference = json["referenceResult"].GetValue<string>();
            throw new NotImplementedException();
        }

        [HttpGet]
        [Route(ServiceHelper.ApiPrefix + "/TestResult/CompareResults/")]
        public async Task<HttpResponseMessage> CompareResults(string projectName, string testJobName, string dataInfo, string currentBuildGuid, string compareBuildGuid, string referenceBuildGuid)
        {
            var current = GetResult(projectName, testJobName, currentBuildGuid, dataInfo);
            var compare = GetResult(projectName, testJobName, compareBuildGuid, dataInfo);
            var reference = GetResult(projectName, testJobName, referenceBuildGuid, dataInfo);
            throw new NotImplementedException();
        }


        protected override object ToSerizalizable(object x)
        {
            throw new NotImplementedException();
        }
    }
}
