using Microsoft.AspNetCore.Http;
using System.Net;
using System.Net.Http;

namespace WopiCore.Models
{
    public class RenameFileRequest : WopiRequest
    {
        internal RenameFileRequest(HttpRequest httpRequest, string fileId) : base(httpRequest, fileId)
        {
            RequestedName = GetHttpRequestHeader(httpRequest, WopiRequestHeaders.REQUESTED_NAME);
            Lock = GetHttpRequestHeader(httpRequest, WopiRequestHeaders.LOCK);
        }
        public string RequestedName { get; private set; }

        public string Lock { get; private set; }

        public RenameFileResponse ResponseOK(string renamedFileBaseName, string itemVersion = null)
        {
            return new RenameFileResponse()
            {
                StatusCode = HttpStatusCode.OK,
                RenamedFileBaseName = renamedFileBaseName,
                ItemVersion = itemVersion
            };
        }
        public RenameFileResponse ResponseLockConflict(string existingLock, string lockFailureReason = null)
        {
            return new RenameFileResponse()
            {
                StatusCode = HttpStatusCode.Conflict,
                Lock = existingLock,
                LockFailureReason = lockFailureReason
            };
        }
        public RenameFileResponse ResponseBadRequest(string invalidFileNameError)
        {
            return new RenameFileResponse()
            {
                StatusCode = HttpStatusCode.BadRequest,
                InvalidFileNameError = invalidFileNameError
            };
        }

        public WopiResponse ResponseNotImplemented()
        {
            return new RenameFileResponse()
            {
                StatusCode = HttpStatusCode.NotImplemented
            };
        }
    }
}
