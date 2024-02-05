using System.Security.Claims;
using System.Collections.Generic;
using Microsoft.IdentityModel.Tokens;

namespace WopiCore.Services
{
    public interface IWopiConfiguration
    {
        string GetWopiBackend(IAuthInfo auth);
        string GetWopiFrontend(IAuthInfo auth);
        string GetWopiDiscovery(IAuthInfo auth);

        IAuthInfo GetAuthInfo(string fileId, IEnumerable<Claim> principal);
        IEnumerable<Claim> GetClaims(string fileId, IAuthInfo auth);
        string GetWopiTokenIssuer();
        string GetWopiTokenAudience();
        string GetWopiTokenSecret();
    }
}
