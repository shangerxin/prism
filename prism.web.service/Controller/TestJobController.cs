using prism.model.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Text.Json;

namespace prism.web.service.Controller
{
    public class TestJobController : PrismControllerBase<TestJob>
    {
        // GET: api/v{apiVersion}/TestJob
        public string Get()
        {
            using (managementDb)
            {
                return JsonSerializer.Serialize(managementDb.TestJobs.ToList().Select(x => ToSerizalizable(x)));
            }
        }

        // GET: api/v{apiVersion}/TestJob/5
        public TestJob Get(int id)
        {
            using (managementDb)
            {
                return managementDb.TestJobs.SingleOrDefault(t => t.id == id);
            }
        }

        [HttpGet]
        [Route(ServiceHelper.ApiPrefix + "/TestJob/Id/{name}")]
        public string Id(string name)
        {
            using (managementDb)
            {
                return managementDb.TestJobs.Where(p => p.name == name).FirstOrDefault()?.id.ToString();
            }
        }

        [HttpPost]
        // POST: api/v{apiVersion}/TestJob
        public TestJob Post([FromBody]string value)
        {
            var testJob = JsonSerializer.Deserialize<TestJob>(value);
            using (managementDb)
            {
                managementDb.TestJobs.Add(testJob);
                managementDb.SaveChanges();
            }
            return testJob;
        }

        // PUT: api/v{apiVersion}/TestJob/5
        public bool Put(int id, [FromBody]string value)
        {
            var testJob = JsonSerializer.Deserialize<TestJob>(value);
            using (managementDb)
            {
                var existingTestJob = managementDb.TestJobs.SingleOrDefault(t => t.name == testJob.name || t.id == id);
                if (existingTestJob != null)
                {
                    existingTestJob.name = testJob.name;
                    existingTestJob.url = testJob.url;
                    existingTestJob.credentialId = testJob.credentialId;
                    existingTestJob.defaultTestMachineId = testJob.defaultTestMachineId;
                    existingTestJob.projectId = testJob.projectId;
                    managementDb.SaveChanges();
                    return true;
                }
            }
            return false;
        }

        // DELETE: api/v{apiVersion}/TestJob/5
        public void Delete(int id)
        {
            using (managementDb)
            {
                var testJob = managementDb.TestJobs.SingleOrDefault(t => t.id == id);
                if (testJob != null)
                {
                    managementDb.TestJobs.Remove(testJob);
                    managementDb.SaveChanges();
                }
            }
        }

        protected override object ToSerizalizable(TestJob x)
        {
            return new
            {
                name = x.name.Trim(),
                x.url,
                x.credentialId,
                x.defaultTestMachineId,
                x.projectId
            };
        }
    }
}
