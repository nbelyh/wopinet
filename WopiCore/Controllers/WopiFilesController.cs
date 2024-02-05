using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.IO;
using System.Text;
using Microsoft.Extensions.Configuration;
using WopiCore.Services;
using WopiCore.Models;
using Microsoft.Extensions.Logging;

namespace WopiCore.Controllers
{
    [ApiController]
    public class WopiFilesController : ControllerBase
    {
        IWopiFileRepository wopiFileRepository;
        WopiSecurity wopiSecurity;
        WopiDiscovery wopiDiscovery;
        IWopiConfiguration wopiConfig;
        ILogger<WopiFilesController> logger;

        public WopiFilesController(
            IWopiFileRepository wopiFileRepository,
            WopiSecurity wopiSecurity,
            WopiDiscovery wopiDiscovery,
            IWopiConfiguration wopiConfig,
            ILogger<WopiFilesController> logger)
        {
            this.wopiFileRepository = wopiFileRepository;
            this.wopiSecurity = wopiSecurity;
            this.wopiDiscovery = wopiDiscovery;
            this.wopiConfig = wopiConfig;
            this.logger = logger;
        }

        [HttpGet]
        [Route("wopi/files/{file_id}")]
        public async Task<IActionResult> CheckFileInfo(string file_id)
        {
            var checkFileInfoRequest = new CheckFileInfoRequest(Request, file_id);
            WopiResponse wopiResponse = null;
            try
            {
                var auth = await Authorize(checkFileInfoRequest);
                if (auth != null)
                {
                    if (await wopiDiscovery.Validate(checkFileInfoRequest, auth))
                    {
                        logger.LogInformation($"[WOPI] CHECK_FILE_INFO {file_id}");
                        wopiResponse = await CheckFileInfo(checkFileInfoRequest, auth);
                    }
                    else
                    {
                        logger.LogWarning($"[WOPI] CHECK_FILE_INFO {file_id} proof validation failed");
                        wopiResponse = checkFileInfoRequest.ResponseServerError("Proof validation failed");
                    }
                }
                else
                {
                    logger.LogWarning($"[WOPI] CHECK_FILE_INFO {file_id} unauthorized");
                    wopiResponse = checkFileInfoRequest.ResponseUnauthorized();
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"[WOPI] CHECK_FILE_INFO {file_id} failed {ex.Message}");
                wopiResponse = checkFileInfoRequest.ResponseServerError(ex.Message);
            }

            if (wopiResponse.StatusCode == HttpStatusCode.OK)
                logger.LogInformation($"[WOPI] CHECK_FILE_INFO {file_id} OK");
            else
                logger.LogWarning($"[WOPI] CHECK_FILE_INFO {file_id} returning {wopiResponse.StatusCode}");

            return wopiResponse.ToResult(Response);
        }

        [HttpGet]
        [Route("wopi/files/{file_id}/contents")]
        public async Task<IActionResult> GetFile(string file_id)
        {
            var getFileRequest = new GetFileRequest(Request, file_id);
            WopiResponse wopiResponse = null;
            try
            {
                var auth = await Authorize(getFileRequest);
                if (auth != null)
                {
                    if (await wopiDiscovery.Validate(getFileRequest, auth))
                    {
                        logger.LogInformation($"[WOPI] GET_FILE {file_id}");
                        wopiResponse = await GetFile(getFileRequest, auth);
                    }
                    else
                    {
                        logger.LogWarning($"[WOPI] GET_FILE {file_id} proof validation failed");
                        getFileRequest.ResponseServerError("Proof validation failed");
                    }
                }
                else
                {
                    logger.LogWarning($"[WOPI] GET_FILE {file_id} unauthorized");
                    wopiResponse = getFileRequest.ResponseUnauthorized();
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"[WOPI] GET_FILE {file_id} failed {ex.Message}");
                wopiResponse = getFileRequest.ResponseServerError(ex.Message);
            }

            if (wopiResponse.StatusCode == HttpStatusCode.OK)
                logger.LogInformation($"[WOPI] GET_FILE {file_id} OK");
            else
                logger.LogWarning($"[WOPI] GET_FILE {file_id} returning {wopiResponse.StatusCode}");

            return wopiResponse.ToResult(Response);
        }

        [HttpPost]
        [Route("wopi/files/{file_id}")]
        public async Task<IActionResult> ProcessPostActions(string file_id)
        {
            WopiRequest wopiRequest = new WopiRequest(Request, file_id);
            WopiResponse wopiResponse = null;

            var filesPostOverride = WopiRequest.GetHttpRequestHeader(Request, WopiRequestHeaders.OVERRIDE);
            logger.LogInformation($"[WOPI] {filesPostOverride} {file_id}");

            try
            {
                var auth = await Authorize(wopiRequest);
                if (auth != null)
                {
                    if (await wopiDiscovery.Validate(wopiRequest, auth))
                    {
                        switch (filesPostOverride)
                        {
                            case "LOCK":
                                var oldLock = WopiRequest.GetHttpRequestHeader(Request, WopiRequestHeaders.OLD_LOCK);
                                if (oldLock != null)
                                {
                                    wopiResponse = await UnlockAndRelock(new UnlockAndRelockRequest(Request, file_id), auth);
                                }
                                else
                                {
                                    wopiResponse = await Lock(new LockRequest(Request, file_id), auth);
                                }
                                break;
                            case "GET_LOCK":
                                wopiResponse = await GetLock(new GetLockRequest(Request, file_id), auth);
                                break;
                            case "REFRESH_LOCK":
                                wopiResponse = await RefreshLock(new RefreshLockRequest(Request, file_id), auth);
                                break;
                            case "UNLOCK":
                                wopiResponse = await Unlock(new UnlockRequest(Request, file_id), auth);
                                break;
                            case "PUT_RELATIVE":
                                wopiResponse = new WopiResponse { StatusCode = HttpStatusCode.NotImplemented };
                                break;
                            case "RENAME_FILE":
                                wopiResponse = new WopiResponse { StatusCode = HttpStatusCode.NotImplemented };
                                // wopiResponse = await RenameFile(new RenameFileRequest(Request, file_id));
                                break;
                            case "PUT_USER_INFO":
                                wopiResponse = new WopiResponse { StatusCode = HttpStatusCode.NotImplemented };
                                //using (var reader = new StreamReader(Request.Body, Encoding.UTF8))
                                //{
                                //    var user_info = await reader.ReadToEndAsync();
                                //    wopiResponse = await PutUserInfo(new PutUserInfoRequest(Request, file_id, user_info));
                                //}
                                break;
                            case "DELETE":
                                wopiResponse = new WopiResponse { StatusCode = HttpStatusCode.NotImplemented };
                                // wopiResponse = await DeleteFile(new DeleteFileRequest(Request, file_id), auth);
                                break;
                            default:
                                wopiResponse = wopiRequest.ResponseServerError(string.Format("Invalid {0} header value: {1}", WopiRequestHeaders.OVERRIDE, filesPostOverride));
                                break;
                        }
                    }
                    else
                    {
                        logger.LogWarning($"[WOPI] {filesPostOverride} {file_id} proof validation failed");
                        wopiResponse = wopiRequest.ResponseServerError("proof validation failed");
                    }
                }
                else
                {
                    logger.LogWarning($"[WOPI] {filesPostOverride} {file_id} unauthorized");
                    wopiResponse = wopiRequest.ResponseUnauthorized();
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"[WOPI] {filesPostOverride} {file_id} failed {ex.Message}");
                wopiResponse = wopiRequest.ResponseServerError(ex.Message);
            }

            if (wopiResponse.StatusCode == HttpStatusCode.OK)
                logger.LogInformation($"[WOPI] {filesPostOverride} {file_id} OK");
            else
                logger.LogWarning($"[WOPI] {filesPostOverride} {file_id} returning {wopiResponse.StatusCode}");

            return wopiResponse.ToResult(Response);
        }

        [HttpPost]
        [Route("wopi/files/{file_id}/contents")]
        public async Task<IActionResult> PutFile(string file_id)
        {
            var putFileRequest = new PutFileRequest(Request, file_id);
            WopiResponse wopiResponse = null;
            try
            {
                var auth = await Authorize(putFileRequest);
                if (auth != null)
                {
                    if (await wopiDiscovery.Validate(putFileRequest, auth))
                    {
                        logger.LogInformation($"[WOPI] PUT_FILE {file_id}");
                        wopiResponse = await PutFile(putFileRequest, auth);
                    }
                    else
                    {
                        logger.LogWarning($"[WOPI] PUT_FILE {file_id} proof validation failed");
                        wopiResponse = putFileRequest.ResponseServerError("Proof validation failed");
                    }
                }
                else
                {
                    logger.LogWarning($"[WOPI] PUT_FILE {file_id} unauthorized");
                    wopiResponse = putFileRequest.ResponseUnauthorized();
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"[WOPI] PUT_FILE {file_id} failed {ex.Message}");
                wopiResponse = putFileRequest.ResponseServerError(ex.Message);
            }

            if (wopiResponse.StatusCode == HttpStatusCode.OK)
                logger.LogInformation($"[WOPI] PUT_FILE {file_id} OK");
            else
                logger.LogWarning($"[WOPI] PUT_FILE {file_id} returning {wopiResponse.StatusCode}");

            return wopiResponse.ToResult(Response);
        }

        private async Task<IAuthInfo> Authorize(WopiRequest wopiRequest)
        {
            try
            {
                if (string.IsNullOrEmpty(wopiRequest.AccessToken))
                {
                    logger.LogWarning($"[WOPI] Authrozie {wopiRequest.ResourceId} failed, no access token");
                    return null;
                }

                // Validate the access token contains authenticated user
                // We're only doing authentication here and deferring authorization to the other WOPI operations
                // to avoid multiple DB queries
                return wopiSecurity.GetAuthInfoFromWopiRequest(wopiRequest);
            }
            catch (Exception ex)
            {
                logger.LogError($"[WOPI] Authrozie {wopiRequest.ResourceId} failed {ex.Message}");
                // Any exception will return false, but should probably return an alternate status codes
                return null;
            }
        }

        private async Task<WopiResponse> CheckFileInfo(CheckFileInfoRequest checkFileInfoRequest, IAuthInfo auth)
        {
            // Lookup the file in the database using special repository method which grants access limited access to users in same tenant (same email domain)
            var result = await wopiFileRepository.GetFileInfo(checkFileInfoRequest.ResourceId, auth);

            // Check for null file
            if (result.StatusCode == HttpStatusCode.NotFound)
                return checkFileInfoRequest.ResponseNotFound();

            else if (result.StatusCode == HttpStatusCode.OK)
            {
                var host = wopiConfig.GetWopiBackend(auth);
                var wopiFile = result.File;
                var closeUrl = host;
                var fileExt = Path.GetExtension(wopiFile.FileName);
                // Write the Response and return a success 200
                var wopiResponse = checkFileInfoRequest.ResponseOK(wopiFile.FileName, wopiFile.OwnerId, wopiFile.Size, auth.UserId, auth.UserFriendlyName, wopiFile.Version.ToString());
                // Add optional items
                wopiResponse.CloseUrl = new Uri(closeUrl);
                wopiResponse.ClosePostMessage = true;
                wopiResponse.PostMessageOrigin = wopiConfig.GetWopiFrontend(auth);
                wopiResponse.HostViewUrl = new Uri($"{host}/view/{fileExt}/{wopiFile.FileId}");
                wopiResponse.HostEditUrl = new Uri($"{host}/edit/{fileExt}/{wopiFile.FileId}");

                // wopiResponse.UserInfo = wopiFile.FilePermissions.First().UserInfo;

                return wopiResponse;
            }
            else
                return checkFileInfoRequest.ResponseServerError(string.Format("Unknown Response from CheckFileInfo: {0}", result.StatusCode));
        }

        private async Task<WopiResponse> GetFile(GetFileRequest getFileRequest, IAuthInfo auth)
        {
            // Lookup the file in the database
            var result = await wopiFileRepository.GetFileContent(getFileRequest.ResourceId, auth);

            // Check for null file
            if (result.StatusCode == HttpStatusCode.NotFound)
                return getFileRequest.ResponseNotFound();
            else
                // Write the Response and return success 200
                return getFileRequest.ResponseOK(result.Stream, result.Version);
        }

        private async Task<WopiResponse> Lock(LockRequest lockRequest, IAuthInfo auth)
        {
            var result = await wopiFileRepository.LockFile(lockRequest.ResourceId, auth, lockRequest.Lock, null);

            if (result.StatusCode == HttpStatusCode.BadRequest)
                return lockRequest.ResponseBadRequest();
            // Check for file not found or no permissions
            else if (result.StatusCode == HttpStatusCode.NotFound)
                return lockRequest.ResponseNotFound();
            // Ensure the file isn't already locked
            else if (result.StatusCode == HttpStatusCode.Conflict)
                return lockRequest.ResponseLockConflict(result.LockValue);
            // File successfully locked
            else if (result.StatusCode == HttpStatusCode.OK)
                return lockRequest.ResponseOK(result.Version);
            else
                return lockRequest.ResponseServerError(string.Format("Unknown HTTPStatusCode from WopiFileRepository.LockFile: {0}", result.StatusCode));
        }

        private async Task<WopiResponse> Unlock(UnlockRequest unlockRequest, IAuthInfo auth)
        {
            var result = await wopiFileRepository.UnlockFile(unlockRequest.ResourceId, auth, unlockRequest.Lock);

            if (result.StatusCode == HttpStatusCode.BadRequest)
                return unlockRequest.ResponseBadRequest();
            // Check for file not found or no permissions
            else if (result.StatusCode == HttpStatusCode.NotFound)
                return unlockRequest.ResponseNotFound();
            // Ensure the file isn't already locked
            else if (result.StatusCode == HttpStatusCode.Conflict)
                return unlockRequest.ResponseLockConflict(result.LockValue);
            // File successfully unlocked
            else if (result.StatusCode == HttpStatusCode.OK)
                return unlockRequest.ResponseOK(result.Version);
            else
                return unlockRequest.ResponseServerError(string.Format("Unknown HTTPStatusCode from WopiFileRepository.UnlockFile: {0}", result.StatusCode));
        }

        private async Task<WopiResponse> UnlockAndRelock(UnlockAndRelockRequest unlockAndRelockRequest, IAuthInfo auth)
        {
            var result = await wopiFileRepository.LockFile(unlockAndRelockRequest.ResourceId, auth, unlockAndRelockRequest.Lock, unlockAndRelockRequest.OldLock);

            if (result.StatusCode == HttpStatusCode.BadRequest)
                return unlockAndRelockRequest.ResponseBadRequest();
            // Check for file not found or no permissions
            else if (result.StatusCode == HttpStatusCode.NotFound)
                return unlockAndRelockRequest.ResponseNotFound();
            // Ensure the file isn't already locked
            else if (result.StatusCode == HttpStatusCode.Conflict)
                return unlockAndRelockRequest.ResponseLockConflict(result.LockValue);
            // File successfully locked
            else if (result.StatusCode == HttpStatusCode.OK)
                return unlockAndRelockRequest.ResponseOK();
            else
                return unlockAndRelockRequest.ResponseServerError(string.Format("Unknown HTTPStatusCode from WopiFileRepository.LockFile: {0}", result.StatusCode));
        }


        private async Task<WopiResponse> RefreshLock(RefreshLockRequest refreshLockRequest, IAuthInfo auth)
        {
            var result = await wopiFileRepository.LockFile(refreshLockRequest.ResourceId, auth, refreshLockRequest.Lock, null);

            if (result.StatusCode == HttpStatusCode.BadRequest)
                return refreshLockRequest.ResponseBadRequest();
            // Check for file not found or no permissions
            else if (result.StatusCode == HttpStatusCode.NotFound)
                return refreshLockRequest.ResponseNotFound();
            // Ensure the file isn't already locked
            else if (result.StatusCode == HttpStatusCode.Conflict)
                return refreshLockRequest.ResponseLockConflict(result.LockValue);
            // File successfully locked
            else if (result.StatusCode == HttpStatusCode.OK)
                return refreshLockRequest.ResponseOK();
            else
                return refreshLockRequest.ResponseServerError(string.Format("Unknown HTTPStatusCode from WopiFileRepository.LockFile: {0}", result.StatusCode));
        }

        private async Task<WopiResponse> GetLock(GetLockRequest getLockRequest, IAuthInfo auth)
        {
            var result = await wopiFileRepository.GetLockStatus(getLockRequest.ResourceId, auth);

            // Check for file not found or no permissions
            if (result.StatusCode == HttpStatusCode.NotFound)
                return getLockRequest.ResponseNotFound();
            // Ensure the file isn't already locked
            else if (result.StatusCode == HttpStatusCode.Conflict)
                return getLockRequest.ResponseLockConflict(result.LockValue);
            // File successfully locked
            else if (result.StatusCode == HttpStatusCode.OK)
            {
                if (result.LockValue != null)
                    return getLockRequest.ResponseFileLocked(result.LockValue);
                else
                    return getLockRequest.ResponseFileNotLocked();
            }
            else
                return getLockRequest.ResponseServerError(string.Format("Unknown HTTPStatusCode from WopiFileRepository.GetLockStatus: {0}", result.StatusCode));

        }

        //private async Task<WopiResponse> RenameFile(RenameFileRequest renameFileRequest)
        //{
        //    var userId = wopiSecurity.GetIdentityNameFromToken(renameFileRequest.AccessToken);

        //    var result = await wopiFileRepository.RenameFile(renameFileRequest.ResourceId, userId, renameFileRequest.Lock, renameFileRequest.RequestedName);
        //    if (result.StatusCode == HttpStatusCode.NotFound)
        //        return renameFileRequest.ResponseNotFound();
        //    else if (result.StatusCode == HttpStatusCode.Conflict)
        //        return renameFileRequest.ResponseLockConflict(result.LockValue);
        //    else if (result.StatusCode == HttpStatusCode.BadRequest)
        //        return renameFileRequest.ResponseBadRequest(result.ErrorMessage);
        //    else if (result.StatusCode == HttpStatusCode.OK)
        //        return renameFileRequest.ResponseOK(result.NewBaseFileName);
        //    else
        //        return renameFileRequest.ResponseServerError(string.Format("Unknown HTTPStatusCode from WopiFileRepository.RenameFile: {0}", result.StatusCode));

        //}

        //private async Task<WopiResponse> PutUserInfo(PutUserInfoRequest putUserInfoRequest)
        //{
        //    var userId = wopiSecurity.GetIdentityNameFromToken(putUserInfoRequest.AccessToken);
        //    var result = await wopiFileRepository.SaveWopiUserInfo(putUserInfoRequest.ResourceId, userId, putUserInfoRequest.UserInfo);
        //    if (result.StatusCode == HttpStatusCode.NotFound)
        //        return putUserInfoRequest.ResponseNotFound();
        //    else if (result.StatusCode == HttpStatusCode.OK)
        //        return putUserInfoRequest.ResponseOK();
        //    else
        //        return putUserInfoRequest.ResponseServerError(string.Format("Unknown HTTPStatusCode from WopiFileRepository.SaveWopiUserInfo: {0}", result.StatusCode));
        //}

        private async Task<WopiResponse> PutFile(PutFileRequest putFileRequest, IAuthInfo auth)
        {
            var result = await wopiFileRepository.UpdateFileContent(putFileRequest.ResourceId, auth, putFileRequest.Lock, putFileRequest.Content, putFileRequest.ContentLength);
            if (result.StatusCode == HttpStatusCode.NotFound)
                return putFileRequest.ResponseNotFound();
            else if (result.StatusCode == HttpStatusCode.Conflict)
                return putFileRequest.ResponseLockConflict(result.LockValue);
            else if (result.StatusCode == HttpStatusCode.OK)
                return putFileRequest.ResponseOK(result.Version);
            else
                return putFileRequest.ResponseServerError(string.Format("Unknown HTTPStatusCode from WopiFileRepository.UpdateFileContent: {0}", result.StatusCode));
        }

        //private async Task<WopiResponse> DeleteFile(DeleteFileRequest deleteFileRequest, AuthInfo auth)
        //{
        //    var result = await wopiFileRepository.DeleteFile(deleteFileRequest.ResourceId, auth);
        //    if (result == HttpStatusCode.NotFound)
        //        return deleteFileRequest.ResponseNotFound();
        //    else if (result == HttpStatusCode.OK)
        //        return deleteFileRequest.ResponseOK();
        //    else
        //        return deleteFileRequest.ResponseServerError(string.Format("Unknown HTTPStatusCode from WopiFileRepository.UpdateFileContent: {0}", result));
        //}
    }
}
