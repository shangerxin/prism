using Microsoft.Practices.EnterpriseLibrary.Common.Utility;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using prism.infra.Enum;
using prism.model;
using prism.web.service.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using System.Web.UI.WebControls;

namespace prism.web.service.Controller
{
    public class DashboardController : PrismControllerBase<Object>
    {
        [HttpGet]
        [Route(ServiceHelper.ApiPrefix + "/Dashboard/GetResults/{projectName}/{testJobName}/{dataInfo}/{start}/{end}")]
        public async Task<HttpResponseMessage> GetResults(string projectName, string testJobName, string dataInfo, DateTime start, DateTime end)
        {
            using (managementDb)
            {
                var buildGuids = (from build in managementDb.TestBuilds
                                     where build.TestJob.name == testJobName &&
                                     build.startTime >= start && build.endTime <= end
                                     orderby build.startTime 
                                     select build.guid.ToString()).ToList();
                var testResultController = new TestResultController();
                var results = await testResultController.GetResults(projectName, testJobName, buildGuids, dataInfo);
                return toResponse(results.ToJson());
            }
        }


        [HttpGet]
        [Route(ServiceHelper.ApiPrefix + "/Dashboard/GetLastResults/{projectName}/{testJobName}/{dataInfo}/{count}/")]
        public async Task<HttpResponseMessage> GetLastResults(string projectName, string testJobName, string dataInfo, int count)
        {
            using (managementDb)
            {
                var buildGuids = (from build in managementDb.TestBuilds
                                  where build.TestJob.name == testJobName 
                                  orderby build.timestamp descending
                                  select build.guid).Take(count);
                var testResultController = new TestResultController();
                var results = await testResultController.GetResults(projectName, testJobName, buildGuids.Select(x=>x.ToString()).ToList(), dataInfo);
                return toResponse(results.ToJson());
            }
        }


        [HttpGet]
        [Route(ServiceHelper.ApiPrefix + "/Dashboard/GetLastResults/{projectName}/{testJobName}/{dataInfo}/{resultType}/{count}/")]
        public async Task<HttpResponseMessage> GetLastResults(string projectName, string testJobName, string dataInfo, string resultType, int count)
        {
            using (managementDb)
            {
                Enum.TryParse<ResultTypes>(resultType, out ResultTypes testResultType);
                var buildGuids = (from build in managementDb.TestBuilds
                                  where build.TestJob.name == testJobName && build.testResultId == (int) testResultType
                                  orderby build.timestamp descending
                                  select new { build.guid, build.timestamp }).Take(count).ToList();
                var testResultController = new TestResultController();

                var results = await testResultController.GetResults(projectName, testJobName, buildGuids.Select(x => x.guid.ToString()).ToList(), dataInfo);
                return toResponse(results.ToJson());
            }
        }

        private class QueryResult
        {
            public Guid guid { get; set; }
            public DateTime timestamp { get; set; }
            public DateTime? startTime { get; set; }
            public DateTime? endTime { get; set; }
        }

        private void InsertTimeStamp(TimeInfoTypes timeInfo, IQueryable<QueryResult> buildInfo, List<BsonDocument> results)
        {
            string getTime(string guid)
            {
                var info = buildInfo.First(x => x.guid.ToString() == guid);
                switch (timeInfo)
                {
                    case TimeInfoTypes.start:
                        return info.startTime?.ToString("o");
                    case TimeInfoTypes.end:
                        return info.endTime?.ToString("o");
                    case TimeInfoTypes.timestamp:
                    default:
                        return info.timestamp.ToString("o");
                }
            }

            var cache = new Dictionary<string, string>();
            foreach (var guid in buildInfo.Select(b => b.guid.ToString()))
            {
                cache.Add(guid, getTime(guid));
            }

            results.ForEach(r =>
            {
                r["data"].AsBsonArray.ForEach(value =>
                {
                    value["__timestamp__"] = cache[r["buildGuid"].AsString];
                });
            });
        }

        [HttpGet]
        [Route(ServiceHelper.ApiPrefix + "/Dashboard/GetLastTimestampedResults/{projectName}/{testJobName}/{dataInfo}/{insertTimeInfo}/{count}/")]
        public async Task<HttpResponseMessage> GetLastTimestampedResults(string projectName, string testJobName, string dataInfo, string insertTimeInfo, int count)
        {
            Enum.TryParse<TimeInfoTypes>(insertTimeInfo, out TimeInfoTypes timeInfo);
            using (managementDb)
            {
                var buildInfo = (from build in managementDb.TestBuilds
                                 where build.TestJob.name == testJobName
                                 orderby build.timestamp descending
                                 select new QueryResult { guid = build.guid, timestamp = build.timestamp, startTime = build.startTime, endTime = build.endTime }).Take(count);
                var testResultController = new TestResultController();
                var results = await testResultController.GetResults(projectName, testJobName, buildInfo.Select(x => x.guid.ToString()).ToList(), dataInfo);

                InsertTimeStamp(timeInfo, buildInfo, results);
                return toResponse(results.ToJson());
            }
        }

        [HttpGet]
        [Route(ServiceHelper.ApiPrefix + "/Dashboard/GetResults/{projectName}/{testJobName}/{dataInfo}/{start}/{end}")]
        public async Task<HttpResponseMessage> GetLastTimestampedResults(string projectName, string testJobName, string dataInfo, DateTime start, DateTime end)
        {
            using (managementDb)
            {
                var buildInfo = (from build in managementDb.TestBuilds
                                 where build.TestJob.name == testJobName &&
                                 build.startTime >= start && build.endTime <= end
                                 orderby build.startTime
                                 select new QueryResult { guid = build.guid, timestamp = build.timestamp, startTime = build.startTime, endTime = build.endTime });
                var testResultController = new TestResultController();
                var results = await testResultController.GetResults(projectName, testJobName, buildInfo.Select(x=>x.guid.ToString()).ToList(), dataInfo);
                InsertTimeStamp(TimeInfoTypes.start, buildInfo, results);
                return toResponse(results.ToJson());
            }
        }

        [HttpPost]
        [Route(ServiceHelper.ApiPrefix + "/Dashboard/GetLastResults/{projectName}/{testJobName}/{dataInfo}/{count}/{resultType}")]
        public async Task<HttpResponseMessage> GetLastResults(string projectName, string testJobName, string dataInfo, int count, string resultType, [FromBody] ResultQueryModel query)
        {
            using (managementDb)
            {
                Enum.TryParse<ResultTypes>(resultType, out ResultTypes testResultType);
                var buildGuids = (from build in managementDb.TestBuilds
                                  where build.TestJob.name == testJobName && build.testResultId == (int)testResultType
                                  orderby build.timestamp descending
                                  select build.guid).Take(count).ToList();
                var testResultController = new TestResultController();

                var results = await testResultController.GetResults(projectName, testJobName, buildGuids.Select(x => x.ToString()).ToList(), dataInfo);
                var colummns = (from column in query.SelectColumns
                select new
                {
                     Column = column,
                     Values = (from result in results
                               where result != null && result.GetValue(column, null) != null
                               select result.GetValue(column)).ToList()
                });

                throw new NotImplementedException();
            }
        }


        protected override object ToSerizalizable(object x)
        {
            throw new NotImplementedException();
        }
    }
}
