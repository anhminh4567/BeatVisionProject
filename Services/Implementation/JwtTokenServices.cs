using Microsoft.IdentityModel.Tokens;
using Services.Interface;
using Shared;
using Shared.ConfigurationBinding;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Services.Implementation
{
    public class JwtTokenServices : ISecurityTokenServices
    {
        private readonly AppsettingBinding _appsettingBinding;
        private readonly JwtSection _jwtSetting;

        public JwtTokenServices(AppsettingBinding appsettingBinding)
        {
            _appsettingBinding = appsettingBinding;
            if (appsettingBinding is null)
                throw new ArgumentNullException("appsetting is null");
            _jwtSetting = _appsettingBinding.JwtSection;
        }

        public (string accessToken, DateTime expiredDate) GenerateAccessToken(IEnumerable<Claim> claims)
        {
            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSetting.Key));
            var signinCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);
            var expiredTime = DateTime.Now.AddMinutes(_jwtSetting.ExpiredAccessToken_Minute);
            var tokeOptions = new JwtSecurityToken(
                issuer: _jwtSetting.Issuers.FirstOrDefault(issuers => issuers.Equals(ApplicationStaticValue.ApplicationTokenIssuer)),
                audience: _jwtSetting.Audiences.First(),
                claims: claims,
                expires: expiredTime,
                signingCredentials: signinCredentials
            ); ;

            var tokenString = new JwtSecurityTokenHandler().WriteToken(tokeOptions);
            return (tokenString, expiredTime);
        }

        public (string refreshToken, DateTime expiredDate ) GenerateRefreshToken()
        {
            //var randomNumber = new byte[32];
            //using (var rng = RandomNumberGenerator.Create())
            //{
            //    rng.GetBytes(randomNumber);
            //}
            return (Convert.ToBase64String(Guid.NewGuid().ToByteArray()), 
                DateTime.Now.AddHours(_jwtSetting.ExpiredRefreshToken_Hour));

        }

        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = true, //you might want to validate the audience and issuer depending on your use case
                ValidateIssuer = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSetting.Key)),
                ValidateLifetime = false, //here we are saying that we don't care about the token's expiration date
                ValidIssuers = _jwtSetting.Issuers,
                ValidAudiences = _jwtSetting.Audiences,
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken;
            ClaimsPrincipal principal;
            try
            {
                principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
            }
            catch (SecurityTokenValidationException ex)
            {
                return null;
            }
            var jwtSecurityToken = securityToken as JwtSecurityToken;
            if (jwtSecurityToken == null || 
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");

            return principal;
        }
    }
}
