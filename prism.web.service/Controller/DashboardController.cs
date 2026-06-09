using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
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
using System.Threading.Tasks;
using System.Web.Http;
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
        public async Task<HttpResponseMessage> GetLastResults(string projectName, string testJobName, string dataInfo, int count, string resultType, string selectColumns)
        {
            using (managementDb)
            {
                Enum.TryParse<ResultTypes>(resultType, out ResultTypes testResultType);
                var buildGuids = (from build in managementDb.TestBuilds
                                  where build.TestJob.name == testJobName && build.testResultId == (int) testResultType
                                  orderby build.timestamp descending
                                  select build.guid).Take(count).ToList();
                var testResultController = new TestResultController();
                
                var results = await testResultController.GetResults(projectName, testJobName, buildGuids.Select(x => x.ToString()).ToList(), dataInfo);
                return toResponse(results.ToJson());
            }
        }

        // POST: api/v{apiVersion}/Dashboard
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/v{apiVersion}/Dashboard/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/v{apiVersion}/Dashboard/5
        public void Delete(int id)
        {
        }

        protected override object ToSerizalizable(object x)
        {
            throw new NotImplementedException();
        }
    }
}
