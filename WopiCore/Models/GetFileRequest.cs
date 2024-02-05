using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace WopiCore.Models
{
    public class GetFileRequest : WopiRequest
    {
        public GetFileRequest(HttpRequest httpRequest, string fileId) : base(httpRequest, fileId)
        {
            if (httpRequest.Headers.ContainsKey(WopiRequestHeaders.MAX_EXPECTED_SIZE))
            {
                var matchingHeaders = httpRequest.Headers[WopiRequestHeaders.MAX_EXPECTED_SIZE];
                MaxExpectedSize = long.Parse(matchingHeaders.First());
            }
        }

        public long? MaxExpectedSize { get; private set; }


        public GetFileResponse ResponseOK(Stream content, string itemVersion = null)
        {
            return new GetFileResponse()
            {
                StatusCode = HttpStatusCode.OK,
                ItemVersion = itemVersion,
                Stream = content
            };
        }

        public GetFileResponse ResponseFileTooLarge()
        {
            return new GetFileResponse()
            {
                StatusCode = HttpStatusCode.PreconditionFailed
            };
        }
    }
}