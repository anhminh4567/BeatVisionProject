using Shared.Helper;
using Shared.Poco;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interface
{
    public interface IEmailService
	{
		Task<Result> SendEmailWithTemplate<T>(EmailMetaData emailMetaData, string templatePath, T templateModel, CancellationToken cancellation = default);
	}
}
