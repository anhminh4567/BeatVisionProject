using FluentEmail.Core;
using Services.Interface;
using Shared.ConfigurationBinding;
using Shared.Helper;
using Shared.IdentityConfiguration;
using Shared.Poco;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Implementation
{
    public class MailServices : IMyEmailService
    {
        private readonly AppsettingBinding appsettingBinding;
        private readonly IFluentEmail _fluentEmailServices;

        public MailServices(AppsettingBinding appsettingBinding, IFluentEmail fluentEmailServices)
        {
            this.appsettingBinding = appsettingBinding;
            _fluentEmailServices = fluentEmailServices;
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
            var emailSendingConfig = _fluentEmailServices
                .To(emailMetaData.ToEmail)
                .Subject(emailMetaData.Subject);
            emailSendingConfig.UsingTemplateFromFile(templatePath, templateModel);
           var sendResult =  await emailSendingConfig.SendAsync(cancellation);
            if (sendResult.Successful is false)
                return Result.Fail();
            return Result.Success();
        }
    }
}
