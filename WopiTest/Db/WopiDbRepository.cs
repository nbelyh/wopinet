using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using WopiCore.Services;
using Microsoft.Extensions.Configuration;

namespace WopiTest
{
    public class WopiDbRepository : IWopiDbRepository
    {
        IConfiguration configuration;

        public WopiDbRepository(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public IDbConnection GetDbConnection(IAuthInfo auth)
        {
            var connectionString = configuration.GetSection("wopi").GetValue<string>("WopiSqlConnectionString");
            return new SqlConnection(connectionString);
        }

        public async Task<IEnumerable<WopiFile>> GetAllFilesAsync(IAuthInfo auth)
        {
            using (var db = GetDbConnection(auth))
            {
                return await db.QueryAsync<WopiFile>("SELECT * FROM WopiFiles");
            }
        }

        public async Task AddFileAsync(WopiFile wopiFile, IAuthInfo auth)
        {
            using (var db = GetDbConnection(auth))
            {
                var parameters = new DynamicParameters();
                parameters.Add("@FileId", wopiFile.FileId);
                parameters.Add("@LockValue", wopiFile.LockValue);
                parameters.Add("@LockExpires", wopiFile.LockExpires);
                parameters.Add("@OwnerId", wopiFile.OwnerId);
                parameters.Add("@FileName", wopiFile.FileName);
                parameters.Add("@Size", wopiFile.Size);
                parameters.Add("@Version", wopiFile.Version);
                parameters.Add("@LastModifiedTime", wopiFile.LastModifiedTime);
                parameters.Add("@LastModifiedUser", wopiFile.LastModifiedUser);
                string insertQuery = 
                    @"INSERT INTO [WopiFiles]
                        ([FileId], [LockValue], [LockExpires], [OwnerId], [FileName], [Size], [Version], [LastModifiedTime], [LastModifiedUser]) 
                        VALUES (@FileId, @LockValue, @LockExpires, @OwnerId, @FileName, @Size, @Version, @LastModifiedTime, @LastModifiedUser)";
                var result = await db.ExecuteAsync(insertQuery, parameters);
            }
        }

        public async Task UpdateFileInfoAsync(WopiFile wopiFile, IAuthInfo auth)
        {
            using (var db = GetDbConnection(auth))
            {
                var parameters = new DynamicParameters();
                parameters.Add("@FileId", wopiFile.FileId);
                parameters.Add("@Size", wopiFile.Size);
                parameters.Add("@Version", wopiFile.Version);
                parameters.Add("@LastModifiedTime", wopiFile.LastModifiedTime);
                parameters.Add("@LastModifiedUser", wopiFile.LastModifiedUser);
                string updateQuery = 
                    @"UPDATE [WopiFiles] 
                        SET [Size]=@Size, [Version]=@Version, [LastModifiedTime]=@LastModifiedTime, [LastModifiedUser]=@LastModifiedUser
                        WHERE [FileId]=@FileId";
                var result = await db.ExecuteAsync(updateQuery, parameters);
            }
        }

        public async Task UpdateFileLockAsync(WopiFile wopiFile, IAuthInfo auth)
        {
            using (var db = GetDbConnection(auth))
            {
                var parameters = new DynamicParameters();
                parameters.Add("@FileId", wopiFile.FileId);
                parameters.Add("@LockValue", wopiFile.LockValue);
                parameters.Add("@LockExpires", wopiFile.LockExpires);
                string updateQuery = 
                    @"UPDATE [WopiFiles] 
                        SET [LockValue]=@LockValue, [LockExpires]=@LockExpires
                        WHERE [FileId]=@FileId";
                var result = await db.ExecuteAsync(updateQuery, parameters);
            }
        }

        public async Task DeleteFileAsync(string fileId, IAuthInfo auth)
        {
            using (var db = GetDbConnection(auth))
            {
                var parameters = new DynamicParameters();
                parameters.Add("@FileId", fileId);
                string deleteQuery = @"DELETE FROM [WopiFiles] WHERE [FileId]=@FileId";
                var result = await db.ExecuteAsync(deleteQuery, parameters);
            }
        }

        public async Task<WopiFile> GetFileAsync(string fileId, IAuthInfo auth)
        {
            using (var db = GetDbConnection(auth))
            {
                var parameters = new DynamicParameters();
                parameters.Add("@FileId", fileId);
                var result = await db.QuerySingleOrDefaultAsync<WopiFile>(
                    @"SELECT [FileId], [LockValue], [LockExpires], [OwnerId], [FileName], [Size], [Version], [LastModifiedTime], [LastModifiedUser]
                    FROM [WopiFiles]
                    WHERE [FileId]=@FileId",
                    parameters);
                return result;
            }
        }
    }
}
