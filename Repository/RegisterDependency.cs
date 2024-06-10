using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Repository.Interface.User;
using Shared.ConfigurationBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repository.Implementation.User;
using Repository.Interface;
using Repository.Implementation;
using Shared.IdentityConfiguration;
using Microsoft.AspNetCore.Identity;
using Shared.Models;
using System.Reflection;
using Shared.MapperProfiles;
using Services.Configurations;

namespace Repository
{
    public static class RegisterDependency
    {
        public static IServiceCollection AddRepositoryService(this IServiceCollection services, AppsettingBinding appsettingBinding)
        {
            services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
            {
                var getSettings = serviceProvider.GetRequiredService<AppsettingBinding>();
                var getConnectionString = getSettings.ConnectionStrings.DefaultConnectionString;
                string serverName = "localhost,1433";
                string uid = "SA";
                string pwd = "Supermarcy@2003";
                string database = "BeatVision";
                var result = bool.TryParse(Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"), out var getEnvironmentIsRunningInDockerContainer);
                var dockerConnectionString = $"server={serverName};uid={uid};pwd={pwd};Database={database};TrustServerCertificate=true;";

                Console.WriteLine(getEnvironmentIsRunningInDockerContainer);
                if (getEnvironmentIsRunningInDockerContainer != null && getEnvironmentIsRunningInDockerContainer == true)
                {
                    try
                    {
                        var env_servername = Environment.GetEnvironmentVariable("MYAPP_SERVERNAME");
                        var env_uid = Environment.GetEnvironmentVariable("MYAPP_UID");
                        var env_pwd = Environment.GetEnvironmentVariable("MYAPP_PWD");
                        var env_database = Environment.GetEnvironmentVariable("MYAPP_DATABASE");
                        if (string.IsNullOrEmpty(env_servername) ||
                            string.IsNullOrEmpty(env_uid) ||
                            string.IsNullOrEmpty(env_pwd) ||
                            string.IsNullOrEmpty(env_database))
                        {
                            throw new ArgumentException("empty environment variable, take default variable");
                        }
                        serverName = env_servername;
                        uid = env_uid;
                        pwd = env_pwd;
                        database = env_database;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    var dockerConnectionStringReal = $"server={serverName};uid={uid};pwd={pwd};Database={database};TrustServerCertificate=true;";
                    Console.WriteLine("yes run in docker");
                    options.UseSqlServer(dockerConnectionStringReal);
                }
                else
                {
                    Console.WriteLine("run in normal environment");
                    options.UseSqlServer(getConnectionString);
                }
                //options.UseSqlServer(getConnectionString);
            });
            services.AddIdentity<CustomIdentityUser, CustomIdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddRoleManager<RoleManager<CustomIdentityRole>>()
                .AddSignInManager<SignInManager<CustomIdentityUser>>()
                .AddUserManager<UserManager<CustomIdentityUser>>()
                .AddDefaultTokenProviders()
                .AddTokenProvider("MyResetPasswordTokenProvider", typeof(ResetPasswordTokenProvider<CustomIdentityUser>)) ;
            //services.Configure<IdentityOptions>(config => 
            //{
            //    config.Tokens.PasswordResetTokenProvider = "MyResetPasswordTokenProvider";
            //});
            services.AddTransient<ResetPasswordTokenProvider<CustomIdentityUser>>();
            services.Configure<ResetPasswordTokenProviderOptions>(config => {
                config.TokenLifespan = TimeSpan.FromMinutes(appsettingBinding.AppConstraints.ExpireResetTokenMinute);
            });
            
            services.AddScoped<ICustomIdentityUserRepository, CustomIdentityUserRepository>();
            services.AddScoped<ICustomIdentityUserTokenRepository, CustomIdentityUserTokenRepository>();
            services.AddScoped<ICustomIdentityUserLoginsRepository, CustomIdentityUserLoginsRepository>();
            services.AddScoped<ICustomIdentityUserClaimsRepository, CustomIdentityUserClaimsRepository>();
            services.AddScoped<ICustomIdentityRoleRepository, CustomIdentityRoleRepository>();
            services.AddScoped<ICustomIdentityUserRoleRepository, CustomIdentityUserRoleRepository>();
            services.AddScoped<ICustomIdentityRoleClaimRepository, CustomIdentityRoleClaimRepository>();

			services.AddScoped<IRepositoryBase<UserProfile>, UserProfileRepository>();

			services.AddScoped<IRepositoryBase<Notification>, RepositoryBase<Notification>>();
			services.AddScoped<IRepositoryBase<Message>, RepositoryBase<Message>>();
			services.AddScoped<IRepositoryBase<CartItem>, RepositoryBase<CartItem>>();
			services.AddScoped<IRepositoryBase<Comment>, RepositoryBase<Comment>>();
			services.AddScoped<IRepositoryBase<TrackComment>, RepositoryBase<TrackComment>>();
			services.AddScoped<IRepositoryBase<Track>, RepositoryBase<Track>>();
			services.AddScoped<IRepositoryBase<TrackLicense>, RepositoryBase<TrackLicense>>();


			services.AddScoped<IRepositoryBase<Tag>, RepositoryBase<Tag>>();
			services.AddScoped<IRepositoryBase<BlobFileData>, RepositoryBase<BlobFileData>>();
            services.AddScoped<IRepositoryBase<Coupon>, RepositoryBase<Coupon>>();
            services.AddScoped<IRepositoryBase<Order>, RepositoryBase<Order>>();
			services.AddScoped<IRepositoryBase<OrderItem>, RepositoryBase<OrderItem>>();
			services.AddScoped<IRepositoryBase<OrderTransaction>, RepositoryBase<OrderTransaction>>();


			services.AddScoped<IRepositoryWrapper, RepositoryWrapper>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.Configure<IdentityOptions>(options =>
            {
                var userOpt = options.User;
                userOpt.RequireUniqueEmail = true;
                var passOpt = options.Password;
                passOpt.RequireNonAlphanumeric = false;
                passOpt.RequireDigit = false;
                passOpt.RequireLowercase = false;
                passOpt.RequireUppercase = false;
                passOpt.RequiredLength = 1;
                var tokenOpt = options.Tokens;
                tokenOpt.EmailConfirmationTokenProvider = TokenOptions.DefaultEmailProvider;
                tokenOpt.ChangeEmailTokenProvider = TokenOptions.DefaultEmailProvider;
                tokenOpt.PasswordResetTokenProvider = "MyResetPasswordTokenProvider";
            });
            services.AddAutoMapper(Assembly.GetAssembly(typeof(Profiles)));
            return services;
        }
    }
}
