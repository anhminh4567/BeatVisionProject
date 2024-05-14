using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.ResponseDto
{
    public class BlobFileResponseDto
    {
        public Stream Stream { get; set; }
        public string ContentType { get; set; }
    }
}
