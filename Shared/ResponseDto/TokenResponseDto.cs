using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.ResponseDto
{
    public class TokenResponseDto
    {
        public string AccessToken { get; set; }
        public DateTime AccessToken_Expired { get; set; }
        public string RefreshToken { get; set; }
        public DateTime RefreshToken_Expired { get; set; }
    }
}
