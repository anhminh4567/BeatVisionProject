using Microsoft.OpenApi.Models;
using Services;
using Shared.ConfigurationBinding;
using Microsoft.AspNetCore.Mvc.Razor;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var appsettingsBinding = new AppsettingBinding();
builder.Configuration.Bind("AppsettingBinding", appsettingsBinding);
foreach(var path in appsettingsBinding.MailTemplateRelativePath)
{
    var wwwrootPath = builder.Environment.WebRootPath;
    var newAbsolutePath = Path.Combine(wwwrootPath,path.TemplatePathWWWRoot);
    appsettingsBinding.MailTemplateAbsolutePath.Add(new MailTemplateAbsolutePath()
    {
        TemplateName = path.TemplateName,
        TemplateAbsolutePath = newAbsolutePath
    });
}
builder.Services.AddSingleton(appsettingsBinding);
builder.Services.AddControllers();
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
//builder.Services.AddFluentEmail(appsettingsBinding.MailSettings.SenderEmail)
//				.AddSmtpSender(
//					host: appsettingsBinding.MailSettings.Host,
//					port: appsettingsBinding.MailSettings.Port,
//					username: appsettingsBinding.MailSettings.SenderEmail,
//					password: appsettingsBinding.MailSettings.AppPassword
//					)
//				.AddRazorRenderer();
builder.Services.AddServicesLayer(appsettingsBinding);

//builder.Services.AddIdentity();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
