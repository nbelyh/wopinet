using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json.Serialization;

namespace WopiCore.Models
{

    public class WopiResponse
    {
        internal WopiResponse()
        {
            HostEndpoint = WopiConfiguration.HostEndpoint;
            MachineName = WopiConfiguration.MachineName;
            ServerVersion = WopiConfiguration.ServerVersion;
        }

        [JsonIgnore]
        public HttpStatusCode StatusCode { get; internal set; }

        public virtual IActionResult ToResult(HttpResponse response)
        {
            response.Headers.Add(WopiResponseHeaders.HOST_ENDPOINT, HostEndpoint);
            response.Headers.Add(WopiResponseHeaders.MACHINE_NAME, MachineName);
            response.Headers.Add(WopiResponseHeaders.SERVER_VERSION, ServerVersion);
            return new StatusCodeResult((int)StatusCode);
        }

        [JsonIgnore]
        public string HostEndpoint { get; internal set; }
        [JsonIgnore]
        public string MachineName { get; internal set; }
        [JsonIgnore]
        public string ServerVersion { get; internal set; }
        [JsonIgnore]
        public string ServerError { get; internal set; }
    }
}