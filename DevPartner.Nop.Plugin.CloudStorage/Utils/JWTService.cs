using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace DevPartner.Nop.Plugin.CloudStorage.Services
{
    public class JWTService
    {

        /// <summary>
        /// Use the below code to generate symmetric Secret Key
        ///     var hmac = new HMACSHA256();
        ///     var key = Convert.ToBase64String(hmac.Key);
        /// </summary>
        private const string Secret = "Yjwx3Po0^F16stTdh!vag8\"RWj51zFbr4eW]v47P_aTuf";
        
        public static bool ValidateLicense(string token, string productName, string domain, string ip)
        {

            var simplePrinciple = GetPrincipal(token);

            var identity = simplePrinciple?.Identity as ClaimsIdentity;

            if (identity == null)
                return false;

            if (!identity.IsAuthenticated)
                return false;

            var customerIdClaim = identity.FindFirst(ClaimTypes.Name);

            if (customerIdClaim == null || !customerIdClaim.Value.Equals(productName, StringComparison.InvariantCulture))
                return false;

            var aud = identity.FindAll("aud");

            var enumerable = aud as Claim[] ?? aud.ToArray();
            if (enumerable.All(x => !String.Equals(x.Value, domain, StringComparison.CurrentCultureIgnoreCase)) 
                && enumerable.All(x => !String.Equals(x.Value, ip, StringComparison.CurrentCultureIgnoreCase)))
                return false;

            return true;
        }

        public static ClaimsPrincipal GetPrincipal(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadToken(token) as JwtSecurityToken;

                if (jwtToken == null)
                    return null;

                var symmetricKey = Encoding.ASCII.GetBytes(Secret);

                var validationParameters = new TokenValidationParameters()
                {
                    RequireExpirationTime = true,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    IssuerSigningKey = new SymmetricSecurityKey(symmetricKey)
                };

                SecurityToken securityToken;
                var principal = tokenHandler.ValidateToken(token, validationParameters, out securityToken);

                return principal;
            }

            catch (Exception)
            {
                //should write log
                return null;
            }
        }
       
    }
}
