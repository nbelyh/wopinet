using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace WopiCore.Services
{
    public struct GetFileContentResult
    {
        public HttpStatusCode StatusCode;
        public string Version;
        public Stream Stream;
    }

    public struct GetFileInfoResult
    {
        public HttpStatusCode StatusCode;
        public WopiFile File;
    }

    public struct LockFileResult
    {
        public HttpStatusCode StatusCode;
        public string LockValue;
        public string Version;
    }
    public struct GetLockStatusResult
    {
        public HttpStatusCode StatusCode;
        public string LockValue;
    }
    public struct UnlockFileResult
    {
        public HttpStatusCode StatusCode;
        public string LockValue;
        public string Version;
    }
    public struct UpdateFileContentResult
    {
        public HttpStatusCode StatusCode;
        public string LockValue;
        public string Version;
    }
    public interface IWopiFileRepository
    {
        Task<GetFileContentResult> GetFileContent(string fileId, IAuthInfo auth);
        Task<GetFileInfoResult> GetFileInfo(string fileId, IAuthInfo auth);
        Task<GetLockStatusResult> GetLockStatus(string fileId, IAuthInfo auth);
        Task<LockFileResult> LockFile(string fileId, IAuthInfo auth, string lockId, string oldLockId, double lockDurationMinutes = 30);
        Task<UnlockFileResult> UnlockFile(string fileId, IAuthInfo auth, string lockId);
        Task<UpdateFileContentResult> UpdateFileContent(string fileId, IAuthInfo auth, string lockValue, Stream content, long? contentLength);
    }
}