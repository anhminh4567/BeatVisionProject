using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.ConfigurationBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Configurations
{
    public class ResetPasswordTokenProvider<TUser>: DataProtectorTokenProvider<TUser> where TUser : class
    {
        public ResetPasswordTokenProvider(
            IDataProtectionProvider dataProtectionProvider,
            IOptions<ResetPasswordTokenProviderOptions> options,
            ILogger<DataProtectorTokenProvider<TUser>> logger): base(dataProtectionProvider, options, logger)
        {

        }
    }
    public class ResetPasswordTokenProviderOptions : DataProtectionTokenProviderOptions
    {
        //private readonly AppsettingBinding _appsettings;
        public ResetPasswordTokenProviderOptions()
        {
            //_appsettings = appsettingBinding;
            Name = "MyResetPasswordTokenProvider";
            //TokenLifespan = TimeSpan.FromMinutes(10);
        }
    }
}
