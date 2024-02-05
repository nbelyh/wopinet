using System;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Caching.Memory;
using WopiCore.Models;
using System.Linq;
using Microsoft.Extensions.Logging;
using System.Text;

namespace WopiCore.Services
{

    /// <summary>
    /// Class handles token generation and validation for the WOPI host
    /// </summary>
    public class WopiSecurity
    {
        private readonly IWopiConfiguration wopiConfiguration;
        private readonly IMemoryCache memoryCache;
        private readonly ILogger<WopiSecurity> logger;

        public WopiSecurity(IMemoryCache memoryCache, IWopiConfiguration wopiConfiguration, ILogger<WopiSecurity> logger)
        {
            this.memoryCache = memoryCache;
            this.wopiConfiguration = wopiConfiguration;
            this.logger = logger;
        }

        /// <summary>
        /// Extracts the user information from a provided access token
        /// </summary>
        public IAuthInfo GetAuthInfoFromWopiRequest(WopiRequest request)
        {
            var secret = Encoding.ASCII.GetBytes(wopiConfiguration.GetWopiTokenSecret());

            // Initialize the token handler and validation parameters
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenValidation = new TokenValidationParameters()
            {
                ValidAudience = wopiConfiguration.GetWopiTokenAudience(),
                ValidIssuer = wopiConfiguration.GetWopiTokenIssuer(),
                IssuerSigningKey = new SymmetricSecurityKey(secret),
            };

            try
            {
                // Try to extract the user principal from the token
                var tokenString = request.AccessToken;
                var resource = request.ResourceId;
                var principal = tokenHandler.ValidateToken(tokenString, tokenValidation, out var token);

                if (principal == null)
                {
                    logger.LogWarning($"[WOPI] Invalid token {request.ResourceId}");
                    return null;
                }

                return wopiConfiguration.GetAuthInfo(request.ResourceId, principal.Claims);
            }
            catch (Exception e)
            {
                logger.LogWarning($"[WOPI] Invalid token {request.ResourceId}: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// Generates an access token for the user and file
        /// </summary>
        public JwtSecurityToken GenerateToken(string fileId, IAuthInfo auth)
        {
            var claims = wopiConfiguration.GetClaims(fileId, auth);

            var key = string.Join("#", claims.Select(c => c.Value));

            if (memoryCache.TryGetValue(key, out JwtSecurityToken existingToken))
                return existingToken;

            var secret = Encoding.ASCII.GetBytes(wopiConfiguration.GetWopiTokenSecret());
            var credentials = new SigningCredentials(new SymmetricSecurityKey(secret), SecurityAlgorithms.HmacSha256);

            var expires = DateTime.Now.AddMinutes(30);

            var newToken = new JwtSecurityToken(
                issuer: wopiConfiguration.GetWopiTokenAudience(),
                audience: wopiConfiguration.GetWopiTokenIssuer(),
                claims: claims,
                expires: expires,
                signingCredentials: credentials
            );

            memoryCache.Set(key, newToken, expires);
            return newToken;
        }

        public string WriteToken(JwtSecurityToken token)
        {
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
