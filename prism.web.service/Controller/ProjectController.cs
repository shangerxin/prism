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
    public class ProjectController : PrismControllerBase<Project>
    {
        public ProjectController()
        {
        }

        public string Get() {
            using (managementDb)
            {
                var projects = managementDb.Projects.ToList();
                return JsonSerializer.Serialize(projects.Select(p => ToSerizalizable(p)));
            }
        }

        // GET: api/v{apiVersion}/Project/5
        public string Get(int id)
        {
            using (managementDb)
            {
                var project = managementDb.Projects.Where(p => p.id == id).FirstOrDefault();
                return Serizalize(project);
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
        public Project Post([FromBody]string value)
        {
            using (managementDb) {
                var project = JsonSerializer.Deserialize<Project>(value);
                managementDb.Projects.Add(project);
                managementDb.SaveChanges();
                return project;
            }   
        }

        // PUT: api/v{apiVersion}/Project/5
        public bool Put(int id, [FromBody]string value)
        {
            using (managementDb) {
                var project = JsonSerializer.Deserialize<Project>(value);
                var existingProject = managementDb.Projects.SingleOrDefault(p => p.id == id || p.name == project.name);
                if (existingProject != null)
                {
                    existingProject.name = project.name;
                    existingProject.description = project.description;
                    existingProject.productId = project.productId;
                    managementDb.SaveChanges();
                    return true;
                }
            }
            return false;
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

        protected override object ToSerizalizable(Project x)
        {
            return  new { x.id, name = x.name.Trim(), x.description, x.productId };
        }
    }
}
