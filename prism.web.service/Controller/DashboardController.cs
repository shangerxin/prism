using Microsoft.Practices.EnterpriseLibrary.Common.Utility;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using prism.infra.Enum;
using prism.infra.Extension;
using prism.infra.Helper;
using prism.model;
using prism.web.service.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
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

using prism.infra.WebAPI;
using System.Configuration;

namespace prism.web.service.Controller
{
    public class DashboardController : PrismControllerBase<Object>
    {
        protected int _defaultTakeCount = ((PrismWebAPIConfigSection)ConfigurationManager.GetSection("PrismWebAPIConfig")).DashboardSettings.DefaultTakeCount;
        protected TestResultController _testResultController = new TestResultController();
        #region Protected

        protected void InsertTimeStamp(TimeInfoTypes timeInfo, IEnumerable<QueryResultModel> buildInfo, List<BsonDocument> results)
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

        protected Boolean IsContains(List<BsonDocument> results, string columnName, string rootItem = "data")
        {
            return results.Count > 0 && results.All(r => r[rootItem].AsBsonArray.Values.OfType<BsonDocument>().All(d => d.Contains(columnName)));
        }

        protected Boolean IsContains(BsonDocument result, string columnName, string rootItem = "data")
        {
            return IsContains(new List<BsonDocument> { result }, columnName, rootItem);
        }

        protected string FisrtOrDefault(BsonDocument result, string columnName, string rootItem = "data", string defaultValue = "")
        {
            if (IsContains(result, columnName, rootItem))
            {
                return result[rootItem].AsBsonArray.Values.OfType<BsonDocument>().FirstOrDefault()?[columnName].ToString() ?? defaultValue;
            }
            return "";
        }

        protected void CalculateGeomeans(List<BsonDocument> results, List<string> colummnNames, List<string> addColumnNames = null)
        {
            foreach (var result in results)
            {
                CalculateGeomean(result, colummnNames, addColumnNames);
            }
        }

        protected void CalculateGeomean(BsonDocument result, List<string> colummnNames, List<string> addColumnNames)
        {
            if(result == null)
            {
                return;
            }

            result["__geomean__"] = new BsonDocument();
            var dataArray = result["data"].AsBsonArray;
            foreach (var columnName in colummnNames)
            {
                try
                {
                    var values = dataArray.Select(d => d.AsBsonDocument.Contains(columnName) && !String.IsNullOrWhiteSpace(d[columnName].ToString()) ? d[columnName].ToDouble() : 0.0);
                    var item = new BsonDocument();
                    var geomean = Calculator.Geomean(values.ToArray());
                    result["__geomean__"][columnName] = geomean;
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex);
                    if (IsContains(result, columnName))
                    {
                        result["__geomean__"][columnName] = FisrtOrDefault(result, columnName);
                    }
                    else
                    {
                        result["__geomean__"][columnName] = "N/A";
                    }
                }
            }

            addColumnNames?.ForEach(columnName =>
            {
                result["__geomean__"][columnName] = FisrtOrDefault(result, columnName);
            });
        }

        protected void CalculatePassrates(List<BsonDocument> results, List<string> columnNames, List<string> addColumnNames = null)
        {
            foreach (var result in results)
            {
                CalculatePassrate(result, columnNames, addColumnNames);
            }
        }

        protected void CalculatePassrate(BsonDocument result, List<string> columnNames, List<string> addColumnNames)
        {
            if (result == null)
            {
                return;
            }

            result["__passrate__"] = new BsonDocument();
            var dataArray = result["data"].AsBsonArray;
            foreach (var columnName in columnNames)
            {
                try
                {
                    var failedCount = dataArray.Count(d =>
                    {
                        var value = d.AsBsonDocument.Contains(columnName) ? d[columnName].AsString : string.Empty;
                        return String.IsNullOrWhiteSpace(value) || value.ToLower().Contains("fail");
                    });
                    var passrate = (double)(dataArray.Count - failedCount) / dataArray.Count;
                    result["__passrate__"][columnName] = passrate;
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex);
                    if (IsContains(result, columnName))
                    {
                        result["__passrate__"][columnName] = FisrtOrDefault(result, columnName);
                    }
                    else
                    {
                        result["__passrate__"][columnName] = "N/A";
                    }
                }
            }

            addColumnNames?.ForEach(columnName =>
            {
                result["__passrate__"][columnName] = FisrtOrDefault(result, columnName);
            });
        }

        protected List<BsonDocument> OrderBy(List<BsonDocument> results, string columnName, bool isDescending = false)
        {
            if (results.Count > 0 &&
               !String.IsNullOrEmpty(columnName) &&
               IsContains(results, columnName))
            {
                results = results.OrderBy(r => r["data"].AsBsonArray.FirstOrDefault()?[columnName].ToString() ?? "").ToList();
            }

            if (isDescending)
            {
                results.Reverse();
            }
            return results;
        }

        protected int DefaultTakeCount(int queryCount = 0)
        {
            return Math.Max(queryCount, _defaultTakeCount);
        }

        protected List<string> GetBuildGuids(string projectName, string testJobName, string orderBy, int count = 1, bool isOldToNew = false)
        {
            List<string> buildGuids = null;
            if (String.IsNullOrEmpty(orderBy))
            {
                buildGuids = (from build in managementDb.TestBuilds
                              where build.TestJob.name == testJobName && build.TestJob.Project.name == projectName
                              orderby build.startTime descending, build.timestamp descending
                              select build.guid.ToString()).Take(count).ToList();
            }
            else
            {
                buildGuids = (from build in managementDb.TestBuilds
                              where build.TestJob.name == testJobName && build.TestJob.Project.name == projectName
                              orderby build.startTime descending, build.timestamp descending
                              select build.guid.ToString()).Take(DefaultTakeCount(count)).ToList();
            }

            if (isOldToNew)
            {
                buildGuids.Reverse();
            }

            return buildGuids;
        }


        #endregion

        [HttpGet]
        [Route(ServiceHelper.ApiPrefix + "/Dashboard/GetGeomeans/{projectName}/{testJobName}/{dataInfo}/{start}/{end}/{columnNames}/{addColumnNames?}/{orderBy?}")]
        public async Task<HttpResponseMessage> GetGeomeans(string projectName, string testJobName, string dataInfo, DateTime start, DateTime end, string columnNames, string addColumnNames, string orderBy)
        {
            List<string> names = columnNames.SplitToList();
            List<string> addNames = addColumnNames.SplitToList();
            using (managementDb)
            {
                var buildGuids = (from build in managementDb.TestBuilds
                                  where build.TestJob.name == testJobName &&
                                  build.TestJob.Project.name == projectName &&
                                  build.startTime >= start && build.endTime <= end
                                  orderby build.startTime, build.timestamp
                                  select build.guid.ToString()).ToList();
                var results = await _testResultController.GetResults(projectName, testJobName, buildGuids, dataInfo);
                results = OrderBy(results, orderBy);
                CalculateGeomeans(results, names, addNames);
                return toResponse(results.ToJson());
            }
        }

        [HttpGet]
        [Route(ServiceHelper.ApiPrefix + "/Dashboard/GetLastGeomeans/{projectName}/{testJobName}/{dataInfo}/{columnNames}/{addColumnNames?}/{orderBy?}")]
        public async Task<HttpResponseMessage> GetLastGeomeans(string projectName, string testJobName, string dataInfo, string columnNames, string addColumnNames, string orderBy = null)
        {
            List<string> names = columnNames.SplitToList();
            List<string> addNames = addColumnNames.SplitToList();
            using (managementDb)
            {
                List<string> buildGuids = GetBuildGuids(projectName, testJobName, orderBy);
                var results = await _testResultController.GetResults(projectName, testJobName, buildGuids, dataInfo);
                var result = OrderBy(results, orderBy).FirstOrDefault();
                CalculateGeomean(result, names, addNames);
                return toResponse(result?.ToJson());
            }
        }

        [HttpGet]
        [Route(ServiceHelper.ApiPrefix + "/Dashboard/GetLastGeomeans/{projectName}/{testJobName}/{dataInfo}/{count}/{columnNames}/{addColumnNames?}/{orderBy?}")]
        public async Task<HttpResponseMessage> GetLastGeomeans(string projectName, string testJobName, string dataInfo, int count, string columnNames, string addColumnNames, string orderBy = null)
        {
            List<string> names = columnNames.SplitToList();
            List<string> addNames = addColumnNames.SplitToList();
            using (managementDb)
            {
                List<string> buildGuids = GetBuildGuids(projectName, testJobName, orderBy, count, true);
                var results = await _testResultController.GetResults(projectName, testJobName, buildGuids, dataInfo);
                results = OrderBy(results, orderBy).Take(count).ToList();
                CalculateGeomeans(results, names, addNames);
                return toResponse(results.ToJson());
            }
        }

        [HttpGet]
        [Route(ServiceHelper.ApiPrefix + "/Dashboard/GetPassrates/{projectName}/{testJobName}/{dataInfo}/{start}/{end}/{columnNames}/{addColumnNames?}/{orderBy?}")]
        public async Task<HttpResponseMessage> GetPassrates(string projectName, string testJobName, string dataInfo, DateTime start, DateTime end, string columnNames, string addColumnNames, string orderBy = null)
        {
            List<string> names = columnNames.SplitToList();
            List<string> addNames = addColumnNames.SplitToList();
            using (managementDb)
            {
                var buildGuids = (from build in managementDb.TestBuilds
                                  where build.TestJob.name == testJobName &&
                                  build.startTime >= start && build.endTime <= end &&
                                  build.TestJob.Project.name == projectName
                                  orderby build.startTime, build.timestamp
                                  select build.guid.ToString()).ToList();
                var results = await _testResultController.GetResults(projectName, testJobName, buildGuids, dataInfo);
                results = OrderBy(results, orderBy);
                CalculatePassrates(results, names, addNames);
                return toResponse(results.ToJson());
            }
        }

        [HttpGet]
        [Route(ServiceHelper.ApiPrefix + "/Dashboard/GetLastPassrates/{projectName}/{testJobName}/{dataInfo}/{columnNames}/{addColumnNames?}/{orderBy?}")]
        public async Task<HttpResponseMessage> GetLastPassrates(string projectName, string testJobName, string dataInfo, string columnNames, string addColumnNames, string orderBy = null)
        {
            List<string> names = columnNames.SplitToList();
            List<string> addNames = addColumnNames.SplitToList();
            using (managementDb)
            {
                var buildGuids = GetBuildGuids(projectName, testJobName, orderBy);
                var results = await _testResultController.GetResults(projectName, testJobName, buildGuids, dataInfo);
                var result = OrderBy(results, orderBy, true).FirstOrDefault();
                CalculatePassrate(result, names, addNames);
                return toResponse(result?.ToJson());
            }
        }

        [HttpGet]
        [Route(ServiceHelper.ApiPrefix + "/Dashboard/GetLastPassrates/{projectName}/{testJobName}/{dataInfo}/{count}/{columnNames}/{addColumnNames?}/{orderBy?}")]
        public async Task<HttpResponseMessage> GetLastPassrates(string projectName, string testJobName, string dataInfo, int count, string columnNames, string addColumnNames, string orderBy = null)
        {
            List<string> names = columnNames.SplitToList();
            List<string> addNames = addColumnNames.SplitToList();
            using (managementDb)
            {
                var buildGuids = GetBuildGuids(projectName, testJobName, orderBy, count, true);
                var results = await _testResultController.GetResults(projectName, testJobName, buildGuids, dataInfo);
                results = OrderBy(results, orderBy).Take(count).ToList();
                CalculatePassrates(results, names, addNames);
                return toResponse(results.ToJson());
            }
        }

        [HttpGet]
        [Route(ServiceHelper.ApiPrefix + "/Dashboard/GetResults/{projectName}/{testJobName}/{dataInfo}/{start}/{end}/{orderBy?}")]
        public async Task<HttpResponseMessage> GetResults(string projectName, string testJobName, string dataInfo, DateTime start, DateTime end, string orderBy = null)
        {
            using (managementDb)
            {
                var buildGuids = (from build in managementDb.TestBuilds
                                  where build.TestJob.name == testJobName &&
                                  build.TestJob.Project.name == projectName &&
                                  build.startTime >= start && build.endTime <= end
                                  orderby build.startTime, build.timestamp
                                  select build.guid.ToString()).ToList();
                var results = await _testResultController.GetResults(projectName, testJobName, buildGuids, dataInfo);
                results = OrderBy(results, orderBy);
                return toResponse(results.ToJson());
            }
        }

        [HttpGet]
        [Route(ServiceHelper.ApiPrefix + "/Dashboard/GetLastResults/{projectName}/{testJobName}/{dataInfo}/{count}/{orderBy?}")]
        public async Task<HttpResponseMessage> GetLastResults(string projectName, string testJobName, string dataInfo, int count, string orderBy = null)
        {
            using (managementDb)
            {
                var buildGuids = GetBuildGuids(projectName, testJobName, orderBy, count, true);
                var results = await _testResultController.GetResults(projectName, testJobName, buildGuids, dataInfo);
                results = OrderBy(results, orderBy).Take(count).ToList();
                return toResponse(results.ToJson());
            }
        }

        [HttpGet]
        [Route(ServiceHelper.ApiPrefix + "/Dashboard/GetLastResults/{projectName}/{testJobName}/{dataInfo}/{resultType}/{count}/{orderBy?}")]
        public async Task<HttpResponseMessage> GetLastResults(string projectName, string testJobName, string dataInfo, string resultType, int count, string orderBy = null)
        {
            using (managementDb)
            {
                Enum.TryParse<ResultTypes>(resultType, out ResultTypes testResultType);
                var buildGuids = (from build in managementDb.TestBuilds
                                  where build.TestJob.name == testJobName && 
                                  build.TestJob.Project.name == projectName && 
                                  build.testResultId == (int)testResultType
                                  orderby build.startTime descending, build.timestamp descending
                                  select build.guid.ToString()).Take(DefaultTakeCount(count)).ToList();
                buildGuids.Reverse();
                var results = await _testResultController.GetResults(projectName, testJobName, buildGuids, dataInfo);
                results = OrderBy(results, orderBy).Take(count).ToList();
                return toResponse(results.ToJson());
            }
        }

        [HttpGet]
        [Route(ServiceHelper.ApiPrefix + "/Dashboard/GetLastTimestampedResults/{projectName}/{testJobName}/{dataInfo}/{insertTimeInfo}/{count}/{orderBy?}")]
        public async Task<HttpResponseMessage> GetLastTimestampedResults(string projectName, string testJobName, string dataInfo, string insertTimeInfo, int count, string orderBy = null)
        {
            Enum.TryParse<TimeInfoTypes>(insertTimeInfo, out TimeInfoTypes timeInfo);
            using (managementDb)
            {
                var buildInfo = (from build in managementDb.TestBuilds
                                 where build.TestJob.name == testJobName && build.TestJob.Project.name == projectName
                                 orderby build.startTime descending, build.timestamp descending
                                 select new QueryResultModel { guid = build.guid.ToString(), timestamp = build.timestamp, startTime = build.startTime, endTime = build.endTime }).Take(DefaultTakeCount(count)).ToList();
                buildInfo.Reverse();
                var results = await _testResultController.GetResults(projectName, testJobName, buildInfo.Select(x => x.guid).ToList(), dataInfo);
                InsertTimeStamp(timeInfo, buildInfo, results);
                results = OrderBy(results, orderBy).Take(count).ToList();
                return toResponse(results.ToJson());
            }
        }

        [HttpGet]
        [Route(ServiceHelper.ApiPrefix + "/Dashboard/GetLastTimestampedResults/{projectName}/{testJobName}/{dataInfo}/{insertTimeInfo}/{start}/{end}/{orderBy?}")]
        public async Task<HttpResponseMessage> GetLastTimestampedResults(string projectName, string testJobName, string dataInfo, string insertTimeInfo, DateTime start, DateTime end, string orderBy = null)
        {
            Enum.TryParse<TimeInfoTypes>(insertTimeInfo, out TimeInfoTypes timeInfo);
            using (managementDb)
            {
                var buildInfo = (from build in managementDb.TestBuilds
                                 where build.TestJob.name == testJobName &&
                                 build.TestJob.Project.name == projectName &&
                                 build.startTime >= start && build.endTime <= end
                                 orderby build.startTime, build.timestamp
                                 select new QueryResultModel { guid = build.guid.ToString(), timestamp = build.timestamp, startTime = build.startTime, endTime = build.endTime });
                var results = await _testResultController.GetResults(projectName, testJobName, buildInfo.Select(x => x.guid).ToList(), dataInfo);
                results = OrderBy(results, orderBy);
                InsertTimeStamp(timeInfo, buildInfo, results);
                return toResponse(results.ToJson());
            }
        }

        public async Task<HttpResponseMessage> GetLastResults(string projectName, string testJobName, string dataInfo, int count, string resultType, [FromBody] ResultQueryModel query, string orderBy = null)
        {
            using (managementDb)
            {
                Enum.TryParse<ResultTypes>(resultType, out ResultTypes testResultType);
                var buildGuids = (from build in managementDb.TestBuilds
                                  where build.TestJob.name == testJobName && build.testResultId == (int)testResultType
                                  orderby build.timestamp descending
                                  select build.guid.ToString()).Take(count).ToList();
                buildGuids.Reverse();
                var results = await _testResultController.GetResults(projectName, testJobName, buildGuids, dataInfo);
                results = OrderBy(results, orderBy);
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
