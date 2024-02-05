using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;

namespace WopiCore.Models
{
    public class GetFileResponse : WopiResponse
    {
        internal GetFileResponse()
        {
        }

        public Stream Stream { get; internal set; }

        public string ItemVersion { get; set; }

        public override IActionResult ToResult(HttpResponse response)
        {
            var result = base.ToResult(response);

            if (StatusCode != HttpStatusCode.OK)
                return result;

            response.Headers.Add(WopiResponseHeaders.ITEM_VERSION, ItemVersion);
            return new FileStreamResult(Stream, "application/x-binary");
        }
    }
}