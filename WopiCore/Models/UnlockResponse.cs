using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace WopiCore.Models
{
    public class UnlockResponse : WopiResponse
    {
        internal UnlockResponse()
        { }
        public string Lock { get; internal set; }
        public string LockFailureReason { get; internal set; }
        public string ItemVersion { get; set; }

        public override IActionResult ToResult(HttpResponse response)
        {
            var httpResponseMessage = base.ToResult(response);
            response.Headers.Add(WopiResponseHeaders.LOCK, Lock);
            response.Headers.Add(WopiResponseHeaders.LOCK_FAILURE_REASON, LockFailureReason);
            response.Headers.Add(WopiResponseHeaders.ITEM_VERSION, ItemVersion);
            return httpResponseMessage;
        }
    }
}
