using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using Autofac;
using Autofac.Builder;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Minio;
using Serilog;

using MusicPlayerBackend.App.Middlewares;
using MusicPlayerBackend.Common;
using MusicPlayerBackend.Data;
using MusicPlayerBackend.Data.Infr;
using MusicPlayerBackend.Services;
using MusicPlayerBackend.Services.Identity;

namespace MusicPlayerBackend.App;

public sealed class Startup(IConfiguration configuration)
{
    public Func<IRegistrationBuilder<object, object, object>, IRegistrationBuilder<object, object, object>> LifetimeScopeConfigurator { get; } =
        registrationBuilder => registrationBuilder.InstancePerLifetimeScope();

    public void ConfigureServices(IServiceCollection services)
    {
        services.Configure<AppConfig>(configuration);
        services.Configure<Common.Minio>(configuration.GetSection(nameof(Minio)));
        services.Configure<TokenConfig>(configuration.GetSection(nameof(TokenConfig)));
        services.Configure<PasswordHasherOptions>(opt => opt.IterationCount = 600_000);

        services.AddSerilog()
            .AddHttpContextAccessor();

        services.AddMinio(o =>
        {
            var minioConfig = configuration.GetSection("Minio").Get<Common.Minio>();
            ArgumentNullException.ThrowIfNull(minioConfig);

            o
                .WithSSL(minioConfig.UseSsl)
                .WithEndpoint(minioConfig.Endpoint, minioConfig.Port)
                .WithCredentials(minioConfig.AccessKey, minioConfig.SecretKey);
        });

        services.AddScoped<AppDbContext>(c =>
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseNpgsql(c.GetRequiredService<IConfiguration>().GetConnectionString(AppDbContext.ConnectionStringName),
                b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.GetName().Name));


            return new AppDbContext(optionsBuilder.Options);
        });
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services
            .AddCustomIdentity()
            .AddTransient<IS3Service, S3Service>()
            .AddTransient<IUserProvider, UserProvider>();

        services
            .AddControllers()
            .AddControllersAsServices()
            .AddJsonOptions(opts =>
                opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()))
            .AddXmlSerializerFormatters();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = false,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.GetSection(nameof(TokenConfig)).Get<TokenConfig>()!.SigningKey)),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    RequireAudience = false,
                };
            });

        services.AddSession()
            .AddAuthorization();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                BearerFormat = JwtConstants.TokenType,
                In = ParameterLocation.Header,
                Scheme = JwtBearerDefaults.AuthenticationScheme,
                Name = "Authorization",
                Description = "Please insert JWT into field"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = JwtBearerDefaults.AuthenticationScheme
                        }
                    }, []
                }
            });

            var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
            c.EnableAnnotations();
        });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, AppDbContext ctx, IOptions<AppConfig> appConfig, IMinioClient minioClient)
    {
        minioClient
            .CreateBucketIfNotExists("tracks")
            .CreateBucketIfNotExists("covers");

        if (appConfig.Value.MigrateDatabaseOnStartup)
            ctx.Database.Migrate();

        app.UseSerilogRequestLogging();
        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(o => o.DisplayOperationId());
        }

        app.UseMiddleware<UnauthorizedMiddleware>();
        app.UseAuthentication()
            .UseAuthorization()
            .UseRouting();

        app.UseEndpoints(endpoints => endpoints.MapControllers());
    }

    public void ConfigureContainer(ContainerBuilder builder)
    {
        builder.RegisterModule(new DataDiRegistrationModule(LifetimeScopeConfigurator));
    }
}
