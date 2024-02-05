using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http;

namespace WopiCore.Models
{
    public class PutFileResponse : WopiResponse
    {
        internal PutFileResponse()
        { }

        public string Lock { get; internal set; }
        public string LockFailureReason { get; internal set; }
        public string ItemVersion { get; set; }

        public override IActionResult ToResult(HttpResponse response)
        {
            var result = base.ToResult(response);
            response.Headers.Add(WopiResponseHeaders.LOCK, Lock);
            response.Headers.Add(WopiResponseHeaders.LOCK_FAILURE_REASON, LockFailureReason);
            response.Headers.Add(WopiResponseHeaders.ITEM_VERSION, ItemVersion);
            return result;
        }
    }
}
