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
    public class TestJobController : PrismControllerBase
    {
        // GET: api/v{apiVersion}/TestJob
        public List<TestJob> Get()
        {
            using (_managementDb)
            {
                return _managementDb.TestJobs.ToList();
            }
        }

        // GET: api/v{apiVersion}/TestJob/5
        public TestJob Get(int id)
        {
            using (_managementDb)
            {
                return _managementDb.TestJobs.SingleOrDefault(t => t.id == id);
            }
        }

        [HttpGet]
        [Route(ServiceHelper.ApiPrefix + "/TestJob/Id/{name}")]
        public string Id(string name)
        {
            using (_managementDb)
            {
                return _managementDb.TestJobs.Where(p => p.name == name).FirstOrDefault()?.id.ToString();
            }
        }

        // POST: api/v{apiVersion}/TestJob
        public void Post([FromBody]string value)
        {
            var testJob = JsonSerializer.Deserialize<TestJob>(value);
            using (_managementDb)
            {
                _managementDb.TestJobs.Add(testJob);
                _managementDb.SaveChanges();
            }
        }

        // PUT: api/v{apiVersion}/TestJob/5
        public void Put(int id, [FromBody]string value)
        {
            var testJob = JsonSerializer.Deserialize<TestJob>(value);
            using (_managementDb)
            {
                var existingTestJob = _managementDb.TestJobs.SingleOrDefault(t => t.id == id);
                if (existingTestJob != null)
                {
                    existingTestJob.name = testJob.name;
                    existingTestJob.url = testJob.url;
                    existingTestJob.credentialId = testJob.credentialId;
                    existingTestJob.defaultTestMachineId = testJob.defaultTestMachineId;
                    existingTestJob.projectId = testJob.projectId;
                    _managementDb.SaveChanges();
                }
            }
        }

        // DELETE: api/v{apiVersion}/TestJob/5
        public void Delete(int id)
        {
            using (_managementDb)
            {
                var testJob = _managementDb.TestJobs.SingleOrDefault(t => t.id == id);
                if (testJob != null)
                {
                    _managementDb.TestJobs.Remove(testJob);
                    _managementDb.SaveChanges();
                }
            }
        }
    }
}
