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

namespace Repository
{
    public static class RegisterDependency
    {
        public static IServiceCollection AddRepositoryService(this IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
            {
                var getSettings = serviceProvider.GetRequiredService<AppsettingBinding>();
                var getConnectionString = getSettings.ConnectionStrings.DefaultConnectionString;
                options.UseSqlServer(getConnectionString);
            });
            services.AddIdentity<CustomIdentityUser, CustomIdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddRoleManager<RoleManager<CustomIdentityRole>>()
                .AddSignInManager<SignInManager<CustomIdentityUser>>()
                .AddUserManager<UserManager<CustomIdentityUser>>()
                .AddDefaultTokenProviders();

            
            
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
                tokenOpt.PasswordResetTokenProvider = TokenOptions.DefaultAuthenticatorProvider;
            });
            services.AddAutoMapper(Assembly.GetAssembly(typeof(Profiles)));
            return services;
        }
    }
}
