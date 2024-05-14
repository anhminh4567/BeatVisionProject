using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Poco
{
    public class EmailMetaData
    {
        public string ToEmail { get; set; }
        public string Subject { get; set; }
        public IEnumerable<string>? Bccs { get; set; }   // do not required if it is to single person
        public IEnumerable<string>? Ccs { get; set; }
        public string? BodyString { get; set; }// do not require if the body is html
        public string? AttachmentPath { get; set; }

        public EmailMetaData(string toEmail, string subject, IEnumerable<string>? bccs, IEnumerable<string>? ccs, string? bodyString, string? attachmentPath)
        {
            ToEmail = toEmail;
            Subject = subject;
            Bccs = bccs;
            Ccs = ccs;
            BodyString = bodyString;
            AttachmentPath = attachmentPath;
        }

        public EmailMetaData()
        {
        }
    }
}
