using System.Collections.Generic;
using System.Threading.Tasks;

namespace WopiCore.Services
{
    public interface IWopiDbRepository
    {
        Task AddFileAsync(WopiFile wopiFile, IAuthInfo auth);
        Task DeleteFileAsync(string fileId, IAuthInfo auth);
        Task<IEnumerable<WopiFile>> GetAllFilesAsync(IAuthInfo auth);
        Task<WopiFile> GetFileAsync(string fileId, IAuthInfo auth);
        Task UpdateFileInfoAsync(WopiFile wopiFile, IAuthInfo auth);
        Task UpdateFileLockAsync(WopiFile wopiFile, IAuthInfo auth);
    }
}