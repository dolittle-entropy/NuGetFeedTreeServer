using System;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Server
{
    [Route("/")]
    public class IndexController : ControllerBase
    {
        [HttpGet("index.json")]
        public ActionResult<ServiceIndex> GetServiceIndex()
        {
            return GetServiceIndexFor(null);
        }

        [HttpGet("{context}/index.json")]
        public ActionResult<ServiceIndex> GetScopedServiceIndex(Guid context)
        {
            if (!ControllerContext.ModelState.IsValid)
            {
                return NotFound();
            }
            return GetServiceIndexFor(context);
        }

        ServiceIndex GetServiceIndexFor(Guid? context)
        {
            var prefix = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/";
            var postfix = context.HasValue ? "/"+context : "";

            return new ServiceIndex(prefix, postfix);
        }

        public class ServiceIndex
        {
            public ServiceIndex(string prefix, string postfix)
            {
                Resources = new [] {
                    //new ServiceResource("SearchQueryService", prefix+"query"+postfix), // Seems like we actually don't need it?
                    new ServiceResource("PackagePublish/2.0.0", prefix+"v2/packages"+postfix),
                    new ServiceResource("RegistrationsBaseUrl/3.4.0", prefix+"v3/registrations"+postfix),
                    new ServiceResource("PackageBaseAddress/3.0.0", prefix+"v3/flatcontainer"+postfix),
                };
            }

            public string Version => "3.0.0";
            public ServiceResource[] Resources { get; private set; }

            public class ServiceResource
            {
                public ServiceResource(string type, string id)
                {
                    Type = type;
                    ID = id;
                }
                [JsonProperty("@type")]
                public string Type { get; private set; }
                [JsonProperty("@id")]
                public string ID { get; private set; }
            }
        }
    }
}