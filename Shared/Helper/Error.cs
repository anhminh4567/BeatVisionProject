using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Helper
{
    public class Error
    {
        public bool isException { get; set; } = false;
        public Exception? Exception { get; set; }
        public int StatusCode { get; set; } = 400;
        public string? ErrorMessage { get; set; }
    }
}
