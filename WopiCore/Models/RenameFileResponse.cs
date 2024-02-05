using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json.Serialization;

namespace WopiCore.Models
{
    public class RenameFileResponse : WopiResponse
    {
        internal RenameFileResponse()
        { }

        public string RenamedFileBaseName { get; internal set; }

        [JsonIgnore]
        public string Lock { get; internal set; }
        [JsonIgnore]
        public string LockFailureReason { get; set; }
        [JsonIgnore]
        public string ItemVersion { get; set; }
        [JsonIgnore]
        public string InvalidFileNameError { get; set; }

        public override IActionResult ToResult(HttpResponse response)
        {
            var httpResponseMessage = base.ToResult(response);

            response.Headers.Add(WopiResponseHeaders.LOCK, Lock);
            response.Headers.Add(WopiResponseHeaders.LOCK_FAILURE_REASON, LockFailureReason);
            response.Headers.Add(WopiResponseHeaders.ITEM_VERSION, ItemVersion);
            response.Headers.Add(WopiResponseHeaders.INVALID_FILE_NAME_ERROR, InvalidFileNameError);


            // Only serialize reponse on success
            if (StatusCode != HttpStatusCode.OK)
                return httpResponseMessage;

            return new JsonResult(this, new System.Text.Json.JsonSerializerOptions { IgnoreNullValues = true });
        }
    }
}
