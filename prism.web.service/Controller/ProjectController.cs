using prism.model.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace prism.web.service.Controller
{
    public class ProjectController : PrismControllerBase<Project>
    {
        public ProjectController()
        {
        }

        public async Task<HttpResponseMessage> Get() {
            using (ManagementDb)
            {
                var projects = ManagementDb.Projects.Select(p => ToSerizalizable(p));
                var content = JsonSerializer.Serialize(projects);
                return toResponse(content);
            }
        }

        // GET: api/v{apiVersion}/Project/5
        public string Get(int id)
        {
            using (ManagementDb)
            {
                var project = ManagementDb.Projects.Where(p => p.id == id).FirstOrDefault();
                return Serizalize(project);
            }
        }

        [HttpGet]
        [Route(ServiceHelper.ApiPrefix + "/Project/Id/{name}")]      
        public async Task<HttpResponseMessage> Id(string name)
        {
            using (ManagementDb)
            {
                var project = ManagementDb.Projects.Where(p => p.name == name).FirstOrDefault();
                var content = project?.id.ToString();
                return toResponse(content);
            }
        }

        // POST: api/v{apiVersion}/Project
        public async Task<HttpResponseMessage> Post([FromBody]string value)
        {
            using (ManagementDb) {
                var project = JsonSerializer.Deserialize<Project>(value);
                ManagementDb.Projects.Add(project);
                ManagementDb.SaveChanges();
                return toResponse(Serizalize(project));
            }   
        }

        // PUT: api/v{apiVersion}/Project/5
        public async Task<HttpResponseMessage> Put(int id, [FromBody]string value)
        {
            using (ManagementDb) {
                var project = JsonSerializer.Deserialize<Project>(value);
                var existingProject = ManagementDb.Projects.SingleOrDefault(p => p.id == id || p.name == project.name);
                if (existingProject != null)
                {
                    existingProject.name = project.name;
                    existingProject.description = project.description;
                    existingProject.productId = project.productId;
                    ManagementDb.SaveChanges();
                    return ResponseOK;
                }
            }
            return ResponseNotFound;
        }

        // DELETE: api/v{apiVersion}/Project/5
        public async Task<HttpResponseMessage> Delete(int id)
        {
            using (ManagementDb) {
                var project = ManagementDb.Projects.SingleOrDefault(p => p.id == id);
                if (project != null)
                {
                    ManagementDb.Projects.Remove(project);
                    ManagementDb.SaveChanges();
                    return ResponseOK;
                }
            }
            return ResponseNotFound;
        }

        protected override object ToSerizalizable(Project x)
        {
            return  new { x.id, name = x.name.Trim(), x.description, x.productId };
        }
    }
}
