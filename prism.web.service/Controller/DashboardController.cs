using Microsoft.Practices.EnterpriseLibrary.Common.Utility;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using prism.model;
using prism.web.service.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net;
using System.Net.Http;
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
        [Route(ServiceHelper.ApiPrefix + "/Dashboard/GetLastResults/{projectName}/{testJobName}/{dataInfo}/{count}/{resultType}")]
        public async Task<HttpResponseMessage> GetLastResults(string projectName, string testJobName, string dataInfo, int count, string resultType)
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
                    
                //query.GroupResultsWithColumnValues.ForEach((group) =>
                //{
                //    var columnKey = group.Key;
                //    var values = group.Value;
                //    if (colummns.Any(x => x.Column == columnKey))
                //    {
                //        var columnValues = colummns.First(x => x.Column == columnKey).Values;
                //        values.ForEach(value =>
                //        {
                //            if (!columnValues.Contains(value))
                //            {
                //                columnValues.Add(value);
                //            }
                //        });
                //    }
                //    else
                //    {
                //        colummns = colummns.Append(new
                //        {
                //            Column = columnKey,
                //            Values = values
                //        });
                //    }
                //});

                return toResponse(colummns.ToJson());
            }
        }

        protected override object ToSerizalizable(object x)
        {
            throw new NotImplementedException();
        }
    }
}
