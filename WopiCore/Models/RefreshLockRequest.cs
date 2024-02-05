using Microsoft.AspNetCore.Http;
using System.Net;
using System.Net.Http;

namespace WopiCore.Models
{
    public class RefreshLockRequest : WopiRequest
    {
        internal RefreshLockRequest(HttpRequest httpRequest, string fileId) : base(httpRequest, fileId)
        {
            Lock = GetHttpRequestHeader(httpRequest, WopiResponseHeaders.LOCK);
        }

        public string Lock { get; private set; }

        public RefreshLockResponse ResponseOK(string itemVersion = null)
        {
            return new RefreshLockResponse()
            {
                StatusCode = HttpStatusCode.OK,
                ItemVersion = itemVersion
            };
        }

        public RefreshLockResponse ResponseBadRequest()
        {
            return new RefreshLockResponse()
            {
                StatusCode = HttpStatusCode.BadRequest
            };
        }

        public RefreshLockResponse ResponseLockConflict(string existingLock, string lockFailureReason = null)
        {
            return new RefreshLockResponse()
            {
                StatusCode = HttpStatusCode.Conflict,
                Lock = existingLock,
                LockFailureReason = lockFailureReason
            };
        }

        public WopiResponse ResponseNotImplemented()
        {
            return new RefreshLockResponse()
            {
                StatusCode = HttpStatusCode.NotImplemented
            };
        }
    }
}