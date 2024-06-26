﻿using Shared.Helper;
using Shared.IdentityConfiguration;
using Shared.Poco;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interface
{
    public interface IMyEmailService : IEmailService
	{
		Task<Result> SendConfirmationEmail(CustomIdentityUser user, EmailMetaData emailMetaData, ConfirmEmailModel confirmEmailModel, CancellationToken cancellationToken = default);
		Task<Result> SendEmailWithTemplate<T>(EmailMetaData emailMetaData, string templatePath, T templateModel, CancellationToken cancellation = default);
		Task<Result> SendEmailWithTemplate_WithAttachment<T>(EmailMetaData emailMetaData, string templatePath, T templateModel, CancellationToken cancellationToken = default);

	}
}
