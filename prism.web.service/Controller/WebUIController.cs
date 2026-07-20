using prism.model.Model.WebAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace prism.web.service.Controller
{
    public class WebUIController : PrismControllerBase<Object>
    {
        public WebUIController() { }

        [HttpPost]
        [Route(ServiceHelper.ApiPrefix + "/WebUI/Login")]
        public async Task<HttpResponseMessage> Login([FromBody] LoginModel request)
        {
            throw new NotImplementedException();
        }

        [HttpGet]
        [Route(ServiceHelper.ApiPrefix + "/WebUI/Project")]
        public async Task<HttpResponseMessage> Project()
        {
            throw new NotImplementedException();
        }

        protected override object ToSerizalizable(object x)
        {
            throw new NotImplementedException();
        }
    }
}