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
        public IList<EmailAttachments>? Attachments { get; set; }// require only when the send with attachment is used

		public EmailMetaData(string toEmail, string subject, IEnumerable<string>? bccs, IEnumerable<string>? ccs, string? bodyString, string? attachmentPath, IList<EmailAttachments>? attachments)
		{
			ToEmail = toEmail;
			Subject = subject;
			Bccs = bccs;
			Ccs = ccs;
			BodyString = bodyString;
			AttachmentPath = attachmentPath;
			Attachments = attachments;
		}

		public EmailMetaData()
        {
        }
    }
    public class EmailAttachments
    {
        public Stream FileStream { get; set; }
        public string ContentType { get; set; }
		public string? FileName { get; set; }


		public EmailAttachments()
		{
		}
	}
}
