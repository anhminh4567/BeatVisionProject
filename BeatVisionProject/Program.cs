using Microsoft.OpenApi.Models;
using Services;
using Shared.ConfigurationBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Repository;
using Microsoft.EntityFrameworkCore;
internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        var appsettingsBinding = new AppsettingBinding();
        builder.Configuration.Bind("AppsettingBinding", appsettingsBinding);
        var result = bool.TryParse(Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"), out var getEnvironmentIsRunningInDockerContainer);
        if (getEnvironmentIsRunningInDockerContainer != null && getEnvironmentIsRunningInDockerContainer == true)
        {
            foreach (var path in appsettingsBinding.MailTemplateRelativePath)
            {
                string[] splitedPath = path.TemplatePathWWWRoot.Split("\\");
                string correctAppPath = Path.Combine(splitedPath);

                var wwwrootPath = builder.Environment.WebRootPath;
                var newAbsolutePath = Path.Combine(wwwrootPath, correctAppPath);
                appsettingsBinding.MailTemplateAbsolutePath.Add(new MailTemplateAbsolutePath()
                {
                    TemplateName = path.TemplateName,
                    TemplateAbsolutePath = newAbsolutePath
                });
            }
            foreach (var path in appsettingsBinding.DefaultContentRelativePath)
            {
                string[] splitedPath = path.ContentPathWWWRoot.Split("\\");
                string correctAppPath = Path.Combine(splitedPath);

                var wwwrootPath = builder.Environment.WebRootPath;
                var newAbsolutePath = Path.Combine(wwwrootPath, correctAppPath);
                path.ContentPathWWWRoot = newAbsolutePath;
            }
        }
        else
        {
            foreach (var path in appsettingsBinding.MailTemplateRelativePath)
            {
                var wwwrootPath = builder.Environment.WebRootPath;
                var newAbsolutePath = Path.Combine(wwwrootPath, path.TemplatePathWWWRoot);
                appsettingsBinding.MailTemplateAbsolutePath.Add(new MailTemplateAbsolutePath()
                {
                    TemplateName = path.TemplateName,
                    TemplateAbsolutePath = newAbsolutePath
                });
            }
            foreach (var path in appsettingsBinding.DefaultContentRelativePath)
            {
                var wwwrootPath = builder.Environment.WebRootPath;
                var newAbsolutePath = Path.Combine(wwwrootPath, path.ContentPathWWWRoot);
                path.ContentPathWWWRoot = newAbsolutePath;
            }
            
        }

        //builder.Services.AddSingleton(appsettingsBinding);
        builder.Services.Configure<FormOptions>(options =>
        {
            options.MultipartBodyLengthLimit = 10000000000;//tam 10GB
        });
        builder.Services.AddControllers()
            .AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                options.SerializerSettings.ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new DefaultNamingStrategy()
                };
                // You can add other settings here if needed
            });
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(opt =>
        {
            opt.SwaggerDoc("v1", new OpenApiInfo { Title = "MyAPI", Version = "v1" });
            opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "Please enter token",
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                BearerFormat = "JWT",
                Scheme = "bearer"
            });
            opt.AddSecurityRequirement(new OpenApiSecurityRequirement
                    {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type=ReferenceType.SecurityScheme,
                            Id="Bearer"
                        }
                    },
                    new string[]{}
                }
                    });
        });
        builder.Services.AddCors(opt =>
        {
            opt.AddDefaultPolicy((config) =>
            {
                config.AllowAnyHeader();
                config.AllowAnyOrigin();
                config.AllowAnyOrigin();
                config.AllowAnyMethod();
            });
        });
        builder.Services.AddServicesLayer(appsettingsBinding);
        builder.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.All;
            options.KnownNetworks.Clear(); // Clear these so all networks are trusted
            options.KnownProxies.Clear();  // Clear these so all proxies are trusted
        });
        builder.Services.AddSingleton(appsettingsBinding);

        var app = builder.Build();
        app.UseForwardedHeaders();

        app.UseSwagger();
        app.UseSwaggerUI(config =>
        {
            
        });

        {
            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            //context.Database.EnsureCreated();
            context.Database.Migrate();
        }
      
        app.UseCors();
        app.UseStaticFiles();

        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        app.Run();
    }
}