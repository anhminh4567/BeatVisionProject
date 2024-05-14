using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interface
{
    public interface ISecurityTokenServices
    {
        (string accessToken,DateTime expiredDate) GenerateAccessToken(IEnumerable<Claim> claims);
        (string refreshToken,DateTime expiredDate) GenerateRefreshToken();
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    }

}
