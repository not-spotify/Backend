using System.Text.Json.Serialization;
using Autofac;
using Autofac.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Minio;
using MusicPlayerBackend.Common;
using MusicPlayerBackend.Data;
using MusicPlayerBackend.Data.Entities;
using MusicPlayerBackend.Data.Identity.Stores;
using MusicPlayerBackend.Data.Infr;
using MusicPlayerBackend.Services;
using MusicPlayerBackend.Services.Identity;
using Serilog;

namespace MusicPlayerBackend.App;

public sealed class Startup(IConfiguration configuration)
{
    public Func<IRegistrationBuilder<object, object, object>, IRegistrationBuilder<object, object, object>> LifetimeScopeConfigurator { get; } =
        registrationBuilder => registrationBuilder.InstancePerLifetimeScope();

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSerilog();
        services.Configure<AppConfig>(configuration);
        services.Configure<Common.Minio>(configuration.GetSection(nameof(Common.Minio)));
        services.AddHttpContextAccessor();

        services.AddTransient<IdentityErrorDescriber>();
        services.AddTransient<ILookupNormalizer, LookupNormalizer>();
        services.AddTransient<IPasswordHasher<User>, PasswordHasher>();
        services.AddTransient<IUserClaimsPrincipalFactory<User>, UserClaimsPrincipalFactory<User>>();
        services.AddTransient<IUserConfirmation<User>, UserConfirmation>();
        services.AddTransient<IUserStore<User>, UserStore>();
        services.AddTransient<IUserValidator<User>, UserValidator>();
        services.AddTransient<UserManager<User>, UserManager>();
        services.AddTransient<SignInManager<User>, SignInManager>();

        services.AddTransient<IMinioClient, MinioClient>();
        services.AddTransient<IS3Service, S3Service>();
        services.AddTransient<IUserResolver, UserResolver>();

        services.AddControllers().AddControllersAsServices().AddJsonOptions(opts =>
        {
            var enumConverter = new JsonStringEnumConverter();
            opts.JsonSerializerOptions.Converters.Add(enumConverter);
        });

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, AppDbContext ctx, IOptions<AppConfig> appConfig)
    {
        if (appConfig.Value.MigrateDatabaseOnStartup)
            ctx.Database.MigrateAsync().GetAwaiter().GetResult();

        app.UseSerilogRequestLogging();
        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseRouting();
        app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
    }

    public void ConfigureContainer(ContainerBuilder builder)
    {
        builder.RegisterModule(new DataDiRegistrationModule(LifetimeScopeConfigurator));
    }
}
