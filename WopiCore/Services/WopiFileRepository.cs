using System.Net;
using System.IO;
using System.Threading.Tasks;
using System;
using WopiCore.Models;

namespace WopiCore.Services
{
    class WopiFileRepository : IWopiFileRepository
    {
        private readonly IWopiDbRepository db;
        private readonly IWopiStorageRepository storage;

        public WopiFileRepository(IWopiDbRepository db, IWopiStorageRepository storage)
        {
            this.db = db;
            this.storage = storage;
        }

        /// <summary>
        /// Deletes the specified file from the repository if the user has appropriate rights.  Currenlty only the the owner of the file is allowed to delete.
        /// </summary>
        /// <param name="fileId">ID file to delete</param>
        /// <param name="userId">ID of user with owner rights on file</param>
        /// <returns></returns>
        //public async Task<HttpStatusCode> DeleteFile(string fileId, AuthInfo auth)
        //{
        //    var file = await db.GetFileAsync(fileId, auth);

        //    if (file == null)
        //        return HttpStatusCode.NotFound;

        //    if (file.OwnerId != auth.UserId)
        //        return HttpStatusCode.Unauthorized;

        //    if (!await storage.DeleteBlobAsync(fileId, auth))
        //        return HttpStatusCode.NotFound;

        //    await db.DeleteFileAsync(fileId, auth);
        //    return HttpStatusCode.OK;
        //}

        /// <summary>
        /// Replaces file content with data in provide file stream if user has appropriate rights.  Currently only the owner is allowed to UpdateFileContents.
        /// </summary>
        /// <param name="fileId"></param>
        /// <param name="userId"></param>
        /// <param name="lockValue"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public async Task<UpdateFileContentResult> UpdateFileContent(string fileId, IAuthInfo auth, string lockValue, Stream content, long? contentLength)
        {
            var file = await db.GetFileAsync(fileId, auth);
            if (file == null)
                return new UpdateFileContentResult { StatusCode = HttpStatusCode.NotFound, LockValue = null, Version = null };

            if (!file.IsLocked())
            {
                if (file.Size > 0)
                    return new UpdateFileContentResult { StatusCode = HttpStatusCode.Conflict, LockValue = Constants.EmptyLock, Version = null };
            }
            else
            {
                if (!file.IsSameLock(lockValue))
                    return new UpdateFileContentResult { StatusCode = HttpStatusCode.Conflict, LockValue = file.LockValue, Version = null };
            }

            await storage.PutBlobContentAsync(fileId, content, auth);

            file.Size = contentLength ?? 0;
            file.LastModifiedTime = DateTime.Now;
            file.LastModifiedUser = auth.UserId;
            file.Version = file.Version + 1;

            await db.UpdateFileInfoAsync(file, auth);
            return new UpdateFileContentResult { StatusCode = HttpStatusCode.OK, LockValue = null, Version = file.Version.ToString() };
        }

        public async Task<GetFileInfoResult> GetFileInfo(string fileId, IAuthInfo auth)
        {
            var file = await db.GetFileAsync(fileId, auth);
            if (file == null)
                return new GetFileInfoResult { StatusCode = HttpStatusCode.NotFound, File = null };

            return new GetFileInfoResult { StatusCode = HttpStatusCode.OK, File = file };
        }

        public async Task<GetFileContentResult> GetFileContent(string fileId, IAuthInfo auth)
        {
            var wopiFile = await db.GetFileAsync(fileId, auth);
            if (wopiFile != null)
            {
                var stream = await storage.GetBlobContentAsync(fileId, auth);
                if (stream != null)
                    return new GetFileContentResult { StatusCode = HttpStatusCode.OK, Stream = stream, Version = wopiFile.Version.ToString() };
            }
            return new GetFileContentResult { StatusCode = HttpStatusCode.NotFound, Stream = null, Version = null };
        }

        public async Task<LockFileResult> LockFile(string fileId, IAuthInfo auth, string lockId, string oldLockId, double lockDurationMinutes = 30)
        {
            if (string.IsNullOrEmpty(lockId))
                return new LockFileResult { StatusCode = HttpStatusCode.BadRequest, LockValue = null, Version = null };

            var file = await db.GetFileAsync(fileId, auth);
            if (file == null)
                return new LockFileResult { StatusCode = HttpStatusCode.NotFound, LockValue = null, Version = null };

            if (oldLockId != null)
            {
                if (file.IsLocked())
                {
                    if (!file.IsSameLock(oldLockId))
                        return new LockFileResult { StatusCode = HttpStatusCode.Conflict, LockValue = file.LockValue, Version = null };
                }
                else
                {
                    return new LockFileResult { StatusCode = HttpStatusCode.Conflict, LockValue = Constants.EmptyLock, Version = null };
                }
            }
            else
            {
                if (file.IsLocked() && !file.IsSameLock(lockId))
                    return new LockFileResult { StatusCode = HttpStatusCode.Conflict, LockValue = file.LockValue, Version = null };

            }

            file.Lock(lockId, lockDurationMinutes);
            await db.UpdateFileLockAsync(file, auth);
            return new LockFileResult { StatusCode = HttpStatusCode.OK, LockValue = null, Version = file.Version.ToString() };
        }

        public async Task<GetLockStatusResult> GetLockStatus(string fileId, IAuthInfo auth)
        {
            var file = await db.GetFileAsync(fileId, auth);
            if (file == null)
                return new GetLockStatusResult { StatusCode = HttpStatusCode.NotFound, LockValue = null };

            if (file.IsLocked())
                return new GetLockStatusResult { StatusCode = HttpStatusCode.OK, LockValue = file.LockValue };
            else
                return new GetLockStatusResult { StatusCode = HttpStatusCode.OK, LockValue = Constants.EmptyLock };
        }

        public async Task<UnlockFileResult> UnlockFile(string fileId, IAuthInfo auth, string lockId)
        {
            if (string.IsNullOrEmpty(lockId))
                return new UnlockFileResult { StatusCode = HttpStatusCode.BadRequest, LockValue = null, Version = null };

            var file = await db.GetFileAsync(fileId, auth);
            if (file == null)
                return new UnlockFileResult { StatusCode = HttpStatusCode.NotFound, LockValue = null, Version = null };

            if (file.IsLocked())
            {
                if (!file.IsSameLock(lockId))
                    return new UnlockFileResult { StatusCode = HttpStatusCode.Conflict, LockValue = file.LockValue, Version = null };
            }
            else
                return new UnlockFileResult { StatusCode = HttpStatusCode.Conflict, LockValue = Constants.EmptyLock, Version = null };

            file.Unlock();
            await db.UpdateFileLockAsync(file, auth);
            return new UnlockFileResult { StatusCode = HttpStatusCode.OK, LockValue = null, Version = file.Version.ToString() };
        }
    }
}
