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
            using (_managementDb)
            {
                return _managementDb.Projects.ToList();
            }
        }

        // GET: api/v{apiVersion}/Project/5
        public Project Get(int id)
        {
            using (_managementDb)
            {
                return _managementDb.Projects.Where(p => p.id == id).FirstOrDefault();
            }
        }

        [HttpGet]
        [Route(ServiceHelper.ApiPrefix + "/Project/Id/{name}")]      
        public string Id(string name)
        {
            using (_managementDb)
            {
                return _managementDb.Projects.Where(p => p.name == name).FirstOrDefault()?.id.ToString();
            }
        }

        // POST: api/v{apiVersion}/Project
        public void Post([FromBody]string value)
        {
            using (_managementDb) {
                var project = JsonSerializer.Deserialize<Project>(value);
                _managementDb.Projects.Add(project);
                _managementDb.SaveChanges();
            }   
        }

        // PUT: api/v{apiVersion}/Project/5
        public void Put(int id, [FromBody]string value)
        {
            using (_managementDb) {
                var project = JsonSerializer.Deserialize<Project>(value);
                var existingProject = _managementDb.Projects.SingleOrDefault(p => p.id == id);
                if (existingProject != null)
                {
                    existingProject.name = project.name;
                    existingProject.description = project.description;
                    existingProject.productId = project.productId;
                    _managementDb.SaveChanges();
                }
            }
        }

        // DELETE: api/v{apiVersion}/Project/5
        public void Delete(int id)
        {
            using (_managementDb) {
                var project = _managementDb.Projects.SingleOrDefault(p => p.id == id);
                if (project != null)
                {
                    _managementDb.Projects.Remove(project);
                    _managementDb.SaveChanges();
                }
            }
        }
    }
}
