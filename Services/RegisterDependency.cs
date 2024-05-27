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
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication;
using System.Net.Http.Json;
using Newtonsoft.Json;
namespace Services
{
    public static class RegisterDependency
    {
        public static IServiceCollection AddServicesLayer(this IServiceCollection services, AppsettingBinding appsettingBinding)
        {
            services.AddRepositoryService();
			services.AddFluentEmail(appsettingBinding.MailSettings.SenderEmail)
				.AddSmtpSender(
					host: appsettingBinding.MailSettings.Host,
					port: appsettingBinding.MailSettings.Port,
					username: appsettingBinding.MailSettings.SenderEmail,
					password: appsettingBinding.MailSettings.AppPassword
					)
				.AddRazorRenderer();
			services.AddSingleton<BlobServiceClient>((serviceProvider) =>
			{
				var newClient = new BlobServiceClient(appsettingBinding.ConnectionStrings.AzureBlobConnectionString);
				return newClient;
			});
			services.AddSingleton<FileService>();
			services.AddScoped<IMyEmailService, MailServices>();
			services.AddScoped<ISecurityTokenServices,JwtTokenServices>();
            services.AddScoped<IUserIdentityService, UserIdentityServices>();
            services.AddScoped<CommentService>();
            services.AddScoped<AppUserManager>();
            services.AddScoped<UserIdentityServices>();
            services.AddScoped<ImageFileServices>();
			services.AddScoped<AudioFileServices>();
            services.AddScoped<TagManager>();
			services.AddScoped<TrackManager>();
            services.AddScoped<LicenseFileService>();
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
                //opt.Scope.Add("https://www.googleapis.com/auth/userinfo.email");
                //opt.Scope.Add("https://www.googleapis.com/auth/userinfo.profile");
                opt.Scope.Add("openid");
                opt.Scope.Add("profile");
                opt.Scope.Add("email");
                opt.Events.OnCreatingTicket = async (ticketContext) =>
                {
                    var oauthProp = ticketContext.Properties;
                    var identity = ticketContext.Identity;
                    var items = oauthProp.Items;
                    var accessToken = ticketContext.AccessToken;//oauthProp.Ticket.Properties.GetTokenValue("access_token");
					var httpClient = new HttpClient();
					httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
					var response = await httpClient.GetAsync("https://www.googleapis.com/oauth2/v2/userinfo");
					if (response.IsSuccessStatusCode is false)
					{
                        identity.AddClaim(new System.Security.Claims.Claim("FAILURE","FAILURE"));
					}
                    else
                    {
                        var readResult = JsonConvert.DeserializeObject<GoogleOauthInfo>(await response.Content.ReadAsStringAsync());
                        identity.AddClaim(new System.Security.Claims.Claim(ApplicationStaticValue.UsernameClaimType,readResult.Name));
						identity.AddClaim(new System.Security.Claims.Claim(ApplicationStaticValue.MailClaimType, readResult.Email));
                        identity.AddClaim(new System.Security.Claims.Claim(ApplicationStaticValue.ProfileImageUrlClaimType, readResult.Picture));
					}
				};
            });

            
            
			
			services.AddStackExchangeRedisCache(opt => 
            {
                var connectionString = appsettingBinding.ConnectionStrings.CacheConnectionString; 
                opt.Configuration = connectionString;
            });
            services.AddBackgroundServices(appsettingBinding);
			return services;
        }
		private static IServiceCollection AddBackgroundServices(this IServiceCollection services, AppsettingBinding appsettingBinding)
        {
            services.AddQuartz(options =>
            {
                var demoJobKey = nameof(QuartzDemoServices);
                var publishTrackServiceKey = nameof(PublishTrackBackgroundService);
				options.UseMicrosoftDependencyInjectionJobFactory();
                options.AddJob<QuartzDemoServices>(JobKey.Create(demoJobKey), config => { })
                    .AddTrigger(trigger => trigger
                        .ForJob(demoJobKey)
                        .WithSimpleSchedule(schedule =>
                        {
                            schedule.WithIntervalInSeconds(5).RepeatForever();
                        }));
                options.AddJob<PublishTrackBackgroundService>(JobKey.Create(publishTrackServiceKey), config =>{ })
                    .AddTrigger(trigger => trigger
                        .ForJob(publishTrackServiceKey)
                        .WithSimpleSchedule(schedule =>
                        {
                            schedule.WithIntervalInMinutes(2).RepeatForever();
                        }));
            });
            services.AddQuartzHostedService(options =>
            {
                options.WaitForJobsToComplete = false;
            });
            return services;
        }
		private class GoogleOauthInfo
		{
			public string Id { get; set; }
			public string Email { get; set; }
			public string Name { get; set; }
			public string Picture { get; set; }
		}
	}
}
