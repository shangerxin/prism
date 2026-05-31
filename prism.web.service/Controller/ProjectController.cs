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
    public class ProjectController : PrismControllerBase
    {
        public ProjectController()
        {
        }

        public List<Project> Get() {
            using (managementDb)
            {
                return managementDb.Projects.ToList();
            }
        }

        // GET: api/v{apiVersion}/Project/5
        public Project Get(int id)
        {
            using (managementDb)
            {
                return managementDb.Projects.Where(p => p.id == id).FirstOrDefault();
            }
        }

        [HttpGet]
        [Route(ServiceHelper.ApiPrefix + "/Project/Id/{name}")]      
        public string Id(string name)
        {
            using (managementDb)
            {
                return managementDb.Projects.Where(p => p.name == name).FirstOrDefault()?.id.ToString();
            }
        }

        // POST: api/v{apiVersion}/Project
        public void Post([FromBody]string value)
        {
            using (managementDb) {
                var project = JsonSerializer.Deserialize<Project>(value);
                managementDb.Projects.Add(project);
                managementDb.SaveChanges();
            }   
        }

        // PUT: api/v{apiVersion}/Project/5
        public void Put(int id, [FromBody]string value)
        {
            using (managementDb) {
                var project = JsonSerializer.Deserialize<Project>(value);
                var existingProject = managementDb.Projects.SingleOrDefault(p => p.id == id);
                if (existingProject != null)
                {
                    existingProject.name = project.name;
                    existingProject.description = project.description;
                    existingProject.productId = project.productId;
                    managementDb.SaveChanges();
                }
            }
        }

        // DELETE: api/v{apiVersion}/Project/5
        public void Delete(int id)
        {
            using (managementDb) {
                var project = managementDb.Projects.SingleOrDefault(p => p.id == id);
                if (project != null)
                {
                    managementDb.Projects.Remove(project);
                    managementDb.SaveChanges();
                }
            }
        }
    }
}
