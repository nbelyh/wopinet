using Microsoft.AspNetCore.Http;
using System.Net;

namespace WopiCore.Models
{
    public class CheckFileInfoRequest : WopiRequest
    {
        public CheckFileInfoRequest(HttpRequest httpRequest, string fileId) : base(httpRequest, fileId)
        {
            SessionContext = GetHttpRequestHeader(httpRequest, WopiRequestHeaders.SESSION_CONTEXT);
        }
        public string SessionContext { get; private set; }

        public CheckFileInfoResponse ResponseOK(string baseFileName, string ownerId, long size, string userId, string userName, string version)
        {
            return new CheckFileInfoResponse(userName)
            {
                StatusCode = HttpStatusCode.OK,
                BaseFileName = baseFileName,
                OwnerId = ownerId,
                Size = size,
                UserId = userId,
                Version = version
            };
        }
    }
}