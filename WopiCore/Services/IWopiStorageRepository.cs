using System.IO;
using System.Threading.Tasks;

namespace WopiCore.Services
{
    public interface IWopiStorageRepository
    {
        Task PutBlobContentAsync(string fileId, Stream content, IAuthInfo auth);
        Task<bool> DeleteBlobAsync(string fileId, IAuthInfo auth);
        Task<Stream> GetBlobContentAsync(string fileId, IAuthInfo auth);
    }
}