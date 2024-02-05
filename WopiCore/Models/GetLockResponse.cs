using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WopiCore.Models
{
    public class GetLockResponse : WopiResponse
    {
        internal GetLockResponse()
        { }

        public string Lock { get; internal set; }

        public string LockFailureReason { get; set; }

        public override IActionResult ToResult(HttpResponse response)
        {
            var result = base.ToResult(response);
            response.Headers.Add(WopiResponseHeaders.LOCK, Lock);
            response.Headers.Add(WopiResponseHeaders.LOCK_FAILURE_REASON, LockFailureReason);
            return result;
        }
    }

}