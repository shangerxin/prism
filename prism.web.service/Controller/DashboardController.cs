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
using prism.infra.Runner;

namespace prism.web.service.Controller
{
    public class DashboardController : PrismControllerBase<Object>
    {
        #region Fields
        protected int _defaultTakeCount = ((PrismWebAPIConfigSection)ConfigurationManager.GetSection("PrismWebAPIConfig")).DashboardSettings.DefaultTakeCount;
        protected string _prismBinaryRootPath = ((PrismWebAPIConfigSection)ConfigurationManager.GetSection("PrismWebAPIConfig")).PrismBinaryRoot.Path;
        protected string _condaBinaryPath = ((PrismWebAPIConfigSection)ConfigurationManager.GetSection("PrismWebAPIConfig")).CondaBinary.Path;
        protected TestResultController _testResultController = new TestResultController();
        protected CondaRunner _conda;
        protected CondaRunner Conda
        {
            get
            {
                if (_conda == null)
                {
                    _conda = new CondaRunner(_condaBinaryPath, Path.Combine(_prismBinaryRootPath, "Venv"));
                }
                return _conda;
            }
        }
        #endregion

        #region Protected Methods

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

        protected Boolean IsAnyContains(List<BsonDocument> results, string columnName, string rootItem = "data")
        {
            return results.Count > 0 && results.Any(r => r[rootItem].AsBsonArray.Values.OfType<BsonDocument>().Any(d => d.Contains(columnName)));
        }

        protected Boolean IsAnyContains(List<BsonDocument> results, string columnName, List<string> values, string rootItem = "data")
        {
            return results.Count > 0 && results.Any(r => r[rootItem].AsBsonArray.Values.OfType<BsonDocument>().Any(d => d.Contains(columnName) && values.Intersect(d[columnName].ToString().Split(',')).Any()));
        }

        protected Boolean IsAnyContains(BsonDocument result, string columnName, string rootItem = "data")
        {
            return IsAnyContains(new List<BsonDocument> { result }, columnName, rootItem);
        }

        protected Boolean IsAnyContains(BsonDocument result, string columnName, List<string> values, string rootItem = "data")
        {
            return IsAnyContains(new List<BsonDocument> { result }, columnName, values, rootItem);
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
            if (result == null)
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
                buildGuids = (from build in ManagementDb.TestBuilds
                              where build.TestJob.name == testJobName && build.TestJob.Project.name == projectName
                              orderby build.startTime descending, build.timestamp descending
                              select build.guid.ToString()).Take(count).ToList();
            }
            else
            {
                buildGuids = (from build in ManagementDb.TestBuilds
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
        [Route(ServiceHelper.ApiPrefix + "/Dashboard/GetDataColumnValues/{projectName}/{testJobName}/{dataInfo}/{columnName}/{isUnique}")]
        public async Task<HttpResponseMessage> GetDataColumnValues(string projectName, string testJobName, string dataInfo, string columnName, bool isUnique)
        {
            using (ManagementDb)
            {
                var buildGuids = (from build in ManagementDb.TestBuilds
                                  where build.TestJob.name == testJobName &&
                                  build.TestJob.Project.name == projectName
                                  orderby build.startTime, build.timestamp
                                  select build.guid.ToString()).ToList();
                var results = await _testResultController.GetResults(projectName, testJobName, buildGuids, dataInfo);
                if (results.Count > 0 && IsAnyContains(results, columnName))
                {
                    var values = results.SelectMany(r => r["data"].AsBsonArray.Values.OfType<BsonDocument>().Where(d => d.Contains(columnName)).Select(d => d[columnName]?.ToString()));
                    if (isUnique)
                    {
                        values = values.Distinct();
                    }
                    return toResponse(values.ToJson());
                }
                else
                {
                    return toResponse(new List<string>().ToJson());
                }
            }
        }

        [HttpGet]
        [Route(ServiceHelper.ApiPrefix + "/Dashboard/GetDataColumnValues/{projectName}/{testJobName}/{dataInfo}/{columnName}/{isUnique}/{start}/{end}")]
        public async Task<HttpResponseMessage> GetDataColumnValues(string projectName, string testJobName, string dataInfo, string columnName, bool isUnique, DateTime start, DateTime end)
        {
            using (ManagementDb)
            {
                var buildGuids = (from build in ManagementDb.TestBuilds
                                  where build.TestJob.name == testJobName &&
                                  build.TestJob.Project.name == projectName &&
                                  build.startTime >= start && build.startTime <= end
                                  orderby build.startTime, build.timestamp
                                  select build.guid.ToString()).ToList();
                var results = await _testResultController.GetResults(projectName, testJobName, buildGuids, dataInfo);
                if (results.Count > 0 && IsAnyContains(results, columnName))
                {
                    var values = results.SelectMany(r => r["data"].AsBsonArray.Values.OfType<BsonDocument>().Where(d => d.Contains(columnName)).Select(d => d[columnName].ToString()));
                    if (isUnique)
                    {
                        values = values.Distinct();
                    }
                    return toResponse(values.ToJson());
                }
                else
                {
                    return toResponse(new List<string>().ToJson());
                }
            }
        }

        [HttpGet]
        [Route(ServiceHelper.ApiPrefix + "/Dashboard/GetGeomeans/{projectName}/{testJobName}/{dataInfo}/{start}/{end}/{columnNames}/{addColumnNames?}/{orderBy?}")]
        public async Task<HttpResponseMessage> GetGeomeans(string projectName, string testJobName, string dataInfo, DateTime start, DateTime end, string columnNames, string addColumnNames, string orderBy)
        {
            List<string> names = columnNames.SplitToList();
            List<string> addNames = addColumnNames.SplitToList();
            using (ManagementDb)
            {
                var buildGuids = (from build in ManagementDb.TestBuilds
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
            using (ManagementDb)
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
            using (ManagementDb)
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
            using (ManagementDb)
            {
                var buildGuids = (from build in ManagementDb.TestBuilds
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
            using (ManagementDb)
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
            using (ManagementDb)
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
            using (ManagementDb)
            {
                var buildGuids = (from build in ManagementDb.TestBuilds
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
            using (ManagementDb)
            {
                var buildGuids = GetBuildGuids(projectName, testJobName, orderBy, count, true);
                var results = await _testResultController.GetResults(projectName, testJobName, buildGuids, dataInfo);
                results = OrderBy(results, orderBy).Take(count).ToList();
                return toResponse(results.ToJson());
            }
        }

        [HttpGet]
        [Route(ServiceHelper.ApiPrefix + "/Dashboard/GetLastResults/{projectName}/{testJobName}/{dataInfo}/{count}/{columnName}/{columnValues}/{orderBy?}")]
        public async Task<HttpResponseMessage> GetLastResults(string projectName, string testJobName, string dataInfo, int count, string columnName, string columnValues, string orderBy = null)
        {
            List<string> values = columnValues.SplitToList();
            using (ManagementDb)
            {
                var buildGuids = GetBuildGuids(projectName, testJobName, orderBy, count, true);
                var results = await _testResultController.GetResults(projectName, testJobName, buildGuids, dataInfo);
                results = OrderBy(results.Where(r => IsAnyContains(r, columnName, values)).ToList(), orderBy).Take(count).ToList();
                return toResponse(results.ToJson());
            }
        }

        [HttpGet]
        [Route(ServiceHelper.ApiPrefix + "/Dashboard/GetLastResults/{projectName}/{testJobName}/{dataInfo}/{resultType}/{count}/{orderBy?}")]
        public async Task<HttpResponseMessage> GetLastResults(string projectName, string testJobName, string dataInfo, string resultType, int count, string orderBy = null)
        {
            using (ManagementDb)
            {
                Enum.TryParse<ResultTypes>(resultType, out ResultTypes testResultType);
                var buildGuids = (from build in ManagementDb.TestBuilds
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
            using (ManagementDb)
            {
                var buildInfo = (from build in ManagementDb.TestBuilds
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
            using (ManagementDb)
            {
                var buildInfo = (from build in ManagementDb.TestBuilds
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

        [HttpGet]
        [Route(ServiceHelper.ApiPrefix + "/Dashboard/GetLastTimestampedResults/{projectName}/{testJobName}/{dataInfo}/{insertTimeInfo}/{count}/{columnName}/{columnValues}/{orderBy?}")]
        public async Task<HttpResponseMessage> GetLastTimestampedResults(string projectName, string testJobName, string dataInfo, string insertTimeInfo, int count, string columnName, string columnValues, string orderBy = null)
        {
            Enum.TryParse<TimeInfoTypes>(insertTimeInfo, out TimeInfoTypes timeInfo);
            var values = columnValues.Split(',').ToList();
            using (ManagementDb)
            {
                var buildInfo = (from build in ManagementDb.TestBuilds
                                 where build.TestJob.name == testJobName &&
                                 build.TestJob.Project.name == projectName
                                 orderby build.startTime, build.timestamp
                                 select new QueryResultModel { guid = build.guid.ToString(), timestamp = build.timestamp, startTime = build.startTime, endTime = build.endTime });
                var results = await _testResultController.GetResults(projectName, testJobName, buildInfo.Select(x => x.guid).ToList(), dataInfo);
                results = OrderBy(results.Where(r => IsAnyContains(r, columnName, values)).ToList(), orderBy).Take(count).ToList();
                InsertTimeStamp(timeInfo, buildInfo, results);
                return toResponse(results.ToJson());
            }
        }

        [HttpPost]
        [Route(ServiceHelper.ApiPrefix + "/Dashboard/CompareResults")]
        public async Task<HttpResponseMessage> CompareResults([FromBody] QueryCompareModel query)
        {
            BsonDocument baseResult;
            BsonDocument cmpResult;
            BsonDocument refResult;
            List<BsonDocument> baseResults;
            List<BsonDocument> cmpResults;
            List<BsonDocument> refResults;

            var isOnlyOneResult = (!string.IsNullOrEmpty(query.BaseGuid) && (string.IsNullOrEmpty(query.CompareGuid) && string.IsNullOrEmpty(query.ReferenceGuid))) ||
                !string.IsNullOrEmpty(query.QueryBaseColumnValue) && (string.IsNullOrEmpty(query.QueryCompareColumnValue) && string.IsNullOrEmpty(query.QueryReferenceColumnValue));


            if (isOnlyOneResult)
            {
                if(query.BaseGuid == null) {
                    baseResult = (await _testResultController.GetResults(query.ProjectName, query.TestJobName, query.DataInfo, query.QueryColumnName, query.QueryBaseColumnValue)).FirstOrDefault();
                }
                else
                {
                    baseResult = (await _testResultController.GetResults(query.ProjectName, query.TestJobName, new List<string>() { query.BaseGuid }, query.DataInfo)).FirstOrDefault();
                }

                if (baseResult == null)
                {
                    return ResponseNotFound;
                }
                else
                {
                    return toResponse(baseResult["data"].ToJson());
                }
            }


            if (string.IsNullOrEmpty(query.BaseGuid) && string.IsNullOrEmpty(query.CompareGuid) && string.IsNullOrEmpty(query.ReferenceGuid))
            {
                baseResults = await _testResultController.GetResults(query.ProjectName, query.TestJobName, query.DataInfo, query.QueryColumnName, query.QueryBaseColumnValue);
                cmpResults = await _testResultController.GetResults(query.ProjectName, query.TestJobName, query.DataInfo, query.QueryColumnName, query.QueryCompareColumnValue);
                refResults = await _testResultController.GetResults(query.ProjectName, query.TestJobName, query.DataInfo, query.QueryColumnName, query.QueryReferenceColumnValue);

            }
            else
            {
                var guidResults = new Dictionary<string, List<BsonDocument>>();
                await Task.WhenAll(new List<string>() { query.BaseGuid, query.CompareGuid, query.ReferenceGuid }.Select(async g =>
                {
                    if (string.IsNullOrEmpty(g))
                    {
                        return;
                    }
                    var result = await _testResultController.GetResults(query.ProjectName, query.TestJobName, new List<string>() { g }, query.DataInfo);
                    guidResults.Add(g, result);
                }));
                baseResults = guidResults.ContainsKey(query.BaseGuid) ? guidResults[query.BaseGuid] : new List<BsonDocument>();
                cmpResults = guidResults.ContainsKey(query.CompareGuid) ? guidResults[query.CompareGuid] : new List<BsonDocument>();
                refResults = guidResults.ContainsKey(query.ReferenceGuid) ? guidResults[query.ReferenceGuid] : new List<BsonDocument>();
            }


            if (baseResults.Count == 0 || (cmpResults.Count == 0 && refResults.Count == 0))
            {
                return toResponse("No results found for the given query parameters.", HttpStatusCode.NotFound);
            }

            baseResult = baseResults.FirstOrDefault();
            cmpResult = cmpResults.FirstOrDefault();
            refResult = refResults.FirstOrDefault();

            var baseContent = Converter.JsonToCsv(baseResult?["data"].ToJson());
            var cmpContent = Converter.JsonToCsv(cmpResult?["data"].ToJson());
            var refContent = Converter.JsonToCsv(refResult?["data"].ToJson());

            using (var context = new TempDirectoryContext("Dashbaord.CompareResults"))
            {
                var baseFile = context.CreateFile("base.csv", baseContent) ?? Guid.NewGuid().ToString();
                var cmpFile = context.CreateFile("compare.csv", cmpContent) ?? Guid.NewGuid().ToString();
                var refFile = context.CreateFile("reference.csv", refContent) ?? Guid.NewGuid().ToString();

                var output = Path.Combine(context.TempPath, "output.csv");
                var cmd = query.ToCompareCommandLine(Path.Combine(_prismBinaryRootPath, "Script", "compare_results.py"), baseFile, cmpFile, refFile, output);
                Conda.ExecuteCmd(cmd);
                var json = Converter.CsvToJson(File.ReadAllText(output));
                return toResponse(json, Conda.ExitCode == 0? HttpStatusCode.OK: HttpStatusCode.Forbidden);
            }
        }

        public async Task<HttpResponseMessage> GetLastResults(string projectName, string testJobName, string dataInfo, int count, string resultType, [FromBody] ResultQueryModel query, string orderBy = null)
        {
            using (ManagementDb)
            {
                Enum.TryParse<ResultTypes>(resultType, out ResultTypes testResultType);
                var buildGuids = (from build in ManagementDb.TestBuilds
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
