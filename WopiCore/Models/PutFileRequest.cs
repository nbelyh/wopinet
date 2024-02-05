using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace WopiCore.Models
{
    public class PutFileRequest : WopiRequest
    {
        internal PutFileRequest(HttpRequest httpRequest, string fileId) : base(httpRequest, fileId)
        {
            Content = httpRequest.Body;
            ContentLength = httpRequest.ContentLength.Value;
            Lock = GetHttpRequestHeader(httpRequest, WopiRequestHeaders.LOCK);
        }

        public string Lock { get; private set; }

        public Stream Content { get; private set; }
        public long? ContentLength { get; private set; }

        public PutFileResponse ResponseOK(string itemVersion = null)
        {
            return new PutFileResponse()
            {
                StatusCode = HttpStatusCode.OK,
                ItemVersion = itemVersion
            };
        }

        public PutFileResponse ResponseBadRequest()
        {
            return new PutFileResponse()
            {
                StatusCode = HttpStatusCode.BadRequest
            };
        }

        public PutFileResponse ResponseLockConflict(string existingLock, string lockFailureReason = null)
        {
            return new PutFileResponse()
            {
                StatusCode = HttpStatusCode.Conflict,
                Lock = existingLock,
                LockFailureReason = lockFailureReason
            };
        }

        public PutFileResponse ResponseFileTooLarge()
        {
            return new PutFileResponse()
            {
                StatusCode = HttpStatusCode.RequestEntityTooLarge
            };
        }

        public WopiResponse ResponseNotImplemented()
        {
            return new PutFileResponse()
            {
                StatusCode = HttpStatusCode.NotImplemented
            };
        }
    }
}
