using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Repository;
using Services.Implementation;
using Services.Interface;
using Shared.ConfigurationBinding;
using System.Text;
using Microsoft.AspNetCore.Authentication.Google;
using Shared;
using Microsoft.AspNetCore.Identity;
using Azure.Storage.Blobs;
using StackExchange.Redis;
using Microsoft.Extensions.Caching.Distributed;
using Quartz;
using Services.BackgroundServices;
namespace Services
{
    public static class RegisterDependency
    {
        public static IServiceCollection AddServicesLayer(this IServiceCollection services, AppsettingBinding appsettingBinding)
        {
            services.AddRepositoryService();
            services.AddScoped<ISecurityTokenServices,JwtTokenServices>();
            services.AddScoped<IUserIdentityService, UserIdentityServices>();
            services.AddAuthentication(opt => 
            {
                opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, config =>
            {
                var jwtSection = appsettingBinding.JwtSection;
                config.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidateIssuerSigningKey = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidIssuers = jwtSection.Issuers,
                    ValidAudiences = jwtSection.Audiences,
                    RoleClaimType = "role",
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection.Key)),
                };
                config.SaveToken = true;
            }).AddGoogle(ApplicationStaticValue.GoogleScheme, opt =>
            {
                //var externalAuthSection = builder.Configuration.GetSection("ExternalAuthenticationSection").Get<ExternalAuthenticationSection>();
                var externalAuthSection = appsettingBinding.ExternalAuthenticationSection;
                opt.ClientId = externalAuthSection.GoogleAuthenticationSection.ClientId;
                opt.ClientSecret = externalAuthSection.GoogleAuthenticationSection.ClientSecret;
                opt.AuthorizationEndpoint = GoogleDefaults.AuthorizationEndpoint;
                opt.UserInformationEndpoint = GoogleDefaults.UserInformationEndpoint;
                opt.TokenEndpoint = GoogleDefaults.TokenEndpoint;
                opt.SignInScheme = IdentityConstants.ExternalScheme;
                opt.CallbackPath = "/signin-google";
                opt.AccessType = "offline";
                opt.SaveTokens = true;
                opt.Scope.Add("https://www.googleapis.com/auth/userinfo.email");
                opt.Scope.Add("https://www.googleapis.com/auth/userinfo.profile");
                opt.Scope.Add("openid");
                opt.Scope.Add("profile");
                opt.Scope.Add("email");
                opt.Events.OnCreatingTicket = (ticketContext) =>
                {
                    return Task.CompletedTask;
                };
            });

            services.AddFluentEmail(appsettingBinding.MailSettings.SenderEmail)
                .AddSmtpSender(
                    host: appsettingBinding.MailSettings.Host,
                    port: appsettingBinding.MailSettings.Port,
                    username: appsettingBinding.MailSettings.SenderEmail,
                    password: appsettingBinding.MailSettings.AppPassword
                    )
                .AddRazorRenderer();

            services.AddScoped<IMyEmailService,MailServices>();
			

			services.AddSingleton<BlobServiceClient>( (serviceProvider) => 
            {
                var newClient =  new BlobServiceClient(appsettingBinding.ConnectionStrings.AzureBlobConnectionString);
                return newClient;
            });
			services.AddSingleton<IFileService,FileService>();
            services.AddStackExchangeRedisCache(opt => 
            {
                var connectionString = appsettingBinding.ConnectionStrings.CacheConnectionString; 
                opt.Configuration = connectionString;
            });
            services.AddBackgroundServices(appsettingBinding);
			//services.AddSingleton<IConnectionMultiplexer>(factory => ConnectionMultiplexer.Connect(appsettingBinding.ConnectionStrings.CacheConnectionString));
   //         services.AddSingleton<ICacheService,RedisCacheService>();
			return services;
        }
		private static IServiceCollection AddBackgroundServices(this IServiceCollection services, AppsettingBinding appsettingBinding)
        {
            services.AddQuartz(options =>
            {
                var demoJobKey = nameof(QuartzDemoServices);
				options.UseMicrosoftDependencyInjectionJobFactory();
                options
                    .AddJob<QuartzDemoServices>(JobKey.Create(demoJobKey), config => { })
                    .AddTrigger(trigger => trigger
                        .ForJob(demoJobKey)
                        .WithSimpleSchedule(schedule =>
                        {
                            schedule.WithIntervalInSeconds(5).RepeatForever();
                        }));
            });
            services.AddQuartzHostedService(options =>
            {
                options.WaitForJobsToComplete = false;
            });
            return services;
        }

	}
}
