using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using WopiCore.Services;

namespace WopiTest
{
    public class AuthInfo : IAuthInfo
    {
        public string UserId { get; set; }
        public string UserFriendlyName { get; set; }
    }

    public class WopiConfiguration : IWopiConfiguration
    {
        private readonly IConfiguration configuration;
        public WopiConfiguration(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public string GetWopiBackend(IAuthInfo auth) => GetConfigString("WopiBackend");
        public string GetWopiFrontend(IAuthInfo auth) => GetConfigString("WopiFrontend");
        public string GetWopiDiscovery(IAuthInfo auth) => GetConfigString("WopiDiscovery");

        private string GetConfigString(string key)
        {
            return configuration.GetSection("wopi").GetValue<string>(key);
        }

        public IAuthInfo GetAuthInfo(string requestedFileId, IEnumerable<Claim> claims)
        {
            var userId = claims.FirstOrDefault(c => c.Type == "userid")?.Value;
            var userName = claims.FirstOrDefault(c => c.Type == "username")?.Value;
            var fileId = claims.FirstOrDefault(c => c.Type == "fileid")?.Value;

            if (fileId != requestedFileId)
            {
                System.Diagnostics.Trace.TraceWarning($"[WOPI] Invalid token (file mismatch) {requestedFileId}");
                return null;
            }

            return new AuthInfo
            {
                UserId = userId,
                UserFriendlyName = userName
            };
        }

        public IEnumerable<Claim> GetClaims(string fileId, IAuthInfo auth)
        {
            return new List<Claim>
            {
                new Claim("userid", auth.UserId),
                new Claim("username", auth.UserFriendlyName),
                new Claim("fileid", fileId),
            };
        }

        public string GetWopiTokenIssuer()
        {
            return GetConfigString("WopiTokenIssuer");
        }

        public string GetWopiTokenAudience()
        {
            return GetConfigString("WopiTokenAudience");
        }

        public string GetWopiTokenSecret()
        {
            return GetConfigString("WopiTokenSecret");
        }
    }
}
