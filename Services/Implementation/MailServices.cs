using FluentEmail.Core;
using FluentEmail.Core.Models;
using Services.Interface;
using Shared;
using Shared.ConfigurationBinding;
using Shared.Helper;
using Shared.IdentityConfiguration;
using Shared.Models;
using Shared.Poco;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Services.Implementation
{
    public class MailServices : IMyEmailService
    {
        private readonly AppsettingBinding appsettingBinding;
        private readonly IFluentEmail _fluentEmailServices;
        private readonly string LOGO_DEFAULT_IMAGE;
        public MailServices(AppsettingBinding appsettingBinding, IFluentEmail fluentEmailServices)
        {
            this.appsettingBinding = appsettingBinding;
            _fluentEmailServices = fluentEmailServices;
            byte[] imageBytes = File.ReadAllBytes( (this.appsettingBinding.DefaultContentRelativePath.FirstOrDefault(d => d.ContentName == "DefaultLogo")) .ContentPathWWWRoot) ;
			LOGO_DEFAULT_IMAGE =  Convert.ToBase64String(imageBytes);
		}
        public async Task<Result> SendConfirmationEmail(CustomIdentityUser user,EmailMetaData emailMetaData, ConfirmEmailModel confirmEmailModel,CancellationToken cancellationToken = default)
        {
            var getConfirmationEmailTemplate = appsettingBinding.MailTemplateAbsolutePath
                .FirstOrDefault(p => p.TemplateName.Equals("ConfirmEmail"))?.TemplateAbsolutePath;
            if (getConfirmationEmailTemplate == null)
                return Result.Fail();
            return await SendEmailWithTemplate<ConfirmEmailModel>(emailMetaData,getConfirmationEmailTemplate,confirmEmailModel, cancellationToken);
            
        }
        public async Task<Result> SendEmailWithTemplate<T>(EmailMetaData emailMetaData, string templatePath,T templateModel, CancellationToken cancellation = default)
        {
			//emailMetaData.LogoImageBase64 = LOGO_DEFAULT_IMAGE;
			var emailSendingConfig = _fluentEmailServices
                .To(emailMetaData.ToEmail)
                .Subject(emailMetaData.Subject);
            emailSendingConfig.UsingTemplateFromFile(templatePath, templateModel);
            emailSendingConfig.Attach(new Attachment() 
            {
                ContentId = "logo",
                ContentType = "image/png",
                Data = new MemoryStream(File.ReadAllBytes((this.appsettingBinding.DefaultContentRelativePath.FirstOrDefault(d => d.ContentName == "DefaultLogo")).ContentPathWWWRoot)),
                IsInline = true,
                Filename = "none.png",
            });

            var sendResult =  await emailSendingConfig.SendAsync(cancellation);
            if (sendResult.Successful is false)
                return Result.Fail();
            return Result.Success();
        }
        public async Task<Result> SendEmailWithTemplate_WithAttachment<T>(EmailMetaData emailMetaData, string templatePath, T templateModel, CancellationToken cancellationToken = default)
        {
            var error = new Error();
			var emailSendingConfig = _fluentEmailServices
                .To(emailMetaData.ToEmail)
                .Subject(emailMetaData.Subject);
            emailSendingConfig.UsingTemplateFromFile(templatePath, templateModel);
            IList<Attachment> attachments = new List<Attachment>();
            if(emailMetaData.Attachments is null)
            {
                error.ErrorMessage = "no stream to send as attachment, consider using another alternative";
                return Result.Fail();
            }
            foreach (var attachment in emailMetaData.Attachments)
            {
				var newAttachment= new Attachment();
                newAttachment.Data = attachment.FileStream;
                newAttachment.ContentType = attachment.ContentType;
                newAttachment.IsInline = false;
                newAttachment.ContentId = Guid.NewGuid().ToString();
                newAttachment.Filename = attachment.FileName;
                attachments.Add(newAttachment);
            }
            foreach(var attachment in attachments)
            {
				emailSendingConfig.Attach(attachment);
			}
			var sendResult = await emailSendingConfig.SendAsync(cancellationToken);
			if (sendResult.Successful is false)
            {
                error.ErrorMessage = "send fail, unknown reason";
				return Result.Fail();
			}
			return Result.Success();

        }
    }
}
