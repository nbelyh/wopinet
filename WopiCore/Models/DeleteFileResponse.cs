using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json.Serialization;

namespace WopiCore.Models
{
    public class DeleteFileResponse : WopiResponse
    {
        internal DeleteFileResponse()
        { }

        public string Lock { get; internal set; }

        [JsonIgnore]
        public string LockFailureReason { get; set; }

        public override IActionResult ToResult(HttpResponse response)
        {
            var httpResponseMessage = base.ToResult(response);

            response.Headers.Add(WopiResponseHeaders.LOCK, Lock);
            response.Headers.Add(WopiResponseHeaders.LOCK_FAILURE_REASON, LockFailureReason);

            // Only serialize reponse on success
            if (StatusCode != HttpStatusCode.OK)
                return httpResponseMessage;

            return new JsonResult(this, new System.Text.Json.JsonSerializerOptions { IgnoreNullValues = true });
        }
    }
}
