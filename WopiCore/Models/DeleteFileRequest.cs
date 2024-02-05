using Microsoft.AspNetCore.Http;
using System.Net;
using System.Net.Http;

namespace WopiCore.Models
{
    public class DeleteFileRequest : WopiRequest
    {
        internal DeleteFileRequest(HttpRequest httpRequest, string fileId) : base(httpRequest, fileId)
        {
        }

        public DeleteFileResponse ResponseOK()
        {
            return new DeleteFileResponse()
            {
                StatusCode = HttpStatusCode.OK
            };
        }
        public DeleteFileResponse ResponseLockConflict(string existingLock, string lockFailureReason = null)
        {
            return new DeleteFileResponse()
            {
                StatusCode = HttpStatusCode.Conflict,
                Lock = existingLock,
                LockFailureReason = lockFailureReason
            };
        }
        public WopiResponse ResponseNotImplemented()
        {
            return new DeleteFileResponse()
            {
                StatusCode = HttpStatusCode.NotImplemented
            };
        }
    }
}
