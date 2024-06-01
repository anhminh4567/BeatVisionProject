using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.ConfigurationBinding
{
    public class AppsettingBinding
    {
        public ConnectionStrings ConnectionStrings { get; set; }
        public JwtSection JwtSection { get; set; }
        public ExternalAuthenticationSection ExternalAuthenticationSection {get;set;}
        public MailSettings MailSettings { get; set; }
        public ExternalUrls ExternalUrls { get; set; }
        public IEnumerable<MailTemplateRelativePath> MailTemplateRelativePath { get; set; }
        public IList<MailTemplateAbsolutePath> MailTemplateAbsolutePath { get; set; } = new List<MailTemplateAbsolutePath>();
		public AppConstraints AppConstraints { get; set; }
        public IList<DefaultRelativePath> DefaultContentRelativePath { get; set; }

	}
    public class JwtSection
    {
        public string Key { get; set; }
        public IEnumerable<string> Issuers { get; set; } = default!;
        public IEnumerable<string> Audiences { get; set; } = default!;
        public int ExpiredAccessToken_Minute { get; set; }
        public int ExpiredRefreshToken_Hour { get; set; }
    }
    public class ConnectionStrings 
    {
        public string DefaultConnectionString { get; set; }
        public string AzureBlobConnectionString { get; set; }
		public string CacheConnectionString { get; set; }
	}
    public class ExternalAuthenticationSection
    {
        public GoogleOption GoogleAuthenticationSection { get; set; } = default!;

    }
    public class GoogleOption
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
    }
    public class MailSettings
    {
        public string Host { get; set; }
        public int Port { get;set; }
        public string SenderName { get; set; }
        public string SenderEmail { get; set; }
        public string AppPassword { get; set; }
    }
    public class MailTemplateRelativePath
    {
        public string TemplateName { get; set; }
        public string TemplatePathWWWRoot { get; set; }
    }
	public class MailTemplateAbsolutePath
	{
		public string TemplateName { get; set; }
		public string TemplateAbsolutePath { get; set; }
	}
    public class ExternalUrls
    {
        public string AzureBlobBaseUrl { get; set; }
	}
    public class AppConstraints
    {
        public string[] AllowAudioExtension { get; set; }
		public string[] AllowImageExension { get; set; }
        public string[] AllowLicenseExtension { get; set; }
	}
    public class DefaultRelativePath
    {
        public string ContentName { get; set; }
		public string ContentPathWWWRoot { get; set; }

	}
}
