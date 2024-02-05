using System.Net.Http;
using System.Net;
using Microsoft.AspNetCore.Http;

namespace WopiCore.Models
{
    public class GetLockRequest : WopiRequest
    {
        internal GetLockRequest(HttpRequest httpRequest, string fileId) : base(httpRequest, fileId)
        { }

        public GetLockResponse ResponseFileLocked(string existingLock)
        {
            return new GetLockResponse()
            {
                StatusCode = HttpStatusCode.OK,
                Lock = string.IsNullOrEmpty(existingLock) ? Constants.EmptyLock : existingLock
            };
        }

        public GetLockResponse ResponseFileNotLocked()
        {
            return new GetLockResponse()
            {
                StatusCode = HttpStatusCode.OK,
                Lock = Constants.EmptyLock
            };
        }

        public GetLockResponse ResponseLockConflict(string lockFailureReason = null)
        {
            return new GetLockResponse()
            {
                StatusCode = HttpStatusCode.Conflict,
                LockFailureReason = lockFailureReason
            };
        }

        public WopiResponse ResponseNotImplemented()
        {
            return new GetLockResponse()
            {
                StatusCode = HttpStatusCode.NotImplemented
            };
        }
    }
}