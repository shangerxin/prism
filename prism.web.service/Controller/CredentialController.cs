using prism.model.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web.Http;

namespace prism.web.service.Controller
{
    public class CredentialController : PrismControllerBase<UserCredential>
    {
        protected Boolean IsValideCredential(UserCredential credential)
        {
            if (credential == null)
            {
                return false;
            }
            if (string.IsNullOrEmpty(credential.userName) || 
                (string.IsNullOrEmpty(credential.password) && 
                string.IsNullOrEmpty(credential.token)))
            {
                return false;
            }
            return true;
        }

        [HttpGet]
        [Route(ServiceHelper.ApiPrefix + "/Credential/Get/")]
        public async Task<IEnumerable<UserCredential>> Get()
        {
            using (managementDb)
            {
                return (from x in managementDb.UserCredentials select x).ToList();
            }
        }


        [HttpGet]
        [Route(ServiceHelper.ApiPrefix + "/Credential/Get/{id}")]
        public async Task<UserCredential> Get(int id)
        {
            using (managementDb)
            {
                var result = (from x in managementDb.UserCredentials
                             where x.id == id
                             select x).FirstOrDefault();

                return result;
            }
        }

        [HttpPost]
        [Route(ServiceHelper.ApiPrefix + "/Credential/")]
        public int Post([FromBody] UserCredential value)
        {
            if (IsValideCredential(value))
            {
                using(managementDb)
                {
                    managementDb.UserCredentials.Add(value);
                    managementDb.SaveChanges();
                    return value.id;
                }
            }
            else
            {
                var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
                response.Content = new StringContent("Invalide user credential");
                throw new HttpResponseException(response);
            }
        }

        [HttpPut]
        [Route(ServiceHelper.ApiPrefix + "/Credential/")]
        public void Put(int id, [FromBody] UserCredential value)
        {
            var credential = (from x in managementDb.UserCredentials
                              where x.userName == value.userName &&
                                    x.createdby == value.createdby &&
                                    x.scope == value.scope
                              select x).FirstOrDefault();
            if (credential == null)
            {
                var response = new HttpResponseMessage(HttpStatusCode.NotFound);
                response.Content = new StringContent("User credential not found");
                throw new HttpResponseException(response);

            }
            else
            {
                credential.password = value.password;
                credential.token = value.token;
                credential.description = value.description;
                managementDb.SaveChanges();
            }
        }

        [HttpDelete]
        [Route(ServiceHelper.ApiPrefix + "/Credential/{id}")]
        public void Delete(int id)
        {
            var credential = (from x in managementDb.UserCredentials
                              where x.id == id
                              select x).FirstOrDefault();
            if (credential == null)
            {
                var response = new HttpResponseMessage(HttpStatusCode.NotFound);
                response.Content = new StringContent("User credential not found");
                throw new HttpResponseException(response);
            }
            else
            {
                managementDb.UserCredentials.Remove(credential);
                managementDb.SaveChanges();
            }
        }

        protected override object ToSerizalizable(UserCredential x)
        {
            return x;
        }
    }
}