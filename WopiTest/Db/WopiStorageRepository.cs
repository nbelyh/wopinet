using System.Threading.Tasks;
using WopiCore.Services;
using System.IO;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;

namespace WopiTest
{
    public class WopiStorageRepository : IWopiStorageRepository
    {
        IConfiguration configuration;

        public WopiStorageRepository(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public BlobClient GetBlob(string fileId, IAuthInfo auth)
        {
            string clientRelevantContainer = configuration.GetSection("wopi").GetValue<string>("WopiStorageContainer");
            string clientStorage = configuration.GetSection("wopi").GetValue<string>("WopiStorageConnectionString");
            return new BlobClient(clientStorage, clientRelevantContainer, fileId);
        }

        public async Task<BlobClient> AddBlobAsync(string fileId, Stream stream, IAuthInfo auth)
        {
            var client = GetBlob(fileId, auth);
            await client.UploadAsync(stream);
            return client;
        }

        public async Task<bool> DeleteBlobAsync(string fileId, IAuthInfo auth)
        {
            var client = GetBlob(fileId, auth);
            if (!await client.ExistsAsync())
                return false;

            await client.DeleteAsync();
            return true;
        }

        public async Task PutBlobContentAsync(string fileId, Stream content, IAuthInfo auth)
        {
            var client = GetBlob(fileId, auth);
            await client.UploadAsync(content);
        }

        public async Task<Stream> GetBlobContentAsync(string fileId, IAuthInfo auth)
        {
            var client = GetBlob(fileId, auth);
            if (!await client.ExistsAsync())
                return null;
            return await client.OpenReadAsync();
        }
    }
}
